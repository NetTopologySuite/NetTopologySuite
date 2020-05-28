using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.OverlayNg;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.OverlayNG
{
    public class RingClipperTest : GeometryTestCase
    {
        [Test]public void TestEmptyEnv()
        {
            CheckClip(
                "POLYGON ((2 9, 7 27, 26 34, 45 10, 26 9, 17 -7, 14 4, 2 9))",
                new Envelope(),
                "LINESTRING EMPTY"
                );
        }

        [Test]public void TestPointEnv()
        {
            CheckClip(
                "POLYGON ((2 9, 7 27, 26 34, 45 10, 26 9, 17 -7, 14 4, 2 9))",
                new Envelope(10, 10, 10, 10),
                "LINESTRING EMPTY"
                );
        }

        [Test]public void TestClipCompletely()
        {
            CheckClip(
                "POLYGON ((2 9, 7 27, 26 34, 45 10, 26 9, 17 -7, 14 4, 2 9))",
                new Envelope(10, 20, 10, 20),
                "LINESTRING (10 20, 20 20, 20 10, 10 10, 10 20)"
                );
        }

        [Test]public void TestInside()
        {
            CheckClip(
                "POLYGON ((12 13, 13 17, 18 17, 15 16, 17 12, 14 14, 12 13))",
                new Envelope(10, 20, 10, 20),
                "LINESTRING (12 13, 13 17, 18 17, 15 16, 17 12, 14 14, 12 13)"
                );
        }

        [Test]public void TestStarClipped()
        {
            CheckClip(
                "POLYGON ((7 15, 12 18, 15 23, 18 18, 24 15, 18 12, 15 7, 12 12, 7 15))",
                new Envelope(10, 20, 10, 20),
                "LINESTRING (10 16.8, 12 18, 13.2 20, 16.8 20, 18 18, 20 17, 20 13, 18 12, 16.8 10, 13.2 10, 12 12, 10 13.2, 10 16.8)"
                );
        }

        [Test]public void TestWrapPartial()
        {
            CheckClip(
                "POLYGON ((30 60, 60 60, 40 80, 40 110, 110 110, 110 80, 90 60, 120 60, 120 120, 30 120, 30 60))",
                new Envelope(50, 100, 50, 100),
                "LINESTRING (50 60, 60 60, 50 70, 50 100, 100 100, 100 70, 90 60, 100 60, 100 100, 50 100, 50 60)"
                );
        }

        [Test]public void TestWrapAllSides()
        {
            CheckClip(
                "POLYGON ((30 80, 60 80, 60 90, 40 90, 40 110, 110 110, 110 40, 40 40, 40 59, 60 59, 60 70, 30 70, 30 30, 120 30, 120 120, 30 120, 30 80))",
                new Envelope(50, 100, 50, 100),
                "LINESTRING (50 80, 60 80, 60 90, 50 90, 50 100, 100 100, 100 50, 50 50, 50 59, 60 59, 60 70, 50 70, 50 50, 100 50, 100 100, 50 100, 50 80)"
                );
        }

        [Test]public void TestWrapOverlap()
        {
            CheckClip(
                "POLYGON ((30 80, 60 80, 60 90, 40 90, 40 110, 110 110, 110 40, 40 40, 40 59, 30 70, 20 100, 10 100, 10 30, 120 30, 120 120, 30 120, 30 80))",
                new Envelope(50, 100, 50, 100),
                "LINESTRING (50 80, 60 80, 60 90, 50 90, 50 100, 100 100, 100 50, 50 50, 100 50, 100 100, 50 100, 50 80)"
                );
        }

        private void CheckClip(string wkt, string wktBox, string wktExpected)
        {
            var box = Read(wktBox);
            var clipEnv = box.EnvelopeInternal;
            CheckClip(wkt, clipEnv, wktExpected);
        }

        private void CheckClip(string wkt, Envelope clipEnv, string wktExpected)
        {
            var line = Read(wkt);
            var expected = Read(wktExpected);

            var clipper = new RingClipper(clipEnv);
            var pts = clipper.Clip(line.Coordinates);

            var result = line.Factory.CreateLineString(pts);
            CheckEqual(expected, result);
        }
    }
}
