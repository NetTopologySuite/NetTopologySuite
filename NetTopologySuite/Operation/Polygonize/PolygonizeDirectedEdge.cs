using NetTopologySuite.Geometries;
using NetTopologySuite.Planargraph;

namespace NetTopologySuite.Operation.Polygonize
{
    /// <summary>
    /// A <c>DirectedEdge</c> of a <c>PolygonizeGraph</c>, which represents
    /// an edge of a polygon formed by the graph.
    /// May be logically deleted from the graph by setting the <c>marked</c> flag.
    /// </summary>
    public class PolygonizeDirectedEdge : DirectedEdge
    {
        private long label = -1;

        /// <summary>
        /// Constructs a directed edge connecting the <c>from</c> node to the
        /// <c>to</c> node.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="directionPt">
        /// Specifies this DirectedEdge's direction (given by an imaginary
        /// line from the <c>from</c> node to <c>directionPt</c>).
        /// </param>
        /// <param name="edgeDirection">
        /// Whether this DirectedEdge's direction is the same as or
        /// opposite to that of the parent Edge (if any).
        /// </param>
        public PolygonizeDirectedEdge(Node from, Node to, Coordinate directionPt, bool edgeDirection)
            : base(from, to, directionPt, edgeDirection) { }

        /// <summary>
        /// Returns the identifier attached to this directed edge.
        /// Attaches an identifier to this directed edge.
        /// </summary>
        public long Label
        {
            get => label;
            set => label = value;
        }

        /// <summary>
        /// Returns the next directed edge in the EdgeRing that this directed edge is a member of.
        /// Sets the next directed edge in the EdgeRing that this directed edge is a member of.
        /// </summary>
        public PolygonizeDirectedEdge Next { get; set; }

        /// <summary>
        /// Returns the ring of directed edges that this directed edge is
        /// a member of, or null if the ring has not been set.
        /// </summary>
        public bool IsInRing => Ring != null;

        /// <summary>
        /// Gets/Sets the ring of directed edges that this directed edge is
        /// a member of.
        /// </summary>
        public EdgeRing Ring { get; set; }
    }
}
