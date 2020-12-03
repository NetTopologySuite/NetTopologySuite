using System.Collections.Generic;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Compares two <see cref="CoordinateSequence"/>s.
    /// </summary><remarks>
    /// <para>
    /// For sequences of the same dimension, the ordering is lexicographic.
    /// Otherwise, lower dimensions are sorted before higher.
    /// The dimensions compared can be limited; if this is done
    /// ordinate dimensions above the limit will not be compared.
    /// </para>
    /// <para>
    /// If different behaviour is required for comparing size, dimension,
    /// or coordinate values, any or all methods can be overridden.</para>
    /// </remarks>
    public class CoordinateSequenceComparator : IComparer<CoordinateSequence>
    {
        /// <summary>
        /// Compare two <c>double</c>s, allowing for NaN values.
        /// NaN is treated as being less than any valid number.
        /// </summary>
        /// <param name="a">A <c>double</c></param>
        /// <param name="b">A <c>double</c></param>
        /// <returns>-1, 0, or 1 depending on whether a is less than, equal to or greater than b</returns>
        public static int Compare(double a, double b)
        {
            if (a < b) return -1;
            if (a > b) return 1;

            if (double.IsNaN(a))
            {
                if (double.IsNaN(b)) return 0;
                return -1;
            }

            if (double.IsNaN(b)) return 1;
            return 0;
        }

        /// <summary>
        /// The number of dimensions to test
        /// </summary>
        protected int DimensionLimit;

        /// <summary>
        /// Creates a comparator which will test all dimensions.
        /// </summary>
        public CoordinateSequenceComparator()
        {
            DimensionLimit = int.MaxValue;
        }

        /// <summary>
        /// Creates a comparator which will test only the specified number of dimensions.
        /// </summary>
        /// <param name="dimensionLimit">The number of dimensions to test</param>
        public CoordinateSequenceComparator(int dimensionLimit)
        {
            DimensionLimit = dimensionLimit;
        }

        /// <summary>
        /// Compares two <see cref="CoordinateSequence" />s for relative order.
        /// </summary>
        /// <param name="o1">A coordinate sequence</param>
        /// <param name="o2">A coordinate sequence</param>
        /// <returns>-1, 0, or 1 depending on whether o1 is less than, equal to, or greater than o2</returns>
        public int Compare(object o1, object o2)
        {
            var s1 = (CoordinateSequence)o1;
            var s2 = (CoordinateSequence)o2;
            return Compare(s1, s2);
        }

        /// <summary>
        /// Compares the same coordinate of two <see cref="CoordinateSequence"/>s
        /// </summary>
        /// <param name="s1">A coordinate sequence</param>
        /// <param name="s2">A coordinate sequence</param>
        /// <param name="i">The index of the coordinate to test</param>
        /// <param name="dimension">the number of dimensions to test</param>
        protected int CompareCoordinate(CoordinateSequence s1, CoordinateSequence s2, int i, int dimension)
        {
            for (int d = 0; d < dimension; d++)
            {
                double ord1 = s1.GetOrdinate(i, d);
                double ord2 = s2.GetOrdinate(i, d);
                int comp = Compare(ord1, ord2);
                if (comp != 0) return comp;
            }
            return 0;
        }

        /// <summary>
        /// Compares two <see cref="CoordinateSequence"/>s for relative order.
        /// </summary>
        /// <param name="s1">A coordinate sequence</param>
        /// <param name="s2">A coordinate sequence</param>
        /// <returns>-1, 0, or 1 depending on whether s1 is less than, equal to, or greater than s2</returns>
        public int Compare(CoordinateSequence s1, CoordinateSequence s2)
        {
            int size1 = s1.Count;
            int size2 = s2.Count;

            int dim1 = s1.Dimension;
            int dim2 = s2.Dimension;

            int minDim = dim1;
            if (dim2 < minDim)
                minDim = dim2;
            bool dimLimited = false;
            if (DimensionLimit <= minDim)
            {
                minDim = DimensionLimit;
                dimLimited = true;
            }

            // lower dimension is less than higher
            if (!dimLimited)
            {
                if (dim1 < dim2) return -1;
                if (dim1 > dim2) return 1;
            }

            // lexicographic ordering of point sequences
            int i = 0;
            while (i < size1 && i < size2)
            {
                int ptComp = CompareCoordinate(s1, s2, i, minDim);
                if (ptComp != 0) return ptComp;
                i++;
            }
            if (i < size1) return 1;
            if (i < size2) return -1;

            return 0;
        }
    }
}
