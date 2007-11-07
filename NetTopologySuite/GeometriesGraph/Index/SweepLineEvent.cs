using System;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph.Index
{
    public class SweepLineEvent : IComparable
    {
        public const Int32 Insert = 1;

        public const Int32 Delete = 2;

        private object edgeSet; // used for red-blue intersection detection
        private Double xValue;
        private Int32 eventType;
        private SweepLineEvent insertEvent; // null if this is an Insert event
        private Int32 deleteEventIndex;
        private object obj;

        public SweepLineEvent(object edgeSet, Double x, SweepLineEvent insertEvent, object obj)
        {
            this.edgeSet = edgeSet;
            xValue = x;
            this.insertEvent = insertEvent;
            eventType = Insert;
            if (insertEvent != null)
            {
                eventType = Delete;
            }
            this.obj = obj;
        }

        public object EdgeSet
        {
            get { return edgeSet; }
            set { edgeSet = value; }
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
        
        public object Object
        {
            get { return obj; }
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