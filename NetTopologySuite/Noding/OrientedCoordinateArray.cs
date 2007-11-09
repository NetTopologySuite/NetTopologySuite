using System;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    ///  Allows comparing <see cref="Coordinate" /> arrays in an orientation-independent way.
    /// </summary>
    public class OrientedCoordinateArray : IComparable
    {
        private ICoordinate[] pts = null;
        private Boolean orientation = false;

        /// <summary>
        /// Creates a new <see cref="OrientedCoordinateArray" />}
        /// for the given <see cref="Coordinate" /> array.
        /// </summary>
        public OrientedCoordinateArray(ICoordinate[] pts)
        {
            this.pts = pts;
            orientation = Orientation(pts);
        }

        /// <summary>
        /// Computes the canonical orientation for a coordinate array.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the points are oriented forwards, or
        /// <c>false</c>if the points are oriented in reverse.
        /// </returns>
        private static Boolean Orientation(ICoordinate[] pts)
        {
            return CoordinateArrays.IncreasingDirection(pts) == 1;
        }

        /// <summary>
        /// Compares two <see cref="OrientedCoordinateArray" />s for their relative order.
        /// </summary>
        /// <returns>
        /// -1 this one is smaller, or
        ///  0 the two objects are equal, or
        ///  1 this one is greater.
        /// </returns>
        public Int32 CompareTo(object o1)
        {
            OrientedCoordinateArray oca = (OrientedCoordinateArray) o1;
            return CompareOriented(pts, orientation, oca.pts, oca.orientation);
        }

        private static Int32 CompareOriented(ICoordinate[] pts1, Boolean orientation1, ICoordinate[] pts2,
                                             Boolean orientation2)
        {
            Int32 dir1 = orientation1 ? 1 : -1;
            Int32 dir2 = orientation2 ? 1 : -1;
            Int32 limit1 = orientation1 ? pts1.Length : -1;
            Int32 limit2 = orientation2 ? pts2.Length : -1;

            Int32 i1 = orientation1 ? 0 : pts1.Length - 1;
            Int32 i2 = orientation2 ? 0 : pts2.Length - 1;
            while (true)
            {
                Int32 compPt = pts1[i1].CompareTo(pts2[i2]);
                if (compPt != 0)
                {
                    return compPt;
                }

                i1 += dir1;
                i2 += dir2;
                Boolean done1 = i1 == limit1;
                Boolean done2 = i2 == limit2;
                if (done1 && !done2)
                {
                    return -1;
                }
                if (!done1 && done2)
                {
                    return 1;
                }
                if (done1 && done2)
                {
                    return 0;
                }
            }
        }
    }
}