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
            XmlDocument doc = new XmlDocument();
            Point geom = new Point(52, -0.9);
            doc.Load(geom.ToGMLFeature());

            string content = doc.OuterXml;
            Assert.That(content, Is.Not.Null);
            Assert.That(content.StartsWith("<gml:Point xmlns:gml=\"http://www.opengis.net/gml\""), Is.True);

            GMLReader reader = new GMLReader();
            IGeometry actual = reader.Read(content);
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.EqualsExact(geom), Is.True);
        }

        [Test, Category("Issue47")]
        public void gml_writer_generates_fragment_without_namespace_if_already_declared()
        {
            string testFragment = GenerateTestFragment();
            Assert.That(testFragment, Is.Not.Null);
            Assert.That(testFragment.StartsWith("<test xmlns:gml=\"http://www.opengis.net/gml\""), Is.True);
            Assert.That(testFragment.Contains("<gml:Point"), Is.True);
            Assert.That(testFragment.Contains("<gml:Point xmlns:gml=\"http://www.opengis.net/gml\""), Is.False);
        }

        private static string GenerateTestFragment()
        {
            StringBuilder sb = new StringBuilder();
            XmlTextWriter writer = new XmlTextWriter(new StringWriter(sb));
            writer.WriteStartElement("test");
            writer.WriteAttributeString("xmlns", "gml", null, "http://www.opengis.net/gml");

            Point geom = new Point(52, -0.9);
            GMLWriter gmlWriter = new GMLWriter();
            gmlWriter.Write(geom, writer);

            writer.WriteEndElement();
            return sb.ToString();
        }

        [Test, Category("Issue47")]
        public void gml_reader_can_read_ToGMLFeature()
        {
            GMLReader reader = new GMLReader();
            Point geom = new Point(52, -0.9);
            IGeometry actual = reader.Read(geom.ToGMLFeature());
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.EqualsExact(geom), Is.True);
        }

        [Test, Category("Issue47")]
        public void gml_reader_can_read_gml_fragment()
        {
            GMLReader reader = new GMLReader();
            string testFragment = GenerateTestFragment();
            IGeometry actual = reader.Read(testFragment);
            Assert.That(actual, Is.Not.Null);
            Point geom = new Point(52, -0.9);
            Assert.That(actual.EqualsExact(geom), Is.True);
        }
    }
}