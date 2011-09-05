using System;
using System.Collections.Generic;
using NetTopologySuite.Coordinates;
using NPack;
//using NUnit.Framework;
using NUnit.Framework;

#if DOTNET35
using System.Linq;
using sl = System.Linq;
#else
using GeoAPI.DataStructures;
using sl = GeoAPI.DataStructures;
#endif

namespace SimpleCoordinateTests
{
    [NUnit.Framework.TestFixture]
    public class CoordinateFactoryTests
    {
        [Test]
        public void CreateFactorySucceeds()
        {
            CoordinateFactory factory = new CoordinateFactory();
            Assert.IsNotNull(factory);
        }

        [Test]
        public void CreateCoordinateSucceeds()
        {
            CoordinateFactory factory = new CoordinateFactory();
            Coordinate coord = factory.Create(5, 10);
            Assert.AreEqual(5, coord.X);
            Assert.AreEqual(10, coord.Y);
            Assert.IsTrue(Double.IsNaN(coord.Z));
        }

        [Test]
        public void CreateTwoCoordinateSucceeds()
        {
            CoordinateFactory factory = new CoordinateFactory();
            Coordinate coord1 = factory.Create(1, 15);
            Coordinate coord2 = factory.Create(15, 1);

            Assert.AreEqual(1, coord1.X);
            Assert.AreEqual(15, coord1.Y);
            Assert.IsTrue(Double.IsNaN(coord1.Z));
            Assert.AreEqual(15, coord2.X);
            Assert.AreEqual(1, coord2.Y);
            Assert.IsTrue(Double.IsNaN(coord1.Z));
        }

        //[Test(Skip = "Shall not fail!")]
        //public void Create3DCoordinateFails()
        //{
        //    Assert.Throws<NotSupportedException>(delegate()
        //                                             {
        //                                                 CoordinateFactory factory = new CoordinateFactory();
        //                                                 factory.Create3D(5, 10, 15);
        //                                             });
        //}

        [Test]
        public void Create3D()
        {
            CoordinateFactory factory = new CoordinateFactory();
            Coordinate coord = factory.Create3D(1, 15, 7);
            Assert.AreEqual(1, coord.X);
            Assert.AreEqual(15, coord.Y);
            Assert.AreEqual(7, coord.Z);
        }

        [Test]
        public void CreateCoordinateCopySucceeds()
        {
            CoordinateFactory factory = new CoordinateFactory();
            Coordinate coord = factory.Create(5, 10);
            Coordinate clone = factory.Create(coord);

            Assert.AreEqual(coord.X, clone.X);
            Assert.AreEqual(coord.Y, clone.Y);
        }

        [Test]
        public void CreateCoordinateCopyOfEmptySucceeds()
        {
            CoordinateFactory factory = new CoordinateFactory();
            Coordinate coord = new Coordinate();
            Coordinate clone = factory.Create(coord);

            Assert.IsTrue(clone.IsEmpty);
        }

        //[Test][Ignore( "Not Implemented")]
        //public void CreateAffineTransformSucceeds()
        //{
        //    CoordinateFactory factory = new CoordinateFactory();
        //    //AffineTransformMatrix<Coordinate> matrix = factory.CreateTransform();

        //    //Assert.IsNotNull(matrix);
        //}

        //[Test][Ignore( "Not Implemented")]
        //public void CreateAffineTransformWithAxisOfRotationSucceeds()
        //{
        //    CoordinateFactory factory = new CoordinateFactory();
        //    //AffineTransformMatrix<Coordinate> matrix = factory.CreateTransform();

        //    //Assert.IsNotNull(matrix);
        //}

        [Test]
        public void CreateHomogenizedCoordinateSucceeds()
        {
            CoordinateFactory factory = new CoordinateFactory();
            Coordinate coord = factory.Create(5, 10);
            Coordinate homogeneous = factory.Homogenize(coord);

            Assert.AreEqual(2, coord.ComponentCount);
            Assert.AreEqual(3, homogeneous.ComponentCount);
        }

