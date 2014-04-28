using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Handlers;
using NUnit.Framework;

namespace NetTopologySuite.IO.Tests.ShapeFile.Extended
{
    [TestFixture]
    public class ShapeReaderTests
    {
        private IO.ShapeFile.Extended.ShapeReader m_Reader;
        private TempFileWriter m_TmpFile;

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_SendNullPath_ShouldThrowException()
        {
            // Act.
            new IO.ShapeFile.Extended.ShapeReader(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Ctor_SendEmptyPath_ShouldThrowException()
        {
            // Act.
            new IO.ShapeFile.Extended.ShapeReader(string.Empty);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Ctor_SendWhitespacePath_ShouldThrowException()
        {
            // Act.
            new IO.ShapeFile.Extended.ShapeReader("   \t   ");
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void Ctor_SendNonExistantFilePath_ShouldThrowException()
        {
            // Act.
            new IO.ShapeFile.Extended.ShapeReader(@"C:\this\is\sheker\path\should\never\exist\on\ur\pc");
        }

        [Test]
        public void Ctor_SendValidParameters_ShouldReturnNotNull()
        {
            // Arrange
            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("line_ed50_geo"));

            // Act.
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);

            // Assert.
            Assert.IsNotNull(m_Reader);
        }

        [Test]
        public void FileHeader_ReadPoint_ShouldReturnCorrectValues()
        {
            // Arrange.
            Envelope expectedMBR = new Envelope(34.14526022208882, 34.28293070132935, 31.85116738930965, 31.92063218020455);

            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("point_ed50_geo"));

            // Act.
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);

            // Assert.
            Assert.IsNotNull(m_Reader);
            Assert.IsNotNull(m_Reader.ShapefileHeader);
            Assert.AreEqual(m_Reader.ShapefileHeader.ShapeType, ShapeGeometryType.Point);
            HelperMethods.AssertEnvelopesEqual(m_Reader.ShapefileHeader.Bounds, expectedMBR);
        }

        [Test]
        public void FileHeader_ReadLine_ShouldReturnCorrectValues()
        {
            // Arrange.
            Envelope expectedMBR = new Envelope(639384.5630270261, 662946.9241196744, 3505730.839052265, 3515879.236960234);

            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("line_ed50_utm36"));

            // Act.
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);

