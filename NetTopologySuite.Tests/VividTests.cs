using System;
using System.IO;
using GisSharpBlog.NetTopologySuite;
using NUnit.Framework;

namespace NetTopologySuite.Tests
{
    [TestFixture]
    public class TestRunnerTests
    {
        private String _testLocation = "..\\..\\..\\NetTopologySuite.TestRunner.Tests\\vivid";

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
                controller.Load(Path.Combine(_testLocation, "TestBoundary.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestCentroid()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(_testLocation, "TestCentroid.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestConvexHull_Big()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(_testLocation, "TestConvexHull-big.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestConvexHull()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(_testLocation, "TestConvexHull.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestFunctionAA()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(_testLocation, "TestFunctionAA.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestFunctionAAPrec()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(_testLocation, "TestFunctionAAPrec.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestFunctionLA()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(_testLocation, "TestFunctionLA.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestFunctionLAPrec()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(_testLocation, "TestFunctionLAPrec.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestFunctionLL()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(_testLocation, "TestFunctionLL.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestFunctionLLPrec()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(_testLocation, "TestFunctionLLPrec.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestFunctionPA()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(_testLocation, "TestFunctionPA.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestFunctionPL()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(_testLocation, "TestFunctionPL.xml"));
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
                controller.Load(Path.Combine(_testLocation, "TestFunctionPLPrec.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestFunctionPP()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(_testLocation, "TestFunctionPP.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestInteriorPoint()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(_testLocation, "TestInteriorPoint.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestRectanglePredicate()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(_testLocation, "TestRectanglePredicate.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestRelateAA()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(_testLocation, "TestRelateAA.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestRelateAC()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(_testLocation, "TestRelateAC.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestRelateLA()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(_testLocation, "TestRelateLA.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestRelateLC()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(_testLocation, "TestRelateLC.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestRelateLL()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(_testLocation, "TestRelateLL.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestRelatePA()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(_testLocation, "TestRelatePA.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestRelatePL()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(_testLocation, "TestRelatePL.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestRelatePP()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(_testLocation, "TestRelatePP.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestSimple()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests = controller.Load(Path.Combine(_testLocation, "TestSimple.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestValid()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests = controller.Load(Path.Combine(_testLocation, "TestValid.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestValid2_Big()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(_testLocation, "TestValid2-big.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestValid2()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests = controller.Load(Path.Combine(_testLocation, "TestValid2.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }

        [Test]
        public void TestWithinDistance()
        {
            XmlTestController controller = new XmlTestController();
            XmlTestCollection tests =
                controller.Load(Path.Combine(_testLocation, "TestWithinDistance.xml"));
            tests.TestEvent += handleTestEvent;
            Assert.IsTrue(tests.RunTests());
            tests.TestEvent -= handleTestEvent;
        }
    }
}