using System;
using System.Collections;
using System.Text;

using GisSharpBlog.NetTopologySuite.CoordinateTransformations;
using GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;

namespace GisSharpBlog.NetTopologySuite.Samples.SimpleTests.Coordinates
{
    public class CoordinatesTest : BaseSamples
    {
        public CoordinatesTest() : base(new GeometryFactory(), new WKTReader())
        {
        }

        public override void Start()
        {            
        }
    }
}
