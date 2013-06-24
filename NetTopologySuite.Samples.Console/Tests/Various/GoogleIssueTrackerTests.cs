using NUnit.Framework;
using NetTopologySuite.IO;

namespace NetTopologySuite.Tests.Various
{
    public class GoogleIssueTrackerTests
    {
        static GoogleIssueTrackerTests()
        {
            if (GeoAPI.GeometryServiceProvider.Instance == null)
                GeoAPI.GeometryServiceProvider.Instance = NtsGeometryServices.Instance;
        }

        [Test]
        public void TestIssue147()
        {
            var wkt = "POLYGON ((-94.16 42.25, -94.15 42.26, -94.14 42.25, -94.16 42.25))";
            var geom = new WKTReader().Read(wkt);
            Assert.AreEqual(
                "0x010300000001000000040000000AD7A3703D8A57C000000000002045409A999999998957C0E17A14AE47214540295C8FC2F58857C000000000002045400AD7A3703D8A57C00000000000204540",
                "0x"+ WKBWriter.ToHex(geom.AsBinary()));
        }
    }
}