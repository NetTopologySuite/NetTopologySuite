using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;

namespace NetTopologySuite.Algorithm.Distance
{

    /// <summary>
    /// The Fréchet distance is a measure of similarity between curves. Thus, it can
    /// be used like the Hausdorff distance.
    /// <para/>
    /// An analogy for the Fréchet distance taken from
    /// <a href="http://www.kr.tuwien.ac.at/staff/eiter/et-archive/cdtr9464.pdf">
    /// Computing Discrete Fréchet Distance</a>:
    /// <pre>
    /// A man is walking a dog on a leash: the man can move
    /// on one curve, the dog on the other; both may vary their
    /// speed, but backtracking is not allowed.
    /// </pre>
    /// </summary>
    /// <remarks>
    /// Its metric is better than the Hausdorff distance
    /// because it takes the directions of the curves into account.
    /// It is possible that two curves have a small Hausdorff but a large
    /// Fréchet distance.
    /// <para/>
    /// This implementation is based on the following optimized Fréchet distance algorithm:
    /// <pre>Thomas Devogele, Maxence Esnault, Laurent Etienne. Distance discrète de Fréchet optimisée. Spatial
    /// Analysis and Geomatics (SAGEO), Nov 2016, Nice, France. hal-02110055</pre>
    /// <para/>
    /// Several matrix storage implementations are provided
    /// <para/>
    /// Additional information:
    /// <list type="bullet">
    /// <item><description><a href="https://en.wikipedia.org/wiki/Fr%C3%A9chet_distance">Fréchet distance</a></description></item>
    /// <item><description><a href="http://www.kr.tuwien.ac.at/staff/eiter/et-archive/cdtr9464.pdf">Computing Discrete Fréchet Distance</a></description></item>
    /// <item><description><a href="https://hal.archives-ouvertes.fr/hal-02110055/document">Distance discrète de Fréchet optimisée</a></description></item>
    /// <item><description><a href="https://towardsdatascience.com/fast-discrete-fr%C3%A9chet-distance-d6b422a8fb77">Fast Discrete Fréchet Distance</a></description></item></list>
    /// </remarks>
    public class DiscreteFrechetDistance
    {
        /// <summary>
        /// Computes the Discrete Fréchet Distance between two <see cref="Geometry"/>s
        /// using a cartesian distance computation function.
        /// </summary>
        /// <param name="g0">The 1st geometry</param>
        /// <param name="g1">The 2nd geometry</param>
        /// <returns>The cartesian distance between <paramref name="g0"/> and <paramref name="g1"/></returns>
        public static double Distance(Geometry g0, Geometry g1)
        {

            var dist = new DiscreteFrechetDistance(g0, g1);
            return dist.Distance();
        }

        private readonly Geometry _g0;
        private readonly Geometry _g1;
        private PointPairDistance _ptDist;

        /// <summary>
        /// Creates an instance of this class using the provided geometries.
        /// </summary>
        /// <param name="g0">A geometry</param>
        /// <param name="g1">A geometry</param>
        public DiscreteFrechetDistance(Geometry g0, Geometry g1)
        {
            _g0 = g0;
            _g1 = g1;
        }

        /// <summary>
        /// Computes the <c>Discrete Fréchet Distance</c> between the input geometries
        /// </summary>
        /// <returns>The Discrete Fréchet Distance</returns>
        private double Distance()
        {
            if (_ptDist != null)
                return _ptDist.Distance;

            var coords0 = _g0.Coordinates;
            var coords1 = _g1.Coordinates;

            var distances = CreateMatrixStorage(coords0.Length, coords1.Length);
            int[] diagonal = BresenhamDiagonal(coords0.Length, coords1.Length);

            var distanceToPair = new Dictionary<double, int[]>();
            ComputeCoordinateDistances(coords0, coords1, diagonal, distances, distanceToPair);
            _ptDist = ComputeFrechet(coords0, coords1, diagonal, distances, distanceToPair);

            return _ptDist.Distance;
        }

