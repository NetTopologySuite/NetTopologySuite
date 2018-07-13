using System.IO;
using GeoAPI.Geometries;

namespace NetTopologySuite.GeometriesGraph
{
    /// <summary>
    ///
    /// </summary>
    public class Node : GraphComponent
    {
        /// <summary>
        /// Only non-null if this node is precise.
        /// </summary>
        private Coordinate _coord;

        /// <summary>
        ///
        /// </summary>
        private EdgeEndStar _edges;

        /// <summary>
        ///
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="edges"></param>
        public Node(Coordinate coord, EdgeEndStar edges)
        {
            _coord = coord;
            _edges = edges;
            Label = new Label(0, Location.Null);
        }

        /// <summary>
        ///
        /// </summary>
        public override Coordinate Coordinate
        {
            get => _coord;
            protected set => _coord = value;
        }

        /// <summary>
        ///
        /// </summary>
        public EdgeEndStar Edges
        {
            get => _edges;
            protected set => _edges = value;
        }

        /// <summary>
        /// Tests whether any incident edge is flagged as
        /// being in the result.
        /// This test can be used to determine if the node is in the result,
        /// since if any incident edge is in the result, the node must be in the result as well.
        /// </summary>
        /// <returns><value>true</value> if any incident edge in the in the result
        /// </returns>
        public bool IsIncidentEdgeInResult()
        {
            foreach (var de in Edges.Edges)
            {
                if (de.Edge.IsInResult)
                    return true;
            }
            return false;
        }

        /// <summary>
        ///
        /// </summary>
        public override bool IsIsolated => (Label.GeometryCount == 1);

        /// <summary>
        /// Basic nodes do not compute IMs.
        /// </summary>
        /// <param name="im"></param>
        public override void ComputeIM(IntersectionMatrix im) { }

        /// <summary>
        /// Add the edge to the list of edges at this node.
        /// </summary>
        /// <param name="e"></param>
        public void Add(EdgeEnd e)
        {
            // Assert: start pt of e is equal to node point
            _edges.Insert(e);
            e.Node = this;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="n"></param>
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
        /// <param name="label2"></param>
        public void MergeLabel(Label label2)
        {
            for (int i = 0; i < 2; i++)
            {
                var loc = ComputeMergedLocation(label2, i);
                var thisLoc = Label.GetLocation(i);
                if (thisLoc == Location.Null)
                    Label.SetLocation(i, loc);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="argIndex"></param>
        /// <param name="onLocation"></param>
        public void SetLabel(int argIndex, Location onLocation)
        {
            if (Label == null)
                 Label = new Label(argIndex, onLocation);
            else Label.SetLocation(argIndex, onLocation);
        }

        /// <summary>
        /// Updates the label of a node to BOUNDARY,
        /// obeying the mod-2 boundaryDetermination rule.
        /// </summary>
        /// <param name="argIndex"></param>
        public void SetLabelBoundary(int argIndex)
        {
            if (Label == null) return;

            // determine the current location for the point (if any)
            var loc = Location.Null;
            if (Label != null)
                loc = Label.GetLocation(argIndex);
            // flip the loc
            Location newLoc;
            switch (loc)
            {
            case Location.Boundary:
                newLoc = Location.Interior;
                break;
            case Location.Interior:
                newLoc = Location.Boundary;
                break;
            default:
                newLoc = Location.Boundary;
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
        /// <param name="label2"></param>
        /// <param name="eltIndex"></param>
        public Location ComputeMergedLocation(Label label2, int eltIndex)
        {
            /*Location loc = Location.Null*/
            var loc = Label.GetLocation(eltIndex);
            if (!label2.IsNull(eltIndex))
            {
                var nLoc = label2.GetLocation(eltIndex);
                if (loc != Location.Boundary)
                    loc = nLoc;
            }
            return loc;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="outstream"></param>
        public void Write(TextWriter outstream)
        {
            outstream.WriteLine("node " + _coord + " lbl: " + Label);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _coord + " " + _edges;
        }
    }
}
