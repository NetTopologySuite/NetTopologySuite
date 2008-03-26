using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;

using GisSharpBlog.NetTopologySuite.Geometries;

using GisSharpBlog.NetTopologySuite.Samples.SimpleTests;
using NetTopologySuite.Coordinates;
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
            BufferedCoordinate2DFactory coordFactory = new BufferedCoordinate2DFactory();
          
            ILineString lineString = GeoFactory.CreateLineString(new ICoordinate[] 
            { 
                coordFactory.Create(10, 10), 
                coordFactory.Create(20, 20), 
                coordFactory.Create(20, 30), 
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
            ILineString lineString1 = GeoFactory.CreateLineString(new ICoordinate[] 
            { 
                CoordFactory.Create(10, 10), 
                CoordFactory.Create(20, 20), 
                CoordFactory.Create(20, 30), 
            });

            ILineString lineString2 = GeoFactory.CreateLineString(new ICoordinate[] 
            { 
                CoordFactory.Create(12, 12), 
                CoordFactory.Create(24, 24), 
                CoordFactory.Create(36, 36), 
            });

            IMultiLineString multiLineString = GeoFactory.CreateMultiLineString(
                    new ILineString[]
                        {
                             lineString1, lineString2,
                        });

            IMultiLineString reverse = multiLineString.Reverse();

            Debug.WriteLine(multiLineString.ToString());
            Debug.WriteLine(reverse.ToString());

            Assert.AreNotEqual(multiLineString, reverse);
        }
    }
}
