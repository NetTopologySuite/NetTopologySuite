using System;
using System.IO;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Operation.Overlay.Snap;
using NetTopologySuite.Operation.Union;
using NetTopologySuite.Samples.SimpleTests;
using NUnit.Framework;

namespace  NetTopologySuite.Samples.Tests.Various
{
    [TestFixture]
    public class CascadedPolygonUnionFixture : BaseSamples
    {
        public CascadedPolygonUnionFixture()
        {
            // Set current dir to shapefiles dir
            string format = String.Format("..{0}..{0}..{0}NetTopologySuite.Samples.Shapefiles", Path.DirectorySeparatorChar);
            Environment.CurrentDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, format);
        }

        [Test]
        public void PerformCascadedPolygonUnion()
        {
            var reader = new ShapefileReader("tnp_pol.shp");
            var collection = reader.ReadAll().Where(e => e is IPolygon).ToList();
            var u1 = collection[0];
            for (var i = 1; i < collection.Count; i++)
                u1 = SnapIfNeededOverlayOp.Overlay(u1, collection[i], SpatialFunction.Union);
            var u2 = CascadedPolygonUnion.Union(collection);
            if (!u1.Equals(u2))
                Assert.Fail("failure");
        }    
    }
}
