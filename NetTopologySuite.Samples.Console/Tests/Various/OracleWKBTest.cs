using System;
using System.Diagnostics;
using System.IO;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownBinary;
using GisSharpBlog.NetTopologySuite.Samples.SimpleTests;
using NetTopologySuite.Coordinates;
using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Various
{
    [TestFixture]
    public class OracleWkbTest : BaseSamples
    {
        private String _blobFile = String.Empty;

        [SetUp]
        public void Init()
        {
            const String relativePath = @"..\..\..\NetTopologySuite.Samples.Shapefiles\blob\";
            String blobDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
            
            _blobFile = blobDir + @"\blob";
            
            if (!File.Exists(_blobFile))
            {
                throw new FileNotFoundException("blob file not found at " + blobDir);
            }
        }

        [Test]
        public void OracleWKBBigIndianReadTest()
        {
            IGeometry result;

            using (Stream stream = new FileStream(_blobFile,
                                                  FileMode.Open,
                                                  FileAccess.Read,
                                                  FileShare.Read))
            {
                WkbReader<BufferedCoordinate> wkbreader =
                    new WkbReader<BufferedCoordinate>(GeoFactory);
                result = wkbreader.Read(stream);
            }

            Debug.WriteLine(result.ToString());
            Assert.IsNotNull(result);
        }

        [Test]
        public void OracleWkbBigEndianWriteTest()
        {
            ILinearRing<BufferedCoordinate> shell = GeoFactory.CreateLinearRing(new[]
                                                                  {
                                                                      CoordFactory.Create(100, 100),
                                                                      CoordFactory.Create(200, 100),
                                                                      CoordFactory.Create(200, 200),
                                                                      CoordFactory.Create(100, 200),
                                                                      CoordFactory.Create(100, 100)
                                                                  });

            ILinearRing<BufferedCoordinate> hole = GeoFactory.CreateLinearRing(new[]
                                                                 {
                                                                     CoordFactory.Create(120, 120),
                                                                     CoordFactory.Create(180, 120),
                                                                     CoordFactory.Create(180, 180),
                                                                     CoordFactory.Create(120, 180),
                                                                     CoordFactory.Create(120, 120)
                                                                 });

            IPolygon<BufferedCoordinate> polygon = GeoFactory.CreatePolygon(shell, new[] {hole});

            WkbWriter<BufferedCoordinate> writer =
                new WkbWriter<BufferedCoordinate>(WkbByteOrder.BigEndian);
            
            Byte[] bytes = writer.Write(polygon);
            
            Assert.IsNotNull(bytes);
            Assert.IsNotEmpty(bytes);
            Debug.WriteLine(bytes.Length);
        }
    }
}