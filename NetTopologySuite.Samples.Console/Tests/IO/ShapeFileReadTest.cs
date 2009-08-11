using System;
using System.Diagnostics;
using System.IO;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using GisSharpBlog.NetTopologySuite.Samples.SimpleTests;
using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Operation.IO
{
    [TestFixture]
    public class ShapeFileDataReaderTest : BaseSamples
    {
        public ShapeFileDataReaderTest()
        {
            // Set current dir to shapefiles dir
            Environment.CurrentDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../NetTopologySuite.Samples.Shapefiles");
        }

        [Test]
        public void TestReadingCrustalTestShapeFile()
        {
            // Original file with characters '°' in NAME field.
            using (var reader = new ShapefileDataReader("crustal_test_bugged", Factory))
            {
                var length = reader.DbaseHeader.NumFields;
                while (reader.Read())
                {
                    Debug.WriteLine(reader.GetValue(length - 1));
                }
            }

            // Removed NAME field characters
            using (var reader = new ShapefileDataReader("crustal_test", Factory))
            {
                var length = reader.DbaseHeader.NumFields;
                while (reader.Read())
                {
                    Debug.WriteLine(reader.GetValue(length - 1));
                }
            }
        }

        [Test]
        public void TestReadingAaaShapeFile()
        {
            using (var reader = new ShapefileDataReader("aaa", Factory))
            {
                var length = reader.DbaseHeader.NumFields;
                while (reader.Read())
                {
                    Debug.WriteLine(reader.GetValue(length - 1));
                }
            }
            Assert.Fail();
        }

        [Test]
        public void TestReadingShapeFileWithNulls()
        {
            using (var reader = new ShapefileDataReader("AllNulls", Factory))
            {
                while (reader.Read())
                {
                    var geom = reader.Geometry;
                    Assert.IsNotNull(geom);

                    var values = new object[5];
                    var result = reader.GetValues(values);
                    Assert.IsNotNull(values);
                }
            }
        }

        [Test]
        public void TestReadingShapeFileAfvalbakken()
        {
            using (var reader = new ShapefileDataReader("afvalbakken", GeometryFactory.Default))
            {
                var index = 0;
                while (reader.Read())
                {                    
                    var geom = reader.Geometry;
                    Assert.IsNotNull(geom);
                    Debug.WriteLine(String.Format("Geom {0}: {1}", index++, geom));

                    Assert.IsTrue(geom.IsValid);
                    var buff = geom.Buffer(10);
                    Assert.IsNotNull(buff);
                }
            }
        }
    }
}
