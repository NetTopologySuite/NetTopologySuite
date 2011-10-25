using System.Collections.Generic;
using GeoAPI.Geometries;

namespace NetTopologySuite.Utilities
{
    /// <summary>
    /// A <c>CoordinateFilter</c> that builds a set of <c>Coordinate</c>s.
    /// The set of coordinates contains no duplicate points.
    /// </summary>
    public class UniqueCoordinateArrayFilter : ICoordinateFilter 
    {
        private readonly List<Coordinate> _list = new List<Coordinate>();

        /// <summary>
        /// Returns the gathered <c>Coordinate</c>s.
        /// </summary>
        public Coordinate[] Coordinates
        {
            get { return _list.ToArray(); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coord"></param>
        public void Filter(Coordinate coord) 
        {
            if (!_list.Contains(coord))
                 _list.Add(coord);            
        }
    }
}
