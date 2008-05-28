using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using NetTopologySuite.Coordinates;
using NPack;
using NPack.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

#if DOTNET35
using System.Linq;
#endif

namespace ManagedBufferedCoordinate2DTests
{
    [TestFixture]
    public class BufferedCoordinate2DFactoryTests
    {
        [Test]
        public void CreateFactorySucceeds()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            Assert.IsNotNull(factory);
        }

        [Test]
        public void CreateBufferedCoordinate2DSucceeds()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            BufferedCoordinate2D coord = factory.Create(5, 10);
            Assert.AreEqual(5, coord.X);
            Assert.AreEqual(10, coord.Y);
        }

        [Test]
        public void CreateTwoBufferedCoordinate2DSucceeds()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            BufferedCoordinate2D coord1 = factory.Create(1, 15);
            BufferedCoordinate2D coord2 = factory.Create(15, 1);

            Assert.AreEqual(1, coord1.X);
            Assert.AreEqual(15, coord1.Y);
            Assert.AreEqual(15, coord2.X);
            Assert.AreEqual(1, coord2.Y);
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void Create3DCoordinateFails()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            factory.Create3D(5, 10, 15);
        }

        [Test]
        public void CreateCoordinateCopySucceeds()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            BufferedCoordinate2D coord = factory.Create(5, 10);
            BufferedCoordinate2D clone = factory.Create(coord);

            Assert.AreEqual(coord.X, clone.X);
            Assert.AreEqual(coord.Y, clone.Y);
        }

        [Test]
        public void CreateCoordinateCopyOfEmptySucceeds()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            BufferedCoordinate2D coord = new BufferedCoordinate2D();
            BufferedCoordinate2D clone = factory.Create(coord);

            Assert.IsTrue(clone.IsEmpty);
        }

        [Test]
        [Ignore("Not Implemented")]
        public void CreateAffineTransformSucceeds()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            //AffineTransformMatrix<BufferedCoordinate2D> matrix = factory.CreateTransform();

            //Assert.IsNotNull(matrix);
        }

        [Test]
        [Ignore("Not Implemented")]
        public void CreateAffineTransformWithAxisOfRotationSucceeds()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            //AffineTransformMatrix<BufferedCoordinate2D> matrix = factory.CreateTransform();

            //Assert.IsNotNull(matrix);
        }

        [Test]
        public void CreateHomogenizedCoordinateSucceeds()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            BufferedCoordinate2D coord = factory.Create(5, 10);
            BufferedCoordinate2D homogeneous = factory.Homogenize(coord);

            Assert.AreEqual(2, coord.ComponentCount);
            Assert.AreEqual(3, homogeneous.ComponentCount);
        }

        [Test]
        public void CreateHomogenizedCoordinateStreamSucceeds()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            Random rnd = new MersenneTwister();
            Int32 count = 10000;

            IEnumerable<BufferedCoordinate2D> coordinates = generateInfiniteCoordinates(factory, rnd);

#if DOTNET35
            IEnumerable<BufferedCoordinate2D> homogeneous = factory.Homogenize(coordinates.Take(count));
#else
            IEnumerable<BufferedCoordinate2D> homogeneous = factory.Homogenize(Enumerable.Take(coordinates, 10000));
