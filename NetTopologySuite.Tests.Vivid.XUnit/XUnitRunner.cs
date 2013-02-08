namespace NetTopologySuite.Tests.XUnit
{
    using System;
    using System.IO;
    using Open.Topology.TestRunner;
    //using Xunit;
    using NUnit.Framework;

    /// <summary>
    /// A class designed to allow debugging of individual tests from within the vivid set
    /// mainly to aid debugging v2 side by side with v1.7.x
    /// </summary>
    [TestFixture]
    public abstract class XUnitRunner
    {
        protected abstract string TestLocation { get; }

        private readonly XmlTestController _controller = new XmlTestController();
        private XmlTestCollection _tests;

        protected XUnitRunner(string testFile)
        {
            this.TestFile = testFile;
        }

        protected string TestFile { get; set; }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
        }

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
            return this._controller.Load(Path.Combine(this.TestLocation, this.TestFile));
        }

        [Test]
        public virtual void TestCountOk()
        {
            if (this.Count > 20)
                this.TestAll();
        }

        [Test]
        public virtual void Test00()
        {
            this.ExecuteTest(0);
        }

        [Test]
        public virtual void Test01()
        {
            this.ExecuteTest(1);
        }

        [Test]
        public virtual void Test02()
        {
            this.ExecuteTest(2);
        }

        [Test]
        public virtual void Test03()
        {
            this.ExecuteTest(3);
        }

        [Test]
        public virtual void Test04()
        {
            this.ExecuteTest(4);
        }

        [Test]
        public virtual void Test05()
        {
            this.ExecuteTest(5);
        }

        [Test]
        public virtual void Test06()
        {
            this.ExecuteTest(6);
        }

        [Test]
        public virtual void Test07()
        {
            this.ExecuteTest(7);
        }

        [Test]
        public virtual void Test08()
        {
            this.ExecuteTest(8);
        }

        [Test]
        public virtual void Test09()
        {
            this.ExecuteTest(9);
        }

        [Test]
        public virtual void Test10()
        {
            this.ExecuteTest(10);
        }

        [Test]
        public virtual void Test11()
        {
            this.ExecuteTest(11);
        }

        [Test]
        public virtual void Test12()
        {
            this.ExecuteTest(12);
        }

        [Test]
        public virtual void Test13()
        {
            this.ExecuteTest(13);
        }

        [Test]
        public virtual void Test14()
        {
            this.ExecuteTest(14);
        }

        [Test]
        public virtual void Test15()
        {
            this.ExecuteTest(15);
        }

        [Test]
        public virtual void Test16()
        {
            this.ExecuteTest(16);
        }

        [Test]
        public virtual void Test17()
        {
            this.ExecuteTest(17);
        }

        [Test]
        public virtual void Test18()
        {
            this.ExecuteTest(18);
        }

        [Test]
        public virtual void Test19()
        {
            this.ExecuteTest(19);
        }

        [Test]
        public virtual void Test20()
        {
            this.ExecuteTest(20);
        }

        private TestResults ExecuteTest(int i)
        {
            if (i >= this.Count)
                return new TestResults("i > Count", true);

            XmlTest test = this.Tests[i];
            var b = test.RunTest();
            return new TestResults(test.Description, b);
        }

        protected virtual void TestAll()
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