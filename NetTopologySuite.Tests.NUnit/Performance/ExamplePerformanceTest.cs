using System;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance
{
    /// <summary>
    /// An example of the usage of the <see cref="PerformanceTestRunner"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    public class ExamplePerformanceTest : PerformanceTestCase
    {
        private int _iter;

        public ExamplePerformanceTest()
            : this("ExamplePerformanceTest") { }

        public ExamplePerformanceTest(String name)
            : base(name)
        {
            RunSize = new[] {10, 20};
            RunIterations = 10;
        }

        public override void SetUp()
        {
            // read data and allocate resources here
        }

        public override void StartRun(int size)
        {
            Console.WriteLine("Running with size " + size);
            _iter = 0;
        }

        public void RunExample1()
        {
            Console.WriteLine("Iter # " + _iter++);
            // do test work here
        }

        public void RunExample2()
        {
            Console.WriteLine("Iter # " + _iter++);
            // do test work here
        }

        public override void TearDown()
        {
            // deallocate resources here
        }
    }
}