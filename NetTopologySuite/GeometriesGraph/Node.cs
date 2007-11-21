using System;
using System.IO;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    public class Node<TCoordinate> : GraphComponent<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        // Only valid if this node is precise.
        private readonly TCoordinate _coord;
        private readonly EdgeEndStar<TCoordinate> _edges;

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

        public EdgeEndStar<TCoordinate> Edges
        {
            get { return _edges; }
        }

        public override Boolean IsIsolated
        {
            get { return (Label.GeometryCount == 1); }
        }

        /// <summary>
        /// Basic nodes do not compute intersection matrixes.
        /// </summary>
        public override void ComputeIntersectionMatrix(IntersectionMatrix matrix) {}

        /// <summary> 
        /// Add the edge to the list of edges at this node.
        /// </summary>
        public void Add(EdgeEnd<TCoordinate> e)
        {
            // Assert: start pt of e is equal to node point
            _edges.Insert(e);
            e.Node = this;
        }

        public void MergeLabel(Node<TCoordinate> n)
        {
            MergeLabel(n.Label);
        }

        /// <summary>
        /// To merge labels for two nodes,
        /// the merged location for each LabelElement is computed.
        /// The location for the corresponding node LabelElement is set to the result,
        /// as long as the location is non-null.
        /// </summary>
        public void MergeLabel(Label label2)
        {
            for (Int32 i = 0; i < 2; i++)
            {
                Locations loc = ComputeMergedLocation(label2, i);
                Locations thisLoc = Label.GetLocation(i);
                if (thisLoc == Locations.None)
                {
                    Label.SetLocation(i, loc);
                }
            }
        }

        public void SetLabel(Int32 argIndex, Locations onLocation)
        {
            if (Label == null)
            {
                Label = new Label(argIndex, onLocation);
            }
            else
            {
                Label.SetLocation(argIndex, onLocation);
            }
        }

        /// <summary> 
        /// Updates the label of a node to BOUNDARY,
        /// obeying the mod-2 boundaryDetermination rule.
        /// </summary>
        public void SetLabelBoundary(Int32 argIndex)
        {
            // determine the current location for the point (if any)
            Locations loc = Locations.None;

            if (Label != null)
            {
                loc = Label.GetLocation(argIndex);
            }

            // flip the loc
            Locations newLoc;

            switch (loc)
            {
                case Locations.Boundary:
                    newLoc = Locations.Interior;
                    break;
                case Locations.Interior:
                    newLoc = Locations.Boundary;
                    break;
                default:
                    newLoc = Locations.Boundary;
                    break;
            }

            Label.SetLocation(argIndex, newLoc);
        }

        /// <summary> 
        /// The location for a given eltIndex for a node will be one
        /// of { Null, Interior, Boundary }.
        /// A node may be on both the boundary and the interior of a point;
        /// in this case, the rule is that the node is considered to be in the boundary.
        /// The merged location is the maximum of the two input values.
        /// </summary>
        public Locations ComputeMergedLocation(Label label2, Int32 eltIndex)
        {
            Locations loc = Label.GetLocation(eltIndex);

            if (!label2.IsNull(eltIndex))
            {
                Locations nLoc = label2.GetLocation(eltIndex);
                if (loc != Locations.Boundary)
                {
                    loc = nLoc;
                }
            }

            return loc;
        }

        public void Write(StreamWriter outstream)
        {
            outstream.WriteLine("node " + Coordinate + " lbl: " + Label);
        }

        public override string ToString()
        {
            return Coordinate + " " + _edges;
        }
    }
}