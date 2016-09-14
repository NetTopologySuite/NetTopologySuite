using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Index.KdTree;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Polygonize;
using NetTopologySuite.Operation.Valid;
using NetTopologySuite.Precision;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            Assert.AreEqual(
                @"MULTILINESTRING ((40 30, 40 40), (40 40, 40 80), (40 80, 40 120), (40 120, 40 160), (40 160, 40 200), (40 200, 40 240), (40 240, 40 280), (40 280, 40 320), (40 320, 40 360), (40 360, 40 440), (80 30, 80 40), (80 40, 80 80), (80 80, 80 120), (80 120, 80 160), (80 160, 80 200), (80 200, 80 240), (80 240, 80 280), (80 280, 80 320), (80 320, 80 360), (80 360, 80 440), (120 30, 120 40), (120 40, 120 80), (120 80, 120 120), (120 120, 120 160), (120 160, 120 200), (120 200, 120 240), (120 240, 120 280), (120 280, 120 320), (120 320, 120 360), (120 360, 120 440), (160 30, 160 40), (160 40, 160 80), (160 80, 160 120), (160 120, 160 160), (160 160, 160 200), (160 200, 160 240), (160 240, 160 280), (160 280, 160 320), (160 320, 160 360), (160 360, 160 440), (200 30, 200 40), (200 40, 200 80), (200 80, 200 120), (200 120, 200 160), (200 160, 200 200), (200 200, 200 240), (200 240, 200 280), (200 280, 200 320), (200 320, 200 360), (200 360, 200 440), (240 30, 240 40), (240 40, 240 80), (240 80, 240 120), (240 120, 240 160), (240 160, 240 200), (240 200, 240 240), (240 240, 240 280), (240 280, 240 320), (240 320, 240 360), (240 360, 240 440), (280 30, 280 40), (280 40, 280 80), (280 80, 280 120), (280 120, 280 160), (280 160, 280 200), (280 200, 280 240), (280 240, 280 280), (280 280, 280 320), (280 320, 280 360), (280 360, 280 440), (320 30, 320 40), (320 40, 320 80), (320 80, 320 120), (320 120, 320 160), (320 160, 320 200), (320 200, 320 240), (320 240, 320 280), (320 280, 320 320), (320 320, 320 360), (320 360, 320 440), (360 30, 360 40), (360 40, 360 80), (360 80, 360 120), (360 120, 360 160), (360 160, 360 200), (360 200, 360 240), (360 240, 360 280), (360 280, 360 320), (360 320, 360 360), (360 360, 360 440), (30 40, 40 40), (40 40, 80 40), (80 40, 120 40), (120 40, 160 40), (160 40, 200 40), (200 40, 240 40), (240 40, 280 40), (280 40, 320 40), (320 40, 360 40), (360 40, 450 40), (30 80, 40 80), (40 80, 80 80), (80 80, 120 80), (120 80, 160 80), (160 80, 200 80), (200 80, 240 80), (240 80, 280 80), (280 80, 320 80), (320 80, 360 80), (360 80, 450 80), (30 120, 40 120), (40 120, 80 120), (80 120, 120 120), (120 120, 160 120), (160 120, 200 120), (200 120, 240 120), (240 120, 280 120), (280 120, 320 120), (320 120, 360 120), (360 120, 450 120), (30 160, 40 160), (40 160, 80 160), (80 160, 120 160), (120 160, 160 160), (160 160, 200 160), (200 160, 240 160), (240 160, 280 160), (280 160, 320 160), (320 160, 360 160), (360 160, 450 160), (30 200, 40 200), (40 200, 80 200), (80 200, 120 200), (120 200, 160 200), (160 200, 200 200), (200 200, 240 200), (240 200, 280 200), (280 200, 320 200), (320 200, 360 200), (360 200, 450 200), (30 240, 40 240), (40 240, 80 240), (80 240, 120 240), (120 240, 160 240), (160 240, 200 240), (200 240, 240 240), (240 240, 280 240), (280 240, 320 240), (320 240, 360 240), (360 240, 450 240), (30 280, 40 280), (40 280, 80 280), (80 280, 120 280), (120 280, 160 280), (160 280, 200 280), (200 280, 240 280), (240 280, 280 280), (280 280, 320 280), (320 280, 360 280), (360 280, 450 280), (30 320, 40 320), (40 320, 80 320), (80 320, 120 320), (120 320, 160 320), (160 320, 200 320), (200 320, 240 320), (240 320, 280 320), (280 320, 320 320), (320 320, 360 320), (360 320, 450 320), (30 360, 40 360), (40 360, 80 360), (80 360, 120 360), (120 360, 160 360), (160 360, 200 360), (200 360, 240 360), (240 360, 280 360), (280 360, 320 360), (320 360, 360 360), (360 360, 450 360))",
                nodedDedupedLinework.ToString());
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
@"LINEARRING (0.0000000000000000 -6.1026585365860800, 3.8000000000000000 -5.7690000000000000, 
7.9000000000000000 -5.4090000000000000, 12.0000000000000000 -5.3690000000000000, 
16.1000000000000000 -5.0990000000000000, 20.3000000000000000 -5.1390000000000000, 
24.5000000000000000 -4.9090000000000000, 28.5000000000000000 -4.6390000000000000, 
32.6000000000000000 -4.1890000000000000, 36.6000000000000000 -3.2890000000000000, 
40.8000000000000000 -3.1890000000000000, 44.8000000000000000 -2.9490000000000000, 
49.0000000000000000 -2.8200000000000000, 53.3000000000000000 -2.3600000000000000,
57.3000000000000000 -2.1400000000000000, 61.3000000000000000 -1.5200000000000000,
65.5000000000000000 -1.3900000000000000, 69.6000000000000000 -1.1700000000000000,
73.6000000000000000 -1.0100000000000000, 77.9000000000000000 -0.9700000000000000,
82.0000000000000000 -0.8000000000000000, 86.2000000000000000 -0.7400000000000000,
90.2000000000000000 -0.6700000000000000, 94.5000000000000000 -0.6000000000000000,
98.6000000000000000 -0.5000000000000000, 102.6000000000000000 -0.3900000000000000,
106.9000000000000000 -0.2400000000000000, 111.1000000000000000 -0.1500000000000000, 
115.1000000000000000 -0.1510000000000000, 119.2000000000000000 -0.1510000000000000, 
123.1000000000000000 -0.0510000000000000, 127.2000000000000000 0.0590000000000000,
131.2000000000000000 0.2090000000000000, 132.2000000000000000 1.0250000000000000,
132.6000000000000000 2.6400000000000000, 132.6000000000000000 2.6400000000000000,
132.6000000000000000 0.5000000000000000, 128.6000000000000000 0.5000000000000000,
106.2000000000000000 -2.3000000000000000, 49.9000000000000000 -2.3000000000000000,
31.0000000000000000 -5.4500000000000000, 0.0000000000000000 -5.4500000000000000, 
0.0000000000000000 -6.1026585365860800)");
            var g2 = reader.Read(
@"LINEARRING (0.0000000000000000 2.6400000000000000, 0.0000000000000000 -6.0030697674418800, 
0.3200000000000000 -5.9800000000000000, 4.6200000000000000 -5.6700000000000000, 
8.8100000000000000 -5.1600000000000000, 13.1000000000000000 -5.2100000000000000, 
17.1200000000000000 -4.8400000000000000, 21.2800000000000000 -5.1700000000000000, 
25.5000000000000000 -5.1600000000000000, 29.7100000000000000 -4.6700000000000000, 
34.1300000000000000 -3.9000000000000000, 38.2800000000000000 -3.4800000000000000,
42.6400000000000000 -3.3000000000000000, 46.9800000000000000 -3.2600000000000000, 
51.1600000000000000 -2.8900000000000000, 55.3200000000000000 -2.8300000000000000,
59.3000000000000000 -2.9200000000000000, 63.6800000000000000 -2.3600000000000000, 
67.4800000000000000 -2.5700000000000000, 70.8900000000000000 -2.4800000000000000, 
74.8900000000000000 -2.5500000000000000, 78.8800000000000000 -2.6900000000000000, 
82.4400000000000000 -2.4600000000000000, 87.5300000000000000 -2.8100000000000000, 
91.6200000000000000 -2.7500000000000000, 95.9000000000000000 -2.1000000000000000, 
100.2200000000000000 -2.2100000000000000, 104.4400000000000000 -2.3000000000000000,
109.1500000000000000 -1.9500000000000000, 113.2400000000000000 -2.8200000000000000,
117.2400000000000000 -1.9600000000000000, 120.9400000000000000 -1.3000000000000000,
125.0300000000000000 -0.8200000000000000, 126.6800000000000000 0.1300000000000000, 
132.5500000000000000 1.5400000000000000, 132.6000000000000000 2.6400000000000000, 
132.6000000000000000 2.6400000000000000, 132.6000000000000000 2.6400000000000000, 
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
@"POLYGON ((0.0000000000000000 -6.1026585365860800, 3.8000000000000000 -5.7690000000000000, 
7.9000000000000000 -5.4090000000000000, 12.0000000000000000 -5.3690000000000000, 
16.1000000000000000 -5.0990000000000000, 20.3000000000000000 -5.1390000000000000, 
24.5000000000000000 -4.9090000000000000, 28.5000000000000000 -4.6390000000000000, 
32.6000000000000000 -4.1890000000000000, 36.6000000000000000 -3.2890000000000000, 
40.8000000000000000 -3.1890000000000000, 44.8000000000000000 -2.9490000000000000, 
49.0000000000000000 -2.8200000000000000, 53.3000000000000000 -2.3600000000000000,
57.3000000000000000 -2.1400000000000000, 61.3000000000000000 -1.5200000000000000,
65.5000000000000000 -1.3900000000000000, 69.6000000000000000 -1.1700000000000000,
73.6000000000000000 -1.0100000000000000, 77.9000000000000000 -0.9700000000000000,
82.0000000000000000 -0.8000000000000000, 86.2000000000000000 -0.7400000000000000,
90.2000000000000000 -0.6700000000000000, 94.5000000000000000 -0.6000000000000000,
98.6000000000000000 -0.5000000000000000, 102.6000000000000000 -0.3900000000000000,
106.9000000000000000 -0.2400000000000000, 111.1000000000000000 -0.1500000000000000, 
115.1000000000000000 -0.1510000000000000, 119.2000000000000000 -0.1510000000000000, 
123.1000000000000000 -0.0510000000000000, 127.2000000000000000 0.0590000000000000,
131.2000000000000000 0.2090000000000000, 132.2000000000000000 1.0250000000000000,
132.6000000000000000 2.6400000000000000, 132.6000000000000000 2.6400000000000000,
132.6000000000000000 0.5000000000000000, 128.6000000000000000 0.5000000000000000,
106.2000000000000000 -2.3000000000000000, 49.9000000000000000 -2.3000000000000000,
31.0000000000000000 -5.4500000000000000, 0.0000000000000000 -5.4500000000000000, 
0.0000000000000000 -6.1026585365860800))");
            var isValidOp = new IsValidOp(g1);
            if (!isValidOp.IsValid)
            {
                Debug.WriteLine("g1 is not valid:" + isValidOp.ValidationError);
                g1 = g1.Buffer(0);
                Debug.WriteLine(g1.AsText());
            }
            var g2 = reader.Read(
@"POLYGON ((0.0000000000000000 2.6400000000000000, 0.0000000000000000 -6.0030697674418800, 
0.3200000000000000 -5.9800000000000000, 4.6200000000000000 -5.6700000000000000, 
8.8100000000000000 -5.1600000000000000, 13.1000000000000000 -5.2100000000000000, 
17.1200000000000000 -4.8400000000000000, 21.2800000000000000 -5.1700000000000000, 
25.5000000000000000 -5.1600000000000000, 29.7100000000000000 -4.6700000000000000, 
34.1300000000000000 -3.9000000000000000, 38.2800000000000000 -3.4800000000000000,
42.6400000000000000 -3.3000000000000000, 46.9800000000000000 -3.2600000000000000, 
51.1600000000000000 -2.8900000000000000, 55.3200000000000000 -2.8300000000000000,
59.3000000000000000 -2.9200000000000000, 63.6800000000000000 -2.3600000000000000, 
67.4800000000000000 -2.5700000000000000, 70.8900000000000000 -2.4800000000000000, 
74.8900000000000000 -2.5500000000000000, 78.8800000000000000 -2.6900000000000000, 
82.4400000000000000 -2.4600000000000000, 87.5300000000000000 -2.8100000000000000, 
91.6200000000000000 -2.7500000000000000, 95.9000000000000000 -2.1000000000000000, 
100.2200000000000000 -2.2100000000000000, 104.4400000000000000 -2.3000000000000000,
109.1500000000000000 -1.9500000000000000, 113.2400000000000000 -2.8200000000000000,
117.2400000000000000 -1.9600000000000000, 120.9400000000000000 -1.3000000000000000,
125.0300000000000000 -0.8200000000000000, 126.6800000000000000 0.1300000000000000, 
132.5500000000000000 1.5400000000000000, 132.6000000000000000 2.6400000000000000, 
132.6000000000000000 2.6400000000000000, 132.6000000000000000 2.6400000000000000, 
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
            var g1 = reader.Read(@"LINESTRING (0 -5.15, 30 -5.15, 48.9 -2, 105.2 -2, 127.6 0.8, 132.6 0.8)");
            var g2 = reader.Read(@"LINEARRING (15.325555555555553 0.8, 42.309375 0.8, 42.309375 -5.15, 15.325555555555553 -5.15, 15.325555555555553 0.8)");

            //Act
            var res = g1.Intersection(g2);

            //
            ToImage(1, g1, g2, res);

            // Assert
            Assert.That(res, Is.Not.Null);
            Debug.WriteLine(res.AsText());
        }

        [Test(Description = "GitHub Issue #120")]
        public void Roundtrip_serialization_of_a_feature_with_null_properties_fails()
        {
            // Arrange
            var f = new Feature(new NetTopologySuite.Geometries.Point(1, 1), null);
            var s = GeoJsonSerializer.Create(new GeometryFactory());

            // Act
            var f1 = SandD(s, f);
            s.NullValueHandling = NullValueHandling.Include;
            var f2 = SandD(s, f);

            // Assert
            Assert.That(f1, Is.Not.Null, "f1 != null");
            Assert.That(f2, Is.Not.Null, "f2 != null");

        }

        private static IFeature SandD(JsonSerializer s , IFeature f)
        {
            var sb = new StringBuilder();
            var jtw = new JsonTextWriter(new StringWriter(sb));
            s.Serialize(jtw, f);
            var jsonText = sb.ToString();

            Debug.WriteLine(jsonText);

            var jtr = new JsonTextReader(new StringReader(jsonText));
            var res = s.Deserialize<IFeature>(jtr);
            return res;
        }

        [Test(Description = "GitHub Issue #125")]
        public void Fixing_invalid_polygon_with_Buffer_0_returns_empty_polygon()
        {
            //arrange
            var rdr = new WKTReader();
            rdr.RepairRings = true;
            var poly = rdr.Read(
@"POLYGON ((1.4749999999994841 -5.15, 30 -5.15, 48.9 -2,
108.1997 -2, 130.25148787313435 0.75647348414179227, 
130.25148787313435 0.75647348414179161, 130 0.75, 
126.3 0.72, 122.9 0.7, 119.2 0.42, 115.2 0.45, 
111 0.29, 106.9 0.23, 102.8 0.2, 98.8 0.12, 94.8 0.04, 
90.7 -0.08, 86.5 -0.2, 82.4 -0.42, 78.3 -0.57, 74.1 -0.69,
69.9 -0.78, 65.8 -0.87, 61.7 -1.07, 57.7 -1.09, 53.7 -1.229,
49.5 -1.289, 45.3 -1.369, 41.2 -1.719, 37 -2.409, 32.8 -3.219,
28.6 -3.769, 24.5 -4.089, 20.4 -4.429, 16.3 -4.599, 12.1 -4.759,
8 -4.889, 4 -5.049, 1.4749999999994841 -5.15))");

            //act
            var gpr = new NetTopologySuite.Precision.GeometryPrecisionReducer(new PrecisionModel(1e10));
            //gpr.ChangePrecisionModel = true;
            //gpr.Pointwise = false;
            var poly1 = gpr.Reduce(poly);
            var poly2 = poly.Buffer(0);

            ToImage(0, poly, poly1, poly2);
            
            var isValidOp = new IsValidOp(poly);
            if (!isValidOp.IsValid)
            {
                Debug.WriteLine(isValidOp.ValidationError);

            }
            Debug.WriteLine(poly1.AsText());
            // assert
            //Assert.That(poly.IsValid, Is.True, "poly.IsValid");
            Assert.That(poly1.IsValid, Is.True, "poly1.IsValid");
            Assert.That(poly2, Is.Not.Null, "poly2 != null");

            //Known to fail
            //Assert.That(poly2.IsEmpty, Is.False, "poly2.IsEmpty");
        }

        static void ToImage(int nr, IGeometry geom1, IGeometry geom2, IGeometry geom3)
        {

            var gpw = new Windows.Forms.GraphicsPathWriter();

            var extent = geom1.EnvelopeInternal;
            if (geom2 != null)
                extent.ExpandToInclude(geom2.EnvelopeInternal);
            extent.ExpandBy(0.05 * extent.Width);

            using (var img = new Bitmap(ImageWidth, ImageHeight))
            {
                using (var gr = Graphics.FromImage(img))
                {
                    var at = CreateAffineTransformation(extent);
                    gr.Clear(Color.WhiteSmoke);
                    gr.SmoothingMode = SmoothingMode.AntiAlias;
                    //gr.Transform = CreateTransform(extent);

                    var gp1 = gpw.ToShape(at.Transform(geom1));
                    if (geom1 is IPolygonal)
                        gr.FillPath(new SolidBrush(Color.FromArgb(64, Color.Blue)), gp1);
                    gr.DrawPath(Pens.Blue, gp1);

                    var gp2 = gpw.ToShape(at.Transform(geom2));
                    if (geom2 is IPolygonal)
                        gr.FillPath(new SolidBrush(Color.FromArgb(64, Color.OrangeRed)), gp2);
                    gr.DrawPath(Pens.OrangeRed, gp2);

                    //at = CreateAffineTransformation(extent, ImageWidth);

                    var gp3 = gpw.ToShape(at.Transform(geom3));
                    if (geom3 is IPolygonal)
                        gr.FillPath(new SolidBrush(Color.FromArgb(64, Color.Gold)), gp3);
                    gr.DrawPath(Pens.Gold, gp3);


                }
                var path = System.IO.Path.ChangeExtension(System.IO.Path.GetTempFileName(), "png");
                img.Save(path, ImageFormat.Png);
                Console.WriteLine("Image for Test {0} written to {1}", nr, new Uri(path).AbsoluteUri);
            }
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

    }
}