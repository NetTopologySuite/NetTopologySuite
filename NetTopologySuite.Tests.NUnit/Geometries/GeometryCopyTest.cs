using System;
using GeoAPI.Geometries;
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

        private void CheckCopy(IGeometry g)
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
    }
}
