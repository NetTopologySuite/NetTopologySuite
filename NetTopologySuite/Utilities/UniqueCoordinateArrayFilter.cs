using System.Collections;
using GeoAPI.Geometries;
using Iesi_NTS.Collections;

namespace GisSharpBlog.NetTopologySuite.Utilities
{
    /// <summary>
    /// A <c>CoordinateFilter</c> that builds a set of <c>Coordinate</c>s.
    /// The set of coordinates contains no duplicate points.
    /// </summary>
    public class UniqueCoordinateArrayFilter : ICoordinateFilter 
    {
        private ISet table = new SortedSet();
        private ArrayList list = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public UniqueCoordinateArrayFilter() { }

        /// <summary>
        /// Returns the gathered <c>Coordinate</c>s.
        /// </summary>
        public ICoordinate[] Coordinates
        {
            get
            {
                return (ICoordinate[]) list.ToArray(typeof(ICoordinate));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coord"></param>
        public void Filter(ICoordinate coord) 
        {
            if (!table.Contains(coord)) 
            {
                list.Add(coord);
                table.Add(coord);
            }
        }
    }
}
