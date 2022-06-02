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
            if (!string.IsNullOrWhiteSpace(TestFile))
            {
                string testPath = Path.Combine(TestRunnerDirectory, this.TestLocation, this.TestFile);
                if (!System.IO.File.Exists(testPath))
                    throw new IgnoreException($"'{testPath}' not found");
            }
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

        private void Test(int index)
        {
            var res = this.ExecuteTest(index);
            Assert.That(res.Success, Is.True, $"Test '{res.Description}' failed.");
        }

        [Test]
        public virtual void Test00() => Test(0);

        [Test]
        public virtual void Test01() => Test(1);

        [Test]
        public virtual void Test02() => Test(2);

        [Test]
        public virtual void Test03() => Test(3);

        [Test]
        public virtual void Test04() => Test(4);

        [Test]
        public virtual void Test05() => Test(5);

        [Test]
        public virtual void Test06() => Test(6);

        [Test]
        public virtual void Test07() => Test(7);

        [Test]
        public virtual void Test08() => Test(8);

        [Test]
        public virtual void Test09() => Test(9);

        [Test]
        public virtual void Test10() => Test(10);

        [Test]
        public virtual void Test11() => Test(11);

        [Test]
        public virtual void Test12() => Test(12);

        [Test]
        public virtual void Test13() => Test(13);

        [Test]
        public virtual void Test14() => Test(14);

        [Test]
        public virtual void Test15() => Test(15);

        [Test]
        public virtual void Test16() => Test(16);

        [Test]
        public virtual void Test17() => Test(17);

        [Test]
        public virtual void Test18() => Test(18);

        [Test]
        public virtual void Test19() => Test(19);

        [Test]
        public virtual void Test20() => Test(20);

        private TestResults ExecuteTest(int i)
        {
            if (i >= this.Count)
            {
                Assert.Ignore($"Index out of range (max = {Count})");
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
            // Hack to debug test built on Windows using WSL
            if (Environment.OSVersion.Platform == PlatformID.Unix && thisFilePath[1] == ':')
            {
                thisFilePath = thisFilePath.Replace('\\', '/');
                thisFilePath = thisFilePath.Replace(thisFilePath.Substring(0, 2), $"/mnt/{thisFilePath.Substring(0, 1).ToLowerInvariant()}");
            }

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