        [Test]
        public void CreateHomogenizedCoordinateStreamSucceeds()
        {
            CoordinateFactory factory = new CoordinateFactory();
            Random rnd = new MersenneTwister();
            Int32 count = 10000;

            IEnumerable<Coordinate> coordinates = generateInfiniteCoordinates(factory, rnd);

#if DOTNET35
            IEnumerable<Coordinate> homogeneous = factory.Homogenize(coordinates.Take(count));
#else
            IEnumerable<Coordinate> homogeneous = factory.Homogenize(Enumerable.Take(coordinates, 10000));
#endif

            foreach (Coordinate coord in homogeneous)
            {
                Assert.AreEqual(3, coord.ComponentCount);
                count--;
            }

            Assert.AreEqual(0, count);
        }

        [Test]
        public void CreateDehomogenizedCoordinateSucceeds()
        {
            CoordinateFactory factory = new CoordinateFactory();
            Coordinate coord = factory.Create(5, 10);
            Coordinate homogeneous = factory.Homogenize(coord);
            Coordinate dehomogeneous = factory.Dehomogenize(homogeneous);

            Assert.AreEqual(2, coord.ComponentCount);
            Assert.AreEqual(3, homogeneous.ComponentCount);
            Assert.AreEqual(2, dehomogeneous.ComponentCount);
        }

        [Test]
        public void CreateDehomogenizedCoordinateStreamSucceeds()
        {
            CoordinateFactory factory = new CoordinateFactory();
            Random rnd = new MersenneTwister();
            Int32 count = 10000;

            IEnumerable<Coordinate> homogeneous = generateInfiniteHomogeneousCoordinates(factory, rnd);
            IEnumerable<Coordinate> dehomogeneous = sl.Enumerable.Take(factory.Dehomogenize(homogeneous), count);

            foreach (Coordinate coord in dehomogeneous)
            {
                Assert.AreEqual(2, coord.ComponentCount);
                count--;
            }

            Assert.AreEqual(0, count);
        }

        //[Test]
        //public void CoordinateAddToBufferSucceeds_1()
        //{
        //    MockRepository mocks = new MockRepository();
        //    IVector<DoubleComponent> vector = mocks.Stub<IVector<DoubleComponent>>();
        //    Expect.Call(vector.ComponentCount).Repeat.Any().Return(2);
        //    vector[0] = 5;
        //    vector[1] = 6;
        //    mocks.ReplayAll();

        //    CoordinateFactory buffer = new CoordinateFactory();

        //    Int32 index = buffer.Add(vector);

        //    Assert.AreEqual(5, buffer[index].X);
        //    Assert.AreEqual(6, buffer[index].Y);
        //}

        //[Test]
        //public void Coordinate3DAddToBufferFails()
        //{
        //    Assert.Throws<ArgumentException>(delegate()
        //                                         {
        //                                             MockRepository mocks = new MockRepository();
        //                                             IVector<DoubleComponent> vector = mocks.Stub<IVector<DoubleComponent>>();
        //                                             Expect.Call(vector.ComponentCount).Repeat.Any().Return(3);
        //                                             vector[0] = 5;
        //                                             vector[1] = 6;
        //                                             vector[2] = 7;
        //                                             mocks.ReplayAll();

        //                                             IVectorBuffer<DoubleComponent, Coordinate> buffer =
        //                                                 new CoordinateFactory();

        //                                             buffer.Add(vector);
        //                                         });
        //}

        //[Test]
        //public void CoordinateAddToBufferSucceeds()
        //{
        //    CoordinateFactory factory = new CoordinateFactory();
        //    IVectorBuffer<DoubleComponent, Coordinate> buffer = factory;
        //    Coordinate vector = factory.Create(5, 5);

        //    Int32 index = buffer.Add(vector);

        //    Assert.AreEqual(vector, buffer[index]);
        //}

        //[Test]
        //public void CoordinateFromOtherFactoryAddToBufferSucceeds()
        //{
        //    IVectorBuffer<DoubleComponent, Coordinate> buffer = new CoordinateFactory();
        //    CoordinateFactory otherFactory = new CoordinateFactory();
        //    Coordinate vector = otherFactory.Create(5, 5);

        //    Int32 index = buffer.Add(vector);

        //    Assert.NotEqual(vector, buffer[index]);
        //    Assert.IsTrue(vector.ValueEquals(buffer[index]));
        //}

