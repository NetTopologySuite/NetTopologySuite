using System;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph.Index
{
    public class MonotoneChain
    {
        private MonotoneChainEdge mce;
        private Int32 chainIndex;

        public MonotoneChain(MonotoneChainEdge mce, Int32 chainIndex)
        {
            this.mce = mce;
            this.chainIndex = chainIndex;
        }

        public void ComputeIntersections(MonotoneChain mc, SegmentIntersector si)
        {
            mce.ComputeIntersectsForChain(chainIndex, mc.mce, mc.chainIndex, si);
        }
    }
}