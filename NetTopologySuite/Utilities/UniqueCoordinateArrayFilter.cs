using System.Collections.Generic;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Utilities
{
    /// <summary>
    /// A <c>CoordinateFilter</c> that builds a set of <c>Coordinate</c>s.
    /// The set of coordinates contains no duplicate points.
    /// </summary>
    public class UniqueCoordinateArrayFilter : ICoordinateFilter 
    {
        private readonly List<ICoordinate> _list = new List<ICoordinate>();

        /// <summary>
        /// Returns the gathered <c>Coordinate</c>s.
        /// </summary>
        public ICoordinate[] Coordinates
        {
            get { return _list.ToArray(); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coord"></param>
        public void Filter(ICoordinate coord) 
        {
            if (!_list.Contains(coord))
                 _list.Add(coord);            
        }
    }
}
