using System;
using System.IO;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary> 
    /// An EdgeIntersection represents a point on an
    /// edge which intersects with another edge.
    /// The intersection may either be a single point, or a line segment
    /// (in which case this point is the start of the line segment)
    /// The label attached to this intersection point applies to
    /// the edge from this point forwards, until the next
    /// intersection or the end of the edge.
    /// The intersection point must be precise.
    /// </summary>
    public class EdgeIntersection : IComparable
    {
        private ICoordinate coordinate;   

        /// <summary>
        /// The point of intersection.
        /// </summary>
        public ICoordinate Coordinate
        {
            get
            {
                return coordinate; 
            }
            set
            {
                coordinate = value; 
            }
        }

        private int segmentIndex;  

        /// <summary>
        /// The index of the containing line segment in the parent edge.
        /// </summary>
        public int SegmentIndex
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

        private double dist;       

        /// <summary>
        /// The edge distance of this point along the containing line segment.
        /// </summary>
        public double Distance
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
        public EdgeIntersection(ICoordinate coord, int segmentIndex, double dist) 
        {
            this.coordinate = new Coordinate(coord);
            this.segmentIndex = segmentIndex;
            this.dist = dist;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            EdgeIntersection other = (EdgeIntersection) obj;
            return Compare(other.SegmentIndex, other.Distance);
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
        public int Compare(int segmentIndex, double dist)
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
        public bool IsEndPoint(int maxSegmentIndex)
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
        /// <param name="outstream"></param>
        public void Write(StreamWriter outstream)
        {
            outstream.Write(Coordinate);
            outstream.Write(" seg # = " + SegmentIndex);
            outstream.WriteLine(" dist = " + Distance);
        }
    }
}
