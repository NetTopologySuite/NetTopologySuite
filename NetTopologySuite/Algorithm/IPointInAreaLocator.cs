using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.Algorithm
{
    ///<summary>
    /// An interface for classes which determine the <see cref="Locations"/> of points in an areal <see cref="IGeometry{TCoordinate}"/>
    ///</summary>
    public interface IPointInAreaLocator<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        /**
         * Determines the {@link Location} of a point in an areal {@link Geometry}.
         * 
         * @param p the point to test
         * @return the location of the point in the geometry  
         */
        Locations Locate(TCoordinate p);
    }
}