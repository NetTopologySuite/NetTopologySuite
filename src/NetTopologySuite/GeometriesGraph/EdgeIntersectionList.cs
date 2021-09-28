using System.Collections.Generic;
using System.IO;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// A list of edge intersections along an Edge.
    /// </summary>
    public class EdgeIntersectionList
    {
        // a list of EdgeIntersections
        private readonly IDictionary<EdgeIntersection, EdgeIntersection> nodeMap = new SortedDictionary<EdgeIntersection, EdgeIntersection>();
        private readonly Edge edge;  // the parent edge

        /// <summary>
        ///
        /// </summary>
        /// <param name="edge"></param>
        public EdgeIntersectionList(Edge edge)
        {
            this.edge = edge;
        }

        /// <summary>
        ///
        /// </summary>
        public int Count => nodeMap.Count;

        /// <summary>
        /// Adds an intersection into the list, if it isn't already there.
        /// The input segmentIndex and dist are expected to be normalized.
        /// </summary>
        /// <param name="intPt">The point of intersection</param>
        /// <param name="segmentIndex">The index of the containing line segment in the parent edge</param>
        /// <param name="dist">The edge distance of this point along the containing line segment</param>
        /// <returns>The EdgeIntersection found or added.</returns>
        public EdgeIntersection Add(Coordinate intPt, int segmentIndex, double dist)
        {
            var eiNew = new EdgeIntersection(intPt, segmentIndex, dist);
            EdgeIntersection ei;
            if (nodeMap.TryGetValue(eiNew, out ei))
                return ei;
            nodeMap[eiNew] = eiNew;
            return eiNew;
        }

        /// <summary>
        /// Returns an iterator of EdgeIntersections.
        /// </summary>
        public IEnumerator<EdgeIntersection> GetEnumerator()
        {
            return nodeMap.Values.GetEnumerator();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public bool IsIntersection(Coordinate pt)
        {
            foreach (var ei in nodeMap.Values    )
            {
                if (ei.Coordinate.Equals(pt))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Adds entries for the first and last points of the edge to the list.
        /// </summary>
        public void AddEndpoints()
        {
            int maxSegIndex = edge.Points.Length - 1;
            Add(edge.Points[0], 0, 0.0);
            Add(edge.Points[maxSegIndex], maxSegIndex, 0.0);
        }

        /// <summary>
        /// Creates new edges for all the edges that the intersections in this
        /// list split the parent edge into.
        /// Adds the edges to the input list (this is so a single list
        /// can be used to accumulate all split edges for a Geometry).
        /// </summary>
        /// <param name="edgeList"></param>
        public void AddSplitEdges(IList<Edge> edgeList)
        {
            // ensure that the list has entries for the first and last point of the edge
            AddEndpoints();

            var it = GetEnumerator();
            it.MoveNext();
            // there should always be at least two entries in the list
            var eiPrev = it.Current;
            while (it.MoveNext())
            {
                var ei = it.Current;
                var newEdge = CreateSplitEdge(eiPrev, ei);
                edgeList.Add(newEdge);

                eiPrev = ei;
            }
        }

        /// <summary>
        /// Create a new "split edge" with the section of points between
        /// (and including) the two intersections.
        /// The label for the new edge is the same as the label for the parent edge.
        /// </summary>
        /// <param name="ei0"></param>
        /// <param name="ei1"></param>
        public Edge CreateSplitEdge(EdgeIntersection ei0, EdgeIntersection ei1)
        {
            int npts = ei1.SegmentIndex - ei0.SegmentIndex + 2;
            var lastSegStartPt = edge.Points[ei1.SegmentIndex];
            // if the last intersection point is not equal to the its segment start pt,
            // add it to the points list as well.
            // (This check is needed because the distance metric is not totally reliable!)
            // The check for point equality is 2D only - Z values are ignored
            bool useIntPt1 = ei1.Distance > 0.0 || ! ei1.Coordinate.Equals2D(lastSegStartPt);
            if (! useIntPt1)
                npts--;

            var pts = new Coordinate[npts];
            int ipt = 0;
            pts[ipt++] = ei0.Coordinate.Copy();
            for (int i = ei0.SegmentIndex + 1; i <= ei1.SegmentIndex; i++)
                pts[ipt++] = edge.Points[i];

            if (useIntPt1)
                pts[ipt] = ei1.Coordinate;
            return new Edge(pts, new Label(edge.Label));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="outstream"></param>
        public void Write(StreamWriter outstream)
        {
            outstream.WriteLine("Intersections:");
            for (var it = GetEnumerator(); it.MoveNext(); )
            {
                var ei = it.Current;
                ei.Write(outstream);
            }
        }
    }
}
