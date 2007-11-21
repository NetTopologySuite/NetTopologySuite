using System;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph.Index
{
    public enum SweepLineEventType
    {
        Insert = 1,
        Delete = 2
    }

    public class SweepLineEvent : IComparable<SweepLineEvent>
    {
        private object _edgeSet; // used for red-blue intersection detection
        private readonly Double _xValue;
        private readonly SweepLineEventType _eventType;
        private readonly SweepLineEvent _insertEvent; // null if this is an Insert event
        private Int32 _deleteEventIndex;
        private readonly object _obj;

        public SweepLineEvent(object edgeSet, Double x, SweepLineEvent insertEvent, object obj)
        {
            _edgeSet = edgeSet;
            _xValue = x;
            _insertEvent = insertEvent;
            _eventType = SweepLineEventType.Insert;

            if (insertEvent != null)
            {
                _eventType = SweepLineEventType.Delete;
            }

            _obj = obj;
        }

        public object EdgeSet
        {
            get { return _edgeSet; }
            set { _edgeSet = value; }
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

        public object Object
        {
            get { return _obj; }
        }

        /// <summary>
        /// <see cref="SweepLineEvent"/>s are ordered first by their x-value, 
        /// and then by their event type. It is important that 
        /// <see cref="SweepLineEventType.Insert"/> events are sorted before 
        /// <see cref="SweepLineEventType.Delete"/> events, so that
        /// items whose Insert and Delete events occur at the same x-value will be
        /// correctly handled.
        /// </summary>
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