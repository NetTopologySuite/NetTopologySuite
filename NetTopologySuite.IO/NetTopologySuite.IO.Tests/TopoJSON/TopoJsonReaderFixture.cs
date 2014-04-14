using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.IO.TopoJSON.Fixtures
{
    [TestFixture]
    public class TopoJsonReaderFixture
    {
        private static readonly IGeometryFactory Factory = GeometryFactory.Fixed;

        [Test]
        public void read_reference_data()
        {
            const string data = TopoData.Reference;
            TopoJsonReader reader = new TopoJsonReader(Factory);
            IDictionary<string, FeatureCollection> coll = reader.
                Read<IDictionary<string, FeatureCollection>>(data);
            ValidateData(coll);
        }

        [Test]
        public void read_quantized_data()
        {
            const string data = TopoData.Quantized;
            TopoJsonReader reader = new TopoJsonReader(Factory);
            IDictionary<string, FeatureCollection> coll = reader.
                Read<IDictionary<string, FeatureCollection>>(data);
            ValidateData(coll);       
        }

        private static void ValidateData(IDictionary<string, FeatureCollection> coll)
        {
            Assert.That(coll, Is.Not.Null);
            Assert.That(coll, Is.Not.Empty);
            Assert.That(coll.Keys, Is.Not.Empty);
            Assert.That(coll.Keys.Count, Is.EqualTo(1));
            Assert.That(coll.ContainsKey("example"), Is.True);

            FeatureCollection fc = coll["example"];
            Assert.That(fc, Is.Not.Null);
            Assert.That(fc.Count, Is.EqualTo(3));

            IFeature f1 = fc[0];
            Assert.That(f1, Is.Not.Null);
            Assert.That(f1.Geometry, Is.Not.Null);
            Assert.That(f1.Geometry.Factory, Is.EqualTo(Factory));
            Assert.That(f1.Geometry, Is.InstanceOf<IPoint>());
            Assert.That(f1.Geometry.AsText(), Is.EqualTo("POINT (102 0.5)"));
            Assert.That(f1.Attributes, Is.Not.Null);
            Assert.That(f1.Attributes.Count, Is.EqualTo(1));
            Assert.That(f1.Attributes.Exists("prop0"), Is.True);
            Assert.That(f1.Attributes["prop0"], Is.EqualTo("value0"));

            IFeature f2 = fc[1];
            Assert.That(f2, Is.Not.Null);
            Assert.That(f2.Geometry, Is.Not.Null);
            Assert.That(f2.Geometry.Factory, Is.EqualTo(Factory));
            Assert.That(f2.Geometry, Is.InstanceOf<ILineString>());
            Assert.That(f2.Geometry.AsText(), Is.EqualTo("LINESTRING (102 0, 103 1, 104 0, 105 1)"));
            Assert.That(f2.Attributes, Is.Not.Null);
            Assert.That(f2.Attributes.Count, Is.EqualTo(0));

            IFeature f3 = fc[2];
            Assert.That(f3, Is.Not.Null);
            Assert.That(f3.Geometry, Is.Not.Null);
            Assert.That(f3.Geometry.Factory, Is.EqualTo(Factory));
            Assert.That(f3.Geometry, Is.InstanceOf<IPolygon>());
            Assert.That(f3.Geometry.AsText(), Is.EqualTo("POLYGON ((100 0, 100 1, 101 1, 101 0, 100 0))"));
            Assert.That(f3.Attributes, Is.Not.Null);
            Assert.That(f3.Attributes.Count, Is.EqualTo(2));
            Assert.That(f3.Attributes.Exists("prop0"), Is.True);
            Assert.That(f3.Attributes["prop0"], Is.EqualTo("value0"));
            Assert.That(f3.Attributes.Exists("prop1"), Is.True);
            Assert.That(f3.Attributes["prop1"], Is.InstanceOf<IAttributesTable>());
            IAttributesTable inner = (IAttributesTable) f3.Attributes["prop1"];
            Assert.That(inner.Count, Is.EqualTo(1));
            Assert.That(inner.Exists("this"), Is.True);
            Assert.That(inner["this"], Is.EqualTo("that"));
        }

        [Test]
        public void read_multi_reference_data()
        {
            const string data = TopoData.MultiReference;
            TopoJsonReader reader = new TopoJsonReader(Factory);
            IDictionary<string, FeatureCollection> coll = reader.
                Read<IDictionary<string, FeatureCollection>>(data);
            ValidateMulti(coll);
        }

        [Test]
        public void read_multi_quantized_data()
        {
            const string data = TopoData.MultiQuantized;
            TopoJsonReader reader = new TopoJsonReader(Factory);
            IDictionary<string, FeatureCollection> coll = reader.
                Read<IDictionary<string, FeatureCollection>>(data);
            ValidateMulti(coll);
        }

        private static void ValidateMulti(IDictionary<string, FeatureCollection> coll)
        {
            Assert.That(coll, Is.Not.Null);
            Assert.That(coll, Is.Not.Empty);
            Assert.That(coll.Keys, Is.Not.Empty);
            Assert.That(coll.Keys.Count, Is.EqualTo(1));
            Assert.That(coll.ContainsKey("example"), Is.True);

            FeatureCollection fc = coll["example"];
            Assert.That(fc, Is.Not.Null);
            Assert.That(fc.Count, Is.EqualTo(3));

            IFeature f1 = fc[0];
            Assert.That(f1, Is.Not.Null);
            Assert.That(f1.Geometry, Is.Not.Null);
            Assert.That(f1.Geometry, Is.InstanceOf<IMultiPoint>());
            Assert.That(f1.Geometry.AsText(), Is.EqualTo("MULTIPOINT ((102 0.5))"));
            Assert.That(f1.Attributes, Is.Not.Null);
            Assert.That(f1.Attributes.Count, Is.EqualTo(0));

            IFeature f2 = fc[1];
            Assert.That(f2, Is.Not.Null);
            Assert.That(f2.Geometry, Is.Not.Null);
            Assert.That(f2.Geometry, Is.InstanceOf<IMultiLineString>());
            Assert.That(f2.Geometry.AsText(), Is.EqualTo("MULTILINESTRING ((102 0, 103 1, 104 0, 105 1))"));
            Assert.That(f2.Attributes, Is.Not.Null);
            Assert.That(f2.Attributes.Count, Is.EqualTo(0));

            IFeature f3 = fc[2];
            Assert.That(f3, Is.Not.Null);
            Assert.That(f3.Geometry, Is.Not.Null);
            Assert.That(f3.Geometry.Factory, Is.EqualTo(Factory));
            Assert.That(f3.Geometry, Is.InstanceOf<IMultiPolygon>());
            Assert.That(f3.Geometry.AsText(), Is.EqualTo("MULTIPOLYGON (((100 0, 100 1, 101 1, 101 0, 100 0)))"));
            Assert.That(f3.Attributes, Is.Not.Null);
            Assert.That(f3.Attributes.Count, Is.EqualTo(0));
        }

        [Test]
        public void read_aruba_data()
        {
            const string data = TopoData.Aruba;
            TopoJsonReader reader = new TopoJsonReader(Factory);
            IDictionary<string, FeatureCollection> coll = reader.
                Read<IDictionary<string, FeatureCollection>>(data);
            FeatureCollection fc = coll["aruba"];
            Assert.That(fc.Count, Is.EqualTo(1));
            // test here: http://jsfiddle.net/D_Guidi/3atZU/embedded/result/
            Console.WriteLine(fc[0].Geometry);
        }

        [Test]
        public void read_airports_data()
        {
            const string data = TopoData.Airports;
            TopoJsonReader reader = new TopoJsonReader(Factory);
            IDictionary<string, FeatureCollection> coll = reader.
                Read<IDictionary<string, FeatureCollection>>(data);
            FeatureCollection fc = coll["airports"];
            Assert.That(fc.Count, Is.EqualTo(1));
            // test here: http://jsfiddle.net/D_Guidi/3atZU/embedded/result/
            Console.WriteLine(fc[0].Geometry);
        }

        [Test]
        public void read_counties_data()
        {
            const string data = TopoData.Counties;
            TopoJsonReader reader = new TopoJsonReader(Factory);
            IDictionary<string, FeatureCollection> coll = reader.
                Read<IDictionary<string, FeatureCollection>>(data);
            FeatureCollection fc = coll["counties"];
            Assert.That(fc.Count, Is.EqualTo(2));
            // test here: http://jsfiddle.net/D_Guidi/3atZU/embedded/result/
            Console.WriteLine(fc[0].Geometry);
            Console.WriteLine(fc[1].Geometry);
        }
    }
}
