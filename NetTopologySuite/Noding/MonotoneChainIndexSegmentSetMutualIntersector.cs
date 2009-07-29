using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Index.Chain;
using GisSharpBlog.NetTopologySuite.Index.Strtree;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
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
        private readonly List<MonotoneChain<TCoordinate>> _monoChains = new List<MonotoneChain<TCoordinate>>();

        /*
        * The {@link SpatialIndex} used should be something that supports
        * envelope (range) queries efficiently (such as a {@link Quadtree}
        * or {@link STRtree}.
        */
        private readonly StrTree<TCoordinate, MonotoneChain<TCoordinate>> _index;
        private int _indexCounter = 0;
        private int _processCounter = 0;
        // statistics
        private int _nOverlaps = 0;

        ///<summary>
        ///</summary>
        ///<param name="geoFactory"></param>
        public MonotoneChainIndexSegmentSetMutualIntersector(IGeometryFactory<TCoordinate> geoFactory)
            :base()
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

        public override void SetBaseSegments(EdgeList<TCoordinate> segStrings)
        {
            foreach (var segString in segStrings)
                AddToIndex(segString);
        }

        private void AddToIndex(Edge<TCoordinate> segStr)
        {
            IEnumerable<MonotoneChain<TCoordinate>> segChains = MonotoneChainBuilder.GetChains(_geoFactory, segStr.Coordinates, segStr);
            foreach (var monotoneChain in segChains)
            {
                monotoneChain.Id = _indexCounter++;
                _index.Insert(monotoneChain);
            }
        }

        public override void Process(EdgeList<TCoordinate> segStrings)
        {
            _processCounter = _indexCounter + 1;
            _nOverlaps = 0;
            _monoChains.Clear();
            foreach (var segString in segStrings)
                AddToMonoChains(segString);

            IntersectChains();
            //System.out.println("MCIndexBichromaticIntersector: # chain overlaps = " + nOverlaps);
            //System.out.println("MCIndexBichromaticIntersector: # oct chain overlaps = " + nOctOverlaps);
        }

        private void AddToMonoChains(Edge<TCoordinate> segStr)
        {
            IEnumerable<MonotoneChain<TCoordinate>> segChains = MonotoneChainBuilder.GetChains(_geoFactory, segStr.Coordinates, segStr);
            foreach (var monotoneChain in segChains)
            {
                monotoneChain.Id = _processCounter++;
                _monoChains.Add(monotoneChain);
            }
        }

        private void IntersectChains()
        {
            MonotoneChainOverlapAction<TCoordinate> overlapAction = 
                new SegmentOverlapAction<TCoordinate>(_geoFactory, _segInt);

            foreach (var queryChain in _monoChains)
            {
                var overlapChains = _index.Query(queryChain.Extents);
                foreach (var testChain in overlapChains)
                    queryChain.ComputeOverlaps(testChain, overlapAction);
                _nOverlaps++;
                //if (((ISegmentIntersector<TCoordinate>)_segInt).IsDone) return;
            }
        }

        public class SegmentOverlapAction<TCoordinate> : MonotoneChainOverlapAction<TCoordinate>
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                                IComputable<Double, TCoordinate>, IConvertible
        {
            private readonly ISegmentIntersector<TCoordinate> _si = null;

            ///<summary>
            ///</summary>
            ///<param name="geometryFactory"></param>
            ///<param name="si"></param>
            public SegmentOverlapAction(IGeometryFactory<TCoordinate> geometryFactory, ISegmentIntersector<TCoordinate> si)
                : base(geometryFactory)
            {
                _si = si;
            }

            public override void Overlap(
                MonotoneChain<TCoordinate> mc1, int start1,
                MonotoneChain<TCoordinate> mc2, int start2)
            {
                NodedSegmentString<TCoordinate> ss1 = (NodedSegmentString<TCoordinate>)mc1.Context;
                NodedSegmentString<TCoordinate> ss2 = (NodedSegmentString<TCoordinate>)mc2.Context;
                _si.ProcessIntersections(ss1, start1, ss2, start2);
            }

        }
    }
}
