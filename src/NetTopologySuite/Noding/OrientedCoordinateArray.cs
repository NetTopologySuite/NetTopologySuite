using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Noding
{
    /// <summary>
    ///  Allows comparing <see cref="Coordinate" /> arrays in an orientation-independent way.
    /// </summary>
    public class OrientedCoordinateArray : IComparable
    {
        private readonly Coordinate[] _pts;
        private readonly bool _orientation;

        /// <summary>
        /// Creates a new <see cref="OrientedCoordinateArray" />}
        /// for the given <see cref="Coordinate" /> array.
        /// </summary>
        /// <param name="pts"></param>
        public OrientedCoordinateArray(Coordinate[] pts)
        {
            _pts = pts;
            _orientation = Orientation(pts);
        }

        /// <summary>
        /// Computes the canonical orientation for a coordinate array.
        /// </summary>
        /// <param name="pts"></param>
        /// <returns>
        /// <c>true</c> if the points are oriented forwards <br/>
        /// or <c>false</c>if the points are oriented in reverse.
        /// </returns>
        private static bool Orientation(Coordinate[] pts)
        {
            return CoordinateArrays.IncreasingDirection(pts) == 1;
        }

        /// <summary>
        /// Compares two <see cref="OrientedCoordinateArray" />s for their relative order.
        /// </summary>
        /// <param name="o1"></param>
        /// <returns>
        /// -1 this one is smaller;<br/>
        ///  0 the two objects are equal;<br/>
        ///  1 this one is greater.
        /// </returns>
        public int CompareTo(object o1)
        {
            var oca = (OrientedCoordinateArray) o1;
            return CompareOriented(_pts, _orientation, oca._pts, oca._orientation);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pts1"></param>
        /// <param name="orientation1"></param>
        /// <param name="pts2"></param>
        /// <param name="orientation2"></param>
        /// <returns></returns>
        private static int CompareOriented(Coordinate[] pts1, bool orientation1, Coordinate[] pts2, bool orientation2)
        {
            int dir1 = orientation1 ? 1 : -1;
            int dir2 = orientation2 ? 1 : -1;
            int limit1 = orientation1 ? pts1.Length : -1;
            int limit2 = orientation2 ? pts2.Length : -1;

            int i1 = orientation1 ? 0 : pts1.Length - 1;
            int i2 = orientation2 ? 0 : pts2.Length - 1;
            while (true)
            {
                int compPt = pts1[i1].CompareTo(pts2[i2]);
                if (compPt != 0)
                    return compPt;

                i1 += dir1;
                i2 += dir2;
                bool done1 = i1 == limit1;
                bool done2 = i2 == limit2;
                if(done1 && !done2)
                    return -1;
                if(!done1 && done2)
                    return 1;
                if(done1 && done2)
                    return 0;
            }
        }
    }
}
