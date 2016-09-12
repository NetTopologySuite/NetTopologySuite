using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;
using Brushes = System.Windows.Media.Brushes;
using Pen = System.Windows.Media.Pen;
using WpfFontFamily = System.Windows.Media.FontFamily;
using WpfFontStyles = System.Windows.FontStyles;
using WpfGeometry = System.Windows.Media.Geometry;
using WpfPoint = System.Windows.Point;

namespace NetTopologySuite.Windows.Media.Test
{
    public class BasicTestMedia
    {
        [Test]
        public void TestFontGlypthReader()
        {
            var geom = FontGlyphReader.Read("NetTopologySuite", new WpfFontFamily("SansSerif"), WpfFontStyles.Normal,
                                            36, new WpfPoint(0, 0), Geometries.GeometryFactory.Default);
            Assert.IsNotNull(geom);
            Assert.IsFalse(geom.IsEmpty);
            Assert.IsInstanceOf(typeof(IGeometryCollection), geom);
            Console.WriteLine(geom.ToString());
        }

        [Test]
        public void TestPolygonWithHoles()
        {
            const string wkt = "POLYGON((2 2, 2 98, 98 98, 98 2, 2 2), (5 90, 10 90, 10 95, 5 95, 5 90))";
            var geom = new WKTReader().Read(wkt);

            var gpw = new WpfStreamGeometryWriter();
            var res = gpw.ToShape(geom);

            WpfGeometryToImage(res, "WPF-PolygonWithHoles.png");

            var reverse = WpfGeometryReader.Read(res, 0d, Geometries.GeometryFactory.Default);
            Assert.AreEqual(geom, reverse);
        }

        [Test]
        public void TestReadTopologicallyInvalid()
        {
            // Create a collection of points for a polygon
            var pathSegments = new PathSegmentCollection();
            
            pathSegments.Add(new LineSegment(new WpfPoint(50, 100), true));
            pathSegments.Add(new LineSegment(new WpfPoint(85, 0), true));
            pathSegments.Add(new LineSegment(new WpfPoint(0, 65), true));
            pathSegments.Add(new LineSegment(new WpfPoint(100, 65), true));
            pathSegments.Add(new LineSegment(new WpfPoint(15, 0), true));

            var pathGeometry = new PathGeometry(new [] {new PathFigure(new WpfPoint(15, 0), pathSegments, true)}, 
                FillRule.EvenOdd, Transform.Identity);
            
            var geomEvenOdd = WpfGeometryReader.Read(pathGeometry, 0, Geometries.GeometryFactory.Default);
            Assert.IsTrue(geomEvenOdd.IsValid);
            Console.WriteLine("EvenOdd:\n{0}", geomEvenOdd.AsText());

            pathGeometry = new PathGeometry(new[] { new PathFigure(new WpfPoint(15, 0), pathSegments, true) },
                FillRule.Nonzero, Transform.Identity);
            var geomNonzero = WpfGeometryReader.Read(pathGeometry, 0, Geometries.GeometryFactory.Default);
            Console.WriteLine("NotNull:\n{0}", geomNonzero.AsText());
            Assert.IsTrue(geomNonzero.IsValid);

            Assert.IsFalse(geomEvenOdd.EqualsTopologically(geomNonzero));
        }

        [Test]
        public void TestMultiPoint()
        {
            PerformTest(new Dot());
            PerformTest(new Triangle(15));
            PerformTest(new Square(15));
            PerformTest(new Star(15));
            PerformTest(new X(15));
            PerformTest(new Circle(15));
            PerformTest(new Cross(15));
        }

        static BasicTestMedia()
        {
            var gf = Geometries.GeometryFactory.Default;
            _multiPoint = gf.CreateMultiPoint(
                new[]
                    {
                        new Coordinate(100*_random.NextDouble(), 100*_random.NextDouble()),
                        new Coordinate(100*_random.NextDouble(), 100*_random.NextDouble()),
                        new Coordinate(100*_random.NextDouble(), 100*_random.NextDouble()),
                        new Coordinate(100*_random.NextDouble(), 100*_random.NextDouble()),
                        new Coordinate(100*_random.NextDouble(), 100*_random.NextDouble())
                    });
        }

