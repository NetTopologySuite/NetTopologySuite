using System;
using System.Collections;
using System.Text;
using System.IO;

using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Represents an intersection point between two <c>SegmentString</c>s.
    /// </summary>
    public class SegmentNode : IComparable
    {
        private Coordinate coord;   // the point of intersection 
       
        /// <summary>
        /// The point of intersection.
        /// </summary>
        public virtual Coordinate Coordinate
        {
            get 
            { 
                return coord; 
            }
            set 
            { 
                coord = value; 
            }
        }

        private int segmentIndex;   // the index of the containing line segment in the parent edge

        /// <summary>
        /// The index of the containing line segment in the parent edge.
        /// </summary>
        public virtual int SegmentIndex
        {
            get 
            { 
                return segmentIndex; 
            }
            set
            { 
                segmentIndex = value; 
            }
        }

        private double dist;        // the edge distance of this point along the containing line segment

        /// <summary>
        /// The edge distance of this point along the containing line segment.
        /// </summary>
        public virtual double Distance
        {
            get 
            {
                return dist; 
            }
            set
            {
                dist = value; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="segmentIndex"></param>
        /// <param name="dist"></param>
        public SegmentNode(Coordinate coord, int segmentIndex, double dist) 
        {
            this.coord = new Coordinate(coord);
            this.segmentIndex = segmentIndex;
            this.dist = dist;        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segmentIndex"></param>
        /// <param name="dist"></param>
        /// <returns>
        /// -1 this EdgeIntersection is located before the argument location,
        /// 0 this EdgeIntersection is at the argument location,
        /// 1 this EdgeIntersection is located after the argument location.
        /// </returns>
        public virtual int Compare(int segmentIndex, double dist)
        {
            if (this.SegmentIndex < segmentIndex) 
                return -1;
            if (this.SegmentIndex > segmentIndex) 
                return 1;
            if (this.Distance < dist) 
                return -1;
            if (this.Distance > dist) 
                return 1;
            return 0;
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxSegmentIndex"></param>
        /// <returns></returns>
        public virtual bool IsEndPoint(int maxSegmentIndex)
        {
            if (SegmentIndex == 0 && Distance == 0.0) 
                return true;
            if (SegmentIndex == maxSegmentIndex) 
                return true;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual int CompareTo(Object obj)
        {
            SegmentNode other = (SegmentNode) obj;
            return Compare(other.SegmentIndex, other.Distance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outstream"></param>
        public virtual void Write(StreamWriter outstream)
        {
            outstream.Write(coord);
            outstream.Write(" seg # = " + segmentIndex);
            outstream.WriteLine(" dist = " + dist);
        }
    }
}
