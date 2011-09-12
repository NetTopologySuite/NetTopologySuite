#if useFullGeoAPI
using GeoAPI.Geometries;
#else
using ICoordinate = NetTopologySuite.Geometries.Coordinate;
#endif

namespace NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// 
    /// </summary>
    public class NodeFactory
    {
        /// <summary> 
        /// The basic node constructor does not allow for incident edges.
        /// </summary>
        /// <param name="coord"></param>
        public virtual Node CreateNode(ICoordinate coord)
        {
            return new Node(coord, null);
        }
    }
}
