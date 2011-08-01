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
    [TestFixture]
    public class PolygonizeTest
    {
        private WKTReader reader = new WKTReader();

        [Test]
        public void Test1()
        {
            DoTest(new String[] { "LINESTRING EMPTY", "LINESTRING EMPTY" },
              new String[] { });
        }

        [Test]
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

        /*
                [Test]
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
            Polygonizer polygonizer = new Polygonizer();
            polygonizer.Add(ToGeometries(inputWKT));
            Compare(ToGeometries(expectedOutputWKT), polygonizer.GetPolygons());
        }

        private void Compare(IList<IGeometry> expectedGeometries,
            IList<IGeometry> actualGeometries)
        {
            Assert.AreEqual(expectedGeometries.Count, actualGeometries.Count,
                "Geometry count, " + actualGeometries);
            foreach (var expectedGeometry in expectedGeometries)
            {
                Assert.IsTrue(Contains(actualGeometries, expectedGeometry),
                    "Not found: " + expectedGeometry + ", " + actualGeometries);
            }
        }

        private bool Contains(IList<IGeometry> geometries, IGeometry g)
        {
            foreach (var element in geometries)
            {
                if (element.EqualsExact(g))
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
                catch (ParseException)
                {
                    NetTopologySuite.Utilities.Assert.ShouldNeverReachHere();
                }
            }

            return geometries;
        }
    }
}
