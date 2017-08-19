using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
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

        [Test]
        public void ConvertingARectangleWithAHoleProducesInvalidMultipolygon()
        {
            var sg = new StreamGeometry();
            using (var ctx = sg.Open())
            {
                ctx.BeginFigure(new WpfPoint(0, 0), true, true);
                ctx.LineTo(new WpfPoint(0, 10), true, false);
                ctx.LineTo(new WpfPoint(10, 10), true, false);
                ctx.LineTo(new WpfPoint(10, 0), true, false);
                ctx.LineTo(new WpfPoint(0, 0), true, false);
                ctx.BeginFigure(new WpfPoint(4, 4), true, true);
                ctx.LineTo(new WpfPoint(4, 6), true, false);
                ctx.LineTo(new WpfPoint(6, 6), true, false);
                ctx.LineTo(new WpfPoint(6, 4), true, false);
                ctx.LineTo(new WpfPoint(4, 4), true, false);
            }
            sg.Freeze();
            var area = sg.GetArea();

            var geometry = WpfGeometryReader.Read(sg, Geometries.GeometryFactory.Default);

            Assert.AreEqual(area, geometry.Area, 1e-4);  // Fails. Area is 104.
        }

        [Test]
        public void ConvertingTwoOverlappingRectanglesProducesInvalidMultipolygon()
        {
            var sg = new StreamGeometry();
            using (var ctx = sg.Open())
            {
                ctx.BeginFigure(new WpfPoint(0, 0), true, true);
                ctx.LineTo(new WpfPoint(0, 10), true, false);
                ctx.LineTo(new WpfPoint(10, 10), true, false);
                ctx.LineTo(new WpfPoint(10, 0), true, false);
                ctx.LineTo(new WpfPoint(0, 0), true, false);
                ctx.BeginFigure(new WpfPoint(5, 5), true, true);
                ctx.LineTo(new WpfPoint(5, 15), true, false);
                ctx.LineTo(new WpfPoint(15, 15), true, false);
                ctx.LineTo(new WpfPoint(15, 5), true, false);
                ctx.LineTo(new WpfPoint(5, 5), true, false);
            }
            sg.Freeze();
            var area = sg.GetArea();

            var geometry = WpfGeometryReader.Read(sg, Geometries.GeometryFactory.Default);

            Assert.AreEqual(area, geometry.Area, 1e-4);
        }

        [Test]
        public void ConvertingTwoTouchingRectanglesProducesInvalidMultipolygon()
        {
            var sg = new StreamGeometry();
            using (var ctx = sg.Open())
            {
                ctx.BeginFigure(new WpfPoint(0, 0), true, true);
                ctx.LineTo(new WpfPoint(0, 10), true, false);
                ctx.LineTo(new WpfPoint(10, 10), true, false);
                ctx.LineTo(new WpfPoint(10, 0), true, false);
                ctx.LineTo(new WpfPoint(0, 0), true, false);
                ctx.BeginFigure(new WpfPoint(10, 5), true, true);
                ctx.LineTo(new WpfPoint(10, 15), true, false);
                ctx.LineTo(new WpfPoint(20, 15), true, false);
                ctx.LineTo(new WpfPoint(20, 5), true, false);
                ctx.LineTo(new WpfPoint(10, 5), true, false);
            }
            sg.Freeze();
            var area = sg.GetArea();

            var geometry = WpfGeometryReader.Read(sg, Geometries.GeometryFactory.Default);

            Assert.AreEqual(area, geometry.Area, 1e-4);
        }

        [Test]
        public void ConvertingAClosedRectangleCrashes()
        {
            // Arrage
            var sg = new StreamGeometry();
            using (var ctx = sg.Open())
            {
                ctx.BeginFigure(new WpfPoint(0, 0), true, true);
                ctx.LineTo(new WpfPoint(0, 10), true, false);
                ctx.LineTo(new WpfPoint(10, 10), true, false);
                ctx.LineTo(new WpfPoint(10, 0), true, false);
            }
            sg.Freeze();

            // Act & assert
            IGeometry geometry = null;
            Assert.DoesNotThrow(() => geometry = WpfGeometryReader.Read(sg, Geometries.GeometryFactory.Default));

        }

        [Test]
        public void InvalidPolygon()
        {
            var wpfGeom = Geometry.Parse("M12.6206216812134, 50.9294242858887L11.5450315475464, 49.8538360595703 10.4694414138794, 50.9294242858887 11.5450315475464, 52.0050163269043 12.6206216812134, 50.9294242858887z M11.5870275497437, 55.99609375L10.0783662796021, 54.487434387207 8.56970500946045, 55.99609375 10.0783662796021, 57.5047569274902 11.5870275497437, 55.99609375z M5.77836227416992, 54.9871711730957L3.80277156829834, 56.962760925293 5.77836227416992, 58.9383506774902 7.7539529800415, 56.962760925293 5.77836227416992, 54.9871711730957z M5.51891326904297, 51.7981262207031L5.5027756690979, 51.99609375 6.71169662475586, 53.2050170898438 7.92061805725098, 51.99609375 6.74462604522705, 50.7876129150391 6.88728761672974, 50.0960960388184 5.14503145217896, 48.3538398742676 3.40277552604675, 50.0960960388184 5.14503145217896, 51.8383522033691 5.51891326904297, 51.7981262207031z M8.37836647033691, 45.7541007995605L6.53637409210205, 47.5960960388184 8.37836647033691, 49.4380874633789 10.2203578948975, 47.5960960388184 8.37836647033691, 45.7541007995605z M1.5868661403656, 45.6459007263184L1.5868661403656, 76.6044692993164 37.3742637634277, 76.6044692993164 37.3742637634277, 45.6459007263184 1.5868661403656, 45.6459007263184z");
            WpfGeometryToImage(wpfGeom, "InvalidPolygon.png");
            var reader = new WpfGeometryReader(Geometries.Geometry.DefaultFactory, true);
            
            IGeometry ntsGeom = null;
            Assert.DoesNotThrow( () => ntsGeom = reader.Read(wpfGeom));
            Debug.WriteLine(ntsGeom.AsText());
            var isValidOp = new Operation.Valid.IsValidOp(ntsGeom);
            if (!isValidOp.IsValid)
                Assert.IsTrue(false, isValidOp.ValidationError.ToString());

            Assert.AreEqual(wpfGeom.GetArea(), ntsGeom.Area, 1e-5);
        }

        [Test]
        public void OutOfMemoryPolygon()
        {
            var wpfGeom = Geometry.Parse("M0,5 L0,15 10,15 10,5 0,5 M0,0 L15,15 M5,0 L5,10 15,10 15,0 5,0");
            WpfGeometryToImage(wpfGeom, "OutOfMemory.png");
            var reader = new WpfGeometryReader(Geometries.Geometry.DefaultFactory, true);

            IGeometry ntsGeom = null;
            Assert.DoesNotThrow(() => ntsGeom = reader.Read(wpfGeom));
            Debug.WriteLine(ntsGeom.AsText());
            var isValidOp = new Operation.Valid.IsValidOp(ntsGeom);
            if (!isValidOp.IsValid)
                Assert.IsTrue(false, isValidOp.ValidationError.ToString());

            Assert.AreEqual(wpfGeom.GetArea(), ntsGeom.Area, 1e-5);
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

        private static readonly Size BitmapSize = new Size(300, 300);

        private static void WpfGeometryToImage(WpfGeometry geom, string fileName)
        {
            var tmp = geom;

            var pen = new Pen(Brushes.Brown, 2);
            var bounds = tmp.GetRenderBounds(pen);
            var scaleX = (BitmapSize.Width - 4)/bounds.Width;
            var scaleY = (BitmapSize.Height - 4)/bounds.Height;

            var scale = Math.Min(scaleX, scaleY);
            var tmpMat = new MatrixTransform(scale, 0, 0, scale, -bounds.Left*scale, -bounds.Top*scale);

            //tmp.Transform = tmpMat;
            
            GeometryDrawing gd = new GeometryDrawing(Brushes.Red, new Pen(Brushes.Brown, 2d/scale), geom);
            DrawingVisual dv = new DrawingVisual();
            //dv.Transform = tmpMat;
            using (var dc = dv.RenderOpen())
            {
                //dc.PushTransform(MatrixTransform);
                dc.DrawRectangle(Brushes.White, null, new Rect(new Point(0, 0), BitmapSize));
                dv.Transform = tmpMat;
                dc.DrawDrawing(gd);
            }
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)BitmapSize.Width, (int)BitmapSize.Height, 96, 96, PixelFormats.Pbgra32);
            
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