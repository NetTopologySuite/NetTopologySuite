using System;
using System.IO;
using GeoAPI.Coordinates;
using GisSharpBlog.NetTopologySuite;
using GisSharpBlog.NetTopologySuite.Algorithm;
using NetTopologySuite.Coordinates;
using Xunit;

namespace NetTopologySuite.Tests.OperationTests
{
    // ReSharper disable InconsistentNaming
    public class TestRunnerTests
    {
        private const String TestLocation = "..\\..\\..\\NetTopologySuite.TestRunner.Tests\\vivid";

        public TestRunnerTests()
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
        public void TestBoundary()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestBoundary.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestCentroid()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestCentroid.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestConvexHullBig()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestConvexHull-big.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestConvexHull()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestConvexHull.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionAA()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionAA.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionAAPrec()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionAAPrec.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionLA()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionLA.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionLAPrec()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionLAPrec.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionLL()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionLL.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionLLPrec()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionLLPrec.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionPA()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionPA.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionPL()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionPL.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Boolean testResults = tests.RunTests();
            Assert.True(testResults);
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionPLPrec()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionPLPrec.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionPP()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionPP.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestInteriorPoint()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestInteriorPoint.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRectanglePredicate()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestRectanglePredicate.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
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
        public void TestRelateLC()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateLC.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
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

        [Fact]
        public void TestSimple()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests = controller.Load(Path.Combine(TestLocation, "TestSimple.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestValid()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests = controller.Load(Path.Combine(TestLocation, "TestValid.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestValid2_Big()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestValid2-big.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestValid2()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests = controller.Load(Path.Combine(TestLocation, "TestValid2.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestWithinDistance()
        {
            XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();
            XmlTestCollection<BufferedCoordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestWithinDistance.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }
    }
}