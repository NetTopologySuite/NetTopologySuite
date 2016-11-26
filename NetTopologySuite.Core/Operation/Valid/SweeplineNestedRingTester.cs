using System.Collections;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.Index.Sweepline;
using NetTopologySuite.Utilities;

#if PCL
using ArrayList = System.Collections.Generic.List<object>;
#endif

namespace NetTopologySuite.Operation.Valid
{
    /// <summary>
    ///     Tests whether any of a set of <c>LinearRing</c>s are
    ///     nested inside another ring in the set, using a <c>SweepLineIndex</c>
    ///     index to speed up the comparisons.
    /// </summary>
    public class SweeplineNestedRingTester
    {
        private readonly GeometryGraph graph; // used to find non-node vertices
        private readonly IList rings = new ArrayList();
        private SweepLineIndex sweepLine;
        private Envelope totalEnv = new Envelope();

        /// <summary>
        /// </summary>
        /// <param name="graph"></param>
        public SweeplineNestedRingTester(GeometryGraph graph)
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
            BuildIndex();
            var action = new OverlapAction(this);
            sweepLine.ComputeOverlaps(action);
            return action.IsNonNested;
        }

        /// <summary>
        /// </summary>
        private void BuildIndex()
        {
            sweepLine = new SweepLineIndex();
            for (var i = 0; i < rings.Count; i++)
            {
                var ring = (ILinearRing) rings[i];
                var env = ring.EnvelopeInternal;
                var sweepInt = new SweepLineInterval(env.MinX, env.MaxX, ring);
                sweepLine.Add(sweepInt);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="innerRing"></param>
        /// <param name="searchRing"></param>
        /// <returns></returns>
        private bool IsInside(ILinearRing innerRing, ILinearRing searchRing)
        {
            var innerRingPts = innerRing.Coordinates;
            var searchRingPts = searchRing.Coordinates;
            if (!innerRing.EnvelopeInternal.Intersects(searchRing.EnvelopeInternal))
                return false;
            var innerRingPt = IsValidOp.FindPointNotNode(innerRingPts, searchRing, graph);
            Assert.IsTrue(innerRingPt != null, "Unable to find a ring point not a node of the search ring");
            var isInside = CGAlgorithms.IsPointInRing(innerRingPt, searchRingPts);
            if (isInside)
            {
                NestedPoint = innerRingPt;
                return true;
            }
            return false;
        }

        /// <summary>
        /// </summary>
        public class OverlapAction : ISweepLineOverlapAction
        {
            private readonly SweeplineNestedRingTester container;

            /// <summary>
            /// </summary>
            /// <param name="container"></param>
            public OverlapAction(SweeplineNestedRingTester container)
            {
                this.container = container;
            }

            /// <summary>
            /// </summary>
            public bool IsNonNested { get; private set; } = true;

            /// <summary>
            /// </summary>
            /// <param name="s0"></param>
            /// <param name="s1"></param>
            public void Overlap(SweepLineInterval s0, SweepLineInterval s1)
            {
                var innerRing = (ILinearRing) s0.Item;
                var searchRing = (ILinearRing) s1.Item;
                if (innerRing == searchRing)
                    return;
                if (container.IsInside(innerRing, searchRing))
                    IsNonNested = false;
            }
        }
    }
}