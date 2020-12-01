using System;
using System.Diagnostics;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.Buffer
{
    /**
     * Test repeated buffering of a given input shape.
     * Intended to test the robustness of buffering.
     * Repeated buffering tends to generate challenging
     * somewhat pathological linework, which stresses the buffer algorithm.
     * 
     * @version 1.7
     */
    public class IteratedBufferStressTest
    {

        private readonly PrecisionModel _precisionModel; 
        private readonly GeometryFactory _geometryFactory;
        private readonly WKTReader _rdr;

        private const string inputWKT =
            "POLYGON ((110 320, 190 220, 60 200, 180 120, 120 40, 290 150, 410 40, 410 230, 500 340, 320 310, 260 370, 220 310, 110 320), (220 260, 250 180, 290 220, 360 150, 350 250, 260 280, 220 260))";

        public IteratedBufferStressTest()
        {
            _precisionModel = new PrecisionModel();
            _geometryFactory = new GeometryFactory(_precisionModel, 0);
            _rdr = new WKTReader(_geometryFactory);
        }

        [Test]
        public void Run()
        {
            var totalSW = new Stopwatch();
            var @base = _rdr.Read(inputWKT);
            double dist = 1.0;
            while (!@base.IsEmpty)
            {
                var b1 = DoBuffer(@base, dist);
                var b2 = DoBuffer(b1, -dist);
                dist += 1;
                @base = b2;
                TestContext.WriteLine("----------------------  " + totalSW.Elapsed);
                TestContext.WriteLine();
            }
        }

        private static Geometry DoBuffer(Geometry g, double dist)
        {
            TestContext.WriteLine("Buffering with dist = " + dist);
            var buf = g.Buffer(dist);
            TestContext.WriteLine("Buffer result has " + buf.NumPoints + " vertices");

            TestContext.WriteLine(buf);
            return buf;

        }
    }
}
