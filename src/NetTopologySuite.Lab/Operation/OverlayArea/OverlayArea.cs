using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.KdTree;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.Mathematics;
using NetTopologySuite.Operation.OverlayArea;
using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Index;

namespace NetTopologySuite.Operation.OverlayArea
{
    /// <summary>
    /// Computes the area of the overlay of two polygons without forming
    /// the actual topology of the overlay.
    /// Since the topology is not needed, the computation is
    /// is insensitive to the fine details of the overlay topology,
    /// and hence is fully robust.
    /// It also allows for a simpler implementation with more aggressive
    /// performance optimization.
    /// <para/>
    /// The algorithm uses mathematics derived from the work of William R. Franklin.
    /// The area of a polygon can be computed as a sum of the partial areas
    /// computed for each {@link EdgeVector} of the polygon.
    /// This allows the area of the intersection of two polygons to be computed
    /// by summing the partial areas for the edge vectors of the intersection resultant.
    /// To determine the edge vectors all that is required
    /// is to compute the vertices of the intersection resultant,
    /// along with the direction (not the length) of the edges they belong to.
    /// The resultant vertices are the vertices where the edges of the inputs intersect,
    /// along with the vertices of each input which lie in the interior of the other input.
    /// The direction of the edge vectors is the same as the parent edges from which they derive.
    /// Determining the vertices of intersection is simpler and more robust
    /// than determining the values of the actual edge line segments in the overlay result.
    /// </summary>
    /// <author>Martin Davis</author>
    public class OverlayArea
    {

        public static double IntersectionArea(Geometry geom0, Geometry geom1)
        {
            if (!Interacts(geom0, geom1))
                return 0;
            var area = new OverlayArea(geom0);
            return area.IntersectionArea(geom1);
        }

        private static bool Interacts(Geometry geom0, Geometry geom1)
        {
            return geom0.EnvelopeInternal.Intersects(geom1.EnvelopeInternal);
        }

        private static readonly LineIntersector li = new RobustLineIntersector();

        private readonly Geometry _geom0;
        private readonly Envelope _geomEnv0;
        private readonly IndexedPointInAreaLocator _locator0;
        private readonly STRtree<LineSegment> _indexSegs;
        private readonly KdTree<object> _vertexIndex;

        public OverlayArea(Geometry geom)
        {
            _geom0 = geom;

            //TODO: handle holes and multipolygons
            if (!(_geom0 is Polygon polygon && (polygon.NumInteriorRings == 0)))
                throw new ArgumentException("Currently only Polygons with no holes supported");

            _geomEnv0 = geom.EnvelopeInternal;
            _locator0 = new IndexedPointInAreaLocator(geom);
            _indexSegs = BuildSegmentIndex(geom);
            _vertexIndex = BuildVertexIndex(geom);
        }

        private bool Interacts(Geometry geom)
        {
            return _geomEnv0.Intersects(geom.EnvelopeInternal);
        }

        public double IntersectionArea(Geometry geom)
        {
            //-- intersection area is 0 if geom does not interact with geom0
            if (!Interacts(geom)) return 0;

            var filter = new PolygonAreaFilter(this);
            geom.Apply(filter);
            return filter.Area;
        }

        private class PolygonAreaFilter : IGeometryFilter
        {
            private readonly OverlayArea _oa;
            double _area = 0;
            public PolygonAreaFilter(OverlayArea oa)
            {
                _oa = oa;
            }

            public double Area => _area;

            public void Filter(Geometry geom)
            {
                if (geom is Polygon polygon) {
                    _area += _oa.IntersectionAreaPolygon(polygon);
                }
            }
        }

        private double IntersectionAreaPolygon(Polygon geom)
        {
            //-- optimization - intersection area is 0 if geom does not interact with geom0
            if (!Interacts(geom)) return 0;

            double area = 0;
            area += IntersectionArea(geom.ExteriorRing);
            for (int i = 0; i < geom.NumInteriorRings; i++)
            {
                var hole = geom.GetInteriorRingN(i);
                // skip holes which do not interact
                if (Interacts(hole))
                {
                    area -= IntersectionArea(hole);
                }
            }
            return area;
        }

