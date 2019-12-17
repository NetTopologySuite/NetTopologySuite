namespace NetTopologySuite.Tests.XUnit
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    //using Xunit;
    using NUnit.Framework;
    using Open.Topology.TestRunner;

    /// <summary>
    /// A class designed to allow debugging of individual tests from within the vivid set
    /// mainly to aid debugging v2 side by side with v1.7.x
    /// </summary>
    [TestFixture]
    public abstract class XUnitRunner
    {
        protected static readonly string TestRunnerDirectory = GetTestRunnerTestDirectory();

        protected abstract string TestLocation { get; }

        private readonly XmlTestController _controller = new XmlTestController();
        private XmlTestCollection _tests;

        protected XUnitRunner(string testFile)
        {
            this.TestFile = Path.Combine(testFile.Split('\\'));
        }

        protected string TestFile { get; set; }

        [OneTimeSetUp]
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

        public int Count => this.Tests.Count;

        protected XmlTestCollection LoadTests()
        {
            return this._controller.Load(Path.Combine(TestRunnerDirectory,this.TestLocation, this.TestFile));
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
            Assert.That(this.ExecuteTest(0).Success, Is.True);
        }

        [Test]
        public virtual void Test01()
        {
            Assert.That(this.ExecuteTest(1).Success, Is.True);
        }

        [Test]
        public virtual void Test02()
        {
            Assert.That(this.ExecuteTest(2).Success, Is.True);
        }

        [Test]
        public virtual void Test03()
        {
            Assert.That(this.ExecuteTest(3).Success, Is.True);
        }

        [Test]
        public virtual void Test04()
        {
            Assert.That(this.ExecuteTest(4).Success, Is.True);
        }

        [Test]
        public virtual void Test05()
        {
            Assert.That(this.ExecuteTest(5).Success, Is.True);
        }

        [Test]
        public virtual void Test06()
        {
            Assert.That(this.ExecuteTest(6).Success, Is.True);
        }

        [Test]
        public virtual void Test07()
        {
            Assert.That(this.ExecuteTest(7).Success, Is.True);
        }

        [Test]
        public virtual void Test08()
        {
            Assert.That(this.ExecuteTest(8).Success, Is.True);
        }

        [Test]
        public virtual void Test09()
        {
            Assert.That(this.ExecuteTest(9).Success, Is.True);
        }

        [Test]
        public virtual void Test10()
        {
            Assert.That(this.ExecuteTest(10).Success, Is.True);
        }

        [Test]
        public virtual void Test11()
        {
            Assert.That(this.ExecuteTest(11).Success, Is.True);
        }

        [Test]
        public virtual void Test12()
        {
            Assert.That(this.ExecuteTest(12).Success, Is.True);
        }

        [Test]
        public virtual void Test13()
        {
            Assert.That(this.ExecuteTest(13).Success, Is.True);
        }

        [Test]
        public virtual void Test14()
        {
            Assert.That(this.ExecuteTest(14).Success, Is.True);
        }

        [Test]
        public virtual void Test15()
        {
            Assert.That(this.ExecuteTest(15).Success, Is.True);
        }

        [Test]
        public virtual void Test16()
        {
            Assert.That(this.ExecuteTest(16).Success, Is.True);
        }

        [Test]
        public virtual void Test17()
        {
            Assert.That(this.ExecuteTest(17).Success, Is.True);
        }

        [Test]
        public virtual void Test18()
        {
            Assert.That(this.ExecuteTest(18).Success, Is.True);
        }

        [Test]
        public virtual void Test19()
        {
            Assert.That(this.ExecuteTest(19).Success, Is.True);
        }

        [Test]
        public virtual void Test20()
        {
            Assert.That(this.ExecuteTest(20).Success, Is.True);
        }

        private TestResults ExecuteTest(int i)
        {
            if (i >= this.Count)
            {
                Assert.Ignore($"Index out of range (max = {Count}");
                return new TestResults("i > Count", true);
            }

            var test = this.Tests[i];
            bool b = test.RunTest();

            return new TestResults(test.Description, b);
        }

        protected virtual void TestAll()
        {
            bool success = true;
            for (int i = 0; i < this.Count; i++)
            {
                try
                {
                    var result = this.ExecuteTest(i);
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

        private static string GetTestRunnerTestDirectory([CallerFilePath] string thisFilePath = null)
        {
            return new FileInfo(thisFilePath)                            // /test/NetTopologySuite.Tests.Vivid.XUnit/XUnitRunner.cs
                .Directory                                               // /test/NetTopologySuite.Tests.Vivid.XUnit
                .Parent                                                  // /test
                .Parent                                                  // /
                .GetDirectories("data")[0]                               // /data
                .GetDirectories("NetTopologySuite.TestRunner.Tests")[0]  // /data/NetTopologySuite.TestRunner.Tests
                .FullName;
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
