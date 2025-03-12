using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.RelateNG;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace NetTopologySuite.Tests.NUnit.Operation.RelateNG
{
    internal class LinearBoundaryTest : GeometryTestCase
    {
        [Test]
        public void TestLineMod2()
        {
            CheckLinearBoundary("LINESTRING (0 0, 9 9)",
                BoundaryNodeRules.Mod2BoundaryRule,
                "MULTIPOINT((0 0), (9 9))");
        }

        [Test]
        public void TestLines2Mod2()
        {
            CheckLinearBoundary("MULTILINESTRING ((0 0, 9 9), (9 9, 5 1))",
                BoundaryNodeRules.Mod2BoundaryRule,
                "MULTIPOINT((0 0), (5 1))");
        }

        [Test]
        public void TestLines3Mod2()
        {
            CheckLinearBoundary("MULTILINESTRING ((0 0, 9 9), (9 9, 5 1), (9 9, 1 5))",
                BoundaryNodeRules.Mod2BoundaryRule,
                "MULTIPOINT((0 0), (5 1), (1 5), (9 9))");
        }

        [Test]
        public void TestLines3Monvalent()
        {
            CheckLinearBoundary("MULTILINESTRING ((0 0, 9 9), (9 9, 5 1), (9 9, 1 5))",
                BoundaryNodeRules.MonoValentEndpointBoundaryRule,
                "MULTIPOINT((0 0), (5 1), (1 5))");
        }

        private void CheckLinearBoundary(string wkt, IBoundaryNodeRule bnr, string wktBdyExpected)
        {
            var geom = Read(wkt);
            var lb = new LinearBoundary(ExtractLines(geom), bnr);
            bool hasBoundaryExpected = wktBdyExpected == null ? false : true;
            Assert.That(lb.HasBoundary, Is.EqualTo(hasBoundaryExpected), "HasBoundary");

            CheckBoundaryPoints(lb, geom, wktBdyExpected);
        }

        private void CheckBoundaryPoints(LinearBoundary lb, Geometry geom, string wktBdyExpected)
        {
            var bdySet = ExtractPoints(wktBdyExpected);

            foreach (var p in bdySet)
            {
                Assert.That(lb.IsBoundary(p));
            }

            var allPts = geom.Coordinates;
            foreach (var p in allPts)
            {
                if (!bdySet.Contains(p))
                {
                    Assert.That(lb.IsBoundary(p), Is.False);
                }
            }
        }

        private ISet<Coordinate> ExtractPoints(string wkt)
        {
            var ptSet = new HashSet<Coordinate>();
            if (wkt == null) return ptSet;
            var pts = Read(wkt).Coordinates;
            foreach (var p in pts)
            {
                ptSet.Add(p);
            }
            return ptSet;
        }

        private IEnumerable<LineString> ExtractLines(Geometry geom)
        {
            return LineStringExtracter.GetLines(geom).Cast<LineString>();
        }

    }
}
