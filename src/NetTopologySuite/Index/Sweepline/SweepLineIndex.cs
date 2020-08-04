using System.Collections.Generic;

namespace NetTopologySuite.Index.Sweepline
{
    /// <summary>
    /// A sweepline implements a sorted index on a set of intervals.
    /// It is used to compute all overlaps between the interval in the index.
    /// </summary>
    public class SweepLineIndex
    {
        private readonly List<SweepLineEvent> _events = new List<SweepLineEvent>();
        private bool _indexBuilt;

        // statistics information
        private int _nOverlaps;

        /*
        /// <summary>
        ///
        /// </summary>
        public SweepLineIndex() { }
        */
        /// <summary>
        ///
        /// </summary>
        /// <param name="sweepInt"></param>
        public void Add(SweepLineInterval sweepInt)
        {
            var insertEvent = new SweepLineEvent(sweepInt.Min, null, sweepInt);
            _events.Add(insertEvent);
            _events.Add(new SweepLineEvent(sweepInt.Max, insertEvent, sweepInt));
        }

        /// <summary>
        /// Because Delete Events have a link to their corresponding Insert event,
        /// it is possible to compute exactly the range of events which must be
        /// compared to a given Insert event object.
        /// </summary>
        private void BuildIndex()
        {
            if (_indexBuilt)
                return;
            _events.Sort();
            for (int i = 0; i < _events.Count; i++)
            {
                var ev = _events[i];
                if (ev.IsDelete)
                    ev.InsertEvent.DeleteEventIndex = i;
            }
            _indexBuilt = true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="action"></param>
        public void ComputeOverlaps(ISweepLineOverlapAction action)
        {
            _nOverlaps = 0;
            BuildIndex();

            for (int i = 0; i < _events.Count; i++)
            {
                var ev = _events[i];
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
                var ev = _events[i];
                if (ev.IsInsert)
                {
                    var s1 = ev.Interval;
                    action.Overlap(s0, s1);
                    _nOverlaps++;
                }
            }
        }
    }
}
