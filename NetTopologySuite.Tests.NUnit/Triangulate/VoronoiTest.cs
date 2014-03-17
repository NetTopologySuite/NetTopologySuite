using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Triangulate;
using NetTopologySuite.Triangulate.QuadEdge;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Triangulate
{
    /// <summary>
    /// Tests Voronoi diagram generation
    /// </summary>
    [TestFixtureAttribute]
    public class VoronoiTest
    {
        [TestAttribute]
        public void TestSimple()
        {
            const string wkt = "MULTIPOINT ((10 10), (20 70), (60 30), (80 70))";
            const string expected = "GEOMETRYCOLLECTION (POLYGON ((-1162.076359832636 462.66344142259413, 50 419.375, 50 60, 27.857142857142854 37.857142857142854, -867 187, -1162.076359832636 462.66344142259413)), POLYGON ((-867 187, 27.857142857142854 37.857142857142854, 245 -505, 45 -725, -867 187)), POLYGON ((27.857142857142854 37.857142857142854, 50 60, 556.6666666666666 -193.33333333333331, 245 -505, 27.857142857142854 37.857142857142854)), POLYGON ((50 60, 50 419.375, 1289.1616314199396 481.3330815709969, 556.6666666666666 -193.33333333333331, 50 60)))";
            RunVoronoi(wkt, expected);
        }

        private const double ComparisonTolerance = 1.0e-7;

        private static void RunVoronoi(string sitesWKT, string expectedWKT)
        {
            WKTReader reader = new WKTReader();
            IGeometry sites = reader.Read(sitesWKT);

            DelaunayTriangulationBuilder builder = new DelaunayTriangulationBuilder();
            builder.SetSites(sites);

            QuadEdgeSubdivision subdiv = builder.GetSubdivision();
            IGeometry result = subdiv.GetVoronoiDiagram(GeometryFactory.Default);
            Assert.IsNotNull(result);

            IGeometry expectedEdges = reader.Read(expectedWKT);
            result.Normalize();
            expectedEdges.Normalize();
            Assert.IsTrue(expectedEdges.EqualsExact(result, ComparisonTolerance));
        }
    }
}
