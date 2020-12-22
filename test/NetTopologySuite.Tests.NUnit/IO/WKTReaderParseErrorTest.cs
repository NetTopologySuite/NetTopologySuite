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

        private void ReadBad(string wkt)
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
