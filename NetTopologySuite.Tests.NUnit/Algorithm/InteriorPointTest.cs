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
    public class InteriorPointTest
    {
        [TestAttribute]
        public void TestAll()
        {
            var name = "NetTopologySuite.Tests.NUnit.TestData.europe.wkt";
            var stream = EmbeddedResourceManager.GetResourceStream(name);
            CheckInteriorPointFile(stream, name);
            name = "NetTopologySuite.Tests.NUnit.TestData.africa.wkt";
            stream = EmbeddedResourceManager.GetResourceStream(name);
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
    }
}