using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    public class Issue276
    {
        [Test, Description("LineSegment.LineIntersection return error"), Category("GitHub Issue #276")]
        public void TestLineIntersection()
        {
            var ls1 = new LineSegment(
                new CoordinateZ(35613471.6165017, 4257145.3061322933, 1108.411),
                new CoordinateZ(35613477.7705378, 4257160.5282227108, 1108.293));
            var ls2 = new LineSegment(
                new CoordinateZ(35613477.775057241, 4257160.5396535359, 1108.293),
                new CoordinateZ(35613479.856073894, 4257165.9236917039, 1108.263));

            var ptRes = ls1.LineIntersection(ls2);
            // reported previous result:
            // var ptExp = new Coordinate(35613477.774291642, 4257160.5403661141)
            var ptExp    = new Coordinate(35613477.77284154, 4257160.533921045);
            Assert.That(NtsGeometryServices.Instance.CoordinateEqualityComparer.Equals(ptRes, ptExp),
                $"expected: {ptExp}\r  result: {ptRes}");
        }
    }
}
