using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.IO.Handlers;
using NetTopologySuite.IO.ShapeFile.Extended;
using NetTopologySuite.IO.ShapeFile.Extended.Entities;
using NUnit.Framework;

namespace NetTopologySuite.IO.Tests.ShapeFile.Extended
{
    [TestFixture]
    public class ShapeDataReaderTests
    {
        private TempFileWriter[] m_TempFiles;
        private ShapeDataReader m_shapeDataReader;

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_SendNullPath_ShouldThrowException()
        {
            // Act.
            m_shapeDataReader = new ShapeDataReader(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_SendEmptyPath_ShouldThrowException()
        {
            // Act.
            m_shapeDataReader = new ShapeDataReader(string.Empty);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_SendWhitespacePath_ShouldThrowException()
        {
            // Act.
            m_shapeDataReader = new ShapeDataReader("   \t   ");
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void Ctor_SendNonExistantFilePath_ShouldThrowException()
        {
            // Act.
            m_shapeDataReader = new ShapeDataReader(@"C:\this\is\sheker\path\should\never\exist\on\ur\pc");
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void Ctor_SendShpWithNoDbf_ShouldThrowException()
        {
            // Arrange.
            m_TempFiles = new TempFileWriter[]
			{
				new TempFileWriter("test.shp", ShpFiles.Read("UnifiedChecksMaterial")),
			};

            // Act.
            m_shapeDataReader = new ShapeDataReader(m_TempFiles[0].Path);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_SendNullSpatialIndex_ShouldThrowException()
        {
            // Arrange.
            m_TempFiles = new TempFileWriter[]
			{
				new TempFileWriter("test.shp", ShpFiles.Read("UnifiedChecksMaterial")),
				new TempFileWriter("test.dbf", DbfFiles.Read("UnifiedChecksMaterial")),
			};

            // Act.
            m_shapeDataReader = new ShapeDataReader(m_TempFiles[0].Path, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_SendNullGeometryFactory_ShouldThrowException()
        {
            // Arrange.
            m_TempFiles = new TempFileWriter[]
			{
				new TempFileWriter("test.shp", ShpFiles.Read("UnifiedChecksMaterial")),
				new TempFileWriter("test.dbf", DbfFiles.Read("UnifiedChecksMaterial")),
			};

            // Act.
            m_shapeDataReader = new ShapeDataReader(m_TempFiles[0].Path, new STRtree<ShapeLocationInFileInfo>(), null);
        }

        [Test]
        public void Ctor_SendShpWithNoPrj_ShouldReturnNotNull()
        {
            // Arrange.
            m_TempFiles = new TempFileWriter[]
			{
				new TempFileWriter("test.shp", ShpFiles.Read("UnifiedChecksMaterial")),
				new TempFileWriter("test.dbf", DbfFiles.Read("UnifiedChecksMaterial")),
			};

            // Act.
            m_shapeDataReader = new ShapeDataReader(m_TempFiles[0].Path);
            Assert.IsNotNull(m_shapeDataReader);
        }

        [Test]
        public void Ctor_SetAsyncIndexToTrue_ShouldReturnNotNull()
        {
            // Arrange.
            m_TempFiles = new TempFileWriter[]
			{
				new TempFileWriter("test.shp", ShpFiles.Read("UnifiedChecksMaterial")),
				new TempFileWriter("test.dbf", DbfFiles.Read("UnifiedChecksMaterial"))
			};

            // Act.
            m_shapeDataReader = new ShapeDataReader(m_TempFiles[0].Path, new STRtree<ShapeLocationInFileInfo>(), new GeometryFactory(), true);

            // Assert.
            Assert.IsNotNull(m_shapeDataReader);
        }

        [Test]
        public void Ctor_SetAsyncIndexToFalse_ShouldReturnNotNull()
        {
            // Arrange.
            m_TempFiles = new TempFileWriter[]
			{
				new TempFileWriter("test.shp", ShpFiles.Read("UnifiedChecksMaterial")),
				new TempFileWriter("test.dbf", DbfFiles.Read("UnifiedChecksMaterial")),
			};

            // Act.
            m_shapeDataReader = new ShapeDataReader(m_TempFiles[0].Path, new STRtree<ShapeLocationInFileInfo>(), new GeometryFactory(), false);

            // Assert.
            Assert.IsNotNull(m_shapeDataReader);
        }

        [Test]
        public void ShapeFileBounds_ReadPointED50Geo_ShouldReturnCorrectEnvelope()
        {
            // Arrange.
            Envelope expectedMBR = new Envelope(34.14526022208882, 34.28293070132935, 31.85116738930965, 31.92063218020455);

            m_TempFiles = new TempFileWriter[]
			{
				new TempFileWriter("test.shp", ShpFiles.Read("point_ed50_geo")),
				new TempFileWriter("test.dbf", DbfFiles.Read("point_ed50_geo")),
			};

            // Act.
            m_shapeDataReader = new ShapeDataReader(m_TempFiles[0].Path);

            // Assert.
            HelperMethods.AssertEnvelopesEqual(expectedMBR, m_shapeDataReader.ShapefileBounds);
        }

        [Test]
        public void ReadByGeoFilter_ReadAllInBounds_ShouldReturnAllShapesAndCorrectDbfData()
        {
            // Arrange.
            m_TempFiles = new TempFileWriter[]
			{
				new TempFileWriter("test.shp", ShpFiles.Read("UnifiedChecksMaterial")),
				new TempFileWriter("test.dbf", DbfFiles.Read("UnifiedChecksMaterial")),
			};

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

            string[] expectedShapeMetadata = new string[] { "Rectangle", "Triangle" };

            m_shapeDataReader = new ShapeDataReader(m_TempFiles[0].Path);

            // Act.
            IEnumerable<IShapefileFeature> results = m_shapeDataReader.ReadByMBRFilter(m_shapeDataReader.ShapefileBounds);

            // Assert.
            Assert.IsNotNull(results);

            int currIndex = 0;
            foreach (IShapefileFeature result in results)
            {
                Assert.IsNotNull(result);
                Assert.IsInstanceOf<ShapefileFeature>(result);
                ShapefileFeature sf = (ShapefileFeature)result;
                Assert.AreEqual(sf.FeatureId, currIndex);
                Assert.IsNotNull(result.Attributes);

                HelperMethods.AssertPolygonsEqual(result.Geometry as IPolygon, expectedResult[currIndex]);

                object shapeNameData = result.Attributes["ShapeName"];
                Assert.IsInstanceOf<string>(shapeNameData);

                Assert.AreEqual((string)shapeNameData, expectedShapeMetadata[currIndex++]);
            }
        }

        [Test]
        public void ReadByGeoFilter_ReadWithWholeTriangleInBounds_ShouldReturnTriangle()
        {
            Envelope boundsWithWholeTriangle = new Envelope(-0.62331, 0.63774, -0.02304, 0.76942);

            // Arrange.
            m_TempFiles = new TempFileWriter[]
			{
				new TempFileWriter("test.shp", ShpFiles.Read("UnifiedChecksMaterial")),
				new TempFileWriter("test.dbf", DbfFiles.Read("UnifiedChecksMaterial")),
			};

            Polygon expectedTriangle = new Polygon(new LinearRing(new Coordinate[]
					{
						new Coordinate(0.068181818181818,0.578282828282829),
						new Coordinate(0.421717171717172,0.070707070707071),
						new Coordinate(-0.457070707070707,0.080808080808081),
						new Coordinate(0.068181818181818,0.578282828282829),
					}));

            string expectedShapeMetadata = "Triangle";

            m_shapeDataReader = new ShapeDataReader(m_TempFiles[0].Path);

            // Act.
            IEnumerable<IShapefileFeature> results = m_shapeDataReader.ReadByMBRFilter(boundsWithWholeTriangle);

            // Assert.
            Assert.IsNotNull(results);

            IShapefileFeature result = results.Single();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ShapefileFeature>(result);
            Assert.AreEqual(((ShapefileFeature)result).FeatureId, 1);
            Assert.IsNotNull(result.Attributes);

            HelperMethods.AssertPolygonsEqual(result.Geometry as IPolygon, expectedTriangle);

            object shapeNameData = result.Attributes["ShapeName"];
            Assert.IsInstanceOf<string>(shapeNameData);

            Assert.AreEqual((string)shapeNameData, expectedShapeMetadata);
        }

        [Test]
        public void ReadByGeoFilter_ReadWithWholeRectangleInBounds_ShouldReturnRectangle()
        {
            Envelope boundsWithWholeTriangle = new Envelope(-1.39510, -0.12716, -1.13938, -0.22977);

            // Arrange.
            m_TempFiles = new TempFileWriter[]
			{
				new TempFileWriter("test.shp", ShpFiles.Read("UnifiedChecksMaterial")),
				new TempFileWriter("test.dbf", DbfFiles.Read("UnifiedChecksMaterial")),
			};

            Polygon expectedTriangle = new Polygon(new LinearRing(new Coordinate[]
					{
						new Coordinate(-0.815656565656566, -0.439393939393939),
						new Coordinate(-0.353535353535354, -0.795454545454545),
						new Coordinate(-0.888888888888889,-0.929292929292929),
						new Coordinate(-1.151515151515152, -0.419191919191919),
						new Coordinate(-0.815656565656566,-0.439393939393939),
					}));

            string expectedShapeMetadata = "Rectangle";

            m_shapeDataReader = new ShapeDataReader(m_TempFiles[0].Path);

            // Act.
            IEnumerable<IShapefileFeature> results = m_shapeDataReader.ReadByMBRFilter(boundsWithWholeTriangle);

            // Assert.
            Assert.IsNotNull(results);

            IShapefileFeature result = results.Single();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ShapefileFeature>(result);
            Assert.AreEqual(((ShapefileFeature)result).FeatureId, 0);
            Assert.IsNotNull(result.Attributes);

            HelperMethods.AssertPolygonsEqual(result.Geometry as IPolygon, expectedTriangle);

            object shapeNameData = result.Attributes["ShapeName"];
            Assert.IsInstanceOf<string>(shapeNameData);

            Assert.AreEqual((string)shapeNameData, expectedShapeMetadata);
        }

        [Test]
        public void ReadByGeoFilter_ReadWithWholeRectangleInBoundsAndFlagSetToTrue_ShouldReturnRectangle()
        {
            Envelope boundsWithWholeTriangle = new Envelope(-1.39510, -0.12716, -1.13938, -0.22977);

            // Arrange.
            m_TempFiles = new TempFileWriter[]
			{
				new TempFileWriter("test.shp", ShpFiles.Read("UnifiedChecksMaterial")),
				new TempFileWriter("test.dbf", DbfFiles.Read("UnifiedChecksMaterial")),
			};

            Polygon expectedTriangle = new Polygon(new LinearRing(new Coordinate[]
					{
						new Coordinate(-0.815656565656566, -0.439393939393939),
						new Coordinate(-0.353535353535354, -0.795454545454545),
						new Coordinate(-0.888888888888889,-0.929292929292929),
						new Coordinate(-1.151515151515152, -0.419191919191919),
						new Coordinate(-0.815656565656566,-0.439393939393939),
					}));

            string expectedShapeMetadata = "Rectangle";

            m_shapeDataReader = new ShapeDataReader(m_TempFiles[0].Path);

            // Act.
            IEnumerable<IShapefileFeature> results = m_shapeDataReader.ReadByMBRFilter(boundsWithWholeTriangle, true);

            // Assert.
            Assert.IsNotNull(results);

            IShapefileFeature result = results.Single();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ShapefileFeature>(result);
            Assert.AreEqual(((ShapefileFeature)result).FeatureId, 0);
            Assert.IsNotNull(result.Attributes);

            HelperMethods.AssertPolygonsEqual(result.Geometry as IPolygon, expectedTriangle);

            object shapeNameData = result.Attributes["ShapeName"];
            Assert.IsInstanceOf<string>(shapeNameData);

            Assert.AreEqual((string)shapeNameData, expectedShapeMetadata);
        }

        [Test]
        public void ReadByGeoFilter_ReadWithRectanglePartiallyInBounds_ShouldReturnRectangle()
        {
            Envelope boundsWithWholeTriangle = new Envelope(-0.93340, -0.38902, -0.73281, -0.29179);

            // Arrange.
            m_TempFiles = new TempFileWriter[]
			{
				new TempFileWriter("test.shp", ShpFiles.Read("UnifiedChecksMaterial")),
				new TempFileWriter("test.dbf", DbfFiles.Read("UnifiedChecksMaterial")),
			};

            Polygon expectedTriangle = new Polygon(new LinearRing(new Coordinate[]
					{
						new Coordinate(-0.815656565656566, -0.439393939393939),
						new Coordinate(-0.353535353535354, -0.795454545454545),
						new Coordinate(-0.888888888888889,-0.929292929292929),
						new Coordinate(-1.151515151515152, -0.419191919191919),
						new Coordinate(-0.815656565656566,-0.439393939393939),
					}));

            string expectedShapeMetadata = "Rectangle";

            m_shapeDataReader = new ShapeDataReader(m_TempFiles[0].Path);

            // Act.
            IEnumerable<IShapefileFeature> results = m_shapeDataReader.ReadByMBRFilter(boundsWithWholeTriangle);

            // Assert.
            Assert.IsNotNull(results);

            IShapefileFeature result = results.Single();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ShapefileFeature>(result);
            Assert.AreEqual(((ShapefileFeature)result).FeatureId, 0);
            Assert.IsNotNull(result.Attributes);

            HelperMethods.AssertPolygonsEqual(result.Geometry as IPolygon, expectedTriangle);

            object shapeNameData = result.Attributes["ShapeName"];
            Assert.IsInstanceOf<string>(shapeNameData);

            Assert.AreEqual((string)shapeNameData, expectedShapeMetadata);
        }

        [Test]
        public void ReadByGeoFilter_ReadWithRectanglePartiallyInBoundsAndFlagSetToTrue_ShouldReturnRectangle()
        {
            Envelope boundsWithWholeTriangle = new Envelope(-0.93340, -0.38902, -0.73281, -0.29179);

            // Arrange.
            m_TempFiles = new TempFileWriter[]
			{
				new TempFileWriter("test.shp", ShpFiles.Read("UnifiedChecksMaterial")),
				new TempFileWriter("test.dbf", DbfFiles.Read("UnifiedChecksMaterial")),
			};

            Polygon expectedTriangle = new Polygon(new LinearRing(new Coordinate[]
					{
						new Coordinate(-0.815656565656566, -0.439393939393939),
						new Coordinate(-0.353535353535354, -0.795454545454545),
						new Coordinate(-0.888888888888889,-0.929292929292929),
						new Coordinate(-1.151515151515152, -0.419191919191919),
						new Coordinate(-0.815656565656566,-0.439393939393939),
					}));

            string expectedShapeMetadata = "Rectangle";

            m_shapeDataReader = new ShapeDataReader(m_TempFiles[0].Path);

            // Act.
            IEnumerable<IShapefileFeature> results = m_shapeDataReader.ReadByMBRFilter(boundsWithWholeTriangle, true);

            // Assert.
            Assert.IsNotNull(results);

            IShapefileFeature result = results.Single();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ShapefileFeature>(result);
            Assert.AreEqual(((ShapefileFeature)result).FeatureId, 0);
            Assert.IsNotNull(result.Attributes);

            HelperMethods.AssertPolygonsEqual(result.Geometry as IPolygon, expectedTriangle);

            object shapeNameData = result.Attributes["ShapeName"];
            Assert.IsInstanceOf<string>(shapeNameData);

            Assert.AreEqual((string)shapeNameData, expectedShapeMetadata);
        }

        [Test]
        public void ReadByGeoFilter_ReadWithRectangleMBRPartiallyInBoundsAndFlagSetToTrue_ShouldReturnNoGeometries()
        {
            Envelope boundsWithWholeTriangle = new Envelope(-1.17459, -1.00231, -1.09803, -0.80861);

            // Arrange.
            m_TempFiles = new TempFileWriter[]
			{
				new TempFileWriter("test.shp", ShpFiles.Read("UnifiedChecksMaterial")),
				new TempFileWriter("test.dbf", DbfFiles.Read("UnifiedChecksMaterial")),
			};

            m_shapeDataReader = new ShapeDataReader(m_TempFiles[0].Path);

            // Act.
            IEnumerable<IShapefileFeature> results = m_shapeDataReader.ReadByMBRFilter(boundsWithWholeTriangle, true);

            // Assert.
            Assert.IsNotNull(results);
            Assert.IsFalse(results.Any());
        }

        // TODO: Don't know how bad it is that this tests passes.
        // I give it as a parameter a rectangle that partially intersects only with the MBR of the
        // shape, and doesn't intersect with the shape itself at all.
        // It only works because the default index is RTree, use a different index?
        [Test]
        public void ReadByGeoFilter_ReadWithRectangleMBRPartiallyInBounds_ShouldReturnRectangle()
        {
            Envelope boundsWithWholeTriangle = new Envelope(-1.17459, -1.00231, -1.09803, -0.80861);

            // Arrange.
            m_TempFiles = new TempFileWriter[]
			{
				new TempFileWriter("test.shp", ShpFiles.Read("UnifiedChecksMaterial")),
				new TempFileWriter("test.dbf", DbfFiles.Read("UnifiedChecksMaterial")),
			};

            Polygon expectedTriangle = new Polygon(new LinearRing(new Coordinate[]
					{
						new Coordinate(-0.815656565656566, -0.439393939393939),
						new Coordinate(-0.353535353535354, -0.795454545454545),
						new Coordinate(-0.888888888888889,-0.929292929292929),
						new Coordinate(-1.151515151515152, -0.419191919191919),
						new Coordinate(-0.815656565656566,-0.439393939393939),
					}));

            string expectedShapeMetadata = "Rectangle";

            m_shapeDataReader = new ShapeDataReader(m_TempFiles[0].Path);

            // Act.
            IEnumerable<IShapefileFeature> results = m_shapeDataReader.ReadByMBRFilter(boundsWithWholeTriangle);

            // Assert.
            Assert.IsNotNull(results);

            IShapefileFeature result = results.Single();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ShapefileFeature>(result);
            Assert.AreEqual(((ShapefileFeature)result).FeatureId, 0);
            Assert.IsNotNull(result.Attributes);

            HelperMethods.AssertPolygonsEqual(result.Geometry as IPolygon, expectedTriangle);

            object shapeNameData = result.Attributes["ShapeName"];
            Assert.IsInstanceOf<string>(shapeNameData);

            Assert.AreEqual((string)shapeNameData, expectedShapeMetadata);
        }

        [Test]
        public void ReadByGeoFilter_ReadWithNoShapeInBounds_ShouldReturnEmptyEnumerable()
        {
            Envelope boundsWithWholeTriangle = new Envelope(-1.17459, -1.00231, -1.09803, -1.5);

            // Arrange.
            m_TempFiles = new TempFileWriter[]
			{
				new TempFileWriter("test.shp", ShpFiles.Read("UnifiedChecksMaterial")),
				new TempFileWriter("test.dbf", DbfFiles.Read("UnifiedChecksMaterial")),
			};

            m_shapeDataReader = new ShapeDataReader(m_TempFiles[0].Path);

            // Act.
            IEnumerable<IShapefileFeature> results = m_shapeDataReader.ReadByMBRFilter(boundsWithWholeTriangle);

            // Assert.
            Assert.IsNotNull(results);
            Assert.IsFalse(results.Any());
        }

        [Test]
        public void ReadByGeoFilter_ReadWithNoShapeInBoundsAndFlagSetToTrue_ShouldReturnEmptyEnumerable()
        {
            Envelope boundsWithWholeTriangle = new Envelope(-1.17459, -1.00231, -1.09803, -1.5);

            // Arrange.
            m_TempFiles = new TempFileWriter[]
			{
				new TempFileWriter("test.shp", ShpFiles.Read("UnifiedChecksMaterial")),
				new TempFileWriter("test.dbf", DbfFiles.Read("UnifiedChecksMaterial")),
			};

            m_shapeDataReader = new ShapeDataReader(m_TempFiles[0].Path);

            // Act.
            IEnumerable<IShapefileFeature> results = m_shapeDataReader.ReadByMBRFilter(boundsWithWholeTriangle, true);

            // Assert.
            Assert.IsNotNull(results);
            Assert.IsFalse(results.Any());
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReadByGeoFilter_ReadDbfDataAfterReaderObjectDisposed_ShouldThrowException()
        {
            Envelope boundsWithWholeTriangle = new Envelope(-1.17459, -1.00231, -1.09803, -0.80861);

            // Arrange.
            m_TempFiles = new TempFileWriter[]
			{
				new TempFileWriter("test.shp", ShpFiles.Read("UnifiedChecksMaterial")),
				new TempFileWriter("test.dbf", DbfFiles.Read("UnifiedChecksMaterial")),
			};

            m_shapeDataReader = new ShapeDataReader(m_TempFiles[0].Path);

            // Act.
            IEnumerable<IShapefileFeature> results = m_shapeDataReader.ReadByMBRFilter(boundsWithWholeTriangle);

            // Assert.
            Assert.IsNotNull(results);
            IShapefileFeature result = results.Single();

            // Dispose of the reader object.
            m_shapeDataReader.Dispose();

            // Try reading dbf data.
            IAttributesTable table = result.Attributes;
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReadByGeoFilter_ReadShapeDataAfterReaderObjectDisposed_ShouldThrowException()
        {
            Envelope boundsWithWholeTriangle = new Envelope(-1.17459, -1.00231, -1.09803, -0.80861);

            // Arrange.
            m_TempFiles = new TempFileWriter[]
			{
				new TempFileWriter("test.shp", ShpFiles.Read("UnifiedChecksMaterial")),
				new TempFileWriter("test.dbf", DbfFiles.Read("UnifiedChecksMaterial")),
			};

            m_shapeDataReader = new ShapeDataReader(m_TempFiles[0].Path);

            // Act.
            IEnumerable<IShapefileFeature> results = m_shapeDataReader.ReadByMBRFilter(boundsWithWholeTriangle);

            // Assert.
            Assert.IsNotNull(results);
            IShapefileFeature result = results.Single();

            // Dispose of the reader object.
            m_shapeDataReader.Dispose();

            // Try reading dbf data.
            IGeometry table = result.Geometry;
        }

        [TearDown]
        public void TestCleanup()
        {
            if (m_shapeDataReader != null)
            {
                m_shapeDataReader.Dispose();
                m_shapeDataReader = null;
            }

            if (m_TempFiles != null)
            {
                foreach (TempFileWriter tempFile in m_TempFiles)
                {
                    tempFile.Dispose();
                }

                m_TempFiles = null;
            }
        }
    }
}
