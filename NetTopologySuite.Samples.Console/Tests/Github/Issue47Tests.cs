using System.IO;
using System.Text;
using System.Xml;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.GML2;
using NUnit.Framework;
namespace NetTopologySuite.Samples.Tests.Github
{
    public class Issue47Tests
    {
        [Test, Category("Issue47")]
        public void gml_writer_generates_fragment_with_namespace_if_needed()
        {
            var doc = new XmlDocument();
            var geom = new Point(52, -0.9);
            doc.Load(geom.ToGMLFeature());
            var content = doc.OuterXml;
            Assert.That(content, Is.Not.Null);
            Assert.That(content.StartsWith("<gml:Point xmlns:gml=\"http://www.opengis.net/gml\""), Is.True);
            var reader = new GMLReader();
            var actual = reader.Read(content);
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.EqualsExact(geom), Is.True);
        }
        [Test, Category("Issue47")]
        public void gml_writer_generates_fragment_without_namespace_if_already_declared()
        {
            var testFragment = GenerateTestFragment();
            Assert.That(testFragment, Is.Not.Null);
            Assert.That(testFragment.StartsWith("<test xmlns:gml=\"http://www.opengis.net/gml\""), Is.True);
            Assert.That(testFragment.Contains("<gml:Point"), Is.True);
            Assert.That(testFragment.Contains("<gml:Point xmlns:gml=\"http://www.opengis.net/gml\""), Is.False);
        }
        private static string GenerateTestFragment()
        {
            var sb = new StringBuilder();
            var writer = new XmlTextWriter(new StringWriter(sb));
            writer.WriteStartElement("test");
            writer.WriteAttributeString("xmlns", "gml", null, "http://www.opengis.net/gml");
            var geom = new Point(52, -0.9);
            var gmlWriter = new GMLWriter();
            gmlWriter.Write(geom, writer);
            writer.WriteEndElement();
            return sb.ToString();
        }
        [Test, Category("Issue47")]
        public void gml_reader_can_read_ToGMLFeature()
        {
            var reader = new GMLReader();
            var geom = new Point(52, -0.9);
            var actual = reader.Read(geom.ToGMLFeature());
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.EqualsExact(geom), Is.True);
        }
        [Test, Category("Issue47")]
        public void gml_reader_can_read_gml_fragment()
        {
            var reader = new GMLReader();
            var testFragment = GenerateTestFragment();
            var actual = reader.Read(testFragment);
            Assert.That(actual, Is.Not.Null);
            var geom = new Point(52, -0.9);
            Assert.That(actual.EqualsExact(geom), Is.True);
        }
    }
}
