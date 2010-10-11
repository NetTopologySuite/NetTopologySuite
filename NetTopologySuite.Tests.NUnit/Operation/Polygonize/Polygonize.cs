/*
 * The JTS Topology Suite is a collection of Java classes that
 * implement the fundamental operations required to validate a given
 * geo-spatial data set to a known topological specification.
 *
 * Copyright (C) 2001 Vivid Solutions
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * For more information, contact:
 *
 *     Vivid Solutions
 *     Suite #1A
 *     2328 Government Street
 *     Victoria BC  V8T 5G5
 *     Canada
 *
 *     (250)385-6040
 *     www.vividsolutions.com
 */
using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Coordinates;
using NetTopologySuite.Operation.Polygonize;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Polygonize
{

    /**
     * @version 1.7
     */
    [TestFixture]
    public class PolygonizeTest
    {

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
          public void Test2() {
            DoTest(new String[]{

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

        private static void DoTest(String[] inputWKT, String[] expectedOutputWKT)
        {
            Polygonizer<Coordinate> polygonizer = new Polygonizer<Coordinate>();
            polygonizer.Add(ToGeometries(inputWKT));
            Compare(ToGeometries(expectedOutputWKT), polygonizer.Polygons);
        }

        private static void Compare(IEnumerable<IGeometry<Coordinate>> expectedGeometries,
          IEnumerable<IPolygon<Coordinate>> actualGeometries)
        {
            List<IGeometry<Coordinate>> expected = new List<IGeometry<Coordinate>>(expectedGeometries);
            List<IPolygon<Coordinate>> actual = new List<IPolygon<Coordinate>>(actualGeometries);

            Assert.AreEqual(expected.Count, actual.Count, "Geometry count, " + actual);
            foreach (IGeometry<Coordinate> expectedGeometry in expectedGeometries)
            {
                Assert.IsTrue(Contains(actualGeometries, (IGeometry<Coordinate>)expectedGeometry), "Not found: " + expectedGeometry + ", " + actualGeometries);
            }
        }

        private static Boolean Contains(IEnumerable<IPolygon<Coordinate>> geometries, IGeometry<Coordinate> g)
        {
            foreach (IPolygon<Coordinate> element in geometries)
            {
                if (element.EqualsExact(g))
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<IGeometry<Coordinate>> ToGeometries(String[] inputWKT)
        {
            return GeometryUtils.ReadWKT(inputWKT);
        }
    }
}