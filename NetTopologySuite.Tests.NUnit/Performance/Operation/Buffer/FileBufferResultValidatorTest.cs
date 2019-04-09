using System;
using System.Collections.Generic;
using System.IO;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Buffer.Validate;
using NetTopologySuite.Tests.NUnit.TestData;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.Buffer
{
    [TestFixture]
    [Category("Stress")]
    public class FileBufferResultValidatorTest
    {
        private const int MAX_FEATURE = 1;
        WKTReader rdr = new WKTReader();

        [Test]
        public void TestAfrica()
        {
            using (var file = EmbeddedResourceManager.GetResourceStream("NetTopologySuite.Tests.NUnit.TestData.africa.wkt"))
            {
                //    runTest(TestFiles.DATA_DIR + "world.wkt");
                RunTest(file);
            }
        }

        [Test]
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
            var fileRdr = new WKTFileReader(new StreamReader(file), rdr);
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
            var fileRdr = new WKTFileReader(new StreamReader(file), rdr);
            var polys = fileRdr.Read();

            //RunAll(polys, 0.01);
            //RunAll(polys, 0.1);
            RunAll(polys, 1.0);
            //RunAll(polys, 10.0);
            //RunAll(polys, 100.0);
            //RunAll(polys, 1000.0);
        }

        void RunAll(ICollection<IGeometry> geoms, double dist)
        {
            //var sw = new Stopwatch();
            //sw.Start();
            int count = 0;
            //System.Console.WriteLine("Geom count = " + geoms.Count + "   distance = " + dist);
            foreach (var g in geoms)
            {
                RunBuffer(g, dist);
                RunBuffer(g.Reverse(), dist);
                //System.Console.WriteLine(".");
                count++;
                if (count > MAX_FEATURE) break;
            }
            //sw.Stop();
            //System.Console.WriteLine("  " + sw.Elapsed.TotalMilliseconds + " milliseconds");
        }

        static void RunBuffer(IGeometry g, double dist)
        {
            var buf = g.Buffer(dist);
            var validator = new BufferResultValidator(g, dist, buf);

            if (!validator.IsValid())
            {
                string msg = validator.ErrorMessage;

                Console.WriteLine(msg);
                Console.WriteLine(WKTWriter.ToPoint(validator.ErrorLocation));
                Console.WriteLine(g);
            }
            Assert.IsTrue(validator.IsValid());
        }
    }
}
