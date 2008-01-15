using System;
using GeoAPI.Coordinates;
using GisSharpBlog.NetTopologySuite.Planargraph;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Polygonize
{
    /// <summary>
    /// A <c>DirectedEdge</c> of a <c>PolygonizeGraph</c>, which represents
    /// an edge of a polygon formed by the graph.
    /// May be logically deleted from the graph by setting the <c>marked</c> flag.
    /// </summary>
    public class PolygonizeDirectedEdge<TCoordinate> : DirectedEdge<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                    IComputable<Double, TCoordinate>, IConvertible
    {
        private EdgeRing<TCoordinate> _edgeRing = null;
        private PolygonizeDirectedEdge<TCoordinate> _next = null;
        private Int64 _label = -1;

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
        public PolygonizeDirectedEdge(Node<TCoordinate> from, Node<TCoordinate> to, TCoordinate directionPt, Boolean edgeDirection)
            : base(from, to, directionPt, edgeDirection) {}

        /// <summary> 
        /// Returns the identifier attached to this directed edge.
        /// Attaches an identifier to this directed edge.
        /// </summary>
        public Int64 Label
        {
            get { return _label; }
            set { _label = value; }
        }

        /// <summary>
        /// Returns the next directed edge in the EdgeRing that this directed edge is a member of.
        /// Sets the next directed edge in the EdgeRing that this directed edge is a member of.
        /// </summary>
        public PolygonizeDirectedEdge<TCoordinate> Next
        {
            get { return _next; }
            set { _next = value; }
        }

        /// <summary>
        /// Returns the ring of directed edges that this directed edge is
        /// a member of, or null if the ring has not been set.
        /// </summary>
        public Boolean IsInRing
        {
            get { return Ring != null; }
        }

        /// <summary> 
        /// Gets or sets the ring of directed edges that this directed edge is
        /// a member of.
        /// </summary>
        public EdgeRing<TCoordinate> Ring
        {
            get { return _edgeRing; }
            set { _edgeRing = value; }
        }
    }
}