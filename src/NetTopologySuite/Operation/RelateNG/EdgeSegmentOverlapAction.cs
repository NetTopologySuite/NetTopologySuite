using NetTopologySuite.Index.Chain;
using NetTopologySuite.Noding;

namespace NetTopologySuite.Operation.RelateNG
{
    internal class EdgeSegmentOverlapAction : MonotoneChainOverlapAction
    {
        private readonly ISegmentIntersector _si = null;

        public EdgeSegmentOverlapAction(ISegmentIntersector si)
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
