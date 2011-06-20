using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Triangulate;
using NetTopologySuite.Coordinates;
using NUnit.Framework;
#if unbuffered
using coord = NetTopologySuite.Coordinates.Simple.Coordinate;
#else
using coord = NetTopologySuite.Coordinates.BufferedCoordinate;
#endif

namespace NetTopologySuite.Tests.Triangulate
{
    /**
     * Tests Delaunay Triangulatin classes
     * 
     */
    [TestFixture]
    public class DelaunayTest
    {

        private readonly GeoAPI.IO.WellKnownText.IWktGeometryReader<coord> _reader =
            TestFactories.GeometryFactory.WktReader;

        [Test]
        public void TestRandom()
        {
            String wkt = "MULTIPOINT ((50 40), (140 70), (80 100), (130 140), (30 150), (70 180), (190 110), (120 20))";
            String expected = "MULTILINESTRING ((70 180, 190 110), (30 150, 70 180), (30 150, 50 40), (50 40, 120 20), (190 110, 120 20), (120 20, 140 70), (190 110, 140 70), (130 140, 140 70), (130 140, 190 110), (70 180, 130 140), (80 100, 130 140), (70 180, 80 100), (30 150, 80 100), (50 40, 80 100), (80 100, 120 20), (80 100, 140 70))";
            runDelaunayEdges(wkt, expected);
            String expectedTri = "GEOMETRYCOLLECTION (POLYGON ((30 150, 50 40, 80 100, 30 150)), POLYGON ((30 150, 80 100, 70 180, 30 150)), POLYGON ((70 180, 80 100, 130 140, 70 180)), POLYGON ((70 180, 130 140, 190 110, 70 180)), POLYGON ((190 110, 130 140, 140 70, 190 110)), POLYGON ((190 110, 140 70, 120 20, 190 110)), POLYGON ((120 20, 140 70, 80 100, 120 20)), POLYGON ((120 20, 80 100, 50 40, 120 20)), POLYGON ((80 100, 140 70, 130 140, 80 100)))";
            runDelaunay(wkt, true, expectedTri);
        }

        [Test]
        public void TestGrid()
        {
            String wkt = "MULTIPOINT ((10 10), (10 20), (20 20), (20 10), (20 0), (10 0), (0 0), (0 10), (0 20))";
            String expected = "MULTILINESTRING ((10 20, 20 20), (0 20, 10 20), (0 10, 0 20), (0 0, 0 10), (0 0, 10 0), (10 0, 20 0), (20 0, 20 10), (20 10, 20 20), (10 20, 20 10), (10 10, 20 10), (10 10, 10 20), (10 10, 0 20), (10 10, 0 10), (10 0, 10 10), (0 10, 10 0), (10 10, 20 0))";
            runDelaunayEdges(wkt, expected);
            String expectedTri = "GEOMETRYCOLLECTION (POLYGON ((0 20, 0 10, 10 10, 0 20)), POLYGON ((0 20, 10 10, 10 20, 0 20)), POLYGON ((10 20, 10 10, 20 10, 10 20)), POLYGON ((10 20, 20 10, 20 20, 10 20)), POLYGON ((10 0, 20 0, 10 10, 10 0)), POLYGON ((10 0, 10 10, 0 10, 10 0)), POLYGON ((10 0, 0 10, 0 0, 10 0)), POLYGON ((10 10, 20 0, 20 10, 10 10)))";
            runDelaunay(wkt, true, expectedTri);
        }

        [Test]
        public void TestCircle()
        {
            String wkt = "POLYGON ((42 30, 41.96 29.61, 41.85 29.23, 41.66 28.89, 41.41 28.59, 41.11 28.34, 40.77 28.15, 40.39 28.04, 40 28, 39.61 28.04, 39.23 28.15, 38.89 28.34, 38.59 28.59, 38.34 28.89, 38.15 29.23, 38.04 29.61, 38 30, 38.04 30.39, 38.15 30.77, 38.34 31.11, 38.59 31.41, 38.89 31.66, 39.23 31.85, 39.61 31.96, 40 32, 40.39 31.96, 40.77 31.85, 41.11 31.66, 41.41 31.41, 41.66 31.11, 41.85 30.77, 41.96 30.39, 42 30))";
            String expected = "MULTILINESTRING ((41.66 31.11, 41.85 30.77), (41.41 31.41, 41.66 31.11), (41.11 31.66, 41.41 31.41), (40.77 31.85, 41.11 31.66), (40.39 31.96, 40.77 31.85), (40 32, 40.39 31.96), (39.61 31.96, 40 32), (39.23 31.85, 39.61 31.96), (38.89 31.66, 39.23 31.85), (38.59 31.41, 38.89 31.66), (38.34 31.11, 38.59 31.41), (38.15 30.77, 38.34 31.11), (38.04 30.39, 38.15 30.77), (38 30, 38.04 30.39), (38 30, 38.04 29.61), (38.04 29.61, 38.15 29.23), (38.15 29.23, 38.34 28.89), (38.34 28.89, 38.59 28.59), (38.59 28.59, 38.89 28.34), (38.89 28.34, 39.23 28.15), (39.23 28.15, 39.61 28.04), (39.61 28.04, 40 28), (40 28, 40.39 28.04), (40.39 28.04, 40.77 28.15), (40.77 28.15, 41.11 28.34), (41.11 28.34, 41.41 28.59), (41.41 28.59, 41.66 28.89), (41.66 28.89, 41.85 29.23), (41.85 29.23, 41.96 29.61), (41.96 29.61, 42 30), (41.96 30.39, 42 30), (41.85 30.77, 41.96 30.39), (41.66 31.11, 41.96 30.39), (41.96 30.39, 41.41 31.41), (41.96 29.61, 41.96 30.39), (41.96 29.61, 41.41 31.41), (41.41 28.59, 41.96 29.61), (41.41 31.41, 41.41 28.59), (38.59 31.41, 41.41 28.59), (38.59 31.41, 41.41 31.41), (40.39 31.96, 38.59 31.41), (41.41 31.41, 40.39 31.96), (40.39 31.96, 41.11 31.66), (38.59 31.41, 39.61 31.96), (39.61 31.96, 40.39 31.96), (38.59 28.59, 41.41 28.59), (38.59 31.41, 38.59 28.59), (38.04 30.39, 38.59 28.59), (38.04 30.39, 38.59 31.41), (38.04 30.39, 38.34 31.11), (38.04 29.61, 38.59 28.59), (38.04 30.39, 38.04 29.61), (40.39 28.04, 41.41 28.59), (38.59 28.59, 40.39 28.04), (39.61 28.04, 40.39 28.04), (38.59 28.59, 39.61 28.04), (38.89 28.34, 39.61 28.04), (41.66 28.89, 41.96 29.61), (40.39 28.04, 41.11 28.34), (38.04 29.61, 38.34 28.89), (38.89 31.66, 39.61 31.96))";
            runDelaunayEdges(wkt, expected);
        }

        [Test]
        public void TestPolygonWithChevronHoles()
        {
            String wkt = "POLYGON ((0 0, 0 200, 180 200, 180 0, 0 0), (20 180, 160 180, 160 20, 152.625 146.75, 20 180), (30 160, 150 30, 70 90, 30 160))";
            String expected = "MULTILINESTRING ((0 200, 180 200), (0 0, 0 200), (0 0, 180 0), (180 200, 180 0), (152.625 146.75, 180 0), (152.625 146.75, 180 200), (152.625 146.75, 160 180), (160 180, 180 200), (0 200, 160 180), (20 180, 160 180), (0 200, 20 180), (20 180, 30 160), (30 160, 0 200), (0 0, 30 160), (30 160, 70 90), (0 0, 70 90), (70 90, 150 30), (150 30, 0 0), (150 30, 160 20), (0 0, 160 20), (160 20, 180 0), (152.625 146.75, 160 20), (150 30, 152.625 146.75), (70 90, 152.625 146.75), (30 160, 152.625 146.75), (30 160, 160 180))";
            runDelaunayEdges(wkt, expected);
        }

        const double ComparisonTolerance = 1.0e-7;

        void runDelaunayEdges(String sitesWKT, String expectedWKT)
        {
            runDelaunay(sitesWKT, false, expectedWKT);
        }

        void runDelaunay(String sitesWKT, Boolean computeTriangles, String expectedWKT)
        {
            IGeometry<coord> sites = _reader.Read(sitesWKT);
            DelaunayTriangulationBuilder<coord> builder = new DelaunayTriangulationBuilder<coord>(TestFactories.GeometryFactory);
            builder.SetSites(sites);

            IGeometry<coord> result;
            if (computeTriangles)
            {
                result = builder.GetTriangles();
            }
            else
            {
                result = builder.GetEdges();
            }
            Console.WriteLine(result);

            IGeometry<coord> expectedEdges = _reader.Read(expectedWKT);
            result.Normalize();
            expectedEdges.Normalize();
            Assert.IsTrue(expectedEdges.EqualsExact(result, new Tolerance(ComparisonTolerance)));
        }
    }
}
