using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.KdTree;
using NetTopologySuite.Operation.Polygonize;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    [NUnit.Framework.TestFixture]
    public class Issues
    {
        [Test(Description = "GitHub pull request #97")]
        public void TestNearestNeighbor2()
        {
            var kd = new KdTree<string>();
            const int Count = 8;

            for (var row = 0; row < Count; ++row)
            {
                for (var column = 0; column < Count; ++column)
                {
                    kd.Insert(new Coordinate(column, row), (column * 100 + row).ToString());
                }
            }

            var testCoordinate = new Coordinate(Count / 2, Count / 2);
            var res = kd.NearestNeighbor(testCoordinate);

            Assert.AreEqual(testCoordinate, res.Coordinate);
        }

        [Test(Description = "GitHub issue request #114")]
        public void Should_Find_Correct_Number_Of_Polygons_From_Lines()
        {
            var paths = new List<IGeometry>();
            var factory = GeometryFactory.Default;

            for (int x = 1; x < 10; x++)
            {
                var startPoint = new Coordinate(x * 40, 30);
                var endPoint = new Coordinate(x * 40, 440);
                paths.Add(factory.CreateLineString(new[] { startPoint, endPoint }));
            }

            for (int y = 1; y < 10; y++)
            {
                var startPoint = new Coordinate(30, y * 40);
                var endPoint = new Coordinate(450, y * 40);

                paths.Add(factory.CreateLineString(new[] { startPoint, endPoint }));
            }

            var noder = new Noding.Snapround.GeometryNoder(new PrecisionModel(1.0d));

            var geomFactory = GeometryFactory.Default;
            var nodedLines = noder.Node(paths);

            var nodedDedupedLinework = geomFactory.BuildGeometry(nodedLines.ToArray()).Union();
            Assert.AreEqual(
                @"MULTILINESTRING ((40 30, 40 40), (40 40, 40 80), (40 80, 40 120), (40 120, 40 160), (40 160, 40 200), (40 200, 40 240), (40 240, 40 280), (40 280, 40 320), (40 320, 40 360), (40 360, 40 440), (80 30, 80 40), (80 40, 80 80), (80 80, 80 120), (80 120, 80 160), (80 160, 80 200), (80 200, 80 240), (80 240, 80 280), (80 280, 80 320), (80 320, 80 360), (80 360, 80 440), (120 30, 120 40), (120 40, 120 80), (120 80, 120 120), (120 120, 120 160), (120 160, 120 200), (120 200, 120 240), (120 240, 120 280), (120 280, 120 320), (120 320, 120 360), (120 360, 120 440), (160 30, 160 40), (160 40, 160 80), (160 80, 160 120), (160 120, 160 160), (160 160, 160 200), (160 200, 160 240), (160 240, 160 280), (160 280, 160 320), (160 320, 160 360), (160 360, 160 440), (200 30, 200 40), (200 40, 200 80), (200 80, 200 120), (200 120, 200 160), (200 160, 200 200), (200 200, 200 240), (200 240, 200 280), (200 280, 200 320), (200 320, 200 360), (200 360, 200 440), (240 30, 240 40), (240 40, 240 80), (240 80, 240 120), (240 120, 240 160), (240 160, 240 200), (240 200, 240 240), (240 240, 240 280), (240 280, 240 320), (240 320, 240 360), (240 360, 240 440), (280 30, 280 40), (280 40, 280 80), (280 80, 280 120), (280 120, 280 160), (280 160, 280 200), (280 200, 280 240), (280 240, 280 280), (280 280, 280 320), (280 320, 280 360), (280 360, 280 440), (320 30, 320 40), (320 40, 320 80), (320 80, 320 120), (320 120, 320 160), (320 160, 320 200), (320 200, 320 240), (320 240, 320 280), (320 280, 320 320), (320 320, 320 360), (320 360, 320 440), (360 30, 360 40), (360 40, 360 80), (360 80, 360 120), (360 120, 360 160), (360 160, 360 200), (360 200, 360 240), (360 240, 360 280), (360 280, 360 320), (360 320, 360 360), (360 360, 360 440), (30 40, 40 40), (40 40, 80 40), (80 40, 120 40), (120 40, 160 40), (160 40, 200 40), (200 40, 240 40), (240 40, 280 40), (280 40, 320 40), (320 40, 360 40), (360 40, 450 40), (30 80, 40 80), (40 80, 80 80), (80 80, 120 80), (120 80, 160 80), (160 80, 200 80), (200 80, 240 80), (240 80, 280 80), (280 80, 320 80), (320 80, 360 80), (360 80, 450 80), (30 120, 40 120), (40 120, 80 120), (80 120, 120 120), (120 120, 160 120), (160 120, 200 120), (200 120, 240 120), (240 120, 280 120), (280 120, 320 120), (320 120, 360 120), (360 120, 450 120), (30 160, 40 160), (40 160, 80 160), (80 160, 120 160), (120 160, 160 160), (160 160, 200 160), (200 160, 240 160), (240 160, 280 160), (280 160, 320 160), (320 160, 360 160), (360 160, 450 160), (30 200, 40 200), (40 200, 80 200), (80 200, 120 200), (120 200, 160 200), (160 200, 200 200), (200 200, 240 200), (240 200, 280 200), (280 200, 320 200), (320 200, 360 200), (360 200, 450 200), (30 240, 40 240), (40 240, 80 240), (80 240, 120 240), (120 240, 160 240), (160 240, 200 240), (200 240, 240 240), (240 240, 280 240), (280 240, 320 240), (320 240, 360 240), (360 240, 450 240), (30 280, 40 280), (40 280, 80 280), (80 280, 120 280), (120 280, 160 280), (160 280, 200 280), (200 280, 240 280), (240 280, 280 280), (280 280, 320 280), (320 280, 360 280), (360 280, 450 280), (30 320, 40 320), (40 320, 80 320), (80 320, 120 320), (120 320, 160 320), (160 320, 200 320), (200 320, 240 320), (240 320, 280 320), (280 320, 320 320), (320 320, 360 320), (360 320, 450 320), (30 360, 40 360), (40 360, 80 360), (80 360, 120 360), (120 360, 160 360), (160 360, 200 360), (200 360, 240 360), (240 360, 280 360), (280 360, 320 360), (320 360, 360 360), (360 360, 450 360))",
                nodedDedupedLinework.ToString());
            Polygonizer polygonizer = new Polygonizer();
            polygonizer.Add(nodedDedupedLinework);

            var polygons = polygonizer.GetPolygons();
            Assert.AreEqual(64, polygons.Count);
        }
    }
}