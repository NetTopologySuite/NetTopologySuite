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
        private readonly IEnvelope _totalEnv = new Envelope();
        private ISpatialIndex _index;
        private ICoordinate _nestedPt;

        public IndexedNestedRingTester(GeometryGraph graph)
        {
            _graph = graph;
        }

        public ICoordinate NestedPoint { get {return _nestedPt; }}

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
                ICoordinate[] innerRingPts = innerRing.Coordinates;

                var results = _index.Query(innerRing.EnvelopeInternal);
                //System.out.println(results.size());
                for (int j = 0; j < results.Count; j++)
                {
                    var searchRing = (ILinearRing)results[j];
                    var searchRingPts = searchRing.Coordinates;

                    if (innerRing == searchRing)
                        continue;

                    if (!innerRing.EnvelopeInternal.Intersects(searchRing.EnvelopeInternal))
                        continue;

                    ICoordinate innerRingPt = IsValidOp.FindPointNotNode(innerRingPts, searchRing, _graph);
                    Assert.IsTrue(innerRingPt != null, "Unable to find a ring point not a node of the search ring");
                    //Coordinate innerRingPt = innerRingPts[0];

                    Boolean isInside = CGAlgorithms.IsPointInRing(innerRingPt, searchRingPts);
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
            _index = new STRtree();

            for (int i = 0; i < _rings.Count; i++)
            {
                var ring = (ILinearRing)_rings[i];
                var env = ring.EnvelopeInternal;
                _index.Insert(env, ring);
            }
        }
    }
}