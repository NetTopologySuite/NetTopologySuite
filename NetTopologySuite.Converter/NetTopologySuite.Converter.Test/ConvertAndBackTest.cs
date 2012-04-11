using DotSpatial.Data;
using DotSpatial.Topology;
using NetTopologySuite.IO.Tests;

namespace GeoAPI.Geometries.Test
{
    using NUnit.Framework;

    [TestFixture]
    public class ConvertAndBackTest
    {
        private RandomGeometryHelper _randomGeometryHelper;

        [TestFixtureSetUp]
        public void SetUp()
        {
            _randomGeometryHelper = new RandomGeometryHelper(NetTopologySuite.Geometries.GeometryFactory.Default);
            _randomGeometryHelper.Ordinates = Ordinates.XYZ;
        }

        [Test]
        public void TestPoint()
        {
            var pt = _randomGeometryHelper.Point;
            var ptDS = pt.ToDotSpatial();

            TestCoordinate(pt.Coordinate, ptDS.Coordinate);
        }

        [Test]
        public void TestLineString()
        {
            var ls = _randomGeometryHelper.LineString;
            var lsDS = ls.ToDotSpatial();
            TestLineString(ls, lsDS);
        }

        private static void TestLineString(ILineString ls, DotSpatial.Topology.ILineString lsDS)
        {
            Assert.AreEqual(ls.Length, lsDS.Length);
            Assert.AreEqual(ls.IsRing, lsDS.IsRing);
            Assert.AreEqual(ls.IsClosed, lsDS.IsClosed);

            TestCoordinate(ls.StartPoint.Coordinate, lsDS.StartPoint.Coordinate);
            TestCoordinate(ls.EndPoint.Coordinate, lsDS.EndPoint.Coordinate);

            var lsGeoApi = lsDS.ToGeoAPI();
            Assert.IsTrue(ls.EqualsExact(lsGeoApi));
        }

        [Test]
        public void TestMultiPoint()
        {
            var g = _randomGeometryHelper.MultiPoint;
            var gDS = g.ToDotSpatial();
            TestMultiPoint(g, gDS);
            TestShape(g);
        }

        private static void TestMultiPoint(IMultiPoint g, DotSpatial.Topology.IMultiPoint gDS)
        {
            Assert.AreEqual(g.NumPoints, gDS.NumPoints);
            Assert.AreEqual(g.NumGeometries, gDS.NumGeometries);
            for (var i = 0; i < g.NumPoints; i++)
                TestCoordinate(g.Coordinates[i], gDS.Coordinates[i]);

            Assert.IsTrue(g.EqualsExact(gDS.ToGeoAPI()));
        }

        [Test]
        public void TestMultiLineString()
        {
            var g = _randomGeometryHelper.MultiPoint;
            var gDS = g.ToDotSpatial();
            TestMulti(g, gDS);
            TestShape(g);
        }

        [Test]
        public void TestGeometry()
        {
            for (var i = 0; i < 50; i++)
            {
                var g = _randomGeometryHelper.Geometry;
                var gDS = g.ToDotSpatial();
                TestGeometry(g, gDS);
            }
        }

        [Test]
        public void TestGeometryCollection()
        {
            var g = _randomGeometryHelper.GeometryCollection;
            var gDS = g.ToDotSpatial();
            TestMulti(g, gDS);
        }

        private static void TestMulti<T1, T2>(T1 g, T2 gDS)
            where T1 : IGeometryCollection
            where T2 : DotSpatial.Topology.IGeometryCollection
        {
            Assert.AreEqual(g.NumGeometries, gDS.NumGeometries);
            Assert.AreEqual(g.Area, gDS.Area, 1e-9);
            for (var i = 0; i < g.NumGeometries; i++)
                TestGeometry(g.GetGeometryN(i), gDS.GetGeometryN(i));
        }

