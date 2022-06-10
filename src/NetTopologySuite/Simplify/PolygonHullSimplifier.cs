using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Mathematics;
using System;
using System.Collections.Generic;

namespace NetTopologySuite.Simplify
{
    /// <summary>
    /// Computes topology-preserving simplified hulls of polygonal geometry.
    /// Both outer and inner hulls can be computed.
    /// Outer hulls contain the input geometry and are larger in area.
    /// Inner hulls are contained by the input geometry and are smaller in area.
    /// In both the hull vertices are a subset of the input vertices.
    /// The hull construction attempts to minimize the area difference
    /// with the input geometry.
    /// <para/>
    /// Hulls are generally concave if the input is.
    /// Computed hulls are topology-preserving: 
    /// they do not contain any self-intersections or overlaps, 
    /// so the result polygonal geometry is valid.
    /// <para/>
    /// Polygons with holes and MultiPolygons are supported.
    /// The result has the same geometric type and structure as the input.
    /// <para/>
    /// The number of vertices in the computed hull is determined by a target parameter.
    /// Two parameters are supported:
    /// <list type="bullet">
    /// <item><term>Vertex Number fraction</term><description>the fraction of the input vertices retained in the result.
    /// Value 1 produces the original geometry.
    /// Smaller values produce less concave results.
    /// For outer hulls, value 0 produces the convex hull (with triangles for any holes).
    /// For inner hulls, value 0 produces a triangle (if no holes are present).</description></item>
    /// <item><term>Area Delta ratio</term><description>
    /// the ratio of the change in area to the input area.Value 0 produces the original geometry.
    /// Larger values produce less concave results.</description></item>
    /// </list>
    /// The algorithm ensures that the result does not cause the target parameter
    /// to be exceeded. This allows computing outer or inner hulls
    /// with a small area delta ratio as an effective way of removing
    /// narrow gores and spikes.
    /// </summary>
    /// <author>Martin Davis</author>
    public class PolygonHullSimplifier
    {
        /// <summary>
        /// Computes topology-preserving simplified hull of a polygonal geometry,
        /// with hull shape determined by a target parameter
        /// specifying the fraction of the input vertices retained in the result.
        /// Larger values compute less concave results.
        /// A value of 1 produces the convex hull; a value of 0 produces the original geometry.
        /// Either outer or inner hulls can be computed.
        /// </summary>
        /// <param name="geom">The polygonal geometry to process</param>
        /// <param name="isOuter">A flag indicating whether to compute an outer or inner hull</param>
        /// <param name="vertexNumFraction">The target fraction of number of input vertices in result</param>
        /// <returns>The hull geometry</returns>
        public static Geometry Hull(Geometry geom, bool isOuter, double vertexNumFraction)
        {
            var hull = new PolygonHullSimplifier(geom, isOuter);
            hull.VertexNumFraction = Math.Abs(vertexNumFraction);
            return hull.GetResult();
        }

        /// <summary>Computes a boundary-respecting hull of a polygonal geometry,
        /// with hull shape determined by a target parameter
        /// specifying the ratio of maximum difference in area to original area.
        /// Larger values compute less concave results.
        /// A value of 0 produces the original geometry.
        /// Either outer or inner hulls can be computed.
        /// </summary>
        /// <param name="geom">The polygonal geometry to process</param>
        /// <param name="isOuter">A flag indicating whether to compute an outer or inner hull</param>
        /// <param name="areaDeltaRatio">The target ratio of area difference to original area</param>
        /// <returns>The hull geometry</returns>
        public static Geometry HullByAreaDelta(Geometry geom, bool isOuter, double areaDeltaRatio)
        {
            var hull = new PolygonHullSimplifier(geom, isOuter);
            hull.AreaDeltaRatio = Math.Abs(areaDeltaRatio);
            return hull.GetResult();
        }

        private readonly Geometry _inputGeom;
        private readonly bool _isOuter;
        private double _vertexNumFraction = -1;
        private double _areaDeltaRatio = -1;
        private readonly GeometryFactory _geomFactory;

