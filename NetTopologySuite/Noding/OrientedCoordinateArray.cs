using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    ///  Allows comparing <typeparamref name="TCoordinate"/> arrays in an orientation-independent way.
    /// </summary>
    public class OrientedCoordinateArray<TCoordinate> : IComparable<OrientedCoordinateArray<TCoordinate>>, IEquatable<OrientedCoordinateArray<TCoordinate>>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private readonly IEnumerable<TCoordinate> _coordinates = null;
        private readonly Boolean _orientation = false;

        /// <summary>
        /// Creates a new <see cref="OrientedCoordinateArray{TCoordinate}" />}
        /// for the given <typeparamref name="TCoordinate"/> set.
        /// </summary>
        public OrientedCoordinateArray(IEnumerable<TCoordinate> pts)
        {
            _coordinates = pts;
            _orientation = orientation(pts);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as OrientedCoordinateArray<TCoordinate>);
        }

        /// <summary>
        /// Compares two <see cref="OrientedCoordinateArray{TCoordinate}" />s 
        /// for their relative order.
        /// </summary>
        /// <returns>
        /// -1 this one is smaller, or
        ///  0 the two objects are equal, or
        ///  1 this one is greater.
        /// </returns>
        public Int32 CompareTo(OrientedCoordinateArray<TCoordinate> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            return compareOriented(_coordinates, _orientation, other._coordinates, other._orientation);
        }

        private static Int32 compareOriented(IEnumerable<TCoordinate> pts1, Boolean orientation1, 
            IEnumerable<TCoordinate> pts2, Boolean orientation2)
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

        #region IEquatable<OrientedCoordinateArray<TCoordinate>> Members

        public bool Equals(OrientedCoordinateArray<TCoordinate> other)
        {
            if(ReferenceEquals(other, null))
            {
                return false;
            }

            return CompareTo(other) == 0;
        }

        #endregion

        /// <summary>
        /// Computes the canonical orientation for a coordinate array.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the points are oriented forwards, or
        /// <c>false</c>if the points are oriented in reverse.
        /// </returns>
        private static Boolean orientation(IEnumerable<TCoordinate> pts)
        {
            return CoordinateArrays.IncreasingDirection(pts) == 1;
        }
    }
}