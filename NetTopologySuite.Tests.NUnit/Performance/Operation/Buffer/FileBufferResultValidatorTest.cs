using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            using (var file = EmbeddedResourceManager.GetResourceStream("NetTopologySuite.Tests.NUnit.TestData.africa.wkt"))
            {
                //    runTest(TestFiles.DATA_DIR + "world.wkt");
                RunTest(file);
            }
        }

        [TestAttribute]
        public void TestPerformanceAfrica()
        {
            using (var file = EmbeddedResourceManager.GetResourceStream("NetTopologySuite.Tests.NUnit.TestData.africa.wkt"))
            {
                //    runTest(TestFiles.DATA_DIR + "world.wkt");
                PerformanceTest(file);
            }
        }

        void RunTest(Stream file)
        {
            WKTFileReader fileRdr = new WKTFileReader(new StreamReader(file), rdr);
            var polys = fileRdr.Read();

            RunAll(polys, 0.01);
            RunAll(polys, 0.1);
            RunAll(polys, 1.0);
            RunAll(polys, 10.0);
            RunAll(polys, 100.0);
            RunAll(polys, 1000.0);
        }

        void PerformanceTest(Stream file)
        {
            WKTFileReader fileRdr = new WKTFileReader(new StreamReader(file), rdr);
            IList<IGeometry> polys = fileRdr.Read();

            //RunAll(polys, 0.01);
            //RunAll(polys, 0.1);
            RunAll(polys, 1.0);
            //RunAll(polys, 10.0);
            //RunAll(polys, 100.0);
            //RunAll(polys, 1000.0);
        }

        void RunAll(ICollection<IGeometry> geoms, double dist)
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

        static void RunBuffer(IGeometry g, double dist)
        {
            var buf = g.Buffer(dist);
            var validator = new BufferResultValidator(g, dist, buf);

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
