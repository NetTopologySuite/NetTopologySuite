using System;
using GeoAPI.Geometries;
using NUnit.Framework;
using Coord = NetTopologySuite.Coordinates.Simple.Coordinate;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixture]
    public abstract class AbstractPointInRingTest
    {
        public void TestBox()
        {
            RunPtInRing(Locations.Interior, GeometryUtils.CoordFac.Create(10, 10),
        "POLYGON ((0 0, 0 20, 20 20, 20 0, 0 0))");
        }

        [Test]
        public void TestComplexRing()
        {
            RunPtInRing(Locations.Interior, GeometryUtils.CoordFac.Create(0, 0),
        "POLYGON ((-40 80, -40 -80, 20 0, 20 -100, 40 40, 80 -80, 100 80, 140 -20, 120 140, 40 180,     60 40, 0 120, -20 -20, -40 80))");
        }

        private const string comb =
          "POLYGON ((0 0, 0 10, 4 5, 6 10, 7 5, 9 10, 10 5, 13 5, 15 10, 16 3, 17 10, 18 3, 25 10, 30 10, 30 0, 15 0, 14 5, 13 0, 9 0, 8 5, 6 0, 0 0))";
        [Test]
        public void TestComb()
        {
            RunPtInRing(Locations.Boundary, GeometryUtils.CoordFac.Create(0, 0), comb);
            RunPtInRing(Locations.Boundary, GeometryUtils.CoordFac.Create(0, 1), comb);
            // at vertex 
            RunPtInRing(Locations.Boundary, GeometryUtils.CoordFac.Create(4, 5), comb);
            RunPtInRing(Locations.Boundary, GeometryUtils.CoordFac.Create(8, 5), comb);

            // on horizontal segment
            RunPtInRing(Locations.Boundary, GeometryUtils.CoordFac.Create(11, 5), comb);
            // on vertical segment
            RunPtInRing(Locations.Boundary, GeometryUtils.CoordFac.Create(30, 5), comb);
            // on angled segment
            RunPtInRing(Locations.Boundary, GeometryUtils.CoordFac.Create(22, 7), comb);



            RunPtInRing(Locations.Interior, GeometryUtils.CoordFac.Create(1, 5), comb);
            RunPtInRing(Locations.Interior, GeometryUtils.CoordFac.Create(5, 5), comb);
            RunPtInRing(Locations.Interior, GeometryUtils.CoordFac.Create(1, 7), comb);



            RunPtInRing(Locations.Exterior, GeometryUtils.CoordFac.Create(12, 10), comb);
            RunPtInRing(Locations.Exterior, GeometryUtils.CoordFac.Create(16, 5), comb);
            RunPtInRing(Locations.Exterior, GeometryUtils.CoordFac.Create(35, 5), comb);
        }

        private const String repeatedPts =
          "POLYGON ((0 0, 0 10, 2 5, 2 5, 2 5, 2 5, 2 5, 3 10, 6 10, 8 5, 8 5, 8 5, 8 5, 10 10, 10 5, 10 5, 10 5, 10 5, 10 0, 0 0))";

        /**
         * Tests that repeated points are handled correctly
         * @throws Exception
         */
        [Test]
        public void TestRepeatedPts()
        {
            RunPtInRing(Locations.Boundary, GeometryUtils.CoordFac.Create(0, 0), repeatedPts);
            RunPtInRing(Locations.Boundary, GeometryUtils.CoordFac.Create(0, 1), repeatedPts);

            // at vertex 
            RunPtInRing(Locations.Boundary, GeometryUtils.CoordFac.Create(2, 5), repeatedPts);
            RunPtInRing(Locations.Boundary, GeometryUtils.CoordFac.Create(8, 5), repeatedPts);
            RunPtInRing(Locations.Boundary, GeometryUtils.CoordFac.Create(10, 5), repeatedPts);

            RunPtInRing(Locations.Interior, GeometryUtils.CoordFac.Create(1, 5), repeatedPts);
            RunPtInRing(Locations.Interior, GeometryUtils.CoordFac.Create(3, 5), repeatedPts);

        }

        protected abstract void RunPtInRing(Locations expectedLoc, Coord pt, String wkt);

    }
}