using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph.Index
{
    /// <summary> 
    /// A <see cref="SimpleMonotoneChainSweepLineIntersector{TCoordinate}"/> 
    /// creates monotone chains from the edges
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
    /// The use of <see cref="MonotoneChain{TCoordinate}"/>s as the items in the index
    /// seems to offer an improvement in performance over a sweep-line alone.
    /// </para>
    /// </remarks>
    public class SimpleMonotoneChainSweepLineIntersector<TCoordinate>
            : EdgeSetIntersector<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        // TODO: implement as tree list to allow neighbor-only evaluation
        //private readonly TreeList<SweepLineEvent> _events = new TreeList<SweepLineEvent>();
        private readonly List<SweepLineEvent> _events = new List<SweepLineEvent>();

        // statistics information
        private Int32 _overlapCount;

        public override void ComputeIntersections(IEnumerable<Edge<TCoordinate>> edges,
                                                  SegmentIntersector<TCoordinate> si,
                                                  Boolean testAllSegments)
        {
            if (testAllSegments)
            {
                add(edges, null);
            }
            else
            {
                add(edges);
            }

            computeIntersections(si);
        }

        public override void ComputeIntersections(IEnumerable<Edge<TCoordinate>> edges0,
                                                  IEnumerable<Edge<TCoordinate>> edges1,
                                                  SegmentIntersector<TCoordinate> si)
        {
            add(edges0, edges0);
            add(edges1, edges1);
            computeIntersections(si);
        }

        private void add(IEnumerable<Edge<TCoordinate>> edges)
        {
            foreach (Edge<TCoordinate> edge in edges)
            {
                // edge is its own group
                add(edge, edge);
            }
        }

        private void add(IEnumerable<Edge<TCoordinate>> edges, Object edgeSet)
        {
            foreach (Edge<TCoordinate> edge in edges)
            {
                add(edge, edgeSet);
            }
        }

        private void add(Edge<TCoordinate> edge, Object edgeSet)
        {
            MonotoneChainEdge<TCoordinate> mce = edge.MonotoneChainEdge;
            IList<Int32> startIndex = mce.StartIndexes;

            for (Int32 i = 0; i < startIndex.Count - 1; i++)
            {
                // create a monotone chain from the edge, which we can use instead
                // of segments to feed into the sweepline algorithm
                MonotoneChain<TCoordinate> mc = new MonotoneChain<TCoordinate>(mce, i);
                
                SweepLineEvent insertEvent
                    = new SweepLineEvent(edgeSet, mce.GetMinX(i), null, mc);

                _events.Add(insertEvent);
                _events.Add(new SweepLineEvent(edgeSet, mce.GetMaxX(i), insertEvent, mc));
                
                /*
                 * 
                // now we can add proceed with the sweepline using the chains in place
                // of segments; so add the lower endpoint of the chain
                Int32 insertIndex = addEvent(mce.GetMinX(i), null, edgeSet, mc);

                // ... and delete the chain when reaching the lower endpoint
                addEvent(mce.GetMaxX(i), insertIndex, edgeSet, mc);
                 */
            }
        }

        /*
        private Int32 addEvent(Double x, Int32? insertEventIndex, Object edgeSet, MonotoneChain<TCoordinate> mc)
        {
            SweepLineEvent e;

            if (insertEventIndex != null)
            {
                SweepLineEvent insertEvent = _events[insertEventIndex.Value];
                e = new SweepLineEvent(edgeSet, x, insertEvent, mc);
            }
            else
            {
                e = new SweepLineEvent(edgeSet, x, null, mc);
            }

            Int32 eventIndex = _events.IndexOf(e);

            if (eventIndex < 0)
            {
                _events.Add(e);
            }
            else
            {
                SweepLineEvent existing = _events[eventIndex];
                existing.Add(e);
            }

            return eventIndex;
        }
        */

        private void computeIntersections(SegmentIntersector<TCoordinate> si)
        {
            _overlapCount = 0;
            prepareEvents();

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

        // Because Delete events have a link to their corresponding Insert event,
        // it is possible to compute exactly the range of events which must be
        // compared to a given Insert event object.
        private void prepareEvents()
        {
            // [codekaizen 2008-04-22] consider using a BST instead of a linear list
            //                         to improve sort performance.
            _events.Sort();

            for (Int32 i = 0; i < _events.Count; i++)
            {
                SweepLineEvent ev = _events[i];

                if (ev.IsDelete)
                {
                    SweepLineEvent e = ev.InsertEvent;
                    e.DeleteEventIndex = i;
                }
            }
        }

        private void processOverlaps(Int32 start, Int32 end, 
                                     SweepLineEvent ev0,
                                     SegmentIntersector<TCoordinate> si)
        {
            MonotoneChain<TCoordinate> mc0 = ev0.Object as MonotoneChain<TCoordinate>;

            Debug.Assert(mc0 != null);

            /*
            * Since we might need to test for self-intersections,
            * include current insert event Object in list of event objects to test.
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