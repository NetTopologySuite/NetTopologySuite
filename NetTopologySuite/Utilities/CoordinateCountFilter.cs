using GeoAPI.Geometries;
namespace NetTopologySuite.Utilities
{
    /// <summary>
    /// A <c>CoordinateFilter</c> that counts the total number of coordinates
    /// in a <c>Geometry</c>.
    /// </summary>
    public class CoordinateCountFilter : ICoordinateFilter
    {
        /*
        /// <summary>
        ///
        /// </summary>
        public CoordinateCountFilter() { }
        */
        /// <summary>
        /// Returns the result of the filtering.
        /// </summary>
        public int Count { get; private set; }
        /// <summary>
        ///
        /// </summary>
        /// <param name="coord"></param>
        public void Filter(Coordinate coord)
        {
            Count++;
        }
    }
}
