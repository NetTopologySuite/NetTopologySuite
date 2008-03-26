using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

using GeoAPI.Geometries;

using GisSharpBlog.NetTopologySuite.Geometries;

using GisSharpBlog.NetTopologySuite.Samples.SimpleTests;
using NetTopologySuite.Coordinates;
using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Various
{
    [TestFixture]
    public class IsValidTest : BaseSamples
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsValidTest"/> class.
        /// </summary>
        public IsValidTest()
            : base(GeometryFactory<BufferedCoordinate2D>.CreateFixedPrecision(
                new BufferedCoordinate2DSequenceFactory())) { }

        [Test]
        public void IsCCWBugTest()
        {
            IGeometry g = Reader.Read("POLYGON ((60.0 40.0, 60.0 240.0, 460.0 240.0, 460.0 40.0, 60.0 40.0), (260.0 200.0, 340.0 60.0, 400.0 120.0, 260.0 200.0), (260.0 200.0, 120.0 100.0, 200.0 60.0, 260.0 200.0))");
            Assert.IsNotNull(g);
            bool result = g.IsValid;
            Assert.IsTrue(result);
        }
    }
}
