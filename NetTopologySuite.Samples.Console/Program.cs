using System;
using GisSharpBlog.NetTopologySuite.Samples.LinearReferencing;
using GisSharpBlog.NetTopologySuite.Samples.SimpleTests.Geometries;
using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Samples.SimpleTests
{
    [TestFixture]
    public class Program
    {
        [STAThread]
        public static void Main(String[] args)
        {
            new Program().Start();
        }

        private static BaseSamples sample;

        public void Start()
        {
            SamplesTest();
        }

        [Test]
        public void SamplesTest()
        {
            try
            {
                sample = new PointSamples();
                sample.Start();
                Console.WriteLine();

                sample = new LineStringSamples();
                sample.Start();
                Console.WriteLine();

                sample = new PolygonSamples();
                sample.Start();
                Console.WriteLine();

                sample = new MultiPointSamples();
                sample.Start();
                Console.WriteLine();

                sample = new ValidationSuite();
                sample.Start();
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