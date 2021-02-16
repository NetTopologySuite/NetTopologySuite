using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Implementation
{
    public class PackedCoordinateSequenceFloatTest : CoordinateSequenceTestBase
    {
        protected override CoordinateSequenceFactory CsFactory { get => PackedCoordinateSequenceFactory.FloatFactory; }

        [Test]
        public void Test4dCoordinateSequence()
        {
            var cs = new PackedCoordinateSequenceFactory(PackedCoordinateSequenceFactory.PackedType.Float)
                .Create(new [] { 0.0f, 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f }, 4, 1);
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
            float[] expected =
            {
                1, 2, float.NaN,
                2, 3, float.NaN,
                3, 4, 5,
                4, 5, float.NaN,
                5, 6, 8,
                6, 7, 8,
            };
            var cs = new PackedFloatCoordinateSequence(coords, 3, 1);
            Assert.That(cs.GetRawCoordinates(), Is.EqualTo(expected));
        }
    }
}
