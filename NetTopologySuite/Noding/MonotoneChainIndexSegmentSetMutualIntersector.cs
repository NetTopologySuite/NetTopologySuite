using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Index.Chain;
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

        /*
        * The {@link SpatialIndex} used should be something that supports
        * envelope (range) queries efficiently (such as a {@link Quadtree}
        * or {@link STRtree}.
        */
        private readonly StrTree<TCoordinate, MonotoneChain<TCoordinate>> _index;
        private readonly List<MonotoneChain<TCoordinate>> _monoChains = new List<MonotoneChain<TCoordinate>>();
        private int _indexCounter;
        // statistics
        private int _nOverlaps;
        private int _processCounter;

        ///<summary>
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

        public override void SetBaseSegments(List<NodedSegmentString<TCoordinate>> segStrings)
        {
            foreach (NodedSegmentString<TCoordinate> segString in segStrings)
                AddToIndex(segString);
        }

        private void AddToIndex(NodedSegmentString<TCoordinate> segStr)
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

        public override void Process(List<NodedSegmentString<TCoordinate>> segStrings)
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

        private void AddToMonoChains(NodedSegmentString<TCoordinate> segStr)
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
                NodedSegmentString<TCoordinate> ss1 = (NodedSegmentString<TCoordinate>) mc1.Context;
                NodedSegmentString<TCoordinate> ss2 = (NodedSegmentString<TCoordinate>) mc2.Context;
                _si.ProcessIntersections(ss1, start1, ss2, start2);
            }
        }

        #endregion
    }
}