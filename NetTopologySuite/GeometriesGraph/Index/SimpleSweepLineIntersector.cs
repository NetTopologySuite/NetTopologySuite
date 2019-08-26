using System.Collections.Generic;

namespace NetTopologySuite.GeometriesGraph.Index
{
    /// <summary>
    /// Finds all intersections in one or two sets of edges,
    /// using a simple x-axis sweepline algorithm.
    /// While still O(n^2) in the worst case, this algorithm
    /// drastically improves the average-case time.
    /// </summary>
    public class SimpleSweepLineIntersector : EdgeSetIntersector
    {
        private readonly List<SweepLineEvent> _events = new List<SweepLineEvent>();

        // statistics information
        private int _nOverlaps;

        /*
        /// <summary>
        ///
        /// </summary>
        public SimpleSweepLineIntersector() { }
        */

        /// <summary>
        ///
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="si"></param>
        /// <param name="testAllSegments"></param>
        public override void ComputeIntersections(IList<Edge> edges, SegmentIntersector si, bool testAllSegments)
        {
            if (testAllSegments)
                Add(edges, null);
            else Add(edges);
            ComputeIntersections(si);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="edges0"></param>
        /// <param name="edges1"></param>
        /// <param name="si"></param>
        public override void ComputeIntersections(IList<Edge> edges0, IList<Edge> edges1, SegmentIntersector si)
        {
            Add(edges0, edges0);
            Add(edges1, edges1);
            ComputeIntersections(si);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="edges"></param>
        private void Add(IEnumerable<Edge> edges)
        {
            foreach (var edge in edges)
            {
                // edge is its own group
                Add(edge, edge);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="edgeSet"></param>
        private void Add(IEnumerable<Edge> edges, object edgeSet)
        {
            foreach (var edge in edges)
            {
                Add(edge, edgeSet);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="edgeSet"></param>
        private void Add(Edge edge, object edgeSet)
        {
            var pts = edge.Coordinates;
            for (int i = 0; i < pts.Length - 1; i++)
            {
                var ss = new SweepLineSegment(edge, i);
                var insertEvent = new SweepLineEvent(edgeSet, ss.MinX, null);
                _events.Add(insertEvent);
                _events.Add(new SweepLineEvent(ss.MaxX, insertEvent));
            }
        }

        /// <summary>
        /// Because DELETE events have a link to their corresponding INSERT event,
        /// it is possible to compute exactly the range of events which must be
        /// compared to a given INSERT event object.
        /// </summary>
        private void PrepareEvents()
        {
            _events.Sort();
            // set DELETE event indexes
            for (int i = 0; i < _events.Count; i++)
            {
                var ev = _events[i];
                if (ev.IsDelete)
                    ev.InsertEvent.DeleteEventIndex = i;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="si"></param>
        private void ComputeIntersections(SegmentIntersector si)
        {
            _nOverlaps = 0;
            PrepareEvents();

            for (int i = 0; i < _events.Count; i++)
            {
                var ev = _events[i];
                if (ev.IsInsert)
                    ProcessOverlaps(i, ev.DeleteEventIndex, ev, si);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="ev0"></param>
        /// <param name="si"></param>
        private void ProcessOverlaps(int start, int end, SweepLineEvent ev0, SegmentIntersector si)
        {
            var ss0 = (SweepLineSegment)ev0.Object;
            /*
            * Since we might need to test for self-intersections,
            * include current INSERT event object in list of event objects to test.
            * Last index can be skipped, because it must be a Delete event.
            */
            for (int i = start; i < end; i++)
            {
                var ev1 = _events[i];
                if (ev1.IsInsert)
                {
                    var ss1 = (SweepLineSegment)ev1.Object;
                    // don't compare edges in same group, if labels are present
                    if (!ev0.IsSameLabel(ev1))
                    {
                        {
                            ss0.ComputeIntersections(ss1, si);
                            _nOverlaps++;
                        }
                    }
                }
            }
        }
    }
}