        //[Test]
        //public void CoordinateAddToBufferFailsOnEmpty()
        //{
        //    Assert.Throws<InvalidOperationException>(delegate
        //                                                 {
        //                                                     IVectorBuffer<DoubleComponent, Coordinate> buffer =
        //                                                         new CoordinateFactory();
        //                                                     Coordinate emptyVector = new Coordinate();

        //                                                     buffer.Add(emptyVector);
        //                                                 });
        //}

        //[Test]
        //public void DoublesAddToBufferSucceeds()
        //{
        //    IVectorBuffer<DoubleComponent, Coordinate> buffer = new CoordinateFactory();

        //    Coordinate result = buffer.Add(1, 2);

        //    Assert.AreEqual(1, result.X);
        //    Assert.AreEqual(2, result.Y);
        //}

        //[Test]
        //public void Doubles3DAddToBufferFails()
        //{
        //    Assert.Throws<ArgumentException>(delegate
        //                                         {
        //                                             IVectorBuffer<DoubleComponent, Coordinate> buffer =
        //                                                 new CoordinateFactory();

        //                                             buffer.Add(1, 2, 3);
        //                                         });
        //}

        //[Test]
        //public void ClearBufferSucceeds()
        //{
        //    CoordinateFactory factory = new CoordinateFactory();
        //    factory.Create(1, 1);
        //    factory.Create(2, 2);

        //    IVectorBuffer<DoubleComponent, Coordinate> buffer = factory;

        //    Assert.AreEqual(2, buffer.Count);

        //    buffer.Clear();
        //    Assert.AreEqual(0, buffer.Count);
        //}

        //[Test]
        //public void BufferContainsIVectorSucceeds()
        //{
        //    Assert.Throws<NotImplementedException>(delegate
        //                                               {
        //                                                   MockRepository mocks = new MockRepository();
        //                                                   IVector<DoubleComponent> vector =
        //                                                       mocks.Stub<IVector<DoubleComponent>>();
        //                                                   Expect.Call(vector.ComponentCount).Repeat.Any().Return(2);
        //                                                   vector[0] = 5;
        //                                                   vector[1] = 6;
        //                                                   mocks.ReplayAll();

        //                                                   CoordinateFactory factory = new CoordinateFactory();
        //                                                   IVectorBuffer<DoubleComponent, Coordinate> buffer = factory;

        //                                                   Assert.IsFalse(buffer.Contains(vector));

        //                                                   factory.Create(5, 6);

        //                                                   Assert.IsTrue(buffer.Contains(vector));
        //                                               });
        //}

        //[Test]
        //public void BufferContainsCoordinateSucceeds()
        //{
        //    Assert.Throws<NotImplementedException>(delegate()
        //                                               {
        //                                                   CoordinateFactory factory1 = new CoordinateFactory();
        //                                                   CoordinateFactory factory2 = new CoordinateFactory();

        //                                                   factory1.Create(1, 1);
        //                                                   Coordinate coord1 = factory1.Create(2, 2);
        //                                                   Coordinate coord2 = factory2.Create(2, 2);

        //                                                   IVectorBuffer<DoubleComponent, Coordinate> buffer = factory1;

        //                                                   Assert.IsTrue(buffer.Contains(coord1));  // Not implemented in NPack
        //                                                   Assert.IsFalse(buffer.Contains(coord2));
        //                                               });
        //}

        //[Test]
        //public void CopyToSucceeds()
        //{
        //    CoordinateFactory factory = new CoordinateFactory();
        //    IVectorBuffer<DoubleComponent, Coordinate> buffer = factory;

        //    factory.Create(1, 1);
        //    factory.Create(1, 2);

        //    Coordinate[] result = new Coordinate[2];

        //    buffer.CopyTo(result, 0, 1);

        //    Assert.AreEqual(buffer[0], result[0]);
        //    Assert.AreEqual(buffer[1], result[1]);
        //}

        //[Test]
        //public void CountSucceeds()
        //{
        //    CoordinateFactory factory = new CoordinateFactory();
        //    factory.Create(1, 1);
        //    factory.Create(2, 2);

        //    IVectorBuffer<DoubleComponent, Coordinate> buffer = factory;

