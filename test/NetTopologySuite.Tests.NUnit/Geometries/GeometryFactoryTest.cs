using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixture]
    public class GeometryFactoryTest
    {
        protected static readonly PrecisionModel PrecModel = new PrecisionModel();
        private readonly GeometryFactory Factory;
        private readonly WKTReader _reader;

        public GeometryFactoryTest()
            : this(NtsGeometryServices.Instance, NtsGeometryServices.Instance.CreateGeometryFactory())
        {
        }

        public GeometryFactoryTest(NtsGeometryServices ntsGeometryServices, GeometryFactory geometryFactory = null)
        {
            Factory = geometryFactory ?? new GeometryFactory();
            _reader = new WKTReader(ntsGeometryServices);
            _reader.Factory = geometryFactory ?? ntsGeometryServices.CreateGeometryFactory();
        }

        [Test]
        public void TestCreatePolygonWithNull()
        {
            Geometry p = null;
            Assert.That(() => p = Factory.CreatePolygon((CoordinateSequence)null), Throws.Nothing);
            Assert.That(p.IsEmpty);

            Assert.That(() => p = Factory.CreatePolygon((Coordinate[])null), Throws.Nothing);
            Assert.That(p.IsEmpty);

            Assert.That(() => p = Factory.CreatePolygon((LinearRing)null), Throws.Nothing);
            Assert.That(p.IsEmpty);

            Assert.That(() => p = Factory.CreatePolygon(null, null), Throws.Nothing);
            Assert.That(p.IsEmpty);
        }

        [Test]
        public virtual void TestCreateGeometry()
        {
            CheckCreateGeometryExact("POINT EMPTY");
            CheckCreateGeometryExact("POINT ( 10 20 )");
            CheckCreateGeometryExact("LINESTRING EMPTY");
            CheckCreateGeometryExact("LINESTRING(0 0, 10 10)");
            CheckCreateGeometryExact("MULTILINESTRING ((50 100, 100 200), (100 100, 150 200))");
            CheckCreateGeometryExact("POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200))");
            CheckCreateGeometryExact(
                "MULTIPOLYGON (((100 200, 200 200, 200 100, 100 100, 100 200)), ((300 200, 400 200, 400 100, 300 100, 300 200)))");
            CheckCreateGeometryExact(
                "GEOMETRYCOLLECTION (POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200)), LINESTRING (250 100, 350 200), POINT (350 150))");
        }

        [Test]
        public void TestCreateEmpty()
        {
            CheckEmpty(Factory.CreateEmpty(Dimension.Point), typeof(Point));
            CheckEmpty(Factory.CreateEmpty(Dimension.Curve), typeof(LineString));
            CheckEmpty(Factory.CreateEmpty(Dimension.Surface), typeof(Polygon));
    
            CheckEmpty(Factory.CreatePoint(), typeof(Point));
            CheckEmpty(Factory.CreateLineString(), typeof(LineString));
            CheckEmpty(Factory.CreatePolygon(), typeof(Polygon));
    
            CheckEmpty(Factory.CreateMultiPoint(), typeof(MultiPoint));
            CheckEmpty(Factory.CreateMultiLineString(), typeof(MultiLineString));
            CheckEmpty(Factory.CreateMultiPolygon(), typeof(MultiPolygon));
            CheckEmpty(Factory.CreateGeometryCollection(), typeof(GeometryCollection));
        }

        private static void CheckEmpty(Geometry geom, Type clz)
        {
            Assert.IsTrue(geom.IsEmpty);
            Assert.IsTrue(geom.GetType() == clz);
        }

        [Test]
        public void TestDeepCopy()
        {
            var g = (Point)Read("POINT ( 10 10) ");
            var g2 = Factory.CreateGeometry(g);
            g.CoordinateSequence.SetOrdinate(0, 0, 99);
            Assert.IsTrue(!g.EqualsExact(g2));
        }

        /// <summary>
        /// CoordinateArraySequences default their dimension to 3 unless explicitly told otherwise.
        /// This test ensures that GeometryFactory.CreateGeometry() recreates the input dimension properly.
        /// </summary>
        [Test]
        public void TestCopyGeometryWithNonDefaultDimension()
        {
            var gf = new GeometryFactory(CoordinateArraySequenceFactory.Instance);
            var mpSeq = gf.CoordinateSequenceFactory.Create(1, 2);
            mpSeq.SetOrdinate(0, Ordinate.X, 50);
            mpSeq.SetOrdinate(0, Ordinate.Y, -2);

            var g = gf.CreatePoint(mpSeq);
            var geometryN = (Point)g.GetGeometryN(0);
            var gSeq = geometryN.CoordinateSequence;
            Assert.AreEqual(2, gSeq.Dimension);

            var g2 = (Point)Factory.CreateGeometry(g);
            var g2Seq = g2.CoordinateSequence;
            Assert.AreEqual(2, g2Seq.Dimension);
        }

        protected void CheckCreateGeometryExact(string wkt)
        {
            var g = Read(wkt);
            var g2 = Factory.CreateGeometry(g);
            Assert.IsTrue(g.EqualsExact(g2));
        }

        private Geometry Read(string wkt)
        {
            return _reader.Read(wkt);
        }
    }
}
