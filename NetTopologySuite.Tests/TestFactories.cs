using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

#if unbuffered
using coord = NetTopologySuite.Coordinates.Simple.Coordinate;
using coordFac = NetTopologySuite.Coordinates.Simple.CoordinateFactory;
using coordSeqFac = NetTopologySuite.Coordinates.Simple.CoordinateSequenceFactory;

#else
using coord = NetTopologySuite.Coordinates.BufferedCoordinate;
using coordFac = NetTopologySuite.Coordinates.BufferedCoordinateFactory;
using coordSeqFac = NetTopologySuite.Coordinates.BufferedCoordinateSequenceFactory;
#endif

using NetTopologySuite.Coordinates;

namespace NetTopologySuite.Tests
{
    public static class TestFactories
    {
        private static readonly GeometryFactory<coord> _geoFactory;
        private static readonly coordFac _coordFactory;
        private static readonly coordSeqFac _coordSequenceFactory;

        static TestFactories()
        {
            _coordFactory = new coordFac();
            _coordSequenceFactory = new coordSeqFac();
            _geoFactory = new GeometryFactory<coord>(CoordSequenceFactory);
            RobustLineIntersector<coord>.FloatingPrecisionCoordinateFactory =
                _coordFactory;
        }

        public static IGeometryFactory<coord> GeometryFactory
        {
            get { return _geoFactory; }
        }

        public static ICoordinateSequenceFactory<coord> CoordSequenceFactory
        {
            get { return _coordSequenceFactory; }
        }

        public static ICoordinateFactory<coord> CoordFactory
        {
            get { return _coordFactory; }
        }
    }
}
