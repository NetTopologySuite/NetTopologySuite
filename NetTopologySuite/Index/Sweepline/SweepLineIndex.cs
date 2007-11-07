using System;
using System.Collections;

namespace GisSharpBlog.NetTopologySuite.Index.Sweepline
{
    /// <summary>
    /// A sweepline implements a sorted index on a set of intervals.
    /// It is used to compute all overlaps between the interval in the index.
    /// </summary>
    public class SweepLineIndex
    {
        private ArrayList events = new ArrayList();
        private Boolean indexBuilt;

        // statistics information
        private Int32 nOverlaps;

        public SweepLineIndex() {}

        public void Add(SweepLineInterval sweepInt)
        {
            SweepLineEvent insertEvent = new SweepLineEvent(sweepInt.Min, null, sweepInt);
            events.Add(insertEvent);
            events.Add(new SweepLineEvent(sweepInt.Max, insertEvent, sweepInt));
        }

        /// <summary>
        /// Because Delete Events have a link to their corresponding Insert event,
        /// it is possible to compute exactly the range of events which must be
        /// compared to a given Insert event object.
        /// </summary>
        private void BuildIndex()
        {
            if (indexBuilt)
            {
                return;
            }
            events.Sort();
            for (Int32 i = 0; i < events.Count; i++)
            {
                SweepLineEvent ev = (SweepLineEvent) events[i];
                if (ev.IsDelete)
                {
                    ev.InsertEvent.DeleteEventIndex = i;
                }
            }
            indexBuilt = true;
        }

        public void ComputeOverlaps(ISweepLineOverlapAction action)
        {
            nOverlaps = 0;
            BuildIndex();

            for (Int32 i = 0; i < events.Count; i++)
            {
                SweepLineEvent ev = (SweepLineEvent) events[i];
                if (ev.IsInsert)
                {
                    ProcessOverlaps(i, ev.DeleteEventIndex, ev.Interval, action);
                }
            }
        }

        private void ProcessOverlaps(Int32 start, Int32 end, SweepLineInterval s0, ISweepLineOverlapAction action)
        {
            /*
             * Since we might need to test for self-intersections,
             * include current insert event object in list of event objects to test.
             * Last index can be skipped, because it must be a Delete event.
             */
            for (Int32 i = start; i < end; i++)
            {
                SweepLineEvent ev = (SweepLineEvent) events[i];
                if (ev.IsInsert)
                {
                    SweepLineInterval s1 = ev.Interval;
                    action.Overlap(s0, s1);
                    nOverlaps++;
                }
            }
        }
    }
}