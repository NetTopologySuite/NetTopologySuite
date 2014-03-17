using System;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixtureAttribute]
    public abstract class AbstractPointInRingTest
    {
        [TestAttribute]
        public void TestBox()
        {
            RunPtInRing(Location.Interior, new Coordinate(10, 10),
                "POLYGON ((0 0, 0 20, 20 20, 20 0, 0 0))");
        }

        [TestAttribute]
        public void TestComplexRing()
        {
            RunPtInRing(Location.Interior, new Coordinate(0, 0),
                "POLYGON ((-40 80, -40 -80, 20 0, 20 -100, 40 40, 80 -80, 100 80, 140 -20, 120 140, 40 180,     60 40, 0 120, -20 -20, -40 80))");
        }

        public static string Comb =
            "POLYGON ((0 0, 0 10, 4 5, 6 10, 7 5, 9 10, 10 5, 13 5, 15 10, 16 3, 17 10, 18 3, 25 10, 30 10, 30 0, 15 0, 14 5, 13 0, 9 0, 8 5, 6 0, 0 0))";

        [TestAttribute]
        public void TestComb()
        {
            RunPtInRing(Location.Boundary, new Coordinate(0, 0), Comb);
            RunPtInRing(Location.Boundary, new Coordinate(0, 1), Comb);
            // at vertex 
            RunPtInRing(Location.Boundary, new Coordinate(4, 5), Comb);
            RunPtInRing(Location.Boundary, new Coordinate(8, 5), Comb);

            // on horizontal segment
            RunPtInRing(Location.Boundary, new Coordinate(11, 5), Comb);
            // on vertical segment
            RunPtInRing(Location.Boundary, new Coordinate(30, 5), Comb);
            // on angled segment
            RunPtInRing(Location.Boundary, new Coordinate(22, 7), Comb);



            RunPtInRing(Location.Interior, new Coordinate(1, 5), Comb);
            RunPtInRing(Location.Interior, new Coordinate(5, 5), Comb);
            RunPtInRing(Location.Interior, new Coordinate(1, 7), Comb);



            RunPtInRing(Location.Exterior, new Coordinate(12, 10), Comb);
            RunPtInRing(Location.Exterior, new Coordinate(16, 5), Comb);
            RunPtInRing(Location.Exterior, new Coordinate(35, 5), Comb);
        }

        public static string RepeatedPts =
            "POLYGON ((0 0, 0 10, 2 5, 2 5, 2 5, 2 5, 2 5, 3 10, 6 10, 8 5, 8 5, 8 5, 8 5, 10 10, 10 5, 10 5, 10 5, 10 5, 10 0, 0 0))";

        /*
        * Tests that repeated points are handled correctly
        */
        public void TestRepeatedPts()
        {
            RunPtInRing(Location.Boundary, new Coordinate(0, 0), RepeatedPts);
            RunPtInRing(Location.Boundary, new Coordinate(0, 1), RepeatedPts);

            // at vertex 
            RunPtInRing(Location.Boundary, new Coordinate(2, 5), RepeatedPts);
            RunPtInRing(Location.Boundary, new Coordinate(8, 5), RepeatedPts);
            RunPtInRing(Location.Boundary, new Coordinate(10, 5), RepeatedPts);

            RunPtInRing(Location.Interior, new Coordinate(1, 5), RepeatedPts);
            RunPtInRing(Location.Interior, new Coordinate(3, 5), RepeatedPts);

        }

        abstract protected void RunPtInRing(Location expectedLoc, Coordinate pt, String wkt);

    }
}