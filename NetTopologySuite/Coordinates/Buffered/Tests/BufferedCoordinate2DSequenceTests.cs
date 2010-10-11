using System;
using System.Collections;
using System.Collections.Generic;
using GeoAPI.Coordinates;
#if !DOTNET40
using GeoAPI.DataStructures.Collections.Generic;
#endif
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Coordinates;
using NPack;
using Xunit;

namespace ManagedBufferedCoordinateTests
{
    using IBufferedCoordFactory = ICoordinateFactory<BufferedCoordinate>;
    using IBufferedCoordSequence = ICoordinateSequence<BufferedCoordinate>;
    using IBufferedCoordSequenceFactory = ICoordinateSequenceFactory<BufferedCoordinate>;

    public class BufferedCoordinateSequenceTests
    {
        [Fact]
        public void ReversingCoordinateSequenceDoesntEqual()
        {
            SequenceGenerator generator = new SequenceGenerator(10, 10, 10);
            IBufferedCoordSequence seq1 = generator.NewSequence(true);
            IBufferedCoordSequence reversed = seq1.Reversed;

            Assert.False(seq1.Equals(reversed));
        }

        [Fact]
        public void CreatingCoordinateSequenceSucceeds()
        {
            BufferedCoordinateSequenceFactory factory
                = new BufferedCoordinateSequenceFactory();
            IBufferedCoordSequence seq = factory.Create(CoordinateDimensions.Two);

            Assert.NotNull(seq);
        }

        [Fact]
        public void CreatingCoordinateSequenceWithSpecificSizeSucceeds()
        {
            BufferedCoordinateSequenceFactory factory
                = new BufferedCoordinateSequenceFactory();
            IBufferedCoordSequence seq = factory.Create(200, CoordinateDimensions.Two);

            Assert.Equal(200, seq.Count);
        }

        [Fact(Skip = "3d Coords ok now")]
        public void CreatingCoordinateSequenceWith3DCoordinateFails()
        {
            Assert.Throws<NotSupportedException>(delegate
                                {
                                    BufferedCoordinateSequenceFactory factory
                                        = new BufferedCoordinateSequenceFactory();
                                    factory.Create(CoordinateDimensions.Three);
                                });
        }

        [Fact]
        public void CreatingCoordinateSequenceWithNegativeSpecificSizeFails()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate
                                {
                                    BufferedCoordinateSequenceFactory factory
                                        = new BufferedCoordinateSequenceFactory();
                                    factory.Create(-1, CoordinateDimensions.Two);
                                });
        }

        [Fact]
        public void CreatingCoordinateSequenceWithAnEnumerationOfCoordinatesSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(9999);

            IBufferedCoordSequence seq = generator.Sequence;

            Assert.Equal(9999, seq.Count);
        }

        [Fact]
        public void CreatingCoordinateSequenceWithoutRepeatedCoordinatesSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(1, 100);
            List<BufferedCoordinate> coords = generator.MainList;
            IBufferedCoordSequence seq1 = generator.NewSequence(coords, true);

            for (Int32 i = 0; i < 100; i++)
            {
                Assert.Equal(seq1[i], coords[i]);
            }

            Assert.Equal(100, seq1.Count);

            IBufferedCoordSequence seq2 = generator.NewSequence(coords, false);

            Assert.Equal(1, seq2.Count);
        }

        [Fact]
        public void CreatingSequenceAsUniqueSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 1000);

            IBufferedCoordSequence seq = generator.Sequence;

            seq = seq.WithoutDuplicatePoints();

#if DOTNET40
            System.Collections.Generic.ISet<BufferedCoordinate> pointSet = new SortedSet<BufferedCoordinate>(seq);
#else
            ISet<BufferedCoordinate> pointSet = new ListSet<BufferedCoordinate>(seq);