        private static readonly Random _random = new Random(55942);
        private static readonly IMultiPoint _multiPoint;

        private static void PerformTest(IPointToStreamGeometryFactory factory)
        {
            var gpw = new WpfStreamGeometryWriter(new IdentityPointTransformation(), factory);
            var res = gpw.ToShape(_multiPoint);

            WpfGeometryToImage(res, string.Format("WPF-MultiPoint-{0}.png", factory.GetType().Name));
        }

        private static void WpfGeometryToImage(WpfGeometry geom, string fileName)
        {
            GeometryDrawing gd = new GeometryDrawing(Brushes.Red, new Pen(Brushes.Brown, 2), geom);
            DrawingVisual dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawDrawing(gd);
            }
            RenderTargetBitmap rtb = new RenderTargetBitmap(100, 100, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);

            PngBitmapEncoder png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(rtb));
            using (Stream stm = File.Create(fileName))
            {
                png.Save(stm);
            }
        }
    }

    public class BasicTestMediaPath
    {
        [Test]
        public void TestFontGlypthReader()
        {
            var geom = FontGlyphReader.Read("NetTopologySuite", new WpfFontFamily("SansSerif"), WpfFontStyles.Normal,
                                            36, new WpfPoint(0, 0), Geometries.GeometryFactory.Default);
            Assert.IsNotNull(geom);
            Assert.IsFalse(geom.IsEmpty);
            Assert.IsInstanceOf(typeof(IGeometryCollection), geom);
            Console.WriteLine(geom.ToString());
        }


        [Test]
        public void TopologicallyValidReader()
        {
            var path = new PolygonWpfPathGeometry();
        }

        [Test]
        public void TestPolygonWithHoles()
        {
            const string wkt = "POLYGON((2 2, 2 98, 98 98, 98 2, 2 2), (5 90, 10 90, 10 95, 5 95, 5 90))";
            var geom = new WKTReader().Read(wkt);

            var gpw = new WpfPathGeometryWriter();
            var res = gpw.ToShape(geom);

            WpfGeometryToImage(res, "WPF-Path-PolygonWithHoles.png");

            var reverse = WpfGeometryReader.Read(res, 0d, Geometries.GeometryFactory.Default);
            Assert.AreEqual(geom, reverse);
        }

        [Test]
        public void TestMultiPoint()
        {
            PerformTest(new DotPath());
            PerformTest(new TrianglePath(15));
            PerformTest(new SquarePath(15));
            PerformTest(new StarPath(15));
            PerformTest(new XPath(15));
            PerformTest(new CirclePath(15));
            PerformTest(new CrossPath(15));
        }

        static BasicTestMediaPath()
        {
            var gf = Geometries.GeometryFactory.Default;
            _multiPoint = gf.CreateMultiPoint(
                new[]
                    {
                        new Coordinate(100*_random.NextDouble(), 100*_random.NextDouble()),
                        new Coordinate(100*_random.NextDouble(), 100*_random.NextDouble()),
                        new Coordinate(100*_random.NextDouble(), 100*_random.NextDouble()),
                        new Coordinate(100*_random.NextDouble(), 100*_random.NextDouble()),
                        new Coordinate(100*_random.NextDouble(), 100*_random.NextDouble())
                    });
        }

        private static readonly Random _random = new Random(55942);
        private static readonly IMultiPoint _multiPoint;

        private static void PerformTest(IPointToPathGeometryFactory factory)
        {
            var gpw = new WpfPathGeometryWriter(new IdentityPointTransformation(), factory);
            var res = gpw.ToShape(_multiPoint);

            WpfGeometryToImage(res, string.Format("WPF-Path-MultiPoint-{0}.png", factory.GetType().Name));
        }

        private static void WpfGeometryToImage(WpfGeometry geom, string fileName)
        {
            GeometryDrawing gd = new GeometryDrawing(Brushes.Red, new Pen(Brushes.Brown, 2), geom);
            DrawingVisual dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawDrawing(gd);
            }
            RenderTargetBitmap rtb = new RenderTargetBitmap(100, 100, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);

            PngBitmapEncoder png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(rtb));
            using (Stream stm = File.Create(fileName))
            {
                png.Save(stm);
            }
        }
    }

}