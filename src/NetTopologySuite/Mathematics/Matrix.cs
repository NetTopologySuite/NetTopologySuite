using System;

namespace NetTopologySuite.Mathematics
{
    /// <summary>
    /// Implements some 2D matrix operations
    /// (in particular, solving systems of linear equations).
    /// </summary>
    /// <author>Martin Davis</author>
    public class Matrix
    {
        private static void SwapRows(double[][] m, int i, int j)
        {
            if (i == j) return;
            for (int col = 0; col < m[0].Length; col++)
            {
                double temp = m[i][col];
                m[i][col] = m[j][col];
                m[j][col] = temp;
            }
        }

        private static void SwapRows(double[] m, int i, int j)
        {
            if (i == j) return;
            double temp = m[i];
            m[i] = m[j];
            m[j] = temp;
        }

        /// <summary>
        /// Solves a system of equations using Gaussian Elimination.
        /// In order to avoid overhead the algorithm runs in-place
        /// on <see paramref="a"/> - if <see paramref="a"/> should not be modified the client must supply a copy.
        /// </summary>
        /// <param name="a">An nxn matrix in row/column order )modified by this method)</param>
        /// <param name="b">A vector of length n</param>
        /// <returns>A vector containing the solution (if any)<br/>or <c>null</c> if the system has no or no unique solution
        /// </returns>
        /// <exception cref="ArgumentException">If the matrix has the wrong size</exception>
        public static double[] Solve(double[][] a, double[] b)
        {
            int n = b.Length;
            if (a.Length != n || a[0].Length != n)
                throw new ArgumentException("Matrix A is incorrectly sized");

            // Use Gaussian Elimination with partial pivoting.
            // Iterate over each row
            for (int i = 0; i < n; i++)
            {
                // Find the largest pivot in the rows below the current one.
                int maxElementRow = i;
                for (int j = i + 1; j < n; j++)
                    if (Math.Abs(a[j][i]) > Math.Abs(a[maxElementRow][i]))
                        maxElementRow = j;

                if (a[maxElementRow][i] == 0.0)
                    return null;

                // Exchange current row and maxElementRow in A and b.
                SwapRows(a, i, maxElementRow);
                SwapRows(b, i, maxElementRow);

                // Eliminate using row i
                for (int j = i + 1; j < n; j++)
                {
                    double rowFactor = a[j][i] / a[i][i];
                    for (int k = n - 1; k >= i; k--)
                        a[j][k] -= a[i][k] * rowFactor;
                    b[j] -= b[i] * rowFactor;
                }
            }

            /*
             * A is now (virtually) in upper-triangular form.
             * The solution vector is determined by back-substitution.
             */
            double[] solution = new double[n];
            for (int j = n - 1; j >= 0; j--)
            {
                double t = 0.0;
                for (int k = j + 1; k < n; k++)
                    t += a[j][k] * solution[k];
                solution[j] = (b[j] - t) / a[j][j];
            }
            return solution;
        }

    }
}
