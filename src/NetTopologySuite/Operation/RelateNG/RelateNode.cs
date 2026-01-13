using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Operation.RelateNG
{
    internal class RelateNode
    {

        private readonly Coordinate _nodePt;

        /// <summary>
        /// A list of the edges around the node in CCW order,
        /// ordered by their CCW angle with the positive X-axis.
        /// </summary>
        private readonly List<RelateEdge> _edges = new List<RelateEdge>();

        public RelateNode(Coordinate pt)
        {
            _nodePt = pt;
        }

        public Coordinate Coordinate => _nodePt;

        public IList<RelateEdge> Edges => _edges;

        public void AddEdges(IEnumerable<NodeSection> nss)
        {
            foreach (var ns in nss)
            {
                AddEdges(ns);
            }
        }

        public void AddEdges(NodeSection ns)
        {
            //Debug.println("Adding NS: " + ns);
            switch (ns.Dimension)
            {
                case Dimension.L:
                    AddLineEdge(ns.IsA, ns.GetVertex(0));
                    AddLineEdge(ns.IsA, ns.GetVertex(1));
                    break;
                case Dimension.A:
                    //-- assumes node edges have CW orientation (as per JTS norm)
                    //-- entering edge - interior on L
                    var e0 = AddAreaEdge(ns.IsA, ns.GetVertex(0), false);
                    //-- exiting edge - interior on R
                    var e1 = AddAreaEdge(ns.IsA, ns.GetVertex(1), true);

                    int index0 = _edges.IndexOf(e0);
                    int index1 = _edges.IndexOf(e1);
                    UpdateEdgesInArea(ns.IsA, index0, index1);
                    UpdateIfAreaPrev(ns.IsA, index0);
                    UpdateIfAreaNext(ns.IsA, index1);
                    break;
            }
        }

        private void UpdateEdgesInArea(bool isA, int indexFrom, int indexTo)
        {
            int index = NextIndex(_edges, indexFrom);
            while (index != indexTo)
            {
                var edge = _edges[index];
                edge.SetAreaInterior(isA);
                index = NextIndex(_edges, index);
            }
        }

        private void UpdateIfAreaPrev(bool isA, int index)
        {
            int indexPrev = PrevIndex(_edges, index);
            var edgePrev = _edges[indexPrev];
            if (edgePrev.IsInterior(isA, Position.Left))
            {
                var edge = _edges[index];
                edge.SetAreaInterior(isA);
            }
        }

        private void UpdateIfAreaNext(bool isA, int index)
        {
            int indexNext = NextIndex(_edges, index);
            var edgeNext = _edges[indexNext];
            if (edgeNext.IsInterior(isA, Position.Right))
            {
                var edge = _edges[index];
                edge.SetAreaInterior(isA);
            }
        }

        private RelateEdge AddLineEdge(bool isA, Coordinate dirPt)
        {
            return AddEdge(isA, dirPt, Dimension.L, false);
        }

        private RelateEdge AddAreaEdge(bool isA, Coordinate dirPt, bool isForward)
        {
            return AddEdge(isA, dirPt, Dimension.A, isForward);
        }

        /// <summary>
        /// Adds or merges an edge to the node.
        /// </summary>
        /// <param name="isA"></param>
        /// <param name="dirPt"></param>
        /// <param name="dim">Dimension of the geometry element containing the edge</param>
        /// <param name="isForward">The direction of the edge</param>
        private RelateEdge AddEdge(bool isA, Coordinate dirPt, Dimension dim, bool isForward)
        {
            //-- check for well-formed edge - skip null or zero-len input
            if (dirPt == null)
                return null;
            if (_nodePt.Equals2D(dirPt))
                return null;

            int insertIndex = -1;
            RelateEdge e;
            for(int i = 0; i < _edges.Count; i++)
            {
                e = _edges[i];
                int comp = e.CompareToEdge(dirPt);
                if (comp == 0)
                {
                    e.Merge(isA, dirPt, dim, isForward);
                    return e;
                }
                if (comp == 1)
                {
                    //-- found further edge, so insert a new edge at this position
                    insertIndex = i;
                    break;
                }
            }
            //-- add a new edge
            e = RelateEdge.Create(this, dirPt, isA, dim, isForward);
            if (insertIndex < 0)
            {
                //-- add edge at end of list
                _edges.Add(e);
            }
            else
            {
                //-- add edge before higher edge found
                _edges.Insert(insertIndex, e);
            }
            return e;
        }

        /// <summary>
        /// Computes the final topology for the edges around this node.
        /// Although nodes lie on the boundary of areas or the interior of lines,
        /// in a mixed GC they may also lie in the interior of an area.
        /// This changes the locations of the sides and line to Interior.
        /// </summary>
        /// <param name="isAreaInteriorA"><c>true</c> if the node is in the interior of A</param>
        /// <param name="isAreaInteriorB"><c>true</c> if the node is in the interior of B</param>
        public void Finish(bool isAreaInteriorA, bool isAreaInteriorB)
        {

            //Debug.println("finish Node.");
            //Debug.println("Before: " + this);

            FinishNode(RelateGeometry.GEOM_A, isAreaInteriorA);
            FinishNode(RelateGeometry.GEOM_B, isAreaInteriorB);
            //Debug.println("After: " + this);
        }

        private void FinishNode(bool isA, bool isAreaInterior)
        {
            if (isAreaInterior)
            {
                RelateEdge.SetAreaInterior(_edges, isA);
            }
            else
            {
                int startIndex = RelateEdge.FindKnownEdgeIndex(_edges, isA);
                //-- only interacting nodes are finished, so this should never happen
                //Assert.isTrue(startIndex >= 0l, "Node at "+ nodePt + "does not have AB interaction");
                PropagateSideLocations(isA, startIndex);
            }
        }

        private void PropagateSideLocations(bool isA, int startIndex)
        {
            var currLoc = _edges[startIndex].Location(isA, Position.Left);
            //-- edges are stored in CCW order
            int index = NextIndex(_edges, startIndex);
            while (index != startIndex)
            {
                var e = _edges[index];
                e.SetUnknownLocations(isA, currLoc);
                currLoc = e.Location(isA, Position.Left);
                index = NextIndex(_edges, index);
            }
        }

        private static int PrevIndex(IList<RelateEdge> list, int index)
        {
            if (index > 0)
                return index - 1;
            //-- index == 0
            return list.Count - 1;
        }

        private static int NextIndex(IList<RelateEdge> list, int i)
        {
            if (i >= list.Count - 1)
            {
                return 0;
            }
            return i + 1;
        }

        public override string ToString()
        {
            var buf = new StringBuilder();
            buf.Append("Node[" + IO.WKTWriter.ToPoint(_nodePt) + "]:");
            buf.Append("\n");
            foreach (var e in _edges)
            {
                buf.Append(e.ToString());
                buf.Append("\n");
            }
            return buf.ToString();
        }

        public bool HasExteriorEdge(bool isA)
        {
            foreach (var e in _edges)
            {
                if (Location.Exterior == e.Location(isA, Position.Left)
                 || Location.Exterior == e.Location(isA, Position.Right))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
