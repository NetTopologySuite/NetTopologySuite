using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using GeoAPI.Geometries;
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
        [Ignore("File aaa.shp not present")]
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
            var factory = GeometryFactory.Default;
            var polys = new List<IPolygon>();
            const int distance = 500;
            using (var reader = new ShapefileDataReader("afvalbakken", factory))
            {
                var index = 0;
                while (reader.Read())
                {                    
                    var geom = reader.Geometry;
                    Assert.IsNotNull(geom);
                    Assert.IsTrue(geom.IsValid);
                    Debug.WriteLine(String.Format("Geom {0}: {1}", index++, geom));
                    
                    var buff = geom.Buffer(distance);
                    Assert.IsNotNull(buff);

                    polys.Add((IPolygon) geom);
                }
            }

            var multiPolygon = factory.CreateMultiPolygon(polys.ToArray());
            Assert.IsNotNull(multiPolygon);
            Assert.IsTrue(multiPolygon.IsValid);

            var multiBuffer = (IMultiPolygon) multiPolygon.Buffer(distance);
            Assert.IsNotNull(multiBuffer);            
            Assert.IsTrue(multiBuffer.IsValid);

            var writer = new ShapefileWriter(factory); 
            writer.Write(@"test_buffer", multiBuffer); 
            ShapefileWriter.WriteDummyDbf(@"test_buffer.dbf", multiBuffer.NumGeometries);        
        }
    }
}