            // Assert.
            Assert.IsNotNull(m_Reader);
            Assert.IsNotNull(m_Reader.ShapefileHeader);
            Assert.AreEqual(m_Reader.ShapefileHeader.ShapeType, ShapeGeometryType.LineString);
            HelperMethods.AssertEnvelopesEqual(m_Reader.ShapefileHeader.Bounds, expectedMBR);
        }

        [Test]
        public void FileHeader_ReadPolygon_ShouldReturnCorrectValues()
        {
            // Arrange.
            Envelope expectedMBR = new Envelope(33.47383821246188, 33.75452922072821, 32.0295864794076, 32.1886342399706);

            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("polygon_wgs84_geo"));

            // Act.
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);

            // Assert.
            Assert.IsNotNull(m_Reader);
            Assert.IsNotNull(m_Reader.ShapefileHeader);
            Assert.AreEqual(m_Reader.ShapefileHeader.ShapeType, ShapeGeometryType.Polygon);
            HelperMethods.AssertEnvelopesEqual(m_Reader.ShapefileHeader.Bounds, expectedMBR);
        }

        [Test]
        public void ReadMBRs_ReadPoint_ShouldReturnCorrectValues()
        {
            // Arrange.
            MBRInfo[] infos = null;

            MBRInfo[] expectedInfos = new[]
				{
					new MBRInfo(new Envelope(new Coordinate(34.282930701329349, 31.851167389309651)),
							    100,
								0),
					new MBRInfo(new Envelope(new Coordinate(34.145260222088822, 31.864369159253059)),
							    128,
								1),
					new MBRInfo(new Envelope(new Coordinate(34.181721116813314, 31.920632180204553)),
							    156,
								2),
				};

            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("point_ed50_geo"));

            // Act.
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);
            infos = m_Reader.ReadMBRs().ToArray();

            // Assert.
            Assert.IsNotNull(infos);
            Assert.AreEqual(3, infos.Length);

            int currIndex = 0;

            foreach (MBRInfo expectedInfo in expectedInfos)
            {
                HelperMethods.AssertMBRInfoEqual(expectedInfo, infos[currIndex++]);
            }
        }

        [Test]
        public void ReadMBRs_ReadUnifiedWithNullAtStart_ShouldReturnCorrectValues()
        {
            // Arrange.
            MBRInfo[] infos = null;

            MBRInfo[] expectedInfos = new[]
				{
					new MBRInfo(new Envelope(-1.151515151515152, -0.353535353535354, -0.929292929292929, -0.419191919191919),
							    112,
								1),
					new MBRInfo(new Envelope(-0.457070707070707, 0.421717171717172, 0.070707070707071, 0.578282828282829),
							    248,
								2),
				};

            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("UnifiedChecksMaterialNullAtStart"));

            // Act.
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);
            infos = m_Reader.ReadMBRs().ToArray();

            // Assert.
            Assert.IsNotNull(infos);
            Assert.AreEqual(expectedInfos.Length, infos.Length);

            int currIndex = 0;

            foreach (MBRInfo expectedInfo in expectedInfos)
            {
                HelperMethods.AssertMBRInfoEqual(expectedInfo, infos[currIndex++]);
            }
        }

        [Test]
        public void ReadMBRs_ReadUnifiedWithNullInMiddle_ShouldReturnCorrectValues()
        {
            // Arrange.
            MBRInfo[] infos = null;

            MBRInfo[] expectedInfos = new[]
				{
					new MBRInfo(new Envelope(-1.151515151515152, -0.353535353535354, -0.929292929292929, -0.419191919191919),
							    100,
								0),
					new MBRInfo(new Envelope(-0.457070707070707, 0.421717171717172, 0.070707070707071, 0.578282828282829),
							    248,
								2),
				};

            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("UnifiedChecksMaterialNullInMiddle"));

            // Act.
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);
            infos = m_Reader.ReadMBRs().ToArray();

            // Assert.
            Assert.IsNotNull(infos);
            Assert.AreEqual(expectedInfos.Length, infos.Length);

            int currIndex = 0;

            foreach (MBRInfo expectedInfo in expectedInfos)
            {
                HelperMethods.AssertMBRInfoEqual(expectedInfo, infos[currIndex++]);
            }
        }

        [Test]
        public void ReadMBRs_ReadUnifiedWithNullAtEnd_ShouldReturnCorrectValues()
        {
            // Arrange.
            MBRInfo[] infos = null;

            MBRInfo[] expectedInfos = new[]
				{
					new MBRInfo(new Envelope(-1.151515151515152, -0.353535353535354, -0.929292929292929, -0.419191919191919),
							    100,
								0),
					new MBRInfo(new Envelope(-0.457070707070707, 0.421717171717172, 0.070707070707071, 0.578282828282829),
							    236,
								1),
				};

            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("UnifiedChecksMaterialNullAtEnd"));

            // Act.
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);
            infos = m_Reader.ReadMBRs().ToArray();

            // Assert.
            Assert.IsNotNull(infos);
            Assert.AreEqual(expectedInfos.Length, infos.Length);

            int currIndex = 0;

            foreach (MBRInfo expectedInfo in expectedInfos)
            {
                HelperMethods.AssertMBRInfoEqual(expectedInfo, infos[currIndex++]);
            }
        }

        [Test]
        public void ReadMBRs_ReadLine_ShouldReturnCorrectValues()
        {
            // Arrange.
            MBRInfo[] infos = null;

            MBRInfo[] expectedInfos = new[]
				{
					new MBRInfo(new Envelope(34.573027972716453, 34.628034609274806, 31.803273460424684, 31.895998933480186),
							    100,
								0),
					new MBRInfo(new Envelope(34.396692412092257, 34.518021336158107, 31.778756216701534, 31.864880893370035),
							    236,
								1),
				};

            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("line_wgs84_geo"));

            // Act.
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);
            infos = m_Reader.ReadMBRs().ToArray();

            // Assert.
            Assert.IsNotNull(infos);
            Assert.AreEqual(2, infos.Length);

            int currIndex = 0;

            foreach (MBRInfo expectedInfo in expectedInfos)
            {
                HelperMethods.AssertMBRInfoEqual(expectedInfo, infos[currIndex++]);
            }
        }

        [Test]
        public void ReadMBRs_ReadPolygon_ShouldReturnCorrectValues()
        {
            // Arrange.
            MBRInfo[] infos = null;

            MBRInfo[] expectedInfos = new[]
				{
					new MBRInfo(new Envelope(33.719047819505683, 33.78096814177016, 31.928805665809271, 32.025301664150398),
							    100,
								0),
					new MBRInfo(new Envelope(33.819000337359398, 33.929011051318348, 31.97406740944362, 32.072449163771559),
							    252,
								1),
				};

            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("polygon_ed50_geo"));

            // Act.
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);
            infos = m_Reader.ReadMBRs().ToArray();

            // Assert.
            Assert.IsNotNull(infos);
            Assert.AreEqual(2, infos.Length);

            int currIndex = 0;

            foreach (MBRInfo expectedInfo in expectedInfos)
            {
                HelperMethods.AssertMBRInfoEqual(expectedInfo, infos[currIndex++]);
            }
        }

        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void ReadShapeAtOffset_SendNegativeOffset_shouldThrowException()
        {
            // Arrange.
            IGeometryFactory factory = new GeometryFactory();
            m_TmpFile = new TempFileWriter("polygon_intersecting_line.shp", ShpFiles.Read("polygon intersecting line"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);

            // Act.
            m_Reader.ReadShapeAtOffset(-1, factory);
        }

        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void ReadShapeAtOffset_SendOffsetAtEndOfFile_shouldThrowException()
        {
            // Arrange.
            IGeometryFactory factory = new GeometryFactory();
            m_TmpFile = new TempFileWriter("polygon_intersecting_line.shp", ShpFiles.Read("polygon intersecting line"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);

            // Act.
            m_Reader.ReadShapeAtOffset(ShpFiles.Read("polygon intersecting line").Length, factory);
        }

        [Test]
        public void ReadShapeAtOffset_ReadPolygonWithIntersectingLine_shouldReturnInvalidGeo()
        {
            // Arrange.
            IGeometryFactory factory = new GeometryFactory();
            m_TmpFile = new TempFileWriter("polygon_intersecting_line.shp", ShpFiles.Read("polygon intersecting line"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);

            long[] shapeOffsets = { 100, 236 };

            bool[] expectedValidityResults = new bool[] { false, true };

            // Act.
            for (int i = 0; i < shapeOffsets.Length; i++)
            {
                IGeometry geo = m_Reader.ReadShapeAtOffset(shapeOffsets[i], factory);

                // Assert.
                Assert.IsNotNull(geo);
                Assert.AreEqual(geo.IsValid, expectedValidityResults[i]);
            }
        }

        [Test]
        public void ReadShapeAtOffset_ReadPoint_shouldReturnCorrectValue()
        {
            // Arrange.
            IGeometryFactory factory = new GeometryFactory();
            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("point_ed50_geo"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);

            long[] shapeOffsets = { 100, 128, 156 };

            double[,] expectedCoordinates = {{ 34.282930701329349, 31.851167389309651 },
											 { 34.145260222088822, 31.864369159253059 },
											 { 34.181721116813314, 31.920632180204553 }};

            // Act.
            for (int i = 0; i < shapeOffsets.Length; i++)
            {
                IGeometry geo = m_Reader.ReadShapeAtOffset(shapeOffsets[i], factory);

                // Assert.
                Assert.IsNotNull(geo);
                Assert.IsTrue(geo.IsValid);
                Assert.IsInstanceOf<IPoint>(geo);
                IPoint givenPoint = geo as IPoint;

                HelperMethods.AssertDoubleValuesEqual(givenPoint.X, expectedCoordinates[i, 0]);
                HelperMethods.AssertDoubleValuesEqual(givenPoint.Y, expectedCoordinates[i, 1]);
            }
        }

        [Test]
        public void ReadShapeAtOffset_ReadLines_shouldReturnCorrectValue()
        {
            // Arrange.
            IGeometryFactory factory = new GeometryFactory();
            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("line_wgs84_geo"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);

            long[] shapeOffsets = { 100, 236 };

            Coordinate[,] expectedLines = new Coordinate[,]
			{
				{
					new Coordinate(34.574599590903837, 31.884368958893564), 
					new Coordinate(34.57648553272869, 31.803273460424684),
					new Coordinate(34.628034609274806, 31.875882220681703),
					new Coordinate(34.573027972716453, 31.895998933480186),
					new Coordinate(34.582143358203268, 31.886883547993374)
				},
				{
					new Coordinate(34.448555812275849, 31.864880893370035), 
					new Coordinate(34.396692412092257, 31.778756216701534),
					new Coordinate(34.468672525074325, 31.794158074937872),
					new Coordinate(34.484703030585621, 31.844135533296601),
					new Coordinate(34.518021336158107, 31.838163384184551)
				}
			};

            // Act.
            for (int i = 0; i < shapeOffsets.Length; i++)
            {
                IGeometry geo = m_Reader.ReadShapeAtOffset(shapeOffsets[i], factory);

                // Assert.
                Assert.IsNotNull(geo);
                Assert.IsTrue(geo.IsValid);
                Assert.IsInstanceOf<ILineString>(geo);
                ILineString givenLine = geo as ILineString;

                for (int j = 0; j < givenLine.Coordinates.Length; j++)
                {
                    Coordinate currPoint = givenLine.Coordinates[j];

                    HelperMethods.AssertDoubleValuesEqual(currPoint.X, expectedLines[i, j].X);
                    HelperMethods.AssertDoubleValuesEqual(currPoint.Y, expectedLines[i, j].Y);
                }
            }
        }

        [Test]
        public void ReadShapeAtOffset_ReadPolygon_shouldReturnCorrectValue()
        {
            // Arrange.
            IGeometryFactory factory = new GeometryFactory();
            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("polygon_ed50_geo"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);

            long[] shapeOffsets = { 100, 252 };

            Coordinate[,] expectedLines = new Coordinate[,]
			{
				{
					new Coordinate(33.719047819505683, 31.989469320254013), 
					new Coordinate(33.730049025918099, 32.025301664150398),
					new Coordinate(33.771538712027194, 32.008956957757299),
					new Coordinate(33.78096814177016, 31.993555297099103),
					new Coordinate(33.744507207486457, 31.928805665809271),
					new Coordinate(33.719047819505683, 31.989469320254013)
				},
				{
					new Coordinate(33.821829475819285, 32.051075573685317), 
					new Coordinate(33.860176141775888, 32.072449163771559),
					new Coordinate(33.927125440097875, 32.054847113210094),
					new Coordinate(33.929011051318348, 31.97878189417845),
					new Coordinate(33.819000337359398, 31.97406740944362),
					new Coordinate(33.821829475819285, 32.051075573685317)
				}
			};

            // Act.
            for (int i = 0; i < shapeOffsets.Length; i++)
            {
                IGeometry geo = m_Reader.ReadShapeAtOffset(shapeOffsets[i], factory);

                // Assert.
                Assert.IsNotNull(geo);
                Assert.IsTrue(geo.IsValid);
                Assert.IsInstanceOf<IPolygon>(geo);
                IPolygon givenPoly = geo as IPolygon;

                Assert.IsNotNull(givenPoly.ExteriorRing);
                Assert.AreSame(givenPoly.ExteriorRing, givenPoly.Shell);
                Assert.AreEqual(givenPoly.Shell.Coordinates.Length, expectedLines.GetLength(1));

                ILineString givenLine = givenPoly.Shell;

                for (int j = 0; j < givenLine.Coordinates.Length; j++)
                {
                    Coordinate currPoint = givenLine.Coordinates[j];

                    HelperMethods.AssertDoubleValuesEqual(currPoint.X, expectedLines[i, j].X);
                    HelperMethods.AssertDoubleValuesEqual(currPoint.Y, expectedLines[i, j].Y);
                }
            }
        }

        [Test]
        public void ReadShapeAtOffset_ReadAllPolygonsFromUnifiedWithNullAtStart_ShouldReturnCorrectValues()
        {
            // Arrange.
            IGeometryFactory factory = new GeometryFactory();
            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("UnifiedChecksMaterialNullAtStart"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);

            Coordinate[][] expectedResult = new Coordinate[][]
			{
                new Coordinate[]
				{
					new Coordinate(-0.815656565656566, -0.439393939393939),
					new Coordinate(-0.353535353535354, -0.795454545454545),
					new Coordinate(-0.888888888888889,-0.929292929292929),
					new Coordinate(-1.151515151515152, -0.419191919191919),
					new Coordinate(-0.815656565656566,-0.439393939393939),
				},
                new Coordinate[]
				{
					new Coordinate(0.068181818181818,0.578282828282829),
					new Coordinate(0.421717171717172,0.070707070707071),
					new Coordinate(-0.457070707070707,0.080808080808081),
					new Coordinate(0.068181818181818,0.578282828282829),
				}
			};
            long[] offsets = { 112, 248 };

            // Act.
            for (int i = 0; i < offsets.Length; i++)
            {
                IGeometry geo = m_Reader.ReadShapeAtOffset(offsets[i], factory);

                // Assert.
                Assert.IsNotNull(geo);
                Assert.IsTrue(geo.IsValid);
                Assert.IsInstanceOf<IPolygon>(geo);
                IPolygon givenPoly = geo as IPolygon;

                Assert.IsNotNull(givenPoly.ExteriorRing);
                Assert.AreSame(givenPoly.ExteriorRing, givenPoly.Shell);
                Assert.AreEqual(givenPoly.Shell.Coordinates.Length, expectedResult[i].Length);

                ILineString givenLine = givenPoly.Shell;

                for (int j = 0; j < givenLine.Coordinates.Length; j++)
                {
                    Coordinate currPoint = givenLine.Coordinates[j];

                    HelperMethods.AssertDoubleValuesEqual(currPoint.X, expectedResult[i][j].X);
                    HelperMethods.AssertDoubleValuesEqual(currPoint.Y, expectedResult[i][j].Y);
                }
            }
        }

        [Test]
        public void ReadShapeAtOffset_ReadAllPolygonsFromUnifiedWithNullInMiddle_ShouldReturnCorrectValues()
        {
            // Arrange.
            IGeometryFactory factory = new GeometryFactory();
            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("UnifiedChecksMaterialNullInMiddle"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);

            Coordinate[][] expectedResult = new Coordinate[][]
			{
                new Coordinate[]
				{
					new Coordinate(-0.815656565656566, -0.439393939393939),
					new Coordinate(-0.353535353535354, -0.795454545454545),
					new Coordinate(-0.888888888888889,-0.929292929292929),
					new Coordinate(-1.151515151515152, -0.419191919191919),
					new Coordinate(-0.815656565656566,-0.439393939393939),
				},
                new Coordinate[]
				{
					new Coordinate(0.068181818181818,0.578282828282829),
					new Coordinate(0.421717171717172,0.070707070707071),
					new Coordinate(-0.457070707070707,0.080808080808081),
					new Coordinate(0.068181818181818,0.578282828282829),
				}
			};
            long[] offsets = { 100, 248 };

            // Act.
            for (int i = 0; i < offsets.Length; i++)
            {
                IGeometry geo = m_Reader.ReadShapeAtOffset(offsets[i], factory);

                // Assert.
                Assert.IsNotNull(geo);
                Assert.IsTrue(geo.IsValid);
                Assert.IsInstanceOf<IPolygon>(geo);
                IPolygon givenPoly = geo as IPolygon;

                Assert.IsNotNull(givenPoly.ExteriorRing);
                Assert.AreSame(givenPoly.ExteriorRing, givenPoly.Shell);
                Assert.AreEqual(givenPoly.Shell.Coordinates.Length, expectedResult[i].Length);

                ILineString givenLine = givenPoly.Shell;

                for (int j = 0; j < givenLine.Coordinates.Length; j++)
                {
                    Coordinate currPoint = givenLine.Coordinates[j];

                    HelperMethods.AssertDoubleValuesEqual(currPoint.X, expectedResult[i][j].X);
                    HelperMethods.AssertDoubleValuesEqual(currPoint.Y, expectedResult[i][j].Y);
                }
            }
        }

        [Test]
        public void ReadShapeAtOffset_ReadAllPolygonsFromUnifiedWithNullAtEnd_ShouldReturnCorrectValues()
        {
            // Arrange.
            IGeometryFactory factory = new GeometryFactory();
            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("UnifiedChecksMaterialNullAtEnd"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);

            Coordinate[][] expectedResult = new Coordinate[][]
			{
                new Coordinate[]
				{
					new Coordinate(-0.815656565656566, -0.439393939393939),
					new Coordinate(-0.353535353535354, -0.795454545454545),
					new Coordinate(-0.888888888888889,-0.929292929292929),
					new Coordinate(-1.151515151515152, -0.419191919191919),
					new Coordinate(-0.815656565656566,-0.439393939393939),
				},
                new Coordinate[]
				{
					new Coordinate(0.068181818181818,0.578282828282829),
					new Coordinate(0.421717171717172,0.070707070707071),
					new Coordinate(-0.457070707070707,0.080808080808081),
					new Coordinate(0.068181818181818,0.578282828282829),
				}
			};
            long[] offsets = { 100, 236 };

            // Act.
            for (int i = 0; i < offsets.Length; i++)
            {
                IGeometry geo = m_Reader.ReadShapeAtOffset(offsets[i], factory);

                // Assert.
                Assert.IsNotNull(geo);
                Assert.IsTrue(geo.IsValid);
                Assert.IsInstanceOf<IPolygon>(geo);
                IPolygon givenPoly = geo as IPolygon;

                Assert.IsNotNull(givenPoly.ExteriorRing);
                Assert.AreSame(givenPoly.ExteriorRing, givenPoly.Shell);
                Assert.AreEqual(givenPoly.Shell.Coordinates.Length, expectedResult[i].Length);

                ILineString givenLine = givenPoly.Shell;

                for (int j = 0; j < givenLine.Coordinates.Length; j++)
                {
                    Coordinate currPoint = givenLine.Coordinates[j];

                    HelperMethods.AssertDoubleValuesEqual(currPoint.X, expectedResult[i][j].X);
                    HelperMethods.AssertDoubleValuesEqual(currPoint.Y, expectedResult[i][j].Y);
                }
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReadShapeAtOffset_TryReadAfterDisposed_shouldThrowException()
        {
            // Arrange.
            IGeometryFactory factory = new GeometryFactory();
            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("line_wgs84_geo"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);

            m_Reader.Dispose();
            m_Reader.ReadShapeAtOffset(108, factory);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReadAllShapes_SendNullFactory_ShouldThrowException()
        {
            // Arrange.
            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("UnifiedChecksMaterial"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);

            // Act.
            m_Reader.ReadAllShapes(null);
        }

        [Test]
        public void ReadAllShapes_ReadEmptyShapeFile_ShouldReturnEmptyEnumerable()
        {
            // Arrange.
            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("EmptyShapeFile"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);
            IGeometryFactory factory = new GeometryFactory();

            // Act.
            IEnumerable<IGeometry> geos = m_Reader.ReadAllShapes(factory);

            // Assert.
            Assert.IsNotNull(geos);
            Assert.IsFalse(geos.Any());
        }

        [Test]
        public void ReadAllShapes_ReadPointZM_ShouldReturnCorrectValues()
        {
            // Arrange.
            IGeometryFactory factory = new GeometryFactory();
            m_TmpFile = new TempFileWriter("shape_PointZM.shp", ShpFiles.Read("shape_PointZM"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);
            double errorMargin = Math.Pow(10, -6);

            double[,] expectedValues = {{-11348202.6085706, 4503476.68482375},
									    {-601708.888562033, 3537065.37906758},
										{-7366588.02885523, -637831.461799072}};

            // Act.
            IEnumerable<IGeometry> shapes = m_Reader.ReadAllShapes(factory);

            // Assert.
            Assert.IsNotNull(shapes);
            IGeometry[] shapesArr = shapes.ToArray();
            Assert.AreEqual(shapesArr.Length, 3);

            for (int i = 0; i < shapesArr.Length; i++)
            {
                Assert.IsInstanceOf<IPoint>(shapesArr[i]);
                IPoint currPoint = shapesArr[i] as IPoint;
                HelperMethods.AssertDoubleValuesEqual(currPoint.X, expectedValues[i, 0], errorMargin);
                HelperMethods.AssertDoubleValuesEqual(currPoint.Y, expectedValues[i, 1], errorMargin);
                HelperMethods.AssertDoubleValuesEqual(currPoint.Z, 0);
                HelperMethods.AssertDoubleValuesEqual(currPoint.M, Double.NaN);
            }
        }

        [Test]
        public void ReadAllShapes_ReadPointZMWithMissingMValues_ShouldReturnCorrectValues()
        {
            // Arrange.
            IGeometryFactory factory = new GeometryFactory();
            m_TmpFile = new TempFileWriter("shape_PointZMWithMissingMValue.shp", ShpFiles.Read("shape_pointZM_MissingM values"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);
            double errorMargin = Math.Pow(10, -6);

            double[,] expectedValues = {{-11348202.6085706, 4503476.68482375},
									    {-601708.888562033, 3537065.37906758},
										{-7366588.02885523, -637831.461799072}};

            // Act.
            IEnumerable<IGeometry> shapes = m_Reader.ReadAllShapes(factory);

            // Assert.
            Assert.IsNotNull(shapes);
            IGeometry[] shapesArr = shapes.ToArray();
            Assert.AreEqual(shapesArr.Length, 3);

            for (int i = 0; i < shapesArr.Length; i++)
            {
                Assert.IsInstanceOf<IPoint>(shapesArr[i]);
                IPoint currPoint = shapesArr[i] as IPoint;
                HelperMethods.AssertDoubleValuesEqual(currPoint.X, expectedValues[i, 0], errorMargin);
                HelperMethods.AssertDoubleValuesEqual(currPoint.Y, expectedValues[i, 1], errorMargin);
                HelperMethods.AssertDoubleValuesEqual(currPoint.Z, 0);
                HelperMethods.AssertDoubleValuesEqual(currPoint.M, Double.NaN);
            }
        }

        [Test]
        public void ReadAllShapes_ReadPointM_ShouldReturnCorrectValues()
        {
            // Arrange.
            IGeometryFactory factory = new GeometryFactory();
            m_TmpFile = new TempFileWriter("shape_pointM.shp", ShpFiles.Read("shape_pointM"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);

            double[,] expectedValues = {{-133.606621226874, 66.8997078870497},
									    {-68.0564751703992, 56.4888023369036},
										{-143.246348588121, 40.6796494644596},
										{-82.3232716650438, -21.014605647517}};

            // Act.
            IEnumerable<IGeometry> shapes = m_Reader.ReadAllShapes(factory);

            // Assert.
            Assert.IsNotNull(shapes);
            IGeometry[] shapesArr = shapes.ToArray();
            Assert.AreEqual(shapesArr.Length, 4);

            for (int i = 0; i < shapesArr.Length; i++)
            {
                Assert.IsInstanceOf<IPoint>(shapesArr[i]);
                IPoint currPoint = shapesArr[i] as IPoint;
                HelperMethods.AssertDoubleValuesEqual(currPoint.X, expectedValues[i, 0]);
                HelperMethods.AssertDoubleValuesEqual(currPoint.Y, expectedValues[i, 1]);
                HelperMethods.AssertDoubleValuesEqual(currPoint.Z, Double.NaN);
                HelperMethods.AssertDoubleValuesEqual(currPoint.M, Double.NaN);
            }
        }

        [Test]
        public void ReadAllShapes_ReadUnifiedChecksMaterial_ShouldRead2ShapesAndCorrectValues()
        {
            // Arrange.
            m_TmpFile = new TempFileWriter("UnifiedChecksMaterial.shp", ShpFiles.Read("UnifiedChecksMaterial"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);
            IGeometryFactory factory = new GeometryFactory();

            IPolygon[] expectedResult = new Polygon[]
			{
				new Polygon(new LinearRing(new Coordinate[]
					{
						new Coordinate(-0.815656565656566, -0.439393939393939),
						new Coordinate(-0.353535353535354, -0.795454545454545),
						new Coordinate(-0.888888888888889,-0.929292929292929),
						new Coordinate(-1.151515151515152, -0.419191919191919),
						new Coordinate(-0.815656565656566,-0.439393939393939),
					})),
				new Polygon(new LinearRing(new Coordinate[]
					{
						new Coordinate(0.068181818181818,0.578282828282829),
						new Coordinate(0.421717171717172,0.070707070707071),
						new Coordinate(-0.457070707070707,0.080808080808081),
						new Coordinate(0.068181818181818,0.578282828282829),
					}))
			};

            // Act.
            IGeometry[] shapes = m_Reader.ReadAllShapes(factory).ToArray();

            Assert.IsNotNull(shapes);
            Assert.AreEqual(shapes.Length, 2);

            for (int i = 0; i < shapes.Length; i++)
            {
                Assert.IsInstanceOf<IPolygon>(shapes[i]);
                HelperMethods.AssertPolygonsEqual(shapes[i] as IPolygon, expectedResult[i]);
            }
        }

        [Test]
        public void ReadAllShapes_ReadAllPolygonsFromUnifiedWithNullAtStart_ShouldReturnCorrectValues()
        {
            // Arrange.
            m_TmpFile = new TempFileWriter("UnifiedChecksMaterial.shp", ShpFiles.Read("UnifiedChecksMaterialNullAtStart"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);
            IGeometryFactory factory = new GeometryFactory();

            IPolygon[] expectedResult = new Polygon[]
			{
				new Polygon(new LinearRing(new Coordinate[]
					{
						new Coordinate(-0.815656565656566, -0.439393939393939),
						new Coordinate(-0.353535353535354, -0.795454545454545),
						new Coordinate(-0.888888888888889,-0.929292929292929),
						new Coordinate(-1.151515151515152, -0.419191919191919),
						new Coordinate(-0.815656565656566,-0.439393939393939),
					})),
				new Polygon(new LinearRing(new Coordinate[]
					{
						new Coordinate(0.068181818181818,0.578282828282829),
						new Coordinate(0.421717171717172,0.070707070707071),
						new Coordinate(-0.457070707070707,0.080808080808081),
						new Coordinate(0.068181818181818,0.578282828282829),
					}))
			};

            // Act.
            IGeometry[] shapes = m_Reader.ReadAllShapes(factory).ToArray();

            Assert.IsNotNull(shapes);
            Assert.AreEqual(shapes.Length, 2);

            for (int i = 0; i < shapes.Length; i++)
            {
                Assert.IsInstanceOf<IPolygon>(shapes[i]);
                HelperMethods.AssertPolygonsEqual(shapes[i] as IPolygon, expectedResult[i]);
            }
        }

        [Test]
        public void ReadAllShapes_ReadAllPolygonsFromUnifiedWithNullInMiddle_ShouldReturnCorrectValues()
        {
            // Arrange.
            m_TmpFile = new TempFileWriter("UnifiedChecksMaterial.shp", ShpFiles.Read("UnifiedChecksMaterialNullInMiddle"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);
            IGeometryFactory factory = new GeometryFactory();

            IPolygon[] expectedResult = new Polygon[]
			{
				new Polygon(new LinearRing(new Coordinate[]
					{
						new Coordinate(-0.815656565656566, -0.439393939393939),
						new Coordinate(-0.353535353535354, -0.795454545454545),
						new Coordinate(-0.888888888888889,-0.929292929292929),
						new Coordinate(-1.151515151515152, -0.419191919191919),
						new Coordinate(-0.815656565656566,-0.439393939393939),
					})),
				new Polygon(new LinearRing(new Coordinate[]
					{
						new Coordinate(0.068181818181818,0.578282828282829),
						new Coordinate(0.421717171717172,0.070707070707071),
						new Coordinate(-0.457070707070707,0.080808080808081),
						new Coordinate(0.068181818181818,0.578282828282829),
					}))
			};

            // Act.
            IGeometry[] shapes = m_Reader.ReadAllShapes(factory).ToArray();

            Assert.IsNotNull(shapes);
            Assert.AreEqual(shapes.Length, 2);

            for (int i = 0; i < shapes.Length; i++)
            {
                Assert.IsInstanceOf<IPolygon>(shapes[i]);
                HelperMethods.AssertPolygonsEqual(shapes[i] as IPolygon, expectedResult[i]);
            }
        }

        [Test]
        public void ReadAllShapes_ReadAllPolygonsFromUnifiedWithNullAtEnd_ShouldReturnCorrectValues()
        {
            // Arrange.
            m_TmpFile = new TempFileWriter("UnifiedChecksMaterial.shp", ShpFiles.Read("UnifiedChecksMaterialNullAtEnd"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);
            IGeometryFactory factory = new GeometryFactory();

            IPolygon[] expectedResult = new Polygon[]
			{
				new Polygon(new LinearRing(new Coordinate[]
					{
						new Coordinate(-0.815656565656566, -0.439393939393939),
						new Coordinate(-0.353535353535354, -0.795454545454545),
						new Coordinate(-0.888888888888889,-0.929292929292929),
						new Coordinate(-1.151515151515152, -0.419191919191919),
						new Coordinate(-0.815656565656566,-0.439393939393939),
					})),
				new Polygon(new LinearRing(new Coordinate[]
					{
						new Coordinate(0.068181818181818,0.578282828282829),
						new Coordinate(0.421717171717172,0.070707070707071),
						new Coordinate(-0.457070707070707,0.080808080808081),
						new Coordinate(0.068181818181818,0.578282828282829),
					}))
			};

            // Act.
            IGeometry[] shapes = m_Reader.ReadAllShapes(factory).ToArray();

            Assert.IsNotNull(shapes);
            Assert.AreEqual(shapes.Length, 2);

            for (int i = 0; i < shapes.Length; i++)
            {
                Assert.IsInstanceOf<IPolygon>(shapes[i]);
                HelperMethods.AssertPolygonsEqual(shapes[i] as IPolygon, expectedResult[i]);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReadAllShapes_TryReadAfterDisposed_ShouldThrowException()
        {
            // Arrange.
            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("UnifiedChecksMaterial"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);
            IGeometryFactory factory = new GeometryFactory();

            // Act.
            m_Reader.Dispose();
            m_Reader.ReadAllShapes(factory);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReadShapeAtIndex_SendNullFactory_ShouldThrowException()
        {
            // Arrange.
            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("UnifiedChecksMaterial"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);

            // Act.
            m_Reader.ReadShapeAtIndex(0, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ReadShapeAtIndex_SendNegativeIndex_ShouldThrowException()
        {
            // Arrange.
            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("UnifiedChecksMaterial"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);
            IGeometryFactory factory = new GeometryFactory();

            // Act.
            m_Reader.ReadShapeAtIndex(-1, factory);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ReadShapeAtIndex_SendOutOfBoundIndex_ShouldThrowException()
        {
            // Arrange.
            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("UnifiedChecksMaterial"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);
            IGeometryFactory factory = new GeometryFactory();

            // Act.
            m_Reader.ReadShapeAtIndex(2, factory);
        }

        [Test]
        public void ReadShapeAtIndex_ReadFirstUnifiedCheckMaterialShape_ShouldReturnRectangle()
        {
            // Arrange.
            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("UnifiedChecksMaterial"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);
            IGeometryFactory factory = new GeometryFactory();

            Polygon expectedPolygon = new Polygon(new LinearRing(new Coordinate[]
					{
						new Coordinate(-0.815656565656566, -0.439393939393939),
						new Coordinate(-0.353535353535354, -0.795454545454545),
						new Coordinate(-0.888888888888889,-0.929292929292929),
						new Coordinate(-1.151515151515152, -0.419191919191919),
						new Coordinate(-0.815656565656566,-0.439393939393939),
					}));

            // Act.
            IGeometry polygon = m_Reader.ReadShapeAtIndex(0, factory);

            Assert.IsNotNull(polygon);
            Assert.IsInstanceOf<IPolygon>(polygon);
            HelperMethods.AssertPolygonsEqual(polygon as IPolygon, expectedPolygon);
        }

        [Test]
        public void ReadShapeAtIndex_ReadSecondUnifiedCheckMaterialShape_ShouldReturnTriangle()
        {
            // Arrange.
            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("UnifiedChecksMaterial"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);
            IGeometryFactory factory = new GeometryFactory();

            Polygon expectedPolygon = new Polygon(new LinearRing(new Coordinate[]
					{
						new Coordinate(0.068181818181818,0.578282828282829),
						new Coordinate(0.421717171717172,0.070707070707071),
						new Coordinate(-0.457070707070707,0.080808080808081),
						new Coordinate(0.068181818181818,0.578282828282829),
					}));

            // Act.
            IGeometry polygon = m_Reader.ReadShapeAtIndex(1, factory);

            Assert.IsNotNull(polygon);
            Assert.IsInstanceOf<IPolygon>(polygon);
            HelperMethods.AssertPolygonsEqual(polygon as IPolygon, expectedPolygon);
        }

        [Test]
        public void ReadShapeAtIndex_ReadUnifiedCheckMaterialWithNullAtStart_ShouldReturnBothShapesCorrectly()
        {
            // Arrange.
            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("UnifiedChecksMaterialNullAtStart"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);
            IGeometryFactory factory = new GeometryFactory();

            IPolygon[] expectedResult = new Polygon[]
			{
				new Polygon(new LinearRing(new Coordinate[]
					{
						new Coordinate(-0.815656565656566, -0.439393939393939),
						new Coordinate(-0.353535353535354, -0.795454545454545),
						new Coordinate(-0.888888888888889,-0.929292929292929),
						new Coordinate(-1.151515151515152, -0.419191919191919),
						new Coordinate(-0.815656565656566,-0.439393939393939),
					})),
				new Polygon(new LinearRing(new Coordinate[]
					{
						new Coordinate(0.068181818181818,0.578282828282829),
						new Coordinate(0.421717171717172,0.070707070707071),
						new Coordinate(-0.457070707070707,0.080808080808081),
						new Coordinate(0.068181818181818,0.578282828282829),
					}))
			};

            // Act.
            for (int i = 0; i < expectedResult.Length; i++)
            {
                IGeometry result = m_Reader.ReadShapeAtIndex(i, factory);

                Assert.IsNotNull(result);
                Assert.IsInstanceOf<IPolygon>(result);

                HelperMethods.AssertPolygonsEqual(expectedResult[i], result as IPolygon);
            }
        }

        [Test]
        public void ReadShapeAtIndex_ReadUnifiedCheckMaterialWithNullAtEnd_ShouldReturnBothShapesCorrectly()
        {
            // Arrange.
            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("UnifiedChecksMaterialNullAtEnd"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);
            IGeometryFactory factory = new GeometryFactory();

            IPolygon[] expectedResult = new Polygon[]
			{
				new Polygon(new LinearRing(new Coordinate[]
					{
						new Coordinate(-0.815656565656566, -0.439393939393939),
						new Coordinate(-0.353535353535354, -0.795454545454545),
						new Coordinate(-0.888888888888889,-0.929292929292929),
						new Coordinate(-1.151515151515152, -0.419191919191919),
						new Coordinate(-0.815656565656566,-0.439393939393939),
					})),
				new Polygon(new LinearRing(new Coordinate[]
					{
						new Coordinate(0.068181818181818,0.578282828282829),
						new Coordinate(0.421717171717172,0.070707070707071),
						new Coordinate(-0.457070707070707,0.080808080808081),
						new Coordinate(0.068181818181818,0.578282828282829),
					}))
			};

            // Act.
            for (int i = 0; i < expectedResult.Length; i++)
            {
                IGeometry result = m_Reader.ReadShapeAtIndex(i, factory);

                Assert.IsNotNull(result);
                Assert.IsInstanceOf<IPolygon>(result);

                HelperMethods.AssertPolygonsEqual(expectedResult[i], result as IPolygon);
            }
        }

        [Test]
        public void ReadShapeAtIndex_ReadUnifiedCheckMaterialWithNulLInMiddle_ShouldReturnBothShapesCorrectly()
        {
            // Arrange.
            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("UnifiedChecksMaterialNullInMiddle"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);
            IGeometryFactory factory = new GeometryFactory();

            IPolygon[] expectedResult = new Polygon[]
			{
				new Polygon(new LinearRing(new Coordinate[]
					{
						new Coordinate(-0.815656565656566, -0.439393939393939),
						new Coordinate(-0.353535353535354, -0.795454545454545),
						new Coordinate(-0.888888888888889,-0.929292929292929),
						new Coordinate(-1.151515151515152, -0.419191919191919),
						new Coordinate(-0.815656565656566,-0.439393939393939),
					})),
				new Polygon(new LinearRing(new Coordinate[]
					{
						new Coordinate(0.068181818181818,0.578282828282829),
						new Coordinate(0.421717171717172,0.070707070707071),
						new Coordinate(-0.457070707070707,0.080808080808081),
						new Coordinate(0.068181818181818,0.578282828282829),
					}))
			};

            // Act.
            for (int i = 0; i < expectedResult.Length; i++)
            {
                IGeometry result = m_Reader.ReadShapeAtIndex(i, factory);

                Assert.IsNotNull(result);
                Assert.IsInstanceOf<IPolygon>(result);

                HelperMethods.AssertPolygonsEqual(expectedResult[i], result as IPolygon);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReadShapeAtIndex_TryReadAfterDisposed_ShouldThrowException()
        {
            // Arrange.
            m_TmpFile = new TempFileWriter("shape.shp", ShpFiles.Read("UnifiedChecksMaterial"));
            m_Reader = new IO.ShapeFile.Extended.ShapeReader(m_TmpFile.Path);
            IGeometryFactory factory = new GeometryFactory();

            // Act.
            m_Reader.Dispose();
            m_Reader.ReadShapeAtIndex(0, factory);
        }

        [TearDown]
        public void TestCleanup()
        {
            if (m_Reader != null)
            {
                m_Reader.Dispose();
                m_Reader = null;
            }

            if (m_TmpFile != null)
            {
                m_TmpFile.Dispose();
                m_TmpFile = null;
            }
        }
    }

    static class ShpFiles
    {
        public static byte[] Read(string filename)
        {
            string basedir = AppDomain.CurrentDomain.BaseDirectory;
            string format = String.Format("..{0}..{0}..{0}NetTopologySuite.Samples.Shapefiles", Path.DirectorySeparatorChar);
            String folder = Path.Combine(basedir, format);
            String file = Path.ChangeExtension(filename, "shp");
            String path = Path.Combine(folder, file);
            Assert.That(File.Exists(path), Is.True, "file not found: " + filename);
            return File.ReadAllBytes(path);
        }
    }
}
