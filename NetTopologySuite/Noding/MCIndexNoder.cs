using System.Collections;
using GisSharpBlog.NetTopologySuite.Index;
using GisSharpBlog.NetTopologySuite.Index.Chain;
using GisSharpBlog.NetTopologySuite.Index.Quadtree;
using GisSharpBlog.NetTopologySuite.Index.Strtree;
#if SILVERLIGHT
using ArrayList = System.Collections.Generic.List<object>;
#endif

namespace GisSharpBlog.NetTopologySuite.Noding
{

    /// <summary>
    /// Nodes a set of <see cref="ISegmentString" />s using a index based
    /// on <see cref="MonotoneChain" />s and a <see cref="ISpatialIndex" />.
    /// The <see cref="ISpatialIndex" /> used should be something that supports
    /// envelope (range) queries efficiently (such as a <see cref="Quadtree" />
    /// or <see cref="STRtree" />.
    /// </summary>
    public class MCIndexNoder : SinglePassNoder
    {
        private readonly IList monoChains = new ArrayList();
        private readonly ISpatialIndex index = new STRtree();
        private int idCounter;
        private IList nodedSegStrings;
        private int nOverlaps; // statistics

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
        public IList MonotoneChains
        {
            get { return monoChains; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ISpatialIndex Index
        {
            get { return index; }
        }

        /// <summary>
        /// Returns a <see cref="IList"/> of fully noded <see cref="ISegmentString"/>s.
        /// The <see cref="ISegmentString"/>s have the same context as their parent.
        /// </summary>
        /// <returns></returns>
        public override IList GetNodedSubstrings()
        {
            return NodedSegmentString.GetNodedSubstrings(nodedSegStrings);
        }

        /// <summary>
        /// Computes the noding for a collection of <see cref="ISegmentString"/>s.
        /// Some Noders may add all these nodes to the input <see cref="ISegmentString"/>s;
        /// others may only add some or none at all.
        /// </summary>
        /// <param name="inputSegStrings"></param>
        public override void ComputeNodes(IList inputSegStrings)
        {
            nodedSegStrings = inputSegStrings;
            foreach(var obj in inputSegStrings)
                Add((ISegmentString)obj);            
            IntersectChains();            
        }

        /// <summary>
        /// 
        /// </summary>
        private void IntersectChains()
        {
            MonotoneChainOverlapAction overlapAction = new SegmentOverlapAction(SegmentIntersector);
            foreach(var obj in monoChains) 
            {
                var queryChain = (MonotoneChain)obj;
                var overlapChains = index.Query(queryChain.Envelope);
                foreach(var j in overlapChains)
                {
                    var testChain = (MonotoneChain)j;
                    /*
                     * following test makes sure we only compare each pair of chains once
                     * and that we don't compare a chain to itself
                     */
                    if (testChain.Id <= queryChain.Id)
                        continue;
                    queryChain.ComputeOverlaps(testChain, overlapAction);
                    nOverlaps++;
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
            foreach (var obj in segChains) 
            {
                var mc = (MonotoneChain)obj;
                mc.Id = idCounter++;
                index.Insert(mc.Envelope, mc);
                monoChains.Add(mc);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class SegmentOverlapAction : MonotoneChainOverlapAction
        {
            private readonly ISegmentIntersector si;

            /// <summary>
            /// Initializes a new instance of the <see cref="SegmentOverlapAction"/> class.
            /// </summary>
            /// <param name="si">The <see cref="ISegmentIntersector" /></param>
            public SegmentOverlapAction(ISegmentIntersector si)
            {   
                this.si = si;
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
                si.ProcessIntersections(ss1, start1, ss2, start2);
            }

        }
    }
}
