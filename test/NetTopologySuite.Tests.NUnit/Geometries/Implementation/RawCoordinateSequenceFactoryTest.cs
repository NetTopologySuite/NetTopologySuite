using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;

using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Implementation
{
    public sealed class RawCoordinateSequenceFactoryTest
    {
        [Test]
        public void TestCreateX_Y()
        {
            double[] expectedXFull = { double.NaN, 1, 2, 3, 4, double.PositiveInfinity, double.NegativeInfinity };
            double[] expectedYFull = { double.NaN, 9, 8, 7, 6, double.PositiveInfinity, double.NegativeInfinity };

            var expectedX = expectedXFull.AsMemory(1, 4);
            var expectedY = expectedYFull.AsMemory(1, 4);
            var seq = RawCoordinateSequenceFactory.CreateXY(expectedX, expectedY);
            AssertRawCoordinates(seq, (Ordinate.X, expectedX, 1), (Ordinate.Y, expectedY, 1));
        }

        [Test]
        public void TestCreateXY()
        {
            double[] expectedXYFull = { double.NaN, 1, 9, 2, 8, 3, 7, 4, 6, double.PositiveInfinity, double.NegativeInfinity };

            var expectedXY = expectedXYFull.AsMemory(1, 8);
            var seq = RawCoordinateSequenceFactory.CreateXY(expectedXY);
            AssertRawCoordinates(seq, (Ordinate.X, expectedXY[..^1], 2), (Ordinate.Y, expectedXY[1..], 2));
        }

        [Test]
        public void TestCreateX_Y_Z()
        {
            double[] expectedXFull = { double.NaN, 1, 2, 3, 4, double.PositiveInfinity, double.NegativeInfinity };
            double[] expectedYFull = { double.NaN, 9, 8, 7, 6, double.PositiveInfinity, double.NegativeInfinity };
            double[] expectedZFull = { double.NaN, 5, 5, 5, 5, double.PositiveInfinity, double.NegativeInfinity };

            var expectedX = expectedXFull.AsMemory(1, 4);
            var expectedY = expectedYFull.AsMemory(1, 4);
            var expectedZ = expectedZFull.AsMemory(1, 4);
            var seq = RawCoordinateSequenceFactory.CreateXYZ(expectedX, expectedY, expectedZ);
            AssertRawCoordinates(seq, (Ordinate.X, expectedX, 1), (Ordinate.Y, expectedY, 1), (Ordinate.Z, expectedZ, 1));
        }

        [Test]
        public void TestCreateXY_Z()
        {
            double[] expectedXYFull = { double.NaN, 1, 9, 2, 8, 3, 7, 4, 6, double.PositiveInfinity, double.NegativeInfinity };
            double[] expectedZFull = { double.NaN, 5, 5, 5, 5, double.PositiveInfinity, double.NegativeInfinity };

            var expectedXY = expectedXYFull.AsMemory(1, 8);
            var expectedZ = expectedZFull.AsMemory(1, 4);
            var seq = RawCoordinateSequenceFactory.CreateXYZ(expectedXY, expectedZ);
            AssertRawCoordinates(seq, (Ordinate.X, expectedXY[..^1], 2), (Ordinate.Y, expectedXY[1..], 2), (Ordinate.Z, expectedZ, 1));
        }

        [Test]
        public void TestCreateXYZ()
        {
            double[] expectedXYZFull = { double.NaN, 1, 9, 5, 2, 8, 5, 3, 7, 5, 4, 6, 5, double.PositiveInfinity, double.NegativeInfinity };

            var expectedXYZ = expectedXYZFull.AsMemory(1, 12);
            var seq = RawCoordinateSequenceFactory.CreateXYZ(expectedXYZ);
            AssertRawCoordinates(seq, (Ordinate.X, expectedXYZ[..^2], 3), (Ordinate.Y, expectedXYZ[1..^1], 3), (Ordinate.Z, expectedXYZ[2..], 3));
        }

        [Test]
        public void TestCreateX_Y_M()
        {
            double[] expectedXFull = { double.NaN, 1, 2, 3, 4, double.PositiveInfinity, double.NegativeInfinity };
            double[] expectedYFull = { double.NaN, 9, 8, 7, 6, double.PositiveInfinity, double.NegativeInfinity };
            double[] expectedMFull = { double.NaN, 5, 5, 5, 5, double.PositiveInfinity, double.NegativeInfinity };

            var expectedX = expectedXFull.AsMemory(1, 4);
            var expectedY = expectedYFull.AsMemory(1, 4);
            var expectedZ = expectedMFull.AsMemory(1, 4);
            var seq = RawCoordinateSequenceFactory.CreateXYM(expectedX, expectedY, expectedZ);
            AssertRawCoordinates(seq, (Ordinate.X, expectedX, 1), (Ordinate.Y, expectedY, 1), (Ordinate.M, expectedZ, 1));
        }

        [Test]
        public void TestCreateXY_M()
        {
            double[] expectedXYFull = { double.NaN, 1, 9, 2, 8, 3, 7, 4, 6, double.PositiveInfinity, double.NegativeInfinity };
            double[] expectedMFull = { double.NaN, 5, 5, 5, 5, double.PositiveInfinity, double.NegativeInfinity };

            var expectedXY = expectedXYFull.AsMemory(1, 8);
            var expectedZ = expectedMFull.AsMemory(1, 4);
            var seq = RawCoordinateSequenceFactory.CreateXYM(expectedXY, expectedZ);
            AssertRawCoordinates(seq, (Ordinate.X, expectedXY[..^1], 2), (Ordinate.Y, expectedXY[1..], 2), (Ordinate.M, expectedZ, 1));
        }

        [Test]
        public void TestCreateXYM()
        {
            double[] expectedXYMFull = { double.NaN, 1, 9, 5, 2, 8, 5, 3, 7, 5, 4, 6, 5, double.PositiveInfinity, double.NegativeInfinity };

            var expectedXYM = expectedXYMFull.AsMemory(1, 12);
            var seq = RawCoordinateSequenceFactory.CreateXYM(expectedXYM);
            AssertRawCoordinates(seq, (Ordinate.X, expectedXYM[..^2], 3), (Ordinate.Y, expectedXYM[1..^1], 3), (Ordinate.M, expectedXYM[2..], 3));
        }

        [Test]
        public void TestCreateX_Y_Z_M()
        {
            double[] expectedXFull = { double.NaN, 1, 2, 3, 4, double.PositiveInfinity, double.NegativeInfinity };
            double[] expectedYFull = { double.NaN, 9, 8, 7, 6, double.PositiveInfinity, double.NegativeInfinity };
            double[] expectedZFull = { double.NaN, 5, 5, 5, 5, double.PositiveInfinity, double.NegativeInfinity };
            double[] expectedMFull = { double.NaN, 0, 0, 0, 0, double.PositiveInfinity, double.NegativeInfinity };

            var expectedX = expectedXFull.AsMemory(1, 4);
            var expectedY = expectedYFull.AsMemory(1, 4);
            var expectedZ = expectedZFull.AsMemory(1, 4);
            var expectedM = expectedMFull.AsMemory(1, 4);
            var seq = RawCoordinateSequenceFactory.CreateXYZM(expectedX, expectedY, expectedZ, expectedM);
            AssertRawCoordinates(seq, (Ordinate.X, expectedX, 1), (Ordinate.Y, expectedY, 1), (Ordinate.Z, expectedZ, 1), (Ordinate.M, expectedM, 1));
        }

        [Test]
        public void TestCreateXY_Z_M()
        {
            double[] expectedXYFull = { double.NaN, 1, 9, 2, 8, 3, 7, 4, 6, double.PositiveInfinity, double.NegativeInfinity };
            double[] expectedZFull = { double.NaN, 5, 5, 5, 5, double.PositiveInfinity, double.NegativeInfinity };
            double[] expectedMFull = { double.NaN, 0, 0, 0, 0, double.PositiveInfinity, double.NegativeInfinity };

            var expectedXY = expectedXYFull.AsMemory(1, 8);
            var expectedZ = expectedZFull.AsMemory(1, 4);
            var expectedM = expectedMFull.AsMemory(1, 4);
            var seq = RawCoordinateSequenceFactory.CreateXYZM(expectedXY, expectedZ, expectedM);
            AssertRawCoordinates(seq, (Ordinate.X, expectedXY[..^1], 2), (Ordinate.Y, expectedXY[1..], 2), (Ordinate.Z, expectedZ, 1), (Ordinate.M, expectedM, 1));
        }

        [Test]
        public void TestCreateXYZM()
        {
            double[] expectedXYZMFull = { double.NaN, 1, 9, 5, 0, 2, 8, 5, 0, 3, 7, 5, 0, 4, 6, 5, 0, double.PositiveInfinity, double.NegativeInfinity };

            var expectedXYZM = expectedXYZMFull.AsMemory(1, 16);
            var seq = RawCoordinateSequenceFactory.CreateXYZM(expectedXYZM);
            AssertRawCoordinates(seq, (Ordinate.X, expectedXYZM[..^3], 4), (Ordinate.Y, expectedXYZM[1..^2], 4), (Ordinate.Z, expectedXYZM[2..^1], 4), (Ordinate.M, expectedXYZM[3..], 4));
        }

        [Test]
        public void TestCreateHighlyAtypical()
        {
            // 6 spatial dimensions plus 4 measures = 10 total dimensions.
            // sprinkle their data throughout the various arrays that we create.
            // note that Measure3 is not represented, so it goes by itself.
            Ordinates[] ordinateGroups =
            {
                Ordinates.X | Ordinates.M,
                Ordinates.Y | Ordinates.Z,
                Ordinates.Spatial4 | Ordinates.Spatial5 | Ordinates.Measure2,
                Ordinates.Spatial6 | Ordinates.Measure4,
            };
            var factory = new RawCoordinateSequenceFactory(ordinateGroups);
            var untypedSeq = factory.Create(102, 10, 4);
            Assert.That(untypedSeq, Is.InstanceOf<RawCoordinateSequence>());
            var seq = (RawCoordinateSequence)untypedSeq;

            // The data should be grouped according to what we requested.
            AssertOrdinateGroup(Ordinate.X, Ordinate.M);
            AssertOrdinateGroup(Ordinate.Y, Ordinate.Z);
            AssertOrdinateGroup(Ordinate.Spatial4, Ordinate.Spatial5, Ordinate.Measure2);
            AssertOrdinateGroup(Ordinate.Spatial6, Ordinate.Measure4);
            AssertOrdinateGroup(Ordinate.Measure3);

            // set all the values...
            for (int i = 0; i < seq.Count; i++)
            {
                for (int j = 0; j < seq.Dimension; j++)
                {
                    seq.SetOrdinate(i, j, ExpectedOrdinateValue(i, j));
                }
            }

            // ...and make sure that they all got set uniquely.
            for (int i = 0; i < seq.Count; i++)
            {
                for (int j = 0; j < seq.Dimension; j++)
                {
                    Assert.That(seq.GetOrdinate(i, j), Is.EqualTo(ExpectedOrdinateValue(i, j)));
                }
            }

            void AssertOrdinateGroup(params Ordinate[] ordinates)
            {
                int[] ordinateIndexes = new int[ordinates.Length];
                Assert.Multiple(
                    () =>
                    {
                        for (int i = 0; i < ordinates.Length; i++)
                        {
                            Assert.That(seq.TryGetOrdinateIndex(ordinates[i], out ordinateIndexes[i]));
                        }
                    });

                // sort the ordinate indexes according to the order they appear in their array.
                Span<int> toSort = ordinateIndexes;
                while (toSort.Length > 1)
                {
                    int minIndex = 0;
                    ref double min = ref FirstVal(toSort[minIndex]);
                    for (int i = 1; i < toSort.Length; i++)
                    {
                        ref double nxt = ref FirstVal(toSort[i]);
                        if (Unsafe.IsAddressLessThan(ref nxt, ref min))
                        {
                            min = ref nxt;
                            minIndex = i;
                        }
                    }

                    if (minIndex != 0)
                    {
                        (toSort[0], toSort[minIndex]) = (toSort[minIndex], toSort[0]);
                    }

                    toSort = toSort[1..];
                }

                // ensure that each ordinate's raw data comes immediately after the previous one's
                int minOrdinateIndex = ordinateIndexes[0];
                ref double prev = ref FirstVal(minOrdinateIndex);
                for (int i = 1; i < ordinateIndexes.Length; i++)
                {
                    ref double curr = ref FirstVal(ordinateIndexes[i]);
                    Assert.That(Unsafe.AreSame(ref Unsafe.Add(ref prev, 1), ref curr));
                    prev = ref curr;
                }

                // ensure that there are no gaps between the raw data for two coordinates.
                Assert.That(
                    Unsafe.AreSame(
                        ref Unsafe.Add(ref prev, 1),
                        ref SecondVal(minOrdinateIndex)));

                ref double FirstVal(int ordinateIndex)
                {
                    var array = seq.GetRawCoordinatesAndStride(ordinateIndex).Array;
                    return ref MemoryMarshal.GetReference(array.Span);
                }

                ref double SecondVal(int ordinateIndex)
                {
                    (var array, int stride) = seq.GetRawCoordinatesAndStride(ordinateIndex);
                    return ref array.Span[stride];
                }
            }

            double ExpectedOrdinateValue(double i, double j)
            {
                return (i * seq.Dimension) + j;
            }
        }

        private static void AssertRawCoordinates(RawCoordinateSequence seq, params (Ordinate ordinate, ReadOnlyMemory<double> Memory, int Stride)[] allExpected)
        {
            Assert.That(seq.Dimension, Is.EqualTo(allExpected.Length));
            Assert.Multiple(
                () =>
                {
                    for (int i = 0; i < allExpected.Length; i++)
                    {
                        (var ordinate, var expectedMemory, int expectedStride) = allExpected[i];
                        if (seq.TryGetOrdinateIndex(ordinate, out int ordinateIndex))
                        {
                            Assert.That(ordinateIndex, Is.EqualTo(i));
                        }
                        else
                        {
                            Assert.Fail("TryGetOrdinateIndex returned false for ordinate {0}", ordinate);
                        }

                        (var actualMemory, int actualStride) = seq.GetRawCoordinatesAndStride(i);
                        Assert.That(actualStride, Is.EqualTo(expectedStride));
                        if (expectedMemory.Length > actualMemory.Length)
                        {
                            Assert.Fail("Expected at least {0} elements for {1}, but got {2}", expectedMemory.Length, ordinate, actualMemory.Length);
                        }
                        else
                        {
                            Assert.That(actualMemory.Slice(0, expectedMemory.Length), Is.EqualTo(expectedMemory));
                        }
                    }
                });
        }
    }
}
