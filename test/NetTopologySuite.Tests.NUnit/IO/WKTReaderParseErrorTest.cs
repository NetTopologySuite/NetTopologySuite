using System;
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
        private readonly WKTReader _rdr;

        public WKTReaderParseErrorTest()
        {
            _rdr = new WKTReader();
        }

        [Test]
        public void TestExtraLParen()
        {
            ReadWithParseException("POINT (( 1e01 -1E02)");
        }

        [Test]
        public void TestMissingOrdinate()
        {
            ReadWithParseException("POINT ( 1e01 )");
        }

        [Test]
        public void TestBadChar()
        {
            ReadWithParseException("POINT ( # 1e-04 1E-05)");
        }

        [Test]
        public void TestBadExpFormat()
        {
            ReadWithParseException("POINT (1e0a1 1X02)");
        }

        [Test]
        public void TestBadExpPlusSign()
        {
            ReadWithParseException("POINT (1e+01 1X02)");
        }

        [Test]
        public void TestBadPlusSign()
        {
            ReadWithParseException("POINT ( +1e+01 1X02)");
        }

        [Test]
        public void TestBadCharsInType()
        {
            ReadWithParseException("POINTABC ( 0 0 )");
            ReadWithParseException("LINESTRINGABC ( 0 0 )");
            ReadWithParseException("LINEARRINGABC ( 0 0, 0 0, 0 0 )");
            ReadWithParseException("POLYGONABC (( 0 0, 0 0, 0 0, 0 0 ))");
            ReadWithParseException("MULTIPOINTABC (( 0 0 ), ( 0 0 ))");
            ReadWithParseException("MULTILINESTRINGABC (( 0 0, 1 1 ), ( 0 0, 1 1 ))");
            ReadWithParseException("MULTIPOLYGONABC ((( 0 0, 1 1, 2 2, 0 0 )), (( 0 0, 1 1, 2 2, 0 0 )))");
            ReadWithParseException("GEOMETRYCOLLECTIONABC (POINT( 0 0 ), LINESTRING( 0 0, 1 1))");
        }

        [Test]
        public void TestBadCharsInTypeZ()
        {
            ReadWithParseException("POINTABCZ ( 0 0 )");
            ReadWithParseException("LINESTRINGABCZ ( 0 0 )");
            ReadWithParseException("LINEARRINGABCZ ( 0 0, 0 0, 0 0 )");
            ReadWithParseException("POLYGONABCZ (( 0 0, 0 0, 0 0, 0 0 ))");
            ReadWithParseException("MULTIPOINTABCZ (( 0 0 ), ( 0 0 ))");
            ReadWithParseException("MULTILINESTRINGABCZ (( 0 0, 1 1 ), ( 0 0, 1 1 ))");
            ReadWithParseException("MULTIPOLYGONABCZ ((( 0 0, 1 1, 2 2, 0 0 )), (( 0 0, 1 1, 2 2, 0 0 )))");
            ReadWithParseException("GEOMETRYCOLLECTIONABCZ (POINT( 0 0 ), LINESTRING( 0 0, 1 1))");
        }

        [Test]
        public void TestBadCharsInTypeM()
        {
            ReadWithParseException("LINESTRINGABCM ( 0 0 0, 1 1 1 )");
        }

        [Test]
        public void TestBadCharsInTypeZM()
        {
            ReadWithParseException("LINESTRINGABCZM ( 0 0 0 0, 1 1 1 1 )");
        }


        private void ReadWithParseException(string wkt)
        {
            bool threwParseEx = false;
            try
            {
                var g = _rdr.Read(wkt);
            }
            catch (ParseException /*ex*/)
            {
                //TestContext.WriteLine(ex.Message);
                threwParseEx = true;
            }

            Assert.IsTrue(threwParseEx);
        }
    }
}
