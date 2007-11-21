using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph.Index
{
    /// <summary> 
    /// A SimpleMCSweepLineIntersector creates monotone chains from the edges
    /// and compares them using a simple sweep-line along the x-axis.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Finds all intersections in one or two sets of edges,
    /// using an x-axis sweepline algorithm in conjunction with Monotone Chains.
    /// </para>
    /// <para>
    /// While still O(n^2) in the worst case, this algorithm
    /// drastically improves the average-case time.
    /// </para>
    /// <para>
    /// The use of MonotoneChains as the items in the index
    /// seems to offer an improvement in performance over a sweep-line alone.
    /// </para>
    /// </remarks>
    public class SimpleMCSweepLineIntersector<TCoordinate> : EdgeSetIntersector<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        private readonly List<SweepLineEvent> _events = new List<SweepLineEvent>();

        // statistics information
        private Int32 _overlapCount;

        public override void ComputeIntersections(IEnumerable<Edge<TCoordinate>> edges, SegmentIntersector<TCoordinate> si, Boolean testAllSegments)
        {
            if (testAllSegments)
            {
                Add(edges, null);
            }
            else
            {
                Add(edges);
            }

            computeIntersections(si);
        }

        public override void ComputeIntersections(IEnumerable<Edge<TCoordinate>> edges0, IEnumerable<Edge<TCoordinate>> edges1, SegmentIntersector<TCoordinate> si)
        {
            Add(edges0, edges0);
            Add(edges1, edges1);
            computeIntersections(si);
        }

        private void Add(IEnumerable<Edge<TCoordinate>> edges)
        {
            foreach (Edge<TCoordinate> edge in edges)
            {
                // edge is its own group
                Add(edge, edge);
            }
        }

        private void Add(IEnumerable<Edge<TCoordinate>> edges, object edgeSet)
        {
            foreach (Edge<TCoordinate> edge in edges)
            {
                Add(edge, edgeSet);
            }
        }

        private void Add(Edge<TCoordinate> edge, object edgeSet)
        {
            MonotoneChainEdge<TCoordinate> mce = edge.MonotoneChainEdge;
            IList<Int32> startIndex = mce.StartIndexes;

            for (Int32 i = 0; i < startIndex.Count - 1; i++)
            {
                MonotoneChain<TCoordinate> mc = new MonotoneChain<TCoordinate>(mce, i);
                SweepLineEvent insertEvent = new SweepLineEvent(edgeSet, mce.GetMinX(i), null, mc);
                _events.Add(insertEvent);
                _events.Add(new SweepLineEvent(edgeSet, mce.GetMaxX(i), insertEvent, mc));
            }
        }

        /// <summary>
        /// Because Delete Events have a link to their corresponding Insert event,
        /// it is possible to compute exactly the range of events which must be
        /// compared to a given Insert event object.
        /// </summary>
        private void PrepareEvents()
        {
            _events.Sort();

            for (Int32 i = 0; i < _events.Count; i++)
            {
                SweepLineEvent ev = _events[i];

                if (ev.IsDelete)
                {
                    ev.InsertEvent.DeleteEventIndex = i;
                }
            }
        }

        private void computeIntersections(SegmentIntersector<TCoordinate> si)
        {
            _overlapCount = 0;
            PrepareEvents();

            for (Int32 i = 0; i < _events.Count; i++)
            {
                SweepLineEvent ev = _events[i];

                if (ev.IsInsert)
                {
                    // Console.WriteLine("Processing event " + i);
                    processOverlaps(i, ev.DeleteEventIndex, ev, si);
                }
            }
        }

        private void processOverlaps(Int32 start, Int32 end, SweepLineEvent ev0, SegmentIntersector<TCoordinate> si)
        {
            MonotoneChain<TCoordinate> mc0 = ev0.Object as MonotoneChain<TCoordinate>;

            Debug.Assert(mc0 != null);

            /*
            * Since we might need to test for self-intersections,
            * include current insert event object in list of event objects to test.
            * Last index can be skipped, because it must be a Delete event.
            */
            for (Int32 i = start; i < end; i++)
            {
                SweepLineEvent ev1 = _events[i];
                if (ev1.IsInsert)
                {
                    MonotoneChain<TCoordinate> mc1 = ev1.Object as MonotoneChain<TCoordinate>;

                    Debug.Assert(mc1 != null);

                    // don't compare edges in same group
                    // null group indicates that edges should be compared
                    if (ev0.EdgeSet == null || (ev0.EdgeSet != ev1.EdgeSet))
                    {
                        mc0.ComputeIntersections(mc1, si);
                        _overlapCount++;
                    }
                }
            }
        }
    }
}