using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using GisSharpBlog.NetTopologySuite.Index.Chain;
using GisSharpBlog.NetTopologySuite.Index.Strtree;
using GisSharpBlog.NetTopologySuite.Index.Quadtree;
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
    public class MonotoneChainIndexNoder<TCoordinate> : SinglePassNoder<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private readonly List<MonotoneChain<TCoordinate>> _monoChains = new List<MonotoneChain<TCoordinate>>();
        private readonly StrTree<TCoordinate, MonotoneChain<TCoordinate>> _index
            = new StrTree<TCoordinate, MonotoneChain<TCoordinate>>();
        private readonly List<SegmentString<TCoordinate>> _nodedSegStrings = new List<SegmentString<TCoordinate>>();
        private Int32 _idCount = 0;
        private Int32 _overlapCount = 0; // statistics

        /// <summary>
        /// Initializes a new instance of the <see cref="MonotoneChainIndexNoder{TCoordinate}"/> class.
        /// </summary>
        /// <param name="segInt">The <see cref="ISegmentIntersector{TCoordinate}"/> to use.</param>
        public MonotoneChainIndexNoder(ISegmentIntersector<TCoordinate> segInt)
            : base(segInt) {}

        public IEnumerable<MonotoneChain<TCoordinate>> MonotoneChains
        {
            get { return _monoChains; }
        }

        public ISpatialIndex<IExtents<TCoordinate>, MonotoneChain<TCoordinate>> Index
        {
            get { return _index; }
        }

        /// <summary>
        /// Returns a set of fully noded 
        /// <see cref="SegmentString{TCoordinate}"/>s.
        /// The <see cref="SegmentString{TCoordinate}"/>s 
        /// have the same context as their parent.
        /// </summary>
        public override IEnumerable<SegmentString<TCoordinate>> GetNodedSubstrings()
        {
            return SegmentString<TCoordinate>.GetNodedSubstrings(_nodedSegStrings);
        }

        /// <summary>
        /// Computes the noding for a collection of <see cref="SegmentString{TCoordinate}"/>s.
        /// </summary>
        /// <remarks>
        /// Some Noders may add all these nodes to the input <see cref="SegmentString{TCoordinate}"/>s;
        /// others may only add some or none at all.
        /// </remarks>
        public override void ComputeNodes(IEnumerable<SegmentString<TCoordinate>> inputSegmentStrings)
        {
            _nodedSegStrings.AddRange(inputSegmentStrings);

            foreach (SegmentString<TCoordinate> segmentString in inputSegmentStrings)
            {
                add(segmentString);
            }

            IntersectChains();
        }

        private void IntersectChains()
        {
            MonotoneChainOverlapAction<TCoordinate> overlapAction = new SegmentOverlapAction(SegmentIntersector);

            foreach (MonotoneChain<TCoordinate> queryChain in _monoChains)
            {
                IEnumerable<MonotoneChain<TCoordinate>> overlapChains = _index.Query(queryChain.Extents);

                foreach (MonotoneChain<TCoordinate> testChain in overlapChains)
                {
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

        private void add(SegmentString<TCoordinate> item)
        {
            IEnumerable<MonotoneChain<TCoordinate>> segChains = MonotoneChainBuilder.GetChains(item.Coordinates, item);
            
            foreach (MonotoneChain<TCoordinate> mc in segChains)
            {
                mc.Id = _idCount++;
                _index.Insert(mc.Extents, mc);
                _monoChains.Add(mc);
            }
        }

        public class SegmentOverlapAction : MonotoneChainOverlapAction<TCoordinate>
        {
            private readonly ISegmentIntersector<TCoordinate> _si = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="SegmentOverlapAction"/> class.
            /// </summary>
            /// <param name="si">The <see cref="ISegmentIntersector{TCoordinate}" /></param>
            public SegmentOverlapAction(ISegmentIntersector<TCoordinate> si)
            {
                _si = si;
            }

            public override void Overlap(MonotoneChain<TCoordinate> mc1, Int32 start1, MonotoneChain<TCoordinate> mc2, Int32 start2)
            {
                SegmentString<TCoordinate> ss1 = (SegmentString<TCoordinate>)mc1.Context;
                SegmentString<TCoordinate> ss2 = (SegmentString<TCoordinate>)mc2.Context;
                _si.ProcessIntersections(ss1, start1, ss2, start2);
            }
        }
    }
}