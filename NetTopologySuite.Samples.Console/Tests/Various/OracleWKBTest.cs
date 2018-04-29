using System;
using System.Diagnostics;
using System.IO;
using GeoAPI.Geometries;
using GeoAPI.IO;
using NetTopologySuite.IO;
using NetTopologySuite.Samples.SimpleTests;
using NUnit.Framework;
namespace NetTopologySuite.Samples.Tests.Various
{
    /// <summary>
    ///
    /// </summary>
    [TestFixture]
    public class OracleWKBTest : BaseSamples
    {
        private string blobFile = string.Empty;
        /// <summary>
        /// Initializes a new instance of the <see cref="OracleWKBTest"/> class.
        /// </summary>
        public OracleWKBTest() : base() { }
        /// <summary>
        ///
        /// </summary>
        [SetUp]
        public void Init()
        {
            var blobDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\NetTopologySuite.Samples.Shapefiles\blob\");
            blobFile = blobDir + @"\blob";
            if (!File.Exists(blobFile))
                throw new FileNotFoundException("blob file not found at " + blobDir);
        }
        /// <summary>
        ///
        /// </summary>
        [Test]
        public void OracleWKBBigEndianReadTest()
        {
            IGeometry result = null;
            using (Stream stream = new FileStream(blobFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var wkbreader = new WKBReader();
                result = wkbreader.Read(stream);
            }
            Debug.WriteLine(result.ToString());
            Assert.IsNotNull(result);
        }
        /// <summary>
        ///
        /// </summary>
        [Test]
        public void OracleWKBBigEndianWriteTest()
        {
            var shell = Factory.CreateLinearRing(new Coordinate[] {    new Coordinate(100,100),
                                                                                new Coordinate(200,100),
                                                                                new Coordinate(200,200),
                                                                                new Coordinate(100,200),
                                                                                new Coordinate(100,100), });
            var hole = Factory.CreateLinearRing(new Coordinate[] {     new Coordinate(120,120),
                                                                                new Coordinate(180,120),
                                                                                new Coordinate(180,180),
                                                                                new Coordinate(120,180),
                                                                                new Coordinate(120,120), });
            var polygon = Factory.CreatePolygon(shell, new ILinearRing[] { hole, });
            var writer = new WKBWriter(ByteOrder.BigEndian);
            var bytes = writer.Write(polygon);
            Assert.IsNotNull(bytes);
            Assert.IsNotEmpty(bytes);
            Debug.WriteLine(bytes.Length);
        }
    }
}
