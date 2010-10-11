using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using NetTopologySuite.Coordinates;
using NPack;
using NPack.Interfaces;
using Xunit;
using Rhino.Mocks;

#if DOTNET35
using System.Linq;
using Enumerable = System.Linq.Enumerable;
#else
using Enumerable = GeoAPI.DataStructures.Enumerable;
#endif

namespace ManagedBufferedCoordinateTests
{
    public class BufferedCoordinateFactoryTests
    {
        [Fact]
        public void CreateFactorySucceeds()
        {
            BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
            Assert.NotNull(factory);
        }

        [Fact]
        public void CreateBufferedCoordinateSucceeds()
        {
            BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
            BufferedCoordinate coord = factory.Create(5, 10);
            Assert.Equal(5, coord.X);
            Assert.Equal(10, coord.Y);
        }

        [Fact]
        public void CreateTwoBufferedCoordinateSucceeds()
        {
            BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
            BufferedCoordinate coord1 = factory.Create(1, 15);
            BufferedCoordinate coord2 = factory.Create(15, 1);

            Assert.Equal(1, coord1.X);
            Assert.Equal(15, coord1.Y);
            Assert.Equal(15, coord2.X);
            Assert.Equal(1, coord2.Y);
        }

        [Fact]
        public void Create3DCoordinateFails()
        {
            Assert.Throws<NotSupportedException>(delegate()
            {
                BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
                factory.Create3D(5, 10, 15);
            });
        }

        [Fact]
        public void CreateCoordinateCopySucceeds()
        {
            BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
            BufferedCoordinate coord = factory.Create(5, 10);
            BufferedCoordinate clone = factory.Create(coord);

            Assert.Equal(coord.X, clone.X);
            Assert.Equal(coord.Y, clone.Y);
        }

        [Fact]
        public void CreateCoordinateCopyOfEmptySucceeds()
        {
            BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
            BufferedCoordinate coord = new BufferedCoordinate();
            BufferedCoordinate clone = factory.Create(coord);

            Assert.True(clone.IsEmpty);
        }

        [Fact(Skip = "Not Implemented")]
        public void CreateAffineTransformSucceeds()
        {
            BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
            //AffineTransformMatrix<BufferedCoordinate> matrix = factory.CreateTransform();

            //Assert.NotNull(matrix);
        }

        [Fact(Skip = "Not Implemented")]
        public void CreateAffineTransformWithAxisOfRotationSucceeds()
        {
            BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
            //AffineTransformMatrix<BufferedCoordinate> matrix = factory.CreateTransform();

            //Assert.NotNull(matrix);
        }

        [Fact]
        public void CreateHomogenizedCoordinateSucceeds()
        {
            BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
            BufferedCoordinate coord = factory.Create(5, 10);
            BufferedCoordinate homogeneous = factory.Homogenize(coord);

            Assert.Equal(2, coord.ComponentCount);
            Assert.Equal(3, homogeneous.ComponentCount);
        }

        [Fact]
        public void CreateHomogenizedCoordinateStreamSucceeds()
        {
            BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
            Random rnd = new MersenneTwister();
            Int32 count = 10000;

            IEnumerable<BufferedCoordinate> coordinates = generateInfiniteCoordinates(factory, rnd);

#if DOTNET35
            IEnumerable<BufferedCoordinate> homogeneous = factory.Homogenize(coordinates.Take(count));
#else
            IEnumerable<BufferedCoordinate> homogeneous = factory.Homogenize(Enumerable.Take(coordinates, 10000));
#endif

            foreach (BufferedCoordinate coord in homogeneous)
            {
                Assert.Equal(3, coord.ComponentCount);
                count--;
            }

            Assert.Equal(0, count);
        }

        [Fact]
        public void CreateDeomogenizedCoordinateSucceeds()
        {
            BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
            BufferedCoordinate coord = factory.Create(5, 10);
            BufferedCoordinate homogeneous = factory.Homogenize(coord);
            BufferedCoordinate dehomogeneous = factory.Dehomogenize(homogeneous);

            Assert.Equal(2, coord.ComponentCount);
            Assert.Equal(3, homogeneous.ComponentCount);
            Assert.Equal(2, dehomogeneous.ComponentCount);
        }

