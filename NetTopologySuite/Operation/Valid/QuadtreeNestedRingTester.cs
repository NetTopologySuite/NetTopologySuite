using System;
using System.Collections;
using System.Text;

using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Index.Quadtree;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// Tests whether any of a set of <c>LinearRing</c>s are
    /// nested inside another ring in the set, using a <c>Quadtree</c>
    /// index to speed up the comparisons.
    /// </summary>
    public class QuadtreeNestedRingTester
    {
        private GeometryGraph graph;  // used to find non-node vertices
        private IList rings = new ArrayList();
        private Envelope totalEnv = new Envelope();
        private Quadtree quadtree;
        private Coordinate nestedPt;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph"></param>
        public QuadtreeNestedRingTester(GeometryGraph graph)
        {
            this.graph = graph;
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
        /// <param name="ring"></param>
        public void Add(LinearRing ring)
        {
            rings.Add(ring);
            totalEnv.ExpandToInclude(ring.EnvelopeInternal);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsNonNested()
        {
            BuildQuadtree();

            for (int i = 0; i < rings.Count; i++)
            {
                LinearRing innerRing = (LinearRing) rings[i];
                Coordinate[] innerRingPts = (Coordinate[]) innerRing.Coordinates;

                IList results = quadtree.Query((Envelope) innerRing.EnvelopeInternal);
                for (int j = 0; j < results.Count; j++)
                {
                    LinearRing searchRing = (LinearRing)results[j];
                    Coordinate[] searchRingPts = (Coordinate[]) searchRing.Coordinates;

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

        /// <summary>
        /// 
        /// </summary>
        private void BuildQuadtree()
        {
            quadtree = new Quadtree();

            for (int i = 0; i < rings.Count; i++)
            {
                LinearRing ring = (LinearRing)rings[i];
                Envelope env = (Envelope) ring.EnvelopeInternal;
                quadtree.Insert(env, ring);
            }
        }
    }
}