        //    Assert.AreEqual(2, buffer.Count);
        //}

        //[Test]
        //[ExpectedException(typeof(NotImplementedException))]
        //public void FactorySucceeds()
        //{
        //    CoordinateFactory factory = new CoordinateFactory();

        //    IVectorBuffer<DoubleComponent, Coordinate> buffer = factory;

        //    Assert.IsInstanceOfType(typeof(IVectorFactory<DoubleComponent, Coordinate>), buffer.Factory);
        //    // not implemented and possibly going away in NPack
        //}

        //[Test]
        //public void IsReadOnlySucceeds()
        //{
        //    CoordinateFactory factory = new CoordinateFactory();
        //    IVectorBuffer<DoubleComponent, Coordinate> buffer = factory;

        //    Assert.IsFalse(buffer.IsReadOnly);
        //}

        //[Test]
        //public void GettingAndSettingMaximumSizeSucceeds()
        //{
        //    CoordinateFactory factory = new CoordinateFactory();
        //    IVectorBuffer<DoubleComponent, Coordinate> buffer = factory;

        //    buffer.MaximumSize = Int32.MaxValue - 1000;
        //    Assert.AreEqual(Int32.MaxValue - 1000, buffer.MaximumSize);
        //}

        //[Test]
        //public void SettingMaximumSizeFailsOnNegativeValue()
        //{
        //    Assert.Throws<ArgumentOutOfRangeException>(delegate
        //                                                   {
        //                                                       CoordinateFactory factory = new CoordinateFactory();
        //                                                       IVectorBuffer<DoubleComponent, Coordinate> buffer = factory;

        //                                                       buffer.MaximumSize = -1;
        //                                                   });
        //}

        //[Test]
        //public void SettingMaximumSizeFailsOnSmallerValueThanCurrentContents()
        //{
        //    Assert.Throws<InvalidOperationException>(delegate
        //                                                 {
        //                                                     CoordinateFactory factory = new CoordinateFactory();
        //                                                     IVectorBuffer<DoubleComponent, Coordinate> buffer = factory;

        //                                                     buffer.MaximumSize = 3;
        //                                                 });
        //}

        //[Test]
        //public void RemoveFails()
        //{
        //    Assert.Throws<NotImplementedException>(delegate
        //                                               {
        //                                                   CoordinateFactory factory = new CoordinateFactory();
        //                                                   Coordinate coord = factory.Create(1, 1);
        //                                                   IVectorBuffer<DoubleComponent, Coordinate> buffer = factory;

        //                                                   buffer.Remove(coord.Index); // potentially invalidates sequences if allowed
        //                                               });
        //}

        //[Test][Ignore( "Need to implement")]
        //public void SizeIncreasedSucceeds()
        //{
        //}

        //[Test][Ignore( "Need to implement")]
        //public void SizeIncreasingSucceeds()
        //{
        //}

        //[Test][Ignore( "Need to implement")]
        //public void VectorChangedSucceeds()
        //{
        //}

        //[Test][Ignore( "Need to implement")]
        //public void VectorLengthSucceeds()
        //{
        //}

        //[Test]
        //public void GettingIndexerSucceeds()
        //{
        //    CoordinateFactory factory = new CoordinateFactory();
        //    Coordinate coord = factory.Create(1, 1);
        //    IVectorBuffer<DoubleComponent, Coordinate> buffer = factory;

        //    Assert.AreEqual(coord, buffer[coord.Index]);
        //}

        //[Test]
        //public void SettingIndexerFails()
        //{
        //    Assert.Throws<NotSupportedException>(delegate
        //                                             {
        //                                                 CoordinateFactory factory =
        //                                                     new CoordinateFactory();
        //                                                 Coordinate coord = factory.Create(1, 1);
        //                                                 IVectorBuffer<DoubleComponent, Coordinate> buffer =
        //                                                     factory;

        //                                                 // coordinate should be immutable except for possible precision snap-to
        //                                                 buffer[coord.Index] = factory.Create(2, 2);
        //                                             });
        //}


