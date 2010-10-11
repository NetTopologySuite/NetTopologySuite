using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NetTopologySuite.Coordinates;
using Xunit;

namespace SimpleCoordinateTests
{
    public class SequencePrependTests
    {
        private const int BigMaxLimit = Int32.MaxValue - 2;

        [Fact]
        public void CoordinateToNewSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5);

            Coordinate coord = generator.RandomCoordinate();

            generator.Sequence.Prepend(coord);

            Assert.Equal(coord, generator.Sequence[0]);
            Assert.Equal(generator.MainList[0], generator.Sequence[1]);
        }

        [Fact]
        public void CoordinateToNewSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, 3);

            Coordinate coord = generator.RandomCoordinate();

            slice.Prepend(coord);

            Assert.Equal(coord, slice[0]);
            Assert.Equal(generator.MainList[1], slice[1]);
        }

        [Fact]
        public void CoordinateToPrependedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5);

            Coordinate coord1 = generator.RandomCoordinate();
            Coordinate coord0 = generator.RandomCoordinate();

            generator.Sequence.Prepend(coord1);

            Assert.Equal(coord1, generator.Sequence[0]);
            // prepend pushes back the indexes, so the first index in MainList will 
            // be the second in Sequence
            Assert.Equal(generator.MainList[0], generator.Sequence[1]);

            generator.Sequence.Prepend(coord0);

            Assert.Equal(coord0, generator.Sequence[0]);
            Assert.Equal(coord1, generator.Sequence[1]);
            Assert.Equal(generator.MainList[0], generator.Sequence[2]);
        }

        [Fact]
        public void CoordinateToPrependedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, 3);

            Coordinate coord1 = generator.RandomCoordinate();
            Coordinate coord0 = generator.RandomCoordinate();

            slice.Prepend(coord1);

            Assert.Equal(coord1, slice[0]);
            Assert.Equal(generator.MainList[1], slice[1]);

            slice.Prepend(coord0);

            Assert.Equal(coord0, slice[0]);
            Assert.Equal(coord1, slice[1]);
            Assert.Equal(generator.MainList[1], slice[2]);
        }

        [Fact]
        public void EnumerationToNewSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);

            EnumerableIsolater<Coordinate> prependList
                = new EnumerableIsolater<Coordinate>(generator.PrependList);
            generator.Sequence.Prepend(prependList);

            Int32 i = 0;
            for (; i < generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i], generator.Sequence[i]);
            }
            Assert.Equal(generator.MainList[0], generator.Sequence[i]);
        }

        [Fact]
        public void EnumerationToNewSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, 3);

            EnumerableIsolater<Coordinate> prependList
                = new EnumerableIsolater<Coordinate>(generator.PrependList);
            slice.Prepend(prependList);

            Int32 i = 0;
            for (; i < generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i], slice[i]);
            }
            Assert.Equal(generator.MainList[1], slice[i]);
        }

        [Fact]
        public void EnumerationToPrependedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);

            Coordinate prependedCoordinate = generator.CoordinateFactory.Create(1, 1);
            generator.Sequence.Prepend(prependedCoordinate);

            EnumerableIsolater<Coordinate> prependList
                = new EnumerableIsolater<Coordinate>(generator.PrependList);
            generator.Sequence.Prepend(prependList);

            Int32 i = 0;
            for (; i < generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i], generator.Sequence[i]);
            }
            Assert.Equal(prependedCoordinate, generator.Sequence[i]);
            Assert.Equal(generator.MainList[0], generator.Sequence[i + 1]);
        }

        [Fact]
        public void EnumerationToPrependedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, 3);

            Coordinate prependedCoordinate = generator.CoordinateFactory.Create(1, 1);
            slice.Prepend(prependedCoordinate);

            EnumerableIsolater<Coordinate> prependList
                = new EnumerableIsolater<Coordinate>(generator.PrependList);
            slice.Prepend(prependList);

            Int32 i = 0;
            for (; i < generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i], slice[i]);
            }
            Assert.Equal(prependedCoordinate, slice[i]);
            Assert.Equal(generator.MainList[1], slice[i + 1]);
        }

        [Fact]
        public void SequenceToNewSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            ICoordinateSequence<Coordinate> prependSeq
                = generator.SequenceFactory.Create(generator.PrependList);

            generator.Sequence.Prepend(prependSeq);

            Int32 i = 0;
            Coordinate expected;
            Coordinate actual;

            for (; i < generator.PrependList.Count; i++)
            {
                expected = generator.PrependList[i];
                actual = generator.Sequence[i];
                Assert.Equal(expected, actual);
            }

            expected = generator.MainList[0];
            actual = generator.Sequence[i];
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SequenceToNewSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, 3);

            ICoordinateSequence<Coordinate> prependSeq
                = generator.SequenceFactory.Create(generator.PrependList);

            slice.Prepend(prependSeq);

            Int32 i = 0;
            for (; i < generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i], slice[i]);
            }
            Assert.Equal(generator.MainList[1], slice[i]);
        }

        [Fact]
        public void SequenceToPrependedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);

            Coordinate prependedCoordinate = generator.RandomCoordinate();
            generator.Sequence.Prepend(prependedCoordinate);

            ICoordinateSequence<Coordinate> prependSeq
                = generator.SequenceFactory.Create(generator.PrependList);

            generator.Sequence.Prepend(prependSeq);

            Int32 i = 0;

            for (; i < generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i], generator.Sequence[i]);
            }

            Assert.Equal(prependedCoordinate, generator.Sequence[i]);
            Assert.Equal(generator.MainList[0], generator.Sequence[i + 1]);
        }

        [Fact]
        public void SequenceToPrependedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, 3);

            Coordinate prependedCoordinate = generator.RandomCoordinate();
            slice.Prepend(prependedCoordinate);

            ICoordinateSequence<Coordinate> prependSeq
                = generator.SequenceFactory.Create(generator.PrependList);

            slice.Prepend(prependSeq);

            Int32 i = 0;
            for (; i < generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i], slice[i]);
            }
            Assert.Equal(prependedCoordinate, slice[i]);
            Assert.Equal(generator.MainList[1], slice[i + 1]);
        }

        [Fact]
        public void ComplexSliceToPrependedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);

            Coordinate prependedCoordinate = generator.RandomCoordinate();
            generator.Sequence.Prepend(prependedCoordinate);

            ICoordinateSequence<Coordinate> prependSlice
                = generator.SequenceFactory.Create(generator.PrependList)
                    .Slice(0, 2);
            Coordinate preSliceCoordinate = generator.RandomCoordinate();
            Coordinate postSliceCoordinate = generator.RandomCoordinate();
            prependSlice.Prepend(preSliceCoordinate);
            prependSlice.Append(postSliceCoordinate);

            generator.Sequence.Prepend(prependSlice);

            Assert.Equal(preSliceCoordinate, generator.Sequence[0]);
            Int32 i = 1;
            for (; i <= generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i - 1], generator.Sequence[i]);
            }
            Assert.Equal(postSliceCoordinate, generator.Sequence[i]);
            Assert.Equal(prependedCoordinate, generator.Sequence[i + 1]);
            Assert.Equal(generator.MainList[0], generator.Sequence[i + 2]);
        }

        [Fact]
        public void ComplexSliceToPrependedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            ICoordinateSequence<Coordinate> target = generator.Sequence.Slice(1, 3);

            Coordinate prependedCoordinate
                = generator.RandomCoordinate();

            target.Prepend(prependedCoordinate);

            ICoordinateSequence<Coordinate> prependSlice = generator.SequenceFactory
                .Create(generator.PrependList)
                .Slice(0, 2);

            Coordinate preSliceCoordinate = generator.RandomCoordinate();
            Coordinate postSliceCoordinate = generator.RandomCoordinate();
            prependSlice.Prepend(preSliceCoordinate);
            prependSlice.Append(postSliceCoordinate);

            Assert.Equal(preSliceCoordinate, prependSlice.First);
            Assert.Equal(postSliceCoordinate, prependSlice.Last);

            target.Prepend(prependSlice);

            Assert.Equal(preSliceCoordinate, target[0]);

            Int32 i = 1;

            for (; i <= generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i - 1], target[i]);
            }

            Assert.Equal(postSliceCoordinate, target[i]);
            Assert.Equal(prependedCoordinate, target[i + 1]);
            Assert.Equal(generator.MainList[1], target[i + 2]);
        }

        [Fact]
        public void CoordinateToNewReversedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5);
            generator.Sequence.Reverse();

            Coordinate coord = generator.RandomCoordinate();

            generator.Sequence.Prepend(coord);

            Assert.Equal(coord, generator.Sequence[0]);
            Assert.Equal(generator.MainList[generator.MainList.Count - 1], generator.Sequence[1]);
        }

        [Fact]
        public void CoordinateToNewReversedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, 3);
            slice.Reverse();

            Coordinate coord = generator.RandomCoordinate();

            slice.Prepend(coord);

            Assert.Equal(coord, slice[0]);
            Assert.Equal(generator.MainList[generator.MainList.Count - 2], slice[1]);
        }

        [Fact]
        public void CoordinateToPrependedReversedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5);

            Coordinate coord1 = generator.RandomCoordinate();
            Coordinate coord0 = generator.RandomCoordinate();

            generator.Sequence.Reverse();
            generator.Sequence.Prepend(coord1);

            Assert.Equal(coord1, generator.Sequence[0]);
            Assert.Equal(generator.MainList[generator.MainList.Count - 1], generator.Sequence[1]);

            generator.Sequence.Prepend(coord0);

            Assert.Equal(coord0, generator.Sequence[0]);
            Assert.Equal(coord1, generator.Sequence[1]);
            Assert.Equal(generator.MainList[generator.MainList.Count - 1], generator.Sequence[2]);
        }

        [Fact]
        public void CoordinateToReversedPrependedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5);

            Coordinate coord1 = generator.RandomCoordinate();
            Coordinate coord0 = generator.RandomCoordinate();

            generator.Sequence.Prepend(coord1);

            Coordinate expected;
            Coordinate actual;

            expected = coord1;
            actual = generator.Sequence[0];
            Assert.Equal(expected, actual);

            expected = generator.MainList[0];
            actual = generator.Sequence[1];
            Assert.Equal(expected, actual);

            generator.Sequence.Reverse();

            generator.Sequence.Prepend(coord0);

            Assert.Equal(coord0, generator.Sequence[0]);

            expected = generator.MainList[generator.MainList.Count - 1];
            actual = generator.Sequence[1];
            Assert.Equal(expected, actual);

            Assert.Equal(coord1, generator.Sequence.Last);
        }

        [Fact]
        public void CoordinateToPrependedReversedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, 3);

            Coordinate coord1 = generator.RandomCoordinate();
            Coordinate coord0 = generator.RandomCoordinate();

            slice.Reverse();
            slice.Prepend(coord1);

            Assert.Equal(coord1, slice[0]);
            Assert.Equal(generator.MainList[generator.MainList.Count - 2], slice[1]);

            slice.Prepend(coord0);

            Assert.Equal(coord0, slice[0]);
            Assert.Equal(coord1, slice[1]);
            Assert.Equal(generator.MainList[generator.MainList.Count - 2], slice[2]);
        }

        [Fact]
        public void CoordinateToReversedPrependedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, 3);

            Coordinate coord1 = generator.RandomCoordinate();
            Coordinate coord0 = generator.RandomCoordinate();

            slice.Prepend(coord1);

            Coordinate expected;
            Coordinate actual;

            expected = coord1;
            actual = slice[0];
            Assert.Equal(expected, actual);

            expected = generator.MainList[1];
            actual = slice[1];
            Assert.Equal(expected, actual);

            slice.Reverse();
            slice.Prepend(coord0);

            expected = coord0;
            actual = slice[0];
            Assert.Equal(expected, actual);

            expected = generator.MainList[3];
            actual = slice[1];
            Assert.Equal(expected, actual);

            expected = coord1;
            actual = slice.Last;
            Assert.Equal(coord1, slice.Last);
        }

        [Fact]
        public void EnumerationToNewReversedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            generator.Sequence.Reverse();

            EnumerableIsolater<Coordinate> prependList
                = new EnumerableIsolater<Coordinate>(generator.PrependList);
            generator.Sequence.Prepend(prependList);

            Int32 i = 0;
            for (; i < generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i], generator.Sequence[i]);
            }

            Assert.Equal(generator.MainList[generator.MainList.Count - 1], generator.Sequence[i]);
        }

        [Fact]
        public void EnumerationToNewReversedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, 3);
            slice.Reverse();

            EnumerableIsolater<Coordinate> prependList
                = new EnumerableIsolater<Coordinate>(generator.PrependList);
            slice.Prepend(prependList);

            Int32 i = 0;

            for (; i < generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i], slice[i]);
            }

            Assert.Equal(generator.MainList[generator.MainList.Count - 2], slice[i]);
        }

        [Fact]
        public void EnumerationToPrependedReversedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            generator.Sequence.Reverse();

            Coordinate prependedCoordinate = generator.CoordinateFactory.Create(1, 1);
            generator.Sequence.Prepend(prependedCoordinate);

            EnumerableIsolater<Coordinate> prependList
                = new EnumerableIsolater<Coordinate>(generator.PrependList);
            generator.Sequence.Prepend(prependList);

            Int32 i = 0;
            for (; i < generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i], generator.Sequence[i]);
            }

            Assert.Equal(prependedCoordinate, generator.Sequence[i]);
            Assert.Equal(generator.MainList[generator.MainList.Count - 1], generator.Sequence[i + 1]);
        }

        [Fact]
        public void EnumerationToReversedPrependedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);

            Coordinate prependedCoordinate
                = generator.CoordinateFactory.Create(1, 1);

            generator.Sequence.Prepend(prependedCoordinate);
            generator.Sequence.Reverse();

            EnumerableIsolater<Coordinate> prependList
                = new EnumerableIsolater<Coordinate>(generator.PrependList);
            generator.Sequence.Prepend(prependList);

            Int32 i = 0;

            for (; i < generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i], generator.Sequence[i]);
            }

            Coordinate expected;
            Coordinate actual;

            // last coordinate in MainList (index 4) is the same as the 
            // first coordinate of the sequence after the prepended coordinates, 
            // at index 3
            expected = generator.MainList[4];
            actual = generator.Sequence[3];
            Assert.Equal(expected, actual);

            expected = prependedCoordinate;
            actual = generator.Sequence.Last;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void EnumerationToPrependedReversedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, 3);
            slice.Reverse();

            Coordinate prependedCoordinate = generator.CoordinateFactory.Create(1, 1);
            slice.Prepend(prependedCoordinate);

            EnumerableIsolater<Coordinate> prependList
                = new EnumerableIsolater<Coordinate>(generator.PrependList);
            slice.Prepend(prependList);

            Int32 i = 0;

            for (; i < generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i], slice[i]);
            }

            Assert.Equal(prependedCoordinate, slice[i]);
            Assert.Equal(generator.MainList[generator.MainList.Count - 2], slice[i + 1]);
        }

        [Fact]
        public void EnumerationToReversedPrependedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, 3);

            Coordinate prependedCoordinate = generator.CoordinateFactory.Create(1, 1);
            slice.Prepend(prependedCoordinate);
            slice.Reverse();

            EnumerableIsolater<Coordinate> prependList
                = new EnumerableIsolater<Coordinate>(generator.PrependList);
            slice.Prepend(prependList);

            Int32 i = 0;

            for (; i < generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i], slice[i]);
            }

            Assert.Equal(generator.MainList[generator.MainList.Count - 2], slice[i]);
            Assert.Equal(prependedCoordinate, slice.Last);
        }

        [Fact]
        public void SequenceToNewReversedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            ICoordinateSequence<Coordinate> prependSeq
                = generator.SequenceFactory.Create(generator.PrependList);

            generator.Sequence.Reverse();
            generator.Sequence.Prepend(prependSeq);

            Int32 i = 0;

            for (; i < generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i], generator.Sequence[i]);
            }

            Assert.Equal(generator.MainList[generator.MainList.Count - 1], generator.Sequence[i]);
        }

        [Fact]
        public void SequenceToNewReversedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, 3);
            slice.Reverse();

            ICoordinateSequence<Coordinate> prependSeq
                = generator.SequenceFactory.Create(generator.PrependList);
            slice.Prepend(prependSeq);

            Int32 i = 0;

            for (; i < generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i], slice[i]);
            }

            Assert.Equal(generator.MainList[generator.MainList.Count - 2], slice[i]);
        }

        [Fact]
        public void SequenceToPrependedReversedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            generator.Sequence.Reverse();

            Coordinate prependedCoordinate = generator.RandomCoordinate();
            generator.Sequence.Prepend(prependedCoordinate);

            ICoordinateSequence<Coordinate> prependSeq
                = generator.SequenceFactory.Create(generator.PrependList);

            generator.Sequence.Prepend(prependSeq);

            Int32 i = 0;

            for (; i < generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i], generator.Sequence[i]);
            }

            Assert.Equal(prependedCoordinate, generator.Sequence[i]);
            Assert.Equal(generator.MainList[generator.MainList.Count - 1], generator.Sequence[i + 1]);
        }

        [Fact]
        public void SequenceToReversedPrependedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);

            Coordinate prependedCoordinate = generator.RandomCoordinate();
            generator.Sequence.Prepend(prependedCoordinate);
            generator.Sequence.Reverse();

            ICoordinateSequence<Coordinate> prependSeq
                = generator.SequenceFactory.Create(generator.PrependList);

            generator.Sequence.Prepend(prependSeq);

            Int32 i = 0;

            for (; i < generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i], generator.Sequence[i]);
            }

            Assert.Equal(generator.MainList[generator.MainList.Count - 1], generator.Sequence[i]);
            Assert.Equal(prependedCoordinate, generator.Sequence.Last);
        }

        [Fact]
        public void SequenceToPrependedReversedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, 3);
            slice.Reverse();

            Coordinate prependedCoordinate = generator.RandomCoordinate();
            slice.Prepend(prependedCoordinate);

            ICoordinateSequence<Coordinate> prependSeq
                = generator.SequenceFactory.Create(generator.PrependList);

            slice.Prepend(prependSeq);

            Int32 i = 0;

            for (; i < generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i], slice[i]);
            }

            Assert.Equal(prependedCoordinate, slice[i]);
            Assert.Equal(generator.MainList[generator.MainList.Count - 2], slice[i + 1]);
        }

        [Fact]
        public void SequenceToReversedPrependedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, 3);

            Coordinate prependedCoordinate = generator.RandomCoordinate();
            slice.Prepend(prependedCoordinate);
            slice.Reverse();

            ICoordinateSequence<Coordinate> prependSeq
                = generator.SequenceFactory.Create(generator.PrependList);

            slice.Prepend(prependSeq);

            Int32 i = 0;

            for (; i < generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i], slice[i]);
            }

            Assert.Equal(generator.MainList[generator.MainList.Count - 2], slice[i]);
            Assert.Equal(prependedCoordinate, slice.Last);
        }

        [Fact]
        public void ComplexSliceToPrependedReversedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            generator.Sequence.Reverse();

            Coordinate prependedCoordinate = generator.RandomCoordinate();
            generator.Sequence.Prepend(prependedCoordinate);

            ICoordinateSequence<Coordinate> prependSlice
                = generator.SequenceFactory.Create(generator.PrependList)
                    .Slice(0, 2);
            Coordinate preSliceCoordinate = generator.RandomCoordinate();
            Coordinate postSliceCoordinate = generator.RandomCoordinate();
            prependSlice.Prepend(preSliceCoordinate);
            prependSlice.Append(postSliceCoordinate);

            generator.Sequence.Prepend(prependSlice);

            Assert.Equal(preSliceCoordinate, generator.Sequence[0]);

            Int32 i = 1;

            for (; i <= generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i - 1], generator.Sequence[i]);
            }

            Assert.Equal(postSliceCoordinate, generator.Sequence[i]);
            Assert.Equal(prependedCoordinate, generator.Sequence[i + 1]);
            Assert.Equal(generator.MainList[generator.MainList.Count - 1], generator.Sequence[i + 2]);
        }

        [Fact]
        public void ComplexSliceToReversedPrependedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);

            Coordinate prependedCoordinate = generator.RandomCoordinate();
            generator.Sequence.Prepend(prependedCoordinate);
            generator.Sequence.Reverse();

            ICoordinateSequence<Coordinate> prependSlice
                = generator.SequenceFactory.Create(generator.PrependList)
                    .Slice(0, 2);
            Coordinate preSliceCoordinate = generator.RandomCoordinate();
            Coordinate postSliceCoordinate = generator.RandomCoordinate();
            prependSlice.Prepend(preSliceCoordinate);
            prependSlice.Append(postSliceCoordinate);

            generator.Sequence.Prepend(prependSlice);

            Assert.Equal(preSliceCoordinate, generator.Sequence[0]);

            Int32 i = 1;

            for (; i <= generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i - 1], generator.Sequence[i]);
            }

            Assert.Equal(postSliceCoordinate, generator.Sequence[i]);
            Assert.Equal(generator.MainList[generator.MainList.Count - 1], generator.Sequence[i + 1]);
            Assert.Equal(prependedCoordinate, generator.Sequence.Last);
        }

        [Fact]
        public void ComplexSliceToPrependedReversedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            ICoordinateSequence<Coordinate> target = generator.Sequence.Slice(1, 3);
            target.Reverse();

            Coordinate prependedCoordinate = generator.RandomCoordinate();
            target.Prepend(prependedCoordinate);

            ICoordinateSequence<Coordinate> prependSlice
                = generator.SequenceFactory.Create(generator.PrependList)
                    .Slice(0, 2);

            Coordinate preSliceCoordinate = generator.RandomCoordinate();
            Coordinate postSliceCoordinate = generator.RandomCoordinate();
            prependSlice.Prepend(preSliceCoordinate);
            prependSlice.Append(postSliceCoordinate);

            target.Prepend(prependSlice);

            Coordinate expected;
            Coordinate actual;

            expected = preSliceCoordinate;
            actual = target[0];
            Assert.Equal(expected, actual);

            Int32 i = 1;

            for (; i <= generator.PrependList.Count; i++)
            {
                expected = generator.PrependList[i - 1];
                actual = target[i];
                Assert.Equal(expected, actual);
            }

            expected = postSliceCoordinate;
            actual = target[i];
            Assert.Equal(expected, actual);

            expected = prependedCoordinate;
            actual = target[i + 1];
            Assert.Equal(expected, actual);

            expected = generator.MainList[generator.MainList.Count - 2];
            actual = target[i + 2];
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ComplexSliceToReversedPrependedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, 3);

            Coordinate prependedCoordinate = generator.RandomCoordinate();
            slice.Prepend(prependedCoordinate);
            slice.Reverse();

            ICoordinateSequence<Coordinate> prependSlice
                = generator.SequenceFactory.Create(generator.PrependList)
                    .Slice(0, 2);
            Coordinate preSliceCoordinate = generator.RandomCoordinate();
            Coordinate postSliceCoordinate = generator.RandomCoordinate();
            prependSlice.Prepend(preSliceCoordinate);
            prependSlice.Append(postSliceCoordinate);

            slice.Prepend(prependSlice);

            Assert.Equal(preSliceCoordinate, slice[0]);

            Int32 i = 1;

            for (; i <= generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i - 1], slice[i]);
            }

            Assert.Equal(postSliceCoordinate, slice[i]);
            Assert.Equal(generator.MainList[generator.MainList.Count - 2], slice[i + 1]);
            Assert.Equal(prependedCoordinate, slice.Last);
        }

        [Fact]
        public void CoordinateToPrependedSliceWithSkip()
        {
            Int32 mainLength = 12;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, sliceLength);

            Assert.True(slice.Remove(generator.MainList[3]));
            Assert.True(slice.Remove(generator.MainList[5]));
            Assert.True(slice.Remove(generator.MainList[7]));

            Coordinate coord1 = generator.RandomCoordinate();
            Coordinate coord0 = generator.RandomCoordinate();

            slice.Prepend(coord1);

            Assert.Equal(sliceLength - 3 + 1, slice.Count);
            Assert.Equal(coord1, slice[0]);
            Assert.Equal(generator.MainList[1], slice[1]);

            slice.Prepend(coord0);

            Assert.Equal(sliceLength - 3 + 1 + 1, slice.Count);
            Assert.Equal(coord0, slice[0]);
            Assert.Equal(coord1, slice[1]);
            Assert.Equal(generator.MainList[1], slice[2]);
        }

        [Fact]
        public void EnumerationToPrependedSliceWithSkip()
        {
            Int32 mainLength = 12;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 3, 0);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, sliceLength);

            Assert.True(slice.Remove(generator.MainList[3]));
            Assert.True(slice.Remove(generator.MainList[5]));
            Assert.True(slice.Remove(generator.MainList[7]));

            Coordinate prependedCoordinate = generator.RandomCoordinate();
            slice.Prepend(prependedCoordinate);

            Assert.Equal(sliceLength - 3 + 1, slice.Count);
            Assert.Equal(prependedCoordinate, slice[0]);
            Assert.Equal(generator.MainList[1], slice[1]);

            EnumerableIsolater<Coordinate> prependList
                = new EnumerableIsolater<Coordinate>(generator.PrependList);
            slice.Prepend(prependList);

            Assert.Equal(sliceLength - 3 + 1 + generator.PrependList.Count, slice.Count);

            Int32 i = 0;
            for (; i < generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i], slice[i]);
            }
            Assert.Equal(prependedCoordinate, slice[i]);
            Assert.Equal(generator.MainList[1], slice[i + 1]);
        }

        [Fact]
        public void SequenceToPrependedSliceWithSkip()
        {
            Int32 mainLength = 12;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 3, 0);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, sliceLength);

            Assert.True(slice.Remove(generator.MainList[3]));
            Assert.True(slice.Remove(generator.MainList[5]));
            Assert.True(slice.Remove(generator.MainList[7]));

            Coordinate prependedCoordinate = generator.RandomCoordinate();
            slice.Prepend(prependedCoordinate);

            Assert.Equal(sliceLength - 3 + 1, slice.Count);
            Assert.Equal(prependedCoordinate, slice[0]);
            Assert.Equal(generator.MainList[1], slice[1]);

            ICoordinateSequence<Coordinate> prependSeq = generator.SequenceFactory.Create(generator.PrependList);

            slice.Prepend(prependSeq);

            Assert.Equal(sliceLength - 3 + 1 + generator.PrependList.Count, slice.Count);

            Int32 i = 0;
            for (; i < generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i], slice[i]);
            }
            Assert.Equal(prependedCoordinate, slice[i]);
            Assert.Equal(generator.MainList[1], slice[i + 1]);
        }

        [Fact]
        public void ComplexSliceToPrependedSliceWithSkip()
        {
            Int32 mainLength = 12;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 3, 0);
            ICoordinateSequence<Coordinate> target = generator.Sequence.Slice(1, sliceLength);

            Assert.True(target.Remove(generator.MainList[3]));
            Assert.True(target.Remove(generator.MainList[5]));
            Assert.True(target.Remove(generator.MainList[7]));

            Coordinate prependedCoordinate = generator.RandomCoordinate();
            target.Prepend(prependedCoordinate);

            Assert.Equal(sliceLength - 3 + 1, target.Count);
            Assert.Equal(prependedCoordinate, target[0]);
            Assert.Equal(generator.MainList[1], target[1]);

            ICoordinateSequence<Coordinate> prependSlice = generator.SequenceFactory
                .Create(generator.PrependList)
                .Slice(0, 2);

            Coordinate preSliceCoordinate = generator.RandomCoordinate();
            Coordinate postSliceCoordinate = generator.RandomCoordinate();
            prependSlice.Prepend(preSliceCoordinate);
            prependSlice.Append(postSliceCoordinate);

            Assert.Equal(preSliceCoordinate, prependSlice.First);
            Assert.Equal(postSliceCoordinate, prependSlice.Last);

            target.Prepend(prependSlice);

            Assert.Equal(sliceLength - 3 + 1 + 3 + 1 + 1, target.Count);

            Assert.Equal(preSliceCoordinate, target[0]);
            Int32 i = 1;
            for (; i <= generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i - 1], target[i]);
            }
            Assert.Equal(postSliceCoordinate, target[i]);

            Assert.Equal(prependedCoordinate, target[i + 1]);
            Assert.Equal(generator.MainList[1], target[i + 2]);
        }

        [Fact]
        public void CoordinateToPrependedReversedSliceWithSkip()
        {
            Int32 mainLength = 12;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, sliceLength);

            Assert.True(slice.Remove(generator.MainList[3]));
            Assert.True(slice.Remove(generator.MainList[5]));
            Assert.True(slice.Remove(generator.MainList[7]));

            Coordinate coord1 = generator.RandomCoordinate();
            Coordinate coord0 = generator.RandomCoordinate();

            slice.Reverse();
            slice.Prepend(coord1);

            Assert.Equal(sliceLength - 3 + 1, slice.Count);
            Assert.Equal(coord1, slice[0]);
            Assert.Equal(generator.MainList[sliceLength], slice[1]);

            slice.Prepend(coord0);

            Assert.Equal(sliceLength - 3 + 1 + 1, slice.Count);
            Assert.Equal(coord0, slice[0]);
            Assert.Equal(coord1, slice[1]);
            Assert.Equal(generator.MainList[sliceLength], slice[2]);
        }

        [Fact]
        public void ComplexSliceToPrependedReversedSliceWithSkip()
        {
            Int32 mainLength = 12;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(mainLength, 3, 0);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, sliceLength);

            Assert.True(slice.Remove(generator.MainList[3]));
            Assert.True(slice.Remove(generator.MainList[5]));
            Assert.True(slice.Remove(generator.MainList[7]));

            slice.Reverse();

            Coordinate prependedCoordinate = generator.RandomCoordinate();
            slice.Prepend(prependedCoordinate);

            Assert.Equal(sliceLength - 3 + 1, slice.Count);
            Assert.Equal(prependedCoordinate, slice[0]);
            Assert.Equal(generator.MainList[sliceLength], slice[1]);

            // TODO: fix test - generator.PrependList is null
            ICoordinateSequence<Coordinate> prependSlice
                = generator.SequenceFactory.Create(generator.PrependList)
                    .Slice(0, 2);
            Coordinate preSliceCoordinate = generator.RandomCoordinate();
            Coordinate postSliceCoordinate = generator.RandomCoordinate();
            prependSlice.Prepend(preSliceCoordinate);
            prependSlice.Append(postSliceCoordinate);

            slice.Prepend(prependSlice);

            Assert.Equal(sliceLength - 3 + 1 + 3 + 1 + 1, slice.Count);

            Assert.Equal(preSliceCoordinate, slice[0]);
            Int32 i = 1;
            for (; i <= generator.PrependList.Count; i++)
            {
                Assert.Equal(generator.PrependList[i - 1], slice[i]);
            }
            Assert.Equal(postSliceCoordinate, slice[i]);

            Assert.Equal(prependedCoordinate, slice[i + 1]);
            Assert.Equal(generator.MainList[sliceLength], slice[i + 2]);
        }

        [Fact]
        public void VeryComplexSliceToVeryComplexSlice()
        {
            Int32 mainLength = 12;
            Int32 targetLength = mainLength - 2;
            Int32 addedLength = 10;

            // get all the coordinates
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, addedLength);
            Coordinate targetPrependedCoordinate = generator.RandomCoordinate();
            Coordinate targetAppendedCoordinate = generator.RandomCoordinate();
            Coordinate addedPrependedCoordinate = generator.RandomCoordinate();
            Coordinate addedAppendedCoordinate = generator.RandomCoordinate();

            // initialize and verify the very complex target slice
            ICoordinateSequence<Coordinate> target = generator.Sequence.Slice(1, targetLength);
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
            List<Coordinate> originalList = new List<Coordinate>(target);

            // initialize and verify the very complex added slice
            ICoordinateSequence<Coordinate> addedSlice
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
            List<Coordinate> addedList = new List<Coordinate>(addedSlice);


            // finally the test
            target.Prepend(addedSlice);


            // verify
            Assert.Equal(originalList.Count + addedList.Count, target.Count);

            IEnumerator<Coordinate> resultingSequence = target.GetEnumerator();
            foreach (Coordinate expected in addedSlice)
            {
                Assert.True(resultingSequence.MoveNext());
                Coordinate actual = resultingSequence.Current;
                Assert.Equal(expected, actual);
            }
            foreach (Coordinate expected in originalList)
            {
                Assert.True(resultingSequence.MoveNext());
                Coordinate actual = resultingSequence.Current;
                Assert.Equal(expected, actual);
            }
        }
    }
}