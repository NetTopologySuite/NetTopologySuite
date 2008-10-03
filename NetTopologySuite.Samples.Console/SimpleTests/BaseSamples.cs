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

        protected static ICoordinateFactory<BufferedCoordinate> CoordFactory
        {
            get { return _coordFactory; }
        }

        private readonly IGeometryFactory<BufferedCoordinate> _factory;

        protected IGeometryFactory<BufferedCoordinate> GeoFactory
        {
            get { return _factory; }
        }

        private readonly IWktGeometryReader<BufferedCoordinate> _reader;

        protected IWktGeometryReader<BufferedCoordinate> Reader
        {
            get { return _reader; }
        }

        protected BaseSamples()
            : this(new GeometryFactory<BufferedCoordinate>(
                       new BufferedCoordinateSequenceFactory(_coordFactory))) {}

        protected BaseSamples(IGeometryFactory<BufferedCoordinate> factory)
            : this(factory, new WktReader<BufferedCoordinate>(factory, null)) {}

        protected BaseSamples(IGeometryFactory<BufferedCoordinate> factory,
                              IWktGeometryReader<BufferedCoordinate> reader)
        {
            this._factory = factory;
            this._reader = reader;
        }

        protected void Write(object o)
        {
            Console.WriteLine(o.ToString());
        }

        protected void Write(String s)
        {
            Console.WriteLine(s);
        }

        public virtual void Start() {}
    }
}