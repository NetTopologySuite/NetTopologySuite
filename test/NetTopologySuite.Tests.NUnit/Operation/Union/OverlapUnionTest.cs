using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Union;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Union
{
    [Obsolete("OverlapUnion is obsolete due to impairing performance")]
    public class OverlapUnionTest : GeometryTestCase
    {
        [Test]
        public void TestFixedPrecCausingBorderChange()
        {

            const string a = "POLYGON ((130 -10, 20 -10, 20 22, 30 20, 130 20, 130 -10))";
            const string b = "MULTIPOLYGON (((50 0, 100 450, 100 0, 50 0)), ((53 28, 50 28, 50 30, 53 30, 53 28)))";

            CheckUnionWithTopologyFailure(a, b, 1);
        }

        [Test]

        public void TestFullPrecision()
        {

            const string a = "POLYGON ((130 -10, 20 -10, 20 22, 30 20, 130 20, 130 -10))";
            const string b = "MULTIPOLYGON (((50 0, 100 450, 100 0, 50 0)), ((53 28, 50 28, 50 30, 53 30, 53 28)))";

            CheckUnion(a, b);
        }

        [Test]

        public void TestSimpleOverlap()
        {

            const string a =
                "MULTIPOLYGON (((0 400, 50 400, 50 350, 0 350, 0 400)), ((200 200, 220 200, 220 180, 200 180, 200 200)), ((350 100, 370 100, 370 80, 350 80, 350 100)))";
            const string b =
                "MULTIPOLYGON (((430 20, 450 20, 450 0, 430 0, 430 20)), ((100 300, 124 300, 124 276, 100 276, 100 300)), ((230 170, 210 170, 210 190, 230 190, 230 170)))";

            CheckUnionOptimized(a, b);
        }

        /// <summary>
        /// It is hard to create a situation where border segments change by
        /// enough to cause an invalid geometry to be returned.
        /// One way is to use a fixed precision model,
        /// which will cause segments to move enough to
        /// intersect with non-overlapping components.
        /// <para/>
        /// However, the current union algorithm
        /// emits topology failures for these situations, since
        /// it is not performing snap-rounding.
        /// These exceptions are irrelevant to the correctness
        /// of the OverlapUnion algorithm, so are prevented from being reported as a test failure.
        /// </summary>
        /// <param name="wktA"></param>
        /// <param name="wktB"></param>
        /// <param name="scaleFactor"></param>
        private static void CheckUnionWithTopologyFailure(string wktA, string wktB, double scaleFactor)
        {
            var rdr = new WKTReader(new NtsGeometryServices(new PrecisionModel(scaleFactor), 0));

            var a = rdr.Read(wktA);
            var b = rdr.Read(wktB);

            var union = new OverlapUnion(a, b);

            Geometry result;
            try
            {
                result = union.Union();
            }
            catch (TopologyException)
            {
                bool isOptimized = union.IsUnionOptimized;

                // if the optimized algorithm was used then this is a real error
                if (isOptimized) throw;

                // otherwise the error is probably due to the fixed precision
                // not being handled by the current union code
                return;
            }

            Assert.IsTrue(result.IsValid, "OverlapUnion result is invalid");
        }

        private static void CheckUnion(string wktA, string wktB)
        {
            CheckUnion(wktA, wktB, false);
        }

        private static void CheckUnionOptimized(string wktA, string wktB)
        {
            CheckUnion(wktA, wktB, true);
        }

        private static void CheckUnion(string wktA, string wktB, bool isCheckOptimized)
        {
            var rdr = new WKTReader();

            var a = rdr.Read(wktA);
            var b = rdr.Read(wktB);

            var union = new OverlapUnion(a, b);
            var result = union.Union();

            if (isCheckOptimized)
            {
                bool isOptimized = union.IsUnionOptimized;
                Assert.IsTrue(isOptimized, "Union was not performed using combine");
            }

            Assert.IsTrue(result.IsValid, "OverlapUnion result is invalid");
        }
    }

}
