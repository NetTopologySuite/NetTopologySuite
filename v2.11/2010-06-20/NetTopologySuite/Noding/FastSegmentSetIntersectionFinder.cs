using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    ///<summary>
    ///Finds if two sets of {@link SegmentStrings}s intersect.
    ///Uses indexing for fast performance and to optimize repeated tests
    ///against a target set of lines.
    ///Short-circuited to return as soon an intersection is found.
    ///
    ///@version 1.7
    ///</summary>
    public class FastSegmentSetIntersectionFinder<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private static LineIntersector<TCoordinate> _li;
        private readonly IGeometryFactory<TCoordinate> _geometryFactory;

        private SegmentSetMutualIntersector<TCoordinate> _segSetMutInt;
        // for testing purposes
        // private SimpleSegmentSetMutualIntersector mci;

        public FastSegmentSetIntersectionFinder(IGeometryFactory<TCoordinate> geometryFactory,
                                                List<ISegmentString<TCoordinate>> baseSegStrings)
        {
            _geometryFactory = geometryFactory;
            if (_li == null || _li.GeometryFactory != geometryFactory)
                _li = new RobustLineIntersector<TCoordinate>(geometryFactory);

            Init(baseSegStrings);
        }

        private void Init(List<ISegmentString<TCoordinate>> baseSegStrings)
        {
            _segSetMutInt = new MonotoneChainIndexSegmentSetMutualIntersector<TCoordinate>(_geometryFactory);
            //    segSetMutInt = new MCIndexIntersectionSegmentSetMutualIntersector();

            //		mci = new SimpleSegmentSetMutualIntersector();
            _segSetMutInt.SetBaseSegments(baseSegStrings);
        }

        ///<summary>
        ///Gets the segment set intersector used by this class.
        ///This allows other uses of the same underlying indexed structure.
        ///</summary>
        ///<returns> the segment set intersector used</returns>
        public SegmentSetMutualIntersector<TCoordinate> GetSegmentSetIntersector()
        {
            return _segSetMutInt;
        }

        public Boolean Intersects(List<ISegmentString<TCoordinate>> segStrings)
        {
            SegmentIntersectionDetector<TCoordinate> intFinder = new SegmentIntersectionDetector<TCoordinate>(_li);
            _segSetMutInt.SetSegmentIntersector(intFinder);

            _segSetMutInt.Process(segStrings);
            return intFinder.HasIntersection;
        }

        public Boolean Intersects(List<ISegmentString<TCoordinate>> segStrings,
                                  SegmentIntersectionDetector<TCoordinate> intDetector)
        {
            _segSetMutInt.SetSegmentIntersector(intDetector);

            _segSetMutInt.Process(segStrings);
            return intDetector.HasIntersection;
        }
    }
}