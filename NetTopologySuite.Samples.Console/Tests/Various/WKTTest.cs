using System;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Samples.SimpleTests;
using NetTopologySuite.Coordinates;
using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Various
{
    [TestFixture]
    public class WktTest : BaseSamples
    {
        private readonly IWktGeometryWriter writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="WktTest"/> class.
        /// </summary>
        public WktTest()
        {
            writer = new WktWriter<BufferedCoordinate>();
        }

        [Test]
        public void WriteZeroBasedCoordinateWithDifferentFactories()
        {
            BufferedCoordinateFactory coordFactory = new BufferedCoordinateFactory();
            TestFormatting(coordFactory.Create(0.00000000001, 0.00000000002));
            TestFormatting(coordFactory.Create(0.00001, 0.00002));
            TestFormatting(coordFactory.Create(0.01, 0.02));
            TestFormatting(coordFactory.Create(0.1, 0.2));
            TestFormatting(coordFactory.Create(0, 0));
        }

        private void TestFormatting(ICoordinate c)
        {
            IGeometryFactory<BufferedCoordinate> geoFactory;
            ICoordinateSequenceFactory<BufferedCoordinate> seqFactory
                = new BufferedCoordinateSequenceFactory();

            // Double floating precision
            geoFactory = GeometryFactory<BufferedCoordinate>.CreateFloatingPrecision(seqFactory);
            IGeometry point = geoFactory.CreatePoint(c);
            String result = writer.Write(point);
            Debug.WriteLine(result);

            // Single floating precision
            geoFactory = GeometryFactory<BufferedCoordinate>.CreateFloatingSinglePrecision(seqFactory);
            point = geoFactory.CreatePoint(c);
            result = writer.Write(point);
            Debug.WriteLine(result);

            // Fixed precision
            geoFactory = GeometryFactory<BufferedCoordinate>.CreateFixedPrecision(seqFactory, 1.0);
            point = geoFactory.CreatePoint(c);
            result = writer.Write(point);
            Debug.WriteLine(result);
        }
    }
}