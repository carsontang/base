﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nr;

namespace NumericalRecipies.ch02
{
    public class SVD
    {
        private int m, n;
        private MatDoub u, v;
        private VecDoub w;
        private double eps, tsh;

        /// <summary>
        /// Constructor.  The single argument is A.  The SVD computation is done by 
        /// decompose(), and the results are sorted by reorder().
        /// </summary>
        /// <param name="a">The matrix to decompose.</param>

        public SVD(MatDoub a)
        {
            m = (a.nrows());
            n = (a.ncols());
            u = new MatDoub(a);
            v = new MatDoub(n, n);
            w = new VecDoub(n);
            eps = double.Epsilon;
            decompose();
            reorder();
            tsh = 0.5 * Math.Sqrt(m + n + 1.0) * w[0] * eps; // Default threshold for nonzero singular values;
        }

        /// <summary>
        /// Solve Ax = b for a vector x using the psudoinverse of A as obtained by SVD.
        /// If positive, thresh is the threshold value below which singular values
        /// as considered to be zero.  If thresh is negative, a default based on roundoff error is used.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="x"></param>
        /// <param name="thresh"></param>

        //void solve(VecDoub_I &b, VecDoub_O &x, double thresh);
        public void solve(VecDoub b, VecDoub x, double thresh)
        {
            int i, j, jj;
            double s;
            if (b.size() != m || x.size() != n) throw new Exception("SVD::solve bad sizes");
            VecDoub tmp = new VecDoub(n);
            tsh = (thresh >= 0.0 ? thresh : 0.5 * Math.Sqrt(m + n + 1.0) * w[0] * eps);
            for (j = 0; j < n; j++)
            {
                s = 0.0;
                if (w[j] > tsh)
                {
                    for (i = 0; i < m; i++) s += u[i][j] * b[i];
                    s /= w[j];
                }
                tmp[j] = s;
            }
            for (j = 0; j < n; j++)
            {
                s = 0.0;
                for (jj = 0; jj < n; jj++) s += v[j][jj] * tmp[jj];
                x[j] = s;
            }
        }

        /// <summary>
        /// Solve with (apply the pseudoinverse to) one or more right-hand sides.
        /// </summary>
        /// <param name="b">The RHS to solve.</param>
        /// <param name="x">The result.</param>

        public void solve(VecDoub b, VecDoub x)
        {
            solve(b, x, -1.0);
        }

        /// <summary>
        /// Solve m sets of n equations AX=B using the pseudoinverse of A. 
        /// The right-hand sides are input as b[0..n-1][0..m-1], while
        /// x[0..n-1][0..m-1] return the solutions.
        /// If positive, thresh is the threshold value below which singular values
        /// as considered to be zero.  If thresh is negative, a default based on rounodd error is used.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="x"></param>
        /// <param name="thresh"></param>

        //void solve(MatDoub_I &b, MatDoub_O &x, double thresh);
        public void solve(MatDoub b, MatDoub x, double thresh)
        {
            int i, j, m = b.ncols();
            if (b.nrows() != n || x.nrows() != n || b.ncols() != x.ncols())
                throw new Exception("SVD::solve bad sizes");
            VecDoub xx = new VecDoub(n);
            for (j = 0; j < m; j++)
            {
                for (i = 0; i < n; i++) xx[i] = b[i][j];
                solve(xx, xx, thresh);
                for (i = 0; i < n; i++) x[i][j] = xx[i];
            }
        }

        public void solve(MatDoub b, MatDoub x)
        {
            solve(b, x, -1.0);
        }

        /// <summary>
        /// Returns the rank of A, after zeroing any singular values smaller than thresh.
        /// If thresh is negative, a default value based on estimated roundoff is used.
        /// </summary>
        /// <returns>The rank.</returns>

