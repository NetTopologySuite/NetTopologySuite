using System;
using System.IO;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    public class Node : GraphComponent
    {
        /// <summary>
        /// Only non-null if this node is precise.
        /// </summary>
        protected ICoordinate coord = null;

        protected EdgeEndStar edges = null;

        public Node(ICoordinate coord, EdgeEndStar edges)
        {
            this.coord = coord;
            this.edges = edges;
            label = new Label(0, Locations.Null);
        }

        public override ICoordinate Coordinate
        {
            get { return coord; }
        }

        public EdgeEndStar Edges
        {
            get { return edges; }
        }

        public override Boolean IsIsolated
        {
            get { return (label.GeometryCount == 1); }
        }

        /// <summary>
        /// Basic nodes do not compute IMs.
        /// </summary>
        public override void ComputeIM(IntersectionMatrix im) {}

        /// <summary> 
        /// Add the edge to the list of edges at this node.
        /// </summary>
        public void Add(EdgeEnd e)
        {
            // Assert: start pt of e is equal to node point
            edges.Insert(e);
            e.Node = this;
        }

        public void MergeLabel(Node n)
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
                Locations thisLoc = label.GetLocation(i);
                if (thisLoc == Locations.Null)
                {
                    label.SetLocation(i, loc);
                }
            }
        }

        public void SetLabel(Int32 argIndex, Locations onLocation)
        {
            if (label == null)
            {
                label = new Label(argIndex, onLocation);
            }
            else
            {
                label.SetLocation(argIndex, onLocation);
            }
        }

        /// <summary> 
        /// Updates the label of a node to BOUNDARY,
        /// obeying the mod-2 boundaryDetermination rule.
        /// </summary>
        public void SetLabelBoundary(Int32 argIndex)
        {
            // determine the current location for the point (if any)
            Locations loc = Locations.Null;

            if (label != null)
            {
                loc = label.GetLocation(argIndex);
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

            label.SetLocation(argIndex, newLoc);
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
            Locations loc = Locations.Null;
            loc = label.GetLocation(eltIndex);

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
            outstream.WriteLine("node " + coord + " lbl: " + label);
        }

        public override string ToString()
        {
            return coord + " " + edges;
        }
    }
}