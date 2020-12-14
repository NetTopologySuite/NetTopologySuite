using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Shape.Random;
using NetTopologySuite.Triangulate;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.OverlayNG
{
    public class RandomPolygonBuilder
    {
        private readonly Random Rnd = new Random(17);

        public static Geometry Build(int npts)
        {
            var builder = new RandomPolygonBuilder(npts);
            return builder.CreatePolygon();
        }

        private readonly Envelope _extent = new Envelope(0, 100, 0, 100);
        private readonly GeometryFactory _geomFact = new GeometryFactory();
        private readonly int _numPoints;
        private readonly Geometry _voronoi;

        public RandomPolygonBuilder(int npts)
        {
            _numPoints = npts;
            var sites = RandomPoints(_extent, npts);
            _voronoi = VoronoiDiagram(sites, _extent);
        }

        public Geometry CreatePolygon()
        {
            var cellsSelect = Select(_voronoi, _numPoints / 2);
            var poly = cellsSelect.Union();
            return poly;
        }

        private Geometry Select(Geometry geoms, int n)
        {
            var selection = new List<Geometry>();
            // add all the geometries
            for (int i = 0; i < geoms.NumGeometries; i++)
            {
                selection.Add(geoms.GetGeometryN(i));
            }
            // toss out random ones to leave n
            while (selection.Count > n)
            {
                int index = (int)(selection.Count * Rnd.NextDouble());
                selection.RemoveAt(index);
            }
            return _geomFact.BuildGeometry(selection);
        }

        public Geometry RandomPoints(Envelope extent, int nPts)
        {
            var shapeBuilder = new RandomPointsBuilder(_geomFact);
            shapeBuilder.Extent = extent;
            shapeBuilder.NumPoints = nPts;
            return shapeBuilder.GetGeometry();
        }

        public Geometry VoronoiDiagram(Geometry sitesGeom, Envelope extent)
        {
            var builder = new VoronoiDiagramBuilder();
            builder.SetSites(sitesGeom);
            builder.ClipEnvelope = extent;
            builder.Tolerance = 0.0001;
            Geometry diagram = builder.GetDiagram(sitesGeom.Factory);
            return diagram;
        }
    }
}
