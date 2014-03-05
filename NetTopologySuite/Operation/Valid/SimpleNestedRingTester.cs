using System.Collections;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.Utilities;
#if PCL
using ArrayList = System.Collections.Generic.List<object>;
#endif

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
        private readonly IList rings = new ArrayList();
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
        public void Add(ILinearRing ring)
        {
            rings.Add(ring);
        }

        /// <summary>
        /// 
        /// </summary>
        public Coordinate NestedPoint
        {
            get
            {
                return nestedPt;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsNonNested()
        {
            for (int i = 0; i < rings.Count; i++) 
            {
                ILinearRing innerRing = (ILinearRing) rings[i];
                Coordinate[] innerRingPts = innerRing.Coordinates;

                for (int j = 0; j < rings.Count; j++) 
                {
                    ILinearRing searchRing = (ILinearRing) rings[j];
                    Coordinate[] searchRingPts = searchRing.Coordinates;

                    if (innerRing == searchRing) continue;

                    if (!innerRing.EnvelopeInternal.Intersects(searchRing.EnvelopeInternal)) continue;

                    Coordinate innerRingPt = IsValidOp.FindPointNotNode(innerRingPts, searchRing, graph);
                    Assert.IsTrue(innerRingPt != null, "Unable to find a ring point not a node of the search ring");

                    bool isInside = CGAlgorithms.IsPointInRing(innerRingPt, searchRingPts);
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
