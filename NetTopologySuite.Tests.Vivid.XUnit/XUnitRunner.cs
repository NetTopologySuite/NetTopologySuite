namespace NetTopologySuite.Tests.XUnit
{
    using System;
    using System.IO;
    using Open.Topology.TestRunner;
    using Xunit;

    /// <summary>
    /// A class designed to allow debugging of individual tests from within the vivid set
    /// mainly to aid debugging v2 side by side with v1.7.x
    /// </summary>
    public abstract class XUnitRunner
    {
        protected abstract string TestLocation { get; }

        private readonly XmlTestController controller = new XmlTestController();
        private XmlTestCollection _tests;

        protected XUnitRunner(string testFile)
        {
            this.TestFile = testFile;
        }

        protected string TestFile { get; set; }

        protected XmlTestCollection Tests
        {
            get
            {
                this._tests = this._tests ?? this.LoadTests();
                return this._tests;
            }
        }

        public Int32 Count
        {
            get { return this.Tests.Count; }
        }

        protected XmlTestCollection LoadTests()
        {
            return this.controller.Load(Path.Combine(this.TestLocation, this.TestFile));
        }

        //[Fact]
        public void TestCountOk()
        {
            if (this.Count > 20)
                this.TestAll();
        }

        //[Fact]
        public void Test1()
        {
            this.ExecuteTest(1);
        }

        //[Fact]
        public void Test2()
        {
            this.ExecuteTest(2);
        }

        //[Fact]
        public void Test3()
        {
            this.ExecuteTest(3);
        }

        //[Fact]
        public void Test4()
        {
            this.ExecuteTest(4);
        }

        //[Fact]
        public void Test5()
        {
            this.ExecuteTest(5);
        }

        //[Fact]
        public void Test6()
        {
            this.ExecuteTest(6);
        }

        //[Fact]
        public void Test7()
        {
            this.ExecuteTest(7);
        }

        //[Fact]
        public void Test8()
        {
            this.ExecuteTest(8);
        }

        //[Fact]
        public void Test9()
        {
            this.ExecuteTest(9);
        }

        //[Fact]
        public void Test10()
        {
            this.ExecuteTest(10);
        }

        //[Fact]
        public void Test11()
        {
            this.ExecuteTest(11);
        }

        //[Fact]
        public void Test12()
        {
            this.ExecuteTest(12);
        }

        //[Fact]
        public void Test13()
        {
            this.ExecuteTest(13);
        }

        //[Fact]
        public void Test14()
        {
            this.ExecuteTest(14);
        }

        //[Fact]
        public void Test15()
        {
            this.ExecuteTest(15);
        }

        //[Fact]
        public void Test16()
        {
            this.ExecuteTest(16);
        }

        //[Fact]
        public void Test17()
        {
            this.ExecuteTest(17);
        }

        //[Fact]
        public void Test18()
        {
            this.ExecuteTest(18);
        }

        //[Fact]
        public void Test19()
        {
            this.ExecuteTest(19);
        }

        //[Fact]
        public void Test20()
        {
            this.ExecuteTest(20);
        }

        private TestResults ExecuteTest(int i)
        {
            if (i >= this.Count)
                throw new ArgumentException("i > Count");

            XmlTest test = this.Tests[i];
            var b = test.RunTest();
            return new TestResults(test.Description, b);
        }

        [Fact]
        public virtual void TestAll()
        {            
            bool success = true;
            for (int i = 0; i < this.Count; i++)
            {
                try
                {
                    TestResults result = this.ExecuteTest(i);
                    if (result.Success)
                    {
                        Console.WriteLine("Test {0} success\n{1}", i, result.Description);
                        continue;
                    }

                    Console.WriteLine("Test {0} failed\n{1}", i, result.Description);
                    success = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Test {0} thrown exception\n{1}", i, ex.Message);
                    success = false;
                }                
            }
            Assert.True(success, "Fixture failed");
        }
    }

    public class TestResults
    {
        public TestResults(string description, bool success)
        {
            this.Description = description;
            this.Success = success;
        }

        public string Description { get; private set; }

        public bool Success { get; private set; }

    }
}