        [Fact]
        public void CreateDehomogenizedCoordinateStreamSucceeds()
        {
            BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
            Random rnd = new MersenneTwister();
            Int32 count = 10000;

            IEnumerable<BufferedCoordinate> homogeneous = generateInfiniteHomogeneousCoordinates(factory, rnd);
            IEnumerable<BufferedCoordinate> dehomogeneous = Enumerable.Take(factory.Dehomogenize(homogeneous), count);

            foreach (BufferedCoordinate coord in dehomogeneous)
            {
                Assert.Equal(2, coord.ComponentCount);
                count--;
            }

            Assert.Equal(0, count);
        }

        [Fact]
        public void CoordinateAddToBufferSucceeds()
        {
            MockRepository mocks = new MockRepository();
            IVector<DoubleComponent> vector = mocks.Stub<IVector<DoubleComponent>>();
            Expect.Call(vector.ComponentCount).Repeat.Any().Return(2);
            vector[0] = 5;
            vector[1] = 6;
            mocks.ReplayAll();

            IVectorBuffer<DoubleComponent, BufferedCoordinate> buffer = new BufferedCoordinateFactory();

            Int32 index = buffer.Add(vector);

            Assert.Equal(5, buffer[index].X);
            Assert.Equal(6, buffer[index].Y);
        }

        [Fact]
        public void Coordinate3DAddToBufferFails()
        {
            Assert.Throws<ArgumentException>(delegate()
                              {
                                  MockRepository mocks = new MockRepository();
                                  IVector<DoubleComponent> vector = mocks.Stub<IVector<DoubleComponent>>();
                                  Expect.Call(vector.ComponentCount).Repeat.Any().Return(3);
                                  vector[0] = 5;
                                  vector[1] = 6;
                                  vector[2] = 7;
                                  mocks.ReplayAll();

                                  IVectorBuffer<DoubleComponent, BufferedCoordinate> buffer =
                                      new BufferedCoordinateFactory();

                                  buffer.Add(vector);
                              });
        }

        [Fact]
        public void BufferedCoordinateAddToBufferSucceeds()
        {
            BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
            IVectorBuffer<DoubleComponent, BufferedCoordinate> buffer = factory;
            BufferedCoordinate vector = factory.Create(5, 5);

            Int32 index = buffer.Add(vector);

            Assert.Equal(vector, buffer[index]);
        }

        [Fact]
        public void BufferedCoordinateFromOtherFactoryAddToBufferSucceeds()
        {
            IVectorBuffer<DoubleComponent, BufferedCoordinate> buffer = new BufferedCoordinateFactory();
            BufferedCoordinateFactory otherFactory = new BufferedCoordinateFactory();
            BufferedCoordinate vector = otherFactory.Create(5, 5);

            Int32 index = buffer.Add(vector);

            Assert.NotEqual(vector, buffer[index]);
            Assert.True(vector.ValueEquals(buffer[index]));
        }

        [Fact]
        public void BufferedCoordinateAddToBufferFailsOnEmpty()
        {
            Assert.Throws<InvalidOperationException>(delegate
                              {
                                  IVectorBuffer<DoubleComponent, BufferedCoordinate> buffer =
                                      new BufferedCoordinateFactory();
                                  BufferedCoordinate emptyVector = new BufferedCoordinate();

                                  buffer.Add(emptyVector);
                              });
        }

        [Fact]
        public void DoublesAddToBufferSucceeds()
        {
            IVectorBuffer<DoubleComponent, BufferedCoordinate> buffer = new BufferedCoordinateFactory();

            BufferedCoordinate result = buffer.Add(1, 2);

            Assert.Equal(1, result.X);
            Assert.Equal(2, result.Y);
        }

        [Fact]
        public void Doubles3DAddToBufferFails()
        {
            Assert.Throws<ArgumentException>(delegate
                              {
                                  IVectorBuffer<DoubleComponent, BufferedCoordinate> buffer =
                                      new BufferedCoordinateFactory();

                                  buffer.Add(1, 2, 3);
                              });
        }

        [Fact]
        public void ClearBufferSucceeds()
        {
            BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
            factory.Create(1, 1);
            factory.Create(2, 2);

            IVectorBuffer<DoubleComponent, BufferedCoordinate> buffer = factory;

            Assert.Equal(2, buffer.Count);

            buffer.Clear();
            Assert.Equal(0, buffer.Count);
        }

