using System;
using System.Collections;
using System.Text;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Index.Sweepline;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// Tests whether any of a set of <c>LinearRing</c>s are
    /// nested inside another ring in the set, using a <c>SweepLineIndex</c>
    /// index to speed up the comparisons.
    /// </summary>
    public class SweeplineNestedRingTester
    {
        private GeometryGraph graph;  // used to find non-node vertices
        private IList rings = new ArrayList();
        private Envelope totalEnv = new Envelope();
        private SweepLineIndex sweepLine;
        private Coordinate nestedPt = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph"></param>
        public SweeplineNestedRingTester(GeometryGraph graph)
        {
            this.graph = graph;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual Coordinate NestedPoint 
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
        public virtual void Add(LinearRing ring)
        {
            rings.Add(ring);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool IsNonNested()
        {
            BuildIndex();
            OverlapAction action = new OverlapAction(this);
            sweepLine.ComputeOverlaps(action);
            return action.IsNonNested;
        }

        /// <summary>
        /// 
        /// </summary>
        private void BuildIndex()
        {
            sweepLine = new SweepLineIndex();
            for (int i = 0; i < rings.Count; i++) 
            {
                LinearRing ring = (LinearRing) rings[i];
                Envelope env = ring.EnvelopeInternal;
                SweepLineInterval sweepInt = new SweepLineInterval(env.MinX, env.MaxX, ring);
                sweepLine.Add(sweepInt);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="innerRing"></param>
        /// <param name="searchRing"></param>
        /// <returns></returns>
        private bool IsInside(LinearRing innerRing, LinearRing searchRing)
        {
            Coordinate[] innerRingPts = innerRing.Coordinates;
            Coordinate[] searchRingPts = searchRing.Coordinates;
            if (!innerRing.EnvelopeInternal.Intersects(searchRing.EnvelopeInternal))
                return false;
            Coordinate innerRingPt = IsValidOp.FindPointNotNode(innerRingPts, searchRing, graph);
            Assert.IsTrue(innerRingPt != null, "Unable to find a ring point not a node of the search ring");
            bool isInside = CGAlgorithms.IsPointInRing(innerRingPt, searchRingPts);
            if (isInside) 
            {
                nestedPt = innerRingPt;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public class OverlapAction : ISweepLineOverlapAction
        {
            private SweeplineNestedRingTester container = null;
            bool isNonNested = true;

            /// <summary>
            /// 
            /// </summary>
            public virtual bool IsNonNested
            {
                get 
                { 
                    return isNonNested; 
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="container"></param>
            public OverlapAction(SweeplineNestedRingTester container)
            {
                this.container = container;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="s0"></param>
            /// <param name="s1"></param>
            public virtual void Overlap(SweepLineInterval s0, SweepLineInterval s1)
            {
                LinearRing innerRing = (LinearRing) s0.Item;
                LinearRing searchRing = (LinearRing) s1.Item;
                if (innerRing == searchRing) 
                    return;
                if (container.IsInside(innerRing, searchRing))
                    isNonNested = false;
            }

        }
    }
}
