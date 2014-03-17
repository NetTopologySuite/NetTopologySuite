using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Polygonize;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Polygonize
{
    [TestFixtureAttribute]
    public class PolygonizeTest
    {
        private WKTReader reader = new WKTReader();

        [TestAttribute]
        public void Test1()
        {
            DoTest(new String[] { "LINESTRING EMPTY", "LINESTRING EMPTY" },
              new String[] { });
        }

        [TestAttribute]
        public void Test2()
        {
            DoTest(new String[]{
                "LINESTRING (100 180, 20 20, 160 20, 100 180)",
                "LINESTRING (100 180, 80 60, 120 60, 100 180)",
            },
            new String[]{
                "POLYGON ((100 180, 120 60, 80 60, 100 180))",
                "POLYGON ((100 180, 160 20, 20 20, 100 180), (100 180, 80 60, 120 60, 100 180))"
            });
        }

        public void test3()
        {
            DoTest(new String[]{
        "LINESTRING (0 0, 4 0)",
        "LINESTRING (4 0, 5 3)",
"LINESTRING (5 3, 4 6, 6 6, 5 3)",
"LINESTRING (5 3, 6 0)",
"LINESTRING (6 0, 10 0, 5 10, 0 0)",
"LINESTRING (4 0, 6 0)"
    },
            new String[]{
"POLYGON ((5 3, 4 0, 0 0, 5 10, 10 0, 6 0, 5 3), (5 3, 6 6, 4 6, 5 3))",
"POLYGON ((5 3, 4 6, 6 6, 5 3))",
"POLYGON ((4 0, 5 3, 6 0, 4 0))"
    });
        }

        /*
                [TestAttribute]
                public void Test2() {
            doTest(new String[]{

        "LINESTRING(20 20, 20 100)",
        "LINESTRING  (20 100, 20 180, 100 180)",
        "LINESTRING  (100 180, 180 180, 180 100)",
        "LINESTRING  (180 100, 180 20, 100 20)",
        "LINESTRING  (100 20, 20 20)",
        "LINESTRING  (100 20, 20 100)",
        "LINESTRING  (20 100, 100 180)",
        "LINESTRING  (100 180, 180 100)",
        "LINESTRING  (180 100, 100 20)"
            },
              new String[]{});
          }
        */

        private void DoTest(String[] inputWKT, String[] expectedOutputWKT)
        {
            var polygonizer = new Polygonizer();
            polygonizer.Add(ToGeometries(inputWKT));
            Compare(ToGeometries(expectedOutputWKT), polygonizer.GetPolygons());
        }

        private void Compare(ICollection<IGeometry> expectedGeometries,
            ICollection<IGeometry> actualGeometries)
        {
            Assert.AreEqual(expectedGeometries.Count, actualGeometries.Count,
                "Geometry count - expected " + expectedGeometries.Count
        + " but actual was " + actualGeometries.Count
        + " in " + actualGeometries);
            foreach (var expectedGeometry in expectedGeometries)
            {
                Assert.IsTrue(Contains(actualGeometries, expectedGeometry),
                    "Expected to find: " + expectedGeometry + " in Actual result:" + actualGeometries);
            }
        }

        private static bool Contains(IEnumerable<IGeometry> geometries, IGeometry g)
        {
            foreach (var element in geometries)
            {
                if (element.EqualsNormalized(g))
                {
                    return true;
                }
            }

            return false;
        }

        private IList<IGeometry> ToGeometries(String[] inputWKT)
        {
            var geometries = new List<IGeometry>();
            foreach (var geomWkt in inputWKT)
            {
                try
                {
                    geometries.Add(reader.Read(geomWkt));
                }
                catch (GeoAPI.IO.ParseException)
                {
                    NetTopologySuite.Utilities.Assert.ShouldNeverReachHere();
                }
            }

            return geometries;
        }
    }
}
