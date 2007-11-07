using System;

namespace GisSharpBlog.NetTopologySuite.Index.Sweepline
{
    public enum SweepLineEvents
    {
        Insert = 1,

        Delete = 2,
    }

    public class SweepLineEvent : IComparable
    {
        private Double xValue;
        private SweepLineEvents eventType;
        private SweepLineEvent insertEvent = null; // null if this is an Insert event
        private Int32 deleteEventIndex;

        private SweepLineInterval sweepInt;

        public SweepLineEvent(Double x, SweepLineEvent insertEvent, SweepLineInterval sweepInt)
        {
            xValue = x;
            this.insertEvent = insertEvent;
            if (insertEvent != null)
            {
                eventType = SweepLineEvents.Delete;
            }
            else
            {
                eventType = SweepLineEvents.Insert;
            }
            this.sweepInt = sweepInt;
        }

        public Boolean IsInsert
        {
            get { return insertEvent == null; }
        }

        public Boolean IsDelete
        {
            get { return insertEvent != null; }
        }

        public SweepLineEvent InsertEvent
        {
            get { return insertEvent; }
        }

        public Int32 DeleteEventIndex
        {
            get { return deleteEventIndex; }
            set { deleteEventIndex = value; }
        }

        public SweepLineInterval Interval
        {
            get { return sweepInt; }
        }

        /// <summary>
        /// ProjectionEvents are ordered first by their x-value, and then by their eventType.
        /// It is important that Insert events are sorted before Delete events, so that
        /// items whose Insert and Delete events occur at the same x-value will be
        /// correctly handled.
        /// </summary>
        public Int32 CompareTo(object o)
        {
            SweepLineEvent pe = (SweepLineEvent) o;
            if (xValue < pe.xValue)
            {
                return -1;
            }
            if (xValue > pe.xValue)
            {
                return 1;
            }
            if (eventType < pe.eventType)
            {
                return -1;
            }
            if (eventType > pe.eventType)
            {
                return 1;
            }
            return 0;
        }
    }
}