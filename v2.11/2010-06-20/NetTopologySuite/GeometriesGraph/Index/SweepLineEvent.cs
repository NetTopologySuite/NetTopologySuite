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
        private readonly SweepLineEventType _eventType;
        private readonly SweepLineEvent _insertEvent; // null if this is an Insert event
        private readonly Object _obj;
        private readonly Double _xValue;
        private Int32 _deleteEventIndex;
        private Object _edgeSet; // used for red-blue intersection detection
        //private SweepLineEvent _next;

        public SweepLineEvent(Object edgeSet, Double x, SweepLineEvent insertEvent, Object obj)
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
            _deleteEventIndex = 0;
        }

        public Object EdgeSet
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

        public Object Object
        {
            get { return _obj; }
        }

        #region IComparable<SweepLineEvent> Members

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

            return ((Int32)_eventType).CompareTo((Int32)other._eventType);
        }

        #endregion

        public override string ToString()
        {
            return String.Format("{0} event at {1}",
                                 _eventType == SweepLineEventType.Insert ? "Insert" : "Delete",
                                 _xValue);
        }
    }
}