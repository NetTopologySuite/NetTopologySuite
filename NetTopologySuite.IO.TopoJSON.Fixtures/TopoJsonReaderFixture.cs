using System;
using GeoAPI.Geometries;
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
        private static readonly dynamic TopoDef = new
        {
            type = "",
            transform = new
            {
                scale = new double[] { },
                translate = new double[] { }
            },
            objects = new
            {
                aruba = new
                        {
                            type = "",
                            arcs = new int[0][],
                            id = ""
                        }
            },
            arcs = new int[0][][]
        };

        // topoJson sample from https://github.com/mbostock/topojson/wiki/Introduction
        private const string TopoSample = @"
{
  ""type"": ""Topology"",
  ""transform"": {
    ""scale"": [0.036003600360036005, 0.017361589674592462],
    ""translate"": [-180, -89.99892578124998]
  },
  ""objects"": {
    ""aruba"": {
      ""type"": ""Polygon"",
      ""arcs"": [[0]],
      ""id"": 533
    }
  },
  ""arcs"": [
    [[3058, 5901], [0, -2], [-2, 1], [-1, 3], [-2, 3], [0, 3], [1, 1], [1, -3], [2, -5], [1, -1]]
  ]
}
";

        private static dynamic TryCreateReferenceObject()
        {
            dynamic obj = JsonConvert.DeserializeAnonymousType(TopoSample, TopoDef);
            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.type, Is.Not.Null);
            Assert.That(obj.type, Is.EqualTo("Topology"));
            Assert.That(obj.transform, Is.Not.Null);
            Assert.That(obj.transform.scale, Is.Not.Null);
            Assert.That(obj.transform.scale.Length, Is.EqualTo(2));
            Assert.That(obj.transform.translate, Is.Not.Null);
            Assert.That(obj.transform.translate.Length, Is.EqualTo(2));
            Assert.That(obj.objects, Is.Not.Null);
            Assert.That(obj.objects.aruba, Is.Not.Null);
            Assert.That(obj.objects.aruba.type, Is.Not.Null);
            Assert.That(obj.objects.aruba.type, Is.EqualTo("Polygon"));
            Assert.That(obj.objects.aruba.arcs, Is.Not.Null);
            Assert.That(obj.objects.aruba.arcs.Length, Is.EqualTo(1));
            Assert.That(obj.objects.aruba.id, Is.Not.Null);
            Assert.That(obj.objects.aruba.id, Is.EqualTo("533"));
            Assert.That(obj.arcs, Is.Not.Null);
            Assert.That(obj.arcs.Length, Is.EqualTo(1));
            Assert.That(obj.arcs[0].Length, Is.EqualTo(10));
            return obj;
        }

        [Test]
        public void try_read_object()
        {
            dynamic refobj = TryCreateReferenceObject();
            Assert.That(refobj, Is.Not.Null);

            Transform transform = new Transform(refobj.transform.scale, refobj.transform.translate);
            Transformer transformer = new Transformer(transform, refobj.arcs);

            dynamic aruba = refobj.objects.aruba;
            IGeometry geom = transformer.Create(aruba.type, aruba.arcs);
            Assert.That(geom, Is.Not.Null);            
            Assert.That(geom, Is.InstanceOf<IPolygon>());
            Assert.That(geom.IsValid, Is.True);
            Console.WriteLine(geom);
        }
    }
}
