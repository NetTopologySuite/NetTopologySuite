using System;
using System.Diagnostics;
using System.IO;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownBinary;
using GisSharpBlog.NetTopologySuite.SimpleTests;
using NUnit.Framework;
#if BUFFERED
using coord = NetTopologySuite.Coordinates.BufferedCoordinate;
using coordFac = NetTopologySuite.Coordinates.BufferedCoordinateFactory;
using coordSeqFac = NetTopologySuite.Coordinates.BufferedCoordinateSequenceFactory;
#else
using coord = NetTopologySuite.Coordinates.Simple.Coordinate;
using coordFac = NetTopologySuite.Coordinates.Simple.CoordinateFactory;
using coordSeqFac = NetTopologySuite.Coordinates.Simple.CoordinateSequenceFactory;
#endif

namespace GisSharpBlog.NetTopologySuite.Tests.Various
{
    [TestFixture]
    public class OracleWkbTest : BaseSamples
    {
        #region Setup/Teardown

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

        #endregion

        private String _blobFile = String.Empty;

        [Test]
        public void OracleWkbBigEndianWriteTest()
        {
            ILinearRing<coord> shell = GeoFactory.CreateLinearRing(new[]
                                                                                    {
                                                                                        CoordFactory.Create(100, 100),
                                                                                        CoordFactory.Create(200, 100),
                                                                                        CoordFactory.Create(200, 200),
                                                                                        CoordFactory.Create(100, 200),
                                                                                        CoordFactory.Create(100, 100)
                                                                                    });

            ILinearRing<coord> hole = GeoFactory.CreateLinearRing(new[]
                                                                                   {
                                                                                       CoordFactory.Create(120, 120),
                                                                                       CoordFactory.Create(180, 120),
                                                                                       CoordFactory.Create(180, 180),
                                                                                       CoordFactory.Create(120, 180),
                                                                                       CoordFactory.Create(120, 120)
                                                                                   });

            IPolygon<coord> polygon = GeoFactory.CreatePolygon(shell, new[] { hole });

            WkbWriter<coord> writer =
                new WkbWriter<coord>(WkbByteOrder.BigEndian);

            Byte[] bytes = writer.Write(polygon);

            Assert.IsNotNull(bytes);
            Assert.IsNotEmpty(bytes);
            Debug.WriteLine(bytes.Length);
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
                WkbReader<coord> wkbreader =
                    new WkbReader<coord>(GeoFactory);
                result = wkbreader.Read(stream);
            }

            Debug.WriteLine(result.ToString());
            Assert.IsNotNull(result);
        }
    }
}