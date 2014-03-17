using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Buffer.Validate;
using NetTopologySuite.Tests.NUnit.TestData;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.Buffer
{
    [TestFixtureAttribute]
    [CategoryAttribute("Stress")]
    public class FileBufferResultValidatorTest
    {
        WKTReader rdr = new WKTReader();

        [TestAttribute]
        public void TestAfrica()
        {
            var filePath = EmbeddedResourceManager.SaveEmbeddedResourceToTempFile("NetTopologySuite.Tests.NUnit.TestData.africa.wkt");

            //    runTest(TestFiles.DATA_DIR + "world.wkt");
            RunTest(filePath);

            EmbeddedResourceManager.CleanUpTempFile(filePath);
        }

        [TestAttribute]
        public void TestPerformanceAfrica()
        {
            var filePath = EmbeddedResourceManager.SaveEmbeddedResourceToTempFile("NetTopologySuite.Tests.NUnit.TestData.africa.wkt");

            //    runTest(TestFiles.DATA_DIR + "world.wkt");
            PerformanceTest(filePath);

            EmbeddedResourceManager.CleanUpTempFile(filePath);
        }

        void RunTest(String filename)
        {
            WKTFileReader fileRdr = new WKTFileReader(filename, rdr);
            IList<IGeometry> polys = fileRdr.Read();

            RunAll(polys, 0.01);
            RunAll(polys, 0.1);
            RunAll(polys, 1.0);
            RunAll(polys, 10.0);
            RunAll(polys, 100.0);
            RunAll(polys, 1000.0);
        }

        void PerformanceTest(String filename)
        {
            WKTFileReader fileRdr = new WKTFileReader(filename, rdr);
            IList<IGeometry> polys = fileRdr.Read();

            //RunAll(polys, 0.01);
            //RunAll(polys, 0.1);
            RunAll(polys, 1.0);
            //RunAll(polys, 10.0);
            //RunAll(polys, 100.0);
            //RunAll(polys, 1000.0);
        }

        void RunAll(IList<IGeometry> geoms, double dist)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Console.WriteLine("Geom count = " + geoms.Count + "   distance = " + dist);
            foreach (var g in geoms)
            {
                RunBuffer(g, dist);
                //RunBuffer(g.reverse(), dist);
                Console.Write(".");
            }
            sw.Stop();
            Console.WriteLine("  " + sw.Elapsed.TotalMilliseconds + " milliseconds");
        }

        void RunBuffer(IGeometry g, double dist)
        {
            IGeometry buf = g.Buffer(dist);
            BufferResultValidator validator = new BufferResultValidator(g, dist, buf);

            if (!validator.IsValid())
            {
                String msg = validator.ErrorMessage;

                Console.WriteLine(msg);
                Console.WriteLine(WKTWriter.ToPoint(validator.ErrorLocation));
                Console.WriteLine(g);
            }
            Assert.IsTrue(validator.IsValid());
        }
    }
}
