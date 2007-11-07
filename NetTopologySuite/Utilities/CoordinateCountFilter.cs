using System;

namespace GisSharpBlog.NetTopologySuite.Utilities
{
    /// <summary>
    /// A <c>CoordinateFilter</c> that counts the total number of coordinates
    /// in a <c>Geometry</c>.
    /// </summary>
    public class CoordinateCountFilter : ICoordinateFilter
    {
        private Int32 n = 0;

        /// <summary>
        /// 
        /// </summary>
        public CoordinateCountFilter() {}

        /// <summary>
        /// Returns the result of the filtering.
        /// </summary>
        public Int32 Count
        {
            get { return n; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coord"></param>
        public void Filter(ICoordinate coord)
        {
            n++;
        }
    }
}