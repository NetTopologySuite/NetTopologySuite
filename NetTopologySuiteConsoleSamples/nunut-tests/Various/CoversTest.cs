using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;

using GisSharpBlog.NetTopologySuite.Samples.SimpleTests;

using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Samples.NUnitTests
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class CoversTest : BaseSamples
    {
        private Polygon polygon1 = null;
        private Polygon polygon2 = null;

        Coordinate[] array1 = new Coordinate[]  {   new Coordinate(10, 10), 
                                                    new Coordinate(50, 10),
                                                    new Coordinate(50, 50),
                                                    new Coordinate(10, 50),
                                                    new Coordinate(10, 10), };

        Coordinate[] array2 = new Coordinate[]  {   new Coordinate(11, 11), 
                                                    new Coordinate(20, 11),
                                                    new Coordinate(20, 20),
                                                    new Coordinate(11, 20),
                                                    new Coordinate(11, 11), };

        /// <summary>
        /// 
        /// </summary>
        public CoversTest() : base()
        {            
            polygon1 = new Polygon(new LinearRing(array1));
            polygon2 = new Polygon(new LinearRing(array2));

        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void CoversTestTest()
        {
            Assert.IsTrue(polygon2.Within(polygon1));
            Assert.IsTrue(polygon1.Covers(polygon2));
            Assert.IsFalse(polygon1.CoveredBy(polygon2));
            
        }        
    }
}
