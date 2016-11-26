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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="insertEvent"></param>
        /// <param name="sweepInt"></param>
        public SweepLineEvent(double x, SweepLineEvent insertEvent, SweepLineInterval sweepInt)
        {
            xValue = x;
            InsertEvent = insertEvent;            
            if (insertEvent != null)
                 eventType = SweepLineEvents.Delete;
            else eventType = SweepLineEvents.Insert;
            Interval = sweepInt;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsInsert => InsertEvent == null;

        /// <summary>
        /// 
        /// </summary>
        public bool IsDelete => InsertEvent != null;

        /// <summary>
        /// 
        /// </summary>
        public SweepLineEvent InsertEvent { get; }

        /// <summary>
        /// 
        /// </summary>
        public int DeleteEventIndex { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public SweepLineInterval Interval { get; }

        /// <summary>
        /// ProjectionEvents are ordered first by their x-value, and then by their eventType.
        /// It is important that Insert events are sorted before Delete events, so that
        /// items whose Insert and Delete events occur at the same x-value will be
        /// correctly handled.
        /// </summary>
        /// <param name="o"></param>
        public int CompareTo(object o) 
        {
            SweepLineEvent pe = (SweepLineEvent) o;
            if (xValue < pe.xValue) return  -1;
            if (xValue > pe.xValue) return   1;
            if (eventType < pe.eventType) return  -1;
            if (eventType > pe.eventType) return   1;
            return 0;
        }
    }
}
