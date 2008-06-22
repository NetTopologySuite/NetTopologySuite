using System;

namespace GisSharpBlog.NetTopologySuite.Index.Sweepline
{
    public enum SweepLineEventType
    {
        Insert = 1,
        Delete = 2,
    }

    public class SweepLineEvent : IComparable<SweepLineEvent>
    {
        private readonly Double _xValue;
        private readonly SweepLineEventType _eventType;
        private readonly SweepLineEvent _insertEvent = null; // null if this is an Insert event
        private Int32 _deleteEventIndex;
        private readonly SweepLineInterval _sweepLineInterval;

        public SweepLineEvent(Double x, SweepLineEvent insertEvent, SweepLineInterval sweepInt)
        {
            _xValue = x;
            _insertEvent = insertEvent;

            if (insertEvent != null)
            {
                _eventType = SweepLineEventType.Delete;
            }
            else
            {
                _eventType = SweepLineEventType.Insert;
            }

            _sweepLineInterval = sweepInt;
        }

        public Boolean IsInsert
        {
            get { return _insertEvent == null; }
        }

        public Boolean IsDelete
        {
            get { return _insertEvent != null; }
        }

        public SweepLineEvent InsertEvent
        {
            get { return _insertEvent; }
        }

        public Int32 DeleteEventIndex
        {
            get { return _deleteEventIndex; }
            set { _deleteEventIndex = value; }
        }

        public SweepLineInterval Interval
        {
            get { return _sweepLineInterval; }
        }

        /// <summary>
        /// Compares two <see cref="SweepLineEvent"/>s to sort them according to 
        /// coordinate value and <see cref="SweepLineEventType"/>.
        /// </summary>
        /// <remarks>
        /// ProjectionEvents are ordered first by their x-value, and then by their eventType.
        /// It is important that Insert events are sorted before Delete events, so that
        /// items whose Insert and Delete events occur at the same x-value will be
        /// correctly handled.
        /// </remarks>
        public Int32 CompareTo(SweepLineEvent other)
        { 
            if (_xValue < other._xValue)
            {
                return -1;
            }

            if (_xValue > other._xValue)
            {
                return 1;
            }

            if (_eventType < other._eventType)
            {
                return -1;
            }

            if (_eventType > other._eventType)
            {
                return 1;
            }

            return 0;
        }
    }
}