using System;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Operation.Distance;
using NetTopologySuite.Coordinates.Simple;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Distance
{
    [TestFixture]
    public class DistanceTest
    {
        [Test]

        public void TestEverything()
        {
            IGeometry<Coordinate> g1 = GeometryUtils.Reader.Read("POLYGON ((40 320, 200 380, 320 80, 40 40, 40 320),  (180 280, 80 280, 100 100, 220 140, 180 280))");
            IGeometry<Coordinate> g2 = GeometryUtils.Reader.Read("POLYGON ((160 240, 120 240, 120 160, 160 140, 160 240))");
            Assert.AreEqual(18.97366596, g1.Distance(g2), 1E-5);

            g2 = GeometryUtils.Reader.Read("POLYGON ((160 240, 120 240, 120 160, 180 100, 160 240))");
            Assert.AreEqual(0.0, g1.Distance(g2), 1E-5);

            ILineString<Coordinate> l1 = (ILineString<Coordinate>)GeometryUtils.Reader.Read("LINESTRING(10 10, 20 20, 30 40)");
            ILineString<Coordinate> l2 = (ILineString<Coordinate>)GeometryUtils.Reader.Read("LINESTRING(10 10, 20 20, 30 40)");
            Assert.AreEqual(0.0, l1.Distance(l2), 1E-5);
        }

        [Test]
        public void TestEmpty()
        {
            IGeometry<Coordinate> g1 = GeometryUtils.Reader.Read("POINT (0 0)");
            IGeometry<Coordinate> g2 = GeometryUtils.Reader.Read("POLYGON EMPTY");
            Assert.AreEqual(0.0, g1.Distance(g2), 0.0);
        }

        [Test]
        public void TestClosestPoints1()
        {
            DoNearestPointsTest("POLYGON ((200 180, 60 140, 60 260, 200 180))", "POINT (140 280)", 57.05597791103589, GeometryUtils.CoordFac.Create(111.6923076923077, 230.46153846153845), GeometryUtils.CoordFac.Create(140, 280));
        }
        [Test]
        public void TestClosestPoints2()
        {
            DoNearestPointsTest("POLYGON ((200 180, 60 140, 60 260, 200 180))", "MULTIPOINT ((140 280), (140 320))", 57.05597791103589, GeometryUtils.CoordFac.Create(111.6923076923077, 230.46153846153845), GeometryUtils.CoordFac.Create(140, 280));
        }
        [Test]
        public void TestClosestPoints3()
        {
            DoNearestPointsTest("LINESTRING (100 100, 200 100, 200 200, 100 200, 100 100)", "POINT (10 10)", 127.27922061357856, GeometryUtils.CoordFac.Create(100, 100), GeometryUtils.CoordFac.Create(10, 10));
        }
        [Test]
        public void TestClosestPoints4()
        {
            DoNearestPointsTest("LINESTRING (100 100, 200 200)", "LINESTRING (100 200, 200 100)", 0.0, GeometryUtils.CoordFac.Create(150, 150), GeometryUtils.CoordFac.Create(150, 150));
        }
        [Test]
        public void TestClosestPoints5()
        {
            DoNearestPointsTest("LINESTRING (100 100, 200 200)", "LINESTRING (150 121, 200 0)", 20.506096654409877, GeometryUtils.CoordFac.Create(135.5, 135.5), GeometryUtils.CoordFac.Create(150, 121));
        }
        [Test]
        public void TestClosestPoints6()
        {
            DoNearestPointsTest("POLYGON ((76 185, 125 283, 331 276, 324 122, 177 70, 184 155, 69 123, 76 185), (267 237, 148 248, 135 185, 223 189, 251 151, 286 183, 267 237))", "LINESTRING (153 204, 185 224, 209 207, 238 222, 254 186)", 13.788860460124573, GeometryUtils.CoordFac.Create(139.4956500724988, 206.78661188980183), GeometryUtils.CoordFac.Create(153, 204));
        }
        [Test]
        public void TestClosestPoints7()
        {
            DoNearestPointsTest("POLYGON ((76 185, 125 283, 331 276, 324 122, 177 70, 184 155, 69 123, 76 185), (267 237, 148 248, 135 185, 223 189, 251 151, 286 183, 267 237))", "LINESTRING (120 215, 185 224, 209 207, 238 222, 254 186)", 0.0, GeometryUtils.CoordFac.Create(120, 215), GeometryUtils.CoordFac.Create(120, 215));
        }

        private static void DoNearestPointsTest(String wkt0, String wkt1, double distance,
                                         Coordinate p0, Coordinate p1)
        {
            DistanceOp<Coordinate> op = new DistanceOp<Coordinate>(GeometryUtils.Reader.Read(wkt0), GeometryUtils.Reader.Read(wkt1));
            Double tolerance = 1e-10;
            Pair<Coordinate>? np = op.NearestPoints();
            if (np.HasValue)
            {
                Assert.AreEqual(distance, np.Value.First.Distance(np.Value.Second), tolerance);
                Assert.AreEqual(p0[Ordinates.X], np.Value.First[Ordinates.X], tolerance);
                Assert.AreEqual(p0[Ordinates.Y], np.Value.First[Ordinates.Y], tolerance);
                Assert.AreEqual(p1[Ordinates.X], np.Value.Second[Ordinates.X], tolerance);
                Assert.AreEqual(p1[Ordinates.Y], np.Value.Second[Ordinates.Y], tolerance);
            }
            else
            {
                Assert.IsTrue(np.HasValue);
            }
        }
    }
}