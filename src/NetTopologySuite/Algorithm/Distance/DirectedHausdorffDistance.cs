using System;
using NetTopologySuite.Algorithm.Construct;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Distance;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Algorithm.Distance
{
    /// <summary>
    /// Computes the directed Hausdorff distance from a query geometry A
    /// to a target geometry B.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The directed Hausdorff distance is the maximum distance any point
    /// on A can be from B:
    /// </para>
    /// <code>
    /// h(A, B) = max_{a in A} min_{b in B} distance(a, b)
    /// </code>
    /// <para>
    /// It is asymmetric. The symmetric Hausdorff distance is
    /// <c>max(h(A, B), h(B, A))</c>.
    /// </para>
    /// <para>
    /// Empty operands yield <see cref="double.NaN"/> and a null realizing pair.
    /// A negative tolerance throws <see cref="ArgumentException"/>.
    /// Zero tolerance is allowed (zero-size input).
    /// </para>
    /// <para>
    /// This is the locus (continuous) algorithm from JTS 1.21 / #1182.
    /// Do not confuse it with <see cref="DiscreteHausdorffDistance"/>, which
    /// only samples vertices (optionally densified).
    /// </para>
    /// <para>
    /// The class-comment formula in JTS that writes <c>max_a (max_b ...)</c>
    /// is farthest-pair; the algorithm implemented here is max-min.
    /// <see cref="FarthestPoints(Geometry)"/> keeps the JTS name but returns
    /// the max-min realizing pair.
    /// </para>
    /// <para>
    /// Mixed-dimension collections follow JTS: only
    /// <c>Dimension == Point</c> routes to the point walker.
    /// A collection of points and lines drops the points
    /// (JTS TODO: handle mixed geoms with points).
    /// </para>
    /// </remarks>
    /// <author>Martin Davis</author>
    public sealed class DirectedHausdorffDistance
    {
        private const double EmptyDistance = double.NaN;

        /// <summary>Heuristic automatic tolerance factor.</summary>
        private const double AutoToleranceFactor = 1.0e4;

        /// <summary>
        /// Largest-empty-circle is slower than the boundary walk;
        /// a coarser tolerance is used for area-interior farthest points.
        /// </summary>
        private const double AreaInteriorToleranceFactor = 20;

        /// <summary>
        /// Larger factor for <see cref="IsFullyWithinDistance(Geometry, double)"/>.
        /// The operation usually short-circuits, so the cost is low.
        /// </summary>
        private const double FullyWithinToleranceFactor = 10 * AutoToleranceFactor;

        private readonly Geometry _target;
        private readonly TargetDistance _targetDistance;

        /// <summary>
        /// Computes the directed Hausdorff distance of query <paramref name="a"/>
        /// from target <paramref name="b"/>.
        /// </summary>
        /// <returns>The directed Hausdorff distance, or NaN if an input is empty.</returns>
        public static double Distance(Geometry a, Geometry b)
        {
            var hd = new DirectedHausdorffDistance(b);
            return PairDistance(hd.FarthestPoints(a));
        }

        /// <summary>
        /// Computes the directed Hausdorff distance up to a given accuracy.
        /// </summary>
        public static double Distance(Geometry a, Geometry b, double tolerance)
        {
            var hd = new DirectedHausdorffDistance(b);
            return PairDistance(hd.FarthestPoints(a, tolerance));
        }

        /// <summary>
        /// Computes a pair of points [ptA, ptB] attaining the directed Hausdorff distance.
        /// </summary>
        /// <returns>The realizing pair, or <c>null</c> if an input is empty.</returns>
        public static Coordinate[] DistancePoints(Geometry a, Geometry b)
        {
            var dhd = new DirectedHausdorffDistance(b);
            return dhd.FarthestPoints(a);
        }

        /// <summary>
        /// Computes a realizing pair up to a given accuracy.
        /// </summary>
        public static Coordinate[] DistancePoints(Geometry a, Geometry b, double tolerance)
        {
            var dhd = new DirectedHausdorffDistance(b);
            return dhd.FarthestPoints(a, tolerance);
        }

        /// <summary>
        /// Computes a pair of points attaining the symmetric Hausdorff distance.
        /// Points are returned in A–B order.
        /// </summary>
        public static Coordinate[] HausdorffDistancePoints(Geometry a, Geometry b)
        {
            var hdAB = new DirectedHausdorffDistance(b);
            var ptsAB = hdAB.FarthestPoints(a);
            var hdBA = new DirectedHausdorffDistance(a);
            var ptsBA = hdBA.FarthestPoints(b);

            var pts = ptsAB;
            if (PairDistance(ptsBA) > PairDistance(ptsAB))
                pts = Pair(ptsBA[1], ptsBA[0]);
            return pts;
        }

        /// <summary>
        /// Computes the symmetric Hausdorff distance
        /// <c>max(h(A, B), h(B, A))</c>.
        /// </summary>
        public static double HausdorffDistance(Geometry a, Geometry b)
        {
            return PairDistance(HausdorffDistancePoints(a, b));
        }

        /// <summary>
        /// Tests whether query <paramref name="a"/> lies fully within
        /// <paramref name="maxDistance"/> of target <paramref name="b"/>.
        /// </summary>
        public static bool IsFullyWithinDistance(Geometry a, Geometry b, double maxDistance)
        {
            var hd = new DirectedHausdorffDistance(b);
            return hd.IsFullyWithinDistance(a, maxDistance);
        }

        /// <summary>
        /// Tests full containment within a distance, up to a given accuracy.
        /// </summary>
        public static bool IsFullyWithinDistance(Geometry a, Geometry b, double maxDistance, double tolerance)
        {
            var hd = new DirectedHausdorffDistance(b);
            return hd.IsFullyWithinDistance(a, maxDistance, tolerance);
        }

        /// <summary>
        /// Creates a prepared instance that indexes the target once.
        /// </summary>
        /// <param name="geom">The geometry to compute the distance from.</param>
        public DirectedHausdorffDistance(Geometry geom)
        {
            _target = geom;
            _targetDistance = new TargetDistance(geom);
        }

        /// <summary>
        /// Tests whether <paramref name="geom"/> lies fully within
        /// <paramref name="maxDistance"/> of the prepared target.
        /// </summary>
        public bool IsFullyWithinDistance(Geometry geom, double maxDistance)
        {
            double tolerance = maxDistance / FullyWithinToleranceFactor;
            return IsFullyWithinDistance(geom, maxDistance, tolerance);
        }

        /// <summary>
        /// Tests full containment within a distance, up to a given accuracy.
        /// Empty operands are not within any finite distance.
        /// </summary>
        public bool IsFullyWithinDistance(Geometry geom, double maxDistance, double tolerance)
        {
            if (geom.IsEmpty || _target.IsEmpty)
                return false;
            if (IsBeyond(geom.EnvelopeInternal, _target.EnvelopeInternal, maxDistance))
                return false;

            var maxDistCoords = ComputeDistancePoints(geom, tolerance, maxDistance);
            if (maxDistCoords == null)
                return false;
            return PairDistance(maxDistCoords) <= maxDistance;
        }

        /// <summary>
        /// Computes a pair of points attaining the directed Hausdorff distance
        /// from <paramref name="geom"/> to the prepared target.
        /// </summary>
        public Coordinate[] FarthestPoints(Geometry geom)
        {
            return FarthestPoints(geom, ComputeTolerance(geom));
        }

        /// <summary>
        /// Computes a realizing pair up to a given accuracy.
        /// </summary>
        public Coordinate[] FarthestPoints(Geometry geom, double tolerance)
        {
            return ComputeDistancePoints(geom, tolerance, -1.0);
        }

        private static double PairDistance(Coordinate[] pts)
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

        /// <summary>
        /// Tests whether envelope A must have a side farther than
        /// <paramref name="maxDistance"/> from envelope B.
        /// Null (empty) envelopes are not beyond.
        /// </summary>
        private static bool IsBeyond(Envelope envA, Envelope envB, double maxDistance)
        {
            if (envA.IsNull || envB.IsNull)
                return false;
            return envA.MinX < envB.MinX - maxDistance
                || envA.MinY < envB.MinY - maxDistance
                || envA.MaxX > envB.MaxX + maxDistance
                || envA.MaxY > envB.MaxY + maxDistance;
        }

        private static bool IsValidLimit(double limit) => limit >= 0.0;

        private static bool IsBeyondLimit(double maxDist, double maxDistanceLimit)
            => maxDistanceLimit >= 0 && maxDist > maxDistanceLimit;

        private static bool IsWithinLimit(double maxDist, double maxDistanceLimit)
            => maxDistanceLimit >= 0 && maxDist <= maxDistanceLimit;

        private Coordinate[] ComputeDistancePoints(Geometry geom, double tolerance, double maxDistanceLimit)
        {
            if (tolerance < 0.0)
                throw new ArgumentException("Tolerance must be non-negative", nameof(tolerance));

            if (geom.IsEmpty || _target.IsEmpty)
                return null;

            if (geom.Dimension == Dimension.P)
                return ComputeForPoints(geom, maxDistanceLimit);

            // TODO: handle mixed geoms with points (JTS)
            var maxDistPtsEdge = ComputeForEdges(geom, tolerance, maxDistanceLimit);

            if (IsBeyondLimit(PairDistance(maxDistPtsEdge), maxDistanceLimit))
                return maxDistPtsEdge;

            if (geom.Dimension == Dimension.A)
            {
                var maxDistPtsInterior = ComputeForAreaInterior(geom, tolerance);
                if (maxDistPtsInterior != null
                    && PairDistance(maxDistPtsInterior) > PairDistance(maxDistPtsEdge))
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
            foreach (var geomElem in new GeometryCollectionEnumerator(geom))
            {
                if (!(geomElem is Point))
                    continue;

                var pA = geomElem.Coordinate;
                var pB = _targetDistance.NearestPoint(pA);
                double dist = pA.Distance(pB);

                bool isInterior = dist > 0 && _targetDistance.IsInterior(pA);
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
                if (IsValidLimit(maxDistanceLimit) && IsBeyondLimit(maxDist, maxDistanceLimit))
                    break;
            }
            return maxDistPtsAB;
        }

        private Coordinate[] ComputeForEdges(Geometry geom, double tolerance, double maxDistanceLimit)
        {
            var segQueue = CreateSegQueue(geom);

            DhdSegment segMaxDist = null;
            while (!segQueue.IsEmpty())
            {
                var segMaxBound = segQueue.Poll();
                if (segMaxDist == null || segMaxBound.MaxDistance > segMaxDist.MaxDistance)
                    segMaxDist = segMaxBound;

                if (segMaxBound.MaxDistanceBound <= segMaxDist.MaxDistance)
                    break;

                if (IsValidLimit(maxDistanceLimit))
                {
                    if (IsWithinLimit(segMaxBound.MaxDistanceBound, maxDistanceLimit)
                        || IsBeyondLimit(segMaxBound.MaxDistance, maxDistanceLimit))
                    {
                        break;
                    }
                }

                if (segMaxBound.MaxDistance == 0.0 && IsSameOrCollinear(segMaxBound))
                    continue;

                if (tolerance > 0 && segMaxBound.Length > tolerance)
                {
                    var bisects = segMaxBound.Bisect(_targetDistance);
                    AddNonInterior(bisects[0], segQueue);
                    AddNonInterior(bisects[1], segQueue);
                }
            }

            if (segMaxDist != null)
                return segMaxDist.GetMaxDistPts();

            var maxPt = geom.Coordinate;
            return Pair(maxPt, maxPt);
        }

        private bool IsSameOrCollinear(DhdSegment seg)
        {
            var f0 = _targetDistance.NearestLocation(seg.P0);
            var f1 = _targetDistance.NearestLocation(seg.P1);
            return f0.IsSameSegment(f1);
        }

        private void AddNonInterior(DhdSegment segment, PriorityQueue<DhdSegment> segQueue)
        {
            if (IsInterior(segment))
                return;
            segQueue.Add(segment);
        }

        private bool IsInterior(DhdSegment segment)
        {
            if (segment.MaxDistance > 0.0)
                return false;
            return _targetDistance.IsInterior(segment.GetEndpoint(0), segment.GetEndpoint(1));
        }

        private Coordinate[] ComputeForAreaInterior(Geometry geom, double tolerance)
        {
            if (tolerance <= 0.0)
                return null;

            var polygonal = geom;
            if (polygonal.EnvelopeInternal.Disjoint(_target.EnvelopeInternal))
                return null;

            var centerPt = LargestEmptyCircle.GetCenter(_target, polygonal, tolerance * AreaInteriorToleranceFactor);
            var ptA = centerPt.Coordinate;
            if (_targetDistance.IsInterior(ptA))
                return null;
            var ptB = _targetDistance.NearestFacetPoint(ptA);
            return Pair(ptA, ptB);
        }

        private PriorityQueue<DhdSegment> CreateSegQueue(Geometry geom)
        {
            var priq = new PriorityQueue<DhdSegment>();
            geom.Apply(new SegmentCollector(this, priq));
            return priq;
        }

        private void AddSegments(Coordinate[] pts, PriorityQueue<DhdSegment> priq)
        {
            DhdSegment segMaxDist = null;
            DhdSegment prevSeg = null;
            for (int i = 0; i < pts.Length - 1; i++)
            {
                var seg = i == 0
                    ? DhdSegment.Create(pts[i], pts[i + 1], _targetDistance)
                    : DhdSegment.Create(prevSeg, pts[i + 1], _targetDistance);
                prevSeg = seg;

                if (segMaxDist == null || seg.MaxDistanceBound > segMaxDist.MaxDistance)
                    AddNonInterior(seg, priq);

                if (segMaxDist == null || seg.MaxDistance > segMaxDist.MaxDistance)
                    segMaxDist = seg;
            }
        }

        private sealed class SegmentCollector : IGeometryComponentFilter
        {
            private readonly DirectedHausdorffDistance _owner;
            private readonly PriorityQueue<DhdSegment> _priq;

            public SegmentCollector(DirectedHausdorffDistance owner, PriorityQueue<DhdSegment> priq)
            {
                _owner = owner;
                _priq = priq;
            }

            public void Filter(Geometry geom)
            {
                if (geom is LineString)
                    _owner.AddSegments(geom.Coordinates, _priq);
            }
        }

        private sealed class TargetDistance
        {
            private readonly IndexedFacetDistance _distanceToFacets;
            private readonly bool _isArea;
            private readonly IndexedPointInPolygonsLocator _ptInArea;

            public TargetDistance(Geometry geom)
            {
                _distanceToFacets = new IndexedFacetDistance(geom);
                _isArea = geom.Dimension >= Dimension.A;
                if (_isArea)
                    _ptInArea = new IndexedPointInPolygonsLocator(geom);
            }

            public CoordinateSequenceLocation NearestLocation(Coordinate p)
                => _distanceToFacets.NearestLocation(p);

            public Coordinate NearestFacetPoint(Coordinate p)
                => _distanceToFacets.NearestPoint(p);

            public Coordinate NearestPoint(Coordinate p)
            {
                if (_ptInArea != null && _ptInArea.Locate(p) != Location.Exterior)
                    return p;
                return _distanceToFacets.NearestPoint(p);
            }

            public bool IsInterior(Coordinate p)
            {
                if (!_isArea)
                    return false;
                return _ptInArea.Locate(p) == Location.Interior;
            }

            public bool IsInterior(Coordinate p0, Coordinate p1)
            {
                if (!_isArea)
                    return false;
                double segDist = _distanceToFacets.Distance(p0, p1);
                if (segDist == 0)
                    return false;
                return IsInterior(p0);
            }
        }

        private sealed class DhdSegment : IComparable<DhdSegment>
        {
            internal readonly Coordinate P0;
            internal readonly Coordinate P1;
            private Coordinate _nearPt0;
            private Coordinate _nearPt1;
            private double _maxDistanceBound = double.NegativeInfinity;
            private double _maxDistance;

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

            public Coordinate GetEndpoint(int index) => index == 0 ? P0 : P1;

            public double Length => P0.Distance(P1);

            public double MaxDistance => _maxDistance;

            public double MaxDistanceBound => _maxDistanceBound;

            public Coordinate[] GetMaxDistPts()
            {
                double dist0 = P0.Distance(_nearPt0);
                double dist1 = P1.Distance(_nearPt1);
                return dist0 > dist1 ? Pair(P0, _nearPt0) : Pair(P1, _nearPt1);
            }

            private void ComputeMaxDistances()
            {
                double dist0 = P0.Distance(_nearPt0);
                double dist1 = P1.Distance(_nearPt1);
                _maxDistance = Math.Max(dist0, dist1);
                _maxDistanceBound = _maxDistance + Length / 2;
            }

            public DhdSegment[] Bisect(TargetDistance dist)
            {
                var mid = new Coordinate((P0.X + P1.X) / 2, (P0.Y + P1.Y) / 2);
                var nearPtMid = dist.NearestPoint(mid);
                return new[]
                {
                    new DhdSegment(P0, _nearPt0, mid, nearPtMid),
                    new DhdSegment(mid, nearPtMid, P1, _nearPt1)
                };
            }

            /// <summary>
            /// Invert so <see cref="PriorityQueue{T}.Poll"/> returns the largest bound.
            /// </summary>
            public int CompareTo(DhdSegment other)
                => -_maxDistanceBound.CompareTo(other._maxDistanceBound);

            public override string ToString() => WKTWriter.ToLineString(P0, P1);
        }
    }
}
