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
        private static readonly BufferedCoordinate2DFactory _coordFactory =
            new BufferedCoordinate2DFactory();

        protected static ICoordinateFactory<BufferedCoordinate2D> CoordFactory
        {
            get { return _coordFactory; }
        }

        private readonly IGeometryFactory<BufferedCoordinate2D> _factory;

        protected IGeometryFactory<BufferedCoordinate2D> GeoFactory
        {
            get { return _factory; }
        }

        private readonly IWktGeometryReader<BufferedCoordinate2D> _reader;

        protected IWktGeometryReader<BufferedCoordinate2D> Reader
        {
            get { return _reader; }
        }

        protected BaseSamples()
            : this(new GeometryFactory<BufferedCoordinate2D>(
                       new BufferedCoordinate2DSequenceFactory(_coordFactory))) {}

        protected BaseSamples(IGeometryFactory<BufferedCoordinate2D> factory)
            : this(factory, new WktReader<BufferedCoordinate2D>(factory, null)) {}

        protected BaseSamples(IGeometryFactory<BufferedCoordinate2D> factory,
                              IWktGeometryReader<BufferedCoordinate2D> reader)
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