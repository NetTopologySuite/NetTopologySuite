using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;

using GisSharpBlog.NetTopologySuite.Samples.SimpleTests;

using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Various
{

    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class WKTTest : BaseSamples
    {
        private WKTWriter writer = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="WKTTest"/> class.
        /// </summary>
        public WKTTest() : base()
        {
            writer = new WKTWriter();
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void WriteZeroBasedCoordinate()
        {
            Geometry point = Factory.CreatePoint(new Coordinate(0.01, 0.02));
            String result = writer.Write(point);
            Debug.Write(result);
            Assert.AreEqual('0', result[7]);
        }
    }
}
