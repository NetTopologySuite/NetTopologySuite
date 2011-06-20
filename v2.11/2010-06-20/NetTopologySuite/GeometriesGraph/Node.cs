using System;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// Represents a node in a <see cref="GeometryGraph{TCoordinate}"/>.
    /// </summary>
    /// <typeparam name="TCoordinate">The type of coordinate.</typeparam>
    public class Node<TCoordinate> : GraphComponent<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        // Only valid if this node is precise.
        private readonly TCoordinate _coord;
        private readonly EdgeEndStar<TCoordinate> _edges;

        /// <summary>
        /// Creates a new <see cref="Node{TCoordinate}"/> instance at the given
        /// <typeparamref name="TCoordinate"/> and with an 
        /// <see cref="EdgeEndStar{TCoordinate}"/> to maintain an ordered list of
        /// incident edges.
        /// </summary>
        /// <param name="coord">The coordinate which the node models.</param>
        /// <param name="edges">
        /// An <see cref="EdgeEndStar{TCoordinate}"/> to maintain incident edges.
        /// </param>
        public Node(TCoordinate coord, EdgeEndStar<TCoordinate> edges)
        {
            _coord = coord;
            _edges = edges;
            Label = new Label(0, Locations.None);
        }

        public override TCoordinate Coordinate
        {
            get { return _coord; }
        }

        /// <summary>
        /// An <see cref="EdgeEndStar{TCoordinate}"/> used to keep an ordered
        /// list of incident edges on the node.
        /// </summary>
        public EdgeEndStar<TCoordinate> Edges
        {
            get { return _edges; }
        }

        public override Boolean IsIsolated
        {
            get { return Label == null || Label.Value.GeometryCount == 1; }
        }

        ///<summary>
        /// Tests whether any incident edge is flagged as being in the result.
        /// This test can be used to determine if the node is in the result,
        /// since if any incident edge is in the result, the node must be in the result as well.
        ///</summary>
        ///<returns>true if any indicident edge in the in the result</returns>
        public Boolean IsIncidentEdgeInResult()
        {
            foreach (DirectedEdge<TCoordinate> de in Edges)
            {
                //DirectedEdge<TCoordinate> de = DirectedEdge<TCoordinate>e;
                if (de.Edge.IsInResult)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Basic nodes do not compute intersection matrixes.
        /// </summary>
        public override void ComputeIntersectionMatrix(IntersectionMatrix matrix)
        {
        }

        /// <summary> 
        /// Add the edge to the list of edges at this node.
        /// </summary>
        public void Add(EdgeEnd<TCoordinate> e)
        {
            // Assert: start pt of e is equal to node point
            _edges.Insert(e);
            e.Node = this;
        }

        /// <summary>
        /// Merges the given <see cref="Node{TCoordinate}"/>'s
        /// <see cref="Node{TCoordinate}.Label"/> to this <see cref="Label"/>.
        /// </summary>
        /// <seealso cref="MergeLabel(Label)"/>
        public void MergeLabel(Node<TCoordinate> n)
        {
            if (n.Label != null)
            {
                MergeLabel(n.Label.Value);
            }
        }

        /// <summary>
        /// Merges the given <see cref="Label"/>'s locations to this node's label's
        /// locations.
        /// </summary>
        /// <remarks>
        /// To merge labels for two nodes, the merged location for each 
        /// <see cref="Label"/> is computed.
        /// The location for the corresponding node <see cref="Label"/> 
        /// is set to the result, as long as the location is non-null.
        /// </remarks>
        public void MergeLabel(Label other)
        {
            for (Int32 i = 0; i < 2; i++)
            {
                Locations loc = ComputeMergedLocation(other, i);
                Locations thisLoc = Label == null ? Locations.None : Label.Value[i].On;

                if (thisLoc == Locations.None)
                {
                    Label = Label == null
                                ? new Label(i, loc)
                                : new Label(Label.Value, i, loc);
                }
            }
        }

        /// <summary>
        /// Sets this node's <see cref="Label"/> for the given geometry
        /// (at <paramref name="geometryIndex"/>) to the given location for the 
        /// 'On' position.
        /// </summary>
        /// <param name="geometryIndex">
        /// The <see cref="Geometry{TCoordinate}"/> to set the label location of.
        /// </param>
        /// <param name="onLocation">
        /// The <see cref="Locations"/> value to set the 'On' position of the
        /// given geometry to.
        /// </param>
        public void SetLabel(Int32 geometryIndex, Locations onLocation)
        {
            Label = Label == null
                        ? new Label(geometryIndex, onLocation)
                        : new Label(Label.Value, geometryIndex, onLocation);
        }

        /// <summary> 
        /// Updates the label of a node to <see cref="Locations.Boundary"/>,
        /// obeying the mod-2 boundary determination rule.
        /// </summary>
        /// <seealso cref="Mod2BoundaryNodeRule"/>
        public void SetLabelBoundary(Int32 geometryIndex)
        {
            Debug.Assert(Label.HasValue);

            Label label = Label.Value;

            // determine the current location for the point (if any)
            Locations loc = label[geometryIndex, Positions.On];

            // flip the loc, which implements mod-2 (every other is non-bounary)
            Locations newLoc = loc == Locations.Boundary
                                   ? Locations.Interior
                                   : Locations.Boundary;

            Label = new Label(label, geometryIndex, newLoc);
        }

        /// <summary>
        /// Merges the given <see cref="Label"/> with this 
        /// <see cref="Node{TCoordinate}"/>'s label, choosing the greatest
        /// <see cref="Locations"/> value.
        /// </summary>
        /// <remarks>
        /// The location for a given <paramref name="elementIndex"/> for a node 
        /// will be one of  { <see cref="Locations.None"/>, 
        /// <see cref="Locations.Interior"/>, <see cref="Locations.Boundary"/> }.
        /// A node may be on both the boundary and the interior of a point;
        /// in this case, the rule is that the node is considered to be in the boundary.
        /// The merged location is the maximum of the two input values.
        /// </remarks>
        public Locations ComputeMergedLocation(Label otherLabel, Int32 elementIndex)
        {
            Locations loc = Label == null
                                ? Locations.None
                                : Label.Value[elementIndex].On;

            if (!otherLabel.IsNone(elementIndex))
            {
                Locations otherLocationOn = otherLabel[elementIndex, Positions.On];

                if (loc != Locations.Boundary)
                {
                    loc = otherLocationOn;
                }
            }

            return loc;
        }

        public override String ToString()
        {
            return Coordinate + "; " + Label + "; " + _edges;
        }

        //public void Write(StreamWriter outstream)
        //{
        //    outstream.WriteLine("node " + Coordinate + " lbl: " + Label);
        //}
    }
}