using System.Collections;
using System.Linq;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Utilities
{
    /// <summary>
    /// A <c>CoordinateFilter</c> that builds a set of <c>Coordinate</c>s.
    /// The set of coordinates contains no duplicate points.
    /// </summary>
    public class UniqueCoordinateArrayFilter : ICoordinateFilter 
    {
        private readonly ArrayList list = new ArrayList();

        /// <summary>
        /// Returns the gathered <c>Coordinate</c>s.
        /// </summary>
        public ICoordinate[] Coordinates
        {
            get { return list.Cast<ICoordinate>().ToArray(); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coord"></param>
        public void Filter(ICoordinate coord) 
        {
            if (!list.Contains(coord))
                 list.Add(coord);            
        }
    }
}
