using GeoAPI.Geometries;

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
        public virtual Node CreateNode(Coordinate coord)
        {
            return new Node(coord, null);
        }
    }
}
