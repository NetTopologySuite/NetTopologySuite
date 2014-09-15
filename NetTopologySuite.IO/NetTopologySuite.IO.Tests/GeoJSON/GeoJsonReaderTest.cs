using NetTopologySuite.CoordinateSystems;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.IO.Tests.GeoJSON
{
    /// <summary>
    /// This is a test class for GeoJsonReaderTest and is intended
    /// to contain all GeoJsonReaderTest Unit Tests
    /// </summary> 
    [TestFixture]
    public class GeoJsonReaderTest
    {
        /// <summary>
        ///    A test for GeoJsonReader Read method
        /// </summary>
        [Test]
        public void GeoJsonReaderReadFeatureCollectionTest()
        {
            const string json = "{\"features\":[{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[23.0,56.0]},\"properties\":{\"test1\":\"value1\"}}],\"type\":\"FeatureCollection\",\"crs\":{\"type\":\"name\",\"properties\":{\"name\":\"name1\"}}}";
            GeoJsonReader reader = new GeoJsonReader();
            FeatureCollection result = reader.Read<FeatureCollection>(json);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.IsNotNull(result.CRS);
            Assert.IsInstanceOf(typeof(NamedCRS), result.CRS);
            Assert.AreEqual(CRSTypes.Name, result.CRS.Type);
            NamedCRS crs = (NamedCRS)result.CRS;
            Assert.AreEqual("name1", crs.Properties["name"]);
        }

        /// <summary>
        ///   A test for GeoJsonReader Read method
        /// </summary>
        [Test]
        public void GeoJsonReaderReadFeatureTest()
        {
            const string json = "{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[23.0,56.0]},\"properties\":{\"test1\":\"value1\"}}";
            IFeature result = new GeoJsonReader().Read<Feature>(json);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(Point), result.Geometry);
            Point p = (Point)result.Geometry;
            Assert.AreEqual(23, p.X);
            Assert.AreEqual(56, p.Y);
            Assert.IsNotNull(result.Attributes);
            Assert.AreEqual(1, result.Attributes.Count);
            Assert.AreEqual("value1", result.Attributes["test1"]);
        }

        /// <summary>
        ///   A test for GeoJsonReader Read method
        /// </summary>
        [Test]
        public void GeoJsonReaderReadCRSTest()
        {
            const string json = "{\"type\":\"name\",\"properties\":{\"name\":\"name1\"}}";
            CRSBase result = new GeoJsonReader().Read<CRSBase>(json);

            Assert.IsNotNull(result);
            Assert.AreEqual(CRSTypes.Name, result.Type);
            Assert.IsNotNull(result.Properties);
            Assert.AreEqual("name1", result.Properties["name"]);
        }

        /// <summary>
        ///   A test for GeoJsonReader Read method
        /// </summary>
        [Test]
        public void GeoJsonReaderReadGeometryPointTest()
        {
            const string json = "{\"type\":\"Point\",\"coordinates\":[23.0,56.0]}";
            Geometry result = new GeoJsonReader().Read<Geometry>(json);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(Point), result);
            Point p = (Point)result;
            Assert.AreEqual(23, p.X);
            Assert.AreEqual(56, p.Y);
        }

        /// <summary>
        ///   A test for GeoJsonReader Read method
        /// </summary>
        [Test]
        public void GeoJsonReaderReadGeometryLineStringTest()
        {
            const string json = "{\"type\": \"LineString\",\"coordinates\": [ [100.0, 0.0], [101.0, 1.0] ]}";
            Geometry result = new GeoJsonReader().Read<Geometry>(json);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(LineString), result);
            LineString ls = (LineString)result;
            Assert.AreEqual(2, ls.Coordinates.Length);
            Assert.AreEqual(100, ls.Coordinates[0].X);
            Assert.AreEqual(0, ls.Coordinates[0].Y);
            Assert.AreEqual(101, ls.Coordinates[1].X);
            Assert.AreEqual(1, ls.Coordinates[1].Y);
        }

        /// <summary>
        ///   A test for GeoJsonReader Read method
        /// </summary>
        [Test]
        public void GeoJsonReaderReadGeometryPolygonTest()
        {
            const string json = "{\"type\": \"Polygon\",\"coordinates\": [[ [100.0, 0.0], [101.0, 0.0], [101.0, 1.0], [100.0, 1.0], [100.0, 0.0] ]]}";
            Geometry result = new GeoJsonReader().Read<Geometry>(json);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(Polygon), result);
            Polygon poly = (Polygon)result;
            Assert.AreEqual(5, poly.Coordinates.Length);
            Assert.AreEqual(100, poly.Coordinates[0].X);
            Assert.AreEqual(0, poly.Coordinates[0].Y);
            Assert.AreEqual(101, poly.Coordinates[1].X);
            Assert.AreEqual(0, poly.Coordinates[1].Y);
            Assert.AreEqual(101, poly.Coordinates[2].X);
            Assert.AreEqual(1, poly.Coordinates[2].Y);
            Assert.AreEqual(100, poly.Coordinates[3].X);
            Assert.AreEqual(1, poly.Coordinates[3].Y);
            Assert.AreEqual(100, poly.Coordinates[4].X);
            Assert.AreEqual(0, poly.Coordinates[4].Y);
        }

        [Test]
        public void GeoJsonReaderReadFeatureWithCoordinatesBeforeTypeTest()
        {
            const string json = "{\"type\":\"Feature\",\"geometry\":{\"coordinates\":[23.0,56.0], \"type\":\"Point\"},\"properties\":{\"test1\":\"value1\"}}";
            IFeature result = new GeoJsonReader().Read<Feature>(json);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(Point), result.Geometry);
            Point p = (Point)result.Geometry;
            Assert.AreEqual(23, p.X);
            Assert.AreEqual(56, p.Y);
            Assert.IsNotNull(result.Attributes);
            Assert.AreEqual(1, result.Attributes.Count);
            Assert.AreEqual("value1", result.Attributes["test1"]);
        }
        /// <summary>
        ///   A test for GeoJsonReader Read method
        /// </summary>
        [Test]
        public void GeoJsonReaderReadGeometryPolygonWithHoleTest()
        {
            const string json = "{\"type\": \"Polygon\",\"coordinates\": [[[100.0, 0.0], [101.0, 0.0], [101.0, 1.0], [100.0, 1.0], [100.0, 0.0]], [[100.2, 0.2], [100.8, 0.2], [100.8, 0.8], [100.2, 0.8], [100.2, 0.2]]]}";
            Geometry result = new GeoJsonReader().Read<Geometry>(json);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(Polygon), result);
            Polygon poly = (Polygon)result;

            Assert.AreEqual(1, poly.NumInteriorRings);
            Assert.AreEqual(100, poly.ExteriorRing.Coordinates[0].X);
            Assert.AreEqual(0, poly.ExteriorRing.Coordinates[0].Y);
            Assert.AreEqual(101, poly.ExteriorRing.Coordinates[1].X);
            Assert.AreEqual(0, poly.ExteriorRing.Coordinates[1].Y);
            Assert.AreEqual(101, poly.ExteriorRing.Coordinates[2].X);
            Assert.AreEqual(1, poly.ExteriorRing.Coordinates[2].Y);
            Assert.AreEqual(100, poly.ExteriorRing.Coordinates[3].X);
            Assert.AreEqual(1, poly.ExteriorRing.Coordinates[3].Y);
            Assert.AreEqual(100, poly.ExteriorRing.Coordinates[4].X);
            Assert.AreEqual(0, poly.ExteriorRing.Coordinates[4].Y);

            Assert.AreEqual(100.2, poly.GetInteriorRingN(0).Coordinates[0].X);
            Assert.AreEqual(0.2, poly.GetInteriorRingN(0).Coordinates[0].Y);
            Assert.AreEqual(100.8, poly.GetInteriorRingN(0).Coordinates[1].X);
            Assert.AreEqual(0.2, poly.GetInteriorRingN(0).Coordinates[1].Y);
            Assert.AreEqual(100.8, poly.GetInteriorRingN(0).Coordinates[2].X);
            Assert.AreEqual(0.8, poly.GetInteriorRingN(0).Coordinates[2].Y);
            Assert.AreEqual(100.2, poly.GetInteriorRingN(0).Coordinates[3].X);
            Assert.AreEqual(0.8, poly.GetInteriorRingN(0).Coordinates[3].Y);
            Assert.AreEqual(100.2, poly.GetInteriorRingN(0).Coordinates[4].X);
            Assert.AreEqual(0.2, poly.GetInteriorRingN(0).Coordinates[4].Y);
        }

        /// <summary>
        ///   A test for GeoJsonReader Read method
        /// </summary>
        [Test]
        public void GeoJsonReaderReadGeometryMultiPointTest()
        {
            const string json = "{\"type\": \"MultiPoint\",\"coordinates\": [[100.0, 0.0], [101.0, 1.0]]}";
            Geometry result = new GeoJsonReader().Read<Geometry>(json);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(MultiPoint), result);
            MultiPoint multiPoint = (MultiPoint)result;

            Assert.AreEqual(2, multiPoint.Coordinates.Length);
            Assert.AreEqual(2, multiPoint.NumGeometries);
            Assert.AreEqual(100, multiPoint.Coordinates[0].X);
            Assert.AreEqual(0, multiPoint.Coordinates[0].Y);
            Assert.AreEqual(101, multiPoint.Coordinates[1].X);
            Assert.AreEqual(1, multiPoint.Coordinates[1].Y);
            
        }

        /// <summary>
        ///   A test for GeoJsonReader Read method
        /// </summary>
        [Test]
        public void GeoJsonReaderReadGeometryMultiLineStringTest()
        {
            const string json = "{\"type\": \"MultiLineString\",\"coordinates\": [[[100.0, 0.0], [101.0, 1.0]], [[102.0, 2.0], [103.0, 3.0]]]}";
            Geometry result = new GeoJsonReader().Read<Geometry>(json);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(MultiLineString), result);
            MultiLineString multiLineString = (MultiLineString)result;

            Assert.AreEqual(2, multiLineString.NumGeometries);
            Assert.AreEqual(100, multiLineString.Coordinates[0].X);
            Assert.AreEqual(0, multiLineString.Coordinates[0].Y);
            Assert.AreEqual(101, multiLineString.Coordinates[1].X);
            Assert.AreEqual(1, multiLineString.Coordinates[1].Y);
            Assert.AreEqual(102, multiLineString.Coordinates[2].X);
            Assert.AreEqual(2, multiLineString.Coordinates[2].Y);
            Assert.AreEqual(103, multiLineString.Coordinates[3].X);
            Assert.AreEqual(3, multiLineString.Coordinates[3].Y);
            
        }

        /// <summary>
        ///   A test for GeoJsonReader Read method
        /// </summary>
        [Test]
        public void GeoJsonReaderReadGeometryMultiPolygonTest()
        {
            const string json = "{\"type\": \"MultiPolygon\",\"coordinates\": [[[[102.0, 2.0], [103.0, 2.0], [103.0, 3.0], [102.0, 3.0], [102.0, 2.0]]],[[[100.0, 0.0], [101.0, 0.0], [101.0, 1.0], [100.0, 1.0], [100.0, 0.0]], [[100.2, 0.2], [100.8, 0.2], [100.8, 0.8], [100.2, 0.8], [100.2, 0.2]]]]}";
            Geometry result = new GeoJsonReader().Read<Geometry>(json);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(MultiPolygon), result);
            MultiPolygon multiPolygon = (MultiPolygon)result;

            Assert.AreEqual(2, multiPolygon.NumGeometries);

            Assert.AreEqual(102, multiPolygon.Geometries[0].Coordinates[0].X);
            Assert.AreEqual(2, multiPolygon.Geometries[0].Coordinates[0].Y);
            Assert.AreEqual(103, multiPolygon.Geometries[0].Coordinates[1].X);
            Assert.AreEqual(2, multiPolygon.Geometries[0].Coordinates[1].Y);
            Assert.AreEqual(103, multiPolygon.Geometries[0].Coordinates[2].X);
            Assert.AreEqual(3, multiPolygon.Geometries[0].Coordinates[2].Y);
            Assert.AreEqual(102, multiPolygon.Geometries[0].Coordinates[3].X);
            Assert.AreEqual(3, multiPolygon.Geometries[0].Coordinates[3].Y);
            Assert.AreEqual(102, multiPolygon.Geometries[0].Coordinates[4].X);
            Assert.AreEqual(2, multiPolygon.Geometries[0].Coordinates[4].Y);
            Polygon poly1 = (Polygon) multiPolygon.Geometries[0];
            Assert.AreEqual(0, poly1.NumInteriorRings);


            Assert.AreEqual(100, multiPolygon.Geometries[1].Coordinates[0].X);
            Assert.AreEqual(0, multiPolygon.Geometries[1].Coordinates[0].Y);
            Assert.AreEqual(101, multiPolygon.Geometries[1].Coordinates[1].X);
            Assert.AreEqual(0, multiPolygon.Geometries[1].Coordinates[1].Y);
            Assert.AreEqual(101, multiPolygon.Geometries[1].Coordinates[2].X);
            Assert.AreEqual(1, multiPolygon.Geometries[1].Coordinates[2].Y);
            Assert.AreEqual(100, multiPolygon.Geometries[1].Coordinates[3].X);
            Assert.AreEqual(1, multiPolygon.Geometries[1].Coordinates[3].Y);
            Assert.AreEqual(100, multiPolygon.Geometries[1].Coordinates[4].X);
            Assert.AreEqual(0, multiPolygon.Geometries[1].Coordinates[4].Y);
            Polygon poly2 = (Polygon)multiPolygon.Geometries[1];
            Assert.AreEqual(1, poly2.NumInteriorRings);

            Assert.AreEqual(100.2, poly2.GetInteriorRingN(0).Coordinates[0].X);
            Assert.AreEqual(0.2, poly2.GetInteriorRingN(0).Coordinates[0].Y);
            Assert.AreEqual(100.8, poly2.GetInteriorRingN(0).Coordinates[1].X);
            Assert.AreEqual(0.2, poly2.GetInteriorRingN(0).Coordinates[1].Y);
            Assert.AreEqual(100.8, poly2.GetInteriorRingN(0).Coordinates[2].X);
            Assert.AreEqual(0.8, poly2.GetInteriorRingN(0).Coordinates[2].Y);
            Assert.AreEqual(100.2, poly2.GetInteriorRingN(0).Coordinates[3].X);
            Assert.AreEqual(0.8, poly2.GetInteriorRingN(0).Coordinates[3].Y);
            Assert.AreEqual(100.2, poly2.GetInteriorRingN(0).Coordinates[4].X);
            Assert.AreEqual(0.2, poly2.GetInteriorRingN(0).Coordinates[4].Y);

        }


        /// <summary>
        ///   A test for GeoJsonReader Read method
        /// </summary>
        [Test]
        public void GeoJsonReaderReadGeometryCollectionTest()
        {
            const string json ="{\"type\": \"GeometryCollection\", \"geometries\": [{\"type\": \"Point\", \"coordinates\": [99.0, 89.0]}, { \"type\": \"LineString\", \"coordinates\": [ [101.0, 0.0], [102.0, 1.0]]}]";
            Geometry result = new GeoJsonReader().Read<Geometry>(json);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(GeometryCollection), result);
            GeometryCollection geometryCollection = (GeometryCollection)result;
            Assert.AreEqual(2, geometryCollection.Count);

            Point point = (Point)geometryCollection.GetGeometryN(0);
            Assert.IsInstanceOf(typeof(Point), point);
            Assert.AreEqual(99, point.X);
            Assert.AreEqual(89, point.Y);

            LineString lineString = (LineString)geometryCollection.GetGeometryN(1);
            Assert.IsInstanceOf(typeof(LineString), lineString);
            Assert.AreEqual(2, lineString.Coordinates.Length);
            Assert.AreEqual(101, lineString.Coordinates[0].X);
            Assert.AreEqual(0, lineString.Coordinates[0].Y);
            Assert.AreEqual(102, lineString.Coordinates[1].X);
            Assert.AreEqual(1, lineString.Coordinates[1].Y);
        }
    }
}
