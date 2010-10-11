using System;
using System.Diagnostics;
using System.Globalization;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using NetTopologySuite.Geometries;
using NetTopologySuite.Samples.SimpleTests;
using NetTopologySuite.Coordinates;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Various
{
    [TestFixture]
    public class FormattingTest : BaseSamples
    {
        private const Double PreciseDouble = 1.2345678901234567890D;

        private readonly NumberFormatInfo _numberFormatInfo;

        public FormattingTest()
        {
            _numberFormatInfo = new NumberFormatInfo();
            _numberFormatInfo.NumberDecimalSeparator = ".";
        }

        private void TestDoubleValueResult()
        {
            String result = Convert.ToString(PreciseDouble, _numberFormatInfo);
            Assert.IsNotNull(result);
            Debug.WriteLine(result);
        }

        [Test]
        public void DoubleFormattingFixedTest()
        {
            _numberFormatInfo.NumberDecimalDigits = 1;
            TestDoubleValueResult();
        }

        [Test]
        public void DoubleFormattingFloatingTest()
        {
            _numberFormatInfo.NumberDecimalDigits = 10;
            TestDoubleValueResult();
        }

        [Test]
        public void FloatFormatting17DigitsTest1()
        {
            ICoordinate coordinate = CoordFactory.Create(0.00000000000000000001,
                                                         0.00000000000000000001);
            IGeometryFactory<BufferedCoordinate> floatingFactory =
                new GeometryFactory<BufferedCoordinate>(
                    new BufferedCoordinateSequenceFactory(
                        (BufferedCoordinateFactory) CoordFactory));
            IPoint2D point = (IPoint2D) floatingFactory.CreatePoint(coordinate);
            IWktGeometryReader wktReader = new WktReader<BufferedCoordinate>(floatingFactory, null);
            IPoint2D test = (IPoint2D) wktReader.Read(point.ToString());

            // If i modify PrecisionModel.MaximumSignificantDigits from 16 to (as example) 20, 
            // all the digits are printed... 
            Debug.WriteLine(point.ToString());
            Debug.WriteLine(test.ToString());

            Assert.IsFalse(point.X == 0);
            Assert.IsFalse(point.Y == 0);
        }

        [Test]
        public void FloatFormatting9MoreDigitsTest1()
        {
            ICoordinate coordinate = CoordFactory.Create(0.0000000000001, 0.0000000000002);
            IGeometryFactory<BufferedCoordinate> floatingFactory =
                new GeometryFactory<BufferedCoordinate>(
                    new BufferedCoordinateSequenceFactory(
                        (BufferedCoordinateFactory) CoordFactory));
            IPoint2D point = (IPoint2D) floatingFactory.CreatePoint(coordinate);
            IWktGeometryReader wktReader = new WktReader<BufferedCoordinate>(floatingFactory, null);
            IPoint2D test = (IPoint2D) wktReader.Read(point.ToString());

            Debug.WriteLine(point.ToString());
            Debug.WriteLine(test.ToString());

            Assert.AreEqual(test.X, point.X);
            Assert.AreEqual(test.Y, point.Y);
            Boolean result = test.Equals(point); // Geometry not overrides ==...
            Assert.IsTrue(result);
        }

        [Test]
        public void FloatFormatting9MoreDigitsTest2()
        {
            ICoordinate coordinate = CoordFactory.Create(0.0000000000001, 0.0000000000002);
            IGeometryFactory<BufferedCoordinate> floatingFactory =
                new GeometryFactory<BufferedCoordinate>(
                    new BufferedCoordinateSequenceFactory(
                        new BufferedCoordinateFactory(PrecisionModelType.SingleFloating)));
            IPoint2D point = (IPoint2D) floatingFactory.CreatePoint(coordinate);
            IWktGeometryReader wktReader = new WktReader<BufferedCoordinate>(floatingFactory, null);
            IPoint2D test = (IPoint2D) wktReader.Read(point.ToString());

            Debug.WriteLine(point.ToString());
            Debug.WriteLine(test.ToString());

            // Assertis correct because WktReader creates test with coordinates == 0
            // point has the Double values as coordinates
            Boolean result = test.Equals(point); // Remember: Geometry not overrides ==...
            Assert.IsFalse(result);
        }

        [Test]
        public void FloatFormatting9MoreDigitsTest3()
        {
            ICoordinate coordinate = CoordFactory.Create(0.0000000000001, 0.0000000000002);
            IGeometryFactory<BufferedCoordinate> fixedFactory =
                new GeometryFactory<BufferedCoordinate>(
                    new BufferedCoordinateSequenceFactory(
                        new BufferedCoordinateFactory(PrecisionModelType.SingleFloating)));
            IPoint2D point = (IPoint2D) fixedFactory.CreatePoint(coordinate);
            IWktGeometryReader wktReader = new WktReader<BufferedCoordinate>(fixedFactory, null);
            IPoint2D test = (IPoint2D) wktReader.Read(point.ToString());

            Debug.WriteLine(point.ToString());
            Debug.WriteLine(test.ToString());

            // Assertis correct because WktReader creates test with coordinates == 0
            // point has the Double values as coordinates
            Boolean result = test.Equals(point); // Are you read that Geometry not overrides ==...
            Assert.IsFalse(result);
        }
    }
}