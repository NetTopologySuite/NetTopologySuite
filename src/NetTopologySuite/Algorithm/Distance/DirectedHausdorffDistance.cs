using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm.Construct;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Distance;
using NetTopologySuite.Utilities;
using Point = NetTopologySuite.Geometries.Point;

namespace NetTopologySuite.Algorithm.Distance
{
    /// <summary>
    /// Computes the directed Hausdorff distance from one geometry to another.
    /// The directed Hausdorff distance is the maximum distance any point
    /// on a query geometry A can be from a target geometry B.
    /// Equivalently, every point in the query geometry is within that distance
    /// of the target geometry.
    /// The class can compute a pair of points at which the distance is attained:
    /// <c>[ farthest A point, nearest B point ]</c>.
    /// <para/>
    /// The directed Hausdorff distance (DHD) is defined as:
    /// <code>
    /// DHD(A, B) = max(a in A)  max(b in B)  distance(a, b)
    /// </code>
    /// DHD is asymmetric: <c>DHD(A, B)</c> may not be equal to <c>DHD(B, A)</c>.
    /// Hence it is not a distance metric.
    /// The Hausdorff distance is a symmetric distance metric defined as:
    /// <code>
    /// HD(A, B) = max(DHD(A, B), DHD(B, A))
    /// </code>
    /// This can be computed via the <see cref="HausdorffDistancePoints"/> function.
    /// <para/>
    /// Points, lines and polygons are supported as input.
    /// If the query geometry is polygonal,
    /// the point at maximum distance may occur in the interior of a query polygon.
    /// For a polygonal target geometry the point always lies on the boundary.
    /// <para/>
    /// The directed Hausdorff distance can be used to test
    /// whether a geometry lies fully within a given distance of another one.
    /// The <see cref="IsFullyWithinDistance(Geometry, Geometry, double)"/> function
    /// is provided to execute this test efficiently. It implements heuristic checks
    /// and short-circuiting to improve performance.
    /// <para/>
    /// The class can be used in prepared mode.
    /// Creating an instance on a target geometry caches indexes for that geometry.
    /// Then <see cref="FarthestPoints(Geometry)"/> or <see cref="IsFullyWithinDistance(Geometry, double)"/>
    /// can be called efficiently for multiple query geometries.
    /// <para/>
    /// If the Hausdorff distance is attained at a non-vertex of the query geometry,
    /// the location must be approximated. The algorithm uses a distance tolerance
    /// to control the approximation accuracy. The tolerance is automatically determined
    /// to balance between accuracy and performance. If more accuracy is desired some
    /// function signatures are provided which allow specifying a distance tolerance.
    /// <para/>
    /// This algorithm is easier to use, more accurate, and much faster than
    /// <see cref="DiscreteHausdorffDistance"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    public class DirectedHausdorffDistance
    {
        private const double EmptyDistance = double.NaN;

        /// <summary>Heuristic automatic tolerance factor.</summary>
        private const double AutoToleranceFactor = 1.0e4;

        /// <summary>
        /// Heuristic factor to improve performance of area interior farthest point computation.
        /// The LargestEmptyCircle computation is much slower than the boundary one,
        /// is unlikely to occur, and accuracy is less critical (and obvious).
        /// </summary>
        private const double AreaInteriorToleranceFactor = 20;

        /// <summary>
        /// Tolerance factor for <see cref="IsFullyWithinDistance(Geometry, double)"/>.
        /// A larger factor is used to increase accuracy. The operation will usually short-circuit,
        /// so performance impact is low.
        /// </summary>
        private const double FullyWithinToleranceFactor = 10 * AutoToleranceFactor;

        /// <summary>
        /// Computes the directed Hausdorff distance of a query geometry A from a target one B.
        /// </summary>
        /// <param name="a">The query geometry</param>
        /// <param name="b">The target geometry</param>
        /// <returns>The directed Hausdorff distance, or <c>NaN</c> if an input is empty</returns>
        public static double Distance(Geometry a, Geometry b)
        {
            var hd = new DirectedHausdorffDistance(b);
            return Distance(hd.FarthestPoints(a));
        }

        /// <summary>
        /// Computes the directed Hausdorff distance of a query geometry A from a target one B,
        /// up to a given distance accuracy.
        /// </summary>
        /// <param name="a">The query geometry</param>
        /// <param name="b">The target geometry</param>
        /// <param name="tolerance">The accuracy distance tolerance</param>
        /// <returns>The directed Hausdorff distance, or <c>NaN</c> if an input is empty</returns>
        public static double Distance(Geometry a, Geometry b, double tolerance)
        {
            var hd = new DirectedHausdorffDistance(b);
            return Distance(hd.FarthestPoints(a, tolerance));
        }

        /// <summary>
        /// Computes a pair of points which attain the directed Hausdorff distance
        /// of a query geometry A from a target one B.
        /// </summary>
        /// <param name="a">The query geometry</param>
        /// <param name="b">The target geometry</param>
        /// <returns>A pair of points <c>[ptA, ptB]</c> demonstrating the distance, or <c>null</c> if an input is empty</returns>
        public static Coordinate[] DistancePoints(Geometry a, Geometry b)
        {
            var dhd = new DirectedHausdorffDistance(b);
            return dhd.FarthestPoints(a);
        }

        /// <summary>
        /// Computes a pair of points which attain the directed Hausdorff distance
        /// of a query geometry A from a target one B, up to a given distance accuracy.
        /// </summary>
        /// <param name="a">The query geometry</param>
        /// <param name="b">The target geometry</param>
        /// <param name="tolerance">The accuracy distance tolerance</param>
        /// <returns>A pair of points <c>[ptA, ptB]</c> demonstrating the distance, or <c>null</c> if an input is empty</returns>
        public static Coordinate[] DistancePoints(Geometry a, Geometry b, double tolerance)
        {
            var dhd = new DirectedHausdorffDistance(b);
            return dhd.FarthestPoints(a, tolerance);
        }

        /// <summary>
        /// Computes a pair of points which attain the symmetric Hausdorff distance between two geometries.
        /// This is the maximum of the two directed Hausdorff distances.
        /// </summary>
        /// <param name="a">A geometry</param>
        /// <param name="b">A geometry</param>
        /// <returns>A pair of points <c>[ptA, ptB]</c> demonstrating the Hausdorff distance, or <c>null</c> if an input is empty</returns>
        public static Coordinate[] HausdorffDistancePoints(Geometry a, Geometry b)
        {
            var hdAB = new DirectedHausdorffDistance(b);
            var ptsAB = hdAB.FarthestPoints(a);
            var hdBA = new DirectedHausdorffDistance(a);
            var ptsBA = hdBA.FarthestPoints(b);

            //-- return points in A-B order
            var pts = ptsAB;
            if (Distance(ptsBA) > Distance(ptsAB))
            {
                //-- reverse the BA points
                pts = Pair(ptsBA[1], ptsBA[0]);
            }
            return pts;
        }

        /// <summary>
        /// Computes the symmetric Hausdorff distance between two geometries.
        /// This is the maximum of the two directed Hausdorff distances.
        /// </summary>
        /// <param name="a">A geometry</param>
        /// <param name="b">A geometry</param>
        /// <returns>The Hausdorff distance, or <c>NaN</c> if an input is empty</returns>
        public static double HausdorffDistance(Geometry a, Geometry b)
        {
            return Distance(HausdorffDistancePoints(a, b));
        }

        /// <summary>
        /// Computes whether a query geometry lies fully within a given distance of a target geometry.
        /// </summary>
        public static bool IsFullyWithinDistance(Geometry a, Geometry b, double maxDistance)
        {
            var hd = new DirectedHausdorffDistance(b);
            return hd.IsFullyWithinDistance(a, maxDistance);
        }

        /// <summary>
        /// Computes whether a query geometry lies fully within a given distance of a target geometry,
        /// up to a given distance accuracy.
        /// </summary>
        public static bool IsFullyWithinDistance(Geometry a, Geometry b, double maxDistance, double tolerance)
        {
            var hd = new DirectedHausdorffDistance(b);
            return hd.IsFullyWithinDistance(a, maxDistance, tolerance);
        }

        private static double Distance(Coordinate[] pts)
        {
            if (pts == null)
                return EmptyDistance;
            return pts[0].Distance(pts[1]);
        }

        private static Coordinate[] Pair(Coordinate p0, Coordinate p1)
        {
            return new[] { p0.Copy(), p1.Copy() };
        }

        private static double ComputeTolerance(Geometry geom)
        {
            return geom.EnvelopeInternal.Diameter / AutoToleranceFactor;
        }

        private readonly Geometry _target;
        private readonly TargetDistance _targetDistance;

        /// <summary>
        /// Create a new instance for a target geometry.
        /// </summary>
        /// <param name="geom">The geometry to compute the distance from</param>
        public DirectedHausdorffDistance(Geometry geom)
        {
            _target = geom;
            _targetDistance = new TargetDistance(_target);
        }

        /// <summary>
        /// Tests whether a query geometry lies fully within a given distance of the target geometry.
        /// </summary>
        public bool IsFullyWithinDistance(Geometry geom, double maxDistance)
        {
            double tolerance = maxDistance / FullyWithinToleranceFactor;
            return IsFullyWithinDistance(geom, maxDistance, tolerance);
        }

        /// <summary>
        /// Tests whether a query geometry lies fully within a given distance of the target geometry,
        /// up to a given distance accuracy.
        /// </summary>
        public bool IsFullyWithinDistance(Geometry geom, double maxDistance, double tolerance)
        {
            //-- envelope checks
            if (IsBeyond(geom.EnvelopeInternal, _target.EnvelopeInternal, maxDistance))
                return false;

            var maxDistCoords = ComputeDistancePoints(geom, tolerance, maxDistance);
            //-- handle empty case
            if (maxDistCoords == null)
                return false;
            return Distance(maxDistCoords) <= maxDistance;
        }

        /// <summary>
        /// Tests if an envelope must have a side farther than the maximum distance from another envelope.
        /// </summary>
        private static bool IsBeyond(Envelope envA, Envelope envB, double maxDistance)
        {
            return envA.MinX < envB.MinX - maxDistance
                || envA.MinY < envB.MinY - maxDistance
                || envA.MaxX > envB.MaxX + maxDistance
                || envA.MaxY > envB.MaxY + maxDistance;
        }

        /// <summary>
        /// Computes a pair of points which attain the directed Hausdorff distance
        /// of a query geometry A from the target B.
        /// </summary>
        public Coordinate[] FarthestPoints(Geometry geom)
        {
            double tolerance = ComputeTolerance(geom);
            return FarthestPoints(geom, tolerance);
        }

        /// <summary>
        /// Computes a pair of points which attain the directed Hausdorff distance
        /// of a query geometry A from the target B, up to a given distance accuracy.
        /// </summary>
        public Coordinate[] FarthestPoints(Geometry geom, double tolerance)
        {
            return ComputeDistancePoints(geom, tolerance, -1.0);
        }

        private Coordinate[] ComputeDistancePoints(Geometry geom, double tolerance, double maxDistanceLimit)
        {
            //-- Negative tolerances are not allowed.
            //-- Zero tolerance is allowed, to support zero-size input.
            if (tolerance < 0.0)
                throw new ArgumentException("Tolerance must be non-negative", nameof(tolerance));

            if (geom.IsEmpty || _target.IsEmpty)
                return null;

            if (geom.Dimension == Dimension.Point)
            {
                return ComputeForPoints(geom, maxDistanceLimit);
            }

            var maxDistPtsEdge = ComputeForEdges(geom, tolerance, maxDistanceLimit);

            if (IsBeyondLimit(Distance(maxDistPtsEdge), maxDistanceLimit))
            {
                return maxDistPtsEdge;
            }

            //-- Polygonal query geometry may have an interior point as the farthest point.
            if (geom.Dimension == Dimension.Surface)
            {
                var maxDistPtsInterior = ComputeForAreaInterior(geom, tolerance);
                if (maxDistPtsInterior != null
                    && Distance(maxDistPtsInterior) > Distance(maxDistPtsEdge))
                {
                    return maxDistPtsInterior;
                }
            }
            return maxDistPtsEdge;
        }

        private Coordinate[] ComputeForPoints(Geometry geom, double maxDistanceLimit)
        {
            double maxDist = -1.0;
            Coordinate[] maxDistPtsAB = null;
            var geomi = new GeometryCollectionEnumerator(geom);
            while (geomi.MoveNext())
            {
                var geomElem = geomi.Current;
                if (!(geomElem is Point))
                    continue;

                var pA = geomElem.Coordinate;
                var pB = _targetDistance.NearestPoint(pA);
                double dist = pA.Distance(pB);

                bool isInterior = dist > 0 && _targetDistance.IsInterior(pA);
                //-- check for interior point
                if (isInterior)
                {
                    dist = 0;
                    pB = pA;
                }
                if (dist > maxDist)
                {
                    maxDist = dist;
                    maxDistPtsAB = Pair(pA, pB);
                }
                if (IsValidLimit(maxDistanceLimit)
                    && IsBeyondLimit(maxDist, maxDistanceLimit))
                {
                    break;
                }
            }
            return maxDistPtsAB;
        }

        private Coordinate[] ComputeForEdges(Geometry geom, double tolerance, double maxDistanceLimit)
        {
            var segQueue = CreateSegQueue(geom);

            DhdSegment segMaxDist = null;
            while (!segQueue.IsEmpty())
            {
                // get the segment with greatest distance bound
                var segMaxBound = segQueue.Poll();

                //-- Save if segment point is farther than current farthest
                if (segMaxDist == null
                    || segMaxBound.MaxDistance > segMaxDist.MaxDistance)
                {
                    segMaxDist = segMaxBound;
                }

                //-- Stop searching if remaining items must all be closer than current maximum.
                if (segMaxBound.MaxDistanceBound <= segMaxDist.MaxDistance)
                {
                    break;
                }

                //-- If maxDistanceLimit is specified, can stop searching in two cases.
                if (IsValidLimit(maxDistanceLimit))
                {
                    if (IsWithinLimit(segMaxBound.MaxDistanceBound, maxDistanceLimit)
                        || IsBeyondLimit(segMaxBound.MaxDistance, maxDistanceLimit))
                    {
                        break;
                    }
                }

                //-- Equal-or-collinear segments don't need further bisection.
                //-- This greatly improves performance when inputs are identical/collinear.
                if (segMaxBound.MaxDistance == 0.0)
                {
                    if (IsSameOrCollinear(segMaxBound))
                        continue;
                }

                //-- If segment is longer than tolerance, bisect and keep searching.
                if (tolerance > 0
                    && segMaxBound.Length > tolerance)
                {
                    var bisects = segMaxBound.Bisect(_targetDistance);
                    AddNonInterior(bisects[0], segQueue);
                    AddNonInterior(bisects[1], segQueue);
                }
            }

            if (segMaxDist != null)
                return segMaxDist.GetMaxDistPts();

            //-- No DHD segment was found: all were inside the target.
            //-- In this case distance is zero. Return a single coordinate as a representative point.
            var maxPt = geom.Coordinate;
            return Pair(maxPt, maxPt);
        }

        private bool IsSameOrCollinear(DhdSegment seg)
        {
            var f0 = _targetDistance.NearestLocation(seg.P0);
            var f1 = _targetDistance.NearestLocation(seg.P1);
            return IsSameSegment(f0, f1);
        }

        private static bool IsSameSegment(GeometryLocation a, GeometryLocation b)
        {
            if (a == null || b == null) return false;
            return ReferenceEquals(a.GeometryComponent, b.GeometryComponent)
                && a.SegmentIndex == b.SegmentIndex;
        }

        private static bool IsValidLimit(double limit) => limit >= 0.0;

        private static bool IsBeyondLimit(double maxDist, double maxDistanceLimit)
            => maxDistanceLimit >= 0 && maxDist > maxDistanceLimit;

        private static bool IsWithinLimit(double maxDist, double maxDistanceLimit)
            => maxDistanceLimit >= 0 && maxDist <= maxDistanceLimit;

        private void AddNonInterior(DhdSegment segment, PriorityQueue<DhdSegment> segQueue)
        {
            //-- discard segment if it is interior to a polygon
            if (IsInterior(segment))
                return;
            //-- DhdSegment.CompareTo inverts ordering so Poll() returns the segment with greatest bound.
            segQueue.Add(segment);
        }

        /// <summary>
        /// Tests if segment is fully in the interior of the target geometry polygons (if any).
        /// </summary>
        private bool IsInterior(DhdSegment segment)
        {
            if (segment.MaxDistance > 0.0)
                return false;
            return _targetDistance.IsInterior(segment.Endpoint(0), segment.Endpoint(1));
        }

        /// <summary>
        /// If the query geometry is polygonal, it is possible the farthest point lies in its interior.
        /// In this case it occurs at the centre of the Largest Empty Circle
        /// with B as obstacles and the query geometry as constraint.
        /// </summary>
        private Coordinate[] ComputeForAreaInterior(Geometry geom, double tolerance)
        {
            if (tolerance <= 0.0)
                return null;

            var polygonal = geom;

            //-- Optimization: skip if A interior cannot intersect B,
            //-- so farthest point must lie on A boundary
            if (polygonal.EnvelopeInternal.Disjoint(_target.EnvelopeInternal))
                return null;

            var centerPt = LargestEmptyCircle.GetCenter(_target, polygonal,
                tolerance * AreaInteriorToleranceFactor);
            var ptA = centerPt.Coordinate;
            //-- If LEC centre is in B, the max distance is zero, so return null.
            if (_targetDistance.IsInterior(ptA))
                return null;
            var ptB = _targetDistance.NearestFacetPoint(ptA);
            return Pair(ptA, ptB);
        }

        /// <summary>
        /// Creates the priority queue for segments. Segments which are interior to a polygonal
        /// target geometry are not added to the queue.
        /// </summary>
        private PriorityQueue<DhdSegment> CreateSegQueue(Geometry geom)
        {
            var priq = new PriorityQueue<DhdSegment>();
            geom.Apply(new GeometryComponentFilter(g =>
            {
                if (g is LineString line)
                {
                    AddSegments(line.Coordinates, priq);
                }
            }));
            return priq;
        }

        private void AddSegments(Coordinate[] pts, PriorityQueue<DhdSegment> priq)
        {
            DhdSegment segMaxDist = null;
            DhdSegment prevSeg = null;
            for (int i = 0; i < pts.Length - 1; i++)
            {
                DhdSegment seg;
                if (i == 0)
                {
                    seg = DhdSegment.Create(pts[i], pts[i + 1], _targetDistance);
                }
                else
                {
                    //-- avoid recomputing prev point distance
                    seg = DhdSegment.Create(prevSeg, pts[i + 1], _targetDistance);
                }
                prevSeg = seg;

                //-- don't add segment if it can't be further away than current max
                if (segMaxDist == null
                    || seg.MaxDistanceBound > segMaxDist.MaxDistance)
                {
                    //-- Don't add interior segments, since their distance must be zero.
                    AddNonInterior(seg, priq);
                }

                if (segMaxDist == null
                    || seg.MaxDistance > segMaxDist.MaxDistance)
                {
                    segMaxDist = seg;
                }
            }
        }

        private class TargetDistance
        {
            private readonly IndexedFacetDistance _distanceToFacets;
            private readonly bool _isArea;
            private readonly IndexedPointInPolygonsLocator _ptInArea;

            public TargetDistance(Geometry geom)
            {
                _distanceToFacets = new IndexedFacetDistance(geom);
                _isArea = (int)geom.Dimension >= (int)Dimension.Surface;
                if (_isArea)
                {
                    _ptInArea = new IndexedPointInPolygonsLocator(geom);
                }
            }

            public GeometryLocation NearestLocation(Coordinate p)
            {
                return _distanceToFacets.NearestLocation(p);
            }

            public Coordinate NearestFacetPoint(Coordinate p)
            {
                return _distanceToFacets.NearestPoint(p);
            }

            public Coordinate NearestPoint(Coordinate p)
            {
                if (_ptInArea != null)
                {
                    if (_ptInArea.Locate(p) != Location.Exterior)
                    {
                        return p;
                    }
                }
                return _distanceToFacets.NearestPoint(p);
            }

            public bool IsInterior(Coordinate p)
            {
                if (!_isArea) return false;
                return _ptInArea.Locate(p) == Location.Interior;
            }

            public bool IsInterior(Coordinate p0, Coordinate p1)
            {
                if (!_isArea)
                    return false;
                //-- compute distance to B linework
                double segDist = _distanceToFacets.Distance(p0, p1);
                //-- if segment touches B linework it is not in interior
                if (segDist == 0)
                    return false;
                //-- only need to test one point to check interior
                return IsInterior(p0);
            }
        }

        private class DhdSegment : IComparable<DhdSegment>
        {
            public static DhdSegment Create(Coordinate p0, Coordinate p1, TargetDistance dist)
            {
                var seg = new DhdSegment(p0, p1);
                seg.Init(dist);
                return seg;
            }

            public static DhdSegment Create(DhdSegment prevSeg, Coordinate p1, TargetDistance dist)
            {
                var seg = new DhdSegment(prevSeg.P1, p1);
                seg.Init(prevSeg._nearPt1, dist);
                return seg;
            }

            public readonly Coordinate P0;
            public readonly Coordinate P1;
            private Coordinate _nearPt0;
            private Coordinate _nearPt1;
            private double _maxDistance;
            private double _maxDistanceBound = double.NegativeInfinity;

            private DhdSegment(Coordinate p0, Coordinate p1)
            {
                P0 = p0;
                P1 = p1;
            }

            private DhdSegment(Coordinate p0, Coordinate nearPt0, Coordinate p1, Coordinate nearPt1)
            {
                P0 = p0;
                _nearPt0 = nearPt0;
                P1 = p1;
                _nearPt1 = nearPt1;
                ComputeMaxDistances();
            }

            private void Init(TargetDistance dist)
            {
                _nearPt0 = dist.NearestPoint(P0);
                _nearPt1 = dist.NearestPoint(P1);
                ComputeMaxDistances();
            }

            private void Init(Coordinate nearest0, TargetDistance dist)
            {
                _nearPt0 = nearest0;
                _nearPt1 = dist.NearestPoint(P1);
                ComputeMaxDistances();
            }

            public Coordinate Endpoint(int index) => index == 0 ? P0 : P1;

            public double Length => P0.Distance(P1);

            public double MaxDistance => _maxDistance;

            public double MaxDistanceBound => _maxDistanceBound;

            public Coordinate[] GetMaxDistPts()
            {
                double dist0 = P0.Distance(_nearPt0);
                double dist1 = P1.Distance(_nearPt1);
                if (dist0 > dist1)
                    return new[] { P0.Copy(), _nearPt0.Copy() };
                return new[] { P1.Copy(), _nearPt1.Copy() };
            }

            /// <summary>
            /// Computes a least upper bound for the maximum distance to a segment.
            /// </summary>
            private void ComputeMaxDistances()
            {
                //-- Least upper bound is the max distance to the endpoints,
                //-- plus half segment length.
                double dist0 = P0.Distance(_nearPt0);
                double dist1 = P1.Distance(_nearPt1);
                _maxDistance = System.Math.Max(dist0, dist1);
                _maxDistanceBound = _maxDistance + Length / 2;
            }

            public DhdSegment[] Bisect(TargetDistance dist)
            {
                var mid = new Coordinate((P0.X + P1.X) / 2, (P0.Y + P1.Y) / 2);
                var nearPtMid = dist.NearestPoint(mid);
                return new[]
                {
                    new DhdSegment(P0, _nearPt0, mid, nearPtMid),
                    new DhdSegment(mid, nearPtMid, P1, _nearPt1),
                };
            }

            /// <summary>
            /// Inverts natural ordering so that <see cref="NetTopologySuite.Utilities.PriorityQueue{T}.Poll"/>
            /// returns the segment with the greatest <see cref="MaxDistanceBound"/> first.
            /// </summary>
            public int CompareTo(DhdSegment other)
            {
                if (other == null) return -1;
                return -_maxDistanceBound.CompareTo(other._maxDistanceBound);
            }
        }
    }
}
