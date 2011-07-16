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
    public class RobustTests
    {
        private const String TestLocation = "..\\..\\..\\NetTopologySuite.TestRunner.Tests\\robust";

        public RobustTests()
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
        public void TestRobustOverlayFixed()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestRobustOverlayFixed.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRobustOverlayFloat()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestRobustOverlayFloat.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRobustRelate()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestRobustRelate.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }
        [Fact]
        public void ExternalRobustness()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "ExternalRobustness.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }
    }
}