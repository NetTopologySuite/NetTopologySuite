using NetTopologySuite.CoordinateSystems;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.IO.Tests.GeoJSON
{
    ///<summary>
    ///    This is a test class for GeoJsonReaderTest and is intended
    ///    to contain all GeoJsonReaderTest Unit Tests
    ///</summary> 
    [TestFixture]
    public class GeoJsonReaderTest
    {
        ///<summary>
        ///    A test for GeoJsonReader Read method
        ///</summary>
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

        ///<summary>
        ///   A test for GeoJsonReader Read method
        ///</summary>
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

        ///<summary>
        ///   A test for GeoJsonReader Read method
        ///</summary>
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

        ///<summary>
        ///   A test for GeoJsonReader Read method
        ///</summary>
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

        ///<summary>
        ///   A test for GeoJsonReader Read method
        ///</summary>
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

        ///<summary>
        ///   A test for GeoJsonReader Read method
        ///</summary>
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
    }
}
