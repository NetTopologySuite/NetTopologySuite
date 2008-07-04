using System;
using System.Diagnostics;
using System.Globalization;
using GeoAPI.Geometries;
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
    public class FormattingTest : BaseSamples
    {
        private const double longDouble = 1.2345678901234567890;

        NumberFormatInfo nfi = null;

        /// <summary>
        /// 
        /// </summary>
        public FormattingTest() : base() 
        {
            nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void DoubleFormattingFixedTest()
        {            
            nfi.NumberDecimalDigits = 1;
            TestDoubleValueResult();
        }    

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void DoubleFormattingFloatingTest()
        {
            nfi.NumberDecimalDigits = 10;
            TestDoubleValueResult();
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FloatFormatting17DigitsTest1()
        {
            Coordinate coordinate = new Coordinate(0.00000000000000000001, 0.00000000000000000001);
            IPoint point = GeometryFactory.Floating.CreatePoint(coordinate);
            IPoint test = (IPoint)new WKTReader(GeometryFactory.Floating).Read(point.ToString());
            
            // If i modify PrecisionModel.MaximumSignificantDigits from 16 to (as example) 20, all the digits are printed... 
            Debug.WriteLine(point.ToString());
            Debug.WriteLine(test.ToString());

            Assert.IsFalse(point.X == 0);
            Assert.IsFalse(point.Y == 0);          
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FloatFormatting9MoreDigitsTest1()
        {
            Coordinate coordinate = new Coordinate(0.0000000000001, 0.0000000000002);
            IPoint point = GeometryFactory.Floating.CreatePoint(coordinate);            
            IPoint test = (IPoint) new WKTReader(GeometryFactory.Floating).Read(point.ToString());
            Debug.WriteLine(point.ToString());
            Debug.WriteLine(test.ToString());

            Assert.AreEqual(test.X, point.X);
            Assert.AreEqual(test.Y, point.Y);
            Boolean result = test.Equals(point);   // Geometry not overrides ==...
            Assert.IsTrue(result);                                                                             
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FloatFormatting9MoreDigitsTest2()
        {
            Coordinate coordinate = new Coordinate(0.0000000000001, 0.0000000000002);
            IPoint point = GeometryFactory.FloatingSingle.CreatePoint(coordinate);
            IPoint test = (IPoint)new WKTReader(GeometryFactory.FloatingSingle).Read(point.ToString());
            Debug.WriteLine(point.ToString());
            Debug.WriteLine(test.ToString());
            
            // Assertis correct because WktReader creates test with coordinates == 0
            // point has the double values as coordinates
            Boolean result = test.Equals(point);   // Remember: Geometry not overrides ==...
            Assert.IsFalse(result); 
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void FloatFormatting9MoreDigitsTest3()
        {
            Coordinate coordinate = new Coordinate(0.0000000000001, 0.0000000000002);
            IPoint point = GeometryFactory.Fixed.CreatePoint(coordinate);
            IPoint test = (IPoint)new WKTReader(GeometryFactory.Fixed).Read(point.ToString());
            Debug.WriteLine(point.ToString());
            Debug.WriteLine(test.ToString());

            // Assertis correct because WktReader creates test with coordinates == 0
            // point has the double values as coordinates
            Boolean result = test.Equals(point);   // Are you read that Geometry not overrides ==...
            Assert.IsFalse(result);
        }

        /// <summary>
        /// 
        /// </summary>
        private void TestDoubleValueResult()
        {
            string result = Convert.ToString(longDouble, nfi);
            Assert.IsNotNull(result);
            Debug.WriteLine(result);
        }
    }
}


