using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.IO
{
    /// <summary>
    /// Tests the <see cref="WKTReader" /> with exponential notation.
    /// </summary>
    [TestFixture]
    public class WKTReaderExpTest
    {
        private IGeometryFactory fact;
        private WKTReader rdr;

        public WKTReaderExpTest()
        {
            fact = new GeometryFactory();
            rdr = new WKTReader(fact);
        }

        [Test]
        public void TestGoodBasicExp()
        {
            ReadGoodCheckCoordinate("POINT ( 1e01 -1E02)", 1E01, -1E02);
        }

        [Test]
        public void TestGoodWithExpSign()
        {
            ReadGoodCheckCoordinate("POINT ( 1e-04 1E-05)", 1e-04, 1e-05);
        }

        [Test]
        public void TestBadExpFormat()
        {
            ReadBad("POINT (1e0a1 1X02)");
        }

        [Test]
        public void TestBadExpPlusSign()
        {
            ReadBad("POINT (1e+01 1X02)");
        }

        [Test]
        public void TestBadPlusSign()
        {
            ReadBad("POINT ( +1e+01 1X02)");
        }

        private void ReadGoodCheckCoordinate(String wkt, double x, double y)
        {
            IGeometry g = rdr.Read(wkt);
            ICoordinate pt = g.Coordinate;
            Assert.AreEqual(pt.X, x, 0.0001);
            Assert.AreEqual(pt.Y, y, 0.0001);
        }

        private void ReadBad(String wkt)
        {
            bool threwParseEx = false;
            try
            {
                IGeometry g = rdr.Read(wkt);
            }
            catch (ParseException ex)
            {
                Console.WriteLine(ex.Message);
                threwParseEx = true;
            }
            Assert.IsTrue(threwParseEx);
        }
    }
}

