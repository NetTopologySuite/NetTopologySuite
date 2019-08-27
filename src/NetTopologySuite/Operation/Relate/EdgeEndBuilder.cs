using System.Collections.Generic;
using NetTopologySuite.GeometriesGraph;

namespace NetTopologySuite.Operation.Relate
{
    /// <summary>
    /// An EdgeEndBuilder creates EdgeEnds for all the "split edges"
    /// created by the intersections determined for an Edge.
    /// Computes the <c>EdgeEnd</c>s which arise from a noded <c>Edge</c>.
    /// </summary>
    public class EdgeEndBuilder
    {
        /*
        /// <summary>
        ///
        /// </summary>
        public EdgeEndBuilder() { }
        */
        /// <summary>
        ///
        /// </summary>
        /// <param name="edges"></param>
        /// <returns></returns>
        public IList<EdgeEnd> ComputeEdgeEnds(IEnumerable<Edge> edges)
        {
            var l = new List<EdgeEnd>();
            foreach (var e in edges)
                ComputeEdgeEnds(e, l);

            return l;
        }

        /// <summary>
        /// Creates stub edges for all the intersections in this
        /// Edge (if any) and inserts them into the graph.
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="l"></param>
        public void ComputeEdgeEnds(Edge edge, IList<EdgeEnd> l)
        {
            var eiList = edge.EdgeIntersectionList;
            // ensure that the list has entries for the first and last point of the edge
            eiList.AddEndpoints();

            var it = eiList.GetEnumerator();
            EdgeIntersection eiPrev;
            EdgeIntersection eiCurr = null;
            // no intersections, so there is nothing to do
            if (! it.MoveNext()) return;
            var eiNext = it.Current;
            do
            {
                eiPrev = eiCurr;
                eiCurr = eiNext;
                eiNext = null;

                if (it.MoveNext())
                    eiNext = it.Current;

                if (eiCurr != null)
                {
                    CreateEdgeEndForPrev(edge, l, eiCurr, eiPrev);
                    CreateEdgeEndForNext(edge, l, eiCurr, eiNext);
                }
            }
            while (eiCurr != null);
        }

        /// <summary>
        /// Create a EdgeStub for the edge before the intersection eiCurr.
        /// The previous intersection is provided
        /// in case it is the endpoint for the stub edge.
        /// Otherwise, the previous point from the parent edge will be the endpoint.
        /// eiCurr will always be an EdgeIntersection, but eiPrev may be null.
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="l"></param>
        /// <param name="eiCurr"></param>
        /// <param name="eiPrev"></param>
        public void CreateEdgeEndForPrev(Edge edge, IList<EdgeEnd> l, EdgeIntersection eiCurr, EdgeIntersection eiPrev)
        {
            int iPrev = eiCurr.SegmentIndex;
            if (eiCurr.Distance == 0.0)
            {
                // if at the start of the edge there is no previous edge
                if (iPrev == 0)
                    return;
                iPrev--;
            }

            var pPrev = edge.GetCoordinate(iPrev);
            // if prev intersection is past the previous vertex, use it instead
            if (eiPrev != null && eiPrev.SegmentIndex >= iPrev)
                pPrev = eiPrev.Coordinate;

            var label = new Label(edge.Label);
            // since edgeStub is oriented opposite to it's parent edge, have to flip sides for edge label
            label.Flip();
            var e = new EdgeEnd(edge, eiCurr.Coordinate, pPrev, label);
            l.Add(e);
        }

        /// <summary>
        /// Create a StubEdge for the edge after the intersection eiCurr.
        /// The next intersection is provided
        /// in case it is the endpoint for the stub edge.
        /// Otherwise, the next point from the parent edge will be the endpoint.
        /// eiCurr will always be an EdgeIntersection, but eiNext may be null.
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="l"></param>
        /// <param name="eiCurr"></param>
        /// <param name="eiNext"></param>
        public void CreateEdgeEndForNext(Edge edge, IList<EdgeEnd> l, EdgeIntersection eiCurr, EdgeIntersection eiNext)
        {
            int iNext = eiCurr.SegmentIndex + 1;
            // if there is no next edge there is nothing to do
            if (iNext >= edge.NumPoints && eiNext == null)
                return;

            var pNext = edge.GetCoordinate(iNext);
            // if the next intersection is in the same segment as the current, use it as the endpoint
            if (eiNext != null && eiNext.SegmentIndex == eiCurr.SegmentIndex)
                pNext = eiNext.Coordinate;

            var e = new EdgeEnd(edge, eiCurr.Coordinate, pNext, new Label(edge.Label));
            l.Add(e);
        }
    }
}
