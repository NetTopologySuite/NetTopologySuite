using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.OverlayNG;
using NetTopologySuite.Operation.Union;
using NUnit.Framework;
using System.Collections.Generic;

namespace NetTopologySuite.Samples.Tests.Github
{
    [TestFixture, Explicit]
    internal class Discussions713Fixture
    {
        [Test, Description("CascadedPolygonUnion with artifacts")]
        public void CascadedPolygonUnionTest()
        {
            var polygon1 = new Polygon(new LinearRing(new[] { new Coordinate(390.57699736961104, 4661.279737082885)
                , new Coordinate(102.37388755252186, 4816.95044382157)
                , new Coordinate(58.74581767428481, 4283.623092809056)
                , new Coordinate(348.48506699658174, 4165.451803054288)
                , new Coordinate(390.57699736961104, 4661.279737082885) }));
            var polygon2 = new Polygon(new LinearRing(new[] { new Coordinate(348.48506699658174, 4165.451803054288)
                , new Coordinate(58.74581767428481, 4283.623092809056)
                , new Coordinate(13.500614179719832, 3749.613111892697)
                , new Coordinate(305.66170073770854, 3670.294968413584)
                , new Coordinate(348.48506699658174, 4165.451803054288) }));
            var polygon3 = new Polygon(new LinearRing(new[] { new Coordinate(58.74581767428481, 4283.623092809056)
                , new Coordinate(0, 4307.585675931142)
                , new Coordinate(0, 3753.278564301302)
                , new Coordinate(13.500614179719832, 3749.613111892697)
                , new Coordinate(58.74581767428481, 4283.623092809056) }));
            var polygon4 = new Polygon(new LinearRing(new[] { new Coordinate(102.37388755252186, 4816.95044382157)
                , new Coordinate(0, 4872.266023739649)
                , new Coordinate(0, 4307.585675931142)
                , new Coordinate(58.74581767428481, 4283.623092809056)
                , new Coordinate(102.37388755252186, 4816.95044382157) }));
            var polygon5 = new Polygon(new LinearRing(new[] { new Coordinate(322.6200851841231, 4797.743180617399)
                , new Coordinate(4.3409324404210565, 4979.065456550775)
                , new Coordinate(0, 4924.659401717726)
                , new Coordinate(0, 4373.8282404906295)
                , new Coordinate(277.28392054659435, 4256.952250532387)
                , new Coordinate(322.6200851841231, 4797.743180617399) }));
            var polygon6 = new Polygon(new LinearRing(new[] { new Coordinate(592.1335978830111, 4644.236124014847)
                , new Coordinate(322.6200851841231, 4797.743180617399)
                , new Coordinate(277.28392054659435, 4256.952250532387)
                , new Coordinate(548.9114813758665, 4142.475123268172)
                , new Coordinate(592.1335978830111, 4644.236124014847) }));
            var polygon7 = new Polygon(new LinearRing(new[] { new Coordinate(277.28392054659435, 4256.952250532387)
                , new Coordinate(0, 4373.8282404906295)
                , new Coordinate(0, 3781.1329663187094)
                , new Coordinate(230.85561202727877, 3717.1774865826924)
                , new Coordinate(277.28392054659435, 4256.952250532387) }));
            var polygon8 = new Polygon(new LinearRing(new[] { new Coordinate(548.9114813758665, 4142.475123268172),
                new Coordinate(277.28392054659435,4256.952250532387),
                new Coordinate(230.85561202727877,3717.1774865826924),
                new Coordinate(505.1726340456544,3641.185531926155),
                new Coordinate(548.9114813758665, 4142.475123268172) }));

            var polygons = new Geometry[] {
                polygon1, polygon2, polygon3, polygon4,
                polygon5, polygon6, polygon7, polygon8 };
            var mp = polygon1.Factory.BuildGeometry(polygons);
            TestContext.WriteLine(mp.ToString());

            var result = CascadedPolygonUnion.Union(polygons);
            TestContext.WriteLine(result.ToString());

            var res2 = mp.Union();
            TestContext.WriteLine(res2.ToString());

            var fixedResultParts = new List<Geometry>();
            for (int i = 0; i < result.NumGeometries; i++)
            {
                var poly = (Polygon)result.GetGeometryN(i);
                var holes = new List<LinearRing>(poly.NumInteriorRings);
                for (int j = 0; j < poly.NumInteriorRings; j++)
                {
                    var ring = (LinearRing)poly.GetInteriorRingN(j);
                    const double areaThreshold = 1e-5;
                    if (ring.Area > areaThreshold) holes.Add(ring);
                }
                fixedResultParts.Add(poly.Factory.CreatePolygon((LinearRing)poly.ExteriorRing, holes.ToArray()));
            }

            result = result.Factory.BuildGeometry(fixedResultParts.ToArray());
            TestContext.WriteLine(result.ToString());

            result = UnaryUnionNG.Union(polygons, new PrecisionModel(10_000_000));
            TestContext.WriteLine(result.ToString());
            result = UnaryUnionNG.Union(polygons, new PrecisionModel(1_000_000));
            TestContext.WriteLine(result.ToString());
            result = UnaryUnionNG.Union(polygons, new PrecisionModel(100_000));
            TestContext.WriteLine(result.ToString());
            result = UnaryUnionNG.Union(polygons, new PrecisionModel(10_000));
            TestContext.WriteLine(result.ToString());
            result = UnaryUnionNG.Union(polygons, new PrecisionModel(1_000));
            TestContext.WriteLine(result.ToString());
        }
    }
}
