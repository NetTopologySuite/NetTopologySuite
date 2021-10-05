using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Triangulate;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    public class Issue151
    {
        [Test, Description("TopologyException when generating a VoronoiDiagram"), Author("hallsbyra")]
        public void Test1()
        {
            var i = new NetTopologySuite.NtsGeometryServices(GeometryOverlay.NG);
            var g = new WKTReader(i).Read(
                "POLYGON ((14.7119 201.6703, 74.2154 201.6703, 74.2154 166.6391, 14.7119 166.6391, 14.7119 201.6703))");
            Assert.That(g.IsValid);

            var v = new VoronoiDiagramBuilder();
            v.SetSites(g);

            Geometry res = null;
            Assert.That(() => res = v.GetDiagram(g.Factory), Throws.Nothing);
            Assert.That(res, Is.Not.Null);
        }

        [Test, Description("TopologyException when generating a VoronoiDiagram"), Author("airbreather")]
        public void Test2()
        {
            var g = new WKTReader().Read(
                "POLYGON ((10 10, 10 100, 40 100, 40 10, 10 10))");
            Assert.That(g.IsValid);

            var v = new VoronoiDiagramBuilder();
            v.SetSites(g);

            Geometry res = null;
            Assert.That(() => res = v.GetDiagram(g.Factory), Throws.Nothing);
            Assert.That(res, Is.Not.Null);
        }
    }
}