        /// <summary>
        /// Creates a matrix to store the computed distances
        /// </summary>
        /// <param name="rows">The number of rows</param>
        /// <param name="cols">The number of cols</param>
        /// <returns>A matrix storage</returns>
        private static MatrixStorage CreateMatrixStorage(int rows, int cols)
        {

            int max = Math.Max(rows, cols);
            // NOTE: these constraints need to be verified
            if (max < 1024)
                return new RectMatrix(rows, cols, double.PositiveInfinity);

            return new CsrMatrix(rows, cols, double.PositiveInfinity);
        }

        /// <summary>
        /// Gets the pair of <see cref="Coordinate"/>s at which the distance is obtained.
        /// </summary>
        /// <returns>The pair of <c>Coordinate</c>s at which the distance is obtained</returns>
        public Coordinate[] Coordinates
        {
            get
            {
                if (_ptDist == null)
                    Distance();

                return _ptDist.Coordinates;
            }
        }

        /// <summary>
        /// Computes the Fréchet Distance for the given distance matrix.
        /// </summary>
        /// <param name="coords0">An array of <c>Coordinate</c>s</param>
        /// <param name="coords1">An array of <c>Coordinate</c>s</param>
        /// <param name="diagonal">An array of alternating col/row index values for the diagonal of the distance matrix</param>
        /// <param name="distances">The distance matrix</param>
        /// <param name="distanceToPair">A lookup for coordinate pairs based on a distance</param>
        /// <returns></returns>
        private static PointPairDistance ComputeFrechet(Coordinate[] coords0, Coordinate[] coords1, int[] diagonal,
                                                        MatrixStorage distances, Dictionary<double, int[]> distanceToPair)
        {
            for (int d = 0; d < diagonal.Length; d += 2)
            {
                int i0 = diagonal[d];
                int j0 = diagonal[d + 1];

                for (int i = i0; i < coords0.Length; i++)
                {
                    if (distances.IsValueSet(i, j0))
                    {
                        double dist = GetMinDistanceAtCorner(distances, i, j0);
                        if (dist > distances[i, j0])
                            distances[i, j0] = dist;
                    }
                    else
                    {
                        break;
                    }
                }
                for (int j = j0 + 1; j < coords1.Length; j++)
                {
                    if (distances.IsValueSet(i0, j))
                    {
                        double dist = GetMinDistanceAtCorner(distances, i0, j);
                        if (dist > distances[i0, j])
                            distances[i0, j] = dist;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            var result = new PointPairDistance();
            double distance = distances[coords0.Length - 1, coords1.Length - 1];
            int[] index = distanceToPair[distance];
            if (index == null)
                throw new InvalidOperationException("Pair of points not recorded for computed distance");

            result.Initialize(coords0[index[0]], coords1[index[1]], distance);

            return result;
        }

        /// <summary>
        /// Returns the minimum distance at the corner (<paramref name="i"/>, <paramref name="j"/>}).
        /// </summary>
        /// <param name="matrix">A (sparse) matrix</param>
        /// <param name="i">The row index</param>
        /// <param name="j">The column index</param>
        /// <returns>The minimum distance</returns>
        private static double GetMinDistanceAtCorner(MatrixStorage matrix, int i, int j)
        {
            if (i > 0 && j > 0)
            {
                double d0 = matrix[i - 1, j - 1];
                double d1 = matrix[i - 1, j];
                double d2 = matrix[i, j - 1];
                return Math.Min(Math.Min(d0, d1), d2);
            }
            if (i == 0 && j == 0)
                return matrix[0, 0];

            if (i == 0)
                return matrix[0, j - 1];

            // j == 0
            return matrix[i - 1, 0];
        }

        /// <summary>
        /// Computes relevant distances between pairs of <see cref="Coordinate"/>s for the
        /// computation of the <c>Discrete Fréchet Distance</c>.
        /// </summary>
        /// <param name="coords0">An array of <c>Coordinate</c>s</param>
        /// <param name="coords1">An array of <c>Coordinate</c>s</param>
        /// <param name="diagonal">An array of alternating col/row index values for the diagonal of the distance matrix</param>
        /// <param name="distances">The distance matrix</param>
        /// <param name="distanceToPair">A lookup for coordinate pairs based on a distance</param>
        private void ComputeCoordinateDistances(Coordinate[] coords0, Coordinate[] coords1, int[] diagonal,
                                                MatrixStorage distances, Dictionary<double, int[]> distanceToPair)
        {
            int numDiag = diagonal.Length;
            double maxDistOnDiag = 0d;
            int imin = 0, jmin = 0;
            int numCoords0 = coords0.Length;
            int numCoords1 = coords1.Length;

            // First compute all the distances along the diagonal.
            // Record the maximum distance.

            for (int k = 0; k < numDiag; k += 2)
            {
                int i0 = diagonal[k];
                int j0 = diagonal[k + 1];
                double diagDist = coords0[i0].Distance(coords1[j0]);
                if (diagDist > maxDistOnDiag) maxDistOnDiag = diagDist;
                distances[i0, j0] = diagDist;
                distanceToPair[diagDist] = new int[] { i0, j0 };
            }

            // Check for distances shorter than maxDistOnDiag along the diagonal
            for (int k = 0; k < numDiag - 2; k += 2)
            {
                // Decode index
                int i0 = diagonal[k];
                int j0 = diagonal[k + 1];

                // Get reference coordinates for col and row
                var coord0 = coords0[i0];
                var coord1 = coords1[j0];

                // Check for shorter distances in this row
                int i = i0 + 1;
                for (; i < numCoords0; i++)
                {
                    if (!distances.IsValueSet(i, j0))
                    {
                        double dist = coords0[i].Distance(coord1);
                        if (dist < maxDistOnDiag || i < imin)
                        {
                            distances[i, j0] = dist;
                            distanceToPair[dist] = new int[] { i, j0 };
                        }
                        else
                            break;
                    }
                    else
                        break;
                }
                imin = i;

                // Check for shorter distances in this column
                int j = j0 + 1;
                for (; j < numCoords1; j++)
                {
                    if (!distances.IsValueSet(i0, j))
                    {
                        double dist = coord0.Distance(coords1[j]);
                        if (dist < maxDistOnDiag || j < jmin)
                        {
                            distances[i0, j] = dist;
                            distanceToPair[dist] = new int[] { i0, j };
                        }
                        else
                            break;
                    }
                    else
                        break;
                }
                jmin = j;
            }
        }

        /// <summary>
        /// Computes the indices for the diagonal of a <c>numCols x numRows</c> grid
        /// using the <a href="https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm">
        /// Bresenham's line algorithm</a>.
        /// </summary>
        /// <param name="numCols">The number of columns</param>
        /// <param name="numRows">The number of rows</param>
        /// <returns>A packed array of column and row indices.</returns>
        internal static int[] BresenhamDiagonal(int numCols, int numRows)
        {
            int dim = Math.Max(numCols, numRows);
            int[] diagXY = new int[2 * dim];

            int dx = numCols - 1;
            int dy = numRows - 1;
            int err;
            int i = 0;
            if (numCols > numRows)
            {
                int y = 0;
                err = 2 * dy - dx;
                for (int x = 0; x < numCols; x++)
                {
                    diagXY[i++] = x;
                    diagXY[i++] = y;
                    if (err > 0)
                    {
                        y += 1;
                        err -= 2 * dx;
                    }
                    err += 2 * dy;
                }
            }
            else
            {
                int x = 0;
                err = 2 * dx - dy;
                for (int y = 0; y < numRows; y++)
                {
                    diagXY[i++] = x;
                    diagXY[i++] = y;
                    if (err > 0)
                    {
                        x += 1;
                        err -= 2 * dy;
                    }
                    err += 2 * dx;
                }
            }
            return diagXY;
        }

        /// <summary>
        /// Abstract base class for storing 2d matrix data
        /// </summary>
        internal abstract class MatrixStorage
        {
            /// <summary>
            /// Gets a value indicating the number of rows
            /// </summary>
            protected int NumRows { get; }
            /// <summary>
            /// Gets a value indicating the number of columns
            /// </summary>
            protected int NumCols { get; }

            /// <summary>
            /// Gets a value indicating the default value
            /// </summary>
            protected double DefaultValue { get; }

            /// <summary>Creates an instance of this class</summary>
            /// <param name="numRows">The number of rows</param>
            /// <param name="numCols">The number of columns</param>
            /// <param name="defaultValue">A default value</param>
            protected MatrixStorage(int numRows, int numCols, double defaultValue)
            {
                NumRows = numRows;
                NumCols = numCols;
                DefaultValue = defaultValue;
            }

            /// <summary>
            /// Gets or sets a value for the cell <paramref name="i"/>, <paramref name="j"/>
            /// </summary>
            /// <param name="i">The row index</param>
            /// <param name="j">The column index</param>
            /// <returns>The value of the cell <paramref name="i"/>, <paramref name="j"/></returns>
            public abstract double this[int i, int j] { get; set; }

            /// <summary>
            /// Gets a flag indicating if the matrix has a set value, e.g. one that is different
            /// than <see cref="DefaultValue"/>.
            /// </summary>
            /// <returns>A flag indicating if the matrix has a set value</returns>
            public abstract bool IsValueSet(int i, int j);

#if DEBUG
            public override string ToString() {
              var sb = new System.Text.StringBuilder("[");
              for (int i = 0; i < NumRows; i++)
              {
                sb.Append('[');
                for(int j = 0; j < NumCols; j++)
                {
                  if (j > 0)
                    sb.Append(", ");
                  sb.Append(string.Format(System.Globalization.NumberFormatInfo.InvariantInfo, "{0,8:F}", this[i, j]));
                }
                sb.Append(']');
                if (i < NumRows - 1) sb.Append(",\n");
              }
              sb.Append(']');
              return sb.ToString();
            }
#endif
        }

        /// <summary>Straight forward implementation of a rectangular matrix</summary>
        internal sealed class RectMatrix : MatrixStorage
        {

            private readonly double[] _matrix;

            /// <summary>
            /// Creates an instance of this matrix using the given number of rows and columns.
            /// A default value can be specified.
            /// </summary>
            /// <param name="numRows">The number of rows</param>
            /// <param name="numCols">The number of columns</param>
            /// <param name="defaultValue">A default value</param>
            public RectMatrix(int numRows, int numCols, double defaultValue)
                    : base(numRows, numCols, defaultValue)
            {
                _matrix = new double[numRows * numCols];
                for (int i = 0; i < _matrix.Length; i++)
                    _matrix[i] = defaultValue;
            }

            public override double this[int i, int j]
            {
                get => _matrix[i * NumCols + j];
                set => _matrix[i * NumCols + j] = value;
            }

            public override bool IsValueSet(int i, int j)
            {
                return BitConverter.DoubleToInt64Bits(this[i, j]) != BitConverter.DoubleToInt64Bits(DefaultValue);
            }
        }

        /// <summary>
        /// A matrix implementation that adheres to the
        /// <a href="https://en.wikipedia.org/wiki/Sparse_matrix#Compressed_sparse_row_(CSR,_CRS_or_Yale_format)">
        /// Compressed sparse row format</a>.<br/>
        /// Note: Unfortunately not as fast as expected.
        /// </summary>
        internal sealed class CsrMatrix : MatrixStorage
        {

            private double[] _v;
            private readonly int[] _ri;
            private int[] _ci;

            /// <summary>
            /// Creates an instance of this matrix using the given number of rows and columns.
            /// A default value can be specified.
            /// </summary>
            /// <param name="numRows">The number of rows</param>
            /// <param name="numCols">The number of columns</param>
            /// <param name="defaultValue">A default value</param>
            public CsrMatrix(int numRows, int numCols, double defaultValue)
                : this(numRows, numCols, defaultValue, ExpectedValuesHeuristic(numRows, numCols))
            {
            }

            /// <summary>
            /// Creates an instance of this matrix using the given number of rows and columns.
            /// A default value can be specified as well as the number of values expected.
            /// </summary>
            /// <param name="numRows">The number of rows</param>
            /// <param name="numCols">The number of columns</param>
            /// <param name="defaultValue">A default value</param>
            /// <param name="expectedValues">The amount of expected values</param>
            public CsrMatrix(int numRows, int numCols, double defaultValue, int expectedValues)
                : base(numRows, numCols, defaultValue)
            {
                _v = new double[expectedValues];
                _ci = new int[expectedValues];
                _ri = new int[numRows + 1];
            }

            /// <summary>
            /// Computes an initial value for the number of expected values
            /// </summary>
            /// <param name="numRows">The number of rows</param>
            /// <param name="numCols">The number of columns</param>
            /// <returns>The expected number of values in the sparse matrix</returns>
            private static int ExpectedValuesHeuristic(int numRows, int numCols)
            {
                int max = Math.Max(numRows, numCols);
                return max * max / 10;
            }

            private int IndexOf(int i, int j)
            {
                int cLow = _ri[i];
                int cHigh = _ri[i + 1];
                if (cHigh <= cLow) return ~cLow;

                return Array.BinarySearch(this._ci, cLow, cHigh - cLow, j);
            }

            public override double this[int i, int j]
            {
                get => Get(i, j);
                set => Set(i, j, value);
            }

            private double Get(int i, int j)
            {

                // get the index in the vector
                int vi = IndexOf(i, j);

                // if the vector index is negative, return default value
                if (vi < 0)
                    return DefaultValue;

                return _v[vi];
            }

            private void Set(int i, int j, double value)
            {

                // get the index in the vector
                int vi = IndexOf(i, j);

                // do we already have a value?
                if (vi < 0)
                {
                    // no, we don't, we need to ensure space!
                    EnsureCapacity(_ri[this.NumRows] + 1);

                    // update row indices
                    for (int ii = i + 1; ii <= NumRows; ii++)
                        _ri[ii] += 1;

                    // move and update column indices, move values
                    vi = ~vi;
                    for (int ii = _ri[NumRows]; ii > vi; ii--)
                    {
                        _ci[ii] = _ci[ii - 1];
                        _v[ii] = _v[ii - 1];
                    }

                    // insert column index
                    _ci[vi] = j;
                }

                // set the new value
                _v[vi] = value;
            }

            public override bool IsValueSet(int i, int j)
            {
                return IndexOf(i, j) >= 0;
            }

            /// <summary>
            /// Ensures that the column index vector (ci) and value vector (v) are sufficiently large.
            /// </summary>
            /// <param name="required">The number of items to store in the matrix</param>
            private void EnsureCapacity(int required)
            {
                if (required < _v.Length)
                    return;

                int increment = Math.Max(NumRows, NumCols);
                Array.Resize(ref _v, _v.Length + increment);
                Array.Resize(ref _ci, _ci.Length + increment);
            }
        }

        /// <summary>
        /// A sparse matrix based on <see cref="Dictionary{TKey, TValue}"/>.
        /// </summary>
        internal sealed class HashMapMatrix : MatrixStorage
        {

            private readonly Dictionary<long, double> _matrix;

            /// <summary>
            /// Creates an instance of this matrix using the given number of rows and columns.
            /// A default value can be specified.
            /// </summary>
            /// <param name="numRows">The number of rows</param>
            /// <param name="numCols">The number of columns</param>
            /// <param name="defaultValue">A default value</param>
            public HashMapMatrix(int numRows, int numCols, double defaultValue)
                : base(numRows, numCols, defaultValue)
            {
                _matrix = new Dictionary<long, double>();
            }

            public override double this[int i, int j]
            {
                get {
                    long key = (long)i << 32 | (long)j;
                    if (_matrix.TryGetValue(key, out double value))
                        return value;
                    return DefaultValue;
                }
                set
                {
                    long key = (long)i << 32 | (long)j;
                    _matrix[key] = value;
                }
            }

            public override bool IsValueSet(int i, int j)
            {
                long key = (long)i << 32 | (long)j;
                return _matrix.ContainsKey(key);
            }
        }
    }

}
