using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.OverlayNG;
using NetTopologySuite.Tests.NUnit.Utilities;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.OverlayNG
{
    public class ElevationModelTest : GeometryTestCase
    {


        private const double TOLERANCE = 0.00001;

        [Test]
        public void TestBox()
        {
            CheckElevation("POLYGON Z ((1 6 50, 9 6 60, 9 4 50, 1 4 40, 1 6 50))",
                0, 10, 50, 5, 10, 50, 10, 10, 60,
                0, 5, 50, 5, 5, 50, 10, 5, 50,
                0, 4, 40, 5, 4, 50, 10, 4, 50,
                0, 0, 40, 5, 0, 50, 10, 0, 50
            );
        }

        [Test]
        public void TestLine()
        {
            CheckElevation("LINESTRING Z (0 0 0, 10 10 10)",
                -1, 11, 5, 11, 11, 10,
                0, 10, 5, 5, 10, 5, 10, 10, 10,
                0, 5, 5, 5, 5, 5, 10, 5, 5,
                0, 0, 0, 5, 0, 5, 10, 0, 5,
                -1, -1, 0, 5, -1, 5, 11, -1, 5
            );
        }

        [Test]
        public void TestPopulateZLine()
        {
            CheckElevationPopulateZ("LINESTRING Z (0 0 0, 10 10 10)",
                "LINESTRING (1 1, 9 9)",
                "LINESTRING (1 1 0, 9 9 10)"
            );
        }

        [Test]
        public void TestPopulateZBox()
        {
            CheckElevationPopulateZ("LINESTRING Z (0 0 0, 10 10 10)",
                "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9))",
                "POLYGON Z ((1 1 0, 1 9 5, 9 9 10, 9 1 5, 1 1 0))"
            );
        }

        [Test]
        public void TestMultiLine()
        {
            CheckElevation("MULTILINESTRING Z ((0 0 0, 10 10 8), (1 2 2, 9 8 6))",
                -1, 11, 4, 11, 11, 7,
                0, 10, 4, 5, 10, 4, 10, 10, 7,
                0, 5, 4, 5, 5, 4, 10, 5, 4,
                0, 0, 1, 5, 0, 4, 10, 0, 4,
                -1, -1, 1, 5, -1, 4, 11, -1, 4
            );
        }

        [Test]
        public void TestTwoLines()
        {
            CheckElevation("LINESTRING Z (0 0 0, 10 10 8)",
                "LINESTRING Z (1 2 2, 9 8 6))",
                -1, 11, 4, 11, 11, 7,
                0, 10, 4, 5, 10, 4, 10, 10, 7,
                0, 5, 4, 5, 5, 4, 10, 5, 4,
                0, 0, 1, 5, 0, 4, 10, 0, 4,
                -1, -1, 1, 5, -1, 4, 11, -1, 4
            );
        }

        /**
         * Tests that XY geometries are scanned correctly (avoiding reading Z)
         * and that they produce a model Z value of NaN
         */
        [Test]
        public void TestLine2D()
        {
            // LINESTRING (0 0, 10 10)
            CheckElevation("0102000000020000000000000000000000000000000000000000000000000024400000000000002440",
                5, 5, double.NaN
            );
        }

        [Test]
        public void TestLineHorizontal()
        {
            CheckElevation("LINESTRING Z (0 5 0, 10 5 10)",
                0, 10, 0, 5, 10, 5, 10, 10, 10,
                0, 5, 0, 5, 5, 5, 10, 5, 10,
                0, 0, 0, 5, 0, 5, 10, 0, 10
            );
        }

        [Test]
        public void TestLineVertical()
        {
            CheckElevation("LINESTRING Z (5 0 0, 5 10 10)",
                0, 10, 10, 5, 10, 10, 10, 10, 10,
                0, 5, 5, 5, 5, 5, 10, 5, 5,
                0, 0, 0, 5, 0, 0, 10, 0, 0
            );
        }

        // tests that single point Z is used for entire grid and beyond
        [Test]
        public void TestPoint()
        {
            CheckElevation("POINT Z (5 5 5)",
                0, 9, 5, 5, 9, 5, 9, 9, 5,
                0, 5, 5, 5, 5, 5, 9, 5, 5,
                0, 0, 5, 5, 0, 5, 9, 0, 5
            );
        }

        // tests that Z is average of input points with same location
        [Test]
        public void TestMultiPointSame()
        {
            CheckElevation("MULTIPOINT Z ((5 5 5), (5 5 9))",
                0, 9, 7, 5, 9, 7, 9, 9, 7,
                0, 5, 7, 5, 5, 7, 9, 5, 7,
                0, 0, 7, 5, 0, 7, 9, 0, 7
            );
        }

        private void CheckElevation(string wkt1, string wkt2, params double[] ords)
        {
            CheckElevation(Read(wkt1), Read(wkt2), ords);
        }

        private void CheckElevation(string wkt1, params double[] ords)
        {
            CheckElevation(Read(wkt1), null, ords);
        }


        private void CheckElevation(Geometry geom1, Geometry geom2, double[] ords)
        {
            var model = ElevationModel.Create(geom1, geom2);
            int numPts = ords.Length / 3;
            if (3 * numPts != ords.Length)
            {
                throw new ArgumentException("Incorrect number of ordinates");
            }

            for (int i = 0; i < numPts; i++)
            {
                double x = ords[3 * i];
                double y = ords[3 * i + 1];
                double expectedZ = ords[3 * i + 2];
                double actualZ = model.GetZ(x, y);
                string msg = "Point ( " + x + ", " + y + " ) : ";
                Assert.That(actualZ, Is.EqualTo(expectedZ).Within(TOLERANCE), msg);
            }
        }

        private void CheckElevationPopulateZ(string wkt, string wktNoZ, string wktZExpected)
        {
            var geom = Read(wkt);
            var model = ElevationModel.Create(geom, null);

            var geomNoZ = AddZDimension.Do(Read(wktNoZ));
            model.PopulateZ(geomNoZ);

            var geomZExpected = Read(wktZExpected);
            CheckEqualXYZ(geomZExpected, geomNoZ);
        }
    }

}
