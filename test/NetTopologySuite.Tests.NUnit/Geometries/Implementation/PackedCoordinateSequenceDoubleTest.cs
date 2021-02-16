using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Implementation
{
    public class PackedCoordinateSequenceDoubleTest : CoordinateSequenceTestBase
    {
        protected override CoordinateSequenceFactory CsFactory { get => PackedCoordinateSequenceFactory.DoubleFactory; }

        [Test]
        public void Test4dCoordinateSequence()
        {
            var cs = new PackedCoordinateSequenceFactory(PackedCoordinateSequenceFactory.PackedType.Double)
                .Create(new[] { 0.0d, 1.0d, 2.0d, 3.0d, 4.0d, 5.0d, 6.0d, 7.0d }, 4, 1);
            Assert.AreEqual(2.0, cs.GetOrdinate(0, Ordinate.Z));
            Assert.AreEqual(3.0, cs.GetOrdinate(0, Ordinate.M));
        }

        [Test]
        public void Test2dMeasuredCoordinateSequenceInitializedFromVariousCoordinates()
        {
            var extraMeasuresCoordinate = Coordinates.Create(4, 2);
            extraMeasuresCoordinate.X = 6;
            extraMeasuresCoordinate.Y = 7;
            extraMeasuresCoordinate.M = 8;
            Coordinate[] coords =
            {
                new Coordinate(1, 2),
                new CoordinateZ(2, 3, 4),
                new CoordinateM(3, 4, 5),
                new CoordinateZ(4, 5, 6),
                new CoordinateZM(5, 6, 7, 8),
                extraMeasuresCoordinate,
            };
            double[] expected =
            {
                1, 2, double.NaN,
                2, 3, double.NaN,
                3, 4, 5,
                4, 5, double.NaN,
                5, 6, 8,
                6, 7, 8,
            };
            var cs = new PackedDoubleCoordinateSequence(coords, 3, 1);
            Assert.That(cs.GetRawCoordinates(), Is.EqualTo(expected));
        }
    }
}
