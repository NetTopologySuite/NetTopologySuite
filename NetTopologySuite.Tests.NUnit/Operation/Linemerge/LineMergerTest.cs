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
using NetTopologySuite.Operation.Linemerge;
using NetTopologySuite.Coordinates.Simple;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Linemerge
{


    /**
     * @version 1.7
     */
    [TestFixture]
    public class LineMergerTest
    {
        [Test]
        public void Test1()
        {
            DoTest(new String[] {
        "LINESTRING (120 120, 180 140)", "LINESTRING (200 180, 180 140)",
        "LINESTRING (200 180, 240 180)"
      }, new String[] { "LINESTRING (120 120, 180 140, 200 180, 240 180)" });
        }

        [Test]
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

        [Test]
        public void Test3()
        {
            DoTest(new String[] { "LINESTRING (0 0, 100 100)", "LINESTRING (0 100, 100 0)" },
              new String[] { "LINESTRING (0 0, 100 100)", "LINESTRING (0 100, 100 0)" });
        }

        [Test]
        public void Test4()
        {
            DoTest(new String[] { "LINESTRING EMPTY", "LINESTRING EMPTY" },
              new String[] { });
        }

        [Test]
        public void Test5()
        {
            DoTest(new String[] { },
              new String[] { });
        }

        private static void DoTest(String[] inputWKT, String[] expectedOutputWKT)
        {
            DoTest(inputWKT, expectedOutputWKT, true);
        }

        public static void DoTest(String[] inputWKT, String[] expectedOutputWKT, Boolean compareDirections)
        {
            LineMerger<Coordinate> lineMerger = new LineMerger<Coordinate>();
            lineMerger.Add(ToGeometries(inputWKT));
            Compare(ToGeometries(expectedOutputWKT), lineMerger.MergedLineStrings, compareDirections);
        }

        public static void Compare(IEnumerable<IGeometry<Coordinate>> expectedGeometries,
          IEnumerable<ILineString<Coordinate>> actualGeometries, Boolean compareDirections)
        {
            List<IGeometry<Coordinate>> expected = new List<IGeometry<Coordinate>>(expectedGeometries);
            List<ILineString<Coordinate>> actual = new List<ILineString<Coordinate>>(actualGeometries);

            Assert.AreEqual(expected.Count, actual.Count, "Geometry count, " + actualGeometries);
            foreach (IGeometry<Coordinate> expectedGeometry in expectedGeometries)
            {
                Assert.IsTrue(
                  Contains(actual, expectedGeometry, compareDirections), "Not found: " + expectedGeometry + ", " + actualGeometries);
            }
        }

        private static Boolean Contains(IEnumerable<ILineString<Coordinate>> geometries, IGeometry<Coordinate> g, Boolean exact)
        {
            foreach (IGeometry<Coordinate> element in geometries)
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

        public static IEnumerable<IGeometry<Coordinate>> ToGeometries(String[] inputWKT)
        {
            for (int i = 0; i < inputWKT.Length; i++)
                yield return GeometryUtils.Reader.Read(inputWKT[i]);
            /*ArrayList geometries = new ArrayList();
          for (int i = 0; i < inputWKT.length; i++) {
            try {
                yield return GeometryUtils.Reader.Read(inputWKT[i]);
            } catch (ParseException e) {
              Assert.shouldNeverReachHere();
            }
          }

          return geometries;*/
        }
    }
}