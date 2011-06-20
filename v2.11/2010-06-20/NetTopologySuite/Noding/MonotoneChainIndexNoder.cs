using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using GisSharpBlog.NetTopologySuite.Index.Chain;
using GisSharpBlog.NetTopologySuite.Index.Quadtree;
using GisSharpBlog.NetTopologySuite.Index.Strtree;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Nodes a set of <see cref="NodedSegmentString{TCoordinate}" />s using a index based
    /// on <see cref="MonotoneChain{TCoordinate}" />s and a <see cref="ISpatialIndex{TBounds,TItem}" />.
    /// The <see cref="ISpatialIndex{TCoordinate, TItem}" /> used should be something that supports
    /// envelope (range) queries efficiently (such as a <see cref="Quadtree{TCoordinate,TItem}" />
    /// or <see cref="StrTree{TCoordinate,TItem}" />.
    /// </summary>
    public class MonotoneChainIndexNoder<TCoordinate> : SinglePassNoder<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly IGeometryFactory<TCoordinate> _geoFactory;
        private readonly StrTree<TCoordinate, MonotoneChain<TCoordinate>> _index;
        private readonly List<MonotoneChain<TCoordinate>> _monoChains = new List<MonotoneChain<TCoordinate>>();
        //private readonly List<SegmentString<TCoordinate>> _nodedSegStrings = new List<SegmentString<TCoordinate>>();
        private Int32 _idCount;
        private Int32 _overlapCount; // statistics

        /// <summary>
        /// Initializes a new instance of the <see cref="MonotoneChainIndexNoder{TCoordinate}"/> class.
        /// </summary>
        /// <param name="segInt">
        /// The <see cref="ISegmentIntersector{TCoordinate}"/> to use.
        /// </param>
        public MonotoneChainIndexNoder(IGeometryFactory<TCoordinate> geoFactory, ISegmentIntersector<TCoordinate> segInt)
            : base(segInt)
        {
            _geoFactory = geoFactory;
            _index = new StrTree<TCoordinate, MonotoneChain<TCoordinate>>(geoFactory);
        }

        public IEnumerable<MonotoneChain<TCoordinate>> MonotoneChains
        {
            get
            {
                return _monoChains;
            }
        }

        public ISpatialIndex<IExtents<TCoordinate>, MonotoneChain<TCoordinate>> Index
        {
            get { return _index; }
        }

        /// <summary>
        /// Computes the noding for a collection of <see cref="NodedSegmentString{TCoordinate}"/>s.
        /// </summary>
        /// <remarks>
        /// Some noders may add all these nodes to the input <see cref="NodedSegmentString{TCoordinate}"/>s;
        /// others may only add some or none at all.
        /// </remarks>
        public override IEnumerable<ISegmentString<TCoordinate>> Node(
            IEnumerable<ISegmentString<TCoordinate>> inputSegmentStrings)
        {
            foreach (ISegmentString<TCoordinate> segmentString in inputSegmentStrings)
            {
                add(segmentString);
            }

            intersectChains();

            return NodedSegmentString<TCoordinate>.GetNodedSubstrings(inputSegmentStrings);
        }

        public override void ComputeNodes(IEnumerable<ISegmentString<TCoordinate>> segmentStrings)
        {
            foreach (ISegmentString<TCoordinate> segmentString in segmentStrings)
            {
                add(segmentString);
            }

            intersectChains();
        }

        public override IEnumerable<TNodingResult> Node<TNodingResult>(
            IEnumerable<ISegmentString<TCoordinate>> segmentStrings,
            Func<ISegmentString<TCoordinate>, TNodingResult> generator)
        {
            foreach (ISegmentString<TCoordinate> segmentString in Node(segmentStrings))
            {
                yield return generator(segmentString);
            }
        }

        private void add(ISegmentString<TCoordinate> item)
        {
            IEnumerable<MonotoneChain<TCoordinate>> segChains
                = MonotoneChainBuilder.GetChains(_geoFactory, item.Coordinates, item);

            foreach (MonotoneChain<TCoordinate> mc in segChains)
            {
                mc.Id = _idCount++;
                _index.Insert(mc);
                _monoChains.Add(mc);
            }
        }

        private void intersectChains()
        {
            //MonotoneChainOverlapAction<TCoordinate> overlapAction = new SegmentOverlapAction(SegmentIntersector);

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
                        ISegmentString<TCoordinate> ss1 = queryChain.Context as ISegmentString<TCoordinate>;
                        foreach (Pair<Int32> pair in queryChain.OverlapIndexes(testChain))
                        {
                            ISegmentString<TCoordinate> ss2 = testChain.Context as ISegmentString<TCoordinate>;

                            Debug.Assert(ss1 != null);
                            Debug.Assert(ss2 != null);

                            SegmentIntersector.ProcessIntersections(ss1, pair.First, ss2, pair.Second);
                        }

                        _overlapCount++;
                    }
                    if (SegmentIntersector.IsDone)
                        return;
                }
            }
        }

        //public class SegmentOverlapAction : MonotoneChainOverlapAction<TCoordinate>
        //{
        //    private readonly ISegmentIntersector<TCoordinate> _si;

        //    /// <summary>
        //    /// Initializes a new instance of the <see cref="SegmentOverlapAction"/> class.
        //    /// </summary>
        //    /// <param name="si">The <see cref="ISegmentIntersector{TCoordinate}" />.</param>
        //    public SegmentOverlapAction(ISegmentIntersector<TCoordinate> si)
        //    {
        //        _si = si;
        //    }

        //    public override void Overlap(MonotoneChain<TCoordinate> mc1, Int32 start1, MonotoneChain<TCoordinate> mc2, Int32 start2)
        //    {
        //        SegmentString<TCoordinate> ss1 = mc1.Context as SegmentString<TCoordinate>;
        //        SegmentString<TCoordinate> ss2 = mc2.Context as SegmentString<TCoordinate>;

        //        Debug.Assert(ss1 != null);
        //        Debug.Assert(ss2 != null);

        //        _si.ProcessIntersections(ss1, start1, ss2, start2);
        //    }
        //}
    }
}