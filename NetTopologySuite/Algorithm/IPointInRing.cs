using System;
using GeoAPI.Coordinates;

namespace NetTopologySuite.Algorithm
{
    /// <summary> 
    /// An interface for classes which test whether a <typeparamref name="TCoordinate"/>
    /// lies inside a ring.
    /// </summary>
    public interface IPointInRing<TCoordinate>
        where TCoordinate : ICoordinate
    {
        Boolean IsInside(TCoordinate pt);
    }
}