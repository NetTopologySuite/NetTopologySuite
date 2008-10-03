using System;
using System.IO;
using GisSharpBlog.NetTopologySuite;
using NUnit.Framework;

namespace NetTopologySuite.Tests
{
    [TestFixture]
    public class TestRunnerTests
    {
        private const String TestLocation = "..\\..\\..\\NetTopologySuite.TestRunner.Tests\\vivid";

        private static void handleTestEvent(Object sender, XmlTestEventArgs args)
        {
            if (!args.Success)
            {
                Assert.Fail();
            }
        }

        [Test]
        public void TestBoundary()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestBoundary.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestCentroid()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestCentroid.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestConvexHull_Big()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestConvexHull-big.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestConvexHull()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestConvexHull.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestFunctionAA()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionAA.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestFunctionAAPrec()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionAAPrec.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestFunctionLA()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionLA.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestFunctionLAPrec()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionLAPrec.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestFunctionLL()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionLL.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestFunctionLLPrec()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionLLPrec.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestFunctionPA()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionPA.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestFunctionPL()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionPL.xml"));
            tests.TestEvent += handleTestEvent;
            Boolean testResults = tests.RunTests();
            Assert.IsTrue(testResults);
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestFunctionPLPrec()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionPLPrec.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestFunctionPP()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestFunctionPP.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestInteriorPoint()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestInteriorPoint.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestRectanglePredicate()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestRectanglePredicate.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestRelateAA()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateAA.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestRelateAC()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateAC.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestRelateLA()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateLA.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestRelateLC()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateLC.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestRelateLL()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestRelateLL.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestRelatePA()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestRelatePA.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestRelatePL()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestRelatePL.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestRelatePP()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestRelatePP.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestSimple()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests = controller.Load(Path.Combine(TestLocation, "TestSimple.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestValid()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests = controller.Load(Path.Combine(TestLocation, "TestValid.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestValid2_Big()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestValid2-big.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestValid2()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests = controller.Load(Path.Combine(TestLocation, "TestValid2.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestWithinDistance()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(TestLocation, "TestWithinDistance.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }
    }
}