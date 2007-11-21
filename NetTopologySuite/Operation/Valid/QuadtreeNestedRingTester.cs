using System;
using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// Tests whether any of a set of <see cref="LinearRing{TCoordinate}" />s are
    /// nested inside another ring in the set, using a <c>Quadtree</c>
    /// index to speed up the comparisons.
    /// </summary>
    public class QuadtreeNestedRingTester
    {
        private GeometryGraph graph; // used to find non-node vertices
        private IList rings = new ArrayList();
        private IExtents totalEnv = new Extents();
        private Quadtree quadtree;
        private ICoordinate nestedPt;

        public QuadtreeNestedRingTester(GeometryGraph graph)
        {
            this.graph = graph;
        }

        public ICoordinate NestedPoint
        {
            get { return nestedPt; }
        }

        public void Add(ILinearRing ring)
        {
            rings.Add(ring);
            totalEnv.ExpandToInclude(ring.EnvelopeInternal);
        }

        public Boolean IsNonNested()
        {
            BuildQuadtree();

            for (Int32 i = 0; i < rings.Count; i++)
            {
                ILinearRing innerRing = (ILinearRing) rings[i];
                ICoordinate[] innerRingPts = innerRing.Coordinates;

                IList results = quadtree.Query((Extents) innerRing.EnvelopeInternal);
                for (Int32 j = 0; j < results.Count; j++)
                {
                    ILinearRing searchRing = (ILinearRing) results[j];
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

        private void BuildQuadtree()
        {
            quadtree = new Quadtree();

            for (Int32 i = 0; i < rings.Count; i++)
            {
                ILinearRing ring = (ILinearRing) rings[i];
                Extents env = (Extents) ring.EnvelopeInternal;
                quadtree.Insert(env, ring);
            }
        }
    }
}