using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Distance;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Distance
{
    [TestFixture]
    public class DistanceTest
    {
        private PrecisionModel precisionModel;
        private GeometryFactory geometryFactory;
        private WKTReader reader;

        public DistanceTest()
        {
            precisionModel = new PrecisionModel(1);
            geometryFactory = new GeometryFactory(precisionModel, 0);
            reader = new WKTReader(geometryFactory);
        }

        [Test]
        public void TestEverything()
        {
            IGeometry g1 = reader.Read("POLYGON ((40 320, 200 380, 320 80, 40 40, 40 320),  (180 280, 80 280, 100 100, 220 140, 180 280))");
            IGeometry g2 = reader.Read("POLYGON ((160 240, 120 240, 120 160, 160 140, 160 240))");
            Assert.AreEqual(18.97366596, g1.Distance(g2), 1E-5);

            g2 = reader.Read("POLYGON ((160 240, 120 240, 120 160, 180 100, 160 240))");
            Assert.AreEqual(0.0, g1.Distance(g2), 1E-5);

            LineString l1 = (LineString) reader.Read("LINESTRING(10 10, 20 20, 30 40)");
            LineString l2 = (LineString) reader.Read("LINESTRING(10 10, 20 20, 30 40)");
            Assert.AreEqual(0.0, l1.Distance(l2), 1E-5);
        }

        [Ignore("This test is resulting in failure for the distance to the empty polygon because the default position is being calculated as the minimum distance location, which is a constance of Double.MaxValue.  In JTS beyond version 1.9 there is a change to default empty geometry to a distance of 0.  This test should be enabled once the new logic in NetTopologySuite.Operation.Distance.DistanceOp is migrated to NTS")]
        public void TestEmpty()
        {
            IGeometry g1 = reader.Read("POINT (0 0)");
            IGeometry g2 = reader.Read("POLYGON EMPTY");
            Assert.AreEqual(0.0, g1.Distance(g2), 0.0);
        }

        [Test]
        public void TestClosestPoints1()
        {
            DoNearestPointsTest("POLYGON ((200 180, 60 140, 60 260, 200 180))", "POINT (140 280)", 57.05597791103589, new Coordinate(111.6923076923077, 230.46153846153845), new Coordinate(140, 280));
        }

        [Test]
        public void TestClosestPoints2()
        {
            DoNearestPointsTest("POLYGON ((200 180, 60 140, 60 260, 200 180))", "MULTIPOINT ((140 280), (140 320))", 57.05597791103589, new Coordinate(111.6923076923077, 230.46153846153845), new Coordinate(140, 280));
        }

        [Test]
        public void TestClosestPoints3()
        {
            DoNearestPointsTest("LINESTRING (100 100, 200 100, 200 200, 100 200, 100 100)", "POINT (10 10)", 127.27922061357856, new Coordinate(100, 100), new Coordinate(10, 10));
        }

        [Test]
        public void TestClosestPoints4()
        {
            DoNearestPointsTest("LINESTRING (100 100, 200 200)", "LINESTRING (100 200, 200 100)", 0.0, new Coordinate(150, 150), new Coordinate(150, 150));
        }

        [Test]
        public void TestClosestPoints5()
        {
            DoNearestPointsTest("LINESTRING (100 100, 200 200)", "LINESTRING (150 121, 200 0)", 20.506096654409877, new Coordinate(135.5, 135.5), new Coordinate(150, 121));
        }

        [Test]
        public void TestClosestPoints6()
        {
            DoNearestPointsTest("POLYGON ((76 185, 125 283, 331 276, 324 122, 177 70, 184 155, 69 123, 76 185), (267 237, 148 248, 135 185, 223 189, 251 151, 286 183, 267 237))", "LINESTRING (153 204, 185 224, 209 207, 238 222, 254 186)", 13.788860460124573, new Coordinate(139.4956500724988, 206.78661188980183), new Coordinate(153, 204));
        }

        [Test]
        public void TestClosestPoints7()
        {
            DoNearestPointsTest("POLYGON ((76 185, 125 283, 331 276, 324 122, 177 70, 184 155, 69 123, 76 185), (267 237, 148 248, 135 185, 223 189, 251 151, 286 183, 267 237))", "LINESTRING (120 215, 185 224, 209 207, 238 222, 254 186)", 0.0, new Coordinate(120, 215), new Coordinate(120, 215));
        }

        private void DoNearestPointsTest(String wkt0, String wkt1, double distance,
                                        Coordinate p0, Coordinate p1)
        {
            DistanceOp op = new DistanceOp(new WKTReader().Read(wkt0), new WKTReader().Read(wkt1));
            double tolerance = 1E-10;
            Assert.AreEqual(distance, op.ClosestPoints()[0].Distance(op.ClosestPoints()[1]), tolerance);
            Assert.AreEqual(p0.X, op.ClosestPoints()[0].X, tolerance);
            Assert.AreEqual(p0.Y, op.ClosestPoints()[0].Y, tolerance);
            Assert.AreEqual(p1.X, op.ClosestPoints()[1].X, tolerance);
            Assert.AreEqual(p1.Y, op.ClosestPoints()[1].Y, tolerance);
        }
    }
}
