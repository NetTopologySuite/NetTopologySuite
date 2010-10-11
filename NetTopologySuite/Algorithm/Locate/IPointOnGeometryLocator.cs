using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.Algorithm.Locate
{
    /// <summary>
    /// An interface for classes which determine the {@link Location} of
    /// points in a <see cref="IGeometry{TCoordinate}"/>.
    /// </summary>
    /// <typeparam name="TCoordinate"></typeparam>
    public interface IPointOnGeometryLocator<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        /// <summary>
        /// Determines the <see cref="Locations"/>  of a point in the <see cref="IGeometry{TCoordinate}"/>.
        /// </summary>
        /// <param name="coordinate">point to test</param>
        /// <returns><see cref="Locations"/>the location of the point in the geometry</returns>
        Locations Locate(TCoordinate coordinate);
    }
}