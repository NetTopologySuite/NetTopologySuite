#nullable disable
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    public class GeometryCopyTest : GeometryTestCase
    {
        [Test]
        public void TestCopy()
        {
            CheckCopy(Read(GeometryTestData.WKT_POINT));
            CheckCopy(Read(GeometryTestData.WKT_LINESTRING));
            CheckCopy(Read(GeometryTestData.WKT_LINEARRING));
            CheckCopy(Read(GeometryTestData.WKT_POLY));
            CheckCopy(Read(GeometryTestData.WKT_MULTIPOINT));
            CheckCopy(Read(GeometryTestData.WKT_MULTILINESTRING));
            CheckCopy(Read(GeometryTestData.WKT_MULTIPOLYGON));
            CheckCopy(Read(GeometryTestData.WKT_GC));
        }

        private void CheckCopy(Geometry g)
        {
            int SRID = 123;
            g.SRID = SRID;

            object DATA = new object();
            g.UserData = DATA;

            var copy = g.Copy();

            Assert.AreEqual(g.SRID, copy.SRID);
            Assert.AreEqual(g.UserData, copy.UserData);

            //TODO: use a test which checks all ordinates of CoordinateSequences
            Assert.True(g.EqualsExact(copy));
        }

        [Test]
        public void TestCopyDoesNotChangeFactory()
        {
            var gf = new GeometryFactory(new PrecisionModel(), 4325);
            var pt1 = gf.CreatePoint(new Coordinate(10, 10));
            var pt2 = pt1.Copy();

            Assert.That(ReferenceEquals(pt1.Factory, pt2.Factory), Is.True);

            pt2.SRID = 4326;
            Assert.That(ReferenceEquals(pt1.Factory, pt2.Factory), Is.False);

            pt1.SRID = 4326;
            Assert.That(ReferenceEquals(pt1.Factory, pt2.Factory), Is.True);

        }
    }
}
