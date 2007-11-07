using System;
using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// Tests whether any of a set of <c>LinearRing</c>s are
    /// nested inside another ring in the set, using a simple O(n^2)
    /// comparison.
    /// </summary>
    public class SimpleNestedRingTester
    {
        private GeometryGraph graph; // used to find non-node vertices
        private IList rings = new ArrayList();
        private ICoordinate nestedPt;

        public SimpleNestedRingTester(GeometryGraph graph)
        {
            this.graph = graph;
        }

        public void Add(ILinearRing ring)
        {
            rings.Add(ring);
        }

        public ICoordinate NestedPoint
        {
            get { return nestedPt; }
        }

        public Boolean IsNonNested()
        {
            for (Int32 i = 0; i < rings.Count; i++)
            {
                ILinearRing innerRing = (ILinearRing) rings[i];
                ICoordinate[] innerRingPts = innerRing.Coordinates;

                for (Int32 j = 0; j < rings.Count; j++)
                {
                    ILinearRing searchRing = (ILinearRing) rings[j];
                    ICoordinate[] searchRingPts = searchRing.Coordinates;

                    if (innerRing == searchRing)
                    {
                        continue;
                    }

                    if (!innerRing.EnvelopeInternal.Intersects(searchRing.EnvelopeInternal))
                    {
                        continue;
                    }

                    ICoordinate innerRingPt = IsValidOp.FindPointNotNode(innerRingPts, searchRing, graph);
                    Assert.IsTrue(innerRingPt != null, "Unable to find a ring point not a node of the search ring");

                    Boolean isInside = CGAlgorithms.IsPointInRing(innerRingPt, searchRingPts);
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