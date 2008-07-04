using System;
using GisSharpBlog.NetTopologySuite.Samples.SimpleTests.ShapeTests;
using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Samples.SimpleTests
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        [STAThread]
        public static void Main(string[] args) 
        {            
            new Program().Start();
        }

        private static BaseSamples sample = null;

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            SamplesTest();
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void SamplesTest()
        {
			try
			{
                //sample = new PointSamples();
                //sample.Start();
                //Console.WriteLine();

                //sample = new LineStringSamples();
                //sample.Start();
                //Console.WriteLine();

                //sample = new PolygonSamples();
                //sample.Start();
                //Console.WriteLine();

                //sample = new MultiPointSamples();
                //sample.Start();
                //Console.WriteLine();

                //sample = new ValidationSuite();
                //sample.Start();
                //Console.WriteLine();

                //sample = new SerializationSamples();
                //sample.Start();
                //Console.WriteLine();

                //sample = new AttributesTest();
                //sample.Start();
                //Console.WriteLine();

                //sample = new ShapeRead();
                //sample.Start();
                //Console.WriteLine();

				sample = new GMLTesting();
				sample.Start();
				Console.WriteLine();

                //new LinearReferencingExample().Run();

			}
            finally
            {
                Console.ReadLine();
            }                       
        }
    }
}
