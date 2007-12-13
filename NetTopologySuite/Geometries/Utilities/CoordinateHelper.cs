using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries.Utilities
{
    public static class CoordinateHelper
    {
        public static Boolean IsEmpty<TCoordinate>(TCoordinate coordinate)
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<TCoordinate>, IConvertible
        {
            if (typeof (TCoordinate).IsValueType)
            {
                return coordinate.IsEmpty;
            }
            else
            {
                return ReferenceEquals(coordinate, null) || coordinate.IsEmpty;
            }
        }

        public static IEnumerable<TCoordinate> RemoveRepeatedPoints<TCoordinate>(IEnumerable<TCoordinate> points)
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<TCoordinate>, IConvertible
        {
            TCoordinate lastCoordinate = default(TCoordinate);

            foreach (TCoordinate point in points)
            {
                if (!point.Equals(lastCoordinate))
                {
                    yield return point;
                }

                lastCoordinate = point;
            }
        }
    }
}