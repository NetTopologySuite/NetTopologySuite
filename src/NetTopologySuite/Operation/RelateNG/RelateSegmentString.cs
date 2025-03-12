using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;

namespace NetTopologySuite.Operation.RelateNG
{
    /// <summary>
    /// Models a linear edge of a <see cref="RelateGeometry"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    internal class RelateSegmentString : BasicSegmentString
    {
        public static RelateSegmentString CreateLine(Coordinate[] pts, bool isA, int elementId, RelateGeometry parent)
        {
            return CreateSegmentString(pts, isA, Dimension.L, elementId, -1, null, parent);
        }

        public static RelateSegmentString CreateRing(Coordinate[] pts, bool isA, int elementId, int ringId,
            Geometry poly, RelateGeometry parent)
        {
            return CreateSegmentString(pts, isA, Dimension.A, elementId, ringId, poly, parent);
        }

        private static RelateSegmentString CreateSegmentString(Coordinate[] pts, bool isA, Dimension dim, int elementId, int ringId,
            Geometry poly, RelateGeometry parent)
        {
            pts = RemoveRepeatedPoints(pts);
            return new RelateSegmentString(pts, isA, dim, elementId, ringId, poly, parent);
        }

        private static Coordinate[] RemoveRepeatedPoints(Coordinate[] pts)
        {
            if (CoordinateArrays.HasRepeatedPoints(pts))
            {
                pts = CoordinateArrays.RemoveRepeatedPoints(pts);
            }
            return pts;
        }

        private readonly Dimension _dimension;
        private readonly int _id;
        private readonly int _ringId;
        private readonly RelateGeometry _inputGeom;
        private readonly Geometry _parentPolygonal;

        private RelateSegmentString(Coordinate[] pts, bool isA, Dimension dimension, int id, int ringId, Geometry poly, RelateGeometry inputGeom)
            : base(pts, null)
        {
            IsA = isA;
            _dimension = dimension;
            _id = id;
            _ringId = ringId;
            _parentPolygonal = poly;
            _inputGeom = inputGeom;
        }

        public bool IsA { get; }

        public RelateGeometry Geometry => _inputGeom;

        public Geometry Polygonal => _parentPolygonal;

        public NodeSection CreateNodeSection(int segIndex, Coordinate intPt)
        {
            bool isNodeAtVertex =
                intPt.Equals2D(Coordinates[segIndex])
                || intPt.Equals2D(Coordinates[segIndex + 1]);
            var prev = PrevVertex(segIndex, intPt);
            var next = NextVertex(segIndex, intPt);
            var a = new NodeSection(IsA, _dimension, _id, _ringId, _parentPolygonal, isNodeAtVertex, prev, intPt, next);
            return a;
        }

        /// <returns>The previous vertex, or null if none exists</returns>
        private Coordinate PrevVertex(int segIndex, Coordinate pt)
        {
            var segStart = Coordinates[segIndex];
            if (!segStart.Equals2D(pt))
                return segStart;
            //-- pt is at segment start, so get previous vertex
            if (segIndex > 0)
                return Coordinates[segIndex - 1];
            if (IsClosed)
                return this.PrevInRing(segIndex);
            return null;
        }

        /// <returns>The previous vertex, or null if none exists</returns>
        private Coordinate NextVertex(int segIndex, Coordinate pt)
        {
            var segEnd = Coordinates[segIndex + 1];
            if (!segEnd.Equals2D(pt))
                return segEnd;
            //-- pt is at seg end, so get next vertex
            if (segIndex < Count - 2)
                return Coordinates[segIndex + 2];
            if (IsClosed)
                return this.NextInRing(segIndex + 1);
            //-- segstring is not closed, so there is no next segment
            return null;
        }

        /// <summary>
        /// Tests if a segment intersection point has that segment as its
        /// canonical containing segment.
        /// Segments are half-closed, and contain their start point but not the endpoint,
        /// except for the final segment in a non-closed segment string, which contains
        /// its endpoint as well.
        /// This test ensures that vertices are assigned to a unique segment in a segment string.
        /// In particular, this avoids double-counting intersections which lie exactly
        /// at segment endpoints.
        /// </summary>
        /// <param name="segIndex">The segment the point may lie on</param>
        /// <param name="pt">The point</param>
        /// <returns><c>true</c> if the segment contains the point</returns>
        public bool IsContainingSegment(int segIndex, Coordinate pt)
        {
            //-- intersection is at segment start vertex - process it
            if (pt.Equals2D(Coordinates[segIndex]))
                return true;
            if (pt.Equals2D(Coordinates[segIndex + 1]))
            {
                bool isFinalSegment = segIndex == Count - 2;
                if (IsClosed || !isFinalSegment)
                    return false;
                //-- for final segment, process intersections with final endpoint
                return true;
            }
            //-- intersection is interior - process it
            return true;
        }
    }
}
