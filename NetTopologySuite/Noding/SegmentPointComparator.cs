using System;
using GeoAPI.Coordinates;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Implements a robust method of comparing the relative position of two points 
    /// along the same segment.
    /// The coordinates are assumed to lie "near" the segment.
    /// This means that this algorithm will only return correct results
    /// if the input coordinates have the same precision and correspond to rounded values
    /// of exact coordinates lying on the segment.
    /// </summary>
    public class SegmentPointComparator
    {
        /// <summary>
        /// Compares two <typeparamref name="TCoordinate"/>s for their relative 
        /// position along a segment lying in the specified <see cref="Octant" />.
        /// </summary>
        /// <returns>
        /// -1 if node0 occurs first, or
        ///  0 if the two nodes are equal, or
        ///  1 if node1 occurs first.
        /// </returns>
        public static Int32 Compare<TCoordinate>(Octants octant, TCoordinate p0, TCoordinate p1)
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
        {
            // nodes can only be equal if their coordinates are equal
            if (p0.Equals(p1))
            {
                return 0;
            }

            Int32 xSign = RelativeSign(p0[Ordinates.X], p1[Ordinates.X]);
            Int32 ySign = RelativeSign(p0[Ordinates.Y], p1[Ordinates.Y]);

            switch (octant)
            {
                case Octants.Zero:
                    return compareValue(xSign, ySign);
                case Octants.One:
                    return compareValue(ySign, xSign);
                case Octants.Two:
                    return compareValue(ySign, -xSign);
                case Octants.Three:
                    return compareValue(-xSign, ySign);
                case Octants.Four:
                    return compareValue(-xSign, -ySign);
                case Octants.Five:
                    return compareValue(-ySign, -xSign);
                case Octants.Six:
                    return compareValue(-ySign, xSign);
                case Octants.Seven:
                    return compareValue(xSign, -ySign);
            }

            Assert.ShouldNeverReachHere("invalid octant value: " + octant);
            return 0;
        }

        public static Int32 RelativeSign(Double x0, Double x1)
        {
            if (x0 < x1)
            {
                return -1;
            }

            if (x0 > x1)
            {
                return 1;
            }

            return 0;
        }

        private static Int32 compareValue(Int32 compareSign0, Int32 compareSign1)
        {
            if (compareSign0 < 0)
            {
                return -1;
            }

            if (compareSign0 > 0)
            {
                return 1;
            }

            if (compareSign1 < 0)
            {
                return -1;
            }

            if (compareSign1 > 0)
            {
                return 1;
            }

            return 0;
        }
    }
}