using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownBinary;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Samples.SimpleTests;
using NetTopologySuite.Coordinates;
using NUnit.Framework;
using GeoAPI.Coordinates;

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
        /// 
        /// </summary>
        [SetUp]
        public void Init()
        {
            String relativePath = @"..\..\..\NetTopologySuite.Samples.Shapefiles\blob\";
            string blobDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
            blobFile = blobDir + @"\blob";
            if (!File.Exists(blobFile))
            {
                throw new FileNotFoundException("blob file not found at " + blobDir);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void OracleWKBBigIndianReadTest()
        {
            IGeometry result = null;

            using (Stream stream = new FileStream(blobFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                WkbReader<BufferedCoordinate2D> wkbreader = new WkbReader<BufferedCoordinate2D>(GeoFactory);
                result = wkbreader.Read(stream);
            }

            Debug.WriteLine(result.ToString());
            Assert.IsNotNull(result);
        }

        [Test]
        public void OracleWKBBigIndianWriteTest()
        {
            ILinearRing<BufferedCoordinate2D> shell = GeoFactory.CreateLinearRing(new BufferedCoordinate2D[]
                                                             {
                                                                 CoordFactory.Create(100, 100),
                                                                 CoordFactory.Create(200, 100),
                                                                 CoordFactory.Create(200, 200),
                                                                 CoordFactory.Create(100, 200),
                                                                 CoordFactory.Create(100, 100),
                                                             });

            ILinearRing<BufferedCoordinate2D> hole = GeoFactory.CreateLinearRing(new BufferedCoordinate2D[]
                                                            {
                                                                CoordFactory.Create(120, 120),
                                                                CoordFactory.Create(180, 120),
                                                                CoordFactory.Create(180, 180),
                                                                CoordFactory.Create(120, 180),
                                                                CoordFactory.Create(120, 120),
                                                            });

            IPolygon<BufferedCoordinate2D> polygon = GeoFactory.CreatePolygon(shell, new ILinearRing<BufferedCoordinate2D>[] { hole });
            WkbWriter<BufferedCoordinate2D> writer = new WkbWriter<BufferedCoordinate2D>(WkbByteOrder.BigEndian);
            byte[] bytes = writer.Write(polygon);
            Assert.IsNotNull(bytes);
            Assert.IsNotEmpty(bytes);
            Debug.WriteLine(bytes.Length);
        }
    }
}