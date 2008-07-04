using System;
using System.Diagnostics;
using System.IO;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using GisSharpBlog.NetTopologySuite.Samples.SimpleTests;
using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Various
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
			string blobDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\NetTopologySuite.Samples.Shapefiles\blob\");
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
            IGeometry result = null;
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
            ILinearRing shell = Factory.CreateLinearRing(new ICoordinate[] {    new Coordinate(100,100),
                                                                                new Coordinate(200,100),
                                                                                new Coordinate(200,200),                
                                                                                new Coordinate(100,200),
                                                                                new Coordinate(100,100), });
            ILinearRing hole = Factory.CreateLinearRing(new ICoordinate[] {     new Coordinate(120,120),
                                                                                new Coordinate(180,120),
                                                                                new Coordinate(180,180),                                                                                
                                                                                new Coordinate(120,180),                                                                
                                                                                new Coordinate(120,120), });
            IPolygon polygon = Factory.CreatePolygon(shell, new ILinearRing[] { hole, });                                    
            WKBWriter writer = new WKBWriter(ByteOrder.BigEndian);
            byte[] bytes = writer.Write(polygon);
            Assert.IsNotNull(bytes);
            Assert.IsNotEmpty(bytes);
            Debug.WriteLine(bytes.Length);
        }
    }
}
