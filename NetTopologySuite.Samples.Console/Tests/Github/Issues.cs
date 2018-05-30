using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Index.KdTree;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Polygonize;
using NetTopologySuite.Operation.Valid;
using NetTopologySuite.Precision;
using NetTopologySuite.SnapRound;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    [TestFixture, Category("GitHub Issue")]
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
            // WKT committed by FObermaier in 01/31/2017 => test fail
            string expected = @"MULTILINESTRING ((40 30,  40 40),  (40 40,  40 80),  (40 80,  40 120),  (40 120,  40 160),  (40 160,  40 200),  (40 200,  40 240),  (40 240,  40 280),  (40 280,  40 320),  (40 320,  40 360),  (40 360,  40 440),  (80 30,  80 40),  (80 40,  80 80),  (80 80,  80 120),  (80 120,  80 160),  (80 160,  80 200),  (80 200,  80 240),  (80 240,  80 280),  (80 280,  80 320),  (80 320,  80 360),  (80 360,  80 440),  (120 30,  120 40),  (120 40,  120 80),  (120 80,  120 120),  (120 120,  120 160),  (120 160,  120 200),  (120 200,  120 240),  (120 240,  120 280),  (120 280,  120 320),  (120 320,  120 360),  (120 360,  120 440),  (160 30,  160 40),  (160 40,  160 80),  (160 80,  160 120),  (160 120,  160 160),  (160 160,  160 200),  (160 200,  160 240),  (160 240,  160 280),  (160 280,  160 320),  (160 320,  160 360),  (160 360,  160 440),  (200 30,  200 40),  (200 40,  200 80),  (200 80,  200 120),  (200 120,  200 160),  (200 160,  200 200),  (200 200,  200 240),  (200 240,  200 280),  (200 280,  200 320),  (200 320,  200 360),  (200 360,  200 440),  (240 30,  240 40),  (240 40,  240 80),  (240 80,  240 120),  (240 120,  240 160),  (240 160,  240 200),  (240 200,  240 240),  (240 240,  240 280),  (240 280,  240 320),  (240 320,  240 360),  (240 360,  240 440),  (280 30,  280 40),  (280 40,  280 80),  (280 80,  280 120),  (280 120,  280 160),  (280 160,  280 200),  (280 200,  280 240),  (280 240,  280 280),  (280 280,  280 320),  (280 320,  280 360),  (280 360,  280 440),  (320 30,  320 40),  (320 40,  320 80),  (320 80,  320 120),  (320 120,  320 160),  (320 160,  320 200),  (320 200,  320 240),  (320 240,  320 280),  (320 280,  320 320),  (320 320,  320 360),  (320 360,  320 440),  (360 30,  360 40),  (360 40,  360 80),  (360 80,  360 120),  (360 120,  360 160),  (360 160,  360 200),  (360 200,  360 240),  (360 240,  360 280),  (360 280,  360 320),  (360 320,  360 360),  (360 360,  360 440),  (30 40,  40 40),  (40 40,  80 40),  (80 40,  120 40),  (120 40,  160 40),  (160 40,  200 40),  (200 40,  240 40),  (240 40,  280 40),  (280 40,  320 40),  (320 40,  360 40),  (360 40,  450 40),  (30 80,  40 80),  (40 80,  80 80),  (80 80,  120 80),  (120 80,  160 80),  (160 80,  200 80),  (200 80,  240 80),  (240 80,  280 80),  (280 80,  320 80),  (320 80,  360 80),  (360 80,  450 80),  (30 120,  40 120),  (40 120,  80 120),  (80 120,  120 120),  (120 120,  160 120),  (160 120,  200 120),  (200 120,  240 120),  (240 120,  280 120),  (280 120,  320 120),  (320 120,  360 120),  (360 120,  450 120),  (30 160,  40 160),  (40 160,  80 160),  (80 160,  120 160),  (120 160,  160 160),  (160 160,  200 160),  (200 160,  240 160),  (240 160,  280 160),  (280 160,  320 160),  (320 160,  360 160),  (360 160,  450 160),  (30 200,  40 200),  (40 200,  80 200),  (80 200,  120 200),  (120 200,  160 200),  (160 200,  200 200),  (200 200,  240 200),  (240 200,  280 200),  (280 200,  320 200),  (320 200,  360 200),  (360 200,  450 200),  (30 240,  40 240),  (40 240,  80 240),  (80 240,  120 240),  (120 240,  160 240),  (160 240,  200 240),  (200 240,  240 240),  (240 240,  280 240),  (280 240,  320 240),  (320 240,  360 240),  (360 240,  450 240),  (30 280,  40 280),  (40 280,  80 280),  (80 280,  120 280),  (120 280,  160 280),  (160 280,  200 280),  (200 280,  240 280),  (240 280,  280 280),  (280 280,  320 280),  (320 280,  360 280),  (360 280,  450 280),  (30 320,  40 320),  (40 320,  80 320),  (80 320,  120 320),  (120 320,  160 320),  (160 320,  200 320),  (200 320,  240 320),  (240 320,  280 320),  (280 320,  320 320),  (320 320,  360 320),  (360 320,  450 320),  (30 360,  40 360),  (40 360,  80 360),  (80 360,  120 360),  (120 360,  160 360),  (160 360,  200 360),  (200 360,  240 360),  (240 360,  280 360),  (280 360,  320 360),  (320 360,  360 360),  (360 360,  450 360))";
            // WKT from previous (to 01/31/2017) commit => test ok
            expected = @"MULTILINESTRING ((40 30, 40 40), (40 40, 40 80), (40 80, 40 120), (40 120, 40 160), (40 160, 40 200), (40 200, 40 240), (40 240, 40 280), (40 280, 40 320), (40 320, 40 360), (40 360, 40 440), (80 30, 80 40), (80 40, 80 80), (80 80, 80 120), (80 120, 80 160), (80 160, 80 200), (80 200, 80 240), (80 240, 80 280), (80 280, 80 320), (80 320, 80 360), (80 360, 80 440), (120 30, 120 40), (120 40, 120 80), (120 80, 120 120), (120 120, 120 160), (120 160, 120 200), (120 200, 120 240), (120 240, 120 280), (120 280, 120 320), (120 320, 120 360), (120 360, 120 440), (160 30, 160 40), (160 40, 160 80), (160 80, 160 120), (160 120, 160 160), (160 160, 160 200), (160 200, 160 240), (160 240, 160 280), (160 280, 160 320), (160 320, 160 360), (160 360, 160 440), (200 30, 200 40), (200 40, 200 80), (200 80, 200 120), (200 120, 200 160), (200 160, 200 200), (200 200, 200 240), (200 240, 200 280), (200 280, 200 320), (200 320, 200 360), (200 360, 200 440), (240 30, 240 40), (240 40, 240 80), (240 80, 240 120), (240 120, 240 160), (240 160, 240 200), (240 200, 240 240), (240 240, 240 280), (240 280, 240 320), (240 320, 240 360), (240 360, 240 440), (280 30, 280 40), (280 40, 280 80), (280 80, 280 120), (280 120, 280 160), (280 160, 280 200), (280 200, 280 240), (280 240, 280 280), (280 280, 280 320), (280 320, 280 360), (280 360, 280 440), (320 30, 320 40), (320 40, 320 80), (320 80, 320 120), (320 120, 320 160), (320 160, 320 200), (320 200, 320 240), (320 240, 320 280), (320 280, 320 320), (320 320, 320 360), (320 360, 320 440), (360 30, 360 40), (360 40, 360 80), (360 80, 360 120), (360 120, 360 160), (360 160, 360 200), (360 200, 360 240), (360 240, 360 280), (360 280, 360 320), (360 320, 360 360), (360 360, 360 440), (30 40, 40 40), (40 40, 80 40), (80 40, 120 40), (120 40, 160 40), (160 40, 200 40), (200 40, 240 40), (240 40, 280 40), (280 40, 320 40), (320 40, 360 40), (360 40, 450 40), (30 80, 40 80), (40 80, 80 80), (80 80, 120 80), (120 80, 160 80), (160 80, 200 80), (200 80, 240 80), (240 80, 280 80), (280 80, 320 80), (320 80, 360 80), (360 80, 450 80), (30 120, 40 120), (40 120, 80 120), (80 120, 120 120), (120 120, 160 120), (160 120, 200 120), (200 120, 240 120), (240 120, 280 120), (280 120, 320 120), (320 120, 360 120), (360 120, 450 120), (30 160, 40 160), (40 160, 80 160), (80 160, 120 160), (120 160, 160 160), (160 160, 200 160), (200 160, 240 160), (240 160, 280 160), (280 160, 320 160), (320 160, 360 160), (360 160, 450 160), (30 200, 40 200), (40 200, 80 200), (80 200, 120 200), (120 200, 160 200), (160 200, 200 200), (200 200, 240 200), (240 200, 280 200), (280 200, 320 200), (320 200, 360 200), (360 200, 450 200), (30 240, 40 240), (40 240, 80 240), (80 240, 120 240), (120 240, 160 240), (160 240, 200 240), (200 240, 240 240), (240 240, 280 240), (280 240, 320 240), (320 240, 360 240), (360 240, 450 240), (30 280, 40 280), (40 280, 80 280), (80 280, 120 280), (120 280, 160 280), (160 280, 200 280), (200 280, 240 280), (240 280, 280 280), (280 280, 320 280), (320 280, 360 280), (360 280, 450 280), (30 320, 40 320), (40 320, 80 320), (80 320, 120 320), (120 320, 160 320), (160 320, 200 320), (200 320, 240 320), (240 320, 280 320), (280 320, 320 320), (320 320, 360 320), (360 320, 450 320), (30 360, 40 360), (40 360, 80 360), (80 360, 120 360), (120 360, 160 360), (160 360, 200 360), (200 360, 240 360), (240 360, 280 360), (280 360, 320 360), (320 360, 360 360), (360 360, 450 360))";
            Assert.AreEqual(expected, nodedDedupedLinework.ToString());
            Polygonizer polygonizer = new Polygonizer();
            polygonizer.Add(nodedDedupedLinework);

            var polygons = polygonizer.GetPolygons();
            Assert.AreEqual(64, polygons.Count);
        }

        [Test(Description = "GitHub Issue #122/1")]
        public void Polygon_intersection_Error1()
        {
            //Arrange
            var reader = new WKTReader();
            var g1 = reader.Read(
@"LINEARRING (0.0000000000000000 -6.1026585365860800,  3.8000000000000000 -5.7690000000000000,
7.9000000000000000 -5.4090000000000000,  12.0000000000000000 -5.3690000000000000,
16.1000000000000000 -5.0990000000000000,  20.3000000000000000 -5.1390000000000000,
24.5000000000000000 -4.9090000000000000,  28.5000000000000000 -4.6390000000000000,
32.6000000000000000 -4.1890000000000000,  36.6000000000000000 -3.2890000000000000,
40.8000000000000000 -3.1890000000000000,  44.8000000000000000 -2.9490000000000000,
49.0000000000000000 -2.8200000000000000,  53.3000000000000000 -2.3600000000000000,
57.3000000000000000 -2.1400000000000000,  61.3000000000000000 -1.5200000000000000,
65.5000000000000000 -1.3900000000000000,  69.6000000000000000 -1.1700000000000000,
73.6000000000000000 -1.0100000000000000,  77.9000000000000000 -0.9700000000000000,
82.0000000000000000 -0.8000000000000000,  86.2000000000000000 -0.7400000000000000,
90.2000000000000000 -0.6700000000000000,  94.5000000000000000 -0.6000000000000000,
98.6000000000000000 -0.5000000000000000,  102.6000000000000000 -0.3900000000000000,
106.9000000000000000 -0.2400000000000000,  111.1000000000000000 -0.1500000000000000,
115.1000000000000000 -0.1510000000000000,  119.2000000000000000 -0.1510000000000000,
123.1000000000000000 -0.0510000000000000,  127.2000000000000000 0.0590000000000000,
131.2000000000000000 0.2090000000000000,  132.2000000000000000 1.0250000000000000,
132.6000000000000000 2.6400000000000000,  132.6000000000000000 2.6400000000000000,
132.6000000000000000 0.5000000000000000,  128.6000000000000000 0.5000000000000000,
106.2000000000000000 -2.3000000000000000,  49.9000000000000000 -2.3000000000000000,
31.0000000000000000 -5.4500000000000000,  0.0000000000000000 -5.4500000000000000,
0.0000000000000000 -6.1026585365860800)");
            var g2 = reader.Read(
@"LINEARRING (0.0000000000000000 2.6400000000000000,  0.0000000000000000 -6.0030697674418800,
0.3200000000000000 -5.9800000000000000,  4.6200000000000000 -5.6700000000000000,
8.8100000000000000 -5.1600000000000000,  13.1000000000000000 -5.2100000000000000,
17.1200000000000000 -4.8400000000000000,  21.2800000000000000 -5.1700000000000000,
25.5000000000000000 -5.1600000000000000,  29.7100000000000000 -4.6700000000000000,
34.1300000000000000 -3.9000000000000000,  38.2800000000000000 -3.4800000000000000,
42.6400000000000000 -3.3000000000000000,  46.9800000000000000 -3.2600000000000000,
51.1600000000000000 -2.8900000000000000,  55.3200000000000000 -2.8300000000000000,
59.3000000000000000 -2.9200000000000000,  63.6800000000000000 -2.3600000000000000,
67.4800000000000000 -2.5700000000000000,  70.8900000000000000 -2.4800000000000000,
74.8900000000000000 -2.5500000000000000,  78.8800000000000000 -2.6900000000000000,
82.4400000000000000 -2.4600000000000000,  87.5300000000000000 -2.8100000000000000,
91.6200000000000000 -2.7500000000000000,  95.9000000000000000 -2.1000000000000000,
100.2200000000000000 -2.2100000000000000,  104.4400000000000000 -2.3000000000000000,
109.1500000000000000 -1.9500000000000000,  113.2400000000000000 -2.8200000000000000,
117.2400000000000000 -1.9600000000000000,  120.9400000000000000 -1.3000000000000000,
125.0300000000000000 -0.8200000000000000,  126.6800000000000000 0.1300000000000000,
132.5500000000000000 1.5400000000000000,  132.6000000000000000 2.6400000000000000,
132.6000000000000000 2.6400000000000000,  132.6000000000000000 2.6400000000000000,
0.0000000000000000 2.6400000000000000)");

            //Act
            var res = g1.Intersection(g2);

            //
            ToImage(1, g1, g2, res);

            // Assert
            Assert.That(res, Is.Not.Null);
            Debug.WriteLine(res.AsText());

        }

        [Test(Description = "GitHub Issue #122/2")]
        public void Polygon_intersection_Error2()
        {
            //Arrange
            var reader = new WKTReader();
            var g1 = reader.Read(
@"POLYGON ((0.0000000000000000 -6.1026585365860800,  3.8000000000000000 -5.7690000000000000,
7.9000000000000000 -5.4090000000000000,  12.0000000000000000 -5.3690000000000000,
16.1000000000000000 -5.0990000000000000,  20.3000000000000000 -5.1390000000000000,
24.5000000000000000 -4.9090000000000000,  28.5000000000000000 -4.6390000000000000,
32.6000000000000000 -4.1890000000000000,  36.6000000000000000 -3.2890000000000000,
40.8000000000000000 -3.1890000000000000,  44.8000000000000000 -2.9490000000000000,
49.0000000000000000 -2.8200000000000000,  53.3000000000000000 -2.3600000000000000,
57.3000000000000000 -2.1400000000000000,  61.3000000000000000 -1.5200000000000000,
65.5000000000000000 -1.3900000000000000,  69.6000000000000000 -1.1700000000000000,
73.6000000000000000 -1.0100000000000000,  77.9000000000000000 -0.9700000000000000,
82.0000000000000000 -0.8000000000000000,  86.2000000000000000 -0.7400000000000000,
90.2000000000000000 -0.6700000000000000,  94.5000000000000000 -0.6000000000000000,
98.6000000000000000 -0.5000000000000000,  102.6000000000000000 -0.3900000000000000,
106.9000000000000000 -0.2400000000000000,  111.1000000000000000 -0.1500000000000000,
115.1000000000000000 -0.1510000000000000,  119.2000000000000000 -0.1510000000000000,
123.1000000000000000 -0.0510000000000000,  127.2000000000000000 0.0590000000000000,
131.2000000000000000 0.2090000000000000,  132.2000000000000000 1.0250000000000000,
132.6000000000000000 2.6400000000000000,  132.6000000000000000 2.6400000000000000,
132.6000000000000000 0.5000000000000000,  128.6000000000000000 0.5000000000000000,
106.2000000000000000 -2.3000000000000000,  49.9000000000000000 -2.3000000000000000,
31.0000000000000000 -5.4500000000000000,  0.0000000000000000 -5.4500000000000000,
0.0000000000000000 -6.1026585365860800))");
            var isValidOp = new IsValidOp(g1);
            if (!isValidOp.IsValid)
            {
                Debug.WriteLine("g1 is not valid:" + isValidOp.ValidationError);
                g1 = g1.Buffer(0);
                Debug.WriteLine(g1.AsText());
            }
            var g2 = reader.Read(
@"POLYGON ((0.0000000000000000 2.6400000000000000,  0.0000000000000000 -6.0030697674418800,
0.3200000000000000 -5.9800000000000000,  4.6200000000000000 -5.6700000000000000,
8.8100000000000000 -5.1600000000000000,  13.1000000000000000 -5.2100000000000000,
17.1200000000000000 -4.8400000000000000,  21.2800000000000000 -5.1700000000000000,
25.5000000000000000 -5.1600000000000000,  29.7100000000000000 -4.6700000000000000,
34.1300000000000000 -3.9000000000000000,  38.2800000000000000 -3.4800000000000000,
42.6400000000000000 -3.3000000000000000,  46.9800000000000000 -3.2600000000000000,
51.1600000000000000 -2.8900000000000000,  55.3200000000000000 -2.8300000000000000,
59.3000000000000000 -2.9200000000000000,  63.6800000000000000 -2.3600000000000000,
67.4800000000000000 -2.5700000000000000,  70.8900000000000000 -2.4800000000000000,
74.8900000000000000 -2.5500000000000000,  78.8800000000000000 -2.6900000000000000,
82.4400000000000000 -2.4600000000000000,  87.5300000000000000 -2.8100000000000000,
91.6200000000000000 -2.7500000000000000,  95.9000000000000000 -2.1000000000000000,
100.2200000000000000 -2.2100000000000000,  104.4400000000000000 -2.3000000000000000,
109.1500000000000000 -1.9500000000000000,  113.2400000000000000 -2.8200000000000000,
117.2400000000000000 -1.9600000000000000,  120.9400000000000000 -1.3000000000000000,
125.0300000000000000 -0.8200000000000000,  126.6800000000000000 0.1300000000000000,
132.5500000000000000 1.5400000000000000,  132.6000000000000000 2.6400000000000000,
132.6000000000000000 2.6400000000000000,  132.6000000000000000 2.6400000000000000,
0.0000000000000000 2.6400000000000000))");
            isValidOp = new IsValidOp(g1);
            if (!isValidOp.IsValid)
            {
                Debug.WriteLine("g2 is not valid:" + isValidOp.ValidationError);
                g2 = g2.Buffer(0);
                Debug.WriteLine(g2.AsText());
            }
            //Act
            var res = g1.Intersection(g2);

            //
            ToImage(1, g1, g2, res);

            // Assert
            Assert.That(res, Is.Not.Null);
            Debug.WriteLine(res.AsText());

        }

        [Test(Description = "GitHub Issue #123")]
        public void LineString_Intersection_with_Polygon_return_null1()
        {
            //Arrange
            var reader = new WKTReader();
            var g1 = reader.Read(@"LINESTRING (0 -5.15,  30 -5.15,  48.9 -2,  105.2 -2,  127.6 0.8,  132.6 0.8)");
            var g2 = reader.Read(@"LINEARRING (15.325555555555553 0.8,  42.309375 0.8,  42.309375 -5.15,  15.325555555555553 -5.15,  15.325555555555553 0.8)");

            //Act
            var res = g1.Intersection(g2);

            //
            ToImage(1, g1, g2, res);

            // Assert
            Assert.That(res, Is.Not.Null);
            Debug.WriteLine(res.AsText());
        }

        [Test(Description = "GitHub Issue #125")]
        public void Fixing_invalid_polygon_with_Buffer_0_returns_empty_polygon()
        {
            //arrange
            var rdr = new WKTReader();
            rdr.RepairRings = true;
            var poly = rdr.Read(
@"POLYGON ((1.4749999999994841 -5.15,  30 -5.15,  48.9 -2,
108.1997 -2,  130.25148787313435 0.75647348414179227,
130.25148787313435 0.75647348414179161,  130 0.75,
126.3 0.72,  122.9 0.7,  119.2 0.42,  115.2 0.45,
111 0.29,  106.9 0.23,  102.8 0.2,  98.8 0.12,  94.8 0.04,
90.7 -0.08,  86.5 -0.2,  82.4 -0.42,  78.3 -0.57,  74.1 -0.69,
69.9 -0.78,  65.8 -0.87,  61.7 -1.07,  57.7 -1.09,  53.7 -1.229,
49.5 -1.289,  45.3 -1.369,  41.2 -1.719,  37 -2.409,  32.8 -3.219,
28.6 -3.769,  24.5 -4.089,  20.4 -4.429,  16.3 -4.599,  12.1 -4.759,
8 -4.889,  4 -5.049,  1.4749999999994841 -5.15))");

            //act
            var gpr = new NetTopologySuite.Precision.GeometryPrecisionReducer(new PrecisionModel(1e10));
            var poly1 = gpr.Reduce(poly);
            var poly2 = poly.Buffer(0);
            var shell = poly.Factory.CreatePolygon(((IPolygon)poly).Shell.CoordinateSequence.Reversed()).Buffer(0);

            ToImage(0, poly, poly1, poly2);

            var isValidOp = new IsValidOp(poly);
            if (!isValidOp.IsValid)
            {
                Debug.WriteLine(isValidOp.ValidationError);

            }
            Debug.WriteLine(poly1.AsText());
            // assert
            //Assert.That(poly.IsValid,  Is.True,  "poly.IsValid");
            Assert.That(poly1.IsValid, Is.True, "poly1.IsValid");
            Assert.That(poly2, Is.Not.Null, "poly2 != null");

            //Known to fail
            //Assert.That(poly2.IsEmpty,  Is.False,  "poly2.IsEmpty");
        }

        [Test(Description = "GitHub Issue #126")]

        [TestCase(Ordinates.XY)]
        [TestCase(Ordinates.XYZ)]
        [TestCase(Ordinates.XYM)]
        [TestCase(Ordinates.XYZM)]
        public void GeometryFactory_CreatePoint_does_not_have_Measure_value(Ordinates ordinates)
        {
            var csf = DotSpatialAffineCoordinateSequenceFactory.Instance = new DotSpatialAffineCoordinateSequenceFactory(ordinates);
            var gf = new GeometryFactory(DotSpatialAffineCoordinateSequenceFactory.Instance);
            var cs1 = csf.Create(1, Ordinates.XYZM);
            Assert.That(cs1.Ordinates == ordinates);

            cs1.SetOrdinate(0, Ordinate.X, 1);
            cs1.SetOrdinate(0, Ordinate.Y, 2);
            cs1.SetOrdinate(0, Ordinate.Z, 3);
            cs1.SetOrdinate(0, Ordinate.M, 4);

            var cs2 = csf.Create(new[] { new Coordinate(1, 2) });
            Assert.That(cs1.Ordinates == ordinates);

            var pt1 = gf.CreatePoint(cs1);
            var pt2 = gf.CreatePoint(cs2);
            var pt3 = gf.CreatePoint(new Coordinate(1, 2));

            Assert.That(pt1, Is.EqualTo(pt2));
            Assert.That(pt2, Is.EqualTo(pt3));
            Assert.That(pt1, Is.EqualTo(pt3));
        }

        [Test, Category("GitHub Issue"), Description("Polygon / rectangle intersection returns empty polygon"), Ignore("Claim not valid")]
        public void TestIssue149()
        {
            var wktreader = new WKTReader();
            var polygon = wktreader.Read(@"POLYGON ((-62.327500000000008 77.777166176470587,
 -62.327500000000008 191.08,
 -39.425176714633878 191.08,
 30.449401394787785 111.06902734290135,
 34.240621449176217 85.17199763498823,
 32.474968591813237 66.628747810197453,
 29.724407935551895 56.755281980420541,
 10.963366463822565 52.502595315049426,
 -62.327500000000008 77.777166176470587))");

            Assert.IsTrue(polygon.IsValid);
            Assert.IsFalse(polygon.IsEmpty);

            var boundingbox = wktreader.Read(@"POLYGON ((-52.5 -34, -52.5 34, 52.5 34, 52.5 -34, -52.5 -34))");
            Assert.IsTrue(boundingbox.IsValid);
            Assert.IsFalse(boundingbox.IsEmpty);

            //ToImage(1, polygon, boundingbox, boundingbox.Intersection(polygon));
            //Assert.IsTrue(polygon.Intersects(boundingbox));

            var result = boundingbox.Intersection(polygon); // =>{ POLYGON EMPTY }
            //Assert.IsFalse(result.IsEmpty, "result.IsEmpty");
            var resultInverted = polygon.Intersection(boundingbox); // => { POLYGON EMPTY }
            //Assert.IsFalse(resultInverted.IsEmpty, "result.IsEmpty");
        }

        [Test, Description("TopologyException when generating a VoronoiDiagram"), Ignore("Known to fail, waiting for fix from JTS.")]
        public void TestIssue151()
        {
            var wktreader = new WKTReader();
            var polygon = wktreader.Read("POLYGON((14.7119 201.6703, 74.2154 201.6703, 74.2154 166.6391, 14.7119 166.6391, 14.7119 201.6703))");

            Assert.IsTrue(polygon.IsValid);
            var vdb = new NetTopologySuite.Triangulate.VoronoiDiagramBuilder();
            vdb.SetSites(polygon);
            IGeometry result = null;
            Assert.DoesNotThrow(() => result= vdb.GetDiagram(polygon.Factory));
            Assert.IsNotNull(result);
        }

        [Test, Description("SnapRoundOverlayFunctions, Casting error in GeometryEditorEx.EditGeometryCollection")]
        public void TestIssue177()
        {
            var reader = new WKTReader();
            var g1 = reader.Read("MULTIPOLYGON (((30 20, 45 40, 10 40, 30 20)),((15 5, 40 10, 10 20, 5 10, 15 5)))");
            var g2 = reader.Read("MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)),((20 35, 10 30, 10 10, 30 5, 45 20, 20 35),(30 20, 20 15, 20 25, 30 20)))");

            IGeometry res = null;
            //                                                                                     |
            //                                                             Some cruel scale factor V
            Assert.DoesNotThrow(() => res = SnapRoundOverlayFunctions.SnappedIntersection(g1, g2, 0.1));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsValid, Is.True);

            ToImage(0, g1, g2, res);

            // To show that the result is correct:
            var r1 = new GeometryPrecisionReducer(new PrecisionModel(0.1)).Reduce(g1);
            var r2 = new GeometryPrecisionReducer(new PrecisionModel(0.1)).Reduce(g2);
            ToImage(0, r1, r2, res);
        }

        [Test(Description = "Bug in difference method #141"), Category("GitHub Issue")]
        public void TestDifference()
        {
            var buffer1 = WKBReader.HexToBytes(
               //         1         2         3         4         5         6         7         8
                "010300000001000000050000000793adf1033458c0a8a08e06b1b34740f6df183c0b3458c0b9d84f"+
                "27e2b247403559f264963458c0f0eeec1ee5b24740fd9c0635913458c067f1cffdb6b347400793ad"+
                "f1033458c0a8a08e06b1b34740");
            var buffer2= WKBReader.HexToBytes(
                "010300000001000000b5000000f5a3caf8a33458c0f66779fd4db34740e8b86ed8a33458c09207fc"+
                "c351b3474086533f9fa33458c0f5565f8855b347407dda504da33458c04fb0764959b347401eedba"+
                "e2a23458c0929317065db3474080e79f5fa23458c0ec5a15bd60b34740208427c4a13458c011ac47"+
                "6d64b347405f278310a13458c07452881568b34740e4a6ea44a03458c00d65b5b46bb347409a499c"+
                "619f3458c011d5aa496fb347403313e1669e3458c0fb044bd372b34740064005559d3458c0c6a27c"+
                "5076b347402dc95f2c9c3458c0338228c079b3474049f34bed9a3458c081c23b217db34740fd992d"+
                "98993458c038f4a97280b34740161d702d983458c0dda766b383b34740e02783ad963458c0417674"+
                "e286b34740cde9dd18953458c0ef86cffe89b34740383c0170933458c0fce380078db34740590b6f"+
                "b3913458c001e395fb8fb3474053edb2e38f3458c09a7024da92b34740a8d65d018e3458c0e9c446"+
                "a295b34740431a060d8c3458c014af1f5398b347406d6947078a3458c00424d4eb9ab34740b9e6c3"+
                "f0873458c0ac469a6b9db34740390022ca853458c04014a6d19fb34740836f0c94833458c03c923a"+
                "1da2b347405372354f813458c0a0119f4da4b34740266c51fc7e3458c034a02562a6b34740fc0b19"+
                "9c7c3458c0c5e2285aa8b3474019724b2f7a3458c064ef0935aab3474084e4a9b6773458c0ec9834"+
                "f2abb34740e5e1f832753458c0fe6e1f91adb34740474702a5723458c03e984911afb34740781792"+
                "0d703458c09aac3872b0b34740077b766d6d3458c084267fb3b1b3474005e681c56a3458c0393dba"+
                "d4b2b3474066df8716683458c0fabe8fd5b3b34740bd265f61653458c00f11aeb5b4b34740828edf"+
                "a6623458c0477bd074b5b34740ea0ee3e75f3458c079dcba12b6b347400db344255d3458c043d03b"+
                "8fb6b34740c6abe15f5a3458c049892aeab6b347400c179698573458c03a686f23b7b347407a4b41"+
                "d0543458c087f3f43ab7b34740e979c007523458c0aedfb630b7b34740f5f8f23f4f3458c0ae2cb5"+
                "04b7b3474077f9b5794c3458c011bdfcb6b6b3474047ace6b5493458c0a97ba947b6b347403f4262"+
                "f5463458c08ec4ddb6b5b3474059d90439443458c0df8ac404b5b34740aa7ca981413458c042a495"+
                "31b4b34740caeb26d03e3458c024a3933db3b347400c0c56253c3458c0fbb00929b2b34740278a0c"+
                "82393458c085ff51f4b0b347406ab41ae7363458c07fc0c69fafb3474006ec5155343458c0399fd7"+
                "2baeb347408a5980cd313458c0fe46f498acb3474041b46d502f3458c029919de7aab3474056c6e2"+
                "de2c3458c056c85a18a9b34740afe8a1792a3458c069a8b92ba7b34740913b6a21283458c0cccf54"+
                "22a5b34740bd93f6d6253458c0b399d1fca2b3474034a0ff9a233458c01087d7bba0b34740ef7835"+
                "6e213458c023211d609eb34740e93548511f3458c0ad3c5dea9bb347403445de441d3458c0376b5d"+
                "5b99b3474004029d491b3458c08fafe9b396b3474085302160193458c088a3d6f493b34740415c04"+
                "89173458c0772cfd1e91b347409f8cd8c4153458c03d1243338eb347405f902c14143458c07a4290"+
                "328bb34740608c8577123458c0141cd31d88b34740db6c65ef103458c0f59402f684b34740048745"+
                "7c0f3458c08bee19bc81b34740abd1991e0e3458c04b011d717eb347405dd2cfd60c3458c06bcb11"+
                "167bb347409f774ca50b3458c0a29602ac77b347405577718a0a3458c0b043023474b34740bbb794"+
                "86093458c052b323af70b3474089d3079a083458c04e5d821e6db3474033f415c5073458c0a69337"+
                "8369b347406686ff07073458c0256567de65b347406b98ff62063458c01295313162b3474046c74a"+
                "d6053458c0f957bc7c5eb34740f6180d62053458c0e92d32c25ab3474092e96806053458c06e4bb9"+
                "0257b34740b0497cc3043458c058567e3f53b34740deb25c99043458c0b7ceab794fb34740e4e114"+
                "88043458c0dca572b24bb34740e75aac8f043458c09a81ffea47b3474064d21eb0043458c0bf077f"+
                "2444b3474036c464e9043458c01ede1d6040b34740aec9693b053458c046d00a9f3cb34740bc1d14"+
                "a6053458c087386ee238b34740e89c4429063458c071e2762b35b34740f766d1c4063458c052284d"+
                "7b31b34740662a8a78073458c033f312d32db34740ceeb3444083458c0619df0332ab34740247794"+
                "27093458c025ea059f26b3474096db60220a3458c041516e1523b347400eb74b340b3458c0ff9549"+
                "981fb347403136005d0c3458c01b99aa281cb347409eee209c0d3458c09415a2c718b3474092174b"+
                "f10e3458c028ec427615b347402164145c103458c04af5903512b347405ef009dc113458c0f25494"+
                "060fb3474095b2b670133458c08f4c48ea0bb347406fd19919153458c00ad2a3e108b34740d14d30"+
                "d6163458c050db9ded05b347409991eea5183458c001561e0f03b3474004ce45881a3458c0fd090b"+
                "4700b3474089779c7c1c3458c0de4d4396fdb247407e155a821e3458c038e19dfdfab24740905fda"+
                "98203458c0dac6e67df8b24740ade776bf223458c09101ea17f6b247401e0786f5243458c0e08b64"+
                "ccf3b247402980553a273458c004ef0c9cf1b247406fdc2f8d293458c0fa429387efb24740d27f5d"+
                "ed2b3458c0b3089f8fedb247402b371e5a2e3458c09cdecab4ebb2474037e2b2d2303458c0dcf1aa"+
                "f7e9b24740acc95356333458c054fecc58e8b247401e493ae4353458c0da91add8e6b24740c25d9a"+
                "7b383458c0463ac977e5b2474047b9a31b3b3458c062578b36e4b247407bfa84c33d3458c0f1b156"+
                "15e3b247404dad6b72403458c036c78914e2b2474047ff7f27433458c066e67134e1b24740f41dea"+
                "e1453458c0b1c75375e0b24740fd23d1a0483458c0c3d76fd7dfb247406af358634b3458c0ba09f1"+
                "5adfb24740426ea5284e3458c0b5500200dfb24740ab63d9ef503458c08597bfc6deb24740cea217"+
                "b8533458c0380c3aafdeb24740efe78180563458c0112078b9deb2474073dc3848593458c04fad77"+
                "e5deb24740824f5f0e5c3458c06ad12b33dfb24740232319d25e3458c04fc77aa2dfb24740bb0087"+
                "92613458c0e7324233e0b2474013f0ce4e643458c051fb54e5e0b24740f3f81606673458c0aa707d"+
                "b8e1b24740262385b7693458c0c2da76ace2b2474016af42626c3458c02310f6c0e3b24740d0157c"+
                "056f3458c0942aa5f5e4b2474081bd5ca0713458c0d2ac254ae6b24740ba6a1532743458c051110a"+
                "bee7b24740cc07d9b9763458c00387e050e9b24740acb7dd36793458c04f5a2a02ebb24740efd55c"+
                "a87b3458c0994060d1ecb24740af09940d7e3458c0fd7df4bdeeb24740e60cc265803458c04f4e4a"+
                "c7f0b24740b41d2db0823458c01f7cbeecf2b24740dab21eec843458c07786a92df5b247409c8ee4"+
                "18873458c019e45489f7b247400099ce35893458c045c005fff9b24740d47637428b3458c07089f6"+
                "8dfcb24740e4cc793d8d3458c00c175935ffb2474044b1f6268f3458c08b405ff401b347402abe16"+
                "fe903458c08f8927ca04b3474050d946c2923458c07f9bd2b507b34740b459fa72943458c0b98878"+
                "b60ab34740d8e1a80f963458c0d5a626cb0db3474025bed397973458c06b4beaf210b34740c460fe"+
                "0a993458c08be9c32c14b34740c4e5b5689a3458c041f4b37717b347407cda8db09b3458c05a6db4"+
                "d21ab34740a42a1fe29c3458c09abfb63c1eb3474019460afd9d3458c0877baeb421b34740bf33f8"+
                "009f3458c01d4f823925b34740de5897ed9f3458c01b0e1bca28b34740018c9cc2a03458c0bd405d"+
                "652cb347407c60c77fa13458c0f9fd260a30b3474045a2da24a23458c0063754b733b34740f9eca4"+
                "b1a23458c09c28c56b37b34740b827f925a33458c029074b263bb34740abd0b281a33458c0e3c3c1"+
                "e53eb34740fdfcb5c4a33458c03793faa842b347401f33edeea33458c017f5ca6e46b34740a87d4a"+
                "00a43458c0f21d04364ab34740f5a3caf8a33458c0f66779fd4db34740");

            NtsGeometryServices.Instance = new NtsGeometryServices();
            var reader = new WKBReader();
            var geom1 = reader.Read(buffer1);
            var geom2 = reader.Read(buffer2);

            var gu = geom1.Difference(geom2);
            Assert.IsNotNull(gu);
            Assert.IsTrue(gu is IPolygonal);

            ToImage(141, geom1, geom2, gu);
            ToImage(141, geom1, geom2, null);
            ToImage(141, null, null, gu);
            ToImage(141, null, null, geom2.Difference(geom1));

            bool isCCW = Orientation.IsCCW(((IPolygon) gu.GetGeometryN(0)).ExteriorRing.CoordinateSequence);
            for (var i = 1; i < gu.NumGeometries; i++)
            {
                var p = (IPolygon) gu.GetGeometryN(1);
                Assert.That(Orientation.IsCCW(p.ExteriorRing.CoordinateSequence), Is.EqualTo(isCCW));
            }
            Console.WriteLine("Orientation CCW = {0}", isCCW);
        }

        [Test]
        public void CrossAndIntersectionTest()
        {
            // Arrange
            var gf = new GeometryFactory(new PrecisionModel(100000000));

            var closestCoordinate = new Coordinate(152608, 594957);
            var closestLine = gf.CreateLineString(new[]
            {
                new Coordinate(152348, 595130),
                new Coordinate(152421, 595061),
                new Coordinate(152455, 595033),
                new Coordinate(152524, 595001),
                new Coordinate(152593, 594973),
                new Coordinate(152622, 594946),
                new Coordinate(152634, 594930),
                new Coordinate(152641, 594921),
                new Coordinate(152649, 594910),
                new Coordinate(152863, 594623),
                new Coordinate(152873, 594607)
            });
            var indexedLine = new NetTopologySuite.LinearReferencing.LengthIndexedLine(closestLine);
            var projectedIndex = indexedLine.Project(closestCoordinate);
            var coordinateToAdd = indexedLine.ExtractPoint(projectedIndex);
            gf.PrecisionModel.MakePrecise(coordinateToAdd);

            var line = gf.CreateLineString(new[] { new Coordinate(152503, 594904), coordinateToAdd });

            ToImage(0, closestLine, line, GeometryFactory.Default.CreatePoint(coordinateToAdd));

            // act
            var intersectionPt = line.Intersection(closestLine).Coordinate;
            gf.PrecisionModel.MakePrecise(intersectionPt);

            // assert intersection point is equal to coordinate to add
            Assert.AreEqual(coordinateToAdd, intersectionPt);

            // act insertion of coordinate to add
            var lip = new NetTopologySuite.LinearReferencing.LocationIndexOfPoint(closestLine);
            var ll = lip.IndexOf(coordinateToAdd);
            if (!ll.IsVertex)
            {
                var cl = (ILineString) closestLine;
                var cls = cl.Factory.CoordinateSequenceFactory.Create(cl.CoordinateSequence.Count + 1, cl.CoordinateSequence.Ordinates);
                CoordinateSequences.Copy(cl.CoordinateSequence, 0, cls, 0, ll.SegmentIndex+1);
                cls.SetOrdinate(ll.SegmentIndex+1, Ordinate.X, coordinateToAdd.X);
                cls.SetOrdinate(ll.SegmentIndex+1, Ordinate.Y, coordinateToAdd.Y);
                CoordinateSequences.Copy(cl.CoordinateSequence, ll.SegmentIndex+1, cls, ll.SegmentIndex + 2, cl.CoordinateSequence.Count-ll.SegmentIndex-1);
                closestLine = gf.CreateLineString(cls);
            }

            ToImage(1, closestLine, line, GeometryFactory.Default.CreatePoint(coordinateToAdd));

            Assert.IsTrue(line.Touches(closestLine));
            Assert.IsFalse(line.Crosses(closestLine));
        }

        #region utility

        static void ToImage(int nr, IGeometry geom1, IGeometry geom2, IGeometry geom3)
        {

            //var gpw = new Windows.Forms.GraphicsPathWriter();

            //var extent = new Envelope();
            //if (geom1 != null)
            //    extent.ExpandToInclude(geom1.EnvelopeInternal);
            //if (geom2 != null)
            //    extent.ExpandToInclude(geom2.EnvelopeInternal);
            //if (geom3 != null)
            //    extent.ExpandToInclude(geom3.EnvelopeInternal);

            //extent.ExpandBy(0.05 * extent.Width);

            //using (var img = new Bitmap(ImageWidth, ImageHeight))
            //{
            //    using (var gr = Graphics.FromImage(img))
            //    {
            //        var at = CreateAffineTransformation(extent);
            //        gr.Clear(Color.WhiteSmoke);
            //        gr.SmoothingMode = SmoothingMode.AntiAlias;
            //        //gr.Transform = CreateTransform(extent);

            //        if (geom1 != null)
            //        {
            //            var gp1 = gpw.ToShape(at.Transform(geom1));
            //            if (geom1 is IPolygonal)
            //                gr.FillPath(new SolidBrush(Color.FromArgb(64, Color.Blue)), gp1);
            //            gr.DrawPath(Pens.Blue, gp1);
            //        }

            //        if (geom2 != null)
            //        {
            //            var gp2 = gpw.ToShape(at.Transform(geom2));
            //            if (geom2 is IPolygonal)
            //                gr.FillPath(new SolidBrush(Color.FromArgb(64, Color.OrangeRed)), gp2);
            //            gr.DrawPath(Pens.OrangeRed, gp2);
            //        }

            //        if (geom3 != null)
            //        {
            //            var gp3 = gpw.ToShape(at.Transform(geom3));
            //            if (geom3 is IPolygonal)
            //                gr.FillPath(new SolidBrush(Color.FromArgb(64, Color.Gold)), gp3);
            //            gr.DrawPath(Pens.Gold, gp3);
            //        }

            //    }
            //    var path = System.IO.Path.ChangeExtension(System.IO.Path.GetTempFileName(), "png");
            //    img.Save(path, ImageFormat.Png);
            //    Console.WriteLine("Image for Test {0} written to {1}", nr, new Uri(path).AbsoluteUri);
            //}
        }

        private const int ImageWidth = 640;
        private const int ImageHeight = 480;

        private static AffineTransformation CreateAffineTransformation(Envelope env, int offsetX = 0)
        {
            var imageRatio = ImageWidth / ImageHeight;
            var ratio = env.Width / env.Height;
            if (ratio > imageRatio)
            {
                var growHeight = (env.Width / imageRatio - env.Height) / 2;
                env.ExpandBy(0, growHeight);
            }
            else if (ratio < imageRatio)
            {
                var growWidth = (env.Height * imageRatio - env.Width) / 2;
                env.ExpandBy(growWidth, 0);
            }

            var s1 = new Coordinate(env.MinX, env.MaxY);
            var t1 = new Coordinate(offsetX, 0);
            var s2 = new Coordinate(env.MaxX, env.MaxY);
            var t2 = new Coordinate(offsetX + ImageWidth, 0);
            var s3 = new Coordinate(env.MaxX, env.MinY);
            var t3 = new Coordinate(offsetX + ImageWidth, ImageHeight);

            var atb = new AffineTransformationBuilder(s1, s2, s3, t1, t2, t3);
            return atb.GetTransformation();
        }

        #endregion
    }
}
