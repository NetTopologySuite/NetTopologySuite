using System;
using System.IO;
using GisSharpBlog.NetTopologySuite;
using Xunit;

namespace NetTopologySuite.Tests.Vivid.XUnit
{
    /// <summary>
    /// A class designed to allow debugging of individual tests from within the vivid set
    /// mainly to aid debugging v2 side by side with v1.7.x
    /// </summary>
    public abstract class XUnitRunner
    {
        private const String TestLocation = "..\\..\\..\\NetTopologySuite.TestRunner.Tests\\vivid";

        private readonly XmlTestController controller = new XmlTestController();
        private XmlTestCollection _tests;

        protected XUnitRunner(string testFile)
        {
            TestFile = testFile;
        }

        private string TestFile { get; set; }

        protected XmlTestCollection Tests
        {
            get
            {
                _tests = _tests ?? LoadTests();
                return _tests;
            }
        }

        public Int32 Count
        {
            get { return Tests.Count; }
        }

        private XmlTestCollection LoadTests()
        {
            return controller.Load(Path.Combine(TestLocation, TestFile));
        }

        [Fact]
        public void TestCountOk()
        {
            if (Count > 20)
                TestAll();
        }

        [Fact]
        public void Test1()
        {
            ExecuteTest(1);
        }

        [Fact]
        public void Test2()
        {
            ExecuteTest(2);
        }

        [Fact]
        public void Test3()
        {
            ExecuteTest(3);
        }

        [Fact]
        public void Test4()
        {
            ExecuteTest(4);
        }

        [Fact]
        public void Test5()
        {
            ExecuteTest(5);
        }

        [Fact]
        public void Test6()
        {
            ExecuteTest(6);
        }

        [Fact]
        public void Test7()
        {
            ExecuteTest(7);
        }

        [Fact]
        public void Test8()
        {
            ExecuteTest(8);
        }

        [Fact]
        public void Test9()
        {
            ExecuteTest(9);
        }

        [Fact]
        public void Test10()
        {
            ExecuteTest(10);
        }

        [Fact]
        public void Test11()
        {
            ExecuteTest(11);
        }

        [Fact]
        public void Test12()
        {
            ExecuteTest(12);
        }

        [Fact]
        public void Test13()
        {
            ExecuteTest(13);
        }

        [Fact]
        public void Test14()
        {
            ExecuteTest(14);
        }

        [Fact]
        public void Test15()
        {
            ExecuteTest(15);
        }

        [Fact]
        public void Test16()
        {
            ExecuteTest(16);
        }

        [Fact]
        public void Test17()
        {
            ExecuteTest(17);
        }

        [Fact]
        public void Test18()
        {
            ExecuteTest(18);
        }

        [Fact]
        public void Test19()
        {
            ExecuteTest(19);
        }

        [Fact]
        public void Test20()
        {
            ExecuteTest(20);
        }

        private void ExecuteTest(int i)
        {
            if (i < Count)
                Tests[i].RunTest();
        }

        //some test files contain hundreds of tests..

        public void TestAll()
        {
            for (int i = 0; i < Count; i++)
                ExecuteTest(i);
        }
    }
}
