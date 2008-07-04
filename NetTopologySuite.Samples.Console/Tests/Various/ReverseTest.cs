using System.Diagnostics;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Samples.SimpleTests;
using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Various
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
            ILineString lineString = Factory.CreateLineString(new ICoordinate[] 
            { 
                new Coordinate(10, 10), 
                new Coordinate(20, 20), 
                new Coordinate(20, 30), 
            });            
            ILineString reverse = lineString.Reverse();

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
            ILineString lineString1 = Factory.CreateLineString(new ICoordinate[] 
            { 
                new Coordinate(10, 10), 
                new Coordinate(20, 20), 
                new Coordinate(20, 30), 
            });
            ILineString lineString2 = Factory.CreateLineString(new ICoordinate[] 
            { 
                new Coordinate(12, 12), 
                new Coordinate(24, 24), 
                new Coordinate(36, 36), 
            });
            IMultiLineString multiLineString = Factory.CreateMultiLineString(new ILineString[] { lineString1, lineString2, });
            IMultiLineString reverse = multiLineString.Reverse();

            Debug.WriteLine(multiLineString.ToString());
            Debug.WriteLine(reverse.ToString());

            Assert.AreNotEqual(multiLineString, reverse);
        }
    }
}