        /// <summary>
        /// Creates a new instance
        /// to compute a simplified hull of a polygonal geometry.
        /// An outer or inner hull is computed
        /// depending on the value of <paramref name="isOuter"/>. 
        /// </summary>
        /// <param name="inputGeom">The polygonal geometry to process</param>
        /// <param name="isOuter">Indicates whether to compute an outer or inner hull</param>
        public PolygonHullSimplifier(Geometry inputGeom, bool isOuter)
        {
            _inputGeom = inputGeom;
            _geomFactory = inputGeom.Factory;
            _isOuter = isOuter;
            if (!(inputGeom is IPolygonal)) {
                throw new ArgumentException("Input geometry must be  polygonal");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the target fraction of input vertices
        /// which are retained in the result.
        /// The value should be in the range [0,1].
        /// </summary>
        public double VertexNumFraction
        {
            get => _vertexNumFraction;
            set => _vertexNumFraction = MathUtil.Clamp(value, 0, 1);
        }

        /// <summary>
        /// Gets or sets a value indicating the target maximum ratio of the change in area of the result to the input area.
        /// The value must be 0 or greater.
        /// </summary>
        public double AreaDeltaRatio
        {
            get => _areaDeltaRatio;
            set => _areaDeltaRatio = MathUtil.Clamp(value, 0, double.PositiveInfinity);
        }

        /// <summary>
        /// Gets the result polygonal hull geometry.
        /// </summary>
        /// <returns>The polygonal geometry for the hull</returns>
        public Geometry GetResult()
        {
            //-- handle trivial parameter values
            if (_vertexNumFraction == 1 || _areaDeltaRatio == 0)
            {
                return _inputGeom.Copy();
            }

            if (_inputGeom is MultiPolygon) {
                /*
                 * Only outer hulls where there is more than one polygon
                 * can potentially overlap.
                 * Shell outer hulls could overlap adjacent shell hulls 
                 * or hole hulls surrounding them; 
                 * hole outer hulls could overlap contained shell hulls.
                 */
                bool isOverlapPossible = _isOuter && _inputGeom.NumGeometries > 1;
                if (isOverlapPossible)
                {
                    return ComputeMultiPolygonAll((MultiPolygon)_inputGeom);
                }
                else
                {
                    return ComputeMultiPolygonEach((MultiPolygon)_inputGeom);
                }
            }
            else if (_inputGeom is Polygon) {
                return ComputePolygon((Polygon)_inputGeom);
            }
            throw new ArgumentException("Input geometry must be polygonal");
        }

        /// <summary>
        /// Computes hulls for MultiPolygon elements for
        /// the cases where hulls might overlap.
        /// </summary>
        /// <param name="multiPoly">The MultiPolygon to process</param>
        /// <returns>The hull geometry</returns>
        private Geometry ComputeMultiPolygonAll(MultiPolygon multiPoly)
        {
            var hullIndex = new RingHullIndex();
            int nPoly = multiPoly.NumGeometries;
            var polyHulls = new List<RingHull>[nPoly];

            //TODO: investigate if reordering input elements improves result

            //-- prepare element polygon hulls and index
            for (int i = 0; i < multiPoly.NumGeometries; i++)
            {
                var poly = (Polygon)multiPoly.GetGeometryN(i);
                var ringHulls = InitPolygon(poly, hullIndex);
                polyHulls[i] = ringHulls;
            }

            //-- compute hull polygons
            var polys = new List<Polygon>(nPoly);
            for (int i = 0; i < multiPoly.NumGeometries; i++)
            {
                var poly = (Polygon)multiPoly.GetGeometryN(i);
                var hull = CreatePolygonHull(poly, polyHulls[i], hullIndex);
                polys.Add(hull);
            }
            return _geomFactory.CreateMultiPolygon(polys.ToArray());
        }

        private Geometry ComputeMultiPolygonEach(MultiPolygon multiPoly)
        {
            var polys = new List<Polygon>();
            for (int i = 0; i < multiPoly.NumGeometries; i++)
            {
                var poly = (Polygon)multiPoly.GetGeometryN(i);
                var hull = ComputePolygon(poly);
                polys.Add(hull);
            }
            return _geomFactory.CreateMultiPolygon(polys.ToArray());
        }

        private Polygon ComputePolygon(Polygon poly)
        {
            RingHullIndex hullIndex = null;
            /*
             * For a single polygon overlaps are only possible for inner hulls
             * and where holes are present.
             */
            bool isOverlapPossible = !_isOuter && poly.NumInteriorRings > 0;
            if (isOverlapPossible) hullIndex = new RingHullIndex();
            var hulls = InitPolygon(poly, hullIndex);
            var hull = CreatePolygonHull(poly, hulls, hullIndex);
            return hull;
        }

        /// <summary>
        /// Create all ring hulls for the rings of a polygon,
        /// so that all are in the hull index if required.
        /// </summary>
        /// <param name="poly" >The polygon being processed</param>
        /// <param name="hullIndex">The hull index if present, or <c>null</c></param>
        /// <returns>A list of ring hulls</returns>
        private List<RingHull> InitPolygon(Polygon poly, RingHullIndex hullIndex)
        {
            var hulls = new List<RingHull>();
            if (poly.IsEmpty)
                return hulls;

            double areaTotal = 0.0;
            if (_areaDeltaRatio >= 0)
            {
                areaTotal = RingArea(poly);
            }
            hulls.Add(CreateRingHull((LinearRing)poly.ExteriorRing, _isOuter, areaTotal, hullIndex));
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                //Assert: interior ring is not empty
                hulls.Add(CreateRingHull((LinearRing)poly.GetInteriorRingN(i), !_isOuter, areaTotal, hullIndex));
            }
            return hulls;
        }

        private static double RingArea(Polygon poly)
        {
            double area = Area.OfRing(poly.ExteriorRing.CoordinateSequence);
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                area += Area.OfRing(poly.GetInteriorRingN(i).CoordinateSequence);
            }
            return area;
        }

        private RingHull CreateRingHull(LinearRing ring, bool isOuter, double areaTotal, RingHullIndex hullIndex)
        {
            var ringHull = new RingHull(ring, isOuter);
            if (_vertexNumFraction >= 0)
            {
                int targetVertexCount = (int)Math.Ceiling(_vertexNumFraction * (ring.NumPoints - 1));
                ringHull.MinVertexNum = targetVertexCount;
            }
            else if (_areaDeltaRatio >= 0)
            {
                double ringArea = Area.OfRing(ring.CoordinateSequence);
                double ringWeight = ringArea / areaTotal;
                double maxAreaDelta = ringWeight * _areaDeltaRatio * ringArea;
                ringHull.MaxAreaDelta = maxAreaDelta;
            }
            if (hullIndex != null) hullIndex.Add(ringHull);
            return ringHull;
        }

        private Polygon CreatePolygonHull(Polygon poly, List<RingHull> ringHulls, RingHullIndex hullIndex)
        {
            if (poly.IsEmpty)
                return _geomFactory.CreatePolygon();

            int ringIndex = 0;
            var shellHull = ringHulls[ringIndex++].GetHull(hullIndex);
            var holeHulls = new List<LinearRing>();
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                var hull = ringHulls[ringIndex++].GetHull(hullIndex);
                //TODO: handle empty
                holeHulls.Add(hull);
            }
            return _geomFactory.CreatePolygon(shellHull, holeHulls.ToArray());
        }
    }

}
