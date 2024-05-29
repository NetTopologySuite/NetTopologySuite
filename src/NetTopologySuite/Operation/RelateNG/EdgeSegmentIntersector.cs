using NetTopologySuite.Algorithm;
using NetTopologySuite.Noding;

namespace NetTopologySuite.Operation.RelateNG
{
    /// <summary>
    /// Tests segments of <see cref="RelateSegmentString"/>s
    /// and if they intersect adds the intersection(s)
    /// to the <see cref="TopologyComputer"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    internal class EdgeSegmentIntersector : ISegmentIntersector
    {
        private readonly RobustLineIntersector _li = new RobustLineIntersector();
        private readonly TopologyComputer _topoComputer;

        public EdgeSegmentIntersector(TopologyComputer topoComputer)
        {
            _topoComputer = topoComputer;
        }

        public bool IsDone => _topoComputer.IsResultKnown;

        public void ProcessIntersections(
            ISegmentString ss0, int segIndex0,
            ISegmentString ss1, int segIndex1)
        {
            // don't intersect a segment with itself
            if (ss0 == ss1 && segIndex0 == segIndex1) return;

            var rss0 = (RelateSegmentString)ss0;
            var rss1 = (RelateSegmentString)ss1;
            //TODO: move this ordering logic to TopologyBuilder
            if (rss0.IsA)
            {
                AddIntersections(rss0, segIndex0, rss1, segIndex1);
            }
            else
            {
                AddIntersections(rss1, segIndex1, rss0, segIndex0);
            }
        }

        private void AddIntersections(
            RelateSegmentString ssA, int segIndexA,
            RelateSegmentString ssB, int segIndexB)
        {

            var a0 = ssA.GetCoordinate(segIndexA);
            var a1 = ssA.GetCoordinate(segIndexA + 1);
            var b0 = ssB.GetCoordinate(segIndexB);
            var b1 = ssB.GetCoordinate(segIndexB + 1);

            _li.ComputeIntersection(a0, a1, b0, b1);

            if (!_li.HasIntersection)
                return;

            for (int i = 0; i < _li.IntersectionNum; i++)
            {
                var intPt = _li.GetIntersection(i);
                /*
                 * Ensure endpoint intersections are added once only, for their canonical segments.
                 * Proper intersections lie on a unique segment so do not need to be checked.
                 * And it is important that the Containing Segment check not be used, 
                 * since due to intersection computation roundoff, 
                 * it is not reliable in that situation. 
                 */
                if (_li.IsProper
                    || (ssA.IsContainingSegment(segIndexA, intPt)
                          && ssB.IsContainingSegment(segIndexB, intPt)))
                {
                    var nsa = ssA.CreateNodeSection(segIndexA, intPt);
                    var nsb = ssB.CreateNodeSection(segIndexB, intPt);
                    _topoComputer.AddIntersection(nsa, nsb);
                }
            }
        }
    }
}
