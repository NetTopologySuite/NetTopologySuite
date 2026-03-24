using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;

namespace NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// Creates nodes for use in the <see cref="PlanarGraph"/>s constructed during
    /// buffer operations.
    /// </summary>
    internal class BufferNodeFactory : NodeFactory
    {
        /// <inheritdoc/>
        public override Node CreateNode(Coordinate coord)
        {
            return new Node(coord, new DirectedEdgeStar());
        }
    }
}
