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
    public class RobustLineIntersectorTest : BaseSamples
    {
        
        private const string lineString1 = "LINESTRING (2089426.5233462777 1180182.3877339689, 2085646.6891757075 1195618.7333999649)";
        private const string lineString2 = "LINESTRING (1889281.8148903656 1997547.0560044837, 2259977.3672235999 483675.17050843034)";
        private const string point1 = "POINT (2097408.2633752143 1144595.8008114607)";        

        /// <summary>
        /// Initializes a new instance of the <see cref="RobustLineIntersectorTest"/> class.
        /// </summary>
        public RobustLineIntersectorTest() : base(GeometryFactory.Fixed) { }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void IntersectionTest()
        {
            Geometry g1 = Reader.Read(lineString1);
            Geometry g2 = Reader.Read(lineString2);
            Geometry p1 = Reader.Read(point1);

            Geometry result = g1.Intersection(g2);            
            
            Debug.WriteLine(result);
            Assert.IsNotNull(result);            
        }

    }
}
