using System.Collections.Generic;
#if useFullGeoAPI
using GeoAPI.Geometries;
#else
using ICoordinate = NetTopologySuite.Geometries.Coordinate;
using IEnvelope = NetTopologySuite.Geometries.Envelope;
using IGeometry = NetTopologySuite.Geometries.Geometry;
using IPoint = NetTopologySuite.Geometries.Point;
using ILineString = NetTopologySuite.Geometries.LineString;
using ILinearRing = NetTopologySuite.Geometries.LinearRing;
using IPolygon = NetTopologySuite.Geometries.Polygon;
using IGeometryCollection = NetTopologySuite.Geometries.GeometryCollection;
using IMultiPoint = NetTopologySuite.Geometries.MultiPoint;
using IMultiLineString = NetTopologySuite.Geometries.MultiLineString;
using IMultiPolygon = NetTopologySuite.Geometries.MultiPolygon;
using IGeometryFactory = NetTopologySuite.Geometries.GeometryFactory;
using IPrecisionModel = NetTopologySuite.Geometries.PrecisionModel;
#endif
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Quadtree;
using NetTopologySuite.Utilities;
#if SILVERLIGHT
using ArrayList = System.Collections.Generic.List<object>;
#endif

namespace NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// Tests whether any of a set of <c>LinearRing</c>s are
    /// nested inside another ring in the set, using a <c>Quadtree</c>
    /// index to speed up the comparisons.
    /// </summary>
    public class QuadtreeNestedRingTester
    {
        private readonly GeometryGraph _graph;  // used to find non-node vertices
        private readonly IList<ILinearRing> _rings = new List<ILinearRing>();
        private readonly IEnvelope _totalEnv = new Envelope();
        private ISpatialIndex<ILinearRing> _quadtree;
        private ICoordinate _nestedPt;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph"></param>
        public QuadtreeNestedRingTester(GeometryGraph graph)
        {
            _graph = graph;
        }

        /// <summary>
        /// 
        /// </summary>
        public ICoordinate NestedPoint
        {
            get
            {
                return _nestedPt;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ring"></param>
        public void Add(ILinearRing ring)
        {
            _rings.Add(ring);
            _totalEnv.ExpandToInclude(ring.EnvelopeInternal);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsNonNested()
        {
            BuildQuadtree();

            for (int i = 0; i < _rings.Count; i++)
            {
                ILinearRing innerRing = _rings[i];
                ICoordinate[] innerRingPts = innerRing.Coordinates;

                var results = _quadtree.Query(innerRing.EnvelopeInternal);
                for (int j = 0; j < results.Count; j++)
                {
                    ILinearRing searchRing = results[j];
                    ICoordinate[] searchRingPts = searchRing.Coordinates;

                    if (innerRing == searchRing) continue;

                    if (!innerRing.EnvelopeInternal.Intersects(searchRing.EnvelopeInternal)) continue;

                    ICoordinate innerRingPt = IsValidOp.FindPointNotNode(innerRingPts, searchRing, _graph);
                    Assert.IsTrue(innerRingPt != null, "Unable to find a ring point not a node of the search ring");

                    bool isInside = CGAlgorithms.IsPointInRing(innerRingPt, searchRingPts);
                    if (isInside)
                    {
                        _nestedPt = innerRingPt;
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        private void BuildQuadtree()
        {
            _quadtree = new Quadtree<ILinearRing>();

            for (int i = 0; i < _rings.Count; i++)
            {
                ILinearRing ring = _rings[i];
                Envelope env = (Envelope) ring.EnvelopeInternal;
                _quadtree.Insert(env, ring);
            }
        }
    }
}
