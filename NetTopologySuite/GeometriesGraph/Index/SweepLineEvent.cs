using System;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph.Index
{
    /// <summary>
    /// 
    /// </summary>
    public class SweepLineEvent : IComparable
    {
        /// <summary>
        /// 
        /// </summary>
        public const int Insert = 1;
        
        /// <summary>
        /// 
        /// </summary>
        public const int Delete = 2;

        private object edgeSet;    // used for red-blue intersection detection
        private double xValue;
        private int eventType;
        private SweepLineEvent insertEvent; // null if this is an Insert event
        private int deleteEventIndex;
        private object obj;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edgeSet"></param>
        /// <param name="x"></param>
        /// <param name="insertEvent"></param>
        /// <param name="obj"></param>
        public SweepLineEvent(object edgeSet, double x, SweepLineEvent insertEvent, object obj)
        {
            this.edgeSet = edgeSet;
            xValue = x;
            this.insertEvent = insertEvent;
            this.eventType = Insert;
            if (insertEvent != null)
                eventType = Delete;
            this.obj = obj;
        }

        /// <summary>
        /// 
        /// </summary>
        public  object EdgeSet
        {
            get
            {
                return this.edgeSet;
            }
            set
            {
                this.edgeSet = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public  bool IsInsert 
        {
            get
            {
                return insertEvent == null; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public  bool IsDelete
        {
            get
            {
                return insertEvent != null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public SweepLineEvent InsertEvent
        {
            get
            {
                return insertEvent;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public  int DeleteEventIndex
        {
            get
            {
                return deleteEventIndex;
            }
            set
            {
                this.deleteEventIndex = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public  object Object
        {
            get
            {
                return obj;
            }
        }

        /// <summary>
        /// ProjectionEvents are ordered first by their x-value, and then by their eventType.
        /// It is important that Insert events are sorted before Delete events, so that
        /// items whose Insert and Delete events occur at the same x-value will be
        /// correctly handled.
        /// </summary>
        /// <param name="o"></param>
        public  int CompareTo(object o)
        {
            SweepLineEvent pe = (SweepLineEvent)o;
            if (xValue < pe.xValue)
                return -1;
            if (xValue > pe.xValue)
                return 1;
            if (eventType < pe.eventType)
                return -1;
            if (eventType > pe.eventType)
                return 1;
            return 0;
        }
    }
}
