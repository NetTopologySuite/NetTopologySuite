using System;
using System.IO;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.GeometriesGraph
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
        private readonly Coordinate _coordinate;

        /// <summary>
        /// The point of intersection.
        /// </summary>
        public Coordinate Coordinate => _coordinate;

        private readonly int _segmentIndex;

        /// <summary>
        /// The index of the containing line segment in the parent edge.
        /// </summary>
        public int SegmentIndex => _segmentIndex;

        private readonly double _distance;

        /// <summary>
        /// The edge distance of this point along the containing line segment.
        /// </summary>
        public double Distance => _distance;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="coord">The point of intersection</param>
        /// <param name="segmentIndex">The index of the containing line segment in the parent edge</param>
        /// <param name="dist">The edge distance or this point along the containing line segment</param>
        public EdgeIntersection(Coordinate coord, int segmentIndex, double dist)
        {
            _coordinate = coord.Copy();
            _segmentIndex = segmentIndex;
            _distance = dist;
        }


        /// <inheritdoc cref="IComparable.CompareTo"/>
        public int CompareTo(object obj)
        {
            var other = (EdgeIntersection) obj;
            return Compare(other.SegmentIndex, other.Distance);
        }

        /// <summary>
        /// Comparison with segment and distance.
        /// </summary>
        /// <param name="segmentIndex">The index of the containing line segment</param>
        /// <param name="dist">The distance of this point along the containing line segment</param>
        /// <returns>
        /// <c>-1</c> this EdgeIntersection is located before the argument location,
        /// <c>0</c> this EdgeIntersection is at the argument location,
        /// <c>1</c> this EdgeIntersection is located after the argument location.
        /// </returns>
        public int Compare(int segmentIndex, double dist)
        {
            if (_segmentIndex < segmentIndex)
                return -1;
            if (_segmentIndex > segmentIndex)
                return 1;
            if (_distance < dist)
                return -1;
            if (_distance > dist)
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

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            return _coordinate + " seg # = " + _segmentIndex + " dist = " + _distance;
        }

    }
}