        [Fact]
        public void BufferContainsIVectorSucceeds()
        {
            Assert.Throws<NotImplementedException>(delegate
             {
                 MockRepository mocks = new MockRepository();
                 IVector<DoubleComponent> vector =
                     mocks.Stub<IVector<DoubleComponent>>();
                 Expect.Call(vector.ComponentCount).Repeat.Any().Return(2);
                 vector[0] = 5;
                 vector[1] = 6;
                 mocks.ReplayAll();

                 BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
                 IVectorBuffer<DoubleComponent, BufferedCoordinate> buffer = factory;

                 Assert.False(buffer.Contains(vector));

                 factory.Create(5, 6);

                 Assert.True(buffer.Contains(vector));
             });
        }

        [Fact]
        public void BufferContainsBufferedCoordinateSucceeds()
        {
            Assert.Throws<NotImplementedException>(delegate()
            {
                BufferedCoordinateFactory factory1 = new BufferedCoordinateFactory();
                BufferedCoordinateFactory factory2 = new BufferedCoordinateFactory();

                factory1.Create(1, 1);
                BufferedCoordinate coord1 = factory1.Create(2, 2);
                BufferedCoordinate coord2 = factory2.Create(2, 2);

                IVectorBuffer<DoubleComponent, BufferedCoordinate> buffer = factory1;

                Assert.True(buffer.Contains(coord1));  // Not implemented in NPack
                Assert.False(buffer.Contains(coord2));
            });
        }

        [Fact]
        public void CopyToSucceeds()
        {
            BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
            IVectorBuffer<DoubleComponent, BufferedCoordinate> buffer = factory;

            factory.Create(1, 1);
            factory.Create(1, 2);

            BufferedCoordinate[] result = new BufferedCoordinate[2];

            buffer.CopyTo(result, 0, 1);

            Assert.Equal(buffer[0], result[0]);
            Assert.Equal(buffer[1], result[1]);
        }

        [Fact]
        public void CountSucceeds()
        {
            BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
            factory.Create(1, 1);
            factory.Create(2, 2);

            IVectorBuffer<DoubleComponent, BufferedCoordinate> buffer = factory;

            Assert.Equal(2, buffer.Count);
        }

        //[Fact]
        //[ExpectedException(typeof(NotImplementedException))]
        //public void FactorySucceeds()
        //{
        //    BufferedCoordinateFactory factory = new BufferedCoordinateFactory();

        //    IVectorBuffer<DoubleComponent, BufferedCoordinate> buffer = factory;

        //    Assert.IsInstanceOfType(typeof(IVectorFactory<DoubleComponent, BufferedCoordinate>), buffer.Factory);
        //    // not implemented and possibly going away in NPack
        //}

        [Fact]
        public void IsReadOnlySucceeds()
        {
            BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
            IVectorBuffer<DoubleComponent, BufferedCoordinate> buffer = factory;

            Assert.False(buffer.IsReadOnly);
        }

        [Fact]
        public void GettingAndSettingMaximumSizeSucceeds()
        {
            BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
            IVectorBuffer<DoubleComponent, BufferedCoordinate> buffer = factory;

            buffer.MaximumSize = Int32.MaxValue - 1000;
            Assert.Equal(Int32.MaxValue - 1000, buffer.MaximumSize);
        }

