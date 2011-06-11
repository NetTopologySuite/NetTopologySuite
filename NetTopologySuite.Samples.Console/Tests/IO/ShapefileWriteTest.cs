using System;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Samples.SimpleTests;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Operation.IO
{
[TestFixture]
    public class ShapeFileDataWriterTest : BaseSamples
    {
        public ShapeFileDataWriterTest()
        {
            // Set current dir to shapefiles dir
			Environment.CurrentDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format("..{0}..{0}..{0}NetTopologySuite.Samples.Shapefiles", Path.DirectorySeparatorChar));
        }

        [Test]
        public void TestWriteSimpleShapeFile()
        {
            var p1 = Factory.CreatePoint(new Coordinate(100, 100));
            var p2 = Factory.CreatePoint(new Coordinate(200, 200));

            var coll = new GeometryCollection(new IGeometry[] { p1, p2, });
            var writer = new ShapefileWriter(Factory);
            writer.Write(@"test_arcview", coll);

            ShapefileWriter.WriteDummyDbf(@"test_arcview.dbf", 2);
            
            // Not read by ArcView!!!
        }
    }
}
