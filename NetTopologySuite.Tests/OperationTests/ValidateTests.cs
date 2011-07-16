using System;
using System.IO;
using GeoAPI.Coordinates;
using NetTopologySuite;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Coordinates;
using Xunit;
#if unbuffered
using coord = NetTopologySuite.Coordinates.Simple.Coordinate;
using coordFac = NetTopologySuite.Coordinates.Simple.CoordinateFactory;
using coordSeqFac = NetTopologySuite.Coordinates.Simple.CoordinateSequenceFactory;

#else
using coord = NetTopologySuite.Coordinates.BufferedCoordinate;
using coordFac = NetTopologySuite.Coordinates.BufferedCoordinateFactory;
using coordSeqFac = NetTopologySuite.Coordinates.BufferedCoordinateSequenceFactory;
#endif

namespace NetTopologySuite.Tests.OperationTests
{
    public class ValidateTests
    {
        private const String TestLocation = "..\\..\\..\\NetTopologySuite.TestRunner.Tests\\Validate";

        public ValidateTests()
        {
            RobustLineIntersector<coord>.FloatingPrecisionCoordinateFactory =
                new coordFac();
        }

        private static void HandleTestEvent(Object sender, XmlTestEventArgs<coord> args)
        {
            Assert.True(args.Success);
        }

        private static ICoordinateFactory<coord> CreateCoordinateFactory(PrecisionModelType type, Double scale)
        {
            if (Double.IsNaN(scale))
                return new coordFac(type);
            return new coordFac(scale);
        }

        public static ICoordinateSequenceFactory<coord> CreateCoordinateSequenceFactory(ICoordinateFactory<coord> coordinateFactory)
        {
            return new coordSeqFac((coordFac)coordinateFactory);
        }

        [Fact]
        public void TestRelateAA()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateAA.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelateAABig()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateAA-big.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelateAC()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateAC.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelateLA()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateLA.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelateLL()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateLL.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelatePA()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelatePA.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelatePL()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelatePL.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelatePP()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelatePP.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

    }
}