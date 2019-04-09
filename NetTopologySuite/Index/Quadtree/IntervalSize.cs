using System;

namespace NetTopologySuite.Index.Quadtree
{
    /// <summary>
    /// Provides a test for whether an interval is
    /// so small it should be considered as zero for the purposes of
    /// inserting it into a binary tree.
    /// The reason this check is necessary is that round-off error can
    /// cause the algorithm used to subdivide an interval to fail, by
    /// computing a midpoint value which does not lie strictly between the
    /// endpoints.
    /// </summary>
    public class IntervalSize
    {
        /// <summary>
        /// Only static methods!
        /// </summary>
        private IntervalSize() { }

        /// <summary>
        /// This value is chosen to be a few powers of 2 less than the
        /// number of bits available in the double representation (i.e. 53).
        /// This should allow enough extra precision for simple computations to be correct,
        /// at least for comparison purposes.
        /// </summary>
        public const int MinBinaryExponent = -50;

        /// <summary>
        /// Computes whether the interval [min, max] is effectively zero width.
        /// I.e. the width of the interval is so much less than the
        /// location of the interval that the midpoint of the interval cannot be
        /// represented precisely.
        /// </summary>
        public static bool IsZeroWidth(double min, double max)
        {
            double width = max - min;
            if (width == 0.0)
                return true;
            double maxAbs = Math.Max(Math.Abs(min), Math.Abs(max));
            double scaledInterval = width / maxAbs;
            int level = DoubleBits.GetExponent(scaledInterval);
            return level <= MinBinaryExponent;
        }
    }
}
