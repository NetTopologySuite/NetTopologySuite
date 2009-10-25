using System;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using NUnit.Framework;

#if unbuffered
using coord = NetTopologySuite.Coordinates.Simple.Coordinate;

#else
using coord = NetTopologySuite.Coordinates.BufferedCoordinate;
#endif
namespace NetTopologySuite.Tests.Issues
{
    [TestFixture]
    public class NtsIssues
    {
        private static readonly IWktGeometryReader<coord> Reader = TestFactories.GeometryFactory.WktReader;

        public void Issue54()
        {
            IGeometry<coord> g =
                Reader.Read(
                    "POLYGON((906.4827 217.8143,927.6762 0.0099999999999909051,36.486899999999991 0.0099999999999909051,0.0099999999999909051 374.8819,906.4827 217.8143))");
            IGeometry<coord> buffer = g.Buffer(2.0d, 4, GeoAPI.Operations.Buffer.BufferStyle.Round);
            Console.WriteLine(buffer.ToString());
            Assert.AreEqual(buffer.GeometryType, OgcGeometryType.Polygon);
        }
    }
}
