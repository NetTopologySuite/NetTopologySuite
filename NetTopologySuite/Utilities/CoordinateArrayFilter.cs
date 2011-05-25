using GeoAPI.Geometries;

namespace NetTopologySuite.Utilities
{
    /// <summary>
    /// A <c>CoordinateFilter</c> that creates an array containing every coordinate in a <c>Geometry</c>.
    /// </summary>
    public class CoordinateArrayFilter : ICoordinateFilter 
    {
        readonly ICoordinate[] _pts;
        int _n;

        /// <summary>
        /// Constructs a <c>CoordinateArrayFilter</c>.
        /// </summary>
        /// <param name="size">The number of points that the <c>CoordinateArrayFilter</c> will collect.</param>
        public CoordinateArrayFilter(int size) 
        {
            _pts = new ICoordinate[size];
        }

        /// <summary>
        /// Returns the <c>Coordinate</c>s collected by this <c>CoordinateArrayFilter</c>.
        /// </summary>
        public ICoordinate[] Coordinates
        {
            get
            {
                return _pts;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coord"></param>
        public void Filter(ICoordinate coord) 
        {
            _pts[_n++] = coord;
        }
    }
}
