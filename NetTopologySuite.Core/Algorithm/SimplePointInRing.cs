using GeoAPI.Geometries;

namespace NetTopologySuite.Algorithm
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
        private readonly Coordinate[] pts;

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
        public bool IsInside(Coordinate pt)
        {
            return CGAlgorithms.IsPointInRing(pt, pts);
        }
    }
}
