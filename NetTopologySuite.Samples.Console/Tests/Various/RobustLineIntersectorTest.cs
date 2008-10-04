using System;
using System.Diagnostics;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Samples.SimpleTests;
using NetTopologySuite.Coordinates;
using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Various
{
    [TestFixture]
    public class RobustLineIntersectorTest : BaseSamples
    {
        private const String lineString1 =
            "LINESTRING (2089426.5233462777 1180182.3877339689, 2085646.6891757075 1195618.7333999649)";

        private const String lineString2 =
            "LINESTRING (1889281.8148903656 1997547.0560044837, 2259977.3672235999 483675.17050843034)";

        private const String point1 = "POINT (2097408.2633752143 1144595.8008114607)";

        /// <summary>
        /// Initializes a new instance of the <see cref="RobustLineIntersectorTest"/> class.
        /// </summary>
        public RobustLineIntersectorTest()
            : base(GeometryFactory<BufferedCoordinate>.CreateFixedPrecision(
                       new BufferedCoordinateSequenceFactory(), 1.0)) { }

        [Test]
        public void IntersectionIsCorrect()
        {
            IGeometry g1 = Reader.Read(lineString1);
            IGeometry g2 = Reader.Read(lineString2);
            IGeometry p1 = Reader.Read(point1);
            IGeometry result = g1.Intersection(g2);

            Debug.WriteLine(result);
            Assert.IsNotNull(result);
        }
    }
}