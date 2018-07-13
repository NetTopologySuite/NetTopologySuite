using System;

namespace NetTopologySuite.Index.Sweepline
{
    /// <summary>
    ///
    /// </summary>
    public enum SweepLineEvents
    {
        /// <summary>
        ///
        /// </summary>
        Insert = 1,

        /// <summary>
        ///
        /// </summary>
        Delete = 2,
    }

    /// <summary>
    ///
    /// </summary>
    public class SweepLineEvent : IComparable
    {
        private readonly double xValue;
        private readonly SweepLineEvents eventType;
        private readonly SweepLineEvent insertEvent; // null if this is an Insert event

        private readonly SweepLineInterval sweepInt;

        /// <summary>
        ///
        /// </summary>
        /// <param name="x"></param>
        /// <param name="insertEvent"></param>
        /// <param name="sweepInt"></param>
        public SweepLineEvent(double x, SweepLineEvent insertEvent, SweepLineInterval sweepInt)
        {
            xValue = x;
            this.insertEvent = insertEvent;
            if (insertEvent != null)
                 eventType = SweepLineEvents.Delete;
            else eventType = SweepLineEvents.Insert;
            this.sweepInt = sweepInt;
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsInsert => insertEvent == null;

        /// <summary>
        ///
        /// </summary>
        public bool IsDelete => insertEvent != null;

        /// <summary>
        ///
        /// </summary>
        public SweepLineEvent InsertEvent => insertEvent;

        /// <summary>
        ///
        /// </summary>
        public int DeleteEventIndex { get; set; }

        /// <summary>
        ///
        /// </summary>
        public SweepLineInterval Interval => sweepInt;

        /// <summary>
        /// ProjectionEvents are ordered first by their x-value, and then by their eventType.
        /// It is important that Insert events are sorted before Delete events, so that
        /// items whose Insert and Delete events occur at the same x-value will be
        /// correctly handled.
        /// </summary>
        /// <param name="o"></param>
        public int CompareTo(object o)
        {
            var pe = (SweepLineEvent) o;
            if (xValue < pe.xValue) return  -1;
            if (xValue > pe.xValue) return   1;
            if (eventType < pe.eventType) return  -1;
            if (eventType > pe.eventType) return   1;
            return 0;
        }
    }
}
