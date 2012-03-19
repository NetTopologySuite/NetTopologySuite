using System;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance
{
    [TestFixture]
    public abstract class PerformanceTestCase
    {
        private readonly String _name;

        protected PerformanceTestCase(String name)
        {
            _name = name;
        }

        public String Name
        {
            get { return _name; }
        }

        public int[] RunSize { get; set; }

        public int RunIterations { get; set; }

        [TestFixtureSetUp]
        public virtual void SetUp()
        {

        }

        public virtual void StartRun(int size)
        {
        }

        public virtual void EndRun()
        {
        }

        [TestFixtureTearDown]
        public virtual void TearDown()
        {

        }
    }
}