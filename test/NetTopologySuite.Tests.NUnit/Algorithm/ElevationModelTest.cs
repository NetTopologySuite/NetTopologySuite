using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using System.Collections.Generic;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    internal class ElevationModelTest
    {
        [Test]
        public void TestDefaultConstructor()
        {
            var emdl = new ElevationModel();
            Assert.That(emdl, Is.Not.Null);
            Assert.That(emdl.Extent, Is.Null);
            Assert.That(emdl.SRID, Is.EqualTo(0));

            foreach ((var pos, double z) in SamplePosZs(double.NaN))
                Assert.That(emdl.GetZ(pos), Is.EqualTo(double.NaN));
        }

        [Test]
        public void TestConstructorWithDefaultZValue()
        {
            var emdl = new ElevationModel(1d);
            Assert.That(emdl, Is.Not.Null);
            Assert.That(emdl.Extent, Is.Null);
            Assert.That(emdl.SRID, Is.EqualTo(0));

            foreach((var pos, double z) in SamplePosZs(1d))
                Assert.That(emdl.GetZ(pos), Is.EqualTo(1d));
        }

        [TestCase(4326, -10, 10, 30, 60, 10)]
        public void TestConstructorWithAllArguments(int srid, double minX, double maxX, double minY, double maxY, double defaultZ)
        {
            var emdl = new ElevationModel(defaultZ, new Envelope(minX, maxX, minY, maxY), srid);

            Assert.That(emdl, Is.Not.Null);
            Assert.That(emdl.Extent, Is.Not.Null);
            Assert.That(emdl.SRID, Is.EqualTo(srid));

            int i = 0;
            foreach ((var pos, double z) in SamplePosZs(emdl.Extent, defaultZ))
            {
                Assert.That(emdl.GetZ(pos), Is.EqualTo(z), $"Assertion failed for test position {i} at {pos}.");
                i++;
            }
        }

        [Test]
        public void TestCopyWithZ()
        {
            var emdl = new ElevationModel(10);

            var pos = new Coordinate(1, 1);
            var posZ = emdl.CopyWithZ(pos);
            Assert.That(posZ, Is.Not.Null);
            Assert.That(posZ, Is.TypeOf<CoordinateZ>());
            Assert.That(posZ.Z, Is.EqualTo(10));

            pos = new CoordinateZ(1, 1, 9);
            posZ = emdl.CopyWithZ(pos);
            Assert.That(posZ, Is.Not.Null);
            Assert.That(posZ, Is.TypeOf<CoordinateZ>());
            Assert.That(posZ.Z, Is.EqualTo(9));

            pos = new CoordinateM(1, 1, 8);
            posZ = emdl.CopyWithZ(pos);
            Assert.That(posZ, Is.Not.Null);
            Assert.That(posZ, Is.TypeOf<CoordinateZM>());
            Assert.That(posZ.Z, Is.EqualTo(10));
            Assert.That(posZ.M, Is.EqualTo(8));
        }

        private static IEnumerable<(Coordinate pos, double z)> SamplePosZs(double defaultZ)
        {
            yield return (new Coordinate(), defaultZ);
        }


        private static IEnumerable<(Coordinate pos, double z)> SamplePosZs(Envelope extent, double defaultZ)
        {
            if (extent == null || extent.Area == 0d)
            {
                Assert.Ignore("extent is null or empty");
                yield break;
            }

            double testDX = 0.1 * extent.Width;
            double testDY = 0.1 * extent.Height;

            yield return (extent.Centre, defaultZ);

            // left bottom
            yield return (new Coordinate(extent.MinX, extent.MinY), defaultZ);
            yield return (new Coordinate(extent.MinX - testDX, extent.MinY), Coordinate.NullOrdinate);
            yield return (new Coordinate(extent.MinX + testDX, extent.MinY), defaultZ);
            yield return (new Coordinate(extent.MinX, extent.MinY - testDY), Coordinate.NullOrdinate);
            yield return (new Coordinate(extent.MinX, extent.MinY + testDY), defaultZ);
            //left top
            yield return (new Coordinate(extent.MinX, extent.MaxY), defaultZ);
            yield return (new Coordinate(extent.MinX - testDX, extent.MaxY), Coordinate.NullOrdinate);
            yield return (new Coordinate(extent.MinX + testDX, extent.MaxY), defaultZ);
            yield return (new Coordinate(extent.MinX, extent.MaxY - testDY), defaultZ);
            yield return (new Coordinate(extent.MinX, extent.MaxY + testDY), Coordinate.NullOrdinate);
            //right top
            yield return (new Coordinate(extent.MaxX, extent.MaxY), defaultZ);
            yield return (new Coordinate(extent.MaxX - testDX, extent.MaxY), defaultZ);
            yield return (new Coordinate(extent.MaxX + testDX, extent.MaxY), Coordinate.NullOrdinate);
            yield return (new Coordinate(extent.MaxX, extent.MaxY - testDY), defaultZ);
            yield return (new Coordinate(extent.MaxX, extent.MaxY + testDY), Coordinate.NullOrdinate);
            //right bottom
            yield return (new Coordinate(extent.MaxX, extent.MinY), defaultZ);
            yield return (new Coordinate(extent.MaxX - testDX, extent.MinY), defaultZ);
            yield return (new Coordinate(extent.MaxX + testDX, extent.MinY), Coordinate.NullOrdinate);
            yield return (new Coordinate(extent.MaxX, extent.MinY - testDY), Coordinate.NullOrdinate);
            yield return (new Coordinate(extent.MaxX, extent.MinY + testDY), defaultZ);
        }

    }
}
