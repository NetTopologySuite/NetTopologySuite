using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using GisSharpBlog.NetTopologySuite.Index.Chain;
using GisSharpBlog.NetTopologySuite.Index.Quadtree;
using GisSharpBlog.NetTopologySuite.Index.Strtree;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    ///<summary>
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    public class MonotoneChainIndexSegmentSetMutualIntersector<TCoordinate> : SegmentSetMutualIntersector<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly IGeometryFactory<TCoordinate> _geoFactory;

        /// <summary>
        /// The Spatial index used should be something that supports
        /// envelope (range) queries efficiently (such as a <see cref="Quadtree{TCoordinate,TItem}"/>
        /// or <see cref="StrTree{TCoordinate,TItem}"/>
        /// </summary>
        private readonly StrTree<TCoordinate, MonotoneChain<TCoordinate>> _index;
        private readonly List<MonotoneChain<TCoordinate>> _monoChains = new List<MonotoneChain<TCoordinate>>();
        private int _indexCounter;
        // statistics
        private int _nOverlaps;
        private int _processCounter;

        ///<summary>
        /// Creates an instance of this class
        ///</summary>
        ///<param name="geoFactory"></param>
        public MonotoneChainIndexSegmentSetMutualIntersector(IGeometryFactory<TCoordinate> geoFactory)
        {
            _geoFactory = geoFactory;
            _index = new StrTree<TCoordinate, MonotoneChain<TCoordinate>>(geoFactory);
        }

        public List<MonotoneChain<TCoordinate>> MonotoneChains
        {
            get { return _monoChains; }
        }

        public StrTree<TCoordinate, MonotoneChain<TCoordinate>> Index
        {
            get { return _index; }
        }

        public override void SetBaseSegments(IEnumerable<ISegmentString<TCoordinate>> segStrings)
        {
            foreach (ISegmentString<TCoordinate> segString in segStrings)
                AddToIndex(segString);
        }

        private void AddToIndex(ISegmentString<TCoordinate> segStr)
        {
            IEnumerable<MonotoneChain<TCoordinate>> segChains = MonotoneChainBuilder.GetChains(_geoFactory,
                                                                                               segStr.Coordinates,
                                                                                               segStr);
            foreach (MonotoneChain<TCoordinate> monotoneChain in segChains)
            {
                monotoneChain.Id = _indexCounter++;
                _index.Insert(monotoneChain);
            }
        }

        public override void Process(List<ISegmentString<TCoordinate>> segStrings)
        {
            _processCounter = _indexCounter + 1;
            _nOverlaps = 0;
            _monoChains.Clear();
            foreach (NodedSegmentString<TCoordinate> segString in segStrings)
                AddToMonoChains(segString);

            IntersectChains();
            //System.out.println("MCIndexBichromaticIntersector: # chain overlaps = " + nOverlaps);
            //System.out.println("MCIndexBichromaticIntersector: # oct chain overlaps = " + nOctOverlaps);
        }

        private void AddToMonoChains(ISegmentString<TCoordinate> segStr)
        {
            IEnumerable<MonotoneChain<TCoordinate>> segChains = MonotoneChainBuilder.GetChains(_geoFactory,
                                                                                               segStr.Coordinates,
                                                                                               segStr);
            foreach (MonotoneChain<TCoordinate> monotoneChain in segChains)
            {
                monotoneChain.Id = _processCounter++;
                _monoChains.Add(monotoneChain);
            }
        }

        private void IntersectChains()
        {
            MonotoneChainOverlapAction<TCoordinate> overlapAction =
                new SegmentOverlapAction<TCoordinate>(_geoFactory, _segInt);

            foreach (MonotoneChain<TCoordinate> queryChain in _monoChains)
            {
                IEnumerable<MonotoneChain<TCoordinate>> overlapChains = _index.Query(queryChain.Extents);
                foreach (MonotoneChain<TCoordinate> testChain in overlapChains)
                    queryChain.ComputeOverlaps(testChain, overlapAction);
                _nOverlaps++;
                //if (((ISegmentIntersector<TCoordinate>)_segInt).IsDone) return;
            }
        }

        #region Nested type: SegmentOverlapAction

        public class SegmentOverlapAction<TCoordinate> : MonotoneChainOverlapAction<TCoordinate>
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<Double, TCoordinate>, IConvertible
        {
            private readonly ISegmentIntersector<TCoordinate> _si;

            ///<summary>
            ///</summary>
            ///<param name="geometryFactory"></param>
            ///<param name="si"></param>
            public SegmentOverlapAction(IGeometryFactory<TCoordinate> geometryFactory,
                                        ISegmentIntersector<TCoordinate> si)
                : base(geometryFactory)
            {
                _si = si;
            }

            public override void Overlap(
                MonotoneChain<TCoordinate> mc1, int start1,
                MonotoneChain<TCoordinate> mc2, int start2)
            {
                ISegmentString<TCoordinate> ss1 = (ISegmentString<TCoordinate>) mc1.Context;
                ISegmentString<TCoordinate> ss2 = (ISegmentString<TCoordinate>) mc2.Context;
                _si.ProcessIntersections(ss1, start1, ss2, start2);
            }
        }

        #endregion
    }
}