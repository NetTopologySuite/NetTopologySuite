using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Implementation
{
    //Tests are exposed by CoordinateSequenceTestBase type
    public class PackedCoordinateSequenceTest : CoordinateSequenceTestBase
    {
        protected override CoordinateSequenceFactory CsFactory => new PackedCoordinateSequenceFactory();

        [Test]
        public void TestDouble()
        {
            CheckAll(PackedCoordinateSequenceFactory.DoubleFactory);
        }

        [Test]
        public void TestFloat()
        {
            CheckAll(PackedCoordinateSequenceFactory.FloatFactory);
        }

        private void CheckAll(CoordinateSequenceFactory factory)
        {
            CheckDim2(1, factory);
            CheckDim2(5, factory);
            CheckDim3(factory);
            CheckDim3_M1(factory);
            CheckDim4_M1(factory);
            CheckDim4(factory);
            CheckDimInvalid(factory);
        }

        private void CheckDim2(int size, CoordinateSequenceFactory factory)
        {
            var seq = factory.Create(size, 2, 0);

            InitProgression(seq);

            Assert.AreEqual(2, seq.Dimension, "Dimension should be 2");
            Assert.IsTrue(!seq.HasZ, "Z should not be present");
            Assert.IsTrue(!seq.HasM, "M should not be present");

            int indexLast = size - 1;
            double valLast = indexLast * 10;

            var coord = seq.GetCoordinate(indexLast);
            Assert.IsTrue(coord.GetType() == typeof(Coordinate));
            Assert.AreEqual(valLast + 0, coord.X);
            Assert.AreEqual(valLast + 1, coord.Y);

            var array = seq.ToCoordinateArray();
            Assert.AreEqual(coord, array[indexLast]);
            Assert.IsTrue(coord != array[indexLast]);
            Assert.IsTrue(IsEqual(seq, array));

            var copy = factory.Create(array);

            Assert.IsTrue(IsEqual(copy, array));

            var copy2 = factory.Create(seq);
            Assert.IsTrue(IsEqual(copy2, array));
        }

        private void CheckDim3(CoordinateSequenceFactory factory)
        {
            var seq = factory.Create(5, 3, 0);
            InitProgression(seq);

            Assert.AreEqual(3, seq.Dimension, "Dimension should be 3");
            Assert.IsTrue(seq.HasZ, "Z should be present");
            Assert.IsTrue(!seq.HasM, "M should not be present");

            var coord = seq.GetCoordinate(4);
            Assert.IsTrue(coord.GetType() == typeof(CoordinateZ));
            var coordZ = (CoordinateZ) coord;
            Assert.AreEqual(40.0, coord.X);
            Assert.AreEqual(41.0, coord.Y);
            Assert.AreEqual(42.0, coordZ.Z);

            var array = seq.ToCoordinateArray();
            Assert.AreEqual(coord, array[4]);
            Assert.IsTrue(coord != array[4]);
            Assert.IsTrue(IsEqual(seq, array));

            var copy = factory.Create(array);
            Assert.IsTrue(IsEqual(copy, array));

            var copy2 = factory.Create(seq);
            Assert.IsTrue(IsEqual(copy2, array));
        }

        private void CheckDim3_M1(CoordinateSequenceFactory factory)
        {
            var seq = factory.Create(5, 3, 1);
            InitProgression(seq);

            Assert.AreEqual(3, seq.Dimension, "Dimension should be 3");
            Assert.IsTrue(!seq.HasZ, "Z should not be present");
            Assert.IsTrue(seq.HasM, "M should be present");

            var coord = seq.GetCoordinate(4);
            Assert.IsTrue(coord is CoordinateM);
            var coordM = (CoordinateM) coord;
            Assert.AreEqual(40.0, coord.X);
            Assert.AreEqual(41.0, coord.Y);
            Assert.AreEqual(42.0, coordM.M);

            var array = seq.ToCoordinateArray();
            Assert.AreEqual(coord, array[4]);
            Assert.IsTrue(coord != array[4]);
            Assert.IsTrue(IsEqual(seq, array));

            var copy = factory.Create(array);
            Assert.IsTrue(IsEqual(copy, array));

            var copy2 = factory.Create(seq);
            Assert.IsTrue(IsEqual(copy2, array));
        }

        private void CheckDim4_M1(CoordinateSequenceFactory factory)
        {
            var seq = factory.Create(5, 4, 1);
            InitProgression(seq);

            Assert.AreEqual(4, seq.Dimension, "Dimension should be 4");
            Assert.IsTrue(seq.HasZ, "Z should be present");
            Assert.IsTrue(seq.HasM, "M should be present");

            var coord = seq.GetCoordinate(4);
            Assert.IsTrue(coord is CoordinateZM);
            var coordZM = (CoordinateZM) coord;
            Assert.AreEqual(40.0, coord.X);
            Assert.AreEqual(41.0, coord.Y);
            Assert.AreEqual(42.0, coordZM.Z);
            Assert.AreEqual(43.0, coordZM.M);

            var array = seq.ToCoordinateArray();
            Assert.AreEqual(coord, array[4]);
            Assert.IsTrue(coord != array[4]);
            Assert.IsTrue(IsEqual(seq, array));

            var copy = factory.Create(array);
            Assert.IsTrue(IsEqual(copy, array));

            var copy2 = factory.Create(seq);
            Assert.IsTrue(IsEqual(copy2, array));
        }

        public void CheckDim4(CoordinateSequenceFactory factory)
        {
            var seq = factory.Create(5, 4, 0);
            InitProgression(seq);

            Assert.AreEqual(4, seq.Dimension, "Dimension should be 4");
            Assert.IsTrue(seq.HasZ, "Z should be present");
            Assert.IsTrue(!seq.HasM, "M should not be present");

            var coord = seq.GetCoordinate(4);
            Assert.AreEqual(40.0, coord.X);
            Assert.AreEqual(41.0, coord.Y);
            Assert.AreEqual(42.0, coord.Z);
            Assert.AreEqual(43.0, coord[Ordinate.Spatial4]);

            var array = seq.ToCoordinateArray();
            Assert.AreEqual(coord, array[4]);
            Assert.IsTrue(coord != array[4]);
            Assert.IsTrue(IsEqual(seq, array));

            var copy = factory.Create(array);
            Assert.IsTrue(IsEqual(copy, array));

            var copy2 = factory.Create(seq);
            Assert.IsTrue(IsEqual(copy2, array));
        }

        /**
         * Disable for now until solution can be found.
         * See Issue 434.
         */
        [Test]
        public void TestMixedFactoryWithXY()
        {
            var factoryPacked = new GeometryFactory(new PackedCoordinateSequenceFactory());
            var polygonPacked = factoryPacked.CreatePolygon(
                    new [] { new Coordinate(0, 0), new Coordinate(10, 0),
                        new Coordinate(10, 10), new Coordinate(0, 10), new Coordinate(0, 0) });
            var factoryDefault = new GeometryFactory();
            var polygonArray = factoryDefault.CreatePolygon(
                new Coordinate[] { new Coordinate(5, 5), new Coordinate(15, 5),
                    new Coordinate(15, 15), new Coordinate(5, 15), new Coordinate(5, 5) });

            Assert.That(() => polygonArray.Intersection(polygonPacked), Throws.Nothing);

            // this fails as of 2019-June-7
            Assert.That(() => polygonPacked.Intersection(polygonArray), Throws.Nothing);
        }

        private void CheckDimInvalid(CoordinateSequenceFactory factory)
        {
            try
            {
                var seq = factory.Create(5, 2, 1);
                Assert.Fail("Dimension=2/Measure=1 (XM) not supported");
            }
            catch (ArgumentException)
            {
            }
        }

        //private static void InitProgression(CoordinateSequence seq)
        //{
        //    for (int index = 0; index < seq.Count; index++)
        //    {
        //        for (int ordinateIndex = 0; ordinateIndex < seq.Dimension; ordinateIndex++)
        //        {
        //            seq.SetOrdinate(index, ordinateIndex, index);
        //        }
        //    }
        //}
    }
}
