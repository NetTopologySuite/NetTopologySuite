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
    // ReSharper disable InconsistentNaming
    public class TestRunnerTests
    {
        private const String TestLocation = "..\\..\\..\\NetTopologySuite.TestRunner.Tests\\vivid";

        public TestRunnerTests()
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
        public void TestBoundary()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestBoundary.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestCentroid()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestCentroid.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestConvexHullBig()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestConvexHull-big.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestConvexHull()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestConvexHull.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionAA()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionAA.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionAAPrec()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionAAPrec.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionLA()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionLA.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionLAPrec()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionLAPrec.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionLL()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionLL.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionLLPrec()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionLLPrec.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionPA()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionPA.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionPL()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionPL.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Boolean testResults = tests.RunTests();
            Assert.True(testResults);
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionPLPrec()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionPLPrec.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionPP()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionPP.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestInteriorPoint()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestInteriorPoint.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRectanglePredicate()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestRectanglePredicate.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
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
        public void TestRelateLC()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateLC.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
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

        [Fact]
        public void TestSimple()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests = controller.Load(Path.Combine(TestLocation, "TestSimple.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestValid()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests = controller.Load(Path.Combine(TestLocation, "TestValid.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestValid2_Big()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestValid2-big.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestValid2()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests = controller.Load(Path.Combine(TestLocation, "TestValid2.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestWithinDistance()
        {
            XmlTestController<coord> controller = new XmlTestController<coord>();
            XmlTestCollection<coord> tests =
                controller.Load(Path.Combine(TestLocation, "TestWithinDistance.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }
    }
}