namespace NetTopologySuite.GeometriesGraph.Index
{
    /// <summary>
    ///
    /// </summary>
    public class MonotoneChain
    {
        private readonly MonotoneChainEdge mce;
        private readonly int chainIndex;

        /// <summary>
        ///
        /// </summary>
        /// <param name="mce"></param>
        /// <param name="chainIndex"></param>
        public MonotoneChain(MonotoneChainEdge mce, int chainIndex)
        {
            this.mce = mce;
            this.chainIndex = chainIndex;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="mc"></param>
        /// <param name="si"></param>
        public void ComputeIntersections(MonotoneChain mc, SegmentIntersector si)
        {
            this.mce.ComputeIntersectsForChain(chainIndex, mc.mce, mc.chainIndex, si);
        }
    }
}
