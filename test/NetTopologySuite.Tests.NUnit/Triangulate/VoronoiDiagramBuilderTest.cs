using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Triangulate
{
    public class VoronoiDiagramBuilderTest : GeometryTestCase
    {
        [Test]
        public void TestClipEnvelope()
        {
            var sites = Read("MULTIPOINT ((50 100), (50 50), (100 50), (100 100))");
            var clip = Read("POLYGON ((0 0, 0 200, 200 200, 200 0, 0 0))");
            var voronoi = VoronoiDiagram(sites, clip);
            Assert.That(voronoi.EnvelopeInternal.Equals(clip.EnvelopeInternal));
        }

        private const double TRIANGULATION_TOLERANCE = 0.0;

        public static Geometry VoronoiDiagram(Geometry sitesGeom, Geometry clipGeom)
        {
            var builder = new VoronoiDiagramBuilder();
            builder.SetSites(sitesGeom);
            if (clipGeom != null)
                builder.ClipEnvelope = clipGeom.EnvelopeInternal;
            builder.Tolerance = TRIANGULATION_TOLERANCE;
            Geometry diagram = builder.GetDiagram(sitesGeom.Factory);
            return diagram;
        }
    }
}
