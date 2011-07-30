using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.IO
{
    /// <summary>
    /// Tests the <see cref="WKTReader"/> with various errors
    /// </summary>
    [TestFixture]
    public class WKTReaderParseErrorTest   
    {   
        private IGeometryFactory fact;
        private WKTReader rdr;

        public WKTReaderParseErrorTest()
        {
            fact = new GeometryFactory();
            rdr = new WKTReader(fact);
        }

        [Test]
        public void TestExtraLParen()
        {
            ReadBad("POINT (( 1e01 -1E02)");
        }

        [Test]
        public void TestMissingOrdinate()
        {
            ReadBad("POINT ( 1e01 )");
        }

        [Test]
        public void TestBadChar()
        {
            ReadBad("POINT ( # 1e-04 1E-05)");
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
