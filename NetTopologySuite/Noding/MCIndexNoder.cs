using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Indexing;
using GisSharpBlog.NetTopologySuite.Index.Chain;
using GisSharpBlog.NetTopologySuite.Index.Strtree;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Nodes a set of <see cref="SegmentString{TCoordinate}" />s using a index based
    /// on <see cref="MonotoneChain{TCoordinate}" />s and a <see cref="ISpatialIndex{TCoordinate,TItem}" />.
    /// The <see cref="ISpatialIndex{TCoordinate, TItem}" /> used should be something that supports
    /// envelope (range) queries efficiently (such as a <see cref="Quadtree{TCoordinate, TItem}" />
    /// or <see cref="StrTree{TCoordinate, TItem}" />.
    /// </summary>
    public class MCIndexNoder<TCoordinate> : SinglePassNoder<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private readonly List<MonotoneChain<TCoordinate>> _monoChains = new List<MonotoneChain<TCoordinate>>();
        private readonly StrTree<TCoordinate, SegmentString<TCoordinate>> _index 
            = new StrTree<TCoordinate, SegmentString<TCoordinate>>();
        private List<SegmentString<TCoordinate>> _nodedSegStrings = null;
        private Int32 _idCount = 0;
        private Int32 _overlapCount = 0; // statistics

        /// <summary>
        /// Initializes a new instance of the <see cref="MCIndexNoder"/> class.
        /// </summary>
        /// <param name="segInt">The <see cref="ISegmentIntersector"/> to use.</param>
        public MCIndexNoder(ISegmentIntersector<TCoordinate> segInt)
            : base(segInt) {}

        public IEnumerable<MonotoneChain<TCoordinate>> MonotoneChains
        {
            get { return _monoChains; }
        }

        public ISpatialIndex<TCoordinate, SegmentString<TCoordinate>> Index
        {
            get { return _index; }
        }

        /// <summary>
        /// Returns a <see cref="IList"/> of fully noded <see cref="SegmentString"/>s.
        /// The <see cref="SegmentString"/>s have the same context as their parent.
        /// </summary>
        public override IList GetNodedSubstrings()
        {
            return SegmentString<TCoordinate>.GetNodedSubstrings(_nodedSegStrings);
        }

        /// <summary>
        /// Computes the noding for a collection of <see cref="SegmentString"/>s.
        /// Some Noders may add all these nodes to the input <see cref="SegmentString"/>s;
        /// others may only add some or none at all.
        /// </summary>
        public override void ComputeNodes(IList inputSegStrings)
        {
            _nodedSegStrings = inputSegStrings;
            foreach (object obj in inputSegStrings)
            {
                Add((SegmentString) obj);
            }
            IntersectChains();
        }

        private void IntersectChains()
        {
            MonotoneChainOverlapAction overlapAction = new SegmentOverlapAction(SegmentIntersector);
            foreach (object obj in _monoChains)
            {
                MonotoneChain queryChain = (MonotoneChain) obj;
                IList overlapChains = _index.Query(queryChain.Envelope);
                foreach (object j in overlapChains)
                {
                    MonotoneChain testChain = (MonotoneChain) j;
                    /*
                     * following test makes sure we only compare each pair of chains once
                     * and that we don't compare a chain to itself
                     */
                    if (testChain.Id > queryChain.Id)
                    {
                        queryChain.ComputeOverlaps(testChain, overlapAction);
                        _overlapCount++;
                    }
                }
            }
        }

        private void Add(SegmentString<TCoordinate> segStr)
        {
            IList segChains = MonotoneChainBuilder.GetChains(segStr.Coordinates, segStr);
            foreach (object obj in segChains)
            {
                MonotoneChain mc = (MonotoneChain) obj;
                mc.Id = _idCount++;
                _index.Insert(mc.Extents, mc);
                _monoChains.Add(mc);
            }
        }

        public class SegmentOverlapAction : MonotoneChainOverlapAction
        {
            private ISegmentIntersector si = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="SegmentOverlapAction"/> class.
            /// </summary>
            /// <param name="si">The <see cref="ISegmentIntersector" /></param>
            public SegmentOverlapAction(ISegmentIntersector si)
            {
                this.si = si;
            }

            public override void Overlap(MonotoneChain mc1, Int32 start1, MonotoneChain mc2, Int32 start2)
            {
                SegmentString ss1 = (SegmentString) mc1.Context;
                SegmentString ss2 = (SegmentString) mc2.Context;
                si.ProcessIntersections(ss1, start1, ss2, start2);
            }
        }
    }
}