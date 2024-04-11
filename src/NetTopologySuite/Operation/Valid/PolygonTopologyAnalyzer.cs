using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;

namespace NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// Analyzes the topology of polygonal geometry
    /// to determine whether it is valid.
    /// </summary>
    /// <author>Martin Davis</author>
    class PolygonTopologyAnalyzer
    {

        /// <summary>
        /// Tests whether a ring is nested inside another ring
        /// <para/>
        /// Preconditions:
        /// <list type="bullet">
        /// <item><description>The rings do not cross (i.e. the test is wholly inside or outside the target)</description></item>
        /// <item><description>The rings may touch at discrete points only</description></item>
        /// <item><description>The <paramref name="target"/> ring does not self-cross, but it may self-touch</description></item>
        /// </list>
        /// If the test ring start point is properly inside or outside, that provides the result.
        /// Otherwise the start point is on the target ring,
        /// and the incident start segment(accounting for repeated points) is
        /// tested for its topology relative to the target ring.
        /// </summary>
        /// <param name="test">The ring to test</param>
        /// <param name="target">The ring to test against</param>
        /// <returns><c>true</c> if the <paramref name="test"/> ring lies inside the <paramref name="target"/> ring</returns>
        public static bool IsRingNested(LineString test, LineString target)
        {
            var p0 = test.GetCoordinateN(0);
            var targetPts = target.Coordinates;

            var loc = PointLocation.LocateInRing(p0, targetPts);
            if (loc == Location.Exterior) return false;
            if (loc == Location.Interior) return true;

            /*
             * The start segment point is on the boundary of the ring.
             * Use the topology at the node to check if the segment
             * is inside or outside the ring.
             */
            var p1 = FindNonEqualVertex(test, p0);
            return IsIncidentSegmentInRing(p0, p1, targetPts);
        }

        private static Coordinate FindNonEqualVertex(LineString ring, Coordinate p)
        {
            int i = 1;
            var next = ring.GetCoordinateN(i);
            while (next.Equals2D(p) && i < ring.NumPoints - 1)
            {
                i += 1;
                next = ring.GetCoordinateN(i);
            }
            return next;
        }

        /// <summary>
        /// Tests whether a touching segment is interior to a ring.
        /// <para/>
        /// Preconditions:
        /// <list type="bullet">
        /// <item><description>The segment does not intersect the ring other than at the endpoints</description></item>
        /// <item><description>The segment vertex p0 lies on the ring</description></item>
        /// <item><description>The ring does not self-cross, but it may self-touch</description></item>
        /// </list>
        /// This works for both shells and holes, but the caller must know
        /// the ring role.
        /// </summary>
        /// <param name="p0">The touching vertex of the segment</param>
        /// <param name="p1">The second vertex of the segment</param>
        /// <param name="ringPts">The points of the ring</param>
        /// <returns><c>true</c> if the segment is inside the ring.</returns>
        private static bool IsIncidentSegmentInRing(Coordinate p0, Coordinate p1, Coordinate[] ringPts)
        {
            int index = IntersectingSegIndex(ringPts, p0);
            if (index < 0)
            {
                throw new ArgumentException("Segment vertex does not intersect ring");
            }
            var rPrev = FindRingVertexPrev(ringPts, index, p0);
            var rNext = FindRingVertexNext(ringPts, index, p0);
            /*
             * If ring orientation is not normalized, flip the corner orientation
             */
            bool isInteriorOnRight = !Orientation.IsCCW(ringPts);
            if (!isInteriorOnRight)
            {
                var temp = rPrev;
                rPrev = rNext;
                rNext = temp;
            }
            return PolygonNodeTopology.IsInteriorSegment(p0, rPrev, rNext, p1);
        }

        /// <summary>
        /// Finds the ring vertex previous to a node point on a ring
        /// (which is contained in the index'th segment,
        /// as either the start vertex or an interior point).
        /// Repeated points are skipped over.
        /// </summary>
        /// <param name="ringPts">The ring</param>
        /// <param name="index">The index of the segment containing the node</param>
        /// <param name="node">The node point</param>
        /// <returns>The previous ring vertex</returns>
        private static Coordinate FindRingVertexPrev(Coordinate[] ringPts, int index, Coordinate node)
        {
            int iPrev = index;
            var prev = ringPts[iPrev];
            while (node.Equals2D(prev))
            {
                iPrev = RingIndexPrev(ringPts, iPrev);
                prev = ringPts[iPrev];
            }
            return prev;
        }

        /// <summary>
        /// Finds the ring vertex next from a node point on a ring
        /// (which is contained in the index'th segment,
        /// as either the start vertex or an interior point). 
        /// </summary>
        /// <param name="ringPts">The ring</param>
        /// <param name="index">The index of the segment containing the node</param>
        /// <param name="node">The node point</param>
        /// <returns>The next ring vertex</returns>
        private static Coordinate FindRingVertexNext(Coordinate[] ringPts, int index, Coordinate node)
        {
            //-- safe, since index is always the start of a ring segment
            int iNext = index + 1;
            var next = ringPts[iNext];
            while (node.Equals2D(next))
            {
                iNext = RingIndexNext(ringPts, iNext);
                next = ringPts[iNext];
            }
            return next;
        }

        private static int RingIndexPrev(Coordinate[] ringPts, int index)
        {
            if (index == 0)
                return ringPts.Length - 2;
            return index - 1;
        }

        private static int RingIndexNext(Coordinate[] ringPts, int index)
        {
            if (index >= ringPts.Length - 2)
                return 0;
            return index + 1;
        }

        /// <summary>
        /// Computes the index of the segment which intersects a given point.
        /// </summary>
        /// <param name="ringPts">The ring points</param>
        /// <param name="pt">The intersection point</param>
        /// <returns>The intersection segment index, or <c>-1</c> if not intersection is found.</returns>
        private static int IntersectingSegIndex(Coordinate[] ringPts, Coordinate pt)
        {
            for (int i = 0; i < ringPts.Length - 1; i++)
            {
                if (PointLocation.IsOnSegment(pt, ringPts[i], ringPts[i + 1]))
                {
                    //-- check if pt is the start point of the next segment
                    if (pt.Equals2D(ringPts[i + 1]))
                    {
                        return i + 1;
                    }
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Finds a self-intersection (if any) in a <see cref="LinearRing"/>.
        /// </summary>
        /// <param name="ring">The ring to analyze</param>
        /// <returns>A self-intersection point if one exists, or <c>null</c></returns>
        public static Coordinate FindSelfIntersection(LinearRing ring)
        {
            var ata = new PolygonTopologyAnalyzer(ring, false);
            if (ata.HasInvalidIntersection)
                return ata.InvalidLocation;
            return null;
        }

        private readonly bool _isInvertedRingValid;

        private PolygonIntersectionAnalyzer _intFinder;
        private List<PolygonRing> _polyRings;
        private Coordinate _disconnectionPt;

        public PolygonTopologyAnalyzer(Geometry geom, bool isInvertedRingValid)
        {
            _isInvertedRingValid = isInvertedRingValid;
            Analyze(geom);
        }

        public bool HasInvalidIntersection
        {
            get => _intFinder.IsInvalid;
        }

        public TopologyValidationErrors InvalidCode => _intFinder.InvalidCode;

        public Coordinate InvalidLocation => _intFinder.InvalidLocation;

        public bool HasDoubleTouch
        {
            get => _intFinder.HasDoubleTouch;
        }

        public Coordinate IntersectionLocation
        {
            get => _intFinder.InvalidLocation;
        }

        /// <summary>
        /// Tests whether the interior of the polygonal geometry is
        /// disconnected.<br/>
        /// If true, the disconnection location is available from 
        /// <see cref="DisconnectionLocation"/>.
        /// </summary>
        /// <returns><c>true</c> if the interior is disconnected</returns>
        public bool IsInteriorDisconnected()
        {
            /*
             * May already be set by a double-touching hole
             */
            if (_disconnectionPt != null)
            {
                return true;
            }
            if (_isInvertedRingValid)
            {
                CheckInteriorDisconnectedBySelfTouch();
                if (_disconnectionPt != null)
                {
                    return true;
                }
            }
            CheckInteriorDisconnectedByHoleCycle();
            if (_disconnectionPt != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets a location where the polyonal interior is disconnected.<br/>
        /// <see cref="IsInteriorDisconnected"/> must be called first.
        /// </summary>
        /// <returns>The location of an interior disconnection, or <c>null</c></returns>
        public Coordinate DisconnectionLocation => _disconnectionPt;

        /// <summary>
        /// Tests whether any polygon with holes has a disconnected interior
        /// by virtue of the holes (and possibly shell) forming a touch cycle.
        /// <para/>
        /// This is a global check, which relies on determining
        /// the touching graph of all holes in a polygon.
        /// <para/>
        /// If inverted rings disconnect the interior
        /// via a self-touch, this is checked by the <see cref="PolygonIntersectionAnalyzer"/>.
        /// If inverted rings are part of a disconnected ring chain
        /// this is detected here.  
        /// </summary>
        /// <returns><c>true</c> if a polygon has a disconnected interior.</returns>
        public bool CheckInteriorDisconnectedByHoleCycle()
        {
            /*
             * PolyRings will be null for empty, no hole or LinearRing inputs
             */
            if (_polyRings != null)
            {
                _disconnectionPt = PolygonRing.FindHoleCycleLocation(_polyRings);
            }
            return _disconnectionPt != null;
        }

        /// <summary>
        /// Tests if an area interior is disconnected by a self-touching ring.
        /// This must be evaluated after other self-intersections have been analyzed
        /// and determined to not exist, since the logic relies on
        /// the rings not self-crossing (winding).
        /// </summary>
        /// <returns><c>true</c> if an area interior is disconnected by a self-touch</returns>
        public void CheckInteriorDisconnectedBySelfTouch()
        {
            if (_polyRings != null)
            {
                _disconnectionPt = PolygonRing.FindInteriorSelfNode(_polyRings);
            }
        }

        private void Analyze(Geometry geom)
        {
            if (geom.IsEmpty)
                return;
            var segStrings = CreateSegmentStrings(geom, _isInvertedRingValid);
            _polyRings = GetPolygonRings(segStrings);
            _intFinder = AnalyzeIntersections(segStrings);

            if (_intFinder.HasDoubleTouch)
            {
                _disconnectionPt = _intFinder.DoubleTouchLocation;
                return;
            }
        }

        private PolygonIntersectionAnalyzer AnalyzeIntersections(IList<ISegmentString> segStrings)
        {
            var segInt = new PolygonIntersectionAnalyzer(_isInvertedRingValid);
            var noder = new MCIndexNoder();
            noder.SegmentIntersector = segInt;
            noder.ComputeNodes(segStrings);
            return segInt;
        }

        private static IList<ISegmentString> CreateSegmentStrings(Geometry geom, bool isInvertedRingValid)
        {
            var segStrings = new List<ISegmentString>();
            if (geom is LinearRing ring) {
                segStrings.Add(CreateSegString(ring, null));
                return segStrings;
            }
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var poly = (Polygon)geom.GetGeometryN(i);
                if (poly.IsEmpty) continue;
                bool hasHoles = poly.NumInteriorRings > 0;

                //--- polygons with no holes do not need connected interior analysis
                PolygonRing shellRing = null;
                if (hasHoles || isInvertedRingValid)
                {
                    shellRing = new PolygonRing((LinearRing)poly.ExteriorRing);
                }
                segStrings.Add(CreateSegString((LinearRing)poly.ExteriorRing, shellRing));

                for (int j = 0; j < poly.NumInteriorRings; j++)
                {
                    var hole = (LinearRing)poly.GetInteriorRingN(j);
                    if (hole.IsEmpty) continue;
                    var holeRing = new PolygonRing(hole, j, shellRing);
                    segStrings.Add(CreateSegString(hole, holeRing));
                }
            }
            return segStrings;
        }

        private static List<PolygonRing> GetPolygonRings(IList<ISegmentString> segStrings)
        {
            List<PolygonRing> polyRings = null;
            foreach(var ss in segStrings)
            {
                var polyRing = (PolygonRing)ss.Context;
                if (polyRing != null)
                {
                    if (polyRings == null)
                    {
                        polyRings = new List<PolygonRing>();
                    }
                    polyRings.Add(polyRing);
                }
            }
            return polyRings;
        }
        private void AddPolygonRing(PolygonRing polyRing)
        {
            if (_polyRings == null)
            {
                _polyRings = new List<PolygonRing>();
            }
            _polyRings.Add(polyRing);
        }

        private static BasicSegmentString CreateSegString(LinearRing ring, PolygonRing polyRing)
        {
            var pts = ring.Coordinates;

            //--- repeated points must be removed for accurate intersection detection
            if (CoordinateArrays.HasRepeatedPoints(pts))
            {
                pts = CoordinateArrays.RemoveRepeatedPoints(pts);
            }

            var ss = new BasicSegmentString(pts, polyRing);
            return ss;
        }

    }
}
