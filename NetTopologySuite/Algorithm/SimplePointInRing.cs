using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Tests whether a <c>Coordinate</c> lies inside
    /// a ring, using a linear-time algorithm.
    /// </summary>
    public class SimplePointInRing : IPointInRing
    {
        /// <summary>
        /// 
        /// </summary>
        private ICoordinate[] pts;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ring"></param>
        public SimplePointInRing(ILinearRing ring)
        {
            pts = ring.Coordinates;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public bool IsInside(ICoordinate pt)
        {
            return CGAlgorithms.IsPointInRing(pt, pts);
        }
    }
}
