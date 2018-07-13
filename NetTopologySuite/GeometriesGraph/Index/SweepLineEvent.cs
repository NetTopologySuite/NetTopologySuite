using System;

namespace NetTopologySuite.GeometriesGraph.Index
{
    /// <summary>
    ///
    /// </summary>
    public class SweepLineEvent : IComparable
    {
        private const int Insert = 1;
        private const int Delete = 2;

        private readonly object _label; // used for red-blue intersection detection
        private readonly double _xValue;
        private readonly int _eventType;
        private readonly SweepLineEvent _insertEvent; // null if this is an Insert event
        private int _deleteEventIndex;
        private readonly object _obj;

        /// <summary>
        /// Creates an INSERT event.
        /// </summary>
        /// <param name="label">The edge set label for this object.</param>
        /// <param name="x">The event location</param>
        /// <param name="obj">the object being inserted</param>
        public SweepLineEvent(object label, double x, object obj)
        {
            _eventType = Insert;
            _label = label;
            _xValue = x;
            _obj = obj;
        }

        /// <summary>
        /// Creates a DELETE event.
        /// </summary>
        /// <param name="x">The event location</param>
        /// <param name="insertEvent">The corresponding INSERT event</param>
        public SweepLineEvent(double x, SweepLineEvent insertEvent)
        {
            _eventType = Delete;
            _xValue = x;
            _insertEvent = insertEvent;
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsInsert => _eventType == Insert;

        /// <summary>
        ///
        /// </summary>
        public bool IsDelete => _eventType == Delete;

        /// <summary>
        ///
        /// </summary>
        public SweepLineEvent InsertEvent => _insertEvent;

        /// <summary>
        ///
        /// </summary>
        public int DeleteEventIndex
        {
            get => _deleteEventIndex;
            set => _deleteEventIndex = value;
        }

        /// <summary>
        ///
        /// </summary>
        public object Object => _obj;

        public bool IsSameLabel(SweepLineEvent ev)
        {
            // no label set indicates single group
            if (_label == null)
                return false;
            return _label == ev._label;
        }

        /// <summary>
        /// Events are ordered first by their x-value, and then by their eventType.
        /// Insert events are sorted before Delete events, so that
        /// items whose Insert and Delete events occur at the same x-value will be
        /// correctly handled.
        /// </summary>
        /// <param name="o"></param>
        public int CompareTo(object o)
        {
            var pe = (SweepLineEvent)o;
            if (_xValue < pe._xValue)
                return -1;
            if (_xValue > pe._xValue)
                return 1;
            if (_eventType < pe._eventType)
                return -1;
            if (_eventType > pe._eventType)
                return 1;
            return 0;
        }
    }
}
