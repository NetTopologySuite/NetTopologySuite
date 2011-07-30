using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    /// <summary>
    /// Test named predicate short-circuits
    /// </summary>
    [TestFixture]
    public class IsRectangleTest
    {
        WKTReader rdr = new WKTReader();

        [Test]
        public void TestValidRectangle()
        {
            Assert.IsTrue(IsRectangle("POLYGON ((0 0, 0 100, 100 100, 100 0, 0 0))"));
        }

        [Test]
        public void TestValidRectangle2()
        {
            Assert.IsTrue(IsRectangle("POLYGON ((0 0, 0 200, 100 200, 100 0, 0 0))"));
        }

        [Test]
        public void TestRectangleWithHole()
        {
            Assert.IsTrue(!IsRectangle("POLYGON ((0 0, 0 100, 100 100, 100 0, 0 0), (10 10, 10 90, 90 90, 90 10, 10 10) ))"));
        }

        [Test]
        public void TestNotRectilinear()
        {
            Assert.IsTrue(!IsRectangle("POLYGON ((0 0, 0 100, 99 100, 100 0, 0 0))"));
        }

        [Test]
        public void TestTooManyPoints()
        {
            Assert.IsTrue(!IsRectangle("POLYGON ((0 0, 0 100, 100 50, 100 100, 100 0, 0 0))"));
        }

        [Test]
        public void TestTooFewPoints()
        {
            Assert.IsTrue(!IsRectangle("POLYGON ((0 0, 0 100, 100 0, 0 0))"));
        }

        [Test]
        public void TestRectangularLinestring()
        {
            Assert.IsTrue(!IsRectangle("LINESTRING (0 0, 0 100, 100 100, 100 0, 0 0)"));
        }

        [Test]
        public void TestPointsInWrongOrder()
        {
            Assert.IsTrue(!IsRectangle("POLYGON ((0 0, 0 100, 100 0, 100 100, 0 0))"));
        }

        public bool IsRectangle(String wkt)
        {
            IGeometry a = rdr.Read(wkt);
            return a.IsRectangle;
        }
    }
}