        private double IntersectionArea(LineString geom)
        {

            double areaInt = AreaForIntersections(geom);

            /*
             * If area for segment intersections is zero then no segments intersect.
             * This means that either the geometries are disjoint, 
             * OR one is inside the other.
             * This allows computing the area efficiently
             * using a simple inside/outside test
             */
            if (areaInt == 0.0)
            {
                return AreaContainedOrDisjoint(geom);
            }

            /*
             * The geometries intersect, so add areas for interior vertices
             */
            double areaVert1 = AreaForInteriorVertices(geom);

            var locator1 = new IndexedPointInAreaLocator(geom);
            double areaVert0 = AreaForInteriorVerticesIndexed(_geom0, _vertexIndex, geom.EnvelopeInternal, locator1);

            return (areaInt + areaVert1 + areaVert0) / 2;
        }

        /// <summary>
        /// Computes the area for the situation where the geometries are known to either
        /// be disjoint, or have one contained in the other.
        /// </summary>
        /// <param name="geom">The other geometry to intersect</param>
        /// <returns>The area of the contained geometry, or 0.0 if disjoint</returns>
        private double AreaContainedOrDisjoint(LineString geom)
        {
            double area0 = AreaForContainedGeom(geom, _geom0.EnvelopeInternal, _locator0);
            // if area is non-zero then geom is contained in geom0
            if (area0 != 0.0) return area0;

            // only checking one point, so non-indexed is faster
            var locator = new SimplePointInAreaLocator(geom);
            double area1 = AreaForContainedGeom(_geom0, geom.EnvelopeInternal, locator);
            // geom0 is either disjoint or contained - either way we are done
            return area1;
        }

        /// <summary>
        /// Tests and computes the area of a geometry contained in the other,
        /// or 0.0 if the geometry is disjoint.
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="env"></param>
        /// <param name="locator"></param>
        /// <returns>The area of the contained geometry, or 0 if it is disjoint</returns>
        private double AreaForContainedGeom(Geometry geom, Envelope env, IPointOnGeometryLocator locator)
        {
            var pt = geom.Coordinate;

            // fast check for disjoint
            if (!env.Covers(pt)) return 0.0;
            // full check for contained
            if (Location.Interior != locator.Locate(pt)) return 0.0;

            return Area(geom);
        }

        private static double Area(Geometry geom)
        {
            if (geom is LinearRing lr) {
                return Algorithm.Area.OfRing(lr.CoordinateSequence);
            }
            return geom.Area;
        }

        private double AreaForIntersections(LineString geom)
        {
            double area = 0.0;
            var seq = geom.CoordinateSequence;

            bool isCCW = Orientation.IsCCW(seq);

            // Compute rays for all intersections   
            for (int j = 0; j < seq.Count - 1; j++)
            {
                var b0 = seq.GetCoordinate(j);
                var b1 = seq.GetCoordinate(j + 1);
                if (isCCW)
                {
                    // flip segment orientation
                    var temp = b0; b0 = b1; b1 = temp;
                }

                var env = new Envelope(b0, b1);
                var intVisitor = new IntersectionVisitor(b0, b1);
                _indexSegs.Query(env, intVisitor);
                area += intVisitor.Area;
            }
            return area;
        }

        private class IntersectionVisitor : IItemVisitor<LineSegment>
        {
            double _area = 0.0;
            private readonly Coordinate _b0;
            private readonly Coordinate _b1;

            public IntersectionVisitor(Coordinate b0, Coordinate b1)
            {
                _b0 = b0;
                _b1 = b1;
            }

            public double Area => _area;

            public void VisitItem(LineSegment seg)
            {
                _area += AreaForIntersection(seg.P0, seg.P1, _b0, _b1);
            }
        }

