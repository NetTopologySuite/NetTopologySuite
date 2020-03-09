#nullable disable
using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// Tests whether any of a set of <c>LinearRing</c>s are
    /// nested inside another ring in the set, using a simple O(n^2)
    /// comparison.
    /// </summary>
    public class SimpleNestedRingTester
    {
        private readonly GeometryGraph graph;  // used to find non-node vertices
        private readonly List<LinearRing> rings = new List<LinearRing>();
        private Coordinate nestedPt;

        /// <summary>
        ///
        /// </summary>
        /// <param name="graph"></param>
        public SimpleNestedRingTester(GeometryGraph graph)
        {
            this.graph = graph;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ring"></param>
        public void Add(LinearRing ring)
        {
            rings.Add(ring);
        }

        /// <summary>
        ///
        /// </summary>
        public Coordinate NestedPoint => nestedPt;

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public bool IsNonNested()
        {
            foreach (var innerRing in rings)
            {
                var innerRingPts = innerRing.Coordinates;

                foreach (var searchRing in rings)
                {
                    var searchRingPts = searchRing.Coordinates;

                    if (innerRing == searchRing) continue;

                    if (!innerRing.EnvelopeInternal.Intersects(searchRing.EnvelopeInternal)) continue;

                    var innerRingPt = IsValidOp.FindPointNotNode(innerRingPts, searchRing, graph);
                    Assert.IsTrue(innerRingPt != null, "Unable to find a ring point not a node of the search ring");

                    bool isInside = PointLocation.IsInRing(innerRingPt, searchRingPts);
                    if (isInside)
                    {
                        nestedPt = innerRingPt;
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