        [Fact]
        public void SettingMaximumSizeFailsOnNegativeValue()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate
             {
                 BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
                 IVectorBuffer<DoubleComponent, BufferedCoordinate> buffer = factory;

                 buffer.MaximumSize = -1;
             });
        }

        [Fact]
        public void SettingMaximumSizeFailsOnSmallerValueThanCurrentContents()
        {
            Assert.Throws<InvalidOperationException>(delegate
                                {
                                    BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
                                    IVectorBuffer<DoubleComponent, BufferedCoordinate> buffer = factory;

                                    buffer.MaximumSize = 3;
                                });
        }

        [Fact]
        public void RemoveFails()
        {
            Assert.Throws<NotImplementedException>(delegate
                                {
                                    BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
                                    BufferedCoordinate coord = factory.Create(1, 1);
                                    IVectorBuffer<DoubleComponent, BufferedCoordinate> buffer = factory;

                                    buffer.Remove(coord.Index); // potentially invalidates sequences if allowed
                                });
        }

        [Fact(Skip = "Need to implement")]
        public void SizeIncreasedSucceeds()
        {

        }

        [Fact(Skip = "Need to implement")]
        public void SizeIncreasingSucceeds()
        {

        }

        [Fact(Skip = "Need to implement")]
        public void VectorChangedSucceeds()
        {

        }

        [Fact(Skip = "Need to implement")]
        public void VectorLengthSucceeds()
        {

        }

        [Fact]
        public void GettingIndexerSucceeds()
        {
            BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
            BufferedCoordinate coord = factory.Create(1, 1);
            IVectorBuffer<DoubleComponent, BufferedCoordinate> buffer = factory;

            Assert.Equal(coord, buffer[coord.Index]);
        }

        [Fact]
        public void SettingIndexerFails()
        {
            Assert.Throws<NotSupportedException>(delegate
             {
                 BufferedCoordinateFactory factory =
                     new BufferedCoordinateFactory();
                 BufferedCoordinate coord = factory.Create(1, 1);
                 IVectorBuffer<DoubleComponent, BufferedCoordinate> buffer =
                     factory;

                 // coordinate should be immutable except for possible precision snap-to
                 buffer[coord.Index] = factory.Create(2, 2);
             });
        }


        [Fact]
        public void EnumeratingVertexesSucceeds()
        {
            BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
            List<BufferedCoordinate> coordList = new List<BufferedCoordinate>(5);
            coordList.Add(factory.Create(1, 1));
            coordList.Add(factory.Create(1, 2));
            coordList.Add(factory.Create(1, 3));
            coordList.Add(factory.Create(1, 4));
            coordList.Add(factory.Create(1, 5));

            IVectorBuffer<DoubleComponent, BufferedCoordinate> buffer = factory;

            foreach (BufferedCoordinate coord in buffer)
            {
                Assert.True(coordList.Contains(coord));
            }
        }

        [Fact]
        public void CreatingMultipleCoordinatesWithTheSameValueReturnsExistingCoordinate()
        {
            BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
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

            Assert.Equal(4, factory.VectorBuffer.Count);
        }

        [Fact(Skip = "Test to check performance")]
        public void Creating1MRandomCoordinatesDoesntKillPerformance()
        {
            //BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
            //Random random = new MersenneTwister();
            //Stopwatch timer = new Stopwatch();

            //Int32[] times = new Int32[10];

            //Double firstAvg = -1;
            //Double secondAvg = -1;
            //Double thirdAvg = -1;
            //Double avg = 0;

            //for (Int32 i = 0; i < 1000000; i++)
            //{
            //    Double x = random.NextDouble();
            //    Double y = random.NextDouble();

            //    timer.Start();
            //    factory.Create(x, y);
            //    timer.Stop();

            //    times[i % 10] = (Int32)timer.ElapsedTicks;

            //    if (i == 9) firstAvg = avg;
            //    if (i == 19) secondAvg = avg;
            //    if (i == 29) thirdAvg = avg;

            //    avg = Enumerable.Average(times);
            //    Assert.Less(avg, 300000.0); // 30 ms

            //    timer.Reset();
            //}

            //Console.WriteLine("First average time to create a coordinate: {0:N4} ms", firstAvg / 10000);
            //Console.WriteLine("Second average time to create a coordinate: {0:N4} ms", secondAvg / 10000);
            //Console.WriteLine("Third average time to create a coordinate: {0:N4} ms", thirdAvg / 10000);
            //Console.WriteLine("Average time to create a coordinate: {0:N4} ms", avg / 10000);
        }

        [Fact(Skip = "Test to check performance")]
        public void Creating1MRepeatedCoordinatesDoesntKillPerformance()
        {
            //BufferedCoordinateFactory factory = new BufferedCoordinateFactory();
            //Random random = new MersenneTwister();
            //Stopwatch timer = new Stopwatch();

            //Int32[] times = new Int32[10];

            //Double firstAvg = -1;
            //Double secondAvg = -1;
            //Double thirdAvg = -1;
            //Double avg = 0;

            //for (Int32 i = 0; i < 1000000; i++)
            //{
            //    Double x = random.Next(500, 1000);
            //    Double y = random.Next(100000, 100500);

            //    timer.Start();
            //    factory.Create(x, y);
            //    timer.Stop();

            //    times[i % 10] = (Int32)timer.ElapsedTicks;

            //    if (i == 9) firstAvg = avg;
            //    if (i == 19) secondAvg = avg;
            //    if (i == 29) thirdAvg = avg;

            //    avg = Enumerable.Average(times);
            //    Assert.Less(avg, 100000.0); // 10 ms

            //    timer.Reset();
            //}

            //Console.WriteLine("First average time to create a coordinate: {0:N4} ms", firstAvg / 10000);
            //Console.WriteLine("Second average time to create a coordinate: {0:N4} ms", secondAvg / 10000);
            //Console.WriteLine("Third average time to create a coordinate: {0:N4} ms", thirdAvg / 10000);
            //Console.WriteLine("Last average time to create a coordinate: {0:N4} ms", avg / 10000);
        }

        [Fact(Skip = "Test to check performance")]
        public void CreatingSequencesWith1MRandomCoordinatesDoesntKillPerformance()
        {
            //BufferedCoordinateFactory coordFactory = new BufferedCoordinateFactory();
            //BufferedCoordinateSequenceFactory sequenceFactory
            //    = new BufferedCoordinateSequenceFactory(coordFactory);

            //Random random = new MersenneTwister();

            //List<ICoordinateSequence<BufferedCoordinate>> sequences =
            //    new List<ICoordinateSequence<BufferedCoordinate>>();

            //Int32 i = 0;

            //// setup sequences
            //for (; i < 10000; i++)
            //{
            //    ICoordinateSequence<BufferedCoordinate> sequence
            //        = sequenceFactory.Create(250, CoordinateDimensions.Two);

            //    for (Int32 j = 0; j < 250; j++)
            //    {
            //        BufferedCoordinate coord = coordFactory.Create(
            //            random.Next(420000000, 440000000) / 100.0,
            //            random.Next(3500000, 8000000) / 100.0);

            //        sequence[j] = coord;
            //    }

            //    sequences.Add(sequence);
            //}

            //Stopwatch timer = new Stopwatch();

            //Int32[] times = new Int32[10];

            //Double firstAvg = -1;
            //Double secondAvg = -1;
            //Double thirdAvg = -1;
            //Double avg = 0;

            //i = 0;

            //foreach (ICoordinateSequence<BufferedCoordinate> sequence in sequences)
            //{
            //    timer.Start();
            //    Assert.True(sequence.Maximum.GreaterThanOrEqualTo(sequence.Minimum));
            //    timer.Stop();

            //    times[i % 10] = (Int32)timer.ElapsedTicks;

            //    if (i == 9) firstAvg = avg;
            //    if (i == 19) secondAvg = avg;
            //    if (i == 29) thirdAvg = avg;

            //    avg = Enumerable.Average(times);
            //    Assert.Less(avg, 10000000.0); // 1000 ms

            //    i++;

            //    timer.Reset();
            //}

            //Console.WriteLine("First average time to compute Minimum and Maximum for a sequence {0:N4} ms", firstAvg / 10000);
            //Console.WriteLine("Second average time to compute Minimum and Maximum for a sequence: {0:N4} ms", secondAvg / 10000);
            //Console.WriteLine("Third average time to compute Minimum and Maximum for a sequence: {0:N4} ms", thirdAvg / 10000);
            //Console.WriteLine("Average time to compute Minimum and Maximum for a sequence: {0:N4} ms", avg / 10000);
        }

        [Fact]
        public void BitResolutionSnapsCoordinatesToGrid()
        {
            throw new NotImplementedException("Need to put correct scale here...");
            BufferedCoordinateFactory factory = new BufferedCoordinateFactory(24);

            factory.Create(10, 10);
            factory.Create(10.000000003, 10.000000003);

            Assert.Equal(1, factory.VectorBuffer.Count);
        }

        #region Private helper methods

        private static IEnumerable<BufferedCoordinate> generateInfiniteCoordinates(BufferedCoordinateFactory factory, Random rnd)
        {
            // Give thanks for Enumerable.Take()!
            while (true)
            {
                yield return factory.Create(rnd.NextDouble(), rnd.NextDouble());
            }
        }

        private static IEnumerable<BufferedCoordinate> generateInfiniteHomogeneousCoordinates(BufferedCoordinateFactory factory, Random rnd)
        {
            return factory.Homogenize(generateInfiniteCoordinates(factory, rnd));
        }
        #endregion
    }
}
