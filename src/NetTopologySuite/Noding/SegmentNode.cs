using System;
using System.IO;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Noding
{
    /// <summary>
    /// Represents an intersection point between two <see cref="ISegmentString" />s.
    /// </summary>
    public class SegmentNode : IComparable
    {
        /// <summary>
        ///
        /// </summary>
        public readonly Coordinate Coord;   // the point of intersection

        /// <summary>
        ///
        /// </summary>
        public readonly int SegmentIndex;   // the index of the containing line segment in the parent edge

        private readonly INodableSegmentString _segString;
        private readonly Octants _segmentOctant = Octants.Null;
        private readonly bool _isInterior;

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentNode"/> class.
        /// </summary>
        /// <param name="segString"></param>
        /// <param name="coord"></param>
        /// <param name="segmentIndex"></param>
        /// <param name="segmentOctant"></param>
        public SegmentNode(INodableSegmentString segString, Coordinate coord, int segmentIndex, Octants segmentOctant)
        {
            Coord = null;
            _segString = segString;
            Coord = coord.Copy();
            SegmentIndex = segmentIndex;
            _segmentOctant = segmentOctant;
            _isInterior = !coord.Equals2D(segString.Coordinates[segmentIndex]);
        }

        /// <summary>
        /// Gets the <see cref="Geometries.Coordinate"/> giving the location of this node.
        /// </summary>
        public Coordinate Coordinate => Coord;

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public bool IsInterior => _isInterior;

        /// <summary>
        ///
        /// </summary>
        /// <param name="maxSegmentIndex"></param>
        /// <returns></returns>
        public bool IsEndPoint(int maxSegmentIndex)
        {
            if (SegmentIndex == 0 && ! _isInterior)
                return true;
            return SegmentIndex == maxSegmentIndex;
        }

        /// <summary>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>
        /// -1 this SegmentNode is located before the argument location;<br/>
        ///  0 this SegmentNode is at the argument location;<br/>
        ///  1 this SegmentNode is located after the argument location.
        /// </returns>
        public int CompareTo(object obj)
        {
            var other = (SegmentNode) obj;
            if (SegmentIndex < other.SegmentIndex)
                return -1;
            if (SegmentIndex > other.SegmentIndex)
                return 1;
            if (Coord.Equals2D(other.Coord))
                return 0;

            // an exterior node is the segment start point, so always sorts first
            // this guards against a robustness problem where the octants are not reliable
            if (!IsInterior) return -1;
            if (!other.IsInterior) return 1;

            return SegmentPointComparator.Compare(_segmentOctant, Coord, other.Coord);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="outstream"></param>
        public void Write(StreamWriter outstream)
        {
            outstream.Write(Coord);
            outstream.Write(" seg # = " + SegmentIndex);
        }

        /// <inheritdoc cref="object.ToString()"/>>
        public override string ToString()
        {
            return $"{SegmentIndex}:{Coordinate}";
        }
    }
}
