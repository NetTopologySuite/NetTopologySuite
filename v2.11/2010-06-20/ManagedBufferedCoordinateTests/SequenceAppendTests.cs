using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NetTopologySuite.Coordinates;
using Xunit;

namespace ManagedBufferedCoordinateTests
{
    using IBufferedCoordSequence = ICoordinateSequence<BufferedCoordinate>;

    public class SequenceAppendTests
    {
        private const int BigMaxLimit = Int32.MaxValue - 2;

        [Fact]
        public void CoordinateToNewSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);

            BufferedCoordinate coord = generator.RandomCoordinate();

            generator.Sequence.Append(coord);

            Assert.Equal(generator.MainList[mainLength - 1], generator.Sequence[mainLength - 1]);
            Assert.Equal(coord, generator.Sequence[mainLength]);
        }

        [Fact]
        public void CoordinateToNewSlice()
        {
            Int32 mainLength = 5;
            Int32 endIndex = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, mainLength - 2);

            BufferedCoordinate coord = generator.RandomCoordinate();

            slice.Append(coord);

            Assert.Equal(generator.MainList[mainLength - 2], slice[endIndex - 1]);
            Assert.Equal(coord, slice[endIndex]);
        }

        [Fact]
        public void CoordinateToAppendedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);

            BufferedCoordinate coord1 = generator.RandomCoordinate();
            BufferedCoordinate coord0 = generator.RandomCoordinate();

            generator.Sequence.Append(coord1);

            Assert.Equal(generator.MainList[mainLength - 1], generator.Sequence[mainLength - 1]);
            Assert.Equal(coord1, generator.Sequence[mainLength]);

            generator.Sequence.Append(coord0);

            Assert.Equal(generator.MainList[mainLength - 1], generator.Sequence[mainLength - 1]);
            Assert.Equal(coord1, generator.Sequence[mainLength]);
            Assert.Equal(coord0, generator.Sequence[mainLength + 1]);
        }

        [Fact]
        public void CoordinateToAppendedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, mainLength - 2);

            BufferedCoordinate coord1 = generator.RandomCoordinate();
            BufferedCoordinate coord0 = generator.RandomCoordinate();

            slice.Append(coord1);

            Assert.Equal(generator.MainList[mainLength - 2], slice[sliceLength - 1]);
            Assert.Equal(coord1, slice[sliceLength]);

            slice.Append(coord0);

            Assert.Equal(generator.MainList[mainLength - 2], slice[sliceLength - 1]);
            Assert.Equal(coord1, slice[sliceLength]);
            Assert.Equal(coord0, slice[sliceLength + 1]);
        }

        [Fact]
        public void EnumerationToNewSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);

            EnumerableIsolater<BufferedCoordinate> appendList = new EnumerableIsolater<BufferedCoordinate>(generator.AppendList);
            generator.Sequence.Append(appendList);

            Assert.Equal(generator.MainList[mainLength - 1], generator.Sequence[mainLength - 1]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], generator.Sequence[mainLength + i]);
            }
        }

        [Fact]
        public void EnumerationToNewSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            EnumerableIsolater<BufferedCoordinate> appendList = new EnumerableIsolater<BufferedCoordinate>(generator.AppendList);
            slice.Append(appendList);

            Assert.Equal(generator.MainList[sliceLength], slice[sliceLength - 1]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], slice[sliceLength + i]);
            }
        }

        [Fact]
        public void EnumerationToAppendedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            generator.Sequence.Append(appendedCoordinate);

            EnumerableIsolater<BufferedCoordinate> appendList = new EnumerableIsolater<BufferedCoordinate>(generator.AppendList);
            generator.Sequence.Append(appendList);

            Assert.Equal(generator.MainList[mainLength - 1], generator.Sequence[mainLength - 1]);
            Assert.Equal(appendedCoordinate, generator.Sequence[mainLength]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], generator.Sequence[mainLength + 1 + i]);
            }
        }

        [Fact]
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

            Assert.Equal(generator.MainList[mainLength - 2], slice[sliceLength - 1]);
            Assert.Equal(appendedCoordinate, slice[sliceLength]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], slice[sliceLength + 1 + i]);
            }
        }

        [Fact]
        public void SequenceToNewSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            ICoordinateSequence<BufferedCoordinate> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            generator.Sequence.Append(appendSeq);

            Assert.Equal(generator.MainList[mainLength - 1], generator.Sequence[mainLength - 1]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], generator.Sequence[mainLength + i]);
            }
        }

        [Fact]
        public void SequenceToNewSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            ICoordinateSequence<BufferedCoordinate> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            slice.Append(appendSeq);

            Assert.Equal(generator.MainList[sliceLength], slice[sliceLength - 1]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], slice[sliceLength + i]);
            }
        }

        [Fact]
        public void SequenceToAppendedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            generator.Sequence.Append(appendedCoordinate);

            ICoordinateSequence<BufferedCoordinate> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            generator.Sequence.Append(appendSeq);

            Assert.Equal(generator.MainList[mainLength - 1], generator.Sequence[mainLength - 1]);
            Assert.Equal(appendedCoordinate, generator.Sequence[mainLength]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], generator.Sequence[mainLength + 1 + i]);
            }
        }

        [Fact]
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

            Assert.Equal(generator.MainList[sliceLength], slice[sliceLength - 1]);
            Assert.Equal(appendedCoordinate, slice[sliceLength]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], slice[sliceLength + 1 + i]);
            }
        }

        [Fact]
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
            Assert.Equal(expected, actual);

            // then we appended appendedCoordinate
            expected = appendedCoordinate;
            actual = generator.Sequence[mainLength];
            Assert.Equal(expected, actual);

            // then we appended a sequence with a prepended sequence, of which 
            // this one is first
            expected = preSliceCoordinate;
            actual = generator.Sequence[mainLength + 1];
            Assert.Equal(expected, actual);

            //for (Int32 i = 0; i <= generator.AppendList.TotalItemCount; i++)
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], generator.Sequence[mainLength + 2 + i]);
            }

            Assert.Equal(postSliceCoordinate, generator.Sequence[mainLength + 2 + generator.AppendList.Count]);
        }

        [Fact]
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

            Assert.Equal(generator.MainList[sliceLength], slice[sliceLength - 1]);
            Assert.Equal(appendedCoordinate, slice[sliceLength]);
            Assert.Equal(preSliceCoordinate, slice[sliceLength + 1]);

            //for (Int32 i = 0; i <= generator.AppendList.TotalItemCount; i++)
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], slice[sliceLength + 2 + i]);
            }

            Assert.Equal(postSliceCoordinate, slice[sliceLength + 2 + generator.AppendList.Count]);
        }

        [Fact]
        public void CoordinateToNewReversedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);
            generator.Sequence.Reverse();

            BufferedCoordinate coord = generator.RandomCoordinate();

            generator.Sequence.Append(coord);

            Assert.Equal(generator.MainList[0], generator.Sequence[mainLength - 1]);
            Assert.Equal(coord, generator.Sequence[mainLength]);
        }

        [Fact]
        public void CoordinateToNewReversedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);
            slice.Reverse();

            BufferedCoordinate coord = generator.RandomCoordinate();

            slice.Append(coord);

            Assert.Equal(generator.MainList[1], slice[sliceLength - 1]);
            Assert.Equal(coord, slice[sliceLength]);
        }

        [Fact]
        public void CoordinateToAppendedReversedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);

            BufferedCoordinate coord1 = generator.RandomCoordinate();
            BufferedCoordinate coord0 = generator.RandomCoordinate();

            generator.Sequence.Reverse();
            generator.Sequence.Append(coord1);

            Assert.Equal(generator.MainList[0], generator.Sequence[mainLength - 1]);
            Assert.Equal(coord1, generator.Sequence[mainLength]);

            generator.Sequence.Append(coord0);

            Assert.Equal(generator.MainList[0], generator.Sequence[mainLength - 1]);
            Assert.Equal(coord1, generator.Sequence[mainLength]);
            Assert.Equal(coord0, generator.Sequence[mainLength + 1]);
        }

        [Fact]
        public void CoordinateToReversedAppendedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);

            BufferedCoordinate coord1 = generator.RandomCoordinate();
            BufferedCoordinate coord0 = generator.RandomCoordinate();

            generator.Sequence.Append(coord1);

            Assert.Equal(generator.MainList[mainLength - 1], generator.Sequence[mainLength - 1]);
            Assert.Equal(coord1, generator.Sequence[mainLength]);
            generator.Sequence.Reverse();

            generator.Sequence.Append(coord0);

            Assert.Equal(generator.MainList[0], generator.Sequence[mainLength]);
            Assert.Equal(coord0, generator.Sequence[mainLength + 1]);
            Assert.Equal(coord1, generator.Sequence[0]);
        }

        [Fact]
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

            Assert.Equal(generator.MainList[1], slice[sliceLength - 1]);
            Assert.Equal(coord1, slice[sliceLength]);

            slice.Append(coord0);

            Assert.Equal(generator.MainList[1], slice[sliceLength - 1]);
            Assert.Equal(coord1, slice[sliceLength]);
            Assert.Equal(coord0, slice[sliceLength + 1]);
        }

        [Fact]
        public void CoordinateToReversedAppendedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            BufferedCoordinate coord1 = generator.RandomCoordinate();
            BufferedCoordinate coord0 = generator.RandomCoordinate();

            slice.Append(coord1);

            Assert.Equal(generator.MainList[sliceLength], slice[sliceLength - 1]);
            Assert.Equal(coord1, slice[sliceLength]);
            slice.Reverse();

            slice.Append(coord0);

            Assert.Equal(generator.MainList[1], slice[sliceLength]);
            Assert.Equal(coord0, slice[sliceLength + 1]);
            Assert.Equal(coord1, slice[0]);
        }

        [Fact]
        public void EnumerationToNewReversedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            generator.Sequence.Reverse();

            BufferedCoordinate expected;
            BufferedCoordinate actual;
                        
            expected = generator.MainList[0];
            actual = generator.Sequence[mainLength - 1];
            Assert.Equal(expected, actual);

            EnumerableIsolater<BufferedCoordinate> appendList
                = new EnumerableIsolater<BufferedCoordinate>(generator.AppendList);
            generator.Sequence.Append(appendList);

            expected = generator.MainList[0];
            actual = generator.Sequence[mainLength - 1];
            Assert.Equal(expected, actual);
            
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                expected = generator.AppendList[i];
                actual = generator.Sequence[mainLength + i];
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void EnumerationToNewReversedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);
            slice.Reverse();

            EnumerableIsolater<BufferedCoordinate> appendList = new EnumerableIsolater<BufferedCoordinate>(generator.AppendList);
            slice.Append(appendList);

            Assert.Equal(generator.MainList[1], slice[sliceLength - 1]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], slice[sliceLength + i]);
            }
        }

        [Fact]
        public void EnumerationToAppendedReversedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            generator.Sequence.Reverse();

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            generator.Sequence.Append(appendedCoordinate);

            EnumerableIsolater<BufferedCoordinate> appendList = new EnumerableIsolater<BufferedCoordinate>(generator.AppendList);
            generator.Sequence.Append(appendList);

            Assert.Equal(generator.MainList[0], generator.Sequence[mainLength - 1]);
            Assert.Equal(appendedCoordinate, generator.Sequence[mainLength]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], generator.Sequence[mainLength + 1 + i]);
            }
        }

        [Fact]
        public void EnumerationToReversedAppendedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            generator.Sequence.Append(appendedCoordinate);
            generator.Sequence.Reverse();

            EnumerableIsolater<BufferedCoordinate> appendList = new EnumerableIsolater<BufferedCoordinate>(generator.AppendList);
            generator.Sequence.Append(appendList);

            Assert.Equal(generator.MainList[0], generator.Sequence[mainLength]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], generator.Sequence[mainLength + 1 + i]);
            }

            Assert.Equal(appendedCoordinate, generator.Sequence[0]);
        }

        [Fact]
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

            Assert.Equal(generator.MainList[1], slice[sliceLength - 1]);
            Assert.Equal(appendedCoordinate, slice[sliceLength]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], slice[sliceLength + 1 + i]);
            }
        }

        [Fact]
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

            Assert.Equal(generator.MainList[1], slice[sliceLength]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], slice[sliceLength + 1 + i]);
            }

            Assert.Equal(appendedCoordinate, slice[0]);
        }

        [Fact]
        public void SequenceToNewReversedSequence()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            ICoordinateSequence<BufferedCoordinate> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            generator.Sequence.Reverse();
            generator.Sequence.Append(appendSeq);

            Assert.Equal(generator.MainList[0], generator.Sequence[mainLength - 1]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], generator.Sequence[mainLength + i]);
            }
        }

        [Fact]
        public void SequenceToNewReversedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);
            slice.Reverse();

            ICoordinateSequence<BufferedCoordinate> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            slice.Append(appendSeq);

            Assert.Equal(generator.MainList[1], slice[sliceLength - 1]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], slice[sliceLength + i]);
            }
        }

        [Fact]
        public void SequenceToAppendedReversedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            generator.Sequence.Reverse();

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            generator.Sequence.Append(appendedCoordinate);

            ICoordinateSequence<BufferedCoordinate> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            generator.Sequence.Append(appendSeq);

            Assert.Equal(generator.MainList[0], generator.Sequence[mainLength - 1]);
            Assert.Equal(appendedCoordinate, generator.Sequence[mainLength]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], generator.Sequence[mainLength + 1 + i]);
            }
        }

        [Fact]
        public void SequenceToReversedAppendedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            generator.Sequence.Append(appendedCoordinate);
            generator.Sequence.Reverse();

            ICoordinateSequence<BufferedCoordinate> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            generator.Sequence.Append(appendSeq);

            Assert.Equal(generator.MainList[0], generator.Sequence[mainLength]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], generator.Sequence[mainLength + 1 + i]);
            }

            Assert.Equal(appendedCoordinate, generator.Sequence[0]);
        }

        [Fact]
        public void SequenceToAppendedReversedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);
            slice.Reverse();

            Assert.Equal(generator.MainList[1], slice[sliceLength - 1]);

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            slice.Append(appendedCoordinate);

            Assert.Equal(generator.MainList[1], slice[sliceLength - 1]);
            Assert.Equal(appendedCoordinate, slice[sliceLength]);

            ICoordinateSequence<BufferedCoordinate> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            slice.Append(appendSeq);

            Assert.Equal(generator.MainList[1], slice[sliceLength - 1]);
            Assert.Equal(appendedCoordinate, slice[sliceLength]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], slice[sliceLength + 1 + i]);
            }
        }

        [Fact]
        public void SequenceToReversedAppendedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            slice.Append(appendedCoordinate);
            slice.Reverse();

            Assert.Equal(generator.MainList[1], slice[sliceLength]);
            Assert.Equal(appendedCoordinate, slice[0]);

            ICoordinateSequence<BufferedCoordinate> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            slice.Append(appendSeq);

            Assert.Equal(generator.MainList[1], slice[sliceLength]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], slice[sliceLength + 1 + i]);
            }

            Assert.Equal(appendedCoordinate, slice[0]);
        }

        [Fact]
        public void ComplexSliceToAppendedReversedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            generator.Sequence.Reverse();

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            generator.Sequence.Append(appendedCoordinate);

            Assert.Equal(generator.MainList[0], generator.Sequence[mainLength - 1]);
            Assert.Equal(appendedCoordinate, generator.Sequence[mainLength]);

            IBufferedCoordSequence appendSlice
                = generator.SequenceFactory.Create(generator.AppendList)
                .Slice(0, 2);
            BufferedCoordinate preSliceCoordinate = generator.RandomCoordinate();
            BufferedCoordinate postSliceCoordinate = generator.RandomCoordinate();
            appendSlice.Prepend(preSliceCoordinate);
            appendSlice.Append(postSliceCoordinate);

            generator.Sequence.Append(appendSlice);

            Assert.Equal(generator.MainList[0], generator.Sequence[mainLength - 1]);
            Assert.Equal(appendedCoordinate, generator.Sequence[mainLength]);

            Assert.Equal(preSliceCoordinate, generator.Sequence[mainLength + 1]);

            //for (Int32 i = 0; i <= generator.AppendList.TotalItemCount; i++)
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], generator.Sequence[mainLength + 2 + i]);
            }

            Assert.Equal(postSliceCoordinate, generator.Sequence[mainLength + 2 + generator.AppendList.Count]);
        }

        [Fact]
        public void ComplexSliceToReversedAppendedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            generator.Sequence.Append(appendedCoordinate);
            generator.Sequence.Reverse();

            Assert.Equal(generator.MainList[0], generator.Sequence[mainLength]);
            Assert.Equal(appendedCoordinate, generator.Sequence[0]);

            IBufferedCoordSequence appendSlice
                = generator.SequenceFactory.Create(generator.AppendList)
                .Slice(0, 2);
            BufferedCoordinate preSliceCoordinate = generator.RandomCoordinate();
            BufferedCoordinate postSliceCoordinate = generator.RandomCoordinate();
            appendSlice.Prepend(preSliceCoordinate);
            appendSlice.Append(postSliceCoordinate);

            generator.Sequence.Append(appendSlice);

            Assert.Equal(generator.MainList[0], generator.Sequence[mainLength]);

            Assert.Equal(preSliceCoordinate, generator.Sequence[mainLength + 1]);

            //for (Int32 i = 0; i <= generator.AppendList.TotalItemCount; i++)
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], generator.Sequence[mainLength + 2 + i]);
            }

            Assert.Equal(postSliceCoordinate, generator.Sequence[mainLength + 2 + generator.AppendList.Count]);

            Assert.Equal(appendedCoordinate, generator.Sequence[0]);
        }

        [Fact]
        public void ComplexSliceToAppendedReversedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);
            slice.Reverse();

            Assert.Equal(generator.MainList[1], slice[sliceLength - 1]);

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            slice.Append(appendedCoordinate);

            Assert.Equal(generator.MainList[1], slice[sliceLength - 1]);
            Assert.Equal(appendedCoordinate, slice[sliceLength]);

            IBufferedCoordSequence appendSlice
                = generator.SequenceFactory.Create(generator.AppendList)
                .Slice(0, 2);
            BufferedCoordinate preSliceCoordinate = generator.RandomCoordinate();
            BufferedCoordinate postSliceCoordinate = generator.RandomCoordinate();
            appendSlice.Prepend(preSliceCoordinate);
            appendSlice.Append(postSliceCoordinate);

            slice.Append(appendSlice);

            Assert.Equal(generator.MainList[1], slice[sliceLength - 1]);
            Assert.Equal(appendedCoordinate, slice[sliceLength]);

            Assert.Equal(preSliceCoordinate, slice[sliceLength + 1]);

            //for (Int32 i = 0; i <= generator.AppendList.TotalItemCount; i++)
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], slice[sliceLength + 2 + i]);
            }

            Assert.Equal(postSliceCoordinate, slice[sliceLength + 2 + generator.AppendList.Count]);
        }

        [Fact]
        public void ComplexSliceToReversedAppendedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            slice.Append(appendedCoordinate);
            slice.Reverse();

            Assert.Equal(generator.MainList[1], slice[sliceLength]);
            Assert.Equal(appendedCoordinate, slice[0]);

            IBufferedCoordSequence appendSlice
                = generator.SequenceFactory.Create(generator.AppendList)
                  .Slice(0, 2);
            BufferedCoordinate preSliceCoordinate = generator.RandomCoordinate();
            BufferedCoordinate postSliceCoordinate = generator.RandomCoordinate();
            appendSlice.Prepend(preSliceCoordinate);
            appendSlice.Append(postSliceCoordinate);

            slice.Append(appendSlice);

            Assert.Equal(generator.MainList[1], slice[sliceLength]);

            Assert.Equal(preSliceCoordinate, slice[sliceLength + 1]);

            //for (Int32 i = 0; i <= generator.AppendList.TotalItemCount; i++)
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], slice[sliceLength + 2 + i]);
            }

            Assert.Equal(postSliceCoordinate, slice[sliceLength + 2 + generator.AppendList.Count]);

            Assert.Equal(appendedCoordinate, slice[0]);
        }

        [Fact]
        public void CoordinateToAppendedSliceWithSkip()
        {
            Int32 mainLength = 12;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            Assert.True(slice.Remove(generator.MainList[3]));
            Assert.True(slice.Remove(generator.MainList[5]));
            Assert.True(slice.Remove(generator.MainList[7]));

            BufferedCoordinate coord1 = generator.RandomCoordinate();
            BufferedCoordinate coord0 = generator.RandomCoordinate();

            slice.Append(coord1);

            Assert.Equal(sliceLength - 3 + 1, slice.Count);
            Assert.Equal(generator.MainList[sliceLength], slice[sliceLength - 3 - 1]);
            Assert.Equal(coord1, slice[sliceLength - 3]);

            slice.Append(coord0);

            Assert.Equal(generator.MainList[sliceLength], slice[sliceLength - 3 - 1]);
            Assert.Equal(coord1, slice[sliceLength - 3]);
            Assert.Equal(coord0, slice[sliceLength - 3 + 1]);
        }

        [Fact]
        public void EnumerationToAppendedSliceWithSkip()
        {
            Int32 mainLength = 12;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            Assert.True(slice.Remove(generator.MainList[3]));
            Assert.True(slice.Remove(generator.MainList[5]));
            Assert.True(slice.Remove(generator.MainList[7]));

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            slice.Append(appendedCoordinate);

            Assert.Equal(sliceLength - 3 + 1, slice.Count);
            Assert.Equal(generator.MainList[sliceLength], slice[sliceLength - 3 - 1]);
            Assert.Equal(appendedCoordinate, slice[sliceLength - 3]);

            EnumerableIsolater<BufferedCoordinate> appendList 
                = new EnumerableIsolater<BufferedCoordinate>(generator.AppendList);
            slice.Append(appendList);

            Assert.Equal(sliceLength - 3 + 1 + generator.AppendList.Count, slice.Count);
            Assert.Equal(generator.MainList[sliceLength], slice[sliceLength - 3 - 1]);
            Assert.Equal(appendedCoordinate, slice[sliceLength - 3]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], slice[sliceLength - 3 + 1 + i]);
            }
        }

        [Fact]
        public void SequenceToAppendedSliceWithSkip()
        {
            Int32 mainLength = 12;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            Assert.True(slice.Remove(generator.MainList[3]));
            Assert.True(slice.Remove(generator.MainList[5]));
            Assert.True(slice.Remove(generator.MainList[7]));

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            slice.Append(appendedCoordinate);

            Assert.Equal(sliceLength - 3 + 1, slice.Count);
            Assert.Equal(generator.MainList[sliceLength], slice[sliceLength - 3 - 1]);
            Assert.Equal(appendedCoordinate, slice[sliceLength - 3]);

            ICoordinateSequence<BufferedCoordinate> appendSeq 
                = generator.SequenceFactory.Create(generator.AppendList);
            slice.Append(appendSeq);

            Assert.Equal(sliceLength - 3 + 1 + generator.AppendList.Count, slice.Count);
            Assert.Equal(generator.MainList[sliceLength], slice[sliceLength - 3 - 1]);
            Assert.Equal(appendedCoordinate, slice[sliceLength - 3]);

            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], slice[sliceLength - 3 + 1 + i]);
            }
        }

        [Fact]
        public void ComplexSliceToAppendedSliceWithSkip()
        {
            Int32 mainLength = 12;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            Assert.True(slice.Remove(generator.MainList[3]));
            Assert.True(slice.Remove(generator.MainList[5]));
            Assert.True(slice.Remove(generator.MainList[7]));

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            slice.Append(appendedCoordinate);

            Assert.Equal(sliceLength - 3 + 1, slice.Count);
            Assert.Equal(generator.MainList[sliceLength], slice[sliceLength - 3 - 1]);
            Assert.Equal(appendedCoordinate, slice[sliceLength - 3]);

            IBufferedCoordSequence appendSlice
                = generator.SequenceFactory.Create(generator.AppendList)
                  .Slice(0, 2);
            BufferedCoordinate preSliceCoordinate = generator.RandomCoordinate();
            BufferedCoordinate postSliceCoordinate = generator.RandomCoordinate();
            appendSlice.Prepend(preSliceCoordinate);
            appendSlice.Append(postSliceCoordinate);

            slice.Append(appendSlice);

            Assert.Equal(sliceLength - 3 + 1 + 3 + 1 + 1, slice.Count);
            Assert.Equal(generator.MainList[sliceLength], slice[sliceLength - 3 - 1]);
            Assert.Equal(appendedCoordinate, slice[sliceLength - 3]);
            Assert.Equal(preSliceCoordinate, slice[sliceLength - 3 + 1]);

            //for (Int32 i = 0; i <= generator.AppendList.TotalItemCount; i++)
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], slice[sliceLength - 3 + 2 + i]);
            }

            Assert.Equal(postSliceCoordinate, slice[sliceLength - 3 + 2 + generator.AppendList.Count]);
        }

        [Fact]
        public void CoordinateToAppendedReversedSliceWithSkip()
        {
            Int32 mainLength = 12;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            Assert.True(slice.Remove(generator.MainList[3]));
            Assert.True(slice.Remove(generator.MainList[5]));
            Assert.True(slice.Remove(generator.MainList[7]));

            BufferedCoordinate coord1 = generator.RandomCoordinate();
            BufferedCoordinate coord0 = generator.RandomCoordinate();

            slice.Reverse();
            slice.Append(coord1);

            Assert.Equal(sliceLength - 3 + 1, slice.Count);
            Assert.Equal(generator.MainList[1], slice[sliceLength - 3 - 1]);
            Assert.Equal(coord1, slice[sliceLength - 3]);

            slice.Append(coord0);

            Assert.Equal(generator.MainList[1], slice[sliceLength - 3 - 1]);
            Assert.Equal(coord1, slice[sliceLength - 3]);
            Assert.Equal(coord0, slice[sliceLength - 3 + 1]);
        }

        [Fact]
        public void ComplexSliceToAppendedReversedSliceWithSkip()
        {
            Int32 mainLength = 12;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            Assert.True(slice.Remove(generator.MainList[3]));
            Assert.True(slice.Remove(generator.MainList[5]));
            Assert.True(slice.Remove(generator.MainList[7]));

            slice.Reverse();

            BufferedCoordinate appendedCoordinate = generator.RandomCoordinate();
            slice.Append(appendedCoordinate);

            Assert.Equal(sliceLength - 3 + 1, slice.Count);
            Assert.Equal(generator.MainList[1], slice[sliceLength - 3 - 1]);
            Assert.Equal(appendedCoordinate, slice[sliceLength - 3]);

            IBufferedCoordSequence appendSlice
                = generator.SequenceFactory.Create(generator.AppendList)
                .Slice(0, 2);
            BufferedCoordinate preSliceCoordinate = generator.RandomCoordinate();
            BufferedCoordinate postSliceCoordinate = generator.RandomCoordinate();
            appendSlice.Prepend(preSliceCoordinate);
            appendSlice.Append(postSliceCoordinate);

            slice.Append(appendSlice);

            Assert.Equal(sliceLength - 3 + 1 + 3 + 1 + 1, slice.Count);
            Assert.Equal(generator.MainList[1], slice[sliceLength - 3 - 1]);
            Assert.Equal(appendedCoordinate, slice[sliceLength - 3]);

            Assert.Equal(preSliceCoordinate, slice[sliceLength - 3 + 1]);

            //for (Int32 i = 0; i <= generator.AppendList.TotalItemCount; i++)
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.Equal(generator.AppendList[i], slice[sliceLength - 3 + 2 + i]);
            }

            Assert.Equal(postSliceCoordinate, slice[sliceLength - 3 + 2 + generator.AppendList.Count]);
        }

        [Fact]
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
            Assert.True(target.Remove(generator.MainList[5]));
            Assert.True(target.Remove(generator.MainList[6]));
            Assert.True(target.Remove(generator.MainList[7]));
            target.Reverse();
            target.Prepend(targetPrependedCoordinate);
            target.Append(targetAppendedCoordinate);

            Assert.Equal(targetLength - 3 + 1 + 1, target.Count);
            Assert.Equal(targetPrependedCoordinate, target.First);
            Assert.Equal(targetAppendedCoordinate, target.Last);
            for (int i = 1; i < 5; i++)
            {
                Assert.Equal(generator.MainList[i], target[target.Count - 1 - i]);
            }
            for (int i = 8; i <= targetLength; i++)
            {
                Assert.Equal(generator.MainList[i], target[target.Count - 1 - i + 3]);
            }
            List<BufferedCoordinate> originalList = new List<BufferedCoordinate>(target);

            // initialize and verify the very complex added slice
            IBufferedCoordSequence addedSlice
                = generator.SequenceFactory.Create(generator.AppendList)
                .Slice(0, addedLength - 1);
            Assert.True(addedSlice.Remove(generator.AppendList[4]));
            Assert.True(addedSlice.Remove(generator.AppendList[5]));
            Assert.True(addedSlice.Remove(generator.AppendList[6]));
            addedSlice.Reverse();
            addedSlice.Prepend(addedPrependedCoordinate);
            addedSlice.Append(addedAppendedCoordinate);

            Assert.Equal(addedLength - 3 + 1 + 1, addedSlice.Count);
            Assert.Equal(addedPrependedCoordinate, addedSlice.First);
            Assert.Equal(addedAppendedCoordinate, addedSlice.Last);
            for (int i = 0; i < 4; i++)
            {
                Assert.Equal(generator.AppendList[i], addedSlice[addedSlice.Count - 2 - i]);
            }
            for (int i = 7; i < addedLength; i++)
            {
                Assert.Equal(generator.AppendList[i], addedSlice[addedSlice.Count - 2 - i + 3]);
            }
            List<BufferedCoordinate> addedList = new List<BufferedCoordinate>(addedSlice);

            
            // finally the test
            target.Append(addedSlice);


            // verify
            Assert.Equal(originalList.Count + addedList.Count, target.Count);

            IEnumerator<BufferedCoordinate> resultingSequence = target.GetEnumerator();
            foreach (BufferedCoordinate expected in originalList)
            {
                Assert.True(resultingSequence.MoveNext());
                BufferedCoordinate actual = resultingSequence.Current;
                Assert.Equal(expected, actual);
            }
            foreach (BufferedCoordinate expected in addedSlice)
            {
                Assert.True(resultingSequence.MoveNext());
                BufferedCoordinate actual = resultingSequence.Current;
                Assert.Equal(expected, actual);
            }
        }
    }
}
