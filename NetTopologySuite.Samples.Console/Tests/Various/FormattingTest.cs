using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
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
    public class FormattingTest : BaseSamples
    {
        private const Double longDouble = 1.2345678901234567890;

        NumberFormatInfo nfi;

        /// <summary>
        /// 
        /// </summary>
        public FormattingTest()
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
            ICoordinate coordinate = CoordFactory.Create(0.00000000000000000001, 0.00000000000000000001);
            IGeometryFactory<BufferedCoordinate2D> floatingFactory =
                GeometryFactory<BufferedCoordinate2D>.CreateFloatingPrecision(
                    new BufferedCoordinate2DSequenceFactory((BufferedCoordinate2DFactory)CoordFactory));
            IPoint2D point = (IPoint2D)floatingFactory.CreatePoint(coordinate);
            IWktGeometryReader wktReader = new WktReader<BufferedCoordinate2D>(floatingFactory, null);
            IPoint2D test = (IPoint2D)wktReader.Read(point.ToString());
            
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
            ICoordinate coordinate = CoordFactory.Create(0.0000000000001, 0.0000000000002);
            IGeometryFactory<BufferedCoordinate2D> floatingFactory =
                GeometryFactory<BufferedCoordinate2D>.CreateFloatingPrecision(
                    new BufferedCoordinate2DSequenceFactory((BufferedCoordinate2DFactory)CoordFactory));
            IPoint2D point = (IPoint2D)floatingFactory.CreatePoint(coordinate);
            IWktGeometryReader wktReader = new WktReader<BufferedCoordinate2D>(floatingFactory, null);
            IPoint2D test = (IPoint2D)wktReader.Read(point.ToString());

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
            ICoordinate coordinate = CoordFactory.Create(0.0000000000001, 0.0000000000002);
            IGeometryFactory<BufferedCoordinate2D> floatingFactory =
                GeometryFactory<BufferedCoordinate2D>.CreateFloatingSinglePrecision(
                    new BufferedCoordinate2DSequenceFactory((BufferedCoordinate2DFactory)CoordFactory));
            IPoint2D point = (IPoint2D)floatingFactory.CreatePoint(coordinate);
            IWktGeometryReader wktReader = new WktReader<BufferedCoordinate2D>(floatingFactory, null);
            IPoint2D test = (IPoint2D)wktReader.Read(point.ToString());

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
            ICoordinate coordinate = CoordFactory.Create(0.0000000000001, 0.0000000000002);
            IGeometryFactory<BufferedCoordinate2D> fixedFactory =
                GeometryFactory<BufferedCoordinate2D>.CreateFloatingSinglePrecision(
                    new BufferedCoordinate2DSequenceFactory((BufferedCoordinate2DFactory)CoordFactory));
            IPoint2D point = (IPoint2D)fixedFactory.CreatePoint(coordinate);
            IWktGeometryReader wktReader = new WktReader<BufferedCoordinate2D>(fixedFactory, null);
            IPoint2D test = (IPoint2D)wktReader.Read(point.ToString());

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


