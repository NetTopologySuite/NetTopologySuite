using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Quadtree;
using NetTopologySuite.Utilities;
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
        private readonly Envelope _totalEnv = new Envelope();
        private ISpatialIndex<ILinearRing> _quadtree;
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
        public Coordinate NestedPoint { get; private set; }
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
            for (var i = 0; i < _rings.Count; i++)
            {
                var innerRing = _rings[i];
                var innerRingPts = innerRing.Coordinates;
                var results = _quadtree.Query(innerRing.EnvelopeInternal);
                for (var j = 0; j < results.Count; j++)
                {
                    var searchRing = results[j];
                    var searchRingPts = searchRing.Coordinates;
                    if (innerRing == searchRing) continue;
                    if (!innerRing.EnvelopeInternal.Intersects(searchRing.EnvelopeInternal)) continue;
                    var innerRingPt = IsValidOp.FindPointNotNode(innerRingPts, searchRing, _graph);
                    Assert.IsTrue(innerRingPt != null, "Unable to find a ring point not a node of the search ring");
                    var isInside = PointLocation.IsInRing(innerRingPt, searchRingPts);
                    if (isInside)
                    {
                        NestedPoint = innerRingPt;
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
            for (var i = 0; i < _rings.Count; i++)
            {
                var ring = _rings[i];
                var env = ring.EnvelopeInternal;
                _quadtree.Insert(env, ring);
            }
        }
    }
}
