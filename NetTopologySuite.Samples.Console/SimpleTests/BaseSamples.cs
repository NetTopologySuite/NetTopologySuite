using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using GisSharpBlog.NetTopologySuite.Geometries;
using NetTopologySuite.Coordinates;

namespace GisSharpBlog.NetTopologySuite.Samples.SimpleTests
{
    public class BaseSamples
    {
        private static readonly BufferedCoordinateFactory _coordFactory =
            new BufferedCoordinateFactory();

        private readonly IGeometryFactory<BufferedCoordinate> _factory;

        private readonly IWktGeometryReader<BufferedCoordinate> _reader;

        protected BaseSamples()
            : this(new GeometryFactory<BufferedCoordinate>(
                       new BufferedCoordinateSequenceFactory(_coordFactory)))
        {
        }

        protected BaseSamples(IGeometryFactory<BufferedCoordinate> factory)
            : this(factory, new WktReader<BufferedCoordinate>(factory, null))
        {
        }

        protected BaseSamples(IGeometryFactory<BufferedCoordinate> factory,
                              IWktGeometryReader<BufferedCoordinate> reader)
        {
            _factory = factory;
            _reader = reader;
        }

        protected static ICoordinateFactory<BufferedCoordinate> CoordFactory
        {
            get { return _coordFactory; }
        }

        protected IGeometryFactory<BufferedCoordinate> GeoFactory
        {
            get { return _factory; }
        }

        protected IWktGeometryReader<BufferedCoordinate> Reader
        {
            get { return _reader; }
        }

        protected void Write(object o)
        {
            Console.WriteLine(o.ToString());
        }

        protected void Write(String s)
        {
            Console.WriteLine(s);
        }

        public virtual void Start()
        {
        }
    }
}