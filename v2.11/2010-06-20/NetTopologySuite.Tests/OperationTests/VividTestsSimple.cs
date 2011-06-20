using System;
using System.IO;
using GeoAPI.Coordinates;
using GisSharpBlog.NetTopologySuite;
using GisSharpBlog.NetTopologySuite.Algorithm;
using NetTopologySuite.Coordinates;
using NetTopologySuite.Coordinates.Simple;
using Xunit;

namespace NetTopologySuite.Tests.OperationTests
{
    // ReSharper disable InconsistentNaming
    public class VividTestsSimple
    {
        private const String TestLocation = "..\\..\\..\\NetTopologySuite.TestRunner.Tests\\vivid";

        public VividTestsSimple()
        {
            RobustLineIntersector<Coordinate>.FloatingPrecisionCoordinateFactory =
                new CoordinateFactory();
        }

        private static void HandleTestEvent(Object sender, XmlTestEventArgs<Coordinate> args)
        {
            Assert.True(args.Success);
        }

        private static ICoordinateFactory<Coordinate> CreateCoordinateFactory(PrecisionModelType type, Double scale)
        {
            if (Double.IsNaN(scale))
                return new CoordinateFactory(type);
            return new CoordinateFactory(scale);
        }

        public static ICoordinateSequenceFactory<Coordinate> CreateCoordinateSequenceFactory(ICoordinateFactory<Coordinate> coordinateFactory)
        {
            return new CoordinateSequenceFactory((CoordinateFactory)coordinateFactory);
        }
        [Fact]
        public void TestBoundary()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestBoundary.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestCentroid()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestCentroid.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestConvexHullBig()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestConvexHull-big.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestConvexHull()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestConvexHull.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionAA()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionAA.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionAAPrec()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionAAPrec.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionLA()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionLA.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionLAPrec()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionLAPrec.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionLL()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionLL.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionLLPrec()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionLLPrec.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionPA()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionPA.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionPL()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionPL.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Boolean testResults = tests.RunTests();
            Assert.True(testResults);
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionPLPrec()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionPLPrec.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionPP()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionPP.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestInteriorPoint()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestInteriorPoint.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRectanglePredicate()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestRectanglePredicate.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelateAA()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateAA.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelateAC()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateAC.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelateLA()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateLA.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelateLC()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateLC.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelateLL()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateLL.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelatePA()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelatePA.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelatePL()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelatePL.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelatePP()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestRelatePP.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestSimple()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests = controller.Load(Path.Combine(TestLocation, "TestSimple.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestValid()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests = controller.Load(Path.Combine(TestLocation, "TestValid.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestValid2_Big()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestValid2-big.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestValid2()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests = controller.Load(Path.Combine(TestLocation, "TestValid2.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestWithinDistance()
        {
            XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();
            XmlTestCollection<Coordinate> tests =
                controller.Load(Path.Combine(TestLocation, "TestWithinDistance.xml"), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }
    }
}