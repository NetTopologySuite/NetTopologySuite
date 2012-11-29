using System.IO;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.IO;
using NetTopologySuite.IO.GML2;

namespace NetTopologySuite.Tests.Various
{
    public class Issue131Test
    {
        [Test]
        public void TestNaNComparison()
        {
            const double d1 = double.NaN, d2 = double.NaN, d3 = 1;
            Assert.False(d1 == d2);
            Assert.False(d1 < d2);
            Assert.False(d1 > d2);
            Assert.False(d1 == d3);
            Assert.False(d1 < d3);
            Assert.False(d1 > d3);
            Assert.True(d1.CompareTo(d2) == 0);
            Assert.True(d1.CompareTo(d3) == -1);
            Assert.True(d3.CompareTo(d1) == 1);
        }
    }

    public class Issue117Tests
    {
        [Test]
        public void Issue117()
        {
            var geometryNts = new WKTReader().Read("POLYGON((0 0,100 0,100 100, 0 100, 0 0 )))");

            //the features must be missing. What should I do there to add the features.Thanks
            var ntsGeometry = GetGeometryUsingNTS(geometryNts);
            using (var sw = new StreamWriter(File.Create("polygon.gml")))
            {
                sw.Write(ntsGeometry);
                sw.Close();
            }
        }

        private static string GetGeometryUsingNTS(IGeometry geometry)
        {
            var gmlWriter = new GMLWriter();
            var ms = new MemoryStream();

            gmlWriter.Write(geometry, ms);
            return System.Text.Encoding.Default.GetString(ms.ToArray());
        }
    }
}
