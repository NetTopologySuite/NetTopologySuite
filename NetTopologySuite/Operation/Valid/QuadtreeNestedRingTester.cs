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
        private Coordinate _nestedPt;

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
        public Coordinate NestedPoint
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
                Coordinate[] innerRingPts = innerRing.Coordinates;

                var results = _quadtree.Query(innerRing.EnvelopeInternal);
                for (int j = 0; j < results.Count; j++)
                {
                    ILinearRing searchRing = results[j];
                    Coordinate[] searchRingPts = searchRing.Coordinates;

                    if (innerRing == searchRing) continue;

                    if (!innerRing.EnvelopeInternal.Intersects(searchRing.EnvelopeInternal)) continue;

                    Coordinate innerRingPt = IsValidOp.FindPointNotNode(innerRingPts, searchRing, _graph);
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
                Envelope env = ring.EnvelopeInternal;
                _quadtree.Insert(env, ring);
            }
        }
    }
}