        //[Test]
        //public void EnumeratingVertexesSucceeds()
        //{
        //    CoordinateFactory factory = new CoordinateFactory();
        //    List<Coordinate> coordList = new List<Coordinate>(5);
        //    coordList.Add(factory.Create(1, 1));
        //    coordList.Add(factory.Create(1, 2));
        //    coordList.Add(factory.Create(1, 3));
        //    coordList.Add(factory.Create(1, 4));
        //    coordList.Add(factory.Create(1, 5));

        //    IVectorBuffer<DoubleComponent, Coordinate> buffer = factory;

        //    foreach (Coordinate coord in buffer)
        //    {
        //        Assert.IsTrue(coordList.Contains(coord));
        //    }
        //}

        //[Test]
        //public void CreatingMultipleCoordinatesWithTheSameValueReturnsExistingCoordinate()
        //{
        //    CoordinateFactory factory = new CoordinateFactory();
        //    Random random = new MersenneTwister();

        //    for (Int32 i = 0; i < 100; i++)
        //    {
        //        switch (random.Next(0, 4))
        //        {
        //            case 0:
        //                factory.Create(0, 0);
        //                break;
        //            case 1:
        //                factory.Create(10, 10);
        //                break;
        //            case 2:
        //                factory.Create(0, 100);
        //                break;
        //            case 3:
        //                factory.Create(100, 100);
        //                break;
        //            default:
        //                break;
        //        }
        //    }

        //    Assert.AreEqual(4, factory.VectorBuffer.Count);
        //}

        //[Test][Ignore( "Test to check performance")]
        //public void Creating1MRandomCoordinatesDoesntKillPerformance()
        //{
        //    //CoordinateFactory factory = new CoordinateFactory();
        //    //Random random = new MersenneTwister();
        //    //Stopwatch timer = new Stopwatch();

        //    //Int32[] times = new Int32[10];

        //    //Double firstAvg = -1;
        //    //Double secondAvg = -1;
        //    //Double thirdAvg = -1;
        //    //Double avg = 0;

        //    //for (Int32 i = 0; i < 1000000; i++)
        //    //{
        //    //    Double x = random.NextDouble();
        //    //    Double y = random.NextDouble();

        //    //    timer.Start();
        //    //    factory.Create(x, y);
        //    //    timer.Stop();

        //    //    times[i % 10] = (Int32)timer.ElapsedTicks;

        //    //    if (i == 9) firstAvg = avg;
        //    //    if (i == 19) secondAvg = avg;
        //    //    if (i == 29) thirdAvg = avg;

        //    //    avg = Enumerable.Average(times);
        //    //    Assert.Less(avg, 300000.0); // 30 ms

        //    //    timer.Reset();
        //    //}

        //    //Console.WriteLine("First average time to create a coordinate: {0:N4} ms", firstAvg / 10000);
        //    //Console.WriteLine("Second average time to create a coordinate: {0:N4} ms", secondAvg / 10000);
        //    //Console.WriteLine("Third average time to create a coordinate: {0:N4} ms", thirdAvg / 10000);
        //    //Console.WriteLine("Average time to create a coordinate: {0:N4} ms", avg / 10000);
        //}

        //[Test][Ignore( "Test to check performance")]
        //public void Creating1MRepeatedCoordinatesDoesntKillPerformance()
        //{
        //    //CoordinateFactory factory = new CoordinateFactory();
        //    //Random random = new MersenneTwister();
        //    //Stopwatch timer = new Stopwatch();

        //    //Int32[] times = new Int32[10];

        //    //Double firstAvg = -1;
        //    //Double secondAvg = -1;
        //    //Double thirdAvg = -1;
        //    //Double avg = 0;

        //    //for (Int32 i = 0; i < 1000000; i++)
        //    //{
        //    //    Double x = random.Next(500, 1000);
        //    //    Double y = random.Next(100000, 100500);

        //    //    timer.Start();
        //    //    factory.Create(x, y);
        //    //    timer.Stop();

        //    //    times[i % 10] = (Int32)timer.ElapsedTicks;

        //    //    if (i == 9) firstAvg = avg;
        //    //    if (i == 19) secondAvg = avg;
        //    //    if (i == 29) thirdAvg = avg;

        //    //    avg = Enumerable.Average(times);
        //    //    Assert.Less(avg, 100000.0); // 10 ms

        //    //    timer.Reset();
        //    //}

