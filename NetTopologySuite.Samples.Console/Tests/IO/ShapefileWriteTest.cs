using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

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
        /// Initializes a new instance of the <see cref="ShapeFileReadTest"/> class.
        /// </summary>
        public ShapeFileDataWriterTest() : base()
        {
            // Set current dir to shapefiles dir
            Environment.CurrentDirectory = @"../../../NetTopologySuite.Samples.Shapefiles";
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void TestWriteSimpleShapeFile()
        {
            Point p1 = Factory.CreatePoint(new Coordinate(100, 100));
            Point p2 = Factory.CreatePoint(new Coordinate(200, 200));

            GeometryCollection coll = new GeometryCollection(new Geometry[] { p1, p2, });
            ShapefileWriter writer = new ShapefileWriter(Factory);
            writer.Write(@"c:\test_arcview", coll);

            ShapefileWriter.WriteDummyDbf(@"c:\test_arcview.dbf", 2);
            
            // Not read by ArcView!!!
        }
    }
}
