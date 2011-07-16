using System;
using NetTopologySuite.Samples.LinearReferencing;
using NetTopologySuite.SimpleTests;
using NetTopologySuite.SimpleTests.Geometries;
using NUnit.Framework;

namespace NetTopologySuite
{
    [TestFixture]
    public class Program
    {
        [STAThread]
        public static void Main(String[] args)
        {
            new Program().Start();
        }

        private static BaseSamples _sample;

        public void Start()
        {
            SamplesTest();
        }

        [Test]
        public void SamplesTest()
        {
            try
            {
                _sample = new PointSamples();
                _sample.Start();
                Console.WriteLine();

                _sample = new LineStringSamples();
                _sample.Start();
                Console.WriteLine();

                _sample = new PolygonSamples();
                _sample.Start();
                Console.WriteLine();

                _sample = new MultiPointSamples();
                _sample.Start();
                Console.WriteLine();

                _sample = new ValidationSuite();
                _sample.Start();
                Console.WriteLine();
                /*
                sample = new SerializationSamples();
                sample.Start();
                Console.WriteLine();
                */
                /*
				sample = new AttributesTest();
				sample.Start();
				Console.WriteLine();
                */

                /*
				sample = new ShapeRead();
				sample.Start();
				Console.WriteLine();
                */

                /*
				sample = new GMLTesting();
				sample.Start();
				Console.WriteLine();
                */

                new LinearReferencingExample().Run();
            }
            finally
            {
                Console.ReadLine();
            }
        }
    }
}