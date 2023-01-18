using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetTopologySuite.Triangulate.Polygon
{
    /// <summary>
    /// Transforms a polygon with holes into a single self-touching (invalid) ring
    /// by joining holes to the exterior shell or to another hole
    /// with out-and-back line segments.
    /// The holes are added in order of their envelopes (leftmost/lowest first).
    /// As the result shell develops, a hole may be added to what was
    /// originally another hole.
    /// <para/>
    /// There is no attempt to optimize the quality of the join lines.
    /// In particular, holes may be joined by lines longer than is optimal.
    /// However, holes which touch the shell or other holes are joined at the touch point.
    /// <para/>
    /// The class does not require the input polygon to have normal
    /// orientation (shell CW and rings CCW).
    /// The output ring is always CW.
    /// </summary>
    public class PolygonHoleJoiner
    {
        /// <summary>
        /// The comparer to use when sorting <see cref="_joinedPtsOrdered"/>
        /// </summary>
        private static readonly IComparer<Coordinate> _comparer =
            Comparer<Coordinate>.Create((u, v) => u.CompareTo(v));

        /// <summary>
        /// Joins the shell and holes of a polygon
        /// and returns the result as an (invalid) Polygon.
        /// </summary>
        /// <param name="polygon">The polygon to join</param>
        /// <returns>The result polygon</returns>
        public static Geometries.Polygon JoinAsPolygon(Geometries.Polygon polygon)
        {
            return polygon.Factory.CreatePolygon(Join(polygon));
        }

        /// <summary>
        /// Joins the shell and holes of a polygon
        /// and returns the result as sequence of Coordinates.
        /// </summary>
        /// <param name="polygon">The polygon to join</param>
        /// <returns>The result coordinates</returns>
        public static Coordinate[] Join(Geometries.Polygon polygon)
        {
            var joiner = new PolygonHoleJoiner(polygon);
            return joiner.Compute();
        }

        private readonly Geometries.Polygon _inputPolygon;
        //-- normalized, sorted and noded polygon rings
        private Coordinate[] _shellRing;
        private Coordinate[][] _holeRings;

        //-- indicates whether a hole should be testing for touching
        private bool[] _isHoleTouchingHint;

        private List<Coordinate> _joinedRing;
        // a sorted and searchable version of the joinedRing
        // Note: _joinedPts is a TreeSet in JTS which has functionality not
        //       provided by dotnet's SortedSet. Thus _joinedPts is split into
        //       HashSet _joinedPts and _joinedPtsOrdered for which
        //       Above, Below and Min are added.
        private HashSet<Coordinate> _joinedPts;
        private Coordinate[] _joinedPtsOrdered;
        //
        private ISegmentSetMutualIntersector _boundaryIntersector;

        /// <summary>
        /// Creates a new hole joiner.
        /// </summary>
        /// <param name="polygon">The polygon to join</param>
        public PolygonHoleJoiner(Geometries.Polygon polygon)
        {
            _inputPolygon = polygon;
        }

        /// <summary>
        /// Computes the joined ring
        /// </summary>
        /// <returns>The points in the joined ring</returns>
        public Coordinate[] Compute()
        {
            ExtractOrientedRings(_inputPolygon);
            if (_holeRings.Length > 0)
                NodeRings();
            _joinedRing = CopyToList(_shellRing);
            if (_holeRings.Length > 0)
                JoinHoles();
            return CoordinateArrays.ToCoordinateArray(_joinedRing);
        }

        private void ExtractOrientedRings(Geometries.Polygon polygon)
        {
            _shellRing = ExtractOrientedRing((LinearRing)polygon.ExteriorRing, true);
            var holes = SortHoles(polygon);
            _holeRings = new Coordinate[holes.Count][];
            for (int i = 0; i < holes.Count; i++)
            {
                _holeRings[i] = ExtractOrientedRing(holes[i], false);
            }
        }

        private static Coordinate[] ExtractOrientedRing(LinearRing ring, bool isCW)
        {
            var pts = ring.Coordinates;
            bool isRingCW = !Orientation.IsCCW(pts);
            if (isCW == isRingCW)
                return pts;
            //-- reverse a copy of the points
            var ptsRev = (Coordinate[])pts.Clone();
            CoordinateArrays.Reverse(ptsRev);
            return ptsRev;
        }

        private void NodeRings()
        {
            var noder = new PolygonNoder(_shellRing, _holeRings);
            noder.Node();
            if (noder.IsShellNoded)
            {
                _shellRing = noder.NodedShell;
            }
            for (int i = 0; i < _holeRings.Length; i++)
            {
                if (noder.IsHoleNoded(i))
                {
                    _holeRings[i] = noder.GetNodedHole(i);
                }
            }
            _isHoleTouchingHint = noder.HolesTouching;
        }

        private static List<Coordinate> CopyToList(Coordinate[] coords)
        {
            var coordList = new List<Coordinate>(coords.Length);
            foreach (var p in coords)
                coordList.Add(p.Copy());

            return coordList;
        }

        private void JoinHoles()
        {
            _boundaryIntersector = CreateBoundaryIntersector(_shellRing, _holeRings);

            _joinedPts = new HashSet<Coordinate>();
            foreach (var c in _joinedRing)
                AddOrderedCoord(c);

            for (int i = 0; i < _holeRings.Length; i++)
            {
                JoinHole(i, _holeRings[i]);
            }
        }

        /// <summary>
        /// Adds a coordinate to the <see cref="_joinedPts"/> set and
        /// clears the <see cref="_joinedPtsOrdered"/> array.
        /// </summary>
        /// <param name="coord">A coordinate</param>
        private void AddOrderedCoord(Coordinate coord)
        {
            if (_joinedPts.Add(coord))
                _joinedPtsOrdered = null;
        }

        private void JoinHole(int index, Coordinate[] holeCoords)
        {
            //-- check if hole is touching
            if (_isHoleTouchingHint[index])
            {
                bool isTouching = JoinTouchingHole(holeCoords);
                if (isTouching)
                    return;
            }
            JoinNonTouchingHole(holeCoords);
        }

        /// <summary>
        /// Joins a hole to the shell only if the hole touches the shell.
        /// Otherwise, reports the hole is non-touching.
        /// </summary>
        /// <param name="holeCoords">The hole to join</param>
        /// <returns><c>true</c> if the hole was touching, <c>false</c> if not</returns>
        private bool JoinTouchingHole(Coordinate[] holeCoords)
        {
            int holeTouchIndex = FindHoleTouchIndex(holeCoords);

            //-- hole does not touch
            if (holeTouchIndex < 0)
                return false;

            /*
             * Find shell corner which contains the hole,
             * by finding corner which has a hole segment at the join pt in interior
             */
            var joinPt = holeCoords[holeTouchIndex];
            var holeSegPt = holeCoords[Prev(holeTouchIndex, holeCoords.Length)];

            int joinIndex = FindJoinIndex(joinPt, holeSegPt);
            AddJoinedHole(joinIndex, holeCoords, holeTouchIndex);
            return true;
        }

        /// <summary>
        /// Finds the vertex index of a hole where it touches the
        /// current shell (if it does).
        /// If a hole does touch, it must touch at a single vertex
        /// (otherwise, the polygon is invalid).
        /// </summary>
        /// <param name="holeCoords">The hole</param>
        /// <returns>The index of the touching vertex, or -1 if no touch</returns>
        private int FindHoleTouchIndex(Coordinate[] holeCoords)
        {
            for (int i = 0; i < holeCoords.Length; i++)
            {
                if (_joinedPts.Contains(holeCoords[i]))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Joins a single non-touching hole to the current joined ring.
        /// </summary>
        /// <param name="holeCoords">The hole to join</param>
        private void JoinNonTouchingHole(Coordinate[] holeCoords)
        {
            int holeJoinIndex = FindLowestLeftVertexIndex(holeCoords);
            var holeJoinCoord = holeCoords[holeJoinIndex];
            var joinCoord = FindJoinableVertex(holeJoinCoord);
            int joinIndex = FindJoinIndex(joinCoord, holeJoinCoord);
            AddJoinedHole(joinIndex, holeCoords, holeJoinIndex);
        }

        /// <summary>
        /// Finds a shell vertex that is joinable to the hole join vertex.
        /// One must always exist, since the hole join vertex is on the left
        /// of the hole, and thus must always have at least one shell vertex visible to it.
        /// <para/>
        /// There is no attempt to optimize the selection of shell vertex
        /// to join to (e.g. by choosing one with shortest distance).
        /// </summary>
        /// <param name="holeJoinCoord">Theo hole join vertex</param>
        /// <returns>The shell vertex to join to</returns>
        private Coordinate FindJoinableVertex(Coordinate holeJoinCoord)
        {
            if (_joinedPtsOrdered == null)
                _joinedPtsOrdered = _joinedPts.OrderBy(x => x, _comparer).ToList().ToArray();

            //-- find highest shell vertex in half-plane left of hole pt
            var candidate = Above(holeJoinCoord);
            while (candidate.X == holeJoinCoord.X)
            {
                candidate = Above(candidate);
            }
            //-- drop back to last vertex with same X as hole
            candidate = Below(candidate);

            //-- find rightmost joinable shell vertex
            while (IntersectsBoundary(holeJoinCoord, candidate))
            {
                candidate = Below(candidate);
                //Assert: candidate is not null, since a joinable candidate always exists 
                if (candidate == null)
                {
                    throw new InvalidOperationException("Unable to find joinable vertex");
                }
            }
            return candidate;
        }

        /**
         * Gets the join ring vertex index that the hole is joined after.
         * A vertex can occur multiple times in the join ring, so it is necessary
         * to choose the one which forms a corner having the 
         * join line in the ring interior.
         * 
         * @param joinCoord the join ring vertex
         * @param holeJoinCoord the hole join vertex
         * @return the join ring vertex index to join after
         */
        private int FindJoinIndex(Coordinate joinCoord, Coordinate holeJoinCoord)
        {
            //-- linear scan is slow but only done once per hole
            for (int i = 0; i < _joinedRing.Count - 1; i++)
            {
                if (joinCoord.Equals2D(_joinedRing[i]))
                {
                    if (IsLineInterior(_joinedRing, i, holeJoinCoord))
                    {
                        return i;
                    }
                }
            }
            throw new InvalidOperationException("Unable to find shell join index with interior join line");
        }

        /// <summary>
        /// Tests if a line between a ring corner vertex and a given point
        /// is interior to the ring corner.
        /// </summary>
        /// <param name="ring">A ring of points</param>
        /// <param name="ringIndex">The index of a ring vertex</param>
        /// <param name="linePt">The point to be joined to the ring</param>
        /// <returns><c>true</c> if the line to the point is interior to the ring corner</returns>
        private bool IsLineInterior(List<Coordinate> ring, int ringIndex,
            Coordinate linePt)
        {
            var nodePt = ring[ringIndex];
            var shell0 = ring[Prev(ringIndex, ring.Count)];
            var shell1 = ring[Next(ringIndex, ring.Count)];
            return PolygonNodeTopology.IsInteriorSegment(nodePt, shell0, shell1, linePt);
        }

        private static int Prev(int i, int size)
        {
            int prev = i - 1;
            if (prev < 0)
                return size - 2;
            return prev;
        }

        private static int Next(int i, int size)
        {
            int next = i + 1;
            if (next > size - 2)
                return 0;
            return next;
        }

        /// <summary>
        /// Add hole vertices at proper position in shell vertex list.
        /// This code assumes that if hole touches (shell or other hole),
        /// it touches at a node.  This requires an initial noding step.
        /// In this case, the code avoids duplicating join vertices.
        /// <para/>
        /// Also adds hole points to ordered coordinates.
        /// </summary>
        /// <param name="joinIndex">Index of join vertex in shell</param>
        /// <param name="holeCoords">The vertices of the hole to be inserted</param>
        /// <param name="holeJoinIndex">Index of join vertex in hole</param>
        private void AddJoinedHole(int joinIndex, Coordinate[] holeCoords, int holeJoinIndex)
        {
            var joinPt = _joinedRing[joinIndex];
            var holeJoinPt = holeCoords[holeJoinIndex];

            //-- check for touching (zero-length) join to avoid inserting duplicate vertices
            bool isVertexTouch = joinPt.Equals2D(holeJoinPt);
            var addJoinPt = isVertexTouch ? null : joinPt;

            //-- create new section of vertices to insert in shell
            var newSection = CreateHoleSection(holeCoords, holeJoinIndex, addJoinPt);

            //-- add section after shell join vertex
            int addIndex = joinIndex + 1;
            _joinedRing.InsertRange(addIndex, newSection);
            foreach (var c in newSection)
                AddOrderedCoord(c);
        }

        /// <summary>
        /// Creates the new section of vertices for ad added hole,
        /// including any required vertices from the shell at the join point,
        /// and ensuring join vertices are not duplicated.
        /// </summary>
        /// <param name="holeCoords">The hole vertices</param>
        /// <param name="holeJoinIndex">The index of the join vertex</param>
        /// <param name="joinPt">The shell join vertex</param>
        /// <returns>A list of new vertices to be added</returns>
        private IList<Coordinate> CreateHoleSection(Coordinate[] holeCoords, int holeJoinIndex,
            Coordinate joinPt)
        {
            var section = new List<Coordinate>();

            bool isNonTouchingHole = joinPt != null;
            /*
             * Add all hole vertices, including duplicate at hole join vertex
             * Except if hole DOES touch, join vertex is already in shell ring
             */
            if (isNonTouchingHole)
                section.Add(holeCoords[holeJoinIndex].Copy());

            int holeSize = holeCoords.Length - 1;
            int index = holeJoinIndex;
            for (int i = 0; i < holeSize; i++)
            {
                index = (index + 1) % holeSize;
                section.Add(holeCoords[index].Copy());
            }
            /*
             * Add duplicate shell vertex at end of the return join line.
             * Except if hole DOES touch, join line is zero-length so do not need dup vertex
             */
            if (isNonTouchingHole)
            {
                section.Add(joinPt.Copy());
            }

            return section;
        }

        /// <summary>
        /// Sort the hole rings by minimum X, minimum Y.
        /// </summary>
        /// <param name="poly">Polygon that contains the holes</param>
        /// <returns>A list of sorted hole rings</returns>
        private static IList<LinearRing> SortHoles(Geometries.Polygon poly)
        {
            var holes = new List<LinearRing>();
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                holes.Add((LinearRing)poly.GetInteriorRingN(i));
            }
            holes.Sort(new EnvelopeComparator());
            return holes;
        }

        private class EnvelopeComparator : IComparer<Geometry> {
            public int Compare(Geometry g1, Geometry g2)
            {
                var e1 = g1.EnvelopeInternal;
                var e2 = g2.EnvelopeInternal;
                return e1.CompareTo(e2);
            }
        }

        private static int FindLowestLeftVertexIndex(Coordinate[] coords)
        {
            Coordinate lowestLeftCoord = null;
            int lowestLeftIndex = -1;
            for (int i = 0; i < coords.Length - 1; i++)
            {
                if (lowestLeftCoord == null || coords[i].CompareTo(lowestLeftCoord) < 0)
                {
                    lowestLeftCoord = coords[i];
                    lowestLeftIndex = i;
                }
            }
            return lowestLeftIndex;
        }

        /// <summary>
        /// Tests whether the interior of a line segment intersects the polygon boundary.
        /// If so, the line is not a valid join line.
        /// </summary>
        /// <param name="p0">A segment vertex</param>
        /// <param name="p1">Another segment vertex</param>
        /// <returns><c>true</c> if the segment interior intersects a polygon boundary segment</returns>
        private bool IntersectsBoundary(Coordinate p0, Coordinate p1)
        {
            var segString = new BasicSegmentString(
                new Coordinate[] { p0, p1 }, null);
            IList<ISegmentString> segStrings = new List<ISegmentString>();
            segStrings.Add(segString);

            var segInt = new InteriorIntersectionDetector();
            _boundaryIntersector.Process(segStrings, segInt);
            return segInt.HasIntersection;
        }

        /// <summary>
        /// Detects if a segment has an interior intersection with another segment. 
        /// </summary>
        private class InteriorIntersectionDetector : ISegmentIntersector
        {

            private readonly LineIntersector li = new RobustLineIntersector();

            public bool HasIntersection { get; private set; }

            public void ProcessIntersections(ISegmentString ss0, int segIndex0, ISegmentString ss1, int segIndex1)
            {
                var p00 = ss0.Coordinates[segIndex0];
                var p01 = ss0.Coordinates[segIndex0 + 1];
                var p10 = ss1.Coordinates[segIndex1];
                var p11 = ss1.Coordinates[segIndex1 + 1];

                li.ComputeIntersection(p00, p01, p10, p11);
                if (li.IntersectionNum == 0)
                {
                    return;
                }
                else if (li.IntersectionNum == 1)
                {
                    if (li.IsInteriorIntersection())
                        HasIntersection = true;
                }
                else
                { // li.getIntersectionNum() >= 2 - must be collinear
                    HasIntersection = true;
                }
            }

            public bool IsDone => HasIntersection;
        }

        private static ISegmentSetMutualIntersector CreateBoundaryIntersector(Coordinate[] shellRing, Coordinate[][] holeRings)
        {
            var polySegStrings = new List<ISegmentString>();
            polySegStrings.Add(new BasicSegmentString(shellRing, null));
            foreach (var hole in holeRings) {
                polySegStrings.Add(new BasicSegmentString(hole, null));
            }
            return new MCIndexSegmentSetMutualIntersector(polySegStrings);
        }
        #region Functionality from TreeSet

        private Coordinate Above(Coordinate coordinate)
        {
            if (_joinedPtsOrdered == null)
                throw new InvalidOperationException("_orderedCoordsArray not initialized");

            int index = Array.BinarySearch(_joinedPtsOrdered, coordinate);
            if (index < 0)
            {
                // Convert to index of item just higher than coordinate
                index = ~index;
            }
            else
            {
                // We have a match, need to increase index to get next higher value
                index++;
            }

            if (index < _joinedPtsOrdered.Length)
                return _joinedPtsOrdered[index];
            return null;
        }

        private Coordinate Below(Coordinate coordinate)
        {
            if (_joinedPtsOrdered == null)
                throw new InvalidOperationException("_orderedCoordsArray not initialized");

            int index = Array.BinarySearch(_joinedPtsOrdered, coordinate);
            if (index < 0)
                index = ~index;

            // We want the index of the item below
            index--;
            if (index >= 0)
                return _joinedPtsOrdered[index];
            return null;
        }

        #endregion

    }
}
