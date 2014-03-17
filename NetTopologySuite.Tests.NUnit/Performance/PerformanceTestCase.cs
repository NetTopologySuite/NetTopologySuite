using System;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance
{
    /// <summary>
    /// A base class for classes implementing performance tests
    /// to be run by the <see cref="PerformanceTestRunner"/>.
    /// <para/>
    /// In a subclass of this class,
    /// all public methods which start with <c>Run</c> are 
    /// executed as performance tests.
    /// <para/>
    /// Multiple test runs with different run sizes may be made.
    /// Within each run, each <c>Run</c> method is executed 
    /// the specified number of iterations.
    /// The time to run the method is printed for each one.
    /// </summary>
    /// <author>Martin Davis</author>
    [TestFixtureAttribute]
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

        /// <summary>
        /// Gets or sets the size(s) for the runs or the test
        /// </summary>
        public int[] RunSize { get; set; }

        /// <summary>
        /// Gets or sets the number of iterations to execute te test methods in each run
        /// </summary>
        public int RunIterations { get; set; }

        /// <summary>
        /// Sets up any fixtures needed for the test runs
        /// </summary>
        /// <exception cref="Exception"></exception>
        [TestFixtureSetUpAttribute]
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
        [TestFixtureTearDownAttribute]
        public virtual void TearDown()
        {

        }
    }
}