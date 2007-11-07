using System;
using System.IO;
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
            get { return coordinate; }
            set { coordinate = value; }
        }

        private Int32 segmentIndex;

        /// <summary>
        /// The index of the containing line segment in the parent edge.
        /// </summary>
        public Int32 SegmentIndex
        {
            get { return segmentIndex; }
            set { segmentIndex = value; }
        }

        private Double dist;

        /// <summary>
        /// The edge distance of this point along the containing line segment.
        /// </summary>
        public Double Distance
        {
            get { return dist; }
            set { dist = value; }
        }

        public EdgeIntersection(ICoordinate coord, Int32 segmentIndex, Double dist)
        {
            coordinate = new Coordinate(coord);
            this.segmentIndex = segmentIndex;
            this.dist = dist;
        }

        public Int32 CompareTo(object obj)
        {
            EdgeIntersection other = (EdgeIntersection) obj;
            return Compare(other.SegmentIndex, other.Distance);
        }

        /// <returns>
        /// -1 this EdgeIntersection is located before the argument location,
        /// 0 this EdgeIntersection is at the argument location,
        /// 1 this EdgeIntersection is located after the argument location.
        /// </returns>
        public Int32 Compare(Int32 segmentIndex, Double dist)
        {
            if (SegmentIndex < segmentIndex)
            {
                return -1;
            }
            if (SegmentIndex > segmentIndex)
            {
                return 1;
            }
            if (Distance < dist)
            {
                return -1;
            }
            if (Distance > dist)
            {
                return 1;
            }
            return 0;
        }

        public Boolean IsEndPoint(Int32 maxSegmentIndex)
        {
            if (SegmentIndex == 0 && Distance == 0.0)
            {
                return true;
            }
            if (SegmentIndex == maxSegmentIndex)
            {
                return true;
            }
            return false;
        }

        public void Write(StreamWriter outstream)
        {
            outstream.Write(Coordinate);
            outstream.Write(" seg # = " + SegmentIndex);
            outstream.WriteLine(" dist = " + Distance);
        }
    }
}