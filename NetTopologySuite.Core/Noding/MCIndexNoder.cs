using System.Collections.Generic;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Chain;
using NetTopologySuite.Index.Strtree;

namespace NetTopologySuite.Noding
{

    /// <summary>
    /// Nodes a set of <see cref="ISegmentString" />s using a index based
    /// on <see cref="MonotoneChain" />s and a <see cref="ISpatialIndex" />.
    /// The <see cref="ISpatialIndex" /> used should be something that supports
    /// envelope (range) queries efficiently (such as a <c>Quadtree</c>"
    /// or <see cref="STRtree{MonotoneChain}" />.
    /// </summary>
    public class MCIndexNoder : SinglePassNoder
    {
        private readonly List<MonotoneChain> _monoChains = new List<MonotoneChain>();
        private readonly ISpatialIndex<MonotoneChain> _index = new STRtree<MonotoneChain>();
        private int _idCounter;
        private IList<ISegmentString> _nodedSegStrings;
        private int _nOverlaps; // statistics

        /// <summary>
        /// Initializes a new instance of the <see cref="MCIndexNoder"/> class.
        /// </summary>
        public MCIndexNoder() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MCIndexNoder"/> class.
        /// </summary>
        /// <param name="segInt">The <see cref="ISegmentIntersector"/> to use.</param>
        public MCIndexNoder(ISegmentIntersector segInt) 
            : base(segInt) { }

        /// <summary>
        /// 
        /// </summary>
        public IList<MonotoneChain> MonotoneChains
        {
            get { return _monoChains; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ISpatialIndex<MonotoneChain> Index
        {
            get { return _index; }
        }

        /// <summary>
        /// Returns a <see cref="IList{ISegmentString}"/> of fully noded <see cref="ISegmentString"/>s.
        /// The <see cref="ISegmentString"/>s have the same context as their parent.
        /// </summary>
        /// <returns></returns>
        public override IList<ISegmentString> GetNodedSubstrings()
        {
            return NodedSegmentString.GetNodedSubstrings(_nodedSegStrings);
        }

        /// <summary>
        /// Computes the noding for a collection of <see cref="ISegmentString"/>s.
        /// Some Noders may add all these nodes to the input <see cref="ISegmentString"/>s;
        /// others may only add some or none at all.
        /// </summary>
        /// <param name="inputSegStrings"></param>
        public override void ComputeNodes(IList<ISegmentString> inputSegStrings)
        {
            _nodedSegStrings = inputSegStrings;
            foreach(var obj in inputSegStrings)
                Add(obj);            
            IntersectChains();            
        }

        /// <summary>
        /// 
        /// </summary>
        private void IntersectChains()
        {
            MonotoneChainOverlapAction overlapAction = new SegmentOverlapAction(SegmentIntersector);
            foreach(var obj in _monoChains) 
            {
                var queryChain = obj;
                var overlapChains = _index.Query(queryChain.Envelope);
                foreach(var testChain in overlapChains)
                {
                    /*
                     * following test makes sure we only compare each pair of chains once
                     * and that we don't compare a chain to itself
                     */
                    if (testChain.Id > queryChain.Id)
                    {
                        queryChain.ComputeOverlaps(testChain, overlapAction);
                        _nOverlaps++;
                    }
                    // short-circuit if possible
                    if (SegmentIntersector.IsDone)
                        return;

                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segStr"></param>
        private void Add(ISegmentString segStr)
        {
            var segChains = MonotoneChainBuilder.GetChains(segStr.Coordinates, segStr);
            foreach (var mc in segChains) 
            {
                mc.Id = _idCounter++;
                _index.Insert(mc.Envelope, mc);
                _monoChains.Add(mc);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class SegmentOverlapAction : MonotoneChainOverlapAction
        {
            private readonly ISegmentIntersector _si;

            /// <summary>
            /// Initializes a new instance of the <see cref="SegmentOverlapAction"/> class.
            /// </summary>
            /// <param name="si">The <see cref="ISegmentIntersector" /></param>
            public SegmentOverlapAction(ISegmentIntersector si)
            {   
                _si = si;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="mc1"></param>
            /// <param name="start1"></param>
            /// <param name="mc2"></param>
            /// <param name="start2"></param>
            public override void Overlap(MonotoneChain mc1, int start1, MonotoneChain mc2, int start2)
            {
                var ss1 = (ISegmentString) mc1.Context;
                var ss2 = (ISegmentString) mc2.Context;
                _si.ProcessIntersections(ss1, start1, ss2, start2);
            }

        }
    }
}
