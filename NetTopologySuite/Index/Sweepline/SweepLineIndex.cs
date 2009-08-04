using System;
using System.Collections.Generic;

namespace GisSharpBlog.NetTopologySuite.Index.Sweepline
{
    /// <summary>
    /// A sweepline implements a sorted index on a set of intervals.
    /// It is used to compute all overlaps between the interval in the index.
    /// </summary>
    public class SweepLineIndex
    {
        private readonly List<SweepLineEvent> _events = new List<SweepLineEvent>();
        private Boolean _isIndexBuilt;

        // statistics information
        private Int32 _overlapCount;

        public void Add(SweepLineInterval sweepLineInterval)
        {
            SweepLineEvent insertEvent
                = new SweepLineEvent(sweepLineInterval.Min, null, sweepLineInterval);
            _events.Add(insertEvent);
            _events.Add(new SweepLineEvent(sweepLineInterval.Max, insertEvent, sweepLineInterval));
        }

        public void ComputeOverlaps(ISweepLineOverlapAction action)
        {
            _overlapCount = 0;
            buildIndex();

            for (Int32 i = 0; i < _events.Count; i++)
            {
                SweepLineEvent ev = _events[i];

                if (ev.IsInsert)
                {
                    processOverlaps(i, ev.DeleteEventIndex, ev.Interval, action);
                }
            }
        }

        // Because Delete Events have a link to their corresponding Insert event,
        // it is possible to compute exactly the range of events which must be
        // compared to a given Insert event object.
        private void buildIndex()
        {
            if (_isIndexBuilt)
            {
                return;
            }

            _events.Sort();

            for (Int32 i = 0; i < _events.Count; i++)
            {
                SweepLineEvent ev = _events[i];

                if (ev.IsDelete)
                {
                    ev.InsertEvent.DeleteEventIndex = i;
                }
            }

            _isIndexBuilt = true;
        }

        private void processOverlaps(Int32 start, Int32 end, SweepLineInterval s0, ISweepLineOverlapAction action)
        {
            // Since we might need to test for self-intersections,
            // include current insert event object in list of event objects to test.
            // Last index can be skipped, because it must be a Delete event.
            for (Int32 i = start; i < end; i++)
            {
                SweepLineEvent ev = _events[i];

                if (ev.IsInsert)
                {
                    SweepLineInterval s1 = ev.Interval;
                    action.Overlap(s0, s1);
                    _overlapCount++;
                }
            }
        }
    }
}