        private static void TestGeometry(IGeometry g, DotSpatial.Topology.IGeometry g2)
        {
            if (g is IPoint)
                TestCoordinate(g.Coordinate, g2.Coordinate);
            else if (g is ILineString)
                TestLineString((ILineString)g, (DotSpatial.Topology.ILineString)g2);
            else if (g is IPolygon)
                TestPolygon((IPolygon)g, (DotSpatial.Topology.IPolygon)g2);
            else
                TestMulti((IGeometryCollection)g, (DotSpatial.Topology.IGeometryCollection)g2);
        }

        [Test]
        public void TestMultiPolygon()
        {
            var g = _randomGeometryHelper.MultiPolygon;
            var gDS = g.ToDotSpatial();
            TestMulti(g, gDS);
            TestShape(g);
        }

        private static void TestShape(IGeometry g)
        {
            if (g is IGeometryCollection) return;

            var shape = g.ToDotSpatialShape();
            var g2 = shape.ToGeoAPI();

            Assert.IsTrue(g.EnvelopeInternal.Equals(g2.EnvelopeInternal));
            if (g is IPolygon)
            {
                var p = g as IPolygon;
                Assert.AreEqual(g.NumGeometries, g2.NumGeometries);
                Assert.AreEqual(p.NumInteriorRings, p.NumInteriorRings);
                Assert.AreEqual(g.Area, g2.Area, 1e-9);
                Assert.AreEqual(g.NumPoints, g2.NumPoints);

                var p2 = g2 as IPolygon;
                for (var i = 0; i < p.NumInteriorRings; i++)
                {
                    var l1 = p.GetInteriorRingN(i);
                    var l2 = p2.GetInteriorRingN(i);
                    Assert.AreEqual(l1.Length, l2.Length);
                    Assert.AreEqual(l1.IsRing, l2.IsRing);
                    Assert.AreEqual(l1.IsClosed, l2.IsClosed);

                    Assert.IsTrue(l1.StartPoint.Coordinate.Equals(l2.StartPoint.Coordinate));
                    Assert.IsTrue(l1.EndPoint.Coordinate.Equals(l2.EndPoint.Coordinate));
                }
            }
            else
                Assert.IsTrue(g.EqualsExact(g2));
        }

        [Test]
        public void TestPolygon()
        {
            var g = _randomGeometryHelper.Polygon;
            var gDS = g.ToDotSpatial();
            TestPolygon(g, gDS);
            TestShape(g);
        }

        private static void TestPolygon(IPolygon g, DotSpatial.Topology.IPolygon gDS)
        {
            Assert.AreEqual(g.NumGeometries, gDS.NumGeometries);
            Assert.AreEqual(g.NumInteriorRings, gDS.NumHoles);
            Assert.AreEqual(g.Area, gDS.Area, 1e-9);
            Assert.AreEqual(g.NumPoints, gDS.NumPoints);

            for (var i = 0; i < g.NumInteriorRings; i++)
                TestLineString(g.GetInteriorRingN(i), gDS.GetInteriorRingN(i));

            Assert.IsTrue(g.EqualsExact(gDS.ToGeoAPI()));
        }

        private static void TestCoordinate(Coordinate c1, DotSpatial.Topology.Coordinate c2)
        {
            Assert.AreEqual(c1.X, c2.X);
            Assert.AreEqual(c1.Y, c2.Y);
            if (!double.IsNaN(c1.Z))
                Assert.AreEqual(c1.Z, c2.Z);
        }

        [Test, Ignore("Provide path to valid shapefile")]
        public void TestPreparedGeometryFilter()
        {
            var sf = Shapefile.OpenFile("");

            var randomId = new System.Random().Next(0, sf.NumRows() - 1);
            var buffer = sf.GetShape(randomId, false).ToGeoAPI().Buffer(20d);
            var bufferShape = buffer.ToDotSpatialShape();

            var res = sf.Select(bufferShape);
        }
    }
}