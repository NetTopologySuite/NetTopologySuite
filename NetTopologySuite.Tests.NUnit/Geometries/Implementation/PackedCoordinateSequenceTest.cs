using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Implementation
{
    //Tests are exposed by CoordinateSequenceTestBase type
    public class PackedCoordinateSequenceTest : CoordinateSequenceTestBase
    {
        protected override ICoordinateSequenceFactory CsFactory => new PackedCoordinateSequenceFactory();

        [TestAttribute]
        public void TestMultiPointDim4()
        {
            var gf = new GeometryFactory(new PackedCoordinateSequenceFactory());
            var mpSeq = gf.CoordinateSequenceFactory.Create(1, Ordinates.XYZM);
            mpSeq.SetOrdinate(0, Ordinate.X, 50);
            mpSeq.SetOrdinate(0, Ordinate.Y, -2);
            mpSeq.SetOrdinate(0, Ordinate.Z, 10);
            mpSeq.SetOrdinate(0, Ordinate.M, 20);

            var mp = gf.CreateMultiPoint(mpSeq);
            var pSeq = ((Point)mp.GetGeometryN(0)).CoordinateSequence;
            Assert.AreEqual(4, pSeq.Dimension);
            Assert.AreEqual(Ordinates.XYZM, pSeq.Ordinates);
            for (int i = 0; i < 4; i++)
                Assert.AreEqual(mpSeq.GetOrdinate(0, (Ordinate)i), pSeq.GetOrdinate(0, (Ordinate)i));
        }

        [Test]
        public void testDimensionAndMeasure()
        {
            var factory = new PackedCoordinateSequenceFactory();
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
            Assert.That(array[4], Is.Not.SameAs(coord));
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
            Assert.That(array[4], Is.Not.SameAs(coord));
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
            Assert.That(array[4], Is.Not.SameAs(coord));
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
            Assert.That(array[4], Is.Not.SameAs(coord));
            Assert.That(IsEqual(seq, array), Is.True);
            copy = factory.Create(array);
            Assert.That(IsEqual(copy, array), Is.True);
            copy = factory.Create(seq);
            Assert.That(IsEqual(copy, array), Is.True);

            Assert.That(() => factory.Create(5, 2, 1), Throws.InstanceOf<ArgumentException>());
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
