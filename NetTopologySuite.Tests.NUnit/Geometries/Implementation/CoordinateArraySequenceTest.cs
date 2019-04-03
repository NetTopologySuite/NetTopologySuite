using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Implementation
{
    //Tests are exposed by CoordinateSequenceTestBase type
    public class CoordinateArraySequenceTest : CoordinateSequenceTestBase
    {
        protected override ICoordinateSequenceFactory CsFactory => CoordinateArraySequenceFactory.Instance;

        [Test]
        public void TestFactoryLimits()
        {
            // Expected to clip dimension and measure value within factory limits

            var factory = (CoordinateArraySequenceFactory)CsFactory;
            var sequence = factory.Create(10, 4);
            Assert.That(sequence.Dimension, Is.EqualTo(3));
            Assert.That(sequence.Measures, Is.EqualTo(0));
            Assert.That(sequence.HasZ);
            Assert.That(!sequence.HasM);

            sequence = factory.Create(10, 4, 0);
            Assert.That(sequence.Dimension, Is.EqualTo(3));
            Assert.That(sequence.Measures, Is.EqualTo(0));
            Assert.That(sequence.HasZ);
            Assert.That(!sequence.HasM);

            sequence = factory.Create(10, 4, 2); // note clip to spatial dimension
            Assert.That(sequence.Dimension, Is.EqualTo(3));
            Assert.That(sequence.Measures, Is.EqualTo(1));
            Assert.That(!sequence.HasZ);
            Assert.That(sequence.HasM);

            sequence = factory.Create(10, 5, 1);
            Assert.That(sequence.Dimension, Is.EqualTo(4));
            Assert.That(sequence.Measures, Is.EqualTo(1));
            Assert.That(sequence.HasZ);
            Assert.That(sequence.HasM);

            // previously this clipped to dimension 3, measure 3
            sequence = factory.Create(10, 1);
            Assert.That(sequence.Dimension, Is.EqualTo(2));
            Assert.That(sequence.Measures, Is.EqualTo(0));
            Assert.That(!sequence.HasZ);
            Assert.That(!sequence.HasM);

            sequence = factory.Create(10, 2, 1);
            Assert.That(sequence.Dimension, Is.EqualTo(3));
            Assert.That(sequence.Measures, Is.EqualTo(1));
            Assert.That(!sequence.HasZ);
            Assert.That(sequence.HasM);
        }

        [Test]
        public void TestDimensionAndMeasure()
        {
            var factory = CsFactory;
            var seq = factory.Create(5, 2);
            ICoordinateSequence copy;
            Coordinate coord;
            Coordinate[] array;

            InitProgression(seq);
            Assert.That(seq.Dimension, Is.EqualTo(2));
            Assert.That(seq.HasZ, Is.False);
            Assert.That(seq.HasM, Is.False);
            coord = seq.GetCoordinate(4);
            Assert.That(coord, Is.TypeOf<Coordinate>());
            Assert.That(coord.X, Is.EqualTo(4));
            Assert.That(coord.Y, Is.EqualTo(4));
            array = seq.ToCoordinateArray();
            Assert.That(array[4], Is.EqualTo(coord));
            Assert.That(IsEqual(seq, array), Is.True);
            copy = factory.Create(array);
            Assert.That(IsEqual(copy, array), Is.True);
            copy = factory.Create(seq);
            Assert.That(IsEqual(copy, array), Is.True);

            seq = factory.Create(5, 3);
            InitProgression(seq);
            Assert.That(seq.Dimension, Is.EqualTo(3));
            Assert.That(seq.HasZ, Is.True);
            Assert.That(seq.HasM, Is.False);
            coord = seq.GetCoordinate(4);
            Assert.That(coord, Is.TypeOf<CoordinateZ>());
            Assert.That(coord.X, Is.EqualTo(4));
            Assert.That(coord.Y, Is.EqualTo(4));
            Assert.That(coord.Z, Is.EqualTo(4));
            array = seq.ToCoordinateArray();
            Assert.That(array[4], Is.EqualTo(coord));
            Assert.That(IsEqual(seq, array), Is.True);
            copy = factory.Create(array);
            Assert.That(IsEqual(copy, array), Is.True);
            copy = factory.Create(seq);
            Assert.That(IsEqual(copy, array), Is.True);

            seq = factory.Create(5, 3, 1);
            InitProgression(seq);
            Assert.That(seq.Dimension, Is.EqualTo(3));
            Assert.That(seq.HasZ, Is.False);
            Assert.That(seq.HasM, Is.True);
            coord = seq.GetCoordinate(4);
            Assert.That(coord, Is.TypeOf<CoordinateM>());
            Assert.That(coord.X, Is.EqualTo(4));
            Assert.That(coord.Y, Is.EqualTo(4));
            Assert.That(coord.M, Is.EqualTo(4));
            array = seq.ToCoordinateArray();
            Assert.That(array[4], Is.EqualTo(coord));
            Assert.That(IsEqual(seq, array), Is.True);
            copy = factory.Create(array);
            Assert.That(IsEqual(copy, array), Is.True);
            copy = factory.Create(seq);
            Assert.That(IsEqual(copy, array), Is.True);

            seq = factory.Create(5, 4, 1);
            InitProgression(seq);
            Assert.That(seq.Dimension, Is.EqualTo(4));
            Assert.That(seq.HasZ, Is.True);
            Assert.That(seq.HasM, Is.True);
            coord = seq.GetCoordinate(4);
            Assert.That(coord, Is.TypeOf<CoordinateZM>());
            Assert.That(coord.X, Is.EqualTo(4));
            Assert.That(coord.Y, Is.EqualTo(4));
            Assert.That(coord.Z, Is.EqualTo(4));
            Assert.That(coord.M, Is.EqualTo(4));
            array = seq.ToCoordinateArray();
            Assert.That(array[4], Is.EqualTo(coord));
            Assert.That(IsEqual(seq, array), Is.True);
            copy = factory.Create(array);
            Assert.That(IsEqual(copy, array), Is.True);
            copy = factory.Create(seq);
            Assert.That(IsEqual(copy, array), Is.True);

            // dimensions clipped from XM to XYM
            seq = factory.Create(5, 2, 1);
            Assert.That(seq.Dimension, Is.EqualTo(3));
            Assert.That(seq.Measures, Is.EqualTo(1));
        }

        [Test]
        public void TestMixedCoordinates()
        {
            var factory = CsFactory;
            var coord1 = new CoordinateZ(1.0, 1.0, 1.0);
            var coord2 = new Coordinate(2.0, 2.0);
            var coord3 = new CoordinateM(3.0, 3.0, 3.0);

            var array = new Coordinate[] { coord1, coord2, coord3, null };
            var seq = factory.Create(array);
            Assert.That(seq.Dimension, Is.EqualTo(3));
            Assert.That(seq.Measures, Is.EqualTo(1));
            Assert.That(seq.GetCoordinate(0), Is.EqualTo(coord1));
            Assert.That(seq.GetCoordinate(1), Is.EqualTo(coord2));
            Assert.That(seq.GetCoordinate(2), Is.EqualTo(coord3));
            Assert.That(seq.GetCoordinate(3), Is.Null);
        }

        private static void InitProgression(ICoordinateSequence seq)
        {
            for (int index = 0; index < seq.Count; index++)
            {
                for (int ordinateIndex = 0; ordinateIndex < seq.Dimension; ordinateIndex++)
                {
                    seq.SetOrdinate(index, (Ordinate)ordinateIndex, index);
                }
            }
        }
    }
}