#endif

            foreach (BufferedCoordinate2D coord in homogeneous)
            {
                Assert.AreEqual(3, coord.ComponentCount);
                count--;
            }

            Assert.AreEqual(0, count);
        }

        [Test]
        public void CreateDeomogenizedCoordinateSucceeds()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            BufferedCoordinate2D coord = factory.Create(5, 10);
            BufferedCoordinate2D homogeneous = factory.Homogenize(coord);
            BufferedCoordinate2D dehomogeneous = factory.Dehomogenize(homogeneous);

            Assert.AreEqual(2, coord.ComponentCount);
            Assert.AreEqual(3, homogeneous.ComponentCount);
            Assert.AreEqual(2, dehomogeneous.ComponentCount);
        }

        [Test]
        public void CreateDehomogenizedCoordinateStreamSucceeds()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            Random rnd = new MersenneTwister();
            Int32 count = 10000;

            IEnumerable<BufferedCoordinate2D> homogeneous = generateInfiniteHomogeneousCoordinates(factory, rnd);
            IEnumerable<BufferedCoordinate2D> dehomogeneous = Enumerable.Take(factory.Dehomogenize(homogeneous), count);

            foreach (BufferedCoordinate2D coord in dehomogeneous)
            {
                Assert.AreEqual(2, coord.ComponentCount);
                count--;
            }

            Assert.AreEqual(0, count);
        }

        [Test]
        public void CoordinateAddToBufferSucceeds()
        {
            MockRepository mocks = new MockRepository();
            IVector<DoubleComponent> vector = mocks.Stub<IVector<DoubleComponent>>();
            Expect.Call(vector.ComponentCount).Repeat.Any().Return(2);
            vector[0] = 5;
            vector[1] = 6;
            mocks.ReplayAll();

            IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer = new BufferedCoordinate2DFactory();

            Int32 index = buffer.Add(vector);

            Assert.AreEqual(5, buffer[index].X);
            Assert.AreEqual(6, buffer[index].Y);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Coordinate3DAddToBufferFails()
        {
            MockRepository mocks = new MockRepository();
            IVector<DoubleComponent> vector = mocks.Stub<IVector<DoubleComponent>>();
            Expect.Call(vector.ComponentCount).Repeat.Any().Return(3);
            vector[0] = 5;
            vector[1] = 6;
            vector[2] = 7;
            mocks.ReplayAll();

            IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer = new BufferedCoordinate2DFactory();

            buffer.Add(vector);
        }

        [Test]
        public void BufferedCoordinate2DAddToBufferSucceeds()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer = factory;
            BufferedCoordinate2D vector = factory.Create(5, 5);

            Int32 index = buffer.Add(vector);

            Assert.AreEqual(vector, buffer[index]);
        }

        [Test]
        public void BufferedCoordinate2DFromOtherFactoryAddToBufferSucceeds()
        {
            IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer = new BufferedCoordinate2DFactory();
            BufferedCoordinate2DFactory otherFactory = new BufferedCoordinate2DFactory();
            BufferedCoordinate2D vector = otherFactory.Create(5, 5);

            Int32 index = buffer.Add(vector);

            Assert.AreNotEqual(vector, buffer[index]);
            Assert.IsTrue(vector.ValueEquals(buffer[index]));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BufferedCoordinate2DAddToBufferFailsOnEmpty()
        {
            IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer = new BufferedCoordinate2DFactory();
            BufferedCoordinate2D emptyVector = new BufferedCoordinate2D();

            buffer.Add(emptyVector);
        }

        [Test]
        public void DoublesAddToBufferSucceeds()
        {
            IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer = new BufferedCoordinate2DFactory();

            BufferedCoordinate2D result = buffer.Add(1, 2);

            Assert.AreEqual(1, result.X);
            Assert.AreEqual(2, result.Y);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Doubles3DAddToBufferFails()
        {
            IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer = new BufferedCoordinate2DFactory();

            buffer.Add(1, 2, 3);
        }

        [Test]
        public void ClearBufferSucceeds()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            factory.Create(1, 1);
            factory.Create(2, 2);

            IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer = factory;

            Assert.AreEqual(2, buffer.Count);

            buffer.Clear();
            Assert.AreEqual(0, buffer.Count);
        }

        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void BufferContainsIVectorSucceeds()
        {
            MockRepository mocks = new MockRepository();
            IVector<DoubleComponent> vector = mocks.Stub<IVector<DoubleComponent>>();
            Expect.Call(vector.ComponentCount).Repeat.Any().Return(2);
            vector[0] = 5;
            vector[1] = 6;
            mocks.ReplayAll();
            
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer = factory;

            Assert.IsFalse(buffer.Contains(vector));

            factory.Create(5, 6);

            Assert.IsTrue(buffer.Contains(vector));
        }

        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void BufferContainsBufferedCoordinate2DSucceeds()
        {
            BufferedCoordinate2DFactory factory1 = new BufferedCoordinate2DFactory();
            BufferedCoordinate2DFactory factory2 = new BufferedCoordinate2DFactory();

            factory1.Create(1, 1);
            BufferedCoordinate2D coord1 = factory1.Create(2, 2);
            BufferedCoordinate2D coord2 = factory2.Create(2, 2);

            IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer = factory1;

            Assert.IsTrue(buffer.Contains(coord1));  // Not implemented in NPack
            Assert.IsFalse(buffer.Contains(coord2));
        }

        [Test]
        public void CopyToSucceeds()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer = factory;

            factory.Create(1, 1);
            factory.Create(1, 2);

            BufferedCoordinate2D[] result = new BufferedCoordinate2D[2];

            buffer.CopyTo(result, 0, 1);

            Assert.AreEqual(buffer[0], result[0]);
            Assert.AreEqual(buffer[1], result[1]);
        }

        [Test]
        public void CountSucceeds()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            factory.Create(1, 1);
            factory.Create(2, 2);

            IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer = factory;

            Assert.AreEqual(2, buffer.Count);
        }

        //[Test]
        //[ExpectedException(typeof(NotImplementedException))]
        //public void FactorySucceeds()
        //{
        //    BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();

        //    IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer = factory;

        //    Assert.IsInstanceOfType(typeof(IVectorFactory<BufferedCoordinate2D, DoubleComponent>), buffer.Factory);
        //    // not implemented and possibly going away in NPack
        //}

        [Test]
        public void IsReadOnlySucceeds()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer = factory;

            Assert.IsFalse(buffer.IsReadOnly);
        }

        [Test]
        public void GettingAndSettingMaximumSizeSucceeds()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer = factory;

            buffer.MaximumSize = Int32.MaxValue - 1000;
            Assert.AreEqual(Int32.MaxValue - 1000, buffer.MaximumSize);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SettingMaximumSizeFailsOnNegativeValue()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer = factory;

            buffer.MaximumSize = -1;
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SettingMaximumSizeFailsOnSmallerValueThanCurrentContents()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer = factory;

            buffer.MaximumSize = 3;
        }

        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void RemoveFails()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            BufferedCoordinate2D coord = factory.Create(1, 1);
            IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer = factory;

            buffer.Remove(coord.Index); // potentially invalidates sequences if allowed
        }

        [Test]
        [Ignore("Need to implement")]
        public void SizeIncreasedSucceeds()
        {

        }

        [Test]
        [Ignore("Need to implement")]
        public void SizeIncreasingSucceeds()
        {

        }

        [Test]
        [Ignore("Need to implement")]
        public void VectorChangedSucceeds()
        {

        }

        [Test]
        [Ignore("Need to implement")]
        public void VectorLengthSucceeds()
        {

        }

        [Test]
        public void GettingIndexerSucceeds()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            BufferedCoordinate2D coord = factory.Create(1, 1);
            IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer = factory;

            Assert.AreEqual(coord, buffer[coord.Index]);
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void SettingIndexerFails()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            BufferedCoordinate2D coord = factory.Create(1, 1);
            IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer = factory;

            // coordinate should be immutable except for possible precision snap-to
            buffer[coord.Index] = factory.Create(2, 2);
        }


        [Test]
        public void EnumeratingVertexesSucceeds()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            List<BufferedCoordinate2D> coordList = new List<BufferedCoordinate2D>(5);
            coordList.Add(factory.Create(1, 1));
            coordList.Add(factory.Create(1, 2));
            coordList.Add(factory.Create(1, 3));
            coordList.Add(factory.Create(1, 4));
            coordList.Add(factory.Create(1, 5));

            IVectorBuffer<BufferedCoordinate2D, DoubleComponent> buffer = factory;

            foreach(BufferedCoordinate2D coord in buffer)
            {
                Assert.IsTrue(coordList.Contains(coord));
            }
        }

        [Test]
        public void CreatingMultipleCoordinatesWithTheSameValueReturnsExistingCoordinate()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            Random random = new MersenneTwister();

            for (Int32 i = 0; i < 100; i++)
            {
                switch (random.Next(0, 4))
                {
                    case 0:
                        factory.Create(0, 0);
                        break;
                    case 1:
                        factory.Create(10, 10);
                        break;
                    case 2:
                        factory.Create(0, 100);
                        break;
                    case 3:
                        factory.Create(100, 100);
                        break;
                    default:
                        break;
                }
            }

            Assert.AreEqual(4, factory.VectorBuffer.Count);
        }

        [Test]
        [Ignore]
        public void Creating1MRandomCoordinatesDoesntKillPerformance()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            Random random = new MersenneTwister();
            Stopwatch timer = new Stopwatch();

            Int32[] times = new Int32[10];

            Double firstAvg = -1;
            Double secondAvg = -1;
            Double thirdAvg = -1;
            Double avg = 0;

            for (Int32 i = 0; i < 1000000; i++)
            {
                Double x = random.NextDouble();
                Double y = random.NextDouble();

                timer.Start();
                factory.Create(x, y);
                timer.Stop();

                times[i % 10] = (Int32)timer.ElapsedTicks;

                if (i == 9) firstAvg = avg;
                if (i == 19) secondAvg = avg;
                if (i == 29) thirdAvg = avg;

                avg = Enumerable.Average(times);
                Assert.Less(avg, 300000.0); // 30 ms

                timer.Reset();
            }

            Console.WriteLine("First average time to create a coordinate: {0:N4} ms", firstAvg / 10000);
            Console.WriteLine("Second average time to create a coordinate: {0:N4} ms", secondAvg / 10000);
            Console.WriteLine("Third average time to create a coordinate: {0:N4} ms", thirdAvg / 10000);
            Console.WriteLine("Average time to create a coordinate: {0:N4} ms", avg / 10000);
        }

        [Test]
        [Ignore]
        public void Creating1MRepeatedCoordinatesDoesntKillPerformance()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();
            Random random = new MersenneTwister();
            Stopwatch timer = new Stopwatch();

            Int32[] times = new Int32[10];

            Double firstAvg = -1;
            Double secondAvg = -1;
            Double thirdAvg = -1;
            Double avg = 0;

            for (Int32 i = 0; i < 1000000; i++)
            {
                Double x = random.Next(500, 1000);
                Double y = random.Next(100000, 100500);

                timer.Start();
                factory.Create(x, y);
                timer.Stop();

                times[i % 10] = (Int32)timer.ElapsedTicks;

                if (i == 9) firstAvg = avg;
                if (i == 19) secondAvg = avg;
                if (i == 29) thirdAvg = avg;

                avg = Enumerable.Average(times);
                Assert.Less(avg, 100000.0); // 10 ms

                timer.Reset();
            }

            Console.WriteLine("First average time to create a coordinate: {0:N4} ms", firstAvg / 10000);
            Console.WriteLine("Second average time to create a coordinate: {0:N4} ms", secondAvg / 10000);
            Console.WriteLine("Third average time to create a coordinate: {0:N4} ms", thirdAvg / 10000);
            Console.WriteLine("Last average time to create a coordinate: {0:N4} ms", avg / 10000);
        }

        [Test]
        [Ignore]
        public void CreatingSequencesWith1MRandomCoordinatesDoesntKillPerformance()
        {
            BufferedCoordinate2DFactory coordFactory = new BufferedCoordinate2DFactory();
            BufferedCoordinate2DSequenceFactory sequenceFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            Random random = new MersenneTwister();

            List<ICoordinateSequence<BufferedCoordinate2D>> sequences =
                new List<ICoordinateSequence<BufferedCoordinate2D>>();

            Int32 i = 0;

            // setup sequences
            for (; i < 10000; i++)
            {
                ICoordinateSequence<BufferedCoordinate2D> sequence
                    = sequenceFactory.Create(250, CoordinateDimensions.Two);

                for (Int32 j = 0; j < 250; j++)
                {
                    BufferedCoordinate2D coord = coordFactory.Create(
                        random.Next(420000000, 440000000) / 100.0,
                        random.Next(3500000, 8000000) / 100.0);

                    sequence[j] = coord;
                }

                sequences.Add(sequence);
            }

            Stopwatch timer = new Stopwatch();

            Int32[] times = new Int32[10];

            Double firstAvg = -1;
            Double secondAvg = -1;
            Double thirdAvg = -1;
            Double avg = 0;

            i = 0;

            foreach (ICoordinateSequence<BufferedCoordinate2D> sequence in sequences)
            {
                timer.Start();
                Assert.IsTrue(sequence.Maximum.GreaterThanOrEqualTo(sequence.Minimum));
                timer.Stop();

                times[i % 10] = (Int32)timer.ElapsedTicks;

                if (i == 9) firstAvg = avg;
                if (i == 19) secondAvg = avg;
                if (i == 29) thirdAvg = avg;

                avg = Enumerable.Average(times);
                Assert.Less(avg, 10000000.0); // 1000 ms

                i++;

                timer.Reset();
            }

            Console.WriteLine("First average time to compute Minimum and Maximum for a sequence {0:N4} ms", firstAvg / 10000);
            Console.WriteLine("Second average time to compute Minimum and Maximum for a sequence: {0:N4} ms", secondAvg / 10000);
            Console.WriteLine("Third average time to compute Minimum and Maximum for a sequence: {0:N4} ms", thirdAvg / 10000);
            Console.WriteLine("Average time to compute Minimum and Maximum for a sequence: {0:N4} ms", avg / 10000);
        }

        [Test]
        public void BitResolutionSnapsCoordinatesToGrid()
        {
            BufferedCoordinate2DFactory factory = new BufferedCoordinate2DFactory();

            factory.BitResolution = 24;

            factory.Create(10, 10);
            factory.Create(10.000000003, 10.000000003);

            Assert.AreEqual(1, factory.VectorBuffer.Count);
        }

        #region Private helper methods

        private static IEnumerable<BufferedCoordinate2D> generateInfiniteCoordinates(BufferedCoordinate2DFactory factory, Random rnd)
        {
            // Give thanks for Enumerable.Take()!
            while (true)
            {
                yield return factory.Create(rnd.NextDouble(), rnd.NextDouble());
            }
        }

        private static IEnumerable<BufferedCoordinate2D> generateInfiniteHomogeneousCoordinates(BufferedCoordinate2DFactory factory, Random rnd)
        {
            return factory.Homogenize(generateInfiniteCoordinates(factory, rnd));
        }
        #endregion
    }
}
