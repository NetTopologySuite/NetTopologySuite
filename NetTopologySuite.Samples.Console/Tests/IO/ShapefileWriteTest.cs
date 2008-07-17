using System;
using System.IO;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using GisSharpBlog.NetTopologySuite.Samples.SimpleTests;
using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Operation.IO
{

    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class ShapeFileDataWriterTest : BaseSamples
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeFileDataWriterTest"/> class.
        /// </summary>
        public ShapeFileDataWriterTest() : base()
        {
            // Set current dir to shapefiles dir
			Environment.CurrentDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../NetTopologySuite.Samples.Shapefiles");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void TestWriteSimpleShapeFile()
        {
            IPoint p1 = Factory.CreatePoint(new Coordinate(100, 100));
            IPoint p2 = Factory.CreatePoint(new Coordinate(200, 200));

            GeometryCollection coll = new GeometryCollection(new IGeometry[] { p1, p2, });
            ShapefileWriter writer = new ShapefileWriter(Factory);
            writer.Write(@"c:\test_arcview", coll);

            ShapefileWriter.WriteDummyDbf(@"c:\test_arcview.dbf", 2);
            
            // Not read by ArcView!!!
        }
    }
}
