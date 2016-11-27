using System.Collections;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Operation.Valid
{
    /// <summary>
    ///     Tests whether any of a set of <c>LinearRing</c>s are
    ///     nested inside another ring in the set, using a simple O(n^2)
    ///     comparison.
    /// </summary>
    public class SimpleNestedRingTester
    {
        private readonly GeometryGraph graph; // used to find non-node vertices
        private readonly IList rings = new ArrayList();

        /// <summary>
        /// </summary>
        /// <param name="graph"></param>
        public SimpleNestedRingTester(GeometryGraph graph)
        {
            this.graph = graph;
        }

        /// <summary>
        /// </summary>
        public Coordinate NestedPoint { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="ring"></param>
        public void Add(ILinearRing ring)
        {
            rings.Add(ring);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public bool IsNonNested()
        {
            for (var i = 0; i < rings.Count; i++)
            {
                var innerRing = (ILinearRing) rings[i];
                var innerRingPts = innerRing.Coordinates;

                for (var j = 0; j < rings.Count; j++)
                {
                    var searchRing = (ILinearRing) rings[j];
                    var searchRingPts = searchRing.Coordinates;

                    if (innerRing == searchRing) continue;

                    if (!innerRing.EnvelopeInternal.Intersects(searchRing.EnvelopeInternal)) continue;

                    var innerRingPt = IsValidOp.FindPointNotNode(innerRingPts, searchRing, graph);
                    Assert.IsTrue(innerRingPt != null, "Unable to find a ring point not a node of the search ring");

                    var isInside = CGAlgorithms.IsPointInRing(innerRingPt, searchRingPts);
                    if (isInside)
                    {
                        NestedPoint = innerRingPt;
                        return false;
                    }
                }
            }
            return true;
        }
    }
}