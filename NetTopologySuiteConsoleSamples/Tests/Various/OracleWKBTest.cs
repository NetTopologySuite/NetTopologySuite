using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;

using GisSharpBlog.NetTopologySuite.Samples.SimpleTests;

using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Samples.NUnitTests
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class OracleWKBTest : BaseSamples
    {
        private string blobFile = String.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OracleWKBTest"/> class.
        /// </summary>
        public OracleWKBTest() : base() { }

        /// <summary>
        /// 
        /// </summary>
        [SetUp]
        public void Init()
        {
            string blobDir = Path.Combine(Environment.CurrentDirectory, @"..\..\..\Shapefiles\blob\");
            blobFile = blobDir + @"\blob"; 
            if (!File.Exists(blobFile))
                throw new FileNotFoundException("blob file not found at " + blobDir);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]        
        public void OracleWKBBigIndianReadTest()
        {
            Geometry result = null;
            using(Stream stream = new FileStream(blobFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                WKBReader wkbreader = new WKBReader();
                result = wkbreader.Read(stream);
            }
            Debug.WriteLine(result.ToString());
            Assert.IsNotNull(result);                           
        }

        /// <summary>
        /// 
        /// </summary>
        [Test] 
        public void OracleWKBBigIndianWriteTest()
        {
            LinearRing shell = Factory.CreateLinearRing(new Coordinate[] { new Coordinate(100,100),
                                                                new Coordinate(200,100),
                                                                new Coordinate(200,200),                
                                                                new Coordinate(100,200),
                                                                new Coordinate(100,100), });
            LinearRing hole = Factory.CreateLinearRing(new Coordinate[] {  new Coordinate(120,120),
                                                                new Coordinate(180,120),
                                                                new Coordinate(180,180),                                                                                
                                                                new Coordinate(120,180),                                                                
                                                                new Coordinate(120,120), });
            Polygon polygon = Factory.CreatePolygon(shell, new LinearRing[] { hole, });                                    
            WKBWriter writer = new WKBWriter(ByteOrder.BigIndian);
            byte[] bytes = writer.Write(polygon);
            Assert.IsNotNull(bytes);
            Assert.IsNotEmpty(bytes);
            Debug.WriteLine(bytes.Length);
        }
    }
}
