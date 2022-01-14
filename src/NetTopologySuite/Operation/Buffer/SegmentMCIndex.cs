using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph.Index;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Chain;
using NetTopologySuite.Index.Strtree;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// A spatial index over a segment sequence
    /// using <see cref="Index.Chain.MonotoneChain"/>s.
    /// </summary>
    /// <author>Martin Davis</author>
    internal class SegmentMCIndex
    {
        private STRtree<Index.Chain.MonotoneChain> index;

        public SegmentMCIndex(Coordinate[] segs)
        {
            index = BuildIndex(segs);
        }

        private STRtree<Index.Chain.MonotoneChain> BuildIndex(Coordinate[] segs)
        {
            var index = new STRtree<Index.Chain.MonotoneChain>();
            var segChains = MonotoneChainBuilder.GetChains(segs, segs);
            foreach (var mc in segChains)
            {
                index.Insert(mc.Envelope, mc);
            }
            return index;
        }

        public void Query(Envelope env, MonotoneChainSelectAction action)
        {
            var v = new MonotoneChainVisitor(env, action);
            index.Query(env, v);
        }

        private class MonotoneChainVisitor : IItemVisitor<Index.Chain.MonotoneChain>
        {
            private readonly Envelope _env;
            private readonly MonotoneChainSelectAction _action;

            public MonotoneChainVisitor(Envelope env, MonotoneChainSelectAction action)
            {
                _env = env;
                _action = action;
            }

            public void VisitItem(Index.Chain.MonotoneChain item)
            {
                item.Select(_env, _action);
            }
        }
    }
}
