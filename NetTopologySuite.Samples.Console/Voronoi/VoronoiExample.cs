using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Voronoi
{
    public class VoronoiExample
    {
        [Test]
        public void TestVoronoi()
        {
            var bmp = new Bitmap(1000, 650);
            var gr = Graphics.FromImage(bmp);
            gr.SmoothingMode = SmoothingMode.AntiAlias;
            gr.CompositingQuality = CompositingQuality.HighQuality;

            var blackPen = new Pen(Color.Black, 3);

            var Coords = new Coordinate[5];
            Coords[0] = new Coordinate(200, 200);
            Coords[1] = new Coordinate(400, 400);
            Coords[2] = new Coordinate(600, 600);
            Coords[3] = new Coordinate(800, 300);
            Coords[4] = new Coordinate(430, 200);

            var voronoiDiagram = new VoronoiDiagramBuilder();
            voronoiDiagram.SetSites(Coords);

            var factory = new GeometryFactory();
            var resultingDiagram = voronoiDiagram.GetDiagram(factory);

            // draw cells
            var i = 0;
            foreach (var voronoiCell in resultingDiagram.Geometries)
            {
                var points = new List<System.Drawing.Point>();
                foreach (var coordinate in voronoiCell.Coordinates)
                {
                    points.Add(new System.Drawing.Point((int)coordinate.X, (int)coordinate.Y));
                }

                gr.DrawPolygon(blackPen, points.ToArray());

                Console.WriteLine("Voronoi {0}: Contains {1}? {2}", i++, voronoiCell.UserData,
                    voronoiCell.Contains(factory.CreatePoint((Coordinate) voronoiCell.UserData)));

                
            }

            // draw sites
            var blueVioletBrush = new SolidBrush(Color.BlueViolet);
            foreach (Coordinate C in Coords)
            {
                gr.FillEllipse(blueVioletBrush, (float)C.X, (float)C.Y, 20, 20);
            }

            var path = new Uri(Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetTempFileName(), "png")));
            bmp.Save(path.LocalPath);
            Console.WriteLine(path.AbsoluteUri);

            blueVioletBrush.Dispose();
            blackPen.Dispose();
            bmp.Dispose();
            gr.Dispose();

        }
    }
}