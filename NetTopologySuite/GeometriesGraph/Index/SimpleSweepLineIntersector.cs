using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph.Index
{
    /// <summary>
    /// Finds all intersections in one or two sets of edges,
    /// using a simple x-axis sweepline algorithm.
    /// While still O(n^2) in the worst case, this algorithm
    /// drastically improves the average-case time.
    /// </summary>
    public class SimpleSweepLineIntersector<TCoordinate> : EdgeSetIntersector<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        private readonly List<SweepLineEvent> _events = new List<SweepLineEvent>();

        // statistics information
        private Int32 nOverlaps;

        public override void ComputeIntersections(IEnumerable<Edge<TCoordinate>> edges, SegmentIntersector<TCoordinate> si, Boolean testAllSegments)
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

        public override void ComputeIntersections(IEnumerable<Edge<TCoordinate>> edges0, IEnumerable<Edge<TCoordinate>> edges1, SegmentIntersector<TCoordinate> si)
        {
            add(edges0, edges0);
            add(edges1, edges1);
            computeIntersections(si);
        }

        private void add(IEnumerable<Edge<TCoordinate>> edges)
        {
            foreach (Edge<TCoordinate> edge in edges)
            {
                add(edge, edge);
            }
        }

        private void add(IEnumerable<Edge<TCoordinate>> edges, object edgeSet)
        {
            foreach (Edge<TCoordinate> edge in edges)
            {
                add(edge, edgeSet);
            }
        }

        private void add(Edge<TCoordinate> edge, object edgeSet)
        {
            IEnumerable<TCoordinate> pts = edge.Coordinates;
            IEnumerator<TCoordinate> enumerator = pts.GetEnumerator();
            Int32 index = 0;

            while(enumerator.MoveNext())
            {
                SweepLineSegment<TCoordinate> ss = new SweepLineSegment<TCoordinate>(edge, index);
                SweepLineEvent insertEvent = new SweepLineEvent(edgeSet, ss.MinX, null, ss);
                _events.Add(insertEvent);
                _events.Add(new SweepLineEvent(edgeSet, ss.MaxX, insertEvent, ss));
                index += 1;
            }
            
            foreach (TCoordinate coordinate in pts)
            {
            }
        }

        /// <summary> 
        /// Because Delete Events have a link to their corresponding Insert event,
        /// it is possible to compute exactly the range of events which must be
        /// compared to a given Insert event object.
        /// </summary>
        private void prepareEvents()
        {
            _events.Sort();

            for (Int32 i = 0; i < _events.Count; i++)
            {
                SweepLineEvent ev = (SweepLineEvent) _events[i];
                if (ev.IsDelete)
                {
                    ev.InsertEvent.DeleteEventIndex = i;
                }
            }
        }

        private void computeIntersections(SegmentIntersector<TCoordinate> si)
        {
            nOverlaps = 0;
            prepareEvents();

            for (Int32 i = 0; i < _events.Count; i++)
            {
                SweepLineEvent ev = (SweepLineEvent) _events[i];
                if (ev.IsInsert)
                {
                    processOverlaps(i, ev.DeleteEventIndex, ev, si);
                }
            }
        }

        private void processOverlaps(Int32 start, Int32 end, SweepLineEvent ev0, SegmentIntersector<TCoordinate> si)
        {
            SweepLineSegment<TCoordinate> ss0 = ev0.Object as SweepLineSegment<TCoordinate>;

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
                    SweepLineSegment<TCoordinate> ss1 = ev1.Object as SweepLineSegment<TCoordinate>;
                    
                    if (ev0.EdgeSet == null || (ev0.EdgeSet != ev1.EdgeSet))
                    {
                        Debug.Assert(ss0 != null);
                        ss0.ComputeIntersections(ss1, si);
                    }
                    
                    nOverlaps++;
                }
            }
        }
    }
}