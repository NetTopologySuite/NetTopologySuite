using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using NUnit.Framework;

namespace NetTopologySuite.IO.TopoJSON.Fixtures
{
    /// <summary>
    /// inspiration from https://github.com/calvinmetcalf/topojson.py
    /// </summary>
    [TestFixture]
    public class TopoJsonReaderFixture
    {
        private static readonly IGeometryFactory Factory = GeometryFactory.Fixed;

        [Test]
        public void read_reference_data()
        {
            dynamic obj = JsonConvert.DeserializeAnonymousType(TopoData.ReferenceData, TopoData.ReferenceDef);
            Assert.That(obj, Is.Not.Null);

            ITransformer transformer = new Transformer(obj.arcs, Factory);

            dynamic data = obj.objects.example;
            Assert.That(data, Is.Not.Null);
            IGeometry geom = transformer.Create(data);
            Assert.That(geom, Is.Not.Null);
            Assert.That(geom, Is.InstanceOf<IGeometryCollection>());

            IGeometryCollection coll = (IGeometryCollection)geom;
            const string expected = "GEOMETRYCOLLECTION (POINT (102 0.5), LINESTRING (102 0, 103 1, 104 0, 105 1), POLYGON ((100 0, 100 1, 101 1, 101 0, 100 0)))";
            Assert.That(coll.ToString(), Is.EqualTo(expected));

            IGeometry g0 = coll.GetGeometryN(0);
            Assert.That(g0, Is.Not.Null);
            Assert.That(g0, Is.InstanceOf<IPoint>());

            IGeometry g1 = coll.GetGeometryN(1);
            Assert.That(g1, Is.Not.Null);
            Assert.That(g1, Is.InstanceOf<ILineString>());

            IGeometry g2 = coll.GetGeometryN(2);
            Assert.That(g2, Is.Not.Null);
            Assert.That(g2, Is.InstanceOf<IPolygon>());
        }

        [Test]
        public void read_multi_reference_data()
        {
            dynamic obj = JsonConvert.DeserializeAnonymousType(TopoData.MultiReferenceData, TopoData.MultiReferenceDef);
            Assert.That(obj, Is.Not.Null);

            ITransformer transformer = new Transformer(obj.arcs, Factory);

            dynamic data = obj.objects.example;
            Assert.That(data, Is.Not.Null);
            IGeometry geom = transformer.Create(data);
            Assert.That(geom, Is.Not.Null);
            Assert.That(geom, Is.InstanceOf<IGeometryCollection>());

            IGeometryCollection coll = (IGeometryCollection)geom;
            const string expected = "GEOMETRYCOLLECTION (MULTIPOINT ((102 0.5)), MULTILINESTRING ((102 0, 103 1, 104 0, 105 1)), MULTIPOLYGON (((100 0, 100 1, 101 1, 101 0, 100 0))))";
            Assert.That(coll.ToString(), Is.EqualTo(expected));

            IGeometry g0 = coll.GetGeometryN(0);
            Assert.That(g0, Is.Not.Null);
            Assert.That(g0, Is.InstanceOf<IMultiPoint>());

            IGeometry g1 = coll.GetGeometryN(1);
            Assert.That(g1, Is.Not.Null);
            Assert.That(g1, Is.InstanceOf<IMultiLineString>());

            IGeometry g2 = coll.GetGeometryN(2);
            Assert.That(g2, Is.Not.Null);
            Assert.That(g2, Is.InstanceOf<IMultiPolygon>());
        }

        [Test]
        public void read_quantized_data()
        {
            dynamic obj = JsonConvert.DeserializeAnonymousType(TopoData.QuantizedData, TopoData.QuantizedDef);
            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.transform, Is.Not.Null);
            Assert.That(obj.transform.scale, Is.Not.Null);
            Assert.That(obj.transform.translate, Is.Not.Null);

            ITransform transform = new Transform(obj.transform.scale, obj.transform.translate);
            ITransformer transformer = new Transformer(transform, obj.arcs, Factory);

            dynamic data = obj.objects.example;
            Assert.That(data, Is.Not.Null);
            IGeometry geom = transformer.Create(data);
            Assert.That(geom, Is.Not.Null);
            Assert.That(geom, Is.InstanceOf<IGeometryCollection>());

            IGeometryCollection coll = (IGeometryCollection)geom;
            const string expected = "GEOMETRYCOLLECTION (POINT (102 0.5), LINESTRING (102 0, 103 1, 104 0, 105 1), POLYGON ((100 0, 100 1, 101 1, 101 0, 100 0)))";
            Assert.That(coll.ToString(), Is.EqualTo(expected));

            IGeometry g0 = coll.GetGeometryN(0);
            Assert.That(g0, Is.Not.Null);
            Assert.That(g0, Is.InstanceOf<IPoint>());

            IGeometry g1 = coll.GetGeometryN(1);
            Assert.That(g1, Is.Not.Null);
            Assert.That(g1, Is.InstanceOf<ILineString>());

            IGeometry g2 = coll.GetGeometryN(2);
            Assert.That(g2, Is.Not.Null);
            Assert.That(g2, Is.InstanceOf<IPolygon>());
        }

        [Test]
        public void read_multi_quantized_data()
        {
            dynamic obj = JsonConvert.DeserializeAnonymousType(TopoData.MultiQuantizedData, TopoData.MultiQuantizedDef);
            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.transform, Is.Not.Null);
            Assert.That(obj.transform.scale, Is.Not.Null);
            Assert.That(obj.transform.translate, Is.Not.Null);

            ITransform transform = new Transform(obj.transform.scale, obj.transform.translate);
            ITransformer transformer = new Transformer(transform, obj.arcs, Factory);

            dynamic data = obj.objects.example;
            Assert.That(data, Is.Not.Null);
            IGeometry geom = transformer.Create(data);
            Assert.That(geom, Is.Not.Null);
            Assert.That(geom, Is.InstanceOf<IGeometryCollection>());

            IGeometryCollection coll = (IGeometryCollection)geom;
            const string expected = "GEOMETRYCOLLECTION (MULTIPOINT ((102 0.5)), MULTILINESTRING ((102 0, 103 1, 104 0, 105 1)), MULTIPOLYGON (((100 0, 100 1, 101 1, 101 0, 100 0))))";
            Assert.That(coll.ToString(), Is.EqualTo(expected));

            IGeometry g0 = coll.GetGeometryN(0);
            Assert.That(g0, Is.Not.Null);
            Assert.That(g0, Is.InstanceOf<IMultiPoint>());

            IGeometry g1 = coll.GetGeometryN(1);
            Assert.That(g1, Is.Not.Null);
            Assert.That(g1, Is.InstanceOf<IMultiLineString>());

            IGeometry g2 = coll.GetGeometryN(2);
            Assert.That(g2, Is.Not.Null);
            Assert.That(g2, Is.InstanceOf<IMultiPolygon>());
        }

        [Test]
        public void read_aruba_polygon()
        {
            dynamic obj = JsonConvert.DeserializeAnonymousType(TopoData.ArubaData, TopoData.ArubaDef);
            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.transform, Is.Not.Null);
            Assert.That(obj.transform.scale, Is.Not.Null);
            Assert.That(obj.transform.translate, Is.Not.Null);

            ITransform transform = new Transform(obj.transform.scale, obj.transform.translate);
            ITransformer transformer = new Transformer(transform, obj.arcs, Factory);

            dynamic data = obj.objects.aruba;
            Assert.That(data, Is.Not.Null);
            IGeometry geom = transformer.Create(data);
            Assert.That(geom, Is.Not.Null);
            Assert.That(geom, Is.InstanceOf<IPolygon>());
            Assert.That(geom.IsValid, Is.True);
            // test here: http://www.openlayers.org/dev/examples/vector-formats.html
            Console.WriteLine(geom);
        }

        [Test]
        public void read_airports_moltipoint()
        {
            dynamic obj = JsonConvert.DeserializeAnonymousType(TopoData.AirportsData, TopoData.AirportsDef);
            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.transform, Is.Not.Null);
            Assert.That(obj.transform.scale, Is.Not.Null);
            Assert.That(obj.transform.translate, Is.Not.Null);

            ITransform transform = new Transform(obj.transform.scale, obj.transform.translate);
            ITransformer transformer = new Transformer(transform, obj.arcs, Factory);

            dynamic data = obj.objects.airports;
            Assert.That(data, Is.Not.Null);
            IGeometry geom = transformer.Create(data);
            Assert.That(geom, Is.Not.Null);
            Assert.That(geom, Is.InstanceOf<IMultiPoint>());
            // test here: http://www.openlayers.org/dev/examples/vector-formats.html
            Console.WriteLine(geom);
            Assert.That(geom.NumGeometries, Is.EqualTo(data.coordinates.Length));
        }

        [Test]
        public void read_counties_moltipolygon()
        {
            dynamic obj = JsonConvert.DeserializeAnonymousType(TopoData.CountiesData, TopoData.CountiesDef);
            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.transform, Is.Not.Null);
            Assert.That(obj.transform.scale, Is.Not.Null);
            Assert.That(obj.transform.translate, Is.Not.Null);

            ITransform transform = new Transform(obj.transform.scale, obj.transform.translate);
            ITransformer transformer = new Transformer(transform, obj.arcs, Factory);

            dynamic data = obj.objects.counties;
            Assert.That(data, Is.Not.Null);
            IGeometry geom = transformer.Create(data);
            Assert.That(geom, Is.Not.Null);
            Assert.That(geom, Is.InstanceOf<IGeometryCollection>());
            // test here: http://www.openlayers.org/dev/examples/vector-formats.html
            Console.WriteLine(geom);
            Assert.That(geom.NumGeometries, Is.EqualTo(data.geometries.Length));
        }
    }
}
