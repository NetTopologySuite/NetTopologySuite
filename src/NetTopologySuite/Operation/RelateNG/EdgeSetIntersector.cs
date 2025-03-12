using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Chain;
using NetTopologySuite.Index.HPRtree;
using NetTopologySuite.Noding;
using System.Collections.Generic;

namespace NetTopologySuite.Operation.RelateNG
{
   internal class EdgeSetIntersector
    {

        private readonly HPRtree<MonotoneChain> _index = new HPRtree<MonotoneChain>();
        private readonly Envelope _envelope;
        private readonly List<MonotoneChain> _monoChains = new List<MonotoneChain>();
        private int _idCounter = 0;

        public EdgeSetIntersector(IEnumerable<RelateSegmentString> edgesA, IEnumerable<RelateSegmentString> edgesB, Envelope env)
        {
            _envelope = env;
            AddEdges(edgesA);
            AddEdges(edgesB);
            // build index to ensure thread-safety
            _index.Build();
        }

        private void AddEdges(IEnumerable<RelateSegmentString> segStrings)
        {
            foreach (var ss in segStrings)
            {
                AddToIndex(ss);
            }
        }

        private void AddToIndex(ISegmentString segStr)
        {
            var segChains = MonotoneChainBuilder.GetChains(segStr.Coordinates, segStr);
            foreach (var mc in segChains)
            {
                if (_envelope == null || _envelope.Intersects(mc.Envelope))
                {
                    mc.Id = _idCounter++;
                    _index.Insert(mc.Envelope, mc);
                    _monoChains.Add(mc);
                }
            }
        }

        public void Process(EdgeSegmentIntersector intersector)
        {
            var overlapAction = new EdgeSegmentOverlapAction(intersector);

            foreach (var queryChain in _monoChains)
            {
                var overlapChains = _index.Query(queryChain.Envelope);
                foreach (var testChain in overlapChains)
                {
                    /*
                     * following test makes sure we only compare each pair of chains once
                     * and that we don't compare a chain to itself
                     */
                    if (testChain.Id <= queryChain.Id)
                        continue;

                    testChain.ComputeOverlaps(queryChain, overlapAction);
                    if (intersector.IsDone)
                        return;
                }
            }
        }

    }
}
