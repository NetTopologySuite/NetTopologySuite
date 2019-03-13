using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.IO;
using NetTopologySuite.Tests.NUnit.TestData;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    public class InteriorPointTest : GeometryTestCase
    {
        [Test]
        public void TestPolygonZeroArea()
        {
            CheckInteriorPoint(Read("POLYGON ((10 10, 10 10, 10 10, 10 10))"), new Coordinate(10, 10));
        }

        [TestCase("europe.wkt")]
        ////[TestCase("africa.wkt")]
        public void TestAll(string name)
        {
            var stream = EmbeddedResourceManager.GetResourceStream($"NetTopologySuite.Tests.NUnit.TestData.{name}");
            CheckInteriorPointFile(stream, name);
        }

        private static void CheckInteriorPointFile(Stream stream, string name)
        {
            var fileRdr = new WKTFileReader(new StreamReader(stream), new WKTReader());
            CheckInteriorPointFile(fileRdr, name);
        }

        private static void CheckInteriorPointFile(WKTFileReader fileRdr, string name)
        {
            var polys = fileRdr.Read();
            CheckInteriorPoint(name, polys);
        }

        private static void CheckInteriorPoint(string name, IEnumerable<IGeometry> geoms)
        {
            Console.WriteLine(name);
            var sw = new Stopwatch();
            sw.Start();
            foreach (var g in geoms)
            {
                CheckInteriorPoint(g);
                Console.Write(".");
            }
            sw.Stop();
            //Console.WriteLine("\n {0}ms\n", sw.ElapsedMilliseconds);
        }

        private static void CheckInteriorPoint(IGeometry g)
        {
            var ip = g.InteriorPoint;
            Assert.IsTrue(g.Contains(ip));
        }

        private static void CheckInteriorPoint(IGeometry g, Coordinate expectedPt)
        {
            var ip = g.InteriorPoint;
            Assert.That(ip.Coordinate, Is.EqualTo(expectedPt));
        }
    }
}
