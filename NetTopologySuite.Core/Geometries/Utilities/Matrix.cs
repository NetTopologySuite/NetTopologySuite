using System;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// Implements some 2D matrix operations (in particular, solving systems of linear equations).
    /// </summary>
    /// <author>Martin Davis</author>
    public class Matrix
    {
        private static void SwapRows(double[][] m, int i, int j)
        {
            if (i == j) return;
            for (var col = 0; col < m[0].Length; col++)
            {
                var temp = m[i][col];
                m[i][col] = m[j][col];
                m[j][col] = temp;
            }
        }

        private static void SwapRows(double[] m, int i, int j)
        {
            if (i == j) return;
            var temp = m[i];
            m[i] = m[j];
            m[j] = temp;
        }

        ///<summary>
        /// Solves a system of equations using Gaussian Elimination.<br/>
        /// In order to avoid overhead the algorithm runs in-place
        /// on A - if A should not be modified the client must supply a copy.
        ///</summary>
        /// <param name="a">A an nxn matrix in row/column order )modified by this method)</param>
        /// <param name="b">A vector of length n</param>
        /// <exception cref="T:System.ArgumentException">if the matrix is the wrong size</exception>
        /// <returns>
        /// <list type="Bullet">
        /// <item>A vector containing the solution (if any)</item>
        /// <item><c>null</c> if the system has no or no unique solution</item>
        /// </list>
        /// </returns>
        public static double[] Solve(double[][] a, double[] b)
        {
            var n = b.Length;
            if (a.Length != n || a[0].Length != n)
                throw new ArgumentException("Matrix A is incorrectly sized");

            // Use Gaussian Elimination with partial pivoting.
            // Iterate over each row
            for (var i = 0; i < n; i++)
            {
                // Find the largest pivot in the rows below the current one.
                var maxElementRow = i;
                for (var j = i + 1; j < n; j++)
                {
                    if (Math.Abs(a[j][i]) > Math.Abs(a[maxElementRow][i]))
                        maxElementRow = j;
                }

                if (a[maxElementRow][i] == 0.0)
                    return null;

                // Exchange current row and maxElementRow in A and b.
                SwapRows(a, i, maxElementRow);
                SwapRows(b, i, maxElementRow);

                // Eliminate using row i
                for (var j = i + 1; j < n; j++)
                {
                    var rowFactor = a[j][i] / a[i][i];
                    for (var k = n - 1; k >= i; k--)
                        a[j][k] -= a[i][k] * rowFactor;
                    b[j] -= b[i] * rowFactor;
                }
            }

            /*
             * A is now (virtually) in upper-triangular form.
             * The solution vector is determined by back-substitution.
             */
            var solution = new double[n];
            for (var j = n - 1; j >= 0; j--)
            {
                var t = 0.0;
                for (var k = j + 1; k < n; k++)
                    t += a[j][k] * solution[k];
                solution[j] = (b[j] - t) / a[j][j];
            }
            return solution;
        }
    }
}