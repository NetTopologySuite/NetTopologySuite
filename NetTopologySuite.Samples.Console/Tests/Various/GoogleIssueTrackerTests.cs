using System;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
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

        [Test(Description = "WKBWriter added 4 extra bytes which caused picky SqlServer to reject polygons")]
        public void TestIssue147()
        {
            var wkt = "POLYGON ((-94.16 42.25, -94.15 42.26, -94.14 42.25, -94.16 42.25))";
            var geom = new WKTReader().Read(wkt);
            Assert.AreEqual(
                "0x010300000001000000040000000AD7A3703D8A57C000000000002045409A999999998957C0E17A14AE47214540295C8FC2F58857C000000000002045400AD7A3703D8A57C00000000000204540",
                "0x"+ WKBWriter.ToHex(geom.AsBinary()));
        }

        [Test(Description =
            "ShapefileDataReader error 'The output char buffer is too small to contain the decoded characters'")]
        public void TestIssue161()
        {
            var testFileDataDirectory = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    string.Format("..{0}..{0}..{0}NetTopologySuite.Samples.Shapefiles", Path.DirectorySeparatorChar));
            
            //SETUP
            var filePath = Path.Combine(testFileDataDirectory, "LSOA_2011_EW_BGC.shp");
            if (!File.Exists(filePath)) Assert.Ignore("File '{0}' not present", filePath);

            //ATTEMPT
            using (var reader = new ShapefileDataReader(filePath, GeometryFactory.Default))
            {
                var header = reader.ShapeHeader;

                while (reader.Read())//&& count++ < 3)
                {
                    object val;
                    Assert.DoesNotThrow(()=> val = reader["LSOA11CD"]);
                }
            }
            
        }
    }
}