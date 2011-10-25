using System;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Pen = System.Windows.Media.Pen;
using Point = NetTopologySuite.Geometries.Point;
using WpfFontFamily = System.Windows.Media.FontFamily;
using WpfFontStyles = System.Windows.FontStyles;
using WpfPoint = System.Windows.Point;
using WpfGeometry = System.Windows.Media.Geometry;

namespace NetTopologySuite.Windows.Media.Test
{
    public class BasicTestMedia
    {
        [Test]
        public void TestFontGlypthReader()
        {
            var geom = FontGlyphReader.Read("NetTopologySuite", new WpfFontFamily("SansSerif"), WpfFontStyles.Normal,
                                            36, new WpfPoint(0, 0), GeometryFactory.Default);
            Assert.IsNotNull(geom);
            Assert.IsFalse(geom.IsEmpty);
            Assert.IsInstanceOf(typeof(IMultiPolygon), geom);
            Console.WriteLine(geom.ToString());
        }

        [Test]
        [Ignore]
        public void TestPolygonWithHoles()
        {
            const string wkt = "POLYGON((2 2, 2 98, 98 98, 98 2, 2 2), (5 90, 10 90, 10 95, 5 95, 5 90))";
            var geom = new WKTReader().Read(wkt);

            var gpw = new WpfGeometryWriter();
            var res = gpw.ToShape(geom);

            WpfGeometryToImage(res, "WPF-PolygonWithHoles.png");

            var reverse = WpfGeometryReader.Read(res, 0d, GeometryFactory.Default);
            //Assert.AreEqual(geom, reverse);
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
            var gf = GeometryFactory.Default;
            _multiPoint = gf.CreateMultiPoint(
                new []
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

        private static void PerformTest(NetTopologySuite.Windows.Media.IPointShapeFactory factory)
        {
            var gpw = new WpfGeometryWriter(new IdentityPointTransformation(), factory);
            var res = gpw.ToShape(_multiPoint);

            WpfGeometryToImage(res, string.Format("WPF-MultiPoint-{0}.png", factory));
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