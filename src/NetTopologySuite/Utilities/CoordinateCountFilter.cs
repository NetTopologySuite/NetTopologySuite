using NetTopologySuite.Geometries;

namespace NetTopologySuite.Utilities
{
    /// <summary>
    /// A <c>CoordinateFilter</c> that counts the total number of coordinates
    /// in a <c>Geometry</c>.
    /// </summary>
    public class CoordinateCountFilter : ICoordinateFilter
    {
        private int _n;

        /*
        /// <summary>
        ///
        /// </summary>
        public CoordinateCountFilter() { }
        */
        /// <summary>
        /// Returns the result of the filtering.
        /// </summary>
        public int Count => _n;

        /// <summary>
        ///
        /// </summary>
        /// <param name="coord"></param>
        public void Filter(Coordinate coord)
        {
            _n++;
        }
    }
}
