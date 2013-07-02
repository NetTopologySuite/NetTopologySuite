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
        [Test]
        public void TestAll()
        {
            var filePath = EmbeddedResourceManager.SaveEmbeddedResourceToTempFile("NetTopologySuite.Tests.NUnit.TestData.europe.wkt");
            CheckInteriorPointFile(filePath);
            filePath = EmbeddedResourceManager.SaveEmbeddedResourceToTempFile("NetTopologySuite.Tests.NUnit.TestData.africa.wkt");
            CheckInteriorPointFile(filePath);
            //checkInteriorPointFile("../../../../../data/africa.wkt");
        }


        private static void CheckInteriorPointFile(String file)
        {
            var fileRdr = new WKTFileReader(file, new WKTReader());
            CheckInteriorPointFile(Path.GetFileName(file), fileRdr);
        }

        private static void CheckInteriorPointFile(string name, WKTFileReader fileRdr)
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
            Console.WriteLine("\n {0}ms\n", sw.ElapsedMilliseconds);
        }

        private static void CheckInteriorPoint(IGeometry g)
        {
            var ip = g.InteriorPoint;
            Assert.IsTrue(g.Contains(ip));
        }
    }
}