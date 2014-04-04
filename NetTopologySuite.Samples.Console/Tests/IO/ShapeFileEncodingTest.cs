using System.Collections.Generic;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.IO
{
    [TestFixture]
    public class ShapeFileEncodingTest
    {
        [SetUp]
        public void Setup()
        {
            ShapefileDataWriter sfdr = new ShapefileDataWriter("encoding_sample");
            DbaseFileHeader h = new DbaseFileHeader();
            h.AddColumn("id", 'n', 8, 0);
            h.AddColumn("Test", 'C', 15, 0);
            h.AddColumn("Ålder", 'N', 8, 0);
            h.AddColumn("Ödestext", 'C', 255, 0);
            h.NumRecords = 1;
            sfdr.Header = h;

            List<IFeature> feats = new List<IFeature>();
            AttributesTable at = new AttributesTable();
            at.AddAttribute("id", "0");
            at.AddAttribute("Test", "Testar");
            at.AddAttribute("Ålder", 10);
            at.AddAttribute("Ödestext", "Lång text med åäö etc");
            feats.Add(new Feature(new Point(0, 0), at));
            sfdr.Write(feats);      
        }

        [Test]
        public void TestLoadShapeFileWithEncoding()
        {
            ShapefileDataReader reader = new ShapefileDataReader("encoding_sample.shp", GeometryFactory.Default);
            DbaseFileHeader header = reader.DbaseHeader;
            Assert.AreEqual(header.Encoding.WindowsCodePage, 1252, "Invalid encoding!");

            Assert.AreEqual(header.Fields[1].Name, "Test");
            Assert.AreEqual(header.Fields[2].Name, "Ålder");
            Assert.AreEqual(header.Fields[3].Name, "Ödestext");

            Assert.IsTrue(reader.Read(), "Error reading file");
            Assert.AreEqual(reader["Test"], "Testar");
            Assert.AreEqual(reader["Ödestext"], "Lång text med åäö etc");
        }
    }
}