#endif

            Assert.Equal(pointSet.Count, seq.Count);
        }

        [Fact]
        public void SequenceToArraySucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(500000, 1000);

            IBufferedCoordSequence seq = generator.Sequence;

            BufferedCoordinate[] coordsArray = seq.ToArray();

            Assert.Equal(seq.Count, coordsArray.Length);

            for (Int32 i = 0; i < seq.Count; i++)
            {
                Assert.Equal(seq[i], coordsArray[i]);
            }
        }

        [Fact]
        public void AddingABufferedCoordinateFromVariousFactoriesSucceeds()
        {
            BufferedCoordinateFactory coordFactory1
                = new BufferedCoordinateFactory();

            BufferedCoordinateFactory coordFactory2
                = new BufferedCoordinateFactory();

            BufferedCoordinateSequenceFactory seqFactory
                = new BufferedCoordinateSequenceFactory(coordFactory1);

            IBufferedCoordSequence seq1 = seqFactory.Create(CoordinateDimensions.Two);

            seq1.Add(coordFactory1.Create(10, 20));
            seq1.Add(coordFactory1.Create(20, 30));

            seq1.Add(coordFactory2.Create(20, 30));
            seq1.Add(coordFactory2.Create(30, 40));

            Assert.Equal(3, coordFactory1.VectorBuffer.Count);
            Assert.Equal(2, coordFactory2.VectorBuffer.Count);

            Assert.Equal(4, seq1.Count);

            Assert.True(seq1.HasRepeatedCoordinates);
        }

        [Fact]
        public void AddingARangeOfBufferedCoordinateInstancesSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(500000, 1000);

            List<BufferedCoordinate> coordsToAdd = generator.MainList;

            IBufferedCoordSequence seq = generator.NewEmptySequence();

            seq.AddRange(coordsToAdd);

            Assert.Equal(coordsToAdd.Count, seq.Count);

            for (Int32 i = 0; i < coordsToAdd.Count; i++)
            {
                Assert.Equal(coordsToAdd[i], seq[i]);
            }
        }

        [Fact]
        public void AddingARangeOfBufferedCoordinateInstancesWithoutRepeatsInReverseSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator();

            IBufferedCoordSequence seq = generator.Sequence;
            ICoordinateFactory<BufferedCoordinate> coordFactory
                = generator.CoordinateFactory;

            List<BufferedCoordinate> coordsToAdd = new List<BufferedCoordinate>();
            coordsToAdd.Add(coordFactory.Create(10, 20));
            coordsToAdd.Add(coordFactory.Create(10, 20));
            coordsToAdd.Add(coordFactory.Create(20, 30));
            coordsToAdd.Add(coordFactory.Create(20, 30));
            coordsToAdd.Add(coordFactory.Create(30, 40));
            coordsToAdd.Add(coordFactory.Create(30, 40));

            seq.AddRange(coordsToAdd, false, true);

            Assert.Equal(3, seq.Count);

            for (Int32 i = 0; i < 3; i++)
            {
                Assert.True(coordsToAdd[i * 2].ValueEquals(seq[2 - i]));
            }
        }

        [Fact]
        public void AddingABufferedCoordinateSequenceFromTheSameFactorySucceeds()
        {
            BufferedCoordinateFactory coordFactory
                = new BufferedCoordinateFactory();

            BufferedCoordinateSequenceFactory seqFactory
                = new BufferedCoordinateSequenceFactory(coordFactory);

            IBufferedCoordSequence seq1
                = seqFactory.Create(CoordinateDimensions.Two);

            seq1.Add(coordFactory.Create(10, 20));
            seq1.Add(coordFactory.Create(11, 21));
            seq1.Add(coordFactory.Create(22, 32));
            seq1.Add(coordFactory.Create(23, 33));
            seq1.Add(coordFactory.Create(34, 44));
            seq1.Add(coordFactory.Create(35, 45));

            IBufferedCoordSequence seq2
                = seqFactory.Create(CoordinateDimensions.Two);

            seq2.AddSequence(seq1);

            Assert.Equal(seq1.Count, seq2.Count);

            for (Int32 i = 0; i < seq1.Count; i++)
            {
                Assert.True(seq1[i].Equals(seq2[i]));
            }
        }

        [Fact]
        public void AddingABufferedCoordinateSequenceFromADifferentFactorySucceeds()
        {
            BufferedCoordinateFactory coordFactory1
                = new BufferedCoordinateFactory();

            BufferedCoordinateSequenceFactory seqFactory1
                = new BufferedCoordinateSequenceFactory(coordFactory1);

            IBufferedCoordSequence seq1
                = seqFactory1.Create(CoordinateDimensions.Two);

            seq1.Add(coordFactory1.Create(10, 20));
            seq1.Add(coordFactory1.Create(11, 21));
            seq1.Add(coordFactory1.Create(22, 32));
            seq1.Add(coordFactory1.Create(23, 33));
            seq1.Add(coordFactory1.Create(34, 44));
            seq1.Add(coordFactory1.Create(35, 45));

            BufferedCoordinateSequenceFactory seqFactory2
                = new BufferedCoordinateSequenceFactory();

            IBufferedCoordSequence seq2
                = seqFactory2.Create(CoordinateDimensions.Two);

            seq2.AddSequence(seq1);

            Assert.Equal(seq1.Count, seq2.Count);

            for (Int32 i = 0; i < seq1.Count; i++)
            {
                Assert.False(seq1[i].Equals(seq2[i]));
                Assert.True(seq1[i].ValueEquals(seq2[i]));
            }
        }

        /*
        [Fact]
        public void ReturningASetFromAsSetSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(5000, 1000);

            IBufferedCoordSequence seq = generator.Sequence;

            ISet<BufferedCoordinate> set = seq.AsSet();

            Assert.NotNull(set);
        }
         */

        [Fact]
        public void CloneSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(5000, 1000);

            IBufferedCoordSequence seq1 = generator.Sequence;

            IBufferedCoordSequence seq2 = seq1.Clone();

            Assert.NotSame(seq1, seq2);
            Assert.Equal(seq1.Count, seq2.Count);

            for (Int32 i = 0; i < seq1.Count; i++)
            {
                Assert.Equal(seq1[i], seq2[i]);
            }
        }

        [Fact]
        public void ClosingARingSucceeds()
        {
            BufferedCoordinateFactory coordFactory
                = new BufferedCoordinateFactory();

            BufferedCoordinateSequenceFactory seqFactory
                = new BufferedCoordinateSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 0));
            seq.Add(coordFactory.Create(0, 1));
            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(1, 0));

            seq.CloseRing();

            Assert.Equal(5, seq.Count);
            Assert.Equal(seq.First, seq.Last);
            Assert.Equal(seq.Last, coordFactory.Create(0, 0));
        }

        [Fact]
        public void ClosingAnAlreadyClosedRingMakesNoChange()
        {
            BufferedCoordinateFactory coordFactory
                = new BufferedCoordinateFactory();

            BufferedCoordinateSequenceFactory seqFactory
                = new BufferedCoordinateSequenceFactory(coordFactory);

            // Create a ring which is closed by definition
            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 0));
            seq.Add(coordFactory.Create(0, 1));
            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(1, 0));
            seq.Add(coordFactory.Create(0, 0));

            seq.CloseRing();

            Assert.Equal(5, seq.Count);
            Assert.Equal(seq.First, seq.Last);

            // Create a ring which is not closed, close it, and reclose it
            seq = seqFactory.Create(CoordinateDimensions.Two);
            seq.Add(coordFactory.Create(0, 0));
            seq.Add(coordFactory.Create(0, 1));
            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(1, 0));

            seq.CloseRing();

            Assert.Equal(5, seq.Count);
            Assert.Equal(seq.First, seq.Last);

            seq.CloseRing();

            Assert.Equal(5, seq.Count);
            Assert.Equal(seq.First, seq.Last);
        }

        [Fact]
        public void ClosingARingOnASequenceWithFewerThan3PointsFails()
        {
            Assert.Throws<InvalidOperationException>(delegate
             {
                 BufferedCoordinateFactory coordFactory
                     = new BufferedCoordinateFactory();

                 BufferedCoordinateSequenceFactory seqFactory
                     = new BufferedCoordinateSequenceFactory(coordFactory);

                 IBufferedCoordSequence seq =
                     seqFactory.Create(CoordinateDimensions.Two);

                 seq.Add(coordFactory.Create(0, 0));
                 seq.Add(coordFactory.Create(0, 1));

                 seq.CloseRing();
             });
        }

        [Fact]
        public void ClosingARingOnASlicedSequenceSucceeds()
        {
            BufferedCoordinateFactory coordFactory
                = new BufferedCoordinateFactory();

            BufferedCoordinateSequenceFactory seqFactory
                = new BufferedCoordinateSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 0));
            seq.Add(coordFactory.Create(0, 1));
            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(1, 2));
            seq.Add(coordFactory.Create(2, 2));
            seq.Add(coordFactory.Create(2, 1));
            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(1, 0));

            // This sequence is a ring
            IBufferedCoordSequence slice1 = seq.Slice(2, 6);

            Assert.Equal(5, slice1.Count);
            Assert.Equal(slice1.First, slice1.Last);

            slice1.CloseRing();

            Assert.Equal(5, slice1.Count);
            Assert.Equal(slice1.First, slice1.Last);

            // This sequence is not a ring
            IBufferedCoordSequence slice2 = seq.Slice(2, 5);

            Assert.Equal(4, slice2.Count);
            Assert.NotEqual(slice2.First, slice2.Last);

            slice2.CloseRing();

            Assert.Equal(5, slice2.Count);
            Assert.Equal(slice2.First, slice2.Last);
        }

        [Fact]
        public void ContainsABufferedCoordinateFromTheSameBufferSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(500000, 1000);

            IBufferedCoordSequence seq = generator.Sequence;

            foreach (BufferedCoordinate coordinate2D in generator.MainList)
            {
                Assert.True(seq.Contains(coordinate2D));
            }
        }

        [Fact]
        public void ContainsABufferedCoordinateFromADifferentBufferFails()
        {
            SequenceGenerator generator1 = new SequenceGenerator();
            SequenceGenerator generator2 = new SequenceGenerator();

            // these coordinates come from a different buffer
            List<BufferedCoordinate> coordsToAdd
                = new List<BufferedCoordinate>(
                    generator2.GenerateCoordinates(1000, 500000));

            IBufferedCoordSequence seq = generator1.NewSequence(coordsToAdd);

            foreach (BufferedCoordinate coordinate2D in coordsToAdd)
            {
                Assert.False(seq.Contains(coordinate2D));
            }
        }

        [Fact]
        public void CompareToComputesLexicographicOrderingCorrectly()
        {
            BufferedCoordinateFactory coordFactory1
                = new BufferedCoordinateFactory();
            BufferedCoordinateSequenceFactory seqFactory1
                = new BufferedCoordinateSequenceFactory(coordFactory1);

            BufferedCoordinateFactory coordFactory2
                = new BufferedCoordinateFactory();
            BufferedCoordinateSequenceFactory seqFactory2
                = new BufferedCoordinateSequenceFactory(coordFactory2);

            IBufferedCoordSequence seq1 = seqFactory1.Create(CoordinateDimensions.Two);
            seq1.Add(coordFactory1.Create(1, 2));
            seq1.Add(coordFactory1.Create(3, 4));
            seq1.Add(coordFactory1.Create(5, 6));

            IBufferedCoordSequence seq2 = seqFactory2.Create(CoordinateDimensions.Two);
            seq2.Add(coordFactory2.Create(1, 2));
            seq2.Add(coordFactory2.Create(3, 4));
            seq2.Add(coordFactory2.Create(5, 6));

            Assert.Equal(0, seq1.CompareTo(seq2));

            seq1.Add(coordFactory1.Create(0, 0));

            Assert.Equal(1, seq1.CompareTo(seq2));

            seq2.Add(coordFactory2.Create(0, 0));

            Assert.Equal(0, seq1.CompareTo(seq2));

            seq2.Add(coordFactory2.Create(-1, -1));

            Assert.Equal(-1, seq1.CompareTo(seq2));

            seq1.Add(coordFactory1.Create(-1, 0));

            Assert.Equal(1, seq1.CompareTo(seq2));//jd: this is incorrect becuase the comparison is done on the index within the vector buffer
            //in our case there are two buffers so the 5th coordinate is in index[4] in both buffers however the values are different  
        }

        [Fact]
        public void CopyToArraySucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(200, 1000);

            IBufferedCoordSequence seq = generator.Sequence;

            BufferedCoordinate[] coordArray = new BufferedCoordinate[2000];
            seq.CopyTo(coordArray, 0);

            for (Int32 i = 0; i < seq.Count; i++)
            {
                Assert.Equal(seq[i], coordArray[i]);
            }

            seq.CopyTo(coordArray, 1000);

            for (Int32 i = 0; i < seq.Count; i++)
            {
                Assert.Equal(seq[i], coordArray[i + 1000]);
            }
        }

        [Fact]
        public void CountIsCorrectOnCreation()
        {
            SequenceGenerator generator = new SequenceGenerator(5000, 1000);

            IBufferedCoordSequence seq = generator.Sequence;

            Assert.Equal(1000, seq.Count);
        }

        [Fact]
        public void CountIsCorrectAfterAddOperations()
        {
            SequenceGenerator generator = new SequenceGenerator(100, 1000);
            BufferedCoordinateFactory coordFactory = generator.CoordinateFactory;
            IBufferedCoordSequence seq = generator.Sequence;

            seq.Add(coordFactory.Create(789, 456));

            Assert.Equal(1001, seq.Count);

            seq.AddRange(generator.GenerateCoordinates(100, 100));

            Assert.Equal(1101, seq.Count);
        }

        [Fact]
        public void CountIsCorrectAfterRemoveOperations()
        {
            SequenceGenerator generator = new SequenceGenerator(100, 1000);
            BufferedCoordinateFactory coordFactory = generator.CoordinateFactory;
            IBufferedCoordSequence seq = generator.Sequence;

            Boolean didRemove = seq.Remove(coordFactory.Create(-1, -1));
            Assert.False(didRemove);
            Assert.Equal(1000, seq.Count);

            BufferedCoordinate coord = seq[4];
            didRemove = seq.Remove(coord);
            Assert.True(didRemove);
            Assert.Equal(999, seq.Count);

            for (Int32 i = 0; i < 100; i++)
            {
                seq.RemoveAt(0);
                Assert.Equal(998 - i, seq.Count);
            }

            for (Int32 i = 0; i < 100; i++)
            {
                seq.RemoveAt(seq.Count - i - 1);
                Assert.Equal(898 - i, seq.Count);
            }
        }

        [Fact]
        public void CountIsCorrectAfterSliceOperation()
        {
            SequenceGenerator generator = new SequenceGenerator(100, 1000);

            IBufferedCoordSequence seq = generator.Sequence;

            IBufferedCoordSequence slice = seq.Slice(0, 9);

            Assert.Equal(10, slice.Count);

            slice = seq.Slice(990, 999);

            Assert.Equal(10, slice.Count);
        }

        [Fact]
        public void CountIsCorrectAfterSpliceOperation()
        {
            SequenceGenerator generator = new SequenceGenerator(100, 1000);

            IBufferedCoordSequence seq = generator.Sequence;

            IBufferedCoordSequence splice = seq.Splice(generator.NewCoordinate(-1, -1), 0, 9);

            Assert.Equal(11, splice.Count);

            splice = seq.Splice(generator.GenerateCoordinates(10, 100), 990, 999);

            Assert.Equal(20, splice.Count);

            splice = seq.Splice(generator.GenerateCoordinates(10, 100), 990, 999, generator.NewCoordinate(0, 0));

            Assert.Equal(21, splice.Count);

            splice = seq.Splice(generator.GenerateCoordinates(10, 100), 990, 999, generator.GenerateCoordinates(10, 100));

            Assert.Equal(30, splice.Count);

            splice = seq.Splice(generator.NewCoordinate(0, 0), 990, 999, generator.NewCoordinate(0, 0));

            Assert.Equal(12, splice.Count);

            splice = seq.Splice(990, 999, generator.NewCoordinate(0, 0));

            Assert.Equal(11, splice.Count);

            splice = seq.Splice(990, 999, generator.GenerateCoordinates(10, 100));

            Assert.Equal(20, splice.Count);
        }

        [Fact]
        public void EqualsTestsEqualityCorrectly()
        {
            SequenceGenerator generator = new SequenceGenerator(1000, 1000);
            IBufferedCoordSequenceFactory seqFactory = generator.SequenceFactory;
            List<BufferedCoordinate> coordsToAdd = generator.MainList;

            IBufferedCoordSequence seq1 = seqFactory.Create(coordsToAdd);
            IBufferedCoordSequence seq2 = seqFactory.Create(coordsToAdd);

            Assert.True(seq1.Equals(seq2));
        }


        [Fact]
        public void EnumeratorBasicOperationSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            Assert.Equal(generator.MainList.Count, generator.Sequence.Count);

            IEnumerator<BufferedCoordinate> enumerator = generator.Sequence.GetEnumerator();

            foreach (BufferedCoordinate coordinate in generator.MainList)
            {
                Assert.True(enumerator.MoveNext());
                Assert.Equal(coordinate, enumerator.Current);
            }

            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void EnumeratorOnReversedSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            generator.Sequence.Reverse();

            IEnumerator<BufferedCoordinate> enumerator = generator.Sequence.GetEnumerator();

            for (Int32 i = 0; i < generator.Sequence.Count; i++)
            {
                Assert.True(enumerator.MoveNext());
                BufferedCoordinate expected
                    = generator.MainList[generator.MainList.Count - i - 1];
                Assert.Equal(expected, enumerator.Current);
            }

            Assert.False(enumerator.MoveNext());
        }


        [Fact]
        public void EnumeratorOnSliceIsCorrect()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            IBufferedCoordSequence slice = generator.Sequence.Slice(5, 9);

            IEnumerator<BufferedCoordinate> enumerator = slice.GetEnumerator();

            for (Int32 i = 0; i < 5; i++)
            {
                Assert.True(enumerator.MoveNext());
                Assert.Equal(generator.Sequence[i + 5], enumerator.Current);
            }

            Assert.False(enumerator.MoveNext());

            IBufferedCoordSequence slice2 = slice.Slice(2, 4);

            enumerator = slice2.GetEnumerator();

            for (Int32 i = 0; i < 3; i++)
            {
                Assert.True(enumerator.MoveNext());
                Assert.Equal(slice[i + 2], enumerator.Current);
            }

            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void EnumeratorWithPrependedList()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 2, 0);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 4);

            slice.Prepend(generator.PrependList);

            Assert.Equal(generator.MainList.Count + generator.PrependList.Count, slice.Count);

            IEnumerator<BufferedCoordinate> enumerator = slice.GetEnumerator();

            foreach (BufferedCoordinate expected in generator.PrependList)
            {
                Assert.True(enumerator.MoveNext());
                Assert.Equal(expected, enumerator.Current);
            }

            foreach (BufferedCoordinate expected in generator.MainList)
            {
                Assert.True(enumerator.MoveNext());
                Assert.Equal(expected, enumerator.Current);
            }

            Assert.False(enumerator.MoveNext());
        }


        [Fact]
        public void EnumeratorWithAppendedList()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 0, 2);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 4);

            slice.Append(generator.AppendList);

            Assert.Equal(generator.MainList.Count + generator.AppendList.Count, slice.Count);

            IEnumerator<BufferedCoordinate> enumerator = slice.GetEnumerator();

            foreach (BufferedCoordinate expected in generator.MainList)
            {
                Assert.True(enumerator.MoveNext());
                Assert.Equal(expected, enumerator.Current);
            }

            foreach (BufferedCoordinate expected in generator.AppendList)
            {
                Assert.True(enumerator.MoveNext());
                Assert.Equal(expected, enumerator.Current);
            }

            Assert.False(enumerator.MoveNext());
        }


        [Fact]
        public void EnumeratorWithSkippedIndexList()
        {
            SequenceGenerator generator = new SequenceGenerator(5);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 4);

            slice.Remove(generator.MainList[1]);
            slice.Remove(generator.MainList[3]);

            Assert.Equal(generator.MainList.Count - 2, slice.Count);

            IEnumerator<BufferedCoordinate> enumerator = slice.GetEnumerator();

            for (Int32 i = 0; i < generator.MainList.Count; i++)
            {
                if (i != 1 && i != 3)
                {
                    Assert.True(enumerator.MoveNext());
                    Assert.Equal(generator.MainList[i], enumerator.Current);
                }
            }

            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void EnumeratorOnReversedSequenceWithPrependedAppendedSkippedIndices()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 2, 2);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 4);
            slice.Remove(generator.MainList[1]);
            slice.Remove(generator.MainList[3]);
            slice.Prepend(generator.PrependList);
            slice.Append(generator.AppendList);
            slice.Reverse();

            Int32 expectedCount = generator.MainList.Count - 2 +
                                  generator.PrependList.Count +
                                  generator.AppendList.Count;

            Assert.Equal(expectedCount, slice.Count);

            IEnumerator<BufferedCoordinate> enumerator = slice.GetEnumerator();

            for (Int32 i = generator.AppendList.Count - 1; i >= 0; i--)
            {
                Assert.True(enumerator.MoveNext());
                BufferedCoordinate expectedCoord = generator.AppendList[i];
                Assert.Equal(expectedCoord, enumerator.Current);
            }

            for (Int32 i = generator.MainList.Count - 1; i >= 0; i--)
            {
                if (i != 1 && i != 3)
                {
                    Assert.True(enumerator.MoveNext());
                    BufferedCoordinate expectedCoord = generator.MainList[i];
                    Assert.Equal(expectedCoord, enumerator.Current);
                }
            }

            for (Int32 i = generator.PrependList.Count - 1; i >= 0; i--)
            {
                Assert.True(enumerator.MoveNext());
                BufferedCoordinate expectedCoord = generator.PrependList[i];
                Assert.Equal(expectedCoord, enumerator.Current);
            }

            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void ExpandExtentsSucceeds()
        {
            BufferedCoordinateFactory coordinateFactory = new BufferedCoordinateFactory();
            BufferedCoordinateSequenceFactory sequenceFactory = new BufferedCoordinateSequenceFactory(coordinateFactory);
            GeometryFactory<BufferedCoordinate> geometryFactory = new GeometryFactory<BufferedCoordinate>(sequenceFactory);

            IBufferedCoordSequence sequence = sequenceFactory.Create(CoordinateDimensions.Two);
            sequence.Add(coordinateFactory.Create(1, 15));
            sequence.Add(coordinateFactory.Create(15, 1));

            IExtents<BufferedCoordinate> extents = sequence.ExpandExtents(geometryFactory.CreateExtents());

            Assert.Equal(1, extents.Min.X);
            Assert.Equal(1, extents.Min.Y);
            Assert.Equal(15, extents.Max.X);
            Assert.Equal(15, extents.Max.Y);
        }

        [Fact]
        public void FirstReturnsTheFirstCoordinate()
        {
            BufferedCoordinateFactory coordFactory
                = new BufferedCoordinateFactory();

            BufferedCoordinateSequenceFactory seqFactory
                = new BufferedCoordinateSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 0));

            Assert.Equal(seq.First, seq[0]);
        }

        [Fact]
        public void GetEnumeratorSucceeds()
        {
            BufferedCoordinateSequenceFactory seqFactory
                = new BufferedCoordinateSequenceFactory();

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            IEnumerator<BufferedCoordinate> enumerator = seq.GetEnumerator();

            Assert.NotNull(enumerator);
        }

        [Fact]
        public void HasRepeatedCoordinatesSucceeds()
        {
            BufferedCoordinateFactory coordFactory
                = new BufferedCoordinateFactory();

            BufferedCoordinateSequenceFactory seqFactory
                = new BufferedCoordinateSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 0));

            Assert.False(seq.HasRepeatedCoordinates);

            seq.Add(coordFactory.Create(1, 1));

            Assert.False(seq.HasRepeatedCoordinates);

            seq.Add(coordFactory.Create(1, 1));

            Assert.True(seq.HasRepeatedCoordinates);

            seq.RemoveAt(1);

            Assert.False(seq.HasRepeatedCoordinates);
        }

        [Fact]
        public void IncreasingDirectionIsCorrect()
        {
            BufferedCoordinateFactory coordFactory
                = new BufferedCoordinateFactory();

            BufferedCoordinateSequenceFactory seqFactory
                = new BufferedCoordinateSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            // palindrome - defined to be positive
            seq.Add(coordFactory.Create(0, 0));
            Assert.Equal(1, seq.IncreasingDirection);

            seq.Add(coordFactory.Create(1, 1));
            Assert.Equal(1, seq.IncreasingDirection);

            seq.Add(coordFactory.Create(-2, 2));
            Assert.Equal(-1, seq.IncreasingDirection);

            seq.Add(coordFactory.Create(-1, 2));
            Assert.Equal(-1, seq.IncreasingDirection);

            seq.Clear();

            seq.Add(coordFactory.Create(0, 0));
            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(2, 2));
            seq.Add(coordFactory.Create(0, 0));
            Assert.Equal(1, seq.IncreasingDirection);
        }

        [Fact]
        public void IndexOfSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            IBufferedCoordSequence seq = generator.Sequence;

            BufferedCoordinate coord;

            for (Int32 i = 0; i < 10; i++)
            {
                coord = seq[i];
                Assert.Equal(i, seq.IndexOf(coord));
            }

            coord = generator.NewCoordinate(Int32.MaxValue, Int32.MaxValue);
            Assert.Equal(-1, seq.IndexOf(coord));

            coord = seq[0];
            seq.Clear();

            Assert.Equal(-1, seq.IndexOf(coord));
        }

        [Fact]
        public void IListIndexerSetterFails()
        {
            Assert.Throws<NotImplementedException>(delegate
                                                       {
                                                           SequenceGenerator generator = new SequenceGenerator(10);
                                                           BufferedCoordinate value = generator.RandomCoordinate();
                                                           (generator.Sequence as IList)[2] = value;
                                                       });
        }

        [Fact]
        public void IListIndexerYieldsSameResultAsIndexer()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            Assert.Equal(generator.MainList.Count, generator.Sequence.Count);

            for (Int32 i = 0; i < generator.Sequence.Count; i++)
            {
                BufferedCoordinate implicitResult = generator.Sequence[i];
                Assert.Equal(generator.MainList[i], implicitResult);
                Object iListResult = (generator.Sequence as IList)[i];
                Assert.IsType(typeof(BufferedCoordinate), iListResult);
                Assert.Equal(implicitResult, (BufferedCoordinate)iListResult);
            }
        }

        [Fact]
        public void IndexerSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            Assert.Equal(generator.MainList.Count, generator.Sequence.Count);

            for (Int32 i = 0; i < generator.Sequence.Count; i++)
            {
                Assert.Equal(generator.MainList[i], generator.Sequence[i]);
            }
        }

        [Fact]
        public void IndexerOnReversedSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            generator.Sequence.Reverse();

            Assert.Equal(generator.MainList.Count, generator.Sequence.Count);

            for (Int32 i = 0; i < generator.Sequence.Count; i++)
            {
                BufferedCoordinate expected
                    = generator.MainList[generator.MainList.Count - i - 1];
                BufferedCoordinate actual = generator.Sequence[i];
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void CallingIndexerWithNegativeNumberFails()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate
                                {
                                    SequenceGenerator generator = new SequenceGenerator(10);

                                    BufferedCoordinate coord = generator.Sequence[-1];

                                    // this shouldn't be hit, due to the above exception
                                    Assert.Null(coord);
                                });
        }

        [Fact]
        public void CallingIndexerWithNumberEqualToCountFails()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate
                                {
                                    SequenceGenerator generator = new SequenceGenerator(10);

                                    BufferedCoordinate coord = generator.Sequence[generator.Sequence.Count];

                                    // this shouldn't be hit, due to the above exception
                                    Assert.Null(coord);
                                });
        }

        [Fact]
        public void CallingIndexerWithNumberGreaterThanCountFails()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate
                                {
                                    SequenceGenerator generator = new SequenceGenerator(10);

                                    BufferedCoordinate coord = generator.Sequence[Int32.MaxValue];

                                    // this shouldn't be hit, due to the above exception
                                    Assert.Null(coord);
                                });
        }

        [Fact]
        public void IndexerOnSliceIsCorrect()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            IBufferedCoordSequence slice = generator.Sequence.Slice(5, 9);

            for (Int32 i = 0; i < 5; i++)
            {
                Assert.Equal(generator.Sequence[i + 5], slice[i]);
            }

            IBufferedCoordSequence slice2 = slice.Slice(2, 4);

            for (Int32 i = 0; i < 3; i++)
            {
                Assert.Equal(slice[i + 2], slice2[i]);
            }
        }

        [Fact]
        public void IndexerWithPrependedList()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 2, 0);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 4);
            slice.Prepend(generator.PrependList);

            Assert.Equal(7, slice.Count);

            Assert.Equal(generator.PrependList[0], slice[0]);
            Assert.Equal(generator.PrependList[1], slice[1]);
            Assert.Equal(generator.MainList[0], slice[2]);
            Assert.Equal(generator.MainList[1], slice[3]);
            Assert.Equal(generator.MainList[2], slice[4]);
            Assert.Equal(generator.MainList[3], slice[5]);
            Assert.Equal(generator.MainList[4], slice[6]);
        }


        [Fact]
        public void IndexerWithAppendedList()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 0, 2);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 4);
            slice.Append(generator.AppendList);

            Assert.Equal(7, slice.Count);

            Assert.Equal(generator.MainList[0], slice[0]);
            Assert.Equal(generator.MainList[1], slice[1]);
            Assert.Equal(generator.MainList[2], slice[2]);
            Assert.Equal(generator.MainList[3], slice[3]);
            Assert.Equal(generator.MainList[4], slice[4]);
            Assert.Equal(generator.AppendList[0], slice[5]);
            Assert.Equal(generator.AppendList[1], slice[6]);
        }


        [Fact]
        public void IndexerWithSkippedIndexList()
        {
            SequenceGenerator generator = new SequenceGenerator(5);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 4);
            slice.Remove(generator.MainList[1]);
            slice.Remove(generator.MainList[3]);

            Assert.Equal(3, slice.Count);

            Assert.Equal(generator.MainList[0], slice[0]);
            Assert.Equal(generator.MainList[2], slice[1]);
            Assert.Equal(generator.MainList[4], slice[2]);
        }

        [Fact]
        public void IndexerIntoReversedSequenceWithPrependedAppendedSkippedIndices()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 2, 2);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 4);
            slice.Remove(generator.MainList[1]);
            slice.Remove(generator.MainList[3]);
            slice.Prepend(generator.PrependList);
            slice.Append(generator.AppendList);
            slice.Reverse();

            Assert.Equal(7, slice.Count);

            Assert.Equal(generator.AppendList[1], slice[0]);
            Assert.Equal(generator.AppendList[0], slice[1]);
            Assert.Equal(generator.MainList[4], slice[2]);
            Assert.Equal(generator.MainList[2], slice[3]);
            Assert.Equal(generator.MainList[0], slice[4]);
            Assert.Equal(generator.PrependList[1], slice[5]);
            Assert.Equal(generator.PrependList[0], slice[6]);
        }

        [Fact]
        public void Indexer2Succeeds()
        {
            BufferedCoordinateFactory coordFactory
                = new BufferedCoordinateFactory();

            BufferedCoordinateSequenceFactory seqFactory
                = new BufferedCoordinateSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create();

            seq.Add(coordFactory.Create(0, 1));
            seq.Add(coordFactory.Create(2, 3));
            seq.Add(coordFactory.Create(2, 3));

            Assert.Equal(0.0, seq[0, Ordinates.X]);
            Assert.Equal(1.0, seq[0, Ordinates.Y]);
            Assert.Equal(2.0, seq[1, Ordinates.X]);
            Assert.Equal(3.0, seq[1, Ordinates.Y]);
            Assert.Equal(2.0, seq[2, Ordinates.X]);
            Assert.Equal(3.0, seq[2, Ordinates.Y]);
        }

        [Fact]
        public void InsertSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            IBufferedCoordSequence seq = generator.NewEmptySequence();

            Int32 count = 0;

            IEnumerable<BufferedCoordinate> coords = generator.GenerateCoordinates(10);

            foreach (BufferedCoordinate expected in coords)
            {
                Int32 index = count % 2 == 0 ? 0 : count - 1;
                seq.Insert(index, expected);
                count++;
                BufferedCoordinate actual = seq[index];
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void IsFixedSizeIsCorrect()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            IBufferedCoordSequence seq = generator.Sequence;

            Assert.False(seq.IsFixedSize);

            seq.Freeze();

            Assert.True(seq.IsFixedSize);
        }

        [Fact]
        public void IsReadOnlyIsCorrect()
        {
            BufferedCoordinateFactory coordFactory
                = new BufferedCoordinateFactory();

            BufferedCoordinateSequenceFactory seqFactory
                = new BufferedCoordinateSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create();

            Assert.False(seq.IsReadOnly);

            seq.Freeze();

            Assert.True(seq.IsReadOnly);
        }

        [Fact]
        public void IsFrozenIsCorrect()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            IBufferedCoordSequence seq = generator.Sequence;

            Assert.False(seq.IsFrozen);

            seq.Freeze();

            Assert.True(seq.IsFrozen);
        }

        [Fact]
        public void LastIsTheLastCoordinate()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            IBufferedCoordSequence seq = generator.Sequence;

            Assert.Equal(seq[seq.Count - 1], seq.Last);

            // on an empty sequence, the last coordinate is an empty coordinate
            seq = generator.NewEmptySequence();

            Assert.Equal(0, seq.Count);
            Assert.Equal(new BufferedCoordinate(), seq.Last);
        }

        [Fact]
        public void LastIndexIsCorrect()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            IBufferedCoordSequence seq = generator.Sequence;

            Assert.Equal(seq.Count - 1, seq.LastIndex);

            seq = generator.NewEmptySequence();

            Assert.Equal(0, seq.Count);
            Assert.Equal(-1, seq.LastIndex);
        }


        [Fact]
        public void MaximumIsCorrectSingleItemSequence()
        {
            BufferedCoordinateFactory coordFactory
                = new BufferedCoordinateFactory();

            BufferedCoordinateSequenceFactory seqFactory
                = new BufferedCoordinateSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 0));

            Assert.Equal(coordFactory.Create(0, 0), seq.Maximum);
        }

        [Fact]
        public void MaximumIsCorrectFirstInMultiItemSequence()
        {
            BufferedCoordinateFactory coordFactory
                = new BufferedCoordinateFactory();

            BufferedCoordinateSequenceFactory seqFactory
                = new BufferedCoordinateSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(0, 0));
            seq.Add(coordFactory.Create(0, 1));

            Assert.Equal(coordFactory.Create(1, 1), seq.Maximum);
        }

        [Fact]
        public void MaximumIsCorrectLastInMultiItemSequence()
        {
            BufferedCoordinateFactory coordFactory
                = new BufferedCoordinateFactory();

            BufferedCoordinateSequenceFactory seqFactory
                = new BufferedCoordinateSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 1));
            seq.Add(coordFactory.Create(0, 0));
            seq.Add(coordFactory.Create(1, 1));

            Assert.Equal(coordFactory.Create(1, 1), seq.Maximum);
        }

        [Fact]
        public void MaximumIsCorrectMiddleOfMultiItemSequence()
        {
            BufferedCoordinateFactory coordFactory
                = new BufferedCoordinateFactory();

            BufferedCoordinateSequenceFactory seqFactory
                = new BufferedCoordinateSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 1));
            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(0, 0));

            Assert.Equal(coordFactory.Create(1, 1), seq.Maximum);
        }

        [Fact]
        public void MaximumIsCorrectAfterMaxInSequenceChanges()
        {
            BufferedCoordinateFactory coordFactory
                = new BufferedCoordinateFactory();

            BufferedCoordinateSequenceFactory seqFactory
                = new BufferedCoordinateSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 1));
            seq.Add(coordFactory.Create(1, 1));

            Assert.Equal(coordFactory.Create(1, 1), seq.Maximum);

            seq.Add(coordFactory.Create(2, 2));
            seq.Add(coordFactory.Create(1, 2));

            Assert.Equal(coordFactory.Create(2, 2), seq.Maximum);
        }


        [Fact]
        public void MaximumOnEmptySequenceReturnsEmptyCoordinate()
        {
            BufferedCoordinateFactory coordFactory
                = new BufferedCoordinateFactory();

            BufferedCoordinateSequenceFactory seqFactory
                = new BufferedCoordinateSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create();

            Assert.Equal(new BufferedCoordinate(), seq.Maximum);

            seq.Add(coordFactory.Create(0, 0));
        }


        [Fact]
        public void MinimumIsCorrectSingleItemSequence()
        {
            BufferedCoordinateFactory coordFactory
                = new BufferedCoordinateFactory();

            BufferedCoordinateSequenceFactory seqFactory
                = new BufferedCoordinateSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 0));

            Assert.Equal(coordFactory.Create(0, 0), seq.Minimum);
        }

        [Fact]
        public void MinimumIsCorrectFirstInMultiItemSequence()
        {
            BufferedCoordinateFactory coordFactory
                = new BufferedCoordinateFactory();

            BufferedCoordinateSequenceFactory seqFactory
                = new BufferedCoordinateSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 0));
            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(0, 1));

            Assert.Equal(coordFactory.Create(0, 0), seq.Minimum);
        }

        [Fact]
        public void MinimumIsCorrectLastInMultiItemSequence()
        {
            BufferedCoordinateFactory coordFactory
                = new BufferedCoordinateFactory();

            BufferedCoordinateSequenceFactory seqFactory
                = new BufferedCoordinateSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 1));
            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(0, 0));

            Assert.Equal(coordFactory.Create(0, 0), seq.Minimum);
        }

        [Fact]
        public void MinimumIsCorrectMiddleOfMultiItemSequence()
        {
            BufferedCoordinateFactory coordFactory
                = new BufferedCoordinateFactory();

            BufferedCoordinateSequenceFactory seqFactory
                = new BufferedCoordinateSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 1));
            seq.Add(coordFactory.Create(0, 0));
            seq.Add(coordFactory.Create(1, 1));

            Assert.Equal(coordFactory.Create(0, 0), seq.Minimum);
        }

        [Fact]
        public void MinimumIsCorrectAfterMinInSequenceChanges()
        {
            BufferedCoordinateFactory coordFactory
                = new BufferedCoordinateFactory();

            BufferedCoordinateSequenceFactory seqFactory
                = new BufferedCoordinateSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(3, 3));
            seq.Add(coordFactory.Create(1, 1));

            Assert.Equal(coordFactory.Create(1, 1), seq.Minimum);

            seq.Add(coordFactory.Create(0, 0));
            seq.Add(coordFactory.Create(0, 1));

            Assert.Equal(coordFactory.Create(0, 0), seq.Minimum);
        }

        [Fact]
        public void MinimumOnEmptySequenceReturnsEmptyCoordinate()
        {
            BufferedCoordinateFactory coordFactory
                = new BufferedCoordinateFactory();

            BufferedCoordinateSequenceFactory seqFactory
                = new BufferedCoordinateSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create();

            Assert.Equal(new BufferedCoordinate(), seq.Minimum);
        }

        [Fact]
        public void MergeSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(100);

            IBufferedCoordSequence seq1 = generator.Sequence;
            IBufferedCoordSequence seq2 = generator.NewSequence();

            IBufferedCoordSequence merged = seq1.Merge(seq2);

            Assert.Equal(200, merged.Count);

            for (int i = 0; i < seq1.Count; i++)
            {
                Assert.Equal(seq1[i], merged[i]);
            }

            for (int i = 0; i < seq1.Count; i++)
            {
                Assert.Equal(seq2[i], merged[i + 100]);
            }
        }

        [Fact]
        public void RemoveSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator();

            IBufferedCoordSequence seq = generator.Sequence;

            Assert.False(seq.Remove(new BufferedCoordinate()));

            // cannot add empty coordinate to sequence
            //seq.Add(coordFactory.Create());
            //Assert.True(seq.Remove(new BufferedCoordinate()));
            //Assert.Equal(0, seq.TotalItemCount);

            seq.Add(generator.NewCoordinate(0, 0));
            Assert.True(seq.Remove(generator.NewCoordinate(0, 0)));
            Assert.Equal(0, seq.Count);

            seq.AddRange(generator.GenerateCoordinates(10000));

            Int32 count = 10000;

            while (seq.Count > 0)
            {
                seq.Remove(seq.Last);
                Assert.True(--count == seq.Count);
            }
        }

        [Fact]
        public void RemoveFromSliceLeavesOriginalSequenceUnchanged()
        {
            Int32 mainLength = 52;
            Int32 sliceLength = mainLength - 2;

            SequenceGenerator generator = new SequenceGenerator(mainLength);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            Assert.True(slice.Remove(generator.MainList[20]));
            Assert.True(slice.Remove(generator.MainList[21]));
            Assert.True(slice.Remove(generator.MainList[22]));

            Assert.Equal(mainLength, generator.Sequence.Count);
            Assert.Equal(sliceLength - 3, slice.Count);

            for (Int32 i = 0; i < mainLength; i++)
            {
                Assert.Equal(generator.MainList[i], generator.Sequence[i]);
            }

            for (Int32 i = 1; i < 20; i++)
            {
                Assert.Equal(generator.MainList[i], slice[i - 1]);
            }

            for (Int32 i = 23; i <= sliceLength; i++)
            {
                Assert.Equal(generator.MainList[i], slice[i - 4]);
            }
        }

        [Fact]
        public void RemoveFromComplexSliceSucceeds()
        {
            Int32 mainLength = 202;
            Int32 sliceLength = mainLength - 2;
            Int32 xpendLength = 50;
            Int32 segmentBufferLength = 10;

            SequenceGenerator generator = new SequenceGenerator(mainLength, xpendLength, xpendLength);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            slice.Prepend(generator.PrependList);
            slice.Append(generator.AppendList);

            Assert.Equal(sliceLength + xpendLength + xpendLength, slice.Count);
            {
                Int32 i = 0;

                foreach (BufferedCoordinate coord in generator.PrependList)
                {
                    Assert.Equal(coord, slice[i++]);
                }

                for (Int32 j = 1; j <= sliceLength; j++)
                {
                    Assert.Equal(generator.MainList[j], slice[i++]);
                }

                foreach (BufferedCoordinate coord in generator.AppendList)
                {
                    Assert.Equal(coord, slice[i++]);
                }
            }

            Int32 removals = 0;

            for (Int32 i = segmentBufferLength + 1; i < sliceLength - segmentBufferLength + 1; i++)
            {
                Assert.True(slice.Remove(generator.MainList[i]));
                removals++;
            }

            for (Int32 i = segmentBufferLength; i < xpendLength - segmentBufferLength; i++)
            {
                Assert.True(slice.Remove(generator.AppendList[i]));
                removals++;
                Assert.True(slice.Remove(generator.PrependList[i]));
                removals++;
            }

            Assert.Equal(xpendLength + xpendLength + sliceLength - segmentBufferLength * 6, removals);

            Assert.Equal(segmentBufferLength * 6, slice.Count);

            for (Int32 i = 0; i < segmentBufferLength; i++)
            {
                Assert.Equal(generator.PrependList[i], slice[i]);
                Assert.Equal(generator.PrependList[i + xpendLength - segmentBufferLength], slice[i + segmentBufferLength]);
                Assert.Equal(generator.MainList[i + 1], slice[i + segmentBufferLength * 2]);
                Assert.Equal(generator.MainList[i + sliceLength - segmentBufferLength + 1], slice[i + segmentBufferLength * 3]);
                Assert.Equal(generator.AppendList[i], slice[i + segmentBufferLength * 4]);
                Assert.Equal(generator.AppendList[i + xpendLength - segmentBufferLength], slice[i + segmentBufferLength * 5]);
            }
        }

        [Fact]
        public void RemoveFromComplexReversedSliceSucceeds()
        {
            Int32 mainLength = 202;
            Int32 sliceLength = mainLength - 2;
            Int32 xpendLength = 50;
            Int32 segmentBufferLength = 10;

            SequenceGenerator generator = new SequenceGenerator(mainLength, xpendLength, xpendLength);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            slice.Prepend(generator.PrependList);
            slice.Append(generator.AppendList);
            slice.Reverse();

            Assert.Equal(sliceLength + xpendLength + xpendLength, slice.Count);
            {
                Int32 i = slice.Count - 1;

                foreach (BufferedCoordinate coord in generator.PrependList)
                {
                    Assert.Equal(coord, slice[i--]);
                }

                for (Int32 j = 1; j <= sliceLength; j++)
                {
                    Assert.Equal(generator.MainList[j], slice[i--]);
                }

                foreach (BufferedCoordinate coord in generator.AppendList)
                {
                    Assert.Equal(coord, slice[i--]);
                }
            }

            Int32 removals = 0;

            for (Int32 i = segmentBufferLength + 1; i < sliceLength - segmentBufferLength + 1; i++)
            {
                Assert.True(slice.Remove(generator.MainList[i]));
                removals++;
            }

            for (Int32 i = segmentBufferLength; i < xpendLength - segmentBufferLength; i++)
            {
                Assert.True(slice.Remove(generator.AppendList[i]));
                removals++;
                Assert.True(slice.Remove(generator.PrependList[i]));
                removals++;
            }

            Assert.Equal(xpendLength + xpendLength + sliceLength - segmentBufferLength * 6, removals);

            Assert.Equal(segmentBufferLength * 6, slice.Count);

            Int32 endIndex = slice.Count - 1;

            for (Int32 i = 0; i < segmentBufferLength; i++)
            {
                Assert.Equal(generator.PrependList[i], slice[endIndex - i]);
                Assert.Equal(generator.PrependList[i + xpendLength - segmentBufferLength], slice[endIndex - i - segmentBufferLength]);
                Assert.Equal(generator.MainList[i + 1], slice[endIndex - i - segmentBufferLength * 2]);
                Assert.Equal(generator.MainList[i + sliceLength - segmentBufferLength + 1], slice[endIndex - i - segmentBufferLength * 3]);
                Assert.Equal(generator.AppendList[i], slice[endIndex - i - segmentBufferLength * 4]);
                Assert.Equal(generator.AppendList[i + xpendLength - segmentBufferLength], slice[endIndex - i - segmentBufferLength * 5]);
            }
        }

        [Fact]
        public void RemoveAtSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(10000);

            IBufferedCoordSequence seq = generator.Sequence;

            Int32 count = 10000;

            Random rnd = new MersenneTwister();

            while (seq.Count > 0)
            {
                seq.RemoveAt(rnd.Next(0, count));
                Assert.True(--count == seq.Count);
            }
        }

        [Fact]
        public void ReverseSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator();
            List<BufferedCoordinate> coordsToTest
                = new List<BufferedCoordinate>(generator.GenerateCoordinates(10000));

            IBufferedCoordSequence seq = generator.Sequence;
            seq.AddRange(coordsToTest);
            seq.Reverse();

            Assert.Equal(coordsToTest.Count, seq.Count);

            Int32 count = coordsToTest.Count;

            for (Int32 i = 0; i < count; i++)
            {
                Assert.Equal(coordsToTest[i], seq[count - i - 1]);
            }
        }

        [Fact]
        public void ReversedIsCorrect()
        {
            SequenceGenerator generator = new SequenceGenerator(10000);

            IBufferedCoordSequence seq = generator.Sequence;
            IBufferedCoordSequence reversed = seq.Reversed;

            Assert.Equal(seq.Count, reversed.Count);

            Int32 count = seq.Count;

            for (Int32 i = 0; i < count; i++)
            {
                Assert.True(seq[i].Equals(reversed[count - i - 1]));
            }
        }

        [Fact]
        public void ScrollSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(10000);

            IBufferedCoordSequence seq = generator.Sequence;
            BufferedCoordinate firstCoord = seq.First;
            BufferedCoordinate midCoord = seq[5000];
            seq.Scroll(midCoord);
            Assert.Equal(midCoord, seq.First);
            seq.Scroll(5000);
            Assert.Equal(firstCoord, seq.First);
        }

        [Fact]
        public void SliceSingleCoordinateSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(1);
            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 0);

            Assert.Equal(1, slice.Count);
            Assert.Equal(generator.Sequence[0], slice[0]);
        }

        [Fact]
        public void SliceSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(10000);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1000, 1100);

            Assert.Equal(101, slice.Count);

            for (Int32 i = 0; i < slice.Count; i++)
            {
                Assert.Equal(generator.Sequence[i + 1000], slice[i]);
            }
        }


        [Fact]
        public void SliceSequenceWithPrepended()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 2, 0);

            generator.Sequence.Prepend(generator.PrependList);

            Assert.Equal(7, generator.Sequence.Count);

            IBufferedCoordSequence slice = generator.Sequence.Slice(1, 5);

            Assert.Equal(5, slice.Count);

            Assert.Equal(generator.PrependList[1], slice[0]);
            Assert.Equal(generator.MainList[0], slice[1]);
            Assert.Equal(generator.MainList[1], slice[2]);
            Assert.Equal(generator.MainList[2], slice[3]);
            Assert.Equal(generator.MainList[3], slice[4]);
        }

        [Fact]
        public void SliceSequenceWithAppended()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 0, 2);

            generator.Sequence.Append(generator.AppendList);

            Assert.Equal(7, generator.Sequence.Count);

            IBufferedCoordSequence slice = generator.Sequence.Slice(1, 5);

            Assert.Equal(5, slice.Count);

            Assert.Equal(generator.MainList[1], slice[0]);
            Assert.Equal(generator.MainList[2], slice[1]);
            Assert.Equal(generator.MainList[3], slice[2]);
            Assert.Equal(generator.MainList[4], slice[3]);
            Assert.Equal(generator.AppendList[0], slice[4]);
        }

        [Fact]
        public void SliceSequenceWithSkippedIndices()
        {
            SequenceGenerator generator = new SequenceGenerator(7);

            generator.Sequence.Remove(generator.MainList[2]);
            generator.Sequence.Remove(generator.MainList[4]);

            Assert.Equal(5, generator.Sequence.Count);

            IBufferedCoordSequence slice = generator.Sequence.Slice(1, 3);

            Assert.Equal(3, slice.Count);

            Assert.Equal(generator.MainList[1], slice[0]);
            Assert.Equal(generator.MainList[3], slice[1]);
            Assert.Equal(generator.MainList[5], slice[2]);
        }

        [Fact]
        public void SliceReversedSequenceWithPrependedAppendedSkippedIndices()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 2, 2);

            generator.Sequence.Remove(generator.MainList[1]);
            generator.Sequence.Remove(generator.MainList[3]); // count: 3
            generator.Sequence.Prepend(generator.PrependList); // prepending <5, 6>; count: 5
            generator.Sequence.Append(generator.AppendList); // appending <7, 8>; count: 7
            generator.Sequence.Reverse();
            // sequence: 8, 7, 4, 2, 0,   6, 5
            //           --------------  -------
            //                main       prepend

            Assert.Equal(7, generator.Sequence.Count);

            IBufferedCoordSequence slice = generator.Sequence.Slice(1, 5); // count 5

            // slice should be: 7, 4, 2, 0,    6,
            //                 ------------   ---
            //                     main        p
            Assert.Equal(5, slice.Count);

            Assert.Equal(generator.AppendList[0], slice[0]);
            Assert.Equal(generator.MainList[4], slice[1]);
            Assert.Equal(generator.MainList[2], slice[2]);
            Assert.Equal(generator.MainList[0], slice[3]);
            Assert.Equal(generator.PrependList[1], slice[4]);
        }

        [Fact]
        public void SliceSliceWithPrepended()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 2, 0);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 4);
            slice.Prepend(generator.PrependList);

            Assert.Equal(7, slice.Count);

            IBufferedCoordSequence slice2 = slice.Slice(1, 5);

            Assert.Equal(5, slice2.Count);

            Assert.Equal(generator.PrependList[1], slice2[0]);
            Assert.Equal(generator.MainList[0], slice2[1]);
            Assert.Equal(generator.MainList[1], slice2[2]);
            Assert.Equal(generator.MainList[2], slice2[3]);
            Assert.Equal(generator.MainList[3], slice2[4]);
        }

        [Fact]
        public void SliceSliceWithAppended()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 0, 2);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 4);
            slice.Append(generator.AppendList);

            Assert.Equal(7, slice.Count);

            IBufferedCoordSequence slice2 = slice.Slice(1, 5);

            Assert.Equal(5, slice2.Count);

            Assert.Equal(generator.MainList[1], slice2[0]);
            Assert.Equal(generator.MainList[2], slice2[1]);
            Assert.Equal(generator.MainList[3], slice2[2]);
            Assert.Equal(generator.MainList[4], slice2[3]);
            Assert.Equal(generator.AppendList[0], slice2[4]);
        }

        [Fact]
        public void SliceSliceWithSkippedIndices()
        {
            SequenceGenerator generator = new SequenceGenerator(7);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 6);
            slice.Remove(generator.MainList[2]);
            slice.Remove(generator.MainList[4]);

            Assert.Equal(5, slice.Count);

            IBufferedCoordSequence slice2 = slice.Slice(1, 3);

            Assert.Equal(3, slice2.Count);

            Assert.Equal(generator.MainList[1], slice2[0]);
            Assert.Equal(generator.MainList[3], slice2[1]);
            Assert.Equal(generator.MainList[5], slice2[2]);
        }

        [Fact]
        public void SliceSliceWithPrependedAppendedSkippedIndices()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 2, 2);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 4);
            slice.Remove(generator.MainList[1]);
            slice.Remove(generator.MainList[3]);
            slice.Prepend(generator.PrependList);
            slice.Append(generator.AppendList);

            Assert.Equal(7, slice.Count);

            IBufferedCoordSequence slice2 = slice.Slice(1, 5);

            Assert.Equal(5, slice2.Count);

            Assert.Equal(generator.PrependList[1], slice2[0]);
            Assert.Equal(generator.MainList[0], slice2[1]);
            Assert.Equal(generator.MainList[2], slice2[2]);
            Assert.Equal(generator.MainList[4], slice2[3]);
            Assert.Equal(generator.AppendList[0], slice2[4]);
        }

        [Fact]
        public void SliceReversedSliceWithPrependedAppendedSkippedIndices()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 2, 2);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 4);
            slice.Remove(generator.MainList[1]);
            slice.Remove(generator.MainList[3]);
            slice.Prepend(generator.PrependList);
            slice.Append(generator.AppendList);
            slice.Reverse();

            Assert.Equal(7, slice.Count);

            IBufferedCoordSequence slice2 = slice.Slice(1, 5);

            Assert.Equal(5, slice2.Count);

            Assert.Equal(generator.AppendList[0], slice2[0]);
            Assert.Equal(generator.MainList[4], slice2[1]);
            Assert.Equal(generator.MainList[2], slice2[2]);
            Assert.Equal(generator.MainList[0], slice2[3]);
            Assert.Equal(generator.PrependList[1], slice2[4]);
        }

        [Fact]
        public void SlicingASequenceFreezesTheParentSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(10000);

            IBufferedCoordSequence seq = generator.Sequence;
            Assert.False(seq.IsFrozen);
            Assert.False(seq.IsReadOnly);
            Assert.False(seq.IsFixedSize);

            seq.Slice(1000, 1100);

            Assert.True(seq.IsFrozen);
            Assert.True(seq.IsReadOnly);
            Assert.True(seq.IsFixedSize);
        }

        [Fact]
        public void SortIsCorrect()
        {
            SequenceGenerator generator = new SequenceGenerator(10000);

            IBufferedCoordSequence seq = generator.Sequence;

            seq.Sort();

            for (Int32 i = 1; i < seq.Count; i++)
            {
                Assert.True(seq[i].CompareTo(seq[i - 1]) >= 0);
            }
        }

        [Fact]
        public void SpliceIsCorrect()
        {
            SequenceGenerator generator = new SequenceGenerator(10000);
            BufferedCoordinateFactory coordFactory = generator.CoordinateFactory;
            IBufferedCoordSequence seq = generator.Sequence;

            List<BufferedCoordinate> coordsToAdd
                = new List<BufferedCoordinate>(generator.GenerateCoordinates(100));

            // Prepend enumeration
            IBufferedCoordSequence splice = seq.Splice(coordsToAdd, 5000, 5099);

            Assert.Equal(200, splice.Count);

            for (Int32 i = 0; i < splice.Count; i++)
            {
                if (i >= 100)
                {
                    Assert.Equal(seq[4900 + i], splice[i]);
                }
                else
                {
                    Assert.Equal(coordsToAdd[i], splice[i]);
                }
            }

            // Append enumeration
            splice = seq.Splice(9900, 9999, coordsToAdd);

            Assert.Equal(200, splice.Count);

            for (Int32 i = 0; i < splice.Count; i++)
            {
                if (i >= 100)
                {
                    Assert.Equal(coordsToAdd[i - 100], splice[i]);
                }
                else
                {
                    Assert.Equal(seq[9900 + i], splice[i]);
                }
            }

            // Prepend single
            splice = seq.Splice(coordFactory.Create(-1, -1), 0, 99);

            Assert.Equal(101, splice.Count);

            for (Int32 i = 0; i < splice.Count; i++)
            {
                if (i == 0)
                {
                    Assert.Equal(coordFactory.Create(-1, -1), splice[i]);
                }
                else
                {
                    Assert.Equal(seq[i - 1], splice[i]);
                }
            }

            // Append single
            splice = seq.Splice(1000, 1099, coordFactory.Create(-1, -1));

            Assert.Equal(101, splice.Count);

            for (Int32 i = 0; i < splice.Count; i++)
            {
                if (i >= 100)
                {
                    Assert.Equal(coordFactory.Create(-1, -1), splice[i]);
                }
                else
                {
                    Assert.Equal(seq[1000 + i], splice[i]);
                }
            }

            // Prepend single, append enumeration
            splice = seq.Splice(coordFactory.Create(-1, -1), 8000, 8099, coordsToAdd);

            Assert.Equal(201, splice.Count);

            for (Int32 i = 0; i < splice.Count; i++)
            {
                if (i == 0)
                {
                    Assert.Equal(coordFactory.Create(-1, -1), splice[i]);
                }
                else if (i <= 100)
                {
                    Assert.Equal(seq[8000 + i - 1], splice[i]);
                }
                else
                {
                    Assert.Equal(coordsToAdd[i - 101], splice[i]);
                }
            }

            // Prepend enumeration, append single
            splice = seq.Splice(coordsToAdd, 0, 0, coordFactory.Create(-1, -1));

            Assert.Equal(102, splice.Count);

            for (Int32 i = 0; i < splice.Count; i++)
            {
                if (i < 100)
                {
                    Assert.Equal(coordsToAdd[i], splice[i]);
                }
                else if (i == 100)
                {
                    Assert.Equal(seq[i - 100], splice[i]);
                }
                else
                {
                    Assert.Equal(coordFactory.Create(-1, -1), splice[i]);
                }
            }

            // Prepend single, append single
            splice = seq.Splice(coordFactory.Create(-1, -1), 0, 9999, coordFactory.Create(-1, -1));

            Assert.Equal(10002, splice.Count);

            for (Int32 i = 0; i < splice.Count; i++)
            {
                if (i > 0 && i < 10001)
                {
                    Assert.Equal(seq[i - 1], splice[i]);
                }
                else
                {
                    Assert.Equal(coordFactory.Create(-1, -1), splice[i]);
                }
            }

            // Prepend enumeration, append enumeration
            splice = seq.Splice(coordsToAdd, 9999, 9999, coordsToAdd);

            Assert.Equal(201, splice.Count);

            for (Int32 i = 0; i < splice.Count; i++)
            {
                if (i < 100 || i > 100)
                {
                    Assert.Equal(coordsToAdd[i > 100 ? i - 101 : i], splice[i]);
                }
                else
                {
                    Assert.Equal(seq[9999], splice[i]);
                }
            }
        }

        [Fact]
        public void WithoutDuplicatePointsCorrect()
        {
            SequenceGenerator generator = new SequenceGenerator(2, 10000);
            BufferedCoordinateFactory coordFactory = generator.CoordinateFactory;
            IBufferedCoordSequence seq = generator.Sequence;

            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(1, 2));
            seq.Add(coordFactory.Create(2, 1));
            seq.Add(coordFactory.Create(2, 2));
            seq.Add(coordFactory.Create(2, 2));
            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(1, 2));
            seq.Add(coordFactory.Create(2, 1));
            seq.Add(coordFactory.Create(2, 2));
            seq.Add(coordFactory.Create(2, 2));
            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(1, 2));
            seq.Add(coordFactory.Create(2, 1));
            seq.Add(coordFactory.Create(2, 2));
            seq.Add(coordFactory.Create(2, 2));

            IBufferedCoordSequence filtered = seq.WithoutDuplicatePoints();

            Assert.Equal(4, filtered.Count);

            Assert.True(filtered.Contains(coordFactory.Create(1, 1)));
            Assert.True(filtered.Contains(coordFactory.Create(1, 2)));
            Assert.True(filtered.Contains(coordFactory.Create(2, 1)));
            Assert.True(filtered.Contains(coordFactory.Create(2, 2)));
        }

        [Fact]
        public void WithoutRepeatedPointsCorrect()
        {
            SequenceGenerator generator = new SequenceGenerator();

            IBufferedCoordSequence seq = generator.NewSequence(
                generator.GenerateCoordinates(100000, 2), false);

            BufferedCoordinate last = new BufferedCoordinate();

            foreach (BufferedCoordinate coordinate in seq)
            {
                Assert.NotEqual(last, coordinate);
                last = coordinate;
            }
        }

        [Fact]
        public void ChangingSequenceElementDoesntAffectOtherSequencesWithTheSameCoordinate()
        {
            BufferedCoordinateSequenceFactory factory
                = new BufferedCoordinateSequenceFactory();

            IBufferedCoordSequence seq1
                = factory.Create(CoordinateDimensions.Two);
            IBufferedCoordSequence seq2
                = factory.Create(CoordinateDimensions.Two);

            ICoordinateFactory<BufferedCoordinate> coordFactory = factory.CoordinateFactory;

            Random rnd = new MersenneTwister();

            for (Int32 i = 0; i < 100; i++)
            {
                BufferedCoordinate coord = coordFactory.Create(rnd.NextDouble(),
                                                                 rnd.NextDouble());
                seq1.Add(coord);
                seq2.Add(coord);
                Assert.True(seq1[i].Equals(seq2[i]));
            }

            BufferedCoordinate c = seq1[10];
            Double x = c.X;
            Double y = c.Y;

            seq1[10] = coordFactory.Create(1234, 1234);

            Assert.Equal(x, seq2[10][Ordinates.X]);
            Assert.Equal(y, seq2[10][Ordinates.Y]);
        }

        //private IEnumerable<BufferedCoordinate> generateCoords(Int32 count,
        //                                                         Int32 max,
        //                                                         BufferedCoordinateFactory coordFactory)
        //{
        //    while (count-- > 0)
        //    {
        //        yield return coordFactory.Create(_rnd.Next(1, max + 1),
        //                                          _rnd.Next(1, max + 1));
        //    }
        //}
    }
}
