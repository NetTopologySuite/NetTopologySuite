#if useFullGeoAPI
using GeoAPI.Geometries;
#else
using ICoordinate = NetTopologySuite.Geometries.Coordinate;
#endif

namespace NetTopologySuite.Algorithm
{
    /// <summary> 
    /// An interface for classes which test whether a <c>Coordinate</c> lies inside a ring.
    /// </summary>
    /// <see cref="Locate.IPointOnGeometryLocator"/> for more general functionality
    public interface IPointInRing
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        bool IsInside(ICoordinate pt);
    }
}