        //int rank(double thresh);
        public int rank(double thresh)
        {
            int j, nr = 0;
            tsh = (thresh >= 0.0 ? thresh : 0.5 * Math.Sqrt(m + n + 1.0) * w[0] * eps);
            for (j = 0; j < n; j++) if (w[j] > tsh) nr++;
            return nr;
        }

        public int rank()
        {
            return rank(-1.0);
        }

        /// <summary>
        /// Return the nullity of A, after zeroing any singular values smaller than thresh.
        /// If thresh is negative, a default value based on estimated roundoff is used.
        /// </summary>
        /// <returns>The nullity.</returns>

        //int nullity(double thresh);
        public int nullity(double thresh)
        {
            int j, nn = 0;
            tsh = (thresh >= 0.0 ? thresh : 0.5 * Math.Sqrt(m + n + 1.0) * w[0] * eps);
            for (j = 0; j < n; j++) if (w[j] <= tsh) nn++;
            return nn;
        }

        public int nullity()
        {
            return nullity(-1.0);
        }

        /// <summary>
        /// Give an orthonormal basis for the range of A as the columns of a returned matrix, 
        /// after zeroing any singular values smaller than thresh.
        /// If thresh is negative, a default value based on estimated roundoff is used.
        /// </summary>
        /// <param name="thresh"></param>
        /// <returns></returns>

        //MatDoub range(double thresh);
        public MatDoub range(double thresh)
        {
            int i, j, nr = 0;
            MatDoub rnge = new MatDoub(m, rank(thresh));
            for (j = 0; j < n; j++)
            {
                if (w[j] > tsh)
                {
                    for (i = 0; i < m; i++) rnge[i][nr] = u[i][j];
                    nr++;
                }
            }
            return rnge;
        }

        public MatDoub range()
        {
            return range(-1.0);
        }

        /// <summary>
        /// Compute the inverse of the condition number of a.
        /// </summary>
        /// <returns>The inverse.</returns>

        public double inv_condition()
        {
            return (w[0] <= 0.0 || w[n - 1] <= 0.0) ? 0.0 : w[n - 1] / w[0];
        }

        /// <summary>
        /// Give an orthonormal basis for the nullspace of A as the columns of a returned matrix,
        /// after zeroing any singular values smaller than thresh.
        /// If thresh is negative, a default value based on estimated roundoff is used.
        /// </summary>
        /// <param name="thresh"></param>
        /// <returns></returns>

        //MatDoub nullspace(double thresh);
        public MatDoub nullspace(double thresh)
        {
            int j, jj, nn = 0;
            MatDoub nullsp = new MatDoub(n, nullity(thresh));
            for (j = 0; j < n; j++)
            {
                if (w[j] <= tsh)
                {
                    for (jj = 0; jj < n; jj++) nullsp[jj][nn] = v[jj][j];
                    nn++;
                }
            }
            return nullsp;
        }

        public MatDoub nullspace()
        {
            return nullspace(-1.0);
        }

