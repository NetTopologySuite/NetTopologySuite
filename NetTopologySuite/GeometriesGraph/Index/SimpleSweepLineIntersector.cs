using System;
using System.Collections;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph.Index
{
    /// <summary>
    /// Finds all intersections in one or two sets of edges,
    /// using a simple x-axis sweepline algorithm.
    /// While still O(n^2) in the worst case, this algorithm
    /// drastically improves the average-case time.
    /// </summary>
    public class SimpleSweepLineIntersector : EdgeSetIntersector
    {
        private ArrayList events = new ArrayList();

        // statistics information
        private Int32 nOverlaps;

        public SimpleSweepLineIntersector() {}

        public override void ComputeIntersections(IList edges, SegmentIntersector si, Boolean testAllSegments)
        {
            if (testAllSegments)
            {
                Add(edges, null);
            }
            else
            {
                Add(edges);
            }
            ComputeIntersections(si);
        }

        public override void ComputeIntersections(IList edges0, IList edges1, SegmentIntersector si)
        {
            Add(edges0, edges0);
            Add(edges1, edges1);
            ComputeIntersections(si);
        }

        private void Add(IList edges)
        {
            for (IEnumerator i = edges.GetEnumerator(); i.MoveNext();)
            {
                Edge edge = (Edge) i.Current;
                // edge is its own group
                Add(edge, edge);
            }
        }

        private void Add(IList edges, object edgeSet)
        {
            for (IEnumerator i = edges.GetEnumerator(); i.MoveNext();)
            {
                Edge edge = (Edge) i.Current;
                Add(edge, edgeSet);
            }
        }

        private void Add(Edge edge, object edgeSet)
        {
            ICoordinate[] pts = edge.Coordinates;
            for (Int32 i = 0; i < pts.Length - 1; i++)
            {
                SweepLineSegment ss = new SweepLineSegment(edge, i);
                SweepLineEvent insertEvent = new SweepLineEvent(edgeSet, ss.MinX, null, ss);
                events.Add(insertEvent);
                events.Add(new SweepLineEvent(edgeSet, ss.MaxX, insertEvent, ss));
            }
        }

        /// <summary> 
        /// Because Delete Events have a link to their corresponding Insert event,
        /// it is possible to compute exactly the range of events which must be
        /// compared to a given Insert event object.
        /// </summary>
        private void PrepareEvents()
        {
            events.Sort();
            for (Int32 i = 0; i < events.Count; i++)
            {
                SweepLineEvent ev = (SweepLineEvent) events[i];
                if (ev.IsDelete)
                {
                    ev.InsertEvent.DeleteEventIndex = i;
                }
            }
        }

        private void ComputeIntersections(SegmentIntersector si)
        {
            nOverlaps = 0;
            PrepareEvents();

            for (Int32 i = 0; i < events.Count; i++)
            {
                SweepLineEvent ev = (SweepLineEvent) events[i];
                if (ev.IsInsert)
                {
                    ProcessOverlaps(i, ev.DeleteEventIndex, ev, si);
                }
            }
        }

        private void ProcessOverlaps(Int32 start, Int32 end, SweepLineEvent ev0, SegmentIntersector si)
        {
            SweepLineSegment ss0 = (SweepLineSegment) ev0.Object;
            /*
            * Since we might need to test for self-intersections,
            * include current insert event object in list of event objects to test.
            * Last index can be skipped, because it must be a Delete event.
            */
            for (Int32 i = start; i < end; i++)
            {
                SweepLineEvent ev1 = (SweepLineEvent) events[i];
                if (ev1.IsInsert)
                {
                    SweepLineSegment ss1 = (SweepLineSegment) ev1.Object;
                    if (ev0.EdgeSet == null || (ev0.EdgeSet != ev1.EdgeSet))
                    {
                        ss0.ComputeIntersections(ss1, si);
                    }
                    nOverlaps++;
                }
            }
        }
    }
}