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
        private bool indexBuilt;

        // statistics information
        private int nOverlaps;

        /// <summary>
        /// 
        /// </summary>
        public SweepLineIndex() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sweepInt"></param>
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
                return;
            events.Sort();
            for (int i = 0; i < events.Count; i++)
            {
                SweepLineEvent ev = (SweepLineEvent)events[i];
                if (ev.IsDelete)                
                    ev.InsertEvent.DeleteEventIndex = i;                
            }
            indexBuilt = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void ComputeOverlaps(ISweepLineOverlapAction action)
        {
            nOverlaps = 0;
            BuildIndex();

            for (int i = 0; i < events.Count; i++)
            {
                SweepLineEvent ev = (SweepLineEvent)events[i];
                if (ev.IsInsert)               
                    ProcessOverlaps(i, ev.DeleteEventIndex, ev.Interval, action);                
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="s0"></param>
        /// <param name="action"></param>
        private void ProcessOverlaps(int start, int end, SweepLineInterval s0, ISweepLineOverlapAction action)
        {
            /*
             * Since we might need to test for self-intersections,
             * include current insert event object in list of event objects to test.
             * Last index can be skipped, because it must be a Delete event.
             */
            for (int i = start; i < end; i++)
            {
                SweepLineEvent ev = (SweepLineEvent)events[i];
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
