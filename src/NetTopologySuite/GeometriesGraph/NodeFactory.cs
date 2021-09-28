using NetTopologySuite.Geometries;

namespace NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// A Factory to create <see cref="Node"/>s.
    /// </summary>
    public class NodeFactory
    {
        /// <summary>
        /// The basic node constructor does not allow for incident edges.
        /// </summary>
        /// <param name="coord">A <c>Coordinate</c></param>
        /// <returns>The created <c>Node</c></returns>
        public virtual Node CreateNode(Coordinate coord)
        {
            return new Node(coord, null);
        }
    }
}
