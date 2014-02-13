using System;
using System.IO;
using System.Xml;
using NetTopologySuite.Tests.NUnit.TestData;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Various
{
    [TestFixture]
    public class Issue103Tests
    {
        [Test, Ignore("Handled in seperate tests")]
        public void RunAllTests()
        {
            ReadAndTest("read-ok-file.gml");
            ReadAndTest("cant-read-file1.gml");
            ReadAndTest("cant-read-file2.gml");
            ReadAndTest("cant-read-file3.gml");

            //Surface / PolygonPatch not supported
            //ReadAndTest("cant-read-file4.gml");
        }

        [Test, Category("Issue103")]
        public void TestFileOk()
        {
            ReadAndTest("read-ok-file.gml");
        }

        [Test, Category("Issue103")]
        public void TestFile1()
        {
            ReadAndTest("cant-read-file1.gml");
        }

        [Test, Category("Issue103")]
        public void TestFile2()
        {
            ReadAndTest("cant-read-file2.gml");
        }

        [Test, Category("Issue103")]
        public void TestFile3()
        {
            ReadAndTest("cant-read-file3.gml");
        }

        [Test, Category("Issue103"), Ignore("Surface / PolygonPatch not supported")]
        public void TestFile4()
        {
            ReadAndTest("cant-read-file4.gml");
        }

        private static void ReadAndTest(string file)
        {
            Console.WriteLine(file);
            var path = EmbeddedResourceManager.SaveEmbeddedResourceToTempFile("NetTopologySuite.Samples.Tests.Various." + file);
            var gml = File.ReadAllText(path);

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(gml);
            var gmlNode = xmlDoc.DocumentElement.FirstChild.NextSibling.FirstChild.LastChild.InnerXml;
            Console.WriteLine(gmlNode);

            var gmlReader = new NetTopologySuite.IO.GML2.GMLReader();
            var geom = gmlReader.Read(gmlNode);

            Assert.IsNotNull(geom);
            Assert.IsFalse(geom.IsEmpty);

            Console.WriteLine(geom.ToString());
            Console.WriteLine(new string('=', 60));

            EmbeddedResourceManager.CleanUpTempFile(path);
        }
    }
}