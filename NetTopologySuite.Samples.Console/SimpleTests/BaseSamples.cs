using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using NetTopologySuite.Geometries;
#if BUFFERED
using coord = NetTopologySuite.Coordinates.BufferedCoordinate;
using coordFac = NetTopologySuite.Coordinates.BufferedCoordinateFactory;
using coordSeqFac = NetTopologySuite.Coordinates.BufferedCoordinateSequenceFactory;
#else
using coord = NetTopologySuite.Coordinates.Simple.Coordinate;
using coordFac = NetTopologySuite.Coordinates.Simple.CoordinateFactory;
using coordSeqFac = NetTopologySuite.Coordinates.Simple.CoordinateSequenceFactory;
#endif

namespace NetTopologySuite.Samples.SimpleTests
{
    public class BaseSamples
    {
        private static readonly coordFac _coordFactory =
            new coordFac();

        private readonly IGeometryFactory<coord> _factory;

        private readonly IWktGeometryReader<coord> _reader;

        protected BaseSamples()
            : this(new GeometryFactory<coord>(
                       new coordSeqFac(_coordFactory)))
        {
        }

        protected BaseSamples(IGeometryFactory<coord> factory)
            : this(factory, new WktReader<coord>(factory, null))
        {
        }

        protected BaseSamples(IGeometryFactory<coord> factory,
                              IWktGeometryReader<coord> reader)
        {
            _factory = factory;
            _reader = reader;
        }

        protected static ICoordinateFactory<coord> CoordFactory
        {
            get { return _coordFactory; }
        }

        protected IGeometryFactory<coord> GeoFactory
        {
            get { return _factory; }
        }

        protected IWktGeometryReader<coord> Reader
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