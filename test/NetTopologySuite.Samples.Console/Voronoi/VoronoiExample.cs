using System;
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

            int i = 0;
            foreach (var voronoiCell in resultingDiagram.Geometries)
            {
                Console.WriteLine("Voronoi {0}: Contains {1}? {2}", i++, voronoiCell.UserData,
                    voronoiCell.Contains(factory.CreatePoint((Coordinate) voronoiCell.UserData)));
            }
        }
    }
}
