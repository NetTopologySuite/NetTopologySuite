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
    public class ReverseTest : BaseSamples
    {
        /// <summary>
        /// 
        /// </summary>
        public ReverseTest() : base() { }        

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void LineStringReverseTest()
        {
            LineString lineString = Factory.CreateLineString(new Coordinate[] 
                { 
                    new Coordinate(10, 10), 
                    new Coordinate(20, 20), 
                    new Coordinate(20, 30), 
                });            
            LineString reverse = lineString.Reverse();

            Debug.WriteLine(lineString.ToString());
            Debug.WriteLine(reverse.ToString());
            
            Assert.AreNotEqual(lineString, reverse);            
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void MultiLineStringReverseTest()
        {
            LineString lineString1 = Factory.CreateLineString(new Coordinate[] 
                { 
                    new Coordinate(10, 10), 
                    new Coordinate(20, 20), 
                    new Coordinate(20, 30), 
                });

            LineString lineString2 = Factory.CreateLineString(new Coordinate[] 
                { 
                    new Coordinate(12, 12), 
                    new Coordinate(24, 24), 
                    new Coordinate(36, 36), 
                });
            MultiLineString multiLineString = Factory.CreateMultiLineString(new LineString[] { lineString1, lineString2, });
            MultiLineString reverse = multiLineString.Reverse();

            Debug.WriteLine(multiLineString.ToString());
            Debug.WriteLine(reverse.ToString());

            Assert.AreNotEqual(multiLineString, reverse);
        }
    }
}
