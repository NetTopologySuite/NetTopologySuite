using System;
using NetTopologySuite.Samples.SimpleTests.ShapeTests;
using NUnit.Framework;

namespace NetTopologySuite.Samples.SimpleTests
{
    [TestFixture]
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            new Program().Start();
        }

        private static BaseSamples _sample;

        public void Start()
        {
            SamplesTest(true);
        }

        [Test]
        public void SamplesTest()
        {
            SamplesTest(false);
        }

        public void SamplesTest(bool readLine)
        {
            try
            {
                _sample = new GMLTesting();
                _sample.Start();
                Console.WriteLine();
            }
            finally
            {
                if (readLine) Console.ReadLine();
            }
        }
    }
}
