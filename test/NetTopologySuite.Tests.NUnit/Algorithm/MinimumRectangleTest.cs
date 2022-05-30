using NetTopologySuite.Algorithm;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    /**
     * @version 1.7
     */
    public class MinimumRectanglelTest : GeometryTestCase
    {

        private const double TOL = 1e-10;

        [Test]
        public void TestLengthZero()
        {
            checkMinRectangle("LINESTRING (1 1, 1 1)", "POINT (1 1)");
        }

        [Test]
        public void TestHorizontal()
        {
            checkMinRectangle("LINESTRING (1 1, 3 1, 5 1, 7 1)", "LINESTRING (1 1, 7 1)");
        }

        [Test]
        public void TestVertical()
        {
            checkMinRectangle("LINESTRING (1 1, 1 4, 1 7, 1 9)", "LINESTRING (1 1, 1 9)");
        }

        [Test]
        public void TestBentLine()
        {
            checkMinRectangle("LINESTRING (1 2, 3 8, 9 6)", "POLYGON ((9 6, 7 10, -1 6, 1 2, 9 6))");
        }

        /**
         * Failure case from https://trac.osgeo.org/postgis/ticket/5163
         * @throws Exception
         */
        [Test]
        public void TestFlatDiagonal()
        {
            checkMinRectangle("LINESTRING(-99.48710639268086 34.79029839231914,-99.48370699999998 34.78689899963806,-99.48152167568102 34.784713675318976)",
                "LINESTRING (-99.48710639268086 34.79029839231914, -99.48152167568102 34.784713675318976)");
        }

        private void checkMinRectangle(string wkt, string wktExpected)
        {
            var geom = Read(wkt);
            var actual = MinimumDiameter.GetMinimumRectangle(geom);
            var expected = Read(wktExpected);
            CheckEqual(expected, actual, TOL);
        }


    }

}
