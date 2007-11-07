using System;
using System.IO;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Represents an intersection point between two <see cref="SegmentString" />s.
    /// </summary>
    public class SegmentNode : IComparable
    {
        public readonly ICoordinate Coordinate = null; // the point of intersection

        public readonly Int32 SegmentIndex = 0; // the index of the containing line segment in the parent edge

        private readonly SegmentString segString = null;
        private readonly Octants segmentOctant = Octants.Null;
        private readonly Boolean isInterior = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentNode"/> class.
        /// </summary>
        public SegmentNode(SegmentString segString, ICoordinate coord, Int32 segmentIndex, Octants segmentOctant)
        {
            this.segString = segString;
            Coordinate = new Coordinate(coord);
            SegmentIndex = segmentIndex;
            this.segmentOctant = segmentOctant;
            isInterior = !coord.Equals2D(segString.GetCoordinate(segmentIndex));
        }

        public Boolean IsInterior
        {
            get { return isInterior; }
        }

        public Boolean IsEndPoint(Int32 maxSegmentIndex)
        {
            if (SegmentIndex == 0 && ! isInterior)
            {
                return true;
            }
            if (SegmentIndex == maxSegmentIndex)
            {
                return true;
            }
            return false;
        }

        /// <returns>
        /// -1 this SegmentNode is located before the argument location, or
        ///  0 this SegmentNode is at the argument location, or
        ///  1 this SegmentNode is located after the argument location.   
        /// </returns>
        public Int32 CompareTo(object obj)
        {
            SegmentNode other = (SegmentNode) obj;
            if (SegmentIndex < other.SegmentIndex)
            {
                return -1;
            }
            if (SegmentIndex > other.SegmentIndex)
            {
                return 1;
            }
            if (Coordinate.Equals2D(other.Coordinate))
            {
                return 0;
            }
            return SegmentPointComparator.Compare(segmentOctant, Coordinate, other.Coordinate);
        }

        public void Write(StreamWriter outstream)
        {
            outstream.Write(Coordinate);
            outstream.Write(" seg # = " + SegmentIndex);
        }
    }
}