using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Strtree;

namespace NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// Tests whether any of a set of <see cref="LinearRing"/>s are
    /// nested inside another ring in the set, using a spatial
    /// index to speed up the comparisons.
    /// </summary>
    [Obsolete]
    public class IndexedNestedRingTester
    {
        private readonly GeometryGraph _graph;  // used to find non-node vertices
        private readonly IList<LineString> _rings = new List<LineString>();
        private readonly Envelope _totalEnv = new Envelope();
        private ISpatialIndex<LineString> _index;
        private Coordinate _nestedPt;

        public IndexedNestedRingTester(GeometryGraph graph)
        {
            _graph = graph;
        }

        public Coordinate NestedPoint => _nestedPt;

        public void Add(LinearRing ring)
        {
            _rings.Add(ring);
            _totalEnv.ExpandToInclude(ring.EnvelopeInternal);
        }

        public bool IsNonNested()
        {
            BuildIndex();

            for (int i = 0; i < _rings.Count; i++)
            {
                var innerRing = (LinearRing)_rings[i];
                var innerRingPts = innerRing.Coordinates;

                var results = _index.Query(innerRing.EnvelopeInternal);
                for (int j = 0; j < results.Count; j++)
                {
                    var searchRing = (LinearRing)results[j];
                    var searchRingPts = searchRing.Coordinates;

                    if (innerRing == searchRing)
                        continue;

                    if (!innerRing.EnvelopeInternal.Intersects(searchRing.EnvelopeInternal))
                        continue;

                    var innerRingPt = IsValidOp.FindPointNotNode(innerRingPts, searchRing, _graph);
                    // Diego Guidi: removed => see Issue 121
                    //Assert.IsTrue(innerRingPt != null, "Unable to find a ring point not a node of the search ring");
                    /*
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

        /*
         * An implementation of an optimization introduced in GEOS
         * https://github.com/libgeos/geos/pull/255/commits/1bf16cdf5a4827b483a1f712e0597ccb243f58cb
         * 
         * Not used for now, since improvement is small and very data-dependent.
         * 
         * @return
         */
        /*
        private bool IsNonNestedWithIndex()
        {
            BuildIndex();

            for (int i = 0; i < _rings.Count; i++)
            {
                var outerRing = (LinearRing) _rings[i];
                var outerRingPts = outerRing.Coordinates;

                var ptLocator = new IndexedPointInAreaLocator(outerRing);
                var results = _index.Query(outerRing.EnvelopeInternal);
                //System.out.println(results.size());
                for (int j = 0; j < results.Count; j++)
                {
                    var searchRing = (LinearRing) results[j];
                    if (outerRing == searchRing)
                        continue;

                    if (!outerRing.EnvelopeInternal.Intersects(searchRing.EnvelopeInternal))
                        continue;

                    var searchRingPts = searchRing.Coordinates;
                    var innerRingPt = IsValidOp.FindPointNotNode(searchRingPts, outerRing, _graph);

                    if (innerRingPt == null)
                        continue;

                    bool isInside = Location.Exterior != ptLocator.Locate(innerRingPt);
                    //boolean isInside = PointLocation.isInRing(innerRingPt, outerRingPts);

                    if (isInside)
                    {
                        _nestedPt = innerRingPt;
                        return false;
                    }
                }
            }

            return true;
        }
        */

        private void BuildIndex()
        {
            _index = new STRtree<LineString>();

            for (int i = 0; i < _rings.Count; i++)
            {
                var ring = (LinearRing)_rings[i];
                var env = ring.EnvelopeInternal;
                _index.Insert(env, ring);
            }
        }
    }
}
