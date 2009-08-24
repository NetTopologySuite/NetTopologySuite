using System;
using System.IO;
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

        private static void HandleTestEvent(Object sender, XmlTestEventArgs args)
        {
            Assert.True(args.Success);
        }

        [Fact]
        public void TestBoundary()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestBoundary.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestCentroid()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestCentroid.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestConvexHullBig()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestConvexHull-big.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestConvexHull()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestConvexHull.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionAA()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionAA.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionAAPrec()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionAAPrec.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionLA()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionLA.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionLAPrec()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionLAPrec.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionLL()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionLL.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionLLPrec()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionLLPrec.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionPA()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionPA.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionPL()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionPL.xml"));
            tests.TestEvent += HandleTestEvent;
            Boolean testResults = tests.RunTests();
            Assert.True(testResults);
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionPLPrec()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionPLPrec.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestFunctionPP()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionPP.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestInteriorPoint()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestInteriorPoint.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRectanglePredicate()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestRectanglePredicate.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelateAA()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateAA.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelateAC()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateAC.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelateLA()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateLA.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelateLC()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateLC.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelateLL()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateLL.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelatePA()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestRelatePA.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelatePL()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestRelatePL.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestRelatePP()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestRelatePP.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestSimple()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests = controller.Load(Path.Combine(TestLocation, "TestSimple.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestValid()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests = controller.Load(Path.Combine(TestLocation, "TestValid.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestValid2_Big()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestValid2-big.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestValid2()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests = controller.Load(Path.Combine(TestLocation, "TestValid2.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }

        [Fact]
        public void TestWithinDistance()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestWithinDistance.xml"));
            tests.TestEvent += HandleTestEvent;
            Assert.True(tests.RunTests());
            tests.TestEvent -= HandleTestEvent;
        }
    }
}