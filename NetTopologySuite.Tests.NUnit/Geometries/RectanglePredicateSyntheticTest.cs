using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    /*
     * Test spatial predicate optimizations for rectangles by
     * synthesizing an exhaustive set of test cases.
     */
    [TestFixtureAttribute]
    public class RectanglePredicateSyntheticTest
    {
        private WKTReader rdr = new WKTReader();
        private GeometryFactory fact = new GeometryFactory();

        double baseX = 10;
        double baseY = 10;
        double rectSize = 20;
        double bufSize = 10;
        double testGeomSize = 10;
        double bufferWidth = 1.0;

        Envelope rectEnv;
        IGeometry rect;

        public RectanglePredicateSyntheticTest()
        {
            rectEnv = new Envelope(baseX, baseX + rectSize, baseY, baseY + rectSize);
            rect = fact.ToGeometry(rectEnv);
        }

        [TestAttribute]
        public void TestLines()
        {
            Console.WriteLine(rect);

            List<IGeometry> testGeoms = getTestGeometries();
            foreach (var testGeom in testGeoms)
            {
                runRectanglePredicates(rect, testGeom);
            }
        }

        [TestAttribute]
        public void TestDenseLines()
        {
            Console.WriteLine(rect);

            var testGeoms = getTestGeometries();
            foreach (var testGeom in testGeoms)
            {
                SegmentDensifier densifier = new SegmentDensifier((LineString)testGeom);
                LineString denseLine = (LineString)densifier.Densify(testGeomSize / 400);

                runRectanglePredicates(rect, denseLine);
            }
        }

        [TestAttribute]
        public void TestPolygons()
        {
            var testGeoms = getTestGeometries();
            foreach (var testGeom in testGeoms)
            {
                runRectanglePredicates(rect, testGeom.Buffer(bufferWidth));
            }
        }

        private List<IGeometry> getTestGeometries()
        {
            Envelope testEnv = new Envelope(rectEnv.MinX - bufSize, rectEnv.MaxX + bufSize,
                                            rectEnv.MinY - bufSize, rectEnv.MaxY + bufSize);
            var testGeoms = CreateTestGeometries(testEnv, 5, testGeomSize);
            return testGeoms;
        }

        private void runRectanglePredicates(IGeometry rect, IGeometry testGeom)
        {
            bool intersectsValue = rect.Intersects(testGeom);
            bool relateIntersectsValue = rect.Relate(testGeom).IsIntersects();
            bool intersectsOK = intersectsValue == relateIntersectsValue;

            bool containsValue = rect.Contains(testGeom);
            bool relateContainsValue = rect.Relate(testGeom).IsContains();
            bool containsOK = containsValue == relateContainsValue;

            //System.out.println(testGeom);
            if (!intersectsOK || !containsOK)
            {
                Console.WriteLine(testGeom);
            }
            Assert.IsTrue(intersectsOK);
            Assert.IsTrue(containsOK);
        }

        public List<IGeometry> CreateTestGeometries(Envelope env, double inc, double size)
        {
            var testGeoms = new List<IGeometry>();

            for (double y = env.MinY; y <= env.MaxY; y += inc)
            {
                for (double x = env.MinX; x <= env.MaxX; x += inc)
                {
                    Coordinate baseCoord = new Coordinate(x, y);
                    testGeoms.Add(CreateAngle(baseCoord, size, 0));
                    testGeoms.Add(CreateAngle(baseCoord, size, 1));
                    testGeoms.Add(CreateAngle(baseCoord, size, 2));
                    testGeoms.Add(CreateAngle(baseCoord, size, 3));
                }
            }
            return testGeoms;
        }

        public IGeometry CreateAngle(Coordinate baseCoord, double size, int quadrant)
        {
            var factor = new int[,] {
                                    { 1, 0 },
                                    { 0, 1 },
                                    { -1, 0 },
                                    { 0, -1 }
                                    };

            int xFac = factor[quadrant, 0];
            int yFac = factor[quadrant, 1];

            Coordinate p0 = new Coordinate(baseCoord.X + xFac * size, baseCoord.Y + yFac * size);
            Coordinate p2 = new Coordinate(baseCoord.X + yFac * size, baseCoord.Y + (-xFac) * size);

            return fact.CreateLineString(new Coordinate[] { p0, baseCoord, p2 });
        }
    }
}