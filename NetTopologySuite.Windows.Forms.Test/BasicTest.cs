using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Windows.Forms.Test
{
    public class BasicTestForms
    {
        [Test]
        public void TestFontGlypthReader()
        {
            var geom = FontGlyphReader.Read("NetTopologySuite", new FontFamily(GenericFontFamilies.SansSerif), FontStyle.Regular,
                                            36, new PointF(0, 0), GeometryFactory.Default);
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

            var gpw = new GraphicsPathWriter();
            var res = gpw.ToShape(geom);

            var b = new Bitmap(100, 100);
            using (var g = Graphics.FromImage(b))
            {
                //g.Transform = new Matrix(1f, 0f, -0f, -1f, 0f, 100f);
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.Clear(Color.White);
                g.FillPath(new SolidBrush(Color.FromArgb(180, Color.Red)), res);
                g.DrawPath(Pens.Red, res);
            }
            b.Save("PolygonWithHoles.png", ImageFormat.Png);

            var reverse = GraphicsPathReader.Read(res, 0d, GeometryFactory.Default);
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

        static BasicTestForms()
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

        private static void PerformTest(IPointShapeFactory factory)
        {
            var gpw = new GraphicsPathWriter(new IdentityPointTransformation(), factory);
            var res = gpw.ToShape(_multiPoint);

            var b = new Bitmap(100, 100);
            using (var g = Graphics.FromImage(b))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.Clear(Color.White);
                g.FillPath(new SolidBrush(Color.FromArgb(180, Color.Red)), res);
                g.DrawPath(Pens.Red, res);
            }
            b.Save(string.Format("MultiPoint-{0}.png", factory), ImageFormat.Png);
            

        }
    
    }
}