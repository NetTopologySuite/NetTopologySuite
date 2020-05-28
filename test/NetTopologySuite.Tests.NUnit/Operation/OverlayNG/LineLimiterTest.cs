using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.OverlayNg;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace NetTopologySuite.Tests.NUnit.Operation.OverlayNG
{
    [TestFixture]
    public class LineLimiterTest : GeometryTestCase
    {


        [Test]
        public void TestEmptyEnv()
        {
            CheckLimit(
                "LINESTRING (5 15, 5 25, 25 25, 25 5, 5 5)",
                new Envelope(),
                "MULTILINESTRING EMPTY"
                );
        }

        [Test]
        public void TestPointEnv()
        {
            CheckLimit(
                "LINESTRING (5 15, 5 25, 25 25, 25 5, 5 5)",
                new Envelope(10, 10, 10, 10),
                "MULTILINESTRING EMPTY"
                );
        }

        [Test]
        public void TestNonIntersecting()
        {
            CheckLimit(
                "LINESTRING (5 15, 5 25, 25 25, 25 5, 5 5)",
                new Envelope(10, 20, 10, 20),
                "MULTILINESTRING EMPTY"
                );
        }

        [Test]
        public void TestPartiallyInside()
        {
            CheckLimit(
                "LINESTRING (4 17, 8 14, 12 18, 15 15)",
                new Envelope(10, 20, 10, 20),
                "LINESTRING (8 14, 12 18, 15 15)"
                );
        }

        [Test]
        public void TestCrossing()
        {
            CheckLimit(
                "LINESTRING (5 17, 8 14, 12 18, 15 15, 18 18, 22 14, 25 18)",
                new Envelope(10, 20, 10, 20),
                "LINESTRING (8 14, 12 18, 15 15, 18 18, 22 14)"
                );
        }

        [Test]
        public void TestCrossesTwice()
        {
            CheckLimit(
                "LINESTRING (7 17, 23 17, 23 13, 7 13)",
                new Envelope(10, 20, 10, 20),
                "MULTILINESTRING ((7 17, 23 17), (23 13, 7 13))"
                );
        }

        [Test]
        public void TestDiamond()
        {
            CheckLimit(
                "LINESTRING (8 15, 15 22, 22 15, 15 8, 8 15)",
                new Envelope(10, 20, 10, 20),
                "LINESTRING (8 15, 15 8, 22 15, 15 22, 8 15)"
                );
        }

        [Test]
        public void TestOctagon()
        {
            CheckLimit(
                "LINESTRING (9 12, 12 9, 18 9, 21 12, 21 18, 18 21, 12 21, 9 18, 9 13)",
                new Envelope(10, 20, 10, 20),
                "MULTILINESTRING ((9 12, 12 9), (18 9, 21 12), (21 18, 18 21), (12 21, 9 18))"
                );
        }

        private void CheckLimit(string wkt, string wktBox, string wktExpected)
        {
            var box = Read(wktBox);
            var clipEnv = box.EnvelopeInternal;
            CheckLimit(wkt, clipEnv, wktExpected);
        }

        private void CheckLimit(string wkt, Envelope clipEnv, string wktExpected)
        {
            var line = Read(wkt);
            var expected = Read(wktExpected);

            var limiter = new LineLimiter(clipEnv);
            var sections = limiter.Limit(line.Coordinates);

            var result = ToLines(sections, line.Factory);
            CheckEqual(expected, result);
        }

        private static Geometry ToLines(IList<Coordinate[]> sections, GeometryFactory factory)
        {
            var lines = new LineString[sections.Count];
            int i = 0;
            foreach (var pts in sections)
            {
                lines[i++] = factory.CreateLineString(pts);
            }
            if (lines.Length == 1) return lines[0];
            return factory.CreateMultiLineString(lines);
        }
    }
}
