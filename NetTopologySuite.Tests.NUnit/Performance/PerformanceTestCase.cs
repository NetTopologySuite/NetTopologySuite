using System;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance
{
    /// <summary>
    /// A base class for classes implementing performance tests
    /// to be run by the <see cref="PerformanceTestRunner"/>.
    /// <para/>
    /// All public methods in a class which start with "run" are 
    /// executed as performance tests.
    /// <para/>
    /// Multiple test runs with different run sizes may be made.
    /// Within each run, each run method is executed 
    /// the specified number of iterations.
    /// The time to run the method is printed for each one.
    /// </summary>
    /// <author>Martin Davis</author>
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

        /// <summary>
        /// Sets up any fixtures needed for the test runs
        /// </summary>
        /// <exception cref="Exception"></exception>
        [TestFixtureSetUp]
        public virtual void SetUp()
        {

        }

        /// <summary>
        /// Starts a test run with the given size.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public virtual void StartRun(int size)
        {
        }

        /// <summary>
        /// Ends a test rund
        /// </summary>
        /// <exception cref="Exception"></exception>
        public virtual void EndRun()
        {
        }

        /// <summary>
        /// Tear down any fixtures made fot the testing
        /// </summary>
        /// <exception cref="Exception"></exception>
        [TestFixtureTearDown]
        public virtual void TearDown()
        {

        }
    }
}