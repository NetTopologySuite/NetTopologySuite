using System;
using System.IO;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Represents an intersection point between two <see cref="SegmentString" />s.
    /// </summary>
    public class SegmentNode : IComparable
    {        

        /// <summary>
        /// 
        /// </summary>
        public readonly ICoordinate Coordinate = null;   // the point of intersection
        
        /// <summary>
        /// 
        /// </summary>
        public readonly int SegmentIndex = 0;   // the index of the containing line segment in the parent edge

        private readonly SegmentString segString = null;
        private readonly Octants segmentOctant = Octants.Null;
        private readonly bool isInterior = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentNode"/> class.
        /// </summary>
        /// <param name="segString"></param>
        /// <param name="coord"></param>
        /// <param name="segmentIndex"></param>
        /// <param name="segmentOctant"></param>
        public SegmentNode(SegmentString segString, ICoordinate coord, int segmentIndex, Octants segmentOctant) 
        {
            this.segString = segString;
            this.Coordinate = new Coordinate(coord);
            this.SegmentIndex = segmentIndex;
            this.segmentOctant = segmentOctant;
            isInterior = !coord.Equals2D(segString.GetCoordinate(segmentIndex));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsInterior
        { 
            get
            {
                return isInterior; 
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxSegmentIndex"></param>
        /// <returns></returns>
        public bool IsEndPoint(int maxSegmentIndex)
        {
            if(SegmentIndex == 0 && ! isInterior) 
                return true;
            if(SegmentIndex == maxSegmentIndex) 
                return true;
            return false;
        } 

        /// <summary>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>
        /// -1 this SegmentNode is located before the argument location, or
        ///  0 this SegmentNode is at the argument location, or
        ///  1 this SegmentNode is located after the argument location.   
        /// </returns>
        public int CompareTo(object obj)
        {
            SegmentNode other = (SegmentNode) obj;
            if(SegmentIndex < other.SegmentIndex) 
                return -1;
            if(SegmentIndex > other.SegmentIndex) 
                return 1;
            if (Coordinate.Equals2D(other.Coordinate)) 
                return 0;
            return SegmentPointComparator.Compare(segmentOctant, Coordinate, other.Coordinate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outstream"></param>
        public void Write(StreamWriter outstream)
        {
            outstream.Write(Coordinate);
            outstream.Write(" seg # = " + SegmentIndex);
        }
    }
}
