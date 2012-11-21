using System;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.IO.Tests
{
    public class Issues
    {
        [Test]
        public void TestIssue132Points()
        {
            var pts = new[] {new Coordinate(0, 0), new Coordinate(10, 10), new Coordinate(20, 20)};
            var factory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(4326);
            var mp = factory.CreateMultiPoint(pts);
            Console.WriteLine(mp);
            
            var gc = factory.CreateGeometryCollection(new IGeometry[] {mp});
            Console.WriteLine(gc);

            var writer = new GaiaGeoWriter();
            var buffer = writer.Write(gc);

            Console.WriteLine("Hex: {0}", WKBWriter.ToHex(buffer));

            var reader = new GaiaGeoReader(factory.CoordinateSequenceFactory, factory.PrecisionModel);
            var gc2 = reader.Read(buffer);

            Assert.IsNotNull(gc2);
            Assert.IsTrue(gc.EqualsExact(gc2));
        }

        [Test]
        public void TestIssue132LineStrings()
        {
            var pts = new[] { new Coordinate(0, 0), new Coordinate(10, 10), new Coordinate(20, 20), new Coordinate(30, 30) };
            var lines = new ILineString[2];

            var factory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(4326);
            for (var i = 0; i < pts.Length; i += 2)
            {
                lines[i/2] = factory.CreateLineString(new[] {pts[i], pts[i + 1]});
            }
            var mp = factory.CreateMultiLineString(lines);
            Console.WriteLine(mp);

            var gc = factory.CreateGeometryCollection(new IGeometry[] { mp });
            Console.WriteLine(gc);

            var writer = new GaiaGeoWriter();
            var buffer = writer.Write(gc);

            Console.WriteLine("Hex: {0}", WKBWriter.ToHex(buffer));

            var reader = new GaiaGeoReader(factory.CoordinateSequenceFactory, factory.PrecisionModel);
            var gc2 = reader.Read(buffer);

            Assert.IsNotNull(gc2);
            Assert.IsTrue(gc.EqualsExact(gc2));
        }

        [Test]
        public void TestIssue132Polygons()
        {
            var pts = new[] { new Coordinate(0, 0), new Coordinate(10, 10), new Coordinate(20, 20) };
            var polys = new IPolygon[3];
            var pm = GeoAPI.GeometryServiceProvider.Instance.CreatePrecisionModel(10d);
            var factory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(pm, 4326);
            for (var i = 0; i <pts.Length; i++)
                polys[i] = (IPolygon) factory.CreatePoint(pts[i]).Buffer(4d, 1);
            var mp = factory.CreateMultiPolygon(polys);
            Console.WriteLine(mp);

            var gc = factory.CreateGeometryCollection(new IGeometry[] { mp });
            Console.WriteLine(gc);

            var writer = new GaiaGeoWriter();
            var buffer = writer.Write(gc);

            Console.WriteLine("Hex: {0}", WKBWriter.ToHex(buffer));

            var reader = new GaiaGeoReader(factory.CoordinateSequenceFactory, factory.PrecisionModel);
            var gc2 = reader.Read(buffer);

            Assert.IsNotNull(gc2);
            Assert.IsTrue(gc.EqualsExact(gc2));
        }
    }
}