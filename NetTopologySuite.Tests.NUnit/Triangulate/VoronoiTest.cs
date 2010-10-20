#define unbuffered
using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Triangulate.Quadedge;
using NetTopologySuite.Coordinates;
using NUnit.Framework;
using NetTopologySuite.Triangulate;

#if unbuffered
using coord = NetTopologySuite.Coordinates.Coordinate;
#else
using coord = NetTopologySuite.Coordinates.BufferedCoordinate;
#endif

namespace NetTopologySuite.Tests.NUnit.Triangulate
{
    ///<summary>Tests Delaunay Triangulation classes and Voronoi diagram builder</summary>
    [TestFixture]
    public class VoronoiTest
    {

        private readonly GeoAPI.IO.WellKnownText.IWktGeometryReader<coord> _reader =
            GeometryUtils.GeometryFactory.WktReader;

        [Test]
        public void TestSimple()
        {
            String wkt = "MULTIPOINT ((10 10), (20 70), (60 30), (80 70))";
            //String expected = "MULTILINESTRING ((70 180, 190 110), (30 150, 70 180), (30 150, 50 40), (50 40, 120 20), (190 110, 120 20), (120 20, 140 70), (190 110, 140 70), (130 140, 140 70), (130 140, 190 110), (70 180, 130 140), (80 100, 130 140), (70 180, 80 100), (30 150, 80 100), (50 40, 80 100), (80 100, 120 20), (80 100, 140 70))";
            //    runDelaunayEdges(wkt, expected);
            String expectedTri = "GEOMETRYCOLLECTION (POLYGON ((30 150, 50 40, 80 100, 30 150)), POLYGON ((30 150, 80 100, 70 180, 30 150)), POLYGON ((70 180, 80 100, 130 140, 70 180)), POLYGON ((70 180, 130 140, 190 110, 70 180)), POLYGON ((190 110, 130 140, 140 70, 190 110)), POLYGON ((190 110, 140 70, 120 20, 190 110)), POLYGON ((120 20, 140 70, 80 100, 120 20)), POLYGON ((120 20, 80 100, 50 40, 120 20)), POLYGON ((80 100, 140 70, 130 140, 80 100)))";
            RunVoronoi(wkt, true, expectedTri);
        }

        const double ComparisonTolerance = 1.0e-7;

        void RunVoronoi(String sitesWKT, Boolean computeTriangles, String expectedWKT)
        {
            IGeometry<coord> sites = _reader.Read(sitesWKT);
            //DelaunayTriangulationBuilder<coord> builder = new DelaunayTriangulationBuilder<coord>(GeometryUtils.GeometryFactory);
            //builder.SetSites(sites);

            //QuadEdgeSubdivision<coord> subdiv = builder.GetSubdivision();

            IGeometry<coord> result = null;
            if (computeTriangles)
            {
                //result = subdiv.GetVoronoiDiagram();
                VoronoiDiagramBuilder<Coordinate> vdg = new VoronoiDiagramBuilder<Coordinate>(GeometryUtils.GeometryFactory);
                vdg.SetSites(sites);
                result = vdg.GetDiagram();
            }
            else
            {
                //result = builder.getEdges(geomFact);
            }
            Console.WriteLine(result);

            IGeometry<coord> expectedEdges = _reader.Read(expectedWKT);
            result.Normalize();
            expectedEdges.Normalize();
            Assert.IsTrue(expectedEdges.EqualsExact(result, new Tolerance(ComparisonTolerance)));
        }
    }
}