        private static double AreaForIntersection(Coordinate a0, Coordinate a1, Coordinate b0, Coordinate b1)
        {
            // TODO: can the intersection computation be optimized?
            li.ComputeIntersection(a0, a1, b0, b1);
            if (!li.HasIntersection) return 0.0;

            /*
             * An intersection creates two edge vectors which contribute to the area.
             * 
             * With both rings oriented CW (effectively)
             * There are two situations for segment intersection:
             * 
             * 1) A entering B, B exiting A => rays are IP->A1:R, IP->B0:L
             * 2) A exiting B, B entering A => rays are IP->A0:L, IP->B1:R
             * (where IP is the intersection point, 
             * and  :L/R indicates result polygon interior is to the Left or Right).
             * 
             * For accuracy the full edge is used to provide the direction vector.
             */
            var intPt = li.GetIntersection(0);

            bool isAenteringB = OrientationIndex.CounterClockwise == Orientation.Index(a0, a1, b1);

            if (isAenteringB)
            {
                return EdgeVector.Area2Term(intPt, a0, a1, true)
                  + EdgeVector.Area2Term(intPt, b1, b0, false);
            }
            else
            {
                return EdgeVector.Area2Term(intPt, a1, a0, false)
                 + EdgeVector.Area2Term(intPt, b0, b1, true);
            }
        }

        private double AreaForInteriorVertices(LineString ring)
        {
            /*
             * Compute rays originating at vertices inside the intersection result
             * (i.e. A vertices inside B, and B vertices inside A)
             */
            double area = 0.0;
            var seq = ring.CoordinateSequence;
            bool isCW = !Orientation.IsCCW(seq);

            for (int i = 0; i < seq.Count - 1; i++)
            {
                var v = seq.GetCoordinate(i);
                // quick bounda check
                if (!_geomEnv0.Contains(v)) continue;
                // is this vertex in interior of intersection result?
                if (Location.Interior == _locator0.Locate(v))
                {
                    var vPrev = i == 0 ? seq.GetCoordinate(seq.Count - 2) : seq.GetCoordinate(i - 1);
                    var vNext = seq.GetCoordinate(i + 1);
                    area += EdgeVector.Area2Term(v, vPrev, !isCW)
                        + EdgeVector.Area2Term(v, vNext, isCW);
                }
            }
            return area;
        }

        private double AreaForInteriorVerticesIndexed(Geometry geom, KdTree<object> vertexIndex, Envelope env, IndexedPointInAreaLocator locator)
        {
            /*
             * Compute rays originating at vertices inside the intersection result
             * (i.e. A vertices inside B, and B vertices inside A)
             */
            double area = 0.0;
            var seq = GetVertices(geom);
            bool isCW = !Orientation.IsCCW(seq);

            var verts = vertexIndex.Query(env);
            foreach (var kdNode in verts) {
                int i = (int)kdNode.Data;
                var v = seq.GetCoordinate(i);
                // is this vertex in interior of intersection result?
                if (Location.Interior == locator.Locate(v))
                {
                    var vPrev = i == 0 ? seq.GetCoordinate(seq.Count - 2) : seq.GetCoordinate(i - 1);
                    var vNext = seq.GetCoordinate(i + 1);
                    area += EdgeVector.Area2Term(v, vPrev, !isCW)
                        + EdgeVector.Area2Term(v, vNext, isCW);
                }
            }
            return area;
        }

        private static CoordinateSequence GetVertices(Geometry geom)
        {
            var poly = (Polygon)geom;
            var seq = poly.ExteriorRing.CoordinateSequence;
            return seq;
        }

        private static STRtree<LineSegment> BuildSegmentIndex(Geometry geom)
        {
            var coords = geom.Coordinates;

            bool isCCW = Orientation.IsCCW(coords);
            var index = new STRtree<LineSegment>();
            for (int i = 0; i < coords.Length - 1; i++)
            {
                var a0 = coords[i];
                var a1 = coords[i + 1];
                var seg = isCCW ? new LineSegment(a1, a0) : new LineSegment(a0, a1);
                var env = new Envelope(a0, a1);
                index.Insert(env, seg);
            }
            return index;
        }

        private static KdTree<object> BuildVertexIndex(Geometry geom)
        {
            var coords = geom.Coordinates;
            var index = new KdTree<object>();
            //-- don't insert duplicate last vertex
            int[] ints = MathUtil.Shuffle(coords.Length - 1);
            //Arrays.sort(ints);
            foreach (int i in ints) {
                index.Insert(coords[i], i);
            }
            //System.out.println("Depth = " + index.depth() +  " size = " + index.size());
            return index;
        }
    }
}
