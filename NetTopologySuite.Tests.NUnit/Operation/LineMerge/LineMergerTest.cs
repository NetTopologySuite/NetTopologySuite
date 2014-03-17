using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;
using NetTopologySuite.Operation.Linemerge;

namespace NetTopologySuite.Tests.NUnit.Operation.LineMerge
{
    [TestFixtureAttribute]
    public class LineMergerTest
    {
        private static WKTReader reader = new WKTReader();

        [TestAttribute]
        public void Test1()
        {
            DoTest(new String[] {
                "LINESTRING (120 120, 180 140)", "LINESTRING (200 180, 180 140)",
                "LINESTRING (200 180, 240 180)"
              }, new String[] { "LINESTRING (120 120, 180 140, 200 180, 240 180)" });
        }

        [TestAttribute]
        public void Test2()
        {
            DoTest(new String[]{"LINESTRING (120 300, 80 340)",
              "LINESTRING (120 300, 140 320, 160 320)",
              "LINESTRING (40 320, 20 340, 0 320)",
              "LINESTRING (0 320, 20 300, 40 320)",
              "LINESTRING (40 320, 60 320, 80 340)",
              "LINESTRING (160 320, 180 340, 200 320)",
              "LINESTRING (200 320, 180 300, 160 320)"},
              new String[]{
              "LINESTRING (160 320, 180 340, 200 320, 180 300, 160 320)",
              "LINESTRING (40 320, 20 340, 0 320, 20 300, 40 320)",
              "LINESTRING (40 320, 60 320, 80 340, 120 300, 140 320, 160 320)"});
        }

        [TestAttribute]
        public void Test3()
        {
            DoTest(new String[] { "LINESTRING (0 0, 100 100)", "LINESTRING (0 100, 100 0)" },
              new String[] { "LINESTRING (0 0, 100 100)", "LINESTRING (0 100, 100 0)" });
        }

        [TestAttribute]
        public void Test4()
        {
            DoTest(new String[] { "LINESTRING EMPTY", "LINESTRING EMPTY" },
              new String[] { });
        }

        [TestAttribute]
        public void Test5()
        {
            DoTest(new String[] { },
              new String[] { });
        }

        [Ignore("This test is currently failing due to the generalization of the line (removal of repeated points) in the AddEdge method of NetTopologySuite.Operation.Linemerge.LineMergeGraph.  Need to see if this test is working in JTS")]
        public void TestSingleUniquePoint()
        {
            DoTest(new String[] { "LINESTRING (10642 31441, 10642 31441)", "LINESTRING EMPTY" },
                new String[] { });
        }

        private void DoTest(String[] inputWKT, String[] expectedOutputWKT)
        {
            DoTest(inputWKT, expectedOutputWKT, true);
        }

        public static void DoTest(String[] inputWKT, String[] expectedOutputWKT, bool compareDirections)
        {
            LineMerger lineMerger = new LineMerger();
            lineMerger.Add(ToGeometries(inputWKT));
            Compare(ToGeometries(expectedOutputWKT), lineMerger.GetMergedLineStrings(), compareDirections);
        }

        public static void Compare(IList<IGeometry> expectedGeometries,
            IList<IGeometry> actualGeometries, bool compareDirections)
        {
            Assert.AreEqual(expectedGeometries.Count, actualGeometries.Count, "Geometry count, " + actualGeometries);
            foreach (var expectedGeometry in expectedGeometries)
            {
                Assert.IsTrue(Contains(actualGeometries, expectedGeometry, compareDirections), "Not found: " + expectedGeometry + ", " + actualGeometries);
            }
        }

        private static bool Contains(IList<IGeometry> geometries, IGeometry g, bool exact)
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

        public static IList<IGeometry> ToGeometries(String[] inputWKT)
        {
            var geometries = new List<IGeometry>();
            foreach (var geomWkt in inputWKT)
            {
                try
                {
                    geometries.Add(reader.Read(geomWkt));
                }
                catch (GeoAPI.IO.ParseException e)
                {
                    NetTopologySuite.Utilities.Assert.ShouldNeverReachHere();
                }
            }

            return geometries;
        }
    }
}