        //    //Console.WriteLine("First average time to create a coordinate: {0:N4} ms", firstAvg / 10000);
        //    //Console.WriteLine("Second average time to create a coordinate: {0:N4} ms", secondAvg / 10000);
        //    //Console.WriteLine("Third average time to create a coordinate: {0:N4} ms", thirdAvg / 10000);
        //    //Console.WriteLine("Last average time to create a coordinate: {0:N4} ms", avg / 10000);
        //}

        //[Test][Ignore( "Test to check performance")]
        //public void CreatingSequencesWith1MRandomCoordinatesDoesntKillPerformance()
        //{
        //    //CoordinateFactory coordFactory = new CoordinateFactory();
        //    //CoordinateSequenceFactory sequenceFactory
        //    //    = new CoordinateSequenceFactory(coordFactory);

        //    //Random random = new MersenneTwister();

        //    //List<ICoordinateSequence<Coordinate>> sequences =
        //    //    new List<ICoordinateSequence<Coordinate>>();

        //    //Int32 i = 0;

        //    //// setup sequences
        //    //for (; i < 10000; i++)
        //    //{
        //    //    ICoordinateSequence<Coordinate> sequence
        //    //        = sequenceFactory.Create(250, CoordinateDimensions.Two);

        //    //    for (Int32 j = 0; j < 250; j++)
        //    //    {
        //    //        Coordinate coord = coordFactory.Create(
        //    //            random.Next(420000000, 440000000) / 100.0,
        //    //            random.Next(3500000, 8000000) / 100.0);

        //    //        sequence[j] = coord;
        //    //    }

        //    //    sequences.Add(sequence);
        //    //}

        //    //Stopwatch timer = new Stopwatch();

        //    //Int32[] times = new Int32[10];

        //    //Double firstAvg = -1;
        //    //Double secondAvg = -1;
        //    //Double thirdAvg = -1;
        //    //Double avg = 0;

        //    //i = 0;

        //    //foreach (ICoordinateSequence<Coordinate> sequence in sequences)
        //    //{
        //    //    timer.Start();
        //    //    Assert.IsTrue(sequence.Maximum.GreaterThanOrEqualTo(sequence.Minimum));
        //    //    timer.Stop();

        //    //    times[i % 10] = (Int32)timer.ElapsedTicks;

        //    //    if (i == 9) firstAvg = avg;
        //    //    if (i == 19) secondAvg = avg;
        //    //    if (i == 29) thirdAvg = avg;

        //    //    avg = Enumerable.Average(times);
        //    //    Assert.Less(avg, 10000000.0); // 1000 ms

        //    //    i++;

        //    //    timer.Reset();
        //    //}

        //    //Console.WriteLine("First average time to compute Minimum and Maximum for a sequence {0:N4} ms", firstAvg / 10000);
        //    //Console.WriteLine("Second average time to compute Minimum and Maximum for a sequence: {0:N4} ms", secondAvg / 10000);
        //    //Console.WriteLine("Third average time to compute Minimum and Maximum for a sequence: {0:N4} ms", thirdAvg / 10000);
        //    //Console.WriteLine("Average time to compute Minimum and Maximum for a sequence: {0:N4} ms", avg / 10000);
        //}

        //[Test][Ignore( "Not implemented")]
        //public void BitResolutionSnapsCoordinatesToGrid()
        //{
        //    throw new NotImplementedException("Need to put correct scale here...");
        //    //CoordinateFactory factory = new CoordinateFactory(24);

        //    //factory.Create(10, 10);
        //    //factory.Create(10.000000003, 10.000000003);

        //    //Assert.AreEqual(1, factory.VectorBuffer.Count);
        //}

        #region Private helper methods

        private static IEnumerable<Coordinate> generateInfiniteCoordinates(CoordinateFactory factory, Random rnd)
        {
            // Give thanks for Enumerable.Take()!
            while (true)
            {
                yield return factory.Create(rnd.NextDouble(), rnd.NextDouble());
            }
        }

        private static IEnumerable<Coordinate> generateInfiniteHomogeneousCoordinates(CoordinateFactory factory, Random rnd)
        {
            return factory.Homogenize(generateInfiniteCoordinates(factory, rnd));
        }
        #endregion
    }
}