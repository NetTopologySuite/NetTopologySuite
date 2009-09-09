using System;
using System.IO;
using GeoAPI.Coordinates;
using GisSharpBlog.NetTopologySuite;
using GisSharpBlog.NetTopologySuite.Algorithm;
using NetTopologySuite.Coordinates;
using Xunit;

namespace NetTopologySuite.Tests.OperationTests
{
    public class RobustTests
    {
        private const String TestLocation = "..\\..\\..\\NetTopologySuite.TestRunner.Tests\\robust";

        public RobustTests()
        {
            RobustLineIntersector<BufferedCoordinate>.FloatingPrecisionCoordinateFactory =
                new BufferedCoordinateFactory();
        }

        private static void HandleTestEvent(Object sender, XmlTestEventArgs<BufferedCoordinate> args)
        {
            Assert.True(args.Success);
        }

        private static ICoordinateFactory<BufferedCoordinate> CreateCoordinateFactory(PrecisionModelType type, Double scale)
        {
            if (Double.IsNaN(scale))
                return new BufferedCoordinateFactory(type);
            return new BufferedCoordinateFactory(scale);
        }

        public static ICoordinateSequenceFactory<BufferedCoordinate> CreateCoordinateSequenceFactory(ICoordinateFactory<BufferedCoordinate> coordinateFactory)
        {
            return new BufferedCoordinateSequenceFactory((BufferedCoordinateFactory)coordinateFactory);
        }

        [Fact]
        public void TestRobustOverlayFixed()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestRobustOverlayFixed.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRobustOverlayFloat()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestRobustOverlayFloat.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRobustRelate()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestRobustRelate.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }
        [Fact]
        public void ExternalRobustness()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "ExternalRobustness.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }
    }
}