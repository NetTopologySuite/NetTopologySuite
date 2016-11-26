﻿using System.Collections.Generic;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Chain;
using NetTopologySuite.Index.Strtree;
using MonotoneChain = NetTopologySuite.Index.Chain.MonotoneChain;

namespace NetTopologySuite.Noding
{
    ///<summary>
    /// Intersects two sets of <see cref="ISegmentString"/>s using a index based
    /// on <see cref="MonotoneChain"/>s and a <see cref="ISpatialIndex"/>.
    /// <para/>
    /// Thread-safe and immutable.
    ///</summary>
    public class MCIndexSegmentSetMutualIntersector : ISegmentSetMutualIntersector
    {
       /*
        * The SpatialIndex used should be something that supports envelope
        * (range) queries efficiently (such as a Quadtree or STRtree).
        */
        private readonly STRtree<MonotoneChain> _index = new STRtree<MonotoneChain>();

        /// <summary>
        /// Constructs a new intersector for a given set of <see cref="ISegmentString"/>s.
        /// </summary>
        /// <param name="baseSegStrings">The base segment strings to intersect</param>
        public MCIndexSegmentSetMutualIntersector(IEnumerable<ISegmentString> baseSegStrings)
        {
            InitBaseSegments(baseSegStrings);
        }

        /// <summary>
        /// Gets the index constructed over the base segment strings
        /// </summary>
        /// <remarks>NOTE: To retain thread-safety, treat returned value as immutable</remarks>
        public ISpatialIndex<MonotoneChain> Index
        {
            get { return _index; }
        }

        private void InitBaseSegments(IEnumerable<ISegmentString> segStrings)
        {
            foreach (var segmentString in segStrings)
            {
                AddToIndex(segmentString);
            }
            // build index to ensure thread-safety
            _index.Build();

        }

        private void AddToIndex(ISegmentString segStr)
        {
            var segChains = MonotoneChainBuilder.GetChains(segStr.Coordinates, segStr);
            foreach (var mc in segChains)
            {
                _index.Insert(mc.Envelope, mc);
            }
        }

        /// <summary>
        /// Calls <see cref="ISegmentIntersector.ProcessIntersections(ISegmentString, int, ISegmentString, int)"/>
        /// for all <i>candidate</i> intersections between
        /// the given collection of SegmentStrings and the set of indexed segments. 
        /// </summary>
        /// <param name="segmentStrings"></param>
        /// <param name="segmentIntersector"></param>
        public void Process(ICollection<ISegmentString> segmentStrings, ISegmentIntersector segmentIntersector)
        {
            var monoChains = new List<MonotoneChain>();
            foreach (var segStr in segmentStrings)
            {
                AddToMonoChains(segStr, monoChains);
            }
            IntersectChains(monoChains, segmentIntersector);
            //    System.out.println("MCIndexBichromaticIntersector: # chain overlaps = " + nOverlaps);
            //    System.out.println("MCIndexBichromaticIntersector: # oct chain overlaps = " + nOctOverlaps);
        }

        private static void AddToMonoChains(ISegmentString segStr, List<MonotoneChain> monotoneChains)
        {
            var segChains = MonotoneChainBuilder.GetChains(segStr.Coordinates, segStr);
            foreach (var mc in segChains)
            {
                monotoneChains.Add(mc);
            }
        }

        private void IntersectChains(IEnumerable<MonotoneChain> monoChains, ISegmentIntersector segmentIntersector)
        {
            MonotoneChainOverlapAction overlapAction = new SegmentOverlapAction(segmentIntersector);

            foreach (var queryChain in monoChains)
            {
                var overlapChains = _index.Query(queryChain.Envelope);
                foreach (var testChain in overlapChains)
                {
                    queryChain.ComputeOverlaps(testChain, overlapAction);
                    if (segmentIntersector.IsDone) return;
                }
            }
        }

        /// <summary>
        /// Segment overlap action class
        /// </summary>
        public class SegmentOverlapAction : MonotoneChainOverlapAction
        {
            private readonly ISegmentIntersector _si;

            /// <summary>
            /// Creates an instance of this class using the provided <see cref="ISegmentIntersector"/>
            /// </summary>
            /// <param name="si">The segment intersector to use</param>
            public SegmentOverlapAction(ISegmentIntersector si)
            {
                _si = si;
            }

            public override void Overlap(MonotoneChain mc1, int start1, MonotoneChain mc2, int start2)
            {
                var ss1 = (ISegmentString)mc1.Context;
                var ss2 = (ISegmentString)mc2.Context;
                _si.ProcessIntersections(ss1, start1, ss2, start2);
            }

        }
    }
}