        //void decompose();
        private void decompose()
        {
            bool flag;
            int i, its, j, jj, k, nm = int.MinValue;
            int l = int.MinValue;
            double anorm, c, f, g, h, s, scale, x, y, z;
            VecDoub rv1 = new VecDoub(n);
            g = scale = anorm = 0.0;
            for (i = 0; i < n; i++)
            {
                l = i + 2;
                rv1[i] = scale * g;
                g = s = scale = 0.0;
                if (i < m)
                {
                    for (k = i; k < m; k++) scale += Math.Abs(u[k][i]);
                    if (scale != 0.0)
                    {
                        for (k = i; k < m; k++)
                        {
                            u[k][i] /= scale;
                            s += u[k][i] * u[k][i];
                        }
                        f = u[i][i];
                        g = -NR.SIGN(Math.Sqrt(s), f);
                        h = f * g - s;
                        u[i][i] = f - g;
                        for (j = l - 1; j < n; j++)
                        {
                            for (s = 0.0, k = i; k < m; k++) s += u[k][i] * u[k][j];
                            f = s / h;
                            for (k = i; k < m; k++) u[k][j] += f * u[k][i];
                        }
                        for (k = i; k < m; k++) u[k][i] *= scale;
                    }
                }
                w[i] = scale * g;
                g = s = scale = 0.0;
                if (i + 1 <= m && i + 1 != n)
                {
                    for (k = l - 1; k < n; k++) scale += Math.Abs(u[i][k]);
                    if (scale != 0.0)
                    {
                        for (k = l - 1; k < n; k++)
                        {
                            u[i][k] /= scale;
                            s += u[i][k] * u[i][k];
                        }
                        f = u[i][l - 1];
                        g = -NR.SIGN(Math.Sqrt(s), f);
                        h = f * g - s;
                        u[i][l - 1] = f - g;
                        for (k = l - 1; k < n; k++) rv1[k] = u[i][k] / h;
                        for (j = l - 1; j < m; j++)
                        {
                            for (s = 0.0, k = l - 1; k < n; k++) s += u[j][k] * u[i][k];
                            for (k = l - 1; k < n; k++) u[j][k] += s * rv1[k];
                        }
                        for (k = l - 1; k < n; k++) u[i][k] *= scale;
                    }
                }
                anorm = Math.Max(anorm, (Math.Abs(w[i]) + Math.Abs(rv1[i])));
            }
            for (i = n - 1; i >= 0; i--)
            {
                if (i < n - 1)
                {
                    if (g != 0.0)
                    {
                        for (j = l; j < n; j++)
                            v[j][i] = (u[i][j] / u[i][l]) / g;
                        for (j = l; j < n; j++)
                        {
                            for (s = 0.0, k = l; k < n; k++) s += u[i][k] * v[k][j];
                            for (k = l; k < n; k++) v[k][j] += s * v[k][i];
                        }
                    }
                    for (j = l; j < n; j++) v[i][j] = v[j][i] = 0.0;
                }
                v[i][i] = 1.0;
                g = rv1[i];
                l = i;
            }
            for (i = Math.Min(m, n) - 1; i >= 0; i--)
            {
                l = i + 1;
                g = w[i];
                for (j = l; j < n; j++) u[i][j] = 0.0;
                if (g != 0.0)
                {
                    g = 1.0 / g;
                    for (j = l; j < n; j++)
                    {
                        for (s = 0.0, k = l; k < m; k++) s += u[k][i] * u[k][j];
                        f = (s / u[i][i]) * g;
                        for (k = i; k < m; k++) u[k][j] += f * u[k][i];
                    }
                    for (j = i; j < m; j++) u[j][i] *= g;
                }
                else for (j = i; j < m; j++) u[j][i] = 0.0;
                ++u[i][i];
            }
            for (k = n - 1; k >= 0; k--)
            {
                for (its = 0; its < 30; its++)
                {
                    flag = true;
                    for (l = k; l >= 0; l--)
                    {
                        nm = l - 1;
                        if (l == 0 || Math.Abs(rv1[l]) <= eps * anorm)
                        {
                            flag = false;
                            break;
                        }
                        if (Math.Abs(w[nm]) <= eps * anorm) break;
                    }
                    if (flag)
                    {
                        c = 0.0;
                        s = 1.0;
                        for (i = l; i < k + 1; i++)
                        {
                            f = s * rv1[i];
                            rv1[i] = c * rv1[i];
                            if (Math.Abs(f) <= eps * anorm) break;
                            g = w[i];
                            h = pythag(f, g);
                            w[i] = h;
                            h = 1.0 / h;
                            c = g * h;
                            s = -f * h;
                            for (j = 0; j < m; j++)
                            {
                                y = u[j][nm];
                                z = u[j][i];
                                u[j][nm] = y * c + z * s;
                                u[j][i] = z * c - y * s;
                            }
                        }
                    }
                    z = w[k];
                    if (l == k)
                    {
                        if (z < 0.0)
                        {
                            w[k] = -z;
                            for (j = 0; j < n; j++) v[j][k] = -v[j][k];
                        }
                        break;
                    }
                    if (its == 29) throw new Exception("no convergence in 30 svdcmp iterations");
                    x = w[l];
                    nm = k - 1;
                    y = w[nm];
                    g = rv1[nm];
                    h = rv1[k];
                    f = ((y - z) * (y + z) + (g - h) * (g + h)) / (2.0 * h * y);
                    g = pythag(f, 1.0);
                    f = ((x - z) * (x + z) + h * ((y / (f + NR.SIGN(g, f))) - h)) / x;
                    c = s = 1.0;
                    for (j = l; j <= nm; j++)
                    {
                        i = j + 1;
                        g = rv1[i];
                        y = w[i];
                        h = s * g;
                        g = c * g;
                        z = pythag(f, h);
                        rv1[j] = z;
                        c = f / z;
                        s = h / z;
                        f = x * c + g * s;
                        g = g * c - x * s;
                        h = y * s;
                        y *= c;
                        for (jj = 0; jj < n; jj++)
                        {
                            x = v[jj][j];
                            z = v[jj][i];
                            v[jj][j] = x * c + z * s;
                            v[jj][i] = z * c - x * s;
                        }
                        z = pythag(f, h);
                        w[j] = z;
                        if (z != 0.0)
                        {
                            z = 1.0 / z;
                            c = f * z;
                            s = h * z;
                        }
                        f = c * g + s * y;
                        x = c * y - s * g;
                        for (jj = 0; jj < m; jj++)
                        {
                            y = u[jj][j];
                            z = u[jj][i];
                            u[jj][j] = y * c + z * s;
                            u[jj][i] = z * c - y * s;
                        }
                    }
                    rv1[l] = 0.0;
                    rv1[k] = f;
                    w[k] = x;
                }
            }
        }

        //void reorder();
        private void reorder()
        {
            int i, j, k, s, inc = 1;
            double sw;
            VecDoub su = new VecDoub(m), sv = new VecDoub(n);
            do { inc *= 3; inc++; } while (inc <= n);
            do
            {
                inc /= 3;
                for (i = inc; i < n; i++)
                {
                    sw = w[i];
                    for (k = 0; k < m; k++) su[k] = u[k][i];
                    for (k = 0; k < n; k++) sv[k] = v[k][i];
                    j = i;
                    while (w[j - inc] < sw)
                    {
                        w[j] = w[j - inc];
                        for (k = 0; k < m; k++) u[k][j] = u[k][j - inc];
                        for (k = 0; k < n; k++) v[k][j] = v[k][j - inc];
                        j -= inc;
                        if (j < inc) break;
                    }
                    w[j] = sw;
                    for (k = 0; k < m; k++) u[k][j] = su[k];
                    for (k = 0; k < n; k++) v[k][j] = sv[k];

                }
            } while (inc > 1);
            for (k = 0; k < n; k++)
            {
                s = 0;
                for (i = 0; i < m; i++) if (u[i][k] < 0.0) s++;
                for (j = 0; j < n; j++) if (v[j][k] < 0.0) s++;
                if (s > (m + n) / 2)
                {
                    for (i = 0; i < m; i++) u[i][k] = -u[i][k];
                    for (j = 0; j < n; j++) v[j][k] = -v[j][k];
                }
            }
        }

        //double pythag(const double a, const double b);
        private double pythag(double a, double b)
        {
            double absa = Math.Abs(a), absb = Math.Abs(b);
            return (absa > absb ? absa * Math.Sqrt(1.0 + NR.SQR(absb / absa)) :
                (absb == 0.0 ? 0.0 : absb * Math.Sqrt(1.0 + NR.SQR(absa / absb))));
        }
    }
}
