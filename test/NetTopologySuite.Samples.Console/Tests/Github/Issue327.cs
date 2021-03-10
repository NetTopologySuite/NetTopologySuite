using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    public class Issue327
    {
        [TestCase(true)]
        [TestCase(false)]
        [Description("Use of exceptions for flow control")]
        public void Test(bool strict)
        {
            const string wktNonClosedPolygon1 = "POLYGON((10 10, 10 20, 20 20))";
            const string wktNonClosedPolygon2 = "POLYGON((10 10, 10 20, 20 20, 20 10))";
            const string wktInsufficientLineString = "LINESTRING(10 10)";

            var wktReader = new WKTReader {IsStrict = strict};
            Assert.That(wktReader.IsStrict, Is.EqualTo(strict));

            Geometry polygon1 = null, polygon2 = null, linestring = null;
            if (strict)
            {
                Assert.That(() => polygon1 = wktReader.Read(wktNonClosedPolygon1), Throws.InstanceOf<ArgumentException>());
                Assert.That(() => polygon2 = wktReader.Read(wktNonClosedPolygon2), Throws.InstanceOf<ArgumentException>());
                Assert.That(() => linestring = wktReader.Read(wktInsufficientLineString), Throws.InstanceOf<ArgumentException>());
            }
            else
            {
                Assert.That(() => polygon1 = wktReader.Read(wktNonClosedPolygon1), Throws.Nothing);
                Assert.That(polygon1, Is.Not.Null);
                Assert.That(() => polygon2 = wktReader.Read(wktNonClosedPolygon2), Throws.Nothing);
                Assert.That(polygon2, Is.Not.Null);
                Assert.That(() => linestring = wktReader.Read(wktInsufficientLineString), Throws.Nothing);
                Assert.That(linestring, Is.Not.Null);
            }
        }
    }
}
