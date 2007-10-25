using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Utilities
{
    /// <summary>
    /// A <c>CoordinateFilter</c> that creates an array containing every coordinate in a <c>Geometry</c>.
    /// </summary>
    public class CoordinateArrayFilter : ICoordinateFilter 
    {
        ICoordinate[] pts = null;
        int n = 0;

        /// <summary>
        /// Constructs a <c>CoordinateArrayFilter</c>.
        /// </summary>
        /// <param name="size">The number of points that the <c>CoordinateArrayFilter</c> will collect.</param>
        public CoordinateArrayFilter(int size) 
        {
            pts = new ICoordinate[size];
        }

        /// <summary>
        /// Returns the <c>Coordinate</c>s collected by this <c>CoordinateArrayFilter</c>.
        /// </summary>
        public ICoordinate[] Coordinates
        {
            get
            {
                return pts;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coord"></param>
        public void Filter(ICoordinate coord) 
        {
            pts[n++] = (ICoordinate) coord;
        }
    }
}
