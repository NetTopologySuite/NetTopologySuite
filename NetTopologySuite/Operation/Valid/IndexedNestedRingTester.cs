using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Operation.Valid
{
    /**
     * Tests whether any of a set of {@link LinearRing}s are
     * nested inside another ring in the set, using a spatial
     * index to speed up the comparisons.
     *
     * @version 1.7
     */
    public class IndexedNestedRingTester
    {
        private readonly GeometryGraph _graph;  // used to find non-node vertices
        private readonly IList<ILineString> _rings = new List<ILineString>();
        private readonly Envelope _totalEnv = new Envelope();
        private ISpatialIndex<ILineString> _index;
        private Coordinate _nestedPt;

        public IndexedNestedRingTester(GeometryGraph graph)
        {
            _graph = graph;
        }

        public Coordinate NestedPoint => _nestedPt;

        public void Add(ILinearRing ring)
        {
            _rings.Add(ring);
            _totalEnv.ExpandToInclude(ring.EnvelopeInternal);
        }

        public bool IsNonNested()
        {
            BuildIndex();

            for (int i = 0; i < _rings.Count; i++)
            {
                var innerRing = (ILinearRing)_rings[i];
                var innerRingPts = innerRing.Coordinates;

                var results = _index.Query(innerRing.EnvelopeInternal);
                for (int j = 0; j < results.Count; j++)
                {
                    var searchRing = (ILinearRing)results[j];
                    var searchRingPts = searchRing.Coordinates;

                    if (innerRing == searchRing)
                        continue;

                    if (!innerRing.EnvelopeInternal.Intersects(searchRing.EnvelopeInternal))
                        continue;

                    var innerRingPt = IsValidOp.FindPointNotNode(innerRingPts, searchRing, _graph);
                    // Diego Guidi: removed => see Issue 121
                    //Assert.IsTrue(innerRingPt != null, "Unable to find a ring point not a node of the search ring");
                    /**
                     * If no non-node pts can be found, this means
                     * that the searchRing touches ALL of the innerRing vertices.
                     * This indicates an invalid polygon, since either
                     * the two holes create a disconnected interior,
                     * or they touch in an infinite number of points
                     * (i.e. along a line segment).
                     * Both of these cases are caught by other tests,
                     * so it is safe to simply skip this situation here.
                     */
                    if (innerRingPt == null)
                        continue;

                    bool isInside = PointLocation.IsInRing(innerRingPt, searchRingPts);
                    if (isInside)
                    {
                        _nestedPt = innerRingPt;
                        return false;
                    }
                }
            }
            return true;
        }

        private void BuildIndex()
        {
            _index = new STRtree<ILineString>();

            for (int i = 0; i < _rings.Count; i++)
            {
                var ring = (ILinearRing)_rings[i];
                var env = ring.EnvelopeInternal;
                _index.Insert(env, ring);
            }
        }
    }
}