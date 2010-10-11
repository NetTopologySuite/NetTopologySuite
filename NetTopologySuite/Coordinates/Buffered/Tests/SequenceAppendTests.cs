using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NetTopologySuite.Coordinates;
using NUnit.Framework;

namespace ManagedBufferedCoordinateTests
{
    using IBufferedCoordSequence = ICoordinateSequence<BufferedCoordinate>;

    public class SequenceAppendTests
    {
        private const int BigMaxLimit = Int32.MaxValue - 2;

        [Test]
        public void CoordinateToNewSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);

            BufferedCoordinate coord = generator.RandomCoordinate();

            generator.Sequence.Append(coord);

            Assert.AreEqual(generator.MainList[mainLength - 1], generator.Sequence[mainLength - 1]);
            Assert.AreEqual(coord, generator.Sequence[mainLength]);
        }

        [Test]
        public void CoordinateToNewSlice()
        {
            Int32 mainLength = 5;
            Int32 endIndex = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, mainLength - 2);

            BufferedCoordinate coord = generator.RandomCoordinate();

            slice.Append(coord);

            Assert.AreEqual(generator.MainList[mainLength - 2], slice[endIndex - 1]);
            Assert.AreEqual(coord, slice[endIndex]);
        }

        [Test]
        public void CoordinateToAppendedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);

            BufferedCoordinate coord1 = generator.RandomCoordinate();
            BufferedCoordinate coord0 = generator.RandomCoordinate();

            generator.Sequence.Append(coord1);

            Assert.AreEqual(generator.MainList[mainLength - 1], generator.Sequence[mainLength - 1]);
            Assert.AreEqual(coord1, generator.Sequence[mainLength]);

            generator.Sequence.Append(coord0);

            Assert.AreEqual(generator.MainList[mainLength - 1], generator.Sequence[mainLength - 1]);
            Assert.AreEqual(coord1, generator.Sequence[mainLength]);
            Assert.AreEqual(coord0, generator.Sequence[mainLength + 1]);
        }

        [Test]
        public void CoordinateToAppendedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, mainLength - 2);

            BufferedCoordinate coord1 = generator.RandomCoordinate();
            BufferedCoordinate coord0 = generator.RandomCoordinate();

            slice.Append(coord1);

            Assert.AreEqual(generator.MainList[mainLength - 2], slice[sliceLength - 1]);
            Assert.AreEqual(coord1, slice[sliceLength]);

            slice.Append(coord0);

            Assert.AreEqual(generator.MainList[mainLength - 2], slice[sliceLength - 1]);
            Assert.AreEqual(coord1, slice[sliceLength]);
            Assert.AreEqual(coord0, slice[sliceLength + 1]);
        }

        [Test]
        public void EnumerationToNewSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);

            EnumerableIsolater<BufferedCoordinate> appendList = new EnumerableIsolater<BufferedCoordinate>(generator.AppendList);
            generator.Sequence.Append(appendList);

            Assert.AreEqual(generator.MainList[mainLength - 1], generator.Sequence[mainLength - 1]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], generator.Sequence[mainLength + i]);
            }
        }

        [Test]
        public void EnumerationToNewSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            EnumerableIsolater<BufferedCoordinate> appendList = new EnumerableIsolater<BufferedCoordinate>(generator.AppendList);
            slice.Append(appendList);

            Assert.AreEqual(generator.MainList[sliceLength], slice[sliceLength - 1]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + i]);
            }
        }

        [Test]
        public void EnumerationToAppendedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            generator.Sequence.Append(appendedCoordinate);

            EnumerableIsolater<BufferedCoordinate> appendList = new EnumerableIsolater<BufferedCoordinate>(generator.AppendList);
            generator.Sequence.Append(appendList);

            Assert.AreEqual(generator.MainList[mainLength - 1], generator.Sequence[mainLength - 1]);
            Assert.AreEqual(appendedCoordinate, generator.Sequence[mainLength]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], generator.Sequence[mainLength + 1 + i]);
            }
        }

        [Test]
        public void EnumerationToAppendedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            slice.Append(appendedCoordinate);

            EnumerableIsolater<BufferedCoordinate> appendList = new EnumerableIsolater<BufferedCoordinate>(generator.AppendList);
            slice.Append(appendList);

            Assert.AreEqual(generator.MainList[mainLength - 2], slice[sliceLength - 1]);
            Assert.AreEqual(appendedCoordinate, slice[sliceLength]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + 1 + i]);
            }
        }

        [Test]
        public void SequenceToNewSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            ICoordinateSequence<BufferedCoordinate> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            generator.Sequence.Append(appendSeq);

            Assert.AreEqual(generator.MainList[mainLength - 1], generator.Sequence[mainLength - 1]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], generator.Sequence[mainLength + i]);
            }
        }

        [Test]
        public void SequenceToNewSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            ICoordinateSequence<BufferedCoordinate> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            slice.Append(appendSeq);

            Assert.AreEqual(generator.MainList[sliceLength], slice[sliceLength - 1]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + i]);
            }
        }

        [Test]
        public void SequenceToAppendedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            generator.Sequence.Append(appendedCoordinate);

            ICoordinateSequence<BufferedCoordinate> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            generator.Sequence.Append(appendSeq);

            Assert.AreEqual(generator.MainList[mainLength - 1], generator.Sequence[mainLength - 1]);
            Assert.AreEqual(appendedCoordinate, generator.Sequence[mainLength]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], generator.Sequence[mainLength + 1 + i]);
            }
        }

        [Test]
        public void SequenceToAppendedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            slice.Append(appendedCoordinate);

            ICoordinateSequence<BufferedCoordinate> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            slice.Append(appendSeq);

            Assert.AreEqual(generator.MainList[sliceLength], slice[sliceLength - 1]);
            Assert.AreEqual(appendedCoordinate, slice[sliceLength]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + 1 + i]);
            }
        }

        [Test]
        public void ComplexSliceToAppendedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            generator.Sequence.Append(appendedCoordinate);

            IBufferedCoordSequence appendSlice = generator.SequenceFactory
                                                      .Create(generator.AppendList)
                                                      .Slice(0, 2);
            BufferedCoordinate preSliceCoordinate = generator.RandomCoordinate();
            BufferedCoordinate postSliceCoordinate = generator.RandomCoordinate();
            appendSlice.Prepend(preSliceCoordinate);
            appendSlice.Append(postSliceCoordinate);

            generator.Sequence.Append(appendSlice);

            BufferedCoordinate expected;
            BufferedCoordinate actual;

            // last coords are the same
            expected = generator.MainList[mainLength - 1];
            actual = generator.Sequence[mainLength - 1];
            Assert.AreEqual(expected, actual);

            // then we appended appendedCoordinate
            expected = appendedCoordinate;
            actual = generator.Sequence[mainLength];
            Assert.AreEqual(expected, actual);

            // then we appended a sequence with a prepended sequence, of which 
            // this one is first
            expected = preSliceCoordinate;
            actual = generator.Sequence[mainLength + 1];
            Assert.AreEqual(expected, actual);

            //for (Int32 i = 0; i <= generator.AppendList.TotalItemCount; i++)
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], generator.Sequence[mainLength + 2 + i]);
            }

            Assert.AreEqual(postSliceCoordinate, generator.Sequence[mainLength + 2 + generator.AppendList.Count]);
        }

        [Test]
        public void ComplexSliceToAppendedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            slice.Append(appendedCoordinate);

            IBufferedCoordSequence appendSlice
                = generator.SequenceFactory.Create(generator.AppendList)
                  .Slice(0, 2);
            BufferedCoordinate preSliceCoordinate = generator.RandomCoordinate();
            BufferedCoordinate postSliceCoordinate = generator.RandomCoordinate();
            appendSlice.Prepend(preSliceCoordinate);
            appendSlice.Append(postSliceCoordinate);

            slice.Append(appendSlice);

            Assert.AreEqual(generator.MainList[sliceLength], slice[sliceLength - 1]);
            Assert.AreEqual(appendedCoordinate, slice[sliceLength]);
            Assert.AreEqual(preSliceCoordinate, slice[sliceLength + 1]);

            //for (Int32 i = 0; i <= generator.AppendList.TotalItemCount; i++)
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + 2 + i]);
            }

            Assert.AreEqual(postSliceCoordinate, slice[sliceLength + 2 + generator.AppendList.Count]);
        }

        [Test]
        public void CoordinateToNewReversedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);
            generator.Sequence.Reverse();

            BufferedCoordinate coord = generator.RandomCoordinate();

            generator.Sequence.Append(coord);

            Assert.AreEqual(generator.MainList[0], generator.Sequence[mainLength - 1]);
            Assert.AreEqual(coord, generator.Sequence[mainLength]);
        }

        [Test]
        public void CoordinateToNewReversedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);
            slice.Reverse();

            BufferedCoordinate coord = generator.RandomCoordinate();

            slice.Append(coord);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 1]);
            Assert.AreEqual(coord, slice[sliceLength]);
        }

        [Test]
        public void CoordinateToAppendedReversedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);

            BufferedCoordinate coord1 = generator.RandomCoordinate();
            BufferedCoordinate coord0 = generator.RandomCoordinate();

            generator.Sequence.Reverse();
            generator.Sequence.Append(coord1);

            Assert.AreEqual(generator.MainList[0], generator.Sequence[mainLength - 1]);
            Assert.AreEqual(coord1, generator.Sequence[mainLength]);

            generator.Sequence.Append(coord0);

            Assert.AreEqual(generator.MainList[0], generator.Sequence[mainLength - 1]);
            Assert.AreEqual(coord1, generator.Sequence[mainLength]);
            Assert.AreEqual(coord0, generator.Sequence[mainLength + 1]);
        }

        [Test]
        public void CoordinateToReversedAppendedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);

            BufferedCoordinate coord1 = generator.RandomCoordinate();
            BufferedCoordinate coord0 = generator.RandomCoordinate();

            generator.Sequence.Append(coord1);

            Assert.AreEqual(generator.MainList[mainLength - 1], generator.Sequence[mainLength - 1]);
            Assert.AreEqual(coord1, generator.Sequence[mainLength]);
            generator.Sequence.Reverse();

            generator.Sequence.Append(coord0);

            Assert.AreEqual(generator.MainList[0], generator.Sequence[mainLength]);
            Assert.AreEqual(coord0, generator.Sequence[mainLength + 1]);
            Assert.AreEqual(coord1, generator.Sequence[0]);
        }

        [Test]
        public void CoordinateToAppendedReversedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            BufferedCoordinate coord1 = generator.RandomCoordinate();
            BufferedCoordinate coord0 = generator.RandomCoordinate();

            slice.Reverse();
            slice.Append(coord1);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 1]);
            Assert.AreEqual(coord1, slice[sliceLength]);

            slice.Append(coord0);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 1]);
            Assert.AreEqual(coord1, slice[sliceLength]);
            Assert.AreEqual(coord0, slice[sliceLength + 1]);
        }

        [Test]
        public void CoordinateToReversedAppendedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            BufferedCoordinate coord1 = generator.RandomCoordinate();
            BufferedCoordinate coord0 = generator.RandomCoordinate();

            slice.Append(coord1);

            Assert.AreEqual(generator.MainList[sliceLength], slice[sliceLength - 1]);
            Assert.AreEqual(coord1, slice[sliceLength]);
            slice.Reverse();

            slice.Append(coord0);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength]);
            Assert.AreEqual(coord0, slice[sliceLength + 1]);
            Assert.AreEqual(coord1, slice[0]);
        }

        [Test]
        public void EnumerationToNewReversedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            generator.Sequence.Reverse();

            BufferedCoordinate expected;
            BufferedCoordinate actual;
                        
            expected = generator.MainList[0];
            actual = generator.Sequence[mainLength - 1];
            Assert.AreEqual(expected, actual);

            EnumerableIsolater<BufferedCoordinate> appendList
                = new EnumerableIsolater<BufferedCoordinate>(generator.AppendList);
            generator.Sequence.Append(appendList);

            expected = generator.MainList[0];
            actual = generator.Sequence[mainLength - 1];
            Assert.AreEqual(expected, actual);
            
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                expected = generator.AppendList[i];
                actual = generator.Sequence[mainLength + i];
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void EnumerationToNewReversedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);
            slice.Reverse();

            EnumerableIsolater<BufferedCoordinate> appendList = new EnumerableIsolater<BufferedCoordinate>(generator.AppendList);
            slice.Append(appendList);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 1]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + i]);
            }
        }

        [Test]
        public void EnumerationToAppendedReversedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            generator.Sequence.Reverse();

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            generator.Sequence.Append(appendedCoordinate);

            EnumerableIsolater<BufferedCoordinate> appendList = new EnumerableIsolater<BufferedCoordinate>(generator.AppendList);
            generator.Sequence.Append(appendList);

            Assert.AreEqual(generator.MainList[0], generator.Sequence[mainLength - 1]);
            Assert.AreEqual(appendedCoordinate, generator.Sequence[mainLength]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], generator.Sequence[mainLength + 1 + i]);
            }
        }

        [Test]
        public void EnumerationToReversedAppendedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            generator.Sequence.Append(appendedCoordinate);
            generator.Sequence.Reverse();

            EnumerableIsolater<BufferedCoordinate> appendList = new EnumerableIsolater<BufferedCoordinate>(generator.AppendList);
            generator.Sequence.Append(appendList);

            Assert.AreEqual(generator.MainList[0], generator.Sequence[mainLength]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], generator.Sequence[mainLength + 1 + i]);
            }

            Assert.AreEqual(appendedCoordinate, generator.Sequence[0]);
        }

        [Test]
        public void EnumerationToAppendedReversedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);
            slice.Reverse();

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            slice.Append(appendedCoordinate);

            EnumerableIsolater<BufferedCoordinate> appendList = new EnumerableIsolater<BufferedCoordinate>(generator.AppendList);
            slice.Append(appendList);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 1]);
            Assert.AreEqual(appendedCoordinate, slice[sliceLength]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + 1 + i]);
            }
        }

        [Test]
        public void EnumerationToReversedAppendedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            slice.Append(appendedCoordinate);
            slice.Reverse();

            EnumerableIsolater<BufferedCoordinate> appendList = new EnumerableIsolater<BufferedCoordinate>(generator.AppendList);
            slice.Append(appendList);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + 1 + i]);
            }

            Assert.AreEqual(appendedCoordinate, slice[0]);
        }

        [Test]
        public void SequenceToNewReversedSequence()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            ICoordinateSequence<BufferedCoordinate> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            generator.Sequence.Reverse();
            generator.Sequence.Append(appendSeq);

            Assert.AreEqual(generator.MainList[0], generator.Sequence[mainLength - 1]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], generator.Sequence[mainLength + i]);
            }
        }

        [Test]
        public void SequenceToNewReversedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);
            slice.Reverse();

            ICoordinateSequence<BufferedCoordinate> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            slice.Append(appendSeq);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 1]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + i]);
            }
        }

        [Test]
        public void SequenceToAppendedReversedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            generator.Sequence.Reverse();

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            generator.Sequence.Append(appendedCoordinate);

            ICoordinateSequence<BufferedCoordinate> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            generator.Sequence.Append(appendSeq);

            Assert.AreEqual(generator.MainList[0], generator.Sequence[mainLength - 1]);
            Assert.AreEqual(appendedCoordinate, generator.Sequence[mainLength]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], generator.Sequence[mainLength + 1 + i]);
            }
        }

        [Test]
        public void SequenceToReversedAppendedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            generator.Sequence.Append(appendedCoordinate);
            generator.Sequence.Reverse();

            ICoordinateSequence<BufferedCoordinate> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            generator.Sequence.Append(appendSeq);

            Assert.AreEqual(generator.MainList[0], generator.Sequence[mainLength]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], generator.Sequence[mainLength + 1 + i]);
            }

            Assert.AreEqual(appendedCoordinate, generator.Sequence[0]);
        }

        [Test]
        public void SequenceToAppendedReversedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);
            slice.Reverse();

            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 1]);

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            slice.Append(appendedCoordinate);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 1]);
            Assert.AreEqual(appendedCoordinate, slice[sliceLength]);

            ICoordinateSequence<BufferedCoordinate> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            slice.Append(appendSeq);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 1]);
            Assert.AreEqual(appendedCoordinate, slice[sliceLength]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + 1 + i]);
            }
        }

        [Test]
        public void SequenceToReversedAppendedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            slice.Append(appendedCoordinate);
            slice.Reverse();

            Assert.AreEqual(generator.MainList[1], slice[sliceLength]);
            Assert.AreEqual(appendedCoordinate, slice[0]);

            ICoordinateSequence<BufferedCoordinate> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            slice.Append(appendSeq);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + 1 + i]);
            }

            Assert.AreEqual(appendedCoordinate, slice[0]);
        }

        [Test]
        public void ComplexSliceToAppendedReversedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            generator.Sequence.Reverse();

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            generator.Sequence.Append(appendedCoordinate);

            Assert.AreEqual(generator.MainList[0], generator.Sequence[mainLength - 1]);
            Assert.AreEqual(appendedCoordinate, generator.Sequence[mainLength]);

            IBufferedCoordSequence appendSlice
                = generator.SequenceFactory.Create(generator.AppendList)
                .Slice(0, 2);
            BufferedCoordinate preSliceCoordinate = generator.RandomCoordinate();
            BufferedCoordinate postSliceCoordinate = generator.RandomCoordinate();
            appendSlice.Prepend(preSliceCoordinate);
            appendSlice.Append(postSliceCoordinate);

            generator.Sequence.Append(appendSlice);

            Assert.AreEqual(generator.MainList[0], generator.Sequence[mainLength - 1]);
            Assert.AreEqual(appendedCoordinate, generator.Sequence[mainLength]);

            Assert.AreEqual(preSliceCoordinate, generator.Sequence[mainLength + 1]);

            //for (Int32 i = 0; i <= generator.AppendList.TotalItemCount; i++)
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], generator.Sequence[mainLength + 2 + i]);
            }

            Assert.AreEqual(postSliceCoordinate, generator.Sequence[mainLength + 2 + generator.AppendList.Count]);
        }

        [Test]
        public void ComplexSliceToReversedAppendedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            generator.Sequence.Append(appendedCoordinate);
            generator.Sequence.Reverse();

            Assert.AreEqual(generator.MainList[0], generator.Sequence[mainLength]);
            Assert.AreEqual(appendedCoordinate, generator.Sequence[0]);

            IBufferedCoordSequence appendSlice
                = generator.SequenceFactory.Create(generator.AppendList)
                .Slice(0, 2);
            BufferedCoordinate preSliceCoordinate = generator.RandomCoordinate();
            BufferedCoordinate postSliceCoordinate = generator.RandomCoordinate();
            appendSlice.Prepend(preSliceCoordinate);
            appendSlice.Append(postSliceCoordinate);

            generator.Sequence.Append(appendSlice);

            Assert.AreEqual(generator.MainList[0], generator.Sequence[mainLength]);

            Assert.AreEqual(preSliceCoordinate, generator.Sequence[mainLength + 1]);

            //for (Int32 i = 0; i <= generator.AppendList.TotalItemCount; i++)
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], generator.Sequence[mainLength + 2 + i]);
            }

            Assert.AreEqual(postSliceCoordinate, generator.Sequence[mainLength + 2 + generator.AppendList.Count]);

            Assert.AreEqual(appendedCoordinate, generator.Sequence[0]);
        }

        [Test]
        public void ComplexSliceToAppendedReversedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);
            slice.Reverse();

            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 1]);

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            slice.Append(appendedCoordinate);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 1]);
            Assert.AreEqual(appendedCoordinate, slice[sliceLength]);

            IBufferedCoordSequence appendSlice
                = generator.SequenceFactory.Create(generator.AppendList)
                .Slice(0, 2);
            BufferedCoordinate preSliceCoordinate = generator.RandomCoordinate();
            BufferedCoordinate postSliceCoordinate = generator.RandomCoordinate();
            appendSlice.Prepend(preSliceCoordinate);
            appendSlice.Append(postSliceCoordinate);

            slice.Append(appendSlice);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 1]);
            Assert.AreEqual(appendedCoordinate, slice[sliceLength]);

            Assert.AreEqual(preSliceCoordinate, slice[sliceLength + 1]);

            //for (Int32 i = 0; i <= generator.AppendList.TotalItemCount; i++)
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + 2 + i]);
            }

            Assert.AreEqual(postSliceCoordinate, slice[sliceLength + 2 + generator.AppendList.Count]);
        }

        [Test]
        public void ComplexSliceToReversedAppendedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            slice.Append(appendedCoordinate);
            slice.Reverse();

            Assert.AreEqual(generator.MainList[1], slice[sliceLength]);
            Assert.AreEqual(appendedCoordinate, slice[0]);

            IBufferedCoordSequence appendSlice
                = generator.SequenceFactory.Create(generator.AppendList)
                  .Slice(0, 2);
            BufferedCoordinate preSliceCoordinate = generator.RandomCoordinate();
            BufferedCoordinate postSliceCoordinate = generator.RandomCoordinate();
            appendSlice.Prepend(preSliceCoordinate);
            appendSlice.Append(postSliceCoordinate);

            slice.Append(appendSlice);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength]);

            Assert.AreEqual(preSliceCoordinate, slice[sliceLength + 1]);

            //for (Int32 i = 0; i <= generator.AppendList.TotalItemCount; i++)
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + 2 + i]);
            }

            Assert.AreEqual(postSliceCoordinate, slice[sliceLength + 2 + generator.AppendList.Count]);

            Assert.AreEqual(appendedCoordinate, slice[0]);
        }

        [Test]
        public void CoordinateToAppendedSliceWithSkip()
        {
            Int32 mainLength = 12;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            Assert.IsTrue(slice.Remove(generator.MainList[3]));
            Assert.IsTrue(slice.Remove(generator.MainList[5]));
            Assert.IsTrue(slice.Remove(generator.MainList[7]));

            BufferedCoordinate coord1 = generator.RandomCoordinate();
            BufferedCoordinate coord0 = generator.RandomCoordinate();

            slice.Append(coord1);

            Assert.AreEqual(sliceLength - 3 + 1, slice.Count);
            Assert.AreEqual(generator.MainList[sliceLength], slice[sliceLength - 3 - 1]);
            Assert.AreEqual(coord1, slice[sliceLength - 3]);

            slice.Append(coord0);

            Assert.AreEqual(generator.MainList[sliceLength], slice[sliceLength - 3 - 1]);
            Assert.AreEqual(coord1, slice[sliceLength - 3]);
            Assert.AreEqual(coord0, slice[sliceLength - 3 + 1]);
        }

        [Test]
        public void EnumerationToAppendedSliceWithSkip()
        {
            Int32 mainLength = 12;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            Assert.IsTrue(slice.Remove(generator.MainList[3]));
            Assert.IsTrue(slice.Remove(generator.MainList[5]));
            Assert.IsTrue(slice.Remove(generator.MainList[7]));

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            slice.Append(appendedCoordinate);

            Assert.AreEqual(sliceLength - 3 + 1, slice.Count);
            Assert.AreEqual(generator.MainList[sliceLength], slice[sliceLength - 3 - 1]);
            Assert.AreEqual(appendedCoordinate, slice[sliceLength - 3]);

            EnumerableIsolater<BufferedCoordinate> appendList 
                = new EnumerableIsolater<BufferedCoordinate>(generator.AppendList);
            slice.Append(appendList);

            Assert.AreEqual(sliceLength - 3 + 1 + generator.AppendList.Count, slice.Count);
            Assert.AreEqual(generator.MainList[sliceLength], slice[sliceLength - 3 - 1]);
            Assert.AreEqual(appendedCoordinate, slice[sliceLength - 3]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength - 3 + 1 + i]);
            }
        }

        [Test]
        public void SequenceToAppendedSliceWithSkip()
        {
            Int32 mainLength = 12;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            Assert.IsTrue(slice.Remove(generator.MainList[3]));
            Assert.IsTrue(slice.Remove(generator.MainList[5]));
            Assert.IsTrue(slice.Remove(generator.MainList[7]));

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            slice.Append(appendedCoordinate);

            Assert.AreEqual(sliceLength - 3 + 1, slice.Count);
            Assert.AreEqual(generator.MainList[sliceLength], slice[sliceLength - 3 - 1]);
            Assert.AreEqual(appendedCoordinate, slice[sliceLength - 3]);

            ICoordinateSequence<BufferedCoordinate> appendSeq 
                = generator.SequenceFactory.Create(generator.AppendList);
            slice.Append(appendSeq);

            Assert.AreEqual(sliceLength - 3 + 1 + generator.AppendList.Count, slice.Count);
            Assert.AreEqual(generator.MainList[sliceLength], slice[sliceLength - 3 - 1]);
            Assert.AreEqual(appendedCoordinate, slice[sliceLength - 3]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength - 3 + 1 + i]);
            }
        }

        [Test]
        public void ComplexSliceToAppendedSliceWithSkip()
        {
            Int32 mainLength = 12;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            Assert.IsTrue(slice.Remove(generator.MainList[3]));
            Assert.IsTrue(slice.Remove(generator.MainList[5]));
            Assert.IsTrue(slice.Remove(generator.MainList[7]));

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            slice.Append(appendedCoordinate);

            Assert.AreEqual(sliceLength - 3 + 1, slice.Count);
            Assert.AreEqual(generator.MainList[sliceLength], slice[sliceLength - 3 - 1]);
            Assert.AreEqual(appendedCoordinate, slice[sliceLength - 3]);

            IBufferedCoordSequence appendSlice
                = generator.SequenceFactory.Create(generator.AppendList)
                  .Slice(0, 2);
            BufferedCoordinate preSliceCoordinate = generator.RandomCoordinate();
            BufferedCoordinate postSliceCoordinate = generator.RandomCoordinate();
            appendSlice.Prepend(preSliceCoordinate);
            appendSlice.Append(postSliceCoordinate);

            slice.Append(appendSlice);

            Assert.AreEqual(sliceLength - 3 + 1 + 3 + 1 + 1, slice.Count);
            Assert.AreEqual(generator.MainList[sliceLength], slice[sliceLength - 3 - 1]);
            Assert.AreEqual(appendedCoordinate, slice[sliceLength - 3]);
            Assert.AreEqual(preSliceCoordinate, slice[sliceLength - 3 + 1]);

            //for (Int32 i = 0; i <= generator.AppendList.TotalItemCount; i++)
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength - 3 + 2 + i]);
            }

            Assert.AreEqual(postSliceCoordinate, slice[sliceLength - 3 + 2 + generator.AppendList.Count]);
        }

        [Test]
        public void CoordinateToAppendedReversedSliceWithSkip()
        {
            Int32 mainLength = 12;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            Assert.IsTrue(slice.Remove(generator.MainList[3]));
            Assert.IsTrue(slice.Remove(generator.MainList[5]));
            Assert.IsTrue(slice.Remove(generator.MainList[7]));

            BufferedCoordinate coord1 = generator.RandomCoordinate();
            BufferedCoordinate coord0 = generator.RandomCoordinate();

            slice.Reverse();
            slice.Append(coord1);

            Assert.AreEqual(sliceLength - 3 + 1, slice.Count);
            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 3 - 1]);
            Assert.AreEqual(coord1, slice[sliceLength - 3]);

            slice.Append(coord0);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 3 - 1]);
            Assert.AreEqual(coord1, slice[sliceLength - 3]);
            Assert.AreEqual(coord0, slice[sliceLength - 3 + 1]);
        }

        [Test]
        public void ComplexSliceToAppendedReversedSliceWithSkip()
        {
            Int32 mainLength = 12;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            Assert.IsTrue(slice.Remove(generator.MainList[3]));
            Assert.IsTrue(slice.Remove(generator.MainList[5]));
            Assert.IsTrue(slice.Remove(generator.MainList[7]));

            slice.Reverse();

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            slice.Append(appendedCoordinate);

            Assert.AreEqual(sliceLength - 3 + 1, slice.Count);
            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 3 - 1]);
            Assert.AreEqual(appendedCoordinate, slice[sliceLength - 3]);

            IBufferedCoordSequence appendSlice
                = generator.SequenceFactory.Create(generator.AppendList)
                .Slice(0, 2);
            BufferedCoordinate preSliceCoordinate = generator.RandomCoordinate();
            BufferedCoordinate postSliceCoordinate = generator.RandomCoordinate();
            appendSlice.Prepend(preSliceCoordinate);
            appendSlice.Append(postSliceCoordinate);

            slice.Append(appendSlice);

            Assert.AreEqual(sliceLength - 3 + 1 + 3 + 1 + 1, slice.Count);
            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 3 - 1]);
            Assert.AreEqual(appendedCoordinate, slice[sliceLength - 3]);

            Assert.AreEqual(preSliceCoordinate, slice[sliceLength - 3 + 1]);

            //for (Int32 i = 0; i <= generator.AppendList.TotalItemCount; i++)
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength - 3 + 2 + i]);
            }

            Assert.AreEqual(postSliceCoordinate, slice[sliceLength - 3 + 2 + generator.AppendList.Count]);
        }

        [Test]
        public void VeryComplexSliceToVeryComplexSlice()
        {
            Int32 mainLength = 12;
            Int32 targetLength = mainLength - 2;
            Int32 addedLength = 10;

            // get all the coordinates
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, addedLength);
            BufferedCoordinate targetPrependedCoordinate = generator.RandomCoordinate();
            BufferedCoordinate targetAppendedCoordinate = generator.RandomCoordinate();
            BufferedCoordinate addedPrependedCoordinate = generator.RandomCoordinate();
            BufferedCoordinate addedAppendedCoordinate = generator.RandomCoordinate();

            // initialize and verify the very complex target slice
            IBufferedCoordSequence target = generator.Sequence.Slice(1, targetLength);
            Assert.IsTrue(target.Remove(generator.MainList[5]));
            Assert.IsTrue(target.Remove(generator.MainList[6]));
            Assert.IsTrue(target.Remove(generator.MainList[7]));
            target.Reverse();
            target.Prepend(targetPrependedCoordinate);
            target.Append(targetAppendedCoordinate);

            Assert.AreEqual(targetLength - 3 + 1 + 1, target.Count);
            Assert.AreEqual(targetPrependedCoordinate, target.First);
            Assert.AreEqual(targetAppendedCoordinate, target.Last);
            for (int i = 1; i < 5; i++)
            {
                Assert.AreEqual(generator.MainList[i], target[target.Count - 1 - i]);
            }
            for (int i = 8; i <= targetLength; i++)
            {
                Assert.AreEqual(generator.MainList[i], target[target.Count - 1 - i + 3]);
            }
            List<BufferedCoordinate> originalList = new List<BufferedCoordinate>(target);

            // initialize and verify the very complex added slice
            IBufferedCoordSequence addedSlice
                = generator.SequenceFactory.Create(generator.AppendList)
                .Slice(0, addedLength - 1);
            Assert.IsTrue(addedSlice.Remove(generator.AppendList[4]));
            Assert.IsTrue(addedSlice.Remove(generator.AppendList[5]));
            Assert.IsTrue(addedSlice.Remove(generator.AppendList[6]));
            addedSlice.Reverse();
            addedSlice.Prepend(addedPrependedCoordinate);
            addedSlice.Append(addedAppendedCoordinate);

            Assert.AreEqual(addedLength - 3 + 1 + 1, addedSlice.Count);
            Assert.AreEqual(addedPrependedCoordinate, addedSlice.First);
            Assert.AreEqual(addedAppendedCoordinate, addedSlice.Last);
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(generator.AppendList[i], addedSlice[addedSlice.Count - 2 - i]);
            }
            for (int i = 7; i < addedLength; i++)
            {
                Assert.AreEqual(generator.AppendList[i], addedSlice[addedSlice.Count - 2 - i + 3]);
            }
            List<BufferedCoordinate> addedList = new List<BufferedCoordinate>(addedSlice);

            
            // finally the test
            target.Append(addedSlice);


            // verify
            Assert.AreEqual(originalList.Count + addedList.Count, target.Count);

            IEnumerator<BufferedCoordinate> resultingSequence = target.GetEnumerator();
            foreach (BufferedCoordinate expected in originalList)
            {
                Assert.IsTrue(resultingSequence.MoveNext());
                BufferedCoordinate actual = resultingSequence.Current;
                Assert.AreEqual(expected, actual);
            }
            foreach (BufferedCoordinate expected in addedSlice)
            {
                Assert.IsTrue(resultingSequence.MoveNext());
                BufferedCoordinate actual = resultingSequence.Current;
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
