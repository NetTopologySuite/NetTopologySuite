using System;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.Various
{
    [TestFixture]
    public class Issue178Fixture
    {
        [SetUp]
        public void SetUp()
        {
            // Set current dir to shapefiles dir
            Environment.CurrentDirectory =
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    String.Format("..{0}..{0}..{0}NetTopologySuite.Samples.Shapefiles", Path.DirectorySeparatorChar));
        }

        [Test]
        public void TestCorruptedShapeFile()
        {
            IGeometryFactory factory = GeometryFactory.Default;
            const string filename = "christchurch-canterbury-h.shp";
            Assert.Throws<ShapefileException>(() =>
            {
                var reader = new ShapefileReader(filename, factory);
                Assert.Fail("Invalid file: code should be unreachable");
            });

            // ensure file isn't locked
            string path = Path.Combine(Environment.CurrentDirectory, filename);
            bool ok;
            using (FileStream file = File.OpenRead(path))
            {
                using (BinaryReader reader = new BinaryReader(file))
                {
                    // read a value
                    int val = reader.Read();                    
                    Console.WriteLine("read a value: " + val);
                    ok = true;
                }
            }
            Assert.That(ok, Is.True);
        }
    }
}