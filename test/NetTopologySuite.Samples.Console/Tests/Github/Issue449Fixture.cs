using System;

using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;

using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    [TestFixture]
    [Category("GitHub Issue")]
    [Category("GitHub Issue #449")]
    public sealed class Issue449Fixture
    {
        [Test]
        public void TestReleaseCoordinateArrayForFloat()
        {
            float[] rawData = { 1, 2, 3, 4 };
            var seq = new PackedFloatCoordinateSequence(rawData, 2, 0);
            rawData = seq.GetRawCoordinates();

            var originalCoords = seq.ToCoordinateArray();
            Assert.That(originalCoords, Is.EqualTo(new[] { new Coordinate(1, 2), new Coordinate(3, 4) }));

            Array.Reverse(rawData);
            seq.ReleaseCoordinateArray();
            var modifiedCoords = seq.ToCoordinateArray();
            Assert.That(modifiedCoords, Is.EqualTo(new[] { new Coordinate(4, 3), new Coordinate(2, 1) }));

            // ensure that this test doesn't "accidentally" pass just because GC happens to run.
            GC.KeepAlive(originalCoords);
        }

        [Test]
        public void TestReleaseCoordinateArrayForDouble()
        {
            double[] rawData = { 1, 2, 3, 4 };
            var seq = new PackedDoubleCoordinateSequence(rawData, 2, 0);
            rawData = seq.GetRawCoordinates();

            var originalCoords = seq.ToCoordinateArray();
            Assert.That(originalCoords, Is.EqualTo(new[] { new Coordinate(1, 2), new Coordinate(3, 4) }));

            Array.Reverse(rawData);
            seq.ReleaseCoordinateArray();
            var modifiedCoords = seq.ToCoordinateArray();
            Assert.That(modifiedCoords, Is.EqualTo(new[] { new Coordinate(4, 3), new Coordinate(2, 1) }));

            // ensure that this test doesn't "accidentally" pass just because GC happens to run.
            GC.KeepAlive(originalCoords);
        }
    }
}
