using System;
using System.Diagnostics;
using NetTopologySuite.Geometries;
using NetTopologySuite.Samples.SimpleTests;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Various
{
    [TestFixture]
    public class RobustLineIntersectorTest : BaseSamples
    {
        public RobustLineIntersectorTest() : base(GeometryFactory.Fixed) { }

        [Test]
        public void IntersectionTest()
        {
            var g1 = Reader.Read("LINESTRING (2089426.5233462777 1180182.3877339689, 2085646.6891757075 1195618.7333999649)");
            var g2 = Reader.Read("LINESTRING (1889281.8148903656 1997547.0560044837, 2259977.3672235999 483675.17050843034)");
            var result = g1.Intersection(g2);            
            
            Debug.WriteLine(result);
            Assert.IsNotNull(result);            
        }

        [Test]
        public void IntersectionTest2()
        {
            var g1 = Reader.Read("LINESTRING(0 10, 620 10, 0 11)");
            var g2 = Reader.Read("LINESTRING(400 60, 400 10)");
            var result = g1.Intersection(g2);

            Debug.WriteLine(result);
            Assert.IsNotNull(result, "");
        }
    }
}
