using System;
using System.IO;
using GeoAPI.Coordinates;
using GisSharpBlog.NetTopologySuite;
using GisSharpBlog.NetTopologySuite.Algorithm;
using NetTopologySuite.Coordinates;
using Xunit;

namespace NetTopologySuite.Tests.OperationTests
{
    public class ValidateTests
    {
        private const String TestLocation = "..\\..\\..\\NetTopologySuite.TestRunner.Tests\\Validate";

        public ValidateTests()
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
        public void TestRelateAA()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateAA.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelateAABig()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateAA-big.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelateAC()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateAC.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelateLA()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateLA.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelateLL()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateLL.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelatePA()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelatePA.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelatePL()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelatePL.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelatePP()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelatePP.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

    }
}
