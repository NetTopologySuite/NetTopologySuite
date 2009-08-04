using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    public class GeometryReferenceEqualityComparer<TCoordinate> : IEqualityComparer<IGeometry<TCoordinate>>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        public static readonly GeometryReferenceEqualityComparer<TCoordinate> Default =
            new GeometryReferenceEqualityComparer<TCoordinate>();

        #region IEqualityComparer<IGeometry<TCoordinate>> Members

        public Boolean Equals(IGeometry<TCoordinate> x, IGeometry<TCoordinate> y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(IGeometry<TCoordinate> obj)
        {
            // Thanks, Rotor 2.0!
            return RuntimeHelpers.GetHashCode(obj);
        }

        #endregion
    }
}