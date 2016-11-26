using GeoAPI.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary> 
    /// An interface for classes which test whether a <c>Coordinate</c> lies inside a ring.
    /// </summary>
    /// <see cref="Locate.IPointOnGeometryLocator"/>
    public interface IPointInRing
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        bool IsInside(Coordinate pt);
    }
}
