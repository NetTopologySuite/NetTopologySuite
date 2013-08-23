using System;
using System.Collections.ObjectModel;
using NetTopologySuite.CoordinateSystems;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.IO.Tests.GeoJSON
{
    ///<summary>
    ///    This is a test class for GeoJsonWriterTest and is intended
    ///    to contain all GeoJsonWriterTest Unit Tests
    ///</summary>
    [TestFixture]
    public class GeoJsonWriterTest
    {       
        ///<summary>
        ///    A test for GeoJsonWriter Write method
        ///</summary>
        [Test]
        public void GeoJsonWriterWriteFeatureCollectionTest()
        {
            AttributesTable attributes = new AttributesTable();
            attributes.AddAttribute("test1", "value1");
            Feature feature = new Feature(new Point(23, 56), attributes);
            FeatureCollection featureCollection = new FeatureCollection(new Collection<Feature> { feature }) { CRS = new NamedCRS("name1") };
            string actual = new GeoJsonWriter().Write(featureCollection);
            Assert.AreEqual("{\"features\":[{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[23.0,56.0]},\"properties\":{\"test1\":\"value1\"}}],\"type\":\"FeatureCollection\",\"crs\":{\"type\":\"name\",\"properties\":{\"name\":\"name1\"}}}", actual);
        }

        ///<summary>
        ///    A test for GeoJsonWriter Write method
        ///</summary>
        [Test]
        public void GeoJsonWriterWriteFeatureTest()
        {
            AttributesTable attributes = new AttributesTable();
            attributes.AddAttribute("test1", "value1");
            Feature feature = new Feature(new Point(23, 56), attributes);
            string actual = new GeoJsonWriter().Write(feature);
            Assert.AreEqual("{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[23.0,56.0]},\"properties\":{\"test1\":\"value1\"}}", actual);
        }

        ///<summary>
        ///    A test for GeoJsonWriter Write method
        ///</summary>
        [Test]
        public void GeoJsonWriterWriteGeometryTest()
        {
            string actual = new GeoJsonWriter().Write(new Point(23, 56));
            Assert.AreEqual("{\"type\":\"Point\",\"coordinates\":[23.0,56.0]}", actual);
        }

        ///<summary>
        ///    A test for GeoJsonWriter Write method
        ///</summary>
        [Test]
        public void GeoJsonWriterWriteAttributesTest()
        {
            AttributesTable attributes = new AttributesTable();
            attributes.AddAttribute("test1", "value1");
            string actual = new GeoJsonWriter().Write(attributes);
            Assert.AreEqual("\"properties\":{\"test1\":\"value1\"}", actual);
        }

        ///<summary>
        ///    A test for GeoJsonWriter Write method
        ///</summary>
        [Test]
        public void GeoJsonWriterWriteAnyObjectTest()
        {
            AttributesTable attributes = new AttributesTable();
            attributes.AddAttribute("test1", "value1");
            Feature feature = new Feature(new Point(23, 56), attributes);
            FeatureCollection featureCollection = new FeatureCollection(new Collection<Feature> { feature }) { CRS = new NamedCRS("name1") };
            string actual = new GeoJsonWriter().Write(new { featureCollection, new DateTime(2012, 8, 8).Date });
            Assert.AreEqual("{\"featureCollection\":{\"features\":[{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[23.0,56.0]},\"properties\":{\"test1\":\"value1\"}}],\"type\":\"FeatureCollection\",\"crs\":{\"type\":\"name\",\"properties\":{\"name\":\"name1\"}}},\"Date\":\"\\/Date(1344376800000+0200)\\/\"}", actual);
        }
    }
}