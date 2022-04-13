using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Linemerge;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.LineMerge
{
    [TestFixture]
    public class LineMergerTest
    {
        private static WKTReader reader = new WKTReader();

        [Test]
        public void Test1()
        {
            DoTest(new string[] {
                "LINESTRING (120 120, 180 140)", "LINESTRING (200 180, 180 140)",
                "LINESTRING (200 180, 240 180)"
              }, new string[] { "LINESTRING (120 120, 180 140, 200 180, 240 180)" });
        }

        [Test]
        public void Test2()
        {
            DoTest(new string[]{"LINESTRING (120 300, 80 340)",
              "LINESTRING (120 300, 140 320, 160 320)",
              "LINESTRING (40 320, 20 340, 0 320)",
              "LINESTRING (0 320, 20 300, 40 320)",
              "LINESTRING (40 320, 60 320, 80 340)",
              "LINESTRING (160 320, 180 340, 200 320)",
              "LINESTRING (200 320, 180 300, 160 320)"},
              new string[]{
              "LINESTRING (160 320, 180 340, 200 320, 180 300, 160 320)",
              "LINESTRING (40 320, 20 340, 0 320, 20 300, 40 320)",
              "LINESTRING (40 320, 60 320, 80 340, 120 300, 140 320, 160 320)"});
        }

        [Test]
        public void Test3()
        {
            DoTest(new string[] { "LINESTRING (0 0, 100 100)", "LINESTRING (0 100, 100 0)" },
              new string[] { "LINESTRING (0 0, 100 100)", "LINESTRING (0 100, 100 0)" });
        }

        [Test]
        public void Test4()
        {
            DoTest(new string[] { "LINESTRING EMPTY", "LINESTRING EMPTY" },
              new string[] { });
        }

        [Test]
        public void Test5()
        {
            DoTest(new string[] { },
              new string[] { });
        }

        [Test]
        public void TestSequenceOnReverseInputDoesNotChangeInput()
        {
            string[] wkt =
            {
                "LINESTRING (0 6, 0 5)",
                "LINESTRING (0 3, 0 6)",
                "LINESTRING (0 8, 0 3)",
            };

            var input = ToGeometries(wkt);
            var expected = ToGeometries(wkt);
            var sequencer = new LineSequencer();
            sequencer.Add(input);

            _ = sequencer.GetSequencedLineStrings();

            // The input should not have been changed
            LineMergerTest.Compare(expected, input, true);
        }

        [Ignore("This test is currently failing due to the generalization of the line (removal of repeated points) in the AddEdge method of NetTopologySuite.Operation.Linemerge.LineMergeGraph.  Need to see if this test is working in JTS")]
        public void TestSingleUniquePoint()
        {
            DoTest(new string[] { "LINESTRING (10642 31441, 10642 31441)", "LINESTRING EMPTY" },
                new string[] { });
        }

        private void DoTest(string[] inputWKT, string[] expectedOutputWKT)
        {
            DoTest(inputWKT, expectedOutputWKT, true);
        }

        public static void DoTest(string[] inputWKT, string[] expectedOutputWKT, bool compareDirections)
        {
            var lineMerger = new LineMerger();
            lineMerger.Add(ToGeometries(inputWKT));
            Compare(ToGeometries(expectedOutputWKT), lineMerger.GetMergedLineStrings(), compareDirections);
        }

        public static void Compare(IList<Geometry> expectedGeometries,
            IList<Geometry> actualGeometries, bool compareDirections)
        {
            Assert.AreEqual(expectedGeometries.Count, actualGeometries.Count, "Geometry count, " + actualGeometries);
            foreach (var expectedGeometry in expectedGeometries)
            {
                Assert.IsTrue(Contains(actualGeometries, expectedGeometry, compareDirections), "Not found: " + expectedGeometry + ", " + actualGeometries);
            }
        }

        private static bool Contains(IList<Geometry> geometries, Geometry g, bool exact)
        {
            foreach (var element in geometries)
            {
                if (exact && element.EqualsExact(g))
                {
                    return true;
                }
                if (!exact && element.Equals(g))
                {
                    return true;
                }
            }

            return false;
        }

        public static IList<Geometry> ToGeometries(string[] inputWKT)
        {
            var geometries = new List<Geometry>();
            foreach (string geomWkt in inputWKT)
            {
                try
                {
                    geometries.Add(reader.Read(geomWkt));
                }
                catch (ParseException)
                {
                    NetTopologySuite.Utilities.Assert.ShouldNeverReachHere();
                }
            }

            return geometries;
        }
    }
}
