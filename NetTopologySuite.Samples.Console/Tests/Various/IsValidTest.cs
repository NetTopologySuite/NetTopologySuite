using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.SimpleTests;
using NUnit.Framework;
#if BUFFERED
using coord = NetTopologySuite.Coordinates.BufferedCoordinate;
using coordFac = NetTopologySuite.Coordinates.BufferedCoordinateFactory;
using coordSeqFac = NetTopologySuite.Coordinates.BufferedCoordinateSequenceFactory;
#else
using coord = NetTopologySuite.Coordinates.Simple.Coordinate;
using coordFac = NetTopologySuite.Coordinates.Simple.CoordinateFactory;
using coordSeqFac = NetTopologySuite.Coordinates.Simple.CoordinateSequenceFactory;
#endif

namespace GisSharpBlog.NetTopologySuite.Tests.Various
{
    [TestFixture]
    public class IsValidTest : BaseSamples
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsValidTest"/> class.
        /// </summary>
        public IsValidTest()
            : base(new GeometryFactory<coord>(
                       new coordSeqFac(
                           new coordFac(1.0))))
        {
        }

        [Test]
        public void IsCCWBugTest()
        {
            IGeometry g =
                Reader.Read(
                    "POLYGON ((60.0 40.0, 60.0 240.0, 460.0 240.0, 460.0 40.0, 60.0 40.0), (260.0 200.0, 340.0 60.0, 400.0 120.0, 260.0 200.0), (260.0 200.0, 120.0 100.0, 200.0 60.0, 260.0 200.0))");
            Assert.IsNotNull(g);
            Boolean result = g.IsValid;
            Assert.IsTrue(result);
        }
    }
}