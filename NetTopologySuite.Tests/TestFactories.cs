using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NetTopologySuite.Coordinates;

namespace NetTopologySuite.Tests
{
    public static class TestFactories
    {
        private static readonly GeometryFactory<BufferedCoordinate> _geoFactory;
        private static readonly BufferedCoordinateFactory _coordFactory;
        private static readonly BufferedCoordinateSequenceFactory _coordSequenceFactory;

        static TestFactories()
        {
            _coordFactory = new BufferedCoordinateFactory();
            _coordSequenceFactory = new BufferedCoordinateSequenceFactory();
            _geoFactory = new GeometryFactory<BufferedCoordinate>(CoordSequenceFactory);
        }

        public static IGeometryFactory<BufferedCoordinate> GeometryFactory
        {
            get { return _geoFactory; }
        }

        public static ICoordinateSequenceFactory<BufferedCoordinate> CoordSequenceFactory
        {
            get { return _coordSequenceFactory; }
        }

        public static ICoordinateFactory<BufferedCoordinate> CoordFactory
        {
            get { return _coordFactory; }
        }
    }
}
