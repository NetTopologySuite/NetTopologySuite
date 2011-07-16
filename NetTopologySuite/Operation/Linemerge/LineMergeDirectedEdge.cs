using System;
using GeoAPI.Coordinates;
using GeoAPI.Diagnostics;
using NetTopologySuite.Planargraph;
using NPack.Interfaces;

namespace NetTopologySuite.Operation.Linemerge
{
    /// <summary>
    /// A <see cref="DirectedEdge{TCoordinate}"/> of a <see cref="LineMergeGraph{TCoordinate}"/>. 
    /// </summary>
    public class LineMergeDirectedEdge<TCoordinate> : DirectedEdge<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        /// <summary>
        /// Constructs a LineMergeDirectedEdge connecting the <c>from</c> node to the <c>to</c> node.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="directionPt">
        /// specifies this DirectedEdge's direction (given by an imaginary
        /// line from the <c>from</c> node to <c>directionPt</c>).
        /// </param>
        /// <param name="edgeDirection">
        /// whether this DirectedEdge's direction is the same as or
        /// opposite to that of the parent Edge (if any).
        /// </param>
        public LineMergeDirectedEdge(Node<TCoordinate> from, Node<TCoordinate> to, TCoordinate directionPt,
                                     Boolean edgeDirection)
            : base(from, to, directionPt, edgeDirection)
        {
        }

        /// <summary>
        /// Returns the directed edge that starts at this directed edge's end point, or null
        /// if there are zero or multiple directed edges starting there.  
        /// </summary>
        public LineMergeDirectedEdge<TCoordinate> Next
        {
            get
            {
                if (ToNode.Degree != 2)
                {
                    return null;
                }

                if (ToNode.OutEdges.Edges[0] == Sym)
                {
                    return ToNode.OutEdges.Edges[1] as LineMergeDirectedEdge<TCoordinate>;
                }

                Assert.IsTrue(ToNode.OutEdges.Edges[1] == Sym);

                return ToNode.OutEdges.Edges[0] as LineMergeDirectedEdge<TCoordinate>;
            }
        }
    }
}