using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixture]
    public class GeometryFactoryTest
    {
        private readonly static PrecisionModel PrecModel = new PrecisionModel();
        private readonly static GeometryFactory Factory = new GeometryFactory(PrecModel, 0);
        private readonly WKTReader _reader = new WKTReader(Factory);

        [Test]
        public void TestCreateGeometry()
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

        private void CheckCreateGeometryExact(string wkt)
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