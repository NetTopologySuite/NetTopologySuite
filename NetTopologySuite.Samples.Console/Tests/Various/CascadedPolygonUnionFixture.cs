using System;
using System.IO;
using System.Linq;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using GisSharpBlog.NetTopologySuite.Operation.Overlay;
using GisSharpBlog.NetTopologySuite.Operation.Overlay.Snap;
using GisSharpBlog.NetTopologySuite.Operation.Union;
using GisSharpBlog.NetTopologySuite.Samples.SimpleTests;
using NUnit.Framework;

namespace  GisSharpBlog.NetTopologySuite.Samples.Tests.Various
{
    [TestFixture]
    public class CascadedPolygonUnionFixture : BaseSamples
    {
        public CascadedPolygonUnionFixture()
        {
            // Set current dir to shapefiles dir
			Environment.CurrentDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../NetTopologySuite.Samples.Shapefiles");
        }
        [Test]
        public void PErformCascadedPolygonUnion()
        {
            var reader = new ShapefileReader("tnp_pol.shp");
            var collection = reader.ReadAll().Cast<IGeometry>().Where(e => e is IPolygon).ToList();
            var u1 = collection[0];
            for (var i = 1; i < collection.Count; i++)
                u1 = SnapIfNeededOverlayOp.Overlay(u1, collection[i], SpatialFunction.Union);
            var u2 = CascadedPolygonUnion.Union(collection);
            if (!u1.Equals(u2))
                Assert.Fail("failure");
        }    
    }
}
