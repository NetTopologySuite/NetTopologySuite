using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay.Validate;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Overlay.Validate
{
    /// <summary>
    /// Tests for <see cref="OffsetPointGenerator"/>.
    /// </summary>
    /// <remarks>
    /// See https://github.com/NetTopologySuite/NetTopologySuite/issues/879 -
    /// <c>ComputeOffsetPoints</c> computed <c>dx</c> as <c>p1.X - p0.Y</c>
    /// instead of <c>p1.X - p0.X</c>.
    /// </remarks>
    public class OffsetPointGeneratorTest : GeometryTestCase
    {
        [Test]
        public void TestOffsetPointsForNonAxisAlignedSegment()
        {
            // Segment chosen so that p0.X != p0.Y, which exposes the bug where
            // dx was computed as (p1.X - p0.Y) instead of (p1.X - p0.X).
            var line = (LineString) Read("LINESTRING (0 5, 10 15)");
            var generator = new OffsetPointGenerator(line);

            var offsetPts = generator.GetPoints(1.0);

            Assert.That(offsetPts.Count, Is.EqualTo(2));

            double ux = 10.0 / System.Math.Sqrt(200);
            double uy = 10.0 / System.Math.Sqrt(200);
            var expectedLeft = new Coordinate(5 - uy, 10 + ux);
            var expectedRight = new Coordinate(5 + uy, 10 - ux);

            CheckEqualXY(expectedLeft, offsetPts[0]);
            CheckEqualXY(expectedRight, offsetPts[1]);
        }

        private static void CheckEqualXY(Coordinate expected, Coordinate actual)
        {
            const double tolerance = 1e-9;
            Assert.That(actual.X, Is.EqualTo(expected.X).Within(tolerance));
            Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(tolerance));
        }
    }
}
