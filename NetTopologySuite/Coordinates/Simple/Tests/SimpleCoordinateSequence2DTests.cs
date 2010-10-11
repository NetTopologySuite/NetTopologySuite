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
using NUnit.Framework;

namespace SimpleCoordinateTests
{
    [TestFixture]
    public class CoordinateSequenceTests
    {
        [Test]
        public void ReversingCoordinateSequenceDoesntEqual()
        {
            SequenceGenerator generator = new SequenceGenerator(10, 10, 10);
            ICoordinateSequence<Coordinate> seq1 = generator.NewSequence(true);
            ICoordinateSequence<Coordinate> reversed = seq1.Reversed;

            Assert.IsFalse(seq1.Equals(reversed));
        }

        [Test]
        public void CreatingCoordinateSequenceSucceeds()
        {
            CoordinateSequenceFactory factory
                = new CoordinateSequenceFactory();
            ICoordinateSequence<Coordinate> seq = factory.Create(CoordinateDimensions.Two);

            Assert.IsNotNull(seq);
        }

        [Test]
        public void CreatingCoordinateSequenceWithSpecificSizeSucceeds()
        {
            CoordinateSequenceFactory factory
                = new CoordinateSequenceFactory();
            ICoordinateSequence<Coordinate> seq = factory.Create(200, CoordinateDimensions.Two);

            Assert.AreEqual(200, seq.Count);
        }

        //[Fact(Skip = "3d Coords ok now")]
        //public void CreatingCoordinateSequenceWith3DCoordinateFails()
        //{
        //    Assert.Throws<NotSupportedException>(delegate
        //                                             {
        //                                                 CoordinateSequenceFactory factory
        //                                                     = new CoordinateSequenceFactory();
        //                                                 factory.Create(CoordinateDimensions.Three);
        //                                             });
        //}

        //[Test]
        //public void CreatingCoordinateSequenceWithNegativeSpecificSizeFails()
        //{
        //    Assert.Throws<ArgumentOutOfRangeException>(delegate
        //                                                   {
        //                                                       CoordinateSequenceFactory factory
        //                                                           = new CoordinateSequenceFactory();
        //                                                       factory.Create(-1, CoordinateDimensions.Two);
        //                                                   });
        //}

        [Test]
        public void CreatingCoordinateSequenceWithAnEnumerationOfCoordinatesSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(9999);

            ICoordinateSequence<Coordinate> seq = generator.Sequence;

            Assert.AreEqual(9999, seq.Count);
        }

        [Test]
        public void CreatingCoordinateSequenceWithoutRepeatedCoordinatesSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(1, 100);
            List<Coordinate> coords = generator.MainList;
            ICoordinateSequence<Coordinate> seq1 = generator.NewSequence(coords, true);

            for (Int32 i = 0; i < 100; i++)
            {
                Assert.AreEqual(seq1[i], coords[i]);
            }

            Assert.AreEqual(100, seq1.Count);

            ICoordinateSequence<Coordinate> seq2 = generator.NewSequence(coords, false);

            Assert.AreEqual(1, seq2.Count);
        }
        /*
        [Test]
        public void CreatingSequenceAsUniqueSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 1000);

            ICoordinateSequence<Coordinate> seq = generator.Sequence;

            seq = seq.WithoutDuplicatePoints();

            Set<Coordinate> pointSet = new ListSet<Coordinate>(seq);

            Assert.AreEqual(pointSet.Count, seq.Count);
        }
        */
        [Test]
        public void SequenceToArraySucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(500000, 1000);

            ICoordinateSequence<Coordinate> seq = generator.Sequence;

            Coordinate[] coordsArray = seq.ToArray();

            Assert.AreEqual(seq.Count, coordsArray.Length);

            for (Int32 i = 0; i < seq.Count; i++)
            {
                Assert.AreEqual(seq[i], coordsArray[i]);
            }
        }

        [Test]
        public void AddingACoordinateFromVariousFactoriesSucceeds()
        {
            CoordinateFactory coordFactory1
                = new CoordinateFactory();

            CoordinateFactory coordFactory2
                = new CoordinateFactory();

            CoordinateSequenceFactory seqFactory
                = new CoordinateSequenceFactory(coordFactory1);

            ICoordinateSequence<Coordinate> seq1 = seqFactory.Create(CoordinateDimensions.Two);

            seq1.Add(coordFactory1.Create(10, 20));
            seq1.Add(coordFactory1.Create(20, 30));

            seq1.Add(coordFactory2.Create(20, 30));
            seq1.Add(coordFactory2.Create(30, 40));

            //Assert.AreEqual(3, coordFactory1.VectorBuffer.Count);
            //Assert.AreEqual(2, coordFactory2.VectorBuffer.Count);

            Assert.AreEqual(4, seq1.Count);

            Assert.IsTrue(seq1.HasRepeatedCoordinates);
        }

        [Test]
        public void AddingARangeOfCoordinateInstancesSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(500000, 1000);

            List<Coordinate> coordsToAdd = generator.MainList;

            ICoordinateSequence<Coordinate> seq = generator.NewEmptySequence();

            seq.AddRange(coordsToAdd);

            Assert.AreEqual(coordsToAdd.Count, seq.Count);

            for (Int32 i = 0; i < coordsToAdd.Count; i++)
            {
                Assert.AreEqual(coordsToAdd[i], seq[i]);
            }
        }

        [Test]
        public void AddingARangeOfCoordinateInstancesWithoutRepeatsInReverseSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator();

            ICoordinateSequence<Coordinate> seq = generator.Sequence;
            ICoordinateFactory<Coordinate> coordFactory
                = generator.CoordinateFactory;

            List<Coordinate> coordsToAdd = new List<Coordinate>();
            coordsToAdd.Add(coordFactory.Create(10, 20));
            coordsToAdd.Add(coordFactory.Create(10, 20));
            coordsToAdd.Add(coordFactory.Create(20, 30));
            coordsToAdd.Add(coordFactory.Create(20, 30));
            coordsToAdd.Add(coordFactory.Create(30, 40));
            coordsToAdd.Add(coordFactory.Create(30, 40));

            seq.AddRange(coordsToAdd, false, true);

            Assert.AreEqual(3, seq.Count);

            for (Int32 i = 0; i < 3; i++)
            {
                Assert.IsTrue(coordsToAdd[i * 2].ValueEquals(seq[2 - i]));
            }
        }

        [Test]
        public void AddingACoordinateSequenceFromTheSameFactorySucceeds()
        {
            CoordinateFactory coordFactory
                = new CoordinateFactory();

            CoordinateSequenceFactory seqFactory
                = new CoordinateSequenceFactory(coordFactory);

            ICoordinateSequence<Coordinate> seq1
                = seqFactory.Create(CoordinateDimensions.Two);

            seq1.Add(coordFactory.Create(10, 20));
            seq1.Add(coordFactory.Create(11, 21));
            seq1.Add(coordFactory.Create(22, 32));
            seq1.Add(coordFactory.Create(23, 33));
            seq1.Add(coordFactory.Create(34, 44));
            seq1.Add(coordFactory.Create(35, 45));

            ICoordinateSequence<Coordinate> seq2
                = seqFactory.Create(CoordinateDimensions.Two);

            seq2.AddSequence(seq1);

            Assert.AreEqual(seq1.Count, seq2.Count);

            for (Int32 i = 0; i < seq1.Count; i++)
            {
                Assert.IsTrue(seq1[i].Equals(seq2[i]));
            }
        }

        [Test]
        public void AddingACoordinateSequenceFromADifferentFactorySucceeds()
        {
            CoordinateFactory coordFactory1
                = new CoordinateFactory();

            CoordinateSequenceFactory seqFactory1
                = new CoordinateSequenceFactory(coordFactory1);

            ICoordinateSequence<Coordinate> seq1
                = seqFactory1.Create(CoordinateDimensions.Two);

            seq1.Add(coordFactory1.Create(10, 20));
            seq1.Add(coordFactory1.Create(11, 21));
            seq1.Add(coordFactory1.Create(22, 32));
            seq1.Add(coordFactory1.Create(23, 33));
            seq1.Add(coordFactory1.Create(34, 44));
            seq1.Add(coordFactory1.Create(35, 45));

            CoordinateSequenceFactory seqFactory2
                = new CoordinateSequenceFactory();

            ICoordinateSequence<Coordinate> seq2
                = seqFactory2.Create(CoordinateDimensions.Two);

            seq2.AddSequence(seq1);

            Assert.AreEqual(seq1.Count, seq2.Count);

            for (Int32 i = 0; i < seq1.Count; i++)
            {
                //Assert.IsFalse(seq1[i].Equals(seq2[i]));
                Assert.IsTrue(seq1[i].ValueEquals(seq2[i]));
            }
        }

        /*
        [Test]
        public void ReturningASetFromAsSetSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(5000, 1000);

            ICoordinateSequence<Coordinate> seq = generator.Sequence;

            ISet<Coordinate> set = seq.AsSet();

            Assert.IsNotNull(set);
        }
        */
        [Test]
        public void CloneSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(5000, 1000);

            ICoordinateSequence<Coordinate> seq1 = generator.Sequence;

            ICoordinateSequence<Coordinate> seq2 = seq1.Clone();

            Assert.AreNotSame(seq1, seq2);
            Assert.AreEqual(seq1.Count, seq2.Count);

            for (Int32 i = 0; i < seq1.Count; i++)
            {
                Assert.AreEqual(seq1[i], seq2[i]);
            }
        }

        [Test]
        public void ClosingARingSucceeds()
        {
            CoordinateFactory coordFactory
                = new CoordinateFactory();

            CoordinateSequenceFactory seqFactory
                = new CoordinateSequenceFactory(coordFactory);

            ICoordinateSequence<Coordinate> seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 0));
            seq.Add(coordFactory.Create(0, 1));
            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(1, 0));

            seq.CloseRing();

            Assert.AreEqual(5, seq.Count);
            Assert.AreEqual(seq.First, seq.Last);
            Assert.AreEqual(seq.Last, coordFactory.Create(0, 0));
        }

        [Test]
        public void ClosingAnAlreadyClosedRingMakesNoChange()
        {
            CoordinateFactory coordFactory
                = new CoordinateFactory();

            CoordinateSequenceFactory seqFactory
                = new CoordinateSequenceFactory(coordFactory);

            // Create a ring which is closed by definition
            ICoordinateSequence<Coordinate> seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 0));
            seq.Add(coordFactory.Create(0, 1));
            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(1, 0));
            seq.Add(coordFactory.Create(0, 0));

            seq.CloseRing();

            Assert.AreEqual(5, seq.Count);
            Assert.AreEqual(seq.First, seq.Last);

            // Create a ring which is not closed, close it, and reclose it
            seq = seqFactory.Create(CoordinateDimensions.Two);
            seq.Add(coordFactory.Create(0, 0));
            seq.Add(coordFactory.Create(0, 1));
            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(1, 0));

            seq.CloseRing();

            Assert.AreEqual(5, seq.Count);
            Assert.AreEqual(seq.First, seq.Last);

            seq.CloseRing();

            Assert.AreEqual(5, seq.Count);
            Assert.AreEqual(seq.First, seq.Last);
        }

        //[Test]
        //public void ClosingARingOnASequenceWithFewerThan3PointsFails()
        //{
        //    Assert.Throws<InvalidOperationException>(delegate
        //                                                 {
        //                                                     CoordinateFactory coordFactory
        //                                                         = new CoordinateFactory();

        //                                                     CoordinateSequenceFactory seqFactory
        //                                                         = new CoordinateSequenceFactory(coordFactory);

        //                                                     ICoordinateSequence<Coordinate> seq =
        //                                                         seqFactory.Create(CoordinateDimensions.Two);

        //                                                     seq.Add(coordFactory.Create(0, 0));
        //                                                     seq.Add(coordFactory.Create(0, 1));

        //                                                     seq.CloseRing();
        //                                                 });
        //}

        [Test]
        public void ClosingARingOnASlicedSequenceSucceeds()
        {
            CoordinateFactory coordFactory
                = new CoordinateFactory();

            CoordinateSequenceFactory seqFactory
                = new CoordinateSequenceFactory(coordFactory);

            ICoordinateSequence<Coordinate> seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 0));
            seq.Add(coordFactory.Create(0, 1));
            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(1, 2));
            seq.Add(coordFactory.Create(2, 2));
            seq.Add(coordFactory.Create(2, 1));
            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(1, 0));

            // This sequence is a ring
            ICoordinateSequence<Coordinate> slice1 = seq.Slice(2, 6);

            Assert.AreEqual(5, slice1.Count);
            Assert.AreEqual(slice1.First, slice1.Last);

            slice1.CloseRing();

            Assert.AreEqual(5, slice1.Count);
            Assert.AreEqual(slice1.First, slice1.Last);

            // This sequence is not a ring
            ICoordinateSequence<Coordinate> slice2 = seq.Slice(2, 5);

            Assert.AreEqual(4, slice2.Count);
            Assert.AreNotEqual(slice2.First, slice2.Last);

            slice2.CloseRing();

            Assert.AreEqual(5, slice2.Count);
            Assert.AreEqual(slice2.First, slice2.Last);
        }

        [Test]
        public void ContainsACoordinateFromTheSameBufferSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(500000, 1000);

            ICoordinateSequence<Coordinate> seq = generator.Sequence;

            foreach (Coordinate coordinate2D in generator.MainList)
            {
                Assert.IsTrue(seq.Contains(coordinate2D));
            }
        }

        [Test]
        [Ignore("Does not apply")]
        public void ContainsACoordinateFromADifferentBufferFails()
        {
            SequenceGenerator generator1 = new SequenceGenerator();
            SequenceGenerator generator2 = new SequenceGenerator();

            // these coordinates come from a different buffer
            List<Coordinate> coordsToAdd
                = new List<Coordinate>(
                    generator2.GenerateCoordinates(1000, 500000));

            ICoordinateSequence<Coordinate> seq = generator1.NewSequence(coordsToAdd);

            foreach (Coordinate coordinate2D in coordsToAdd)
            {
                Assert.IsFalse(seq.Contains(coordinate2D));
            }
        }

        [Test]
        public void CompareToComputesLexicographicOrderingCorrectly()
        {
            CoordinateFactory coordFactory1
                = new CoordinateFactory();
            CoordinateSequenceFactory seqFactory1
                = new CoordinateSequenceFactory(coordFactory1);

            CoordinateFactory coordFactory2
                = new CoordinateFactory();
            CoordinateSequenceFactory seqFactory2
                = new CoordinateSequenceFactory(coordFactory2);

            ICoordinateSequence<Coordinate> seq1 = seqFactory1.Create(CoordinateDimensions.Two);
            seq1.Add(coordFactory1.Create(1, 2));
            seq1.Add(coordFactory1.Create(3, 4));
            seq1.Add(coordFactory1.Create(5, 6));

            ICoordinateSequence<Coordinate> seq2 = seqFactory2.Create(CoordinateDimensions.Two);
            seq2.Add(coordFactory2.Create(1, 2));
            seq2.Add(coordFactory2.Create(3, 4));
            seq2.Add(coordFactory2.Create(5, 6));

            Assert.AreEqual(0, seq1.CompareTo(seq2));

            seq1.Add(coordFactory1.Create(0, 0));

            Assert.AreEqual(1, seq1.CompareTo(seq2));

            seq2.Add(coordFactory2.Create(0, 0));

            Assert.AreEqual(0, seq1.CompareTo(seq2));

            seq2.Add(coordFactory2.Create(-1, -1));

            Assert.AreEqual(-1, seq1.CompareTo(seq2));

            seq1.Add(coordFactory1.Create(-1, 0));

            Assert.AreEqual(1, seq1.CompareTo(seq2));//jd: this is incorrect becuase the comparison is done on the index within the vector buffer
            //in our case there are two buffers so the 5th coordinate is in index[4] in both buffers however the values are different  
        }

        [Test]
        public void CopyToArraySucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(200, 1000);

            ICoordinateSequence<Coordinate> seq = generator.Sequence;

            Coordinate[] coordArray = new Coordinate[2000];
            seq.CopyTo(coordArray, 0);

            for (Int32 i = 0; i < seq.Count; i++)
            {
                Assert.AreEqual(seq[i], coordArray[i]);
            }

            seq.CopyTo(coordArray, 1000);

            for (Int32 i = 0; i < seq.Count; i++)
            {
                Assert.AreEqual(seq[i], coordArray[i + 1000]);
            }
        }

        [Test]
        public void CountIsCorrectOnCreation()
        {
            SequenceGenerator generator = new SequenceGenerator(5000, 1000);

            ICoordinateSequence<Coordinate> seq = generator.Sequence;

            Assert.AreEqual(1000, seq.Count);
        }

        [Test]
        public void CountIsCorrectAfterAddOperations()
        {
            SequenceGenerator generator = new SequenceGenerator(100, 1000);
            CoordinateFactory coordFactory = generator.CoordinateFactory;
            ICoordinateSequence<Coordinate> seq = generator.Sequence;

            seq.Add(coordFactory.Create(789, 456));

            Assert.AreEqual(1001, seq.Count);

            seq.AddRange(generator.GenerateCoordinates(100, 100));

            Assert.AreEqual(1101, seq.Count);
        }

        [Test]
        public void CountIsCorrectAfterRemoveOperations()
        {
            SequenceGenerator generator = new SequenceGenerator(100, 1000);
            CoordinateFactory coordFactory = generator.CoordinateFactory;
            ICoordinateSequence<Coordinate> seq = generator.Sequence;

            Boolean didRemove = seq.Remove(coordFactory.Create(-1, -1));
            Assert.IsFalse(didRemove);
            Assert.AreEqual(1000, seq.Count);

            Coordinate coord = seq[4];
            didRemove = seq.Remove(coord);
            Assert.IsTrue(didRemove);
            Assert.AreEqual(999, seq.Count);

            for (Int32 i = 0; i < 100; i++)
            {
                seq.RemoveAt(0);
                Assert.AreEqual(998 - i, seq.Count);
            }

            for (Int32 i = 0; i < 100; i++)
            {
                seq.RemoveAt(seq.Count - i - 1);
                Assert.AreEqual(898 - i, seq.Count);
            }
        }

        [Test]
        public void CountIsCorrectAfterSliceOperation()
        {
            SequenceGenerator generator = new SequenceGenerator(100, 1000);

            ICoordinateSequence<Coordinate> seq = generator.Sequence;

            ICoordinateSequence<Coordinate> slice = seq.Slice(0, 9);

            Assert.AreEqual(10, slice.Count);

            slice = seq.Slice(990, 999);

            Assert.AreEqual(10, slice.Count);
        }

        [Test]
        public void CountIsCorrectAfterSpliceOperation()
        {
            SequenceGenerator generator = new SequenceGenerator(100, 1000);

            ICoordinateSequence<Coordinate> seq = generator.Sequence;

            ICoordinateSequence<Coordinate> splice = seq.Splice(generator.NewCoordinate(-1, -1), 0, 9);

            Assert.AreEqual(11, splice.Count);

            splice = seq.Splice(generator.GenerateCoordinates(10, 100), 990, 999);

            Assert.AreEqual(20, splice.Count);

            splice = seq.Splice(generator.GenerateCoordinates(10, 100), 990, 999, generator.NewCoordinate(0, 0));

            Assert.AreEqual(21, splice.Count);

            splice = seq.Splice(generator.GenerateCoordinates(10, 100), 990, 999, generator.GenerateCoordinates(10, 100));

            Assert.AreEqual(30, splice.Count);

            splice = seq.Splice(generator.NewCoordinate(0, 0), 990, 999, generator.NewCoordinate(0, 0));

            Assert.AreEqual(12, splice.Count);

            splice = seq.Splice(990, 999, generator.NewCoordinate(0, 0));

            Assert.AreEqual(11, splice.Count);

            splice = seq.Splice(990, 999, generator.GenerateCoordinates(10, 100));

            Assert.AreEqual(20, splice.Count);
        }

        [Test]
        public void EqualsTestsEqualityCorrectly()
        {
            SequenceGenerator generator = new SequenceGenerator(1000, 1000);
            ICoordinateSequenceFactory<Coordinate> seqFactory = generator.SequenceFactory;
            List<Coordinate> coordsToAdd = generator.MainList;

            ICoordinateSequence<Coordinate> seq1 = seqFactory.Create(coordsToAdd);
            ICoordinateSequence<Coordinate> seq2 = seqFactory.Create(coordsToAdd);

            Assert.IsTrue(seq1.Equals(seq2));
        }


        [Test]
        public void EnumeratorBasicOperationSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            Assert.AreEqual(generator.MainList.Count, generator.Sequence.Count);

            IEnumerator<Coordinate> enumerator = generator.Sequence.GetEnumerator();

            foreach (Coordinate coordinate in generator.MainList)
            {
                Assert.IsTrue(enumerator.MoveNext());
                Assert.AreEqual(coordinate, enumerator.Current);
            }

            Assert.IsFalse(enumerator.MoveNext());
        }

        [Test]
        public void EnumeratorOnReversedSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            generator.Sequence.Reverse();

            IEnumerator<Coordinate> enumerator = generator.Sequence.GetEnumerator();

            for (Int32 i = 0; i < generator.Sequence.Count; i++)
            {
                Assert.IsTrue(enumerator.MoveNext());
                Coordinate expected
                    = generator.MainList[generator.MainList.Count - i - 1];
                Assert.AreEqual(expected, enumerator.Current);
            }

            Assert.IsFalse(enumerator.MoveNext());
        }


        [Test]
        public void EnumeratorOnSliceIsCorrect()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(5, 9);

            IEnumerator<Coordinate> enumerator = slice.GetEnumerator();

            for (Int32 i = 0; i < 5; i++)
            {
                Assert.IsTrue(enumerator.MoveNext());
                Assert.AreEqual(generator.Sequence[i + 5], enumerator.Current);
            }

            Assert.IsFalse(enumerator.MoveNext());

            ICoordinateSequence<Coordinate> slice2 = slice.Slice(2, 4);

            enumerator = slice2.GetEnumerator();

            for (Int32 i = 0; i < 3; i++)
            {
                Assert.IsTrue(enumerator.MoveNext());
                Assert.AreEqual(slice[i + 2], enumerator.Current);
            }

            Assert.IsFalse(enumerator.MoveNext());
        }

        [Test]
        public void EnumeratorWithPrependedList()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 2, 0);

            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(0, 4);

            slice.Prepend(generator.PrependList);

            Assert.AreEqual(generator.MainList.Count + generator.PrependList.Count, slice.Count);

            IEnumerator<Coordinate> enumerator = slice.GetEnumerator();

            foreach (Coordinate expected in generator.PrependList)
            {
                Assert.IsTrue(enumerator.MoveNext());
                Assert.AreEqual(expected, enumerator.Current);
            }

            foreach (Coordinate expected in generator.MainList)
            {
                Assert.IsTrue(enumerator.MoveNext());
                Assert.AreEqual(expected, enumerator.Current);
            }

            Assert.IsFalse(enumerator.MoveNext());
        }


        [Test]
        public void EnumeratorWithAppendedList()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 0, 2);

            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(0, 4);

            slice.Append(generator.AppendList);

            Assert.AreEqual(generator.MainList.Count + generator.AppendList.Count, slice.Count);

            IEnumerator<Coordinate> enumerator = slice.GetEnumerator();

            foreach (Coordinate expected in generator.MainList)
            {
                Assert.IsTrue(enumerator.MoveNext());
                Assert.AreEqual(expected, enumerator.Current);
            }

            foreach (Coordinate expected in generator.AppendList)
            {
                Assert.IsTrue(enumerator.MoveNext());
                Assert.AreEqual(expected, enumerator.Current);
            }

            Assert.IsFalse(enumerator.MoveNext());
        }


        [Test]
        public void EnumeratorWithSkippedIndexList()
        {
            SequenceGenerator generator = new SequenceGenerator(5);

            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(0, 4);

            slice.Remove(generator.MainList[1]);
            slice.Remove(generator.MainList[3]);

            Assert.AreEqual(generator.MainList.Count - 2, slice.Count);

            IEnumerator<Coordinate> enumerator = slice.GetEnumerator();

            for (Int32 i = 0; i < generator.MainList.Count; i++)
            {
                if (i != 1 && i != 3)
                {
                    Assert.IsTrue(enumerator.MoveNext());
                    Assert.AreEqual(generator.MainList[i], enumerator.Current);
                }
            }

            Assert.IsFalse(enumerator.MoveNext());
        }

        [Test]
        public void EnumeratorOnReversedSequenceWithPrependedAppendedSkippedIndices()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 2, 2);

            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(0, 4);
            slice.Remove(generator.MainList[1]);
            slice.Remove(generator.MainList[3]);
            slice.Prepend(generator.PrependList);
            slice.Append(generator.AppendList);
            slice.Reverse();

            Int32 expectedCount = generator.MainList.Count - 2 +
                                  generator.PrependList.Count +
                                  generator.AppendList.Count;

            Assert.AreEqual(expectedCount, slice.Count);

            IEnumerator<Coordinate> enumerator = slice.GetEnumerator();

            for (Int32 i = generator.AppendList.Count - 1; i >= 0; i--)
            {
                Assert.IsTrue(enumerator.MoveNext());
                Coordinate expectedCoord = generator.AppendList[i];
                Assert.AreEqual(expectedCoord, enumerator.Current);
            }

            for (Int32 i = generator.MainList.Count - 1; i >= 0; i--)
            {
                if (i != 1 && i != 3)
                {
                    Assert.IsTrue(enumerator.MoveNext());
                    Coordinate expectedCoord = generator.MainList[i];
                    Assert.AreEqual(expectedCoord, enumerator.Current);
                }
            }

            for (Int32 i = generator.PrependList.Count - 1; i >= 0; i--)
            {
                Assert.IsTrue(enumerator.MoveNext());
                Coordinate expectedCoord = generator.PrependList[i];
                Assert.AreEqual(expectedCoord, enumerator.Current);
            }

            Assert.IsFalse(enumerator.MoveNext());
        }

        [Test]
        public void ExpandExtentsSucceeds()
        {
            CoordinateFactory coordinateFactory = new CoordinateFactory();
            CoordinateSequenceFactory sequenceFactory = new CoordinateSequenceFactory(coordinateFactory);
            GeometryFactory<Coordinate> geometryFactory = new GeometryFactory<Coordinate>(sequenceFactory);

            ICoordinateSequence<Coordinate> sequence = sequenceFactory.Create(CoordinateDimensions.Two);
            sequence.Add(coordinateFactory.Create(1, 15));
            sequence.Add(coordinateFactory.Create(15, 1));

            IExtents<Coordinate> extents = sequence.ExpandExtents(geometryFactory.CreateExtents());

            Assert.AreEqual(1, extents.Min.X);
            Assert.AreEqual(1, extents.Min.Y);
            Assert.AreEqual(15, extents.Max.X);
            Assert.AreEqual(15, extents.Max.Y);
        }

        [Test]
        public void FirstReturnsTheFirstCoordinate()
        {
            CoordinateFactory coordFactory
                = new CoordinateFactory();

            CoordinateSequenceFactory seqFactory
                = new CoordinateSequenceFactory(coordFactory);

            ICoordinateSequence<Coordinate> seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 0));

            Assert.AreEqual(seq.First, seq[0]);
        }

        [Test]
        public void GetEnumeratorSucceeds()
        {
            CoordinateSequenceFactory seqFactory
                = new CoordinateSequenceFactory();

            ICoordinateSequence<Coordinate> seq = seqFactory.Create(CoordinateDimensions.Two);

            IEnumerator<Coordinate> enumerator = seq.GetEnumerator();

            Assert.IsNotNull(enumerator);
        }

        [Test]
        public void HasRepeatedCoordinatesSucceeds()
        {
            CoordinateFactory coordFactory
                = new CoordinateFactory();

            CoordinateSequenceFactory seqFactory
                = new CoordinateSequenceFactory(coordFactory);

            ICoordinateSequence<Coordinate> seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 0));

            Assert.IsFalse(seq.HasRepeatedCoordinates);

            seq.Add(coordFactory.Create(1, 1));

            Assert.IsFalse(seq.HasRepeatedCoordinates);

            seq.Add(coordFactory.Create(1, 1));

            Assert.IsTrue(seq.HasRepeatedCoordinates);

            seq.RemoveAt(1);

            Assert.IsFalse(seq.HasRepeatedCoordinates);
        }

        [Test]
        public void IncreasingDirectionIsCorrect()
        {
            CoordinateFactory coordFactory
                = new CoordinateFactory();

            CoordinateSequenceFactory seqFactory
                = new CoordinateSequenceFactory(coordFactory);

            ICoordinateSequence<Coordinate> seq = seqFactory.Create(CoordinateDimensions.Two);

            // palindrome - defined to be positive
            seq.Add(coordFactory.Create(0, 0));
            Assert.AreEqual(1, seq.IncreasingDirection);

            seq.Add(coordFactory.Create(1, 1));
            Assert.AreEqual(1, seq.IncreasingDirection);

            seq.Add(coordFactory.Create(-2, 2));
            Assert.AreEqual(-1, seq.IncreasingDirection);

            seq.Add(coordFactory.Create(-1, 2));
            Assert.AreEqual(-1, seq.IncreasingDirection);

            seq.Clear();

            seq.Add(coordFactory.Create(0, 0));
            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(2, 2));
            seq.Add(coordFactory.Create(0, 0));
            Assert.AreEqual(1, seq.IncreasingDirection);
        }

        [Test]
        public void IndexOfSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            ICoordinateSequence<Coordinate> seq = generator.Sequence;

            Coordinate coord;

            for (Int32 i = 0; i < 10; i++)
            {
                coord = seq[i];
                Assert.AreEqual(i, seq.IndexOf(coord));
            }

            coord = generator.NewCoordinate(Int32.MaxValue, Int32.MaxValue);
            Assert.AreEqual(-1, seq.IndexOf(coord));

            coord = seq[0];
            seq.Clear();

            Assert.AreEqual(-1, seq.IndexOf(coord));
        }

        //[Test]
        //public void IListIndexerSetterFails()
        //{
        //    Assert.Throws<NotImplementedException>(delegate
        //                                               {
        //                                                   SequenceGenerator generator = new SequenceGenerator(10);
        //                                                   Coordinate value = generator.RandomCoordinate();
        //                                                   (generator.Sequence as IList)[2] = value;
        //                                               });
        //}

        [Test]
        public void IListIndexerYieldsSameResultAsIndexer()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            Assert.AreEqual(generator.MainList.Count, generator.Sequence.Count);

            for (Int32 i = 0; i < generator.Sequence.Count; i++)
            {
                Coordinate implicitResult = generator.Sequence[i];
                Assert.AreEqual(generator.MainList[i], implicitResult);
                Object iListResult = (generator.Sequence as IList)[i];
                Assert.IsInstanceOfType(typeof(Coordinate), iListResult);
                Assert.AreEqual(implicitResult, (Coordinate)iListResult);
            }
        }

        [Test]
        public void IndexerSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            Assert.AreEqual(generator.MainList.Count, generator.Sequence.Count);

            for (Int32 i = 0; i < generator.Sequence.Count; i++)
            {
                Assert.AreEqual(generator.MainList[i], generator.Sequence[i]);
            }
        }

        [Test]
        public void IndexerOnReversedSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            generator.Sequence.Reverse();

            Assert.AreEqual(generator.MainList.Count, generator.Sequence.Count);

            for (Int32 i = 0; i < generator.Sequence.Count; i++)
            {
                Coordinate expected
                    = generator.MainList[generator.MainList.Count - i - 1];
                Coordinate actual = generator.Sequence[i];
                Assert.AreEqual(expected, actual);
            }
        }

        //[Test]
        //public void CallingIndexerWithNegativeNumberFails()
        //{
        //    Assert.Fail(Throws<ArgumentOutOfRangeException>(delegate
        //                                                   {
        //                                                       SequenceGenerator generator = new SequenceGenerator(10);

        //                                                       Coordinate coord = generator.Sequence[-1];

        //                                                       // this shouldn't be hit, due to the above exception
        //                                                       Assert.Null(coord);
        //                                                   }));
        //}

        //[Test]
        //public void CallingIndexerWithNumberEqualToCountFails()
        //{
        //    Assert.Throws<ArgumentOutOfRangeException>(delegate
        //                                                   {
        //                                                       SequenceGenerator generator = new SequenceGenerator(10);

        //                                                       Coordinate coord = generator.Sequence[generator.Sequence.Count];

        //                                                       // this shouldn't be hit, due to the above exception
        //                                                       Assert.Null(coord);
        //                                                   });
        //}

        //[Test]
        //public void CallingIndexerWithNumberGreaterThanCountFails()
        //{
        //    Assert.Throws<ArgumentOutOfRangeException>(delegate
        //                                                   {
        //                                                       SequenceGenerator generator = new SequenceGenerator(10);

        //                                                       Coordinate coord = generator.Sequence[Int32.MaxValue];

        //                                                       // this shouldn't be hit, due to the above exception
        //                                                       Assert.Null(coord);
        //                                                   });
        //}

        [Test]
        public void IndexerOnSliceIsCorrect()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(5, 9);

            for (Int32 i = 0; i < 5; i++)
            {
                Assert.AreEqual(generator.Sequence[i + 5], slice[i]);
            }

            ICoordinateSequence<Coordinate> slice2 = slice.Slice(2, 4);

            for (Int32 i = 0; i < 3; i++)
            {
                Assert.AreEqual(slice[i + 2], slice2[i]);
            }
        }

        [Test]
        public void IndexerWithPrependedList()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 2, 0);

            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(0, 4);
            slice.Prepend(generator.PrependList);

            Assert.AreEqual(7, slice.Count);

            Assert.AreEqual(generator.PrependList[0], slice[0]);
            Assert.AreEqual(generator.PrependList[1], slice[1]);
            Assert.AreEqual(generator.MainList[0], slice[2]);
            Assert.AreEqual(generator.MainList[1], slice[3]);
            Assert.AreEqual(generator.MainList[2], slice[4]);
            Assert.AreEqual(generator.MainList[3], slice[5]);
            Assert.AreEqual(generator.MainList[4], slice[6]);
        }


        [Test]
        public void IndexerWithAppendedList()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 0, 2);

            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(0, 4);
            slice.Append(generator.AppendList);

            Assert.AreEqual(7, slice.Count);

            Assert.AreEqual(generator.MainList[0], slice[0]);
            Assert.AreEqual(generator.MainList[1], slice[1]);
            Assert.AreEqual(generator.MainList[2], slice[2]);
            Assert.AreEqual(generator.MainList[3], slice[3]);
            Assert.AreEqual(generator.MainList[4], slice[4]);
            Assert.AreEqual(generator.AppendList[0], slice[5]);
            Assert.AreEqual(generator.AppendList[1], slice[6]);
        }


        [Test]
        public void IndexerWithSkippedIndexList()
        {
            SequenceGenerator generator = new SequenceGenerator(5);

            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(0, 4);
            slice.Remove(generator.MainList[1]);
            slice.Remove(generator.MainList[3]);

            Assert.AreEqual(3, slice.Count);

            Assert.AreEqual(generator.MainList[0], slice[0]);
            Assert.AreEqual(generator.MainList[2], slice[1]);
            Assert.AreEqual(generator.MainList[4], slice[2]);
        }

        [Test]
        public void IndexerIntoReversedSequenceWithPrependedAppendedSkippedIndices()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 2, 2);

            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(0, 4);
            slice.Remove(generator.MainList[1]);
            slice.Remove(generator.MainList[3]);
            slice.Prepend(generator.PrependList);
            slice.Append(generator.AppendList);
            slice.Reverse();

            Assert.AreEqual(7, slice.Count);

            Assert.AreEqual(generator.AppendList[1], slice[0]);
            Assert.AreEqual(generator.AppendList[0], slice[1]);
            Assert.AreEqual(generator.MainList[4], slice[2]);
            Assert.AreEqual(generator.MainList[2], slice[3]);
            Assert.AreEqual(generator.MainList[0], slice[4]);
            Assert.AreEqual(generator.PrependList[1], slice[5]);
            Assert.AreEqual(generator.PrependList[0], slice[6]);
        }

        [Test]
        public void Indexer2Succeeds()
        {
            CoordinateFactory coordFactory
                = new CoordinateFactory();

            CoordinateSequenceFactory seqFactory
                = new CoordinateSequenceFactory(coordFactory);

            ICoordinateSequence<Coordinate> seq = seqFactory.Create();

            seq.Add(coordFactory.Create(0, 1));
            seq.Add(coordFactory.Create(2, 3));
            seq.Add(coordFactory.Create(2, 3));

            Assert.AreEqual(0.0, seq[0, Ordinates.X]);
            Assert.AreEqual(1.0, seq[0, Ordinates.Y]);
            Assert.AreEqual(2.0, seq[1, Ordinates.X]);
            Assert.AreEqual(3.0, seq[1, Ordinates.Y]);
            Assert.AreEqual(2.0, seq[2, Ordinates.X]);
            Assert.AreEqual(3.0, seq[2, Ordinates.Y]);
        }

        [Test]
        public void InsertSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            ICoordinateSequence<Coordinate> seq = generator.NewEmptySequence();

            Int32 count = 0;

            IEnumerable<Coordinate> coords = generator.GenerateCoordinates(10);

            foreach (Coordinate expected in coords)
            {
                Int32 index = count % 2 == 0 ? 0 : count - 1;
                seq.Insert(index, expected);
                count++;
                Coordinate actual = seq[index];
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void IsFixedSizeIsCorrect()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            ICoordinateSequence<Coordinate> seq = generator.Sequence;

            Assert.IsFalse(seq.IsFixedSize);

            seq.Freeze();

            Assert.IsTrue(seq.IsFixedSize);
        }

        [Test]
        public void IsReadOnlyIsCorrect()
        {
            CoordinateFactory coordFactory
                = new CoordinateFactory();

            CoordinateSequenceFactory seqFactory
                = new CoordinateSequenceFactory(coordFactory);

            ICoordinateSequence<Coordinate> seq = seqFactory.Create();

            Assert.IsFalse(seq.IsReadOnly);

            seq.Freeze();

            Assert.IsTrue(seq.IsReadOnly);
        }

        [Test]
        public void IsFrozenIsCorrect()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            ICoordinateSequence<Coordinate> seq = generator.Sequence;

            Assert.IsFalse(seq.IsFrozen);

            seq.Freeze();

            Assert.IsTrue(seq.IsFrozen);
        }

        [Test]
        public void LastIsTheLastCoordinate()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            ICoordinateSequence<Coordinate> seq = generator.Sequence;

            Assert.AreEqual(seq[seq.Count - 1], seq.Last);

            // on an empty sequence, the last coordinate is an empty coordinate
            seq = generator.NewEmptySequence();

            Assert.AreEqual(0, seq.Count);
            Assert.AreEqual(new Coordinate(), seq.Last);
        }

        [Test]
        public void LastIndexIsCorrect()
        {
            SequenceGenerator generator = new SequenceGenerator(10);

            ICoordinateSequence<Coordinate> seq = generator.Sequence;

            Assert.AreEqual(seq.Count - 1, seq.LastIndex);

            seq = generator.NewEmptySequence();

            Assert.AreEqual(0, seq.Count);
            Assert.AreEqual(-1, seq.LastIndex);
        }


        [Test]
        public void MaximumIsCorrectSingleItemSequence()
        {
            CoordinateFactory coordFactory
                = new CoordinateFactory();

            CoordinateSequenceFactory seqFactory
                = new CoordinateSequenceFactory(coordFactory);

            ICoordinateSequence<Coordinate> seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 0));

            Assert.AreEqual(coordFactory.Create(0, 0), seq.Maximum);
        }

        [Test]
        public void MaximumIsCorrectFirstInMultiItemSequence()
        {
            CoordinateFactory coordFactory
                = new CoordinateFactory();

            CoordinateSequenceFactory seqFactory
                = new CoordinateSequenceFactory(coordFactory);

            ICoordinateSequence<Coordinate> seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(0, 0));
            seq.Add(coordFactory.Create(0, 1));

            Assert.AreEqual(coordFactory.Create(1, 1), seq.Maximum);
        }

        [Test]
        public void MaximumIsCorrectLastInMultiItemSequence()
        {
            CoordinateFactory coordFactory
                = new CoordinateFactory();

            CoordinateSequenceFactory seqFactory
                = new CoordinateSequenceFactory(coordFactory);

            ICoordinateSequence<Coordinate> seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 1));
            seq.Add(coordFactory.Create(0, 0));
            seq.Add(coordFactory.Create(1, 1));

            Assert.AreEqual(coordFactory.Create(1, 1), seq.Maximum);
        }

        [Test]
        public void MaximumIsCorrectMiddleOfMultiItemSequence()
        {
            CoordinateFactory coordFactory
                = new CoordinateFactory();

            CoordinateSequenceFactory seqFactory
                = new CoordinateSequenceFactory(coordFactory);

            ICoordinateSequence<Coordinate> seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 1));
            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(0, 0));

            Assert.AreEqual(coordFactory.Create(1, 1), seq.Maximum);
        }

        [Test]
        public void MaximumIsCorrectAfterMaxInSequenceChanges()
        {
            CoordinateFactory coordFactory
                = new CoordinateFactory();

            CoordinateSequenceFactory seqFactory
                = new CoordinateSequenceFactory(coordFactory);

            ICoordinateSequence<Coordinate> seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 1));
            seq.Add(coordFactory.Create(1, 1));

            Assert.AreEqual(coordFactory.Create(1, 1), seq.Maximum);

            seq.Add(coordFactory.Create(2, 2));
            seq.Add(coordFactory.Create(1, 2));

            Assert.AreEqual(coordFactory.Create(2, 2), seq.Maximum);
        }


        [Test]
        public void MaximumOnEmptySequenceReturnsEmptyCoordinate()
        {
            CoordinateFactory coordFactory
                = new CoordinateFactory();

            CoordinateSequenceFactory seqFactory
                = new CoordinateSequenceFactory(coordFactory);

            ICoordinateSequence<Coordinate> seq = seqFactory.Create();

            Assert.AreEqual(new Coordinate(), seq.Maximum);

            seq.Add(coordFactory.Create(0, 0));
        }


        [Test]
        public void MinimumIsCorrectSingleItemSequence()
        {
            CoordinateFactory coordFactory
                = new CoordinateFactory();

            CoordinateSequenceFactory seqFactory
                = new CoordinateSequenceFactory(coordFactory);

            ICoordinateSequence<Coordinate> seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 0));

            Assert.AreEqual(coordFactory.Create(0, 0), seq.Minimum);
        }

        [Test]
        public void MinimumIsCorrectFirstInMultiItemSequence()
        {
            CoordinateFactory coordFactory
                = new CoordinateFactory();

            CoordinateSequenceFactory seqFactory
                = new CoordinateSequenceFactory(coordFactory);

            ICoordinateSequence<Coordinate> seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 0));
            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(0, 1));

            Assert.AreEqual(coordFactory.Create(0, 0), seq.Minimum);
        }

        [Test]
        public void MinimumIsCorrectLastInMultiItemSequence()
        {
            CoordinateFactory coordFactory
                = new CoordinateFactory();

            CoordinateSequenceFactory seqFactory
                = new CoordinateSequenceFactory(coordFactory);

            ICoordinateSequence<Coordinate> seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 1));
            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(0, 0));

            Assert.AreEqual(coordFactory.Create(0, 0), seq.Minimum);
        }

        [Test]
        public void MinimumIsCorrectMiddleOfMultiItemSequence()
        {
            CoordinateFactory coordFactory
                = new CoordinateFactory();

            CoordinateSequenceFactory seqFactory
                = new CoordinateSequenceFactory(coordFactory);

            ICoordinateSequence<Coordinate> seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 1));
            seq.Add(coordFactory.Create(0, 0));
            seq.Add(coordFactory.Create(1, 1));

            Assert.AreEqual(coordFactory.Create(0, 0), seq.Minimum);
        }

        [Test]
        public void MinimumIsCorrectAfterMinInSequenceChanges()
        {
            CoordinateFactory coordFactory
                = new CoordinateFactory();

            CoordinateSequenceFactory seqFactory
                = new CoordinateSequenceFactory(coordFactory);

            ICoordinateSequence<Coordinate> seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(3, 3));
            seq.Add(coordFactory.Create(1, 1));

            Assert.AreEqual(coordFactory.Create(1, 1), seq.Minimum);

            seq.Add(coordFactory.Create(0, 0));
            seq.Add(coordFactory.Create(0, 1));

            Assert.AreEqual(coordFactory.Create(0, 0), seq.Minimum);
        }

        [Test]
        public void MinimumOnEmptySequenceReturnsEmptyCoordinate()
        {
            CoordinateFactory coordFactory
                = new CoordinateFactory();

            CoordinateSequenceFactory seqFactory
                = new CoordinateSequenceFactory(coordFactory);

            ICoordinateSequence<Coordinate> seq = seqFactory.Create();

            Assert.AreEqual(new Coordinate(), seq.Minimum);
        }

        [Test]
        public void MergeSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(100);

            ICoordinateSequence<Coordinate> seq1 = generator.Sequence;
            ICoordinateSequence<Coordinate> seq2 = generator.NewSequence();

            ICoordinateSequence<Coordinate> merged = seq1.Merge(seq2);

            Assert.AreEqual(200, merged.Count);

            for (int i = 0; i < seq1.Count; i++)
            {
                Assert.AreEqual(seq1[i], merged[i]);
            }

            for (int i = 0; i < seq1.Count; i++)
            {
                Assert.AreEqual(seq2[i], merged[i + 100]);
            }
        }

        [Test]
        public void RemoveSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator();

            ICoordinateSequence<Coordinate> seq = generator.Sequence;

            Assert.IsFalse(seq.Remove(new Coordinate()));

            // cannot add empty coordinate to sequence
            //seq.Add(coordFactory.Create());
            //Assert.IsTrue(seq.Remove(new Coordinate()));
            //Assert.AreEqual(0, seq.TotalItemCount);

            seq.Add(generator.NewCoordinate(0, 0));
            Assert.IsTrue(seq.Remove(generator.NewCoordinate(0, 0)));
            Assert.AreEqual(0, seq.Count);

            seq.AddRange(generator.GenerateCoordinates(10000));

            Int32 count = 10000;

            while (seq.Count > 0)
            {
                seq.Remove(seq.Last);
                Assert.IsTrue(--count == seq.Count);
            }
        }

        [Test]
        public void RemoveFromSliceLeavesOriginalSequenceUnchanged()
        {
            Int32 mainLength = 52;
            Int32 sliceLength = mainLength - 2;

            SequenceGenerator generator = new SequenceGenerator(mainLength);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, sliceLength);

            Assert.IsTrue(slice.Remove(generator.MainList[20]));
            Assert.IsTrue(slice.Remove(generator.MainList[21]));
            Assert.IsTrue(slice.Remove(generator.MainList[22]));

            Assert.AreEqual(mainLength, generator.Sequence.Count);
            Assert.AreEqual(sliceLength - 3, slice.Count);

            for (Int32 i = 0; i < mainLength; i++)
            {
                Assert.AreEqual(generator.MainList[i], generator.Sequence[i]);
            }

            for (Int32 i = 1; i < 20; i++)
            {
                Assert.AreEqual(generator.MainList[i], slice[i - 1]);
            }

            for (Int32 i = 23; i <= sliceLength; i++)
            {
                Assert.AreEqual(generator.MainList[i], slice[i - 4]);
            }
        }

        [Test]
        public void RemoveFromComplexSliceSucceeds()
        {
            Int32 mainLength = 202;
            Int32 sliceLength = mainLength - 2;
            Int32 xpendLength = 50;
            Int32 segmentBufferLength = 10;

            SequenceGenerator generator = new SequenceGenerator(mainLength, xpendLength, xpendLength);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, sliceLength);

            slice.Prepend(generator.PrependList);
            slice.Append(generator.AppendList);

            Assert.AreEqual(sliceLength + xpendLength + xpendLength, slice.Count);
            {
                Int32 i = 0;

                foreach (Coordinate coord in generator.PrependList)
                {
                    Assert.AreEqual(coord, slice[i++]);
                }

                for (Int32 j = 1; j <= sliceLength; j++)
                {
                    Assert.AreEqual(generator.MainList[j], slice[i++]);
                }

                foreach (Coordinate coord in generator.AppendList)
                {
                    Assert.AreEqual(coord, slice[i++]);
                }
            }

            Int32 removals = 0;

            for (Int32 i = segmentBufferLength + 1; i < sliceLength - segmentBufferLength + 1; i++)
            {
                Assert.IsTrue(slice.Remove(generator.MainList[i]));
                removals++;
            }

            for (Int32 i = segmentBufferLength; i < xpendLength - segmentBufferLength; i++)
            {
                Assert.IsTrue(slice.Remove(generator.AppendList[i]));
                removals++;
                Assert.IsTrue(slice.Remove(generator.PrependList[i]));
                removals++;
            }

            Assert.AreEqual(xpendLength + xpendLength + sliceLength - segmentBufferLength * 6, removals);

            Assert.AreEqual(segmentBufferLength * 6, slice.Count);

            for (Int32 i = 0; i < segmentBufferLength; i++)
            {
                Assert.AreEqual(generator.PrependList[i], slice[i]);
                Assert.AreEqual(generator.PrependList[i + xpendLength - segmentBufferLength], slice[i + segmentBufferLength]);
                Assert.AreEqual(generator.MainList[i + 1], slice[i + segmentBufferLength * 2]);
                Assert.AreEqual(generator.MainList[i + sliceLength - segmentBufferLength + 1], slice[i + segmentBufferLength * 3]);
                Assert.AreEqual(generator.AppendList[i], slice[i + segmentBufferLength * 4]);
                Assert.AreEqual(generator.AppendList[i + xpendLength - segmentBufferLength], slice[i + segmentBufferLength * 5]);
            }
        }

        [Test]
        public void RemoveFromComplexReversedSliceSucceeds()
        {
            Int32 mainLength = 202;
            Int32 sliceLength = mainLength - 2;
            Int32 xpendLength = 50;
            Int32 segmentBufferLength = 10;

            SequenceGenerator generator = new SequenceGenerator(mainLength, xpendLength, xpendLength);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, sliceLength);

            slice.Prepend(generator.PrependList);
            slice.Append(generator.AppendList);
            slice.Reverse();

            Assert.AreEqual(sliceLength + xpendLength + xpendLength, slice.Count);
            {
                Int32 i = slice.Count - 1;

                foreach (Coordinate coord in generator.PrependList)
                {
                    Assert.AreEqual(coord, slice[i--]);
                }

                for (Int32 j = 1; j <= sliceLength; j++)
                {
                    Assert.AreEqual(generator.MainList[j], slice[i--]);
                }

                foreach (Coordinate coord in generator.AppendList)
                {
                    Assert.AreEqual(coord, slice[i--]);
                }
            }

            Int32 removals = 0;

            for (Int32 i = segmentBufferLength + 1; i < sliceLength - segmentBufferLength + 1; i++)
            {
                Assert.IsTrue(slice.Remove(generator.MainList[i]));
                removals++;
            }

            for (Int32 i = segmentBufferLength; i < xpendLength - segmentBufferLength; i++)
            {
                Assert.IsTrue(slice.Remove(generator.AppendList[i]));
                removals++;
                Assert.IsTrue(slice.Remove(generator.PrependList[i]));
                removals++;
            }

            Assert.AreEqual(xpendLength + xpendLength + sliceLength - segmentBufferLength * 6, removals);

            Assert.AreEqual(segmentBufferLength * 6, slice.Count);

            Int32 endIndex = slice.Count - 1;

            for (Int32 i = 0; i < segmentBufferLength; i++)
            {
                Assert.AreEqual(generator.PrependList[i], slice[endIndex - i]);
                Assert.AreEqual(generator.PrependList[i + xpendLength - segmentBufferLength], slice[endIndex - i - segmentBufferLength]);
                Assert.AreEqual(generator.MainList[i + 1], slice[endIndex - i - segmentBufferLength * 2]);
                Assert.AreEqual(generator.MainList[i + sliceLength - segmentBufferLength + 1], slice[endIndex - i - segmentBufferLength * 3]);
                Assert.AreEqual(generator.AppendList[i], slice[endIndex - i - segmentBufferLength * 4]);
                Assert.AreEqual(generator.AppendList[i + xpendLength - segmentBufferLength], slice[endIndex - i - segmentBufferLength * 5]);
            }
        }

        [Test]
        public void RemoveAtSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(10000);

            ICoordinateSequence<Coordinate> seq = generator.Sequence;

            Int32 count = 10000;

            Random rnd = new MersenneTwister();

            while (seq.Count > 0)
            {
                seq.RemoveAt(rnd.Next(0, count));
                Assert.IsTrue(--count == seq.Count);
            }
        }

        [Test]
        public void ReverseSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator();
            List<Coordinate> coordsToTest
                = new List<Coordinate>(generator.GenerateCoordinates(10000));

            ICoordinateSequence<Coordinate> seq = generator.Sequence;
            seq.AddRange(coordsToTest);
            seq.Reverse();

            Assert.AreEqual(coordsToTest.Count, seq.Count);

            Int32 count = coordsToTest.Count;

            for (Int32 i = 0; i < count; i++)
            {
                Assert.AreEqual(coordsToTest[i], seq[count - i - 1]);
            }
        }

        [Test]
        public void ReversedIsCorrect()
        {
            SequenceGenerator generator = new SequenceGenerator(10000);

            ICoordinateSequence<Coordinate> seq = generator.Sequence;
            ICoordinateSequence<Coordinate> reversed = seq.Reversed;

            Assert.AreEqual(seq.Count, reversed.Count);

            Int32 count = seq.Count;

            for (Int32 i = 0; i < count; i++)
            {
                Assert.IsTrue(seq[i].Equals(reversed[count - i - 1]));
            }
        }

        [Test]
        public void ScrollSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(10000);

            ICoordinateSequence<Coordinate> seq = generator.Sequence;
            Coordinate firstCoord = seq.First;
            Coordinate midCoord = seq[5000];
            seq.Scroll(midCoord);
            Assert.AreEqual(midCoord, seq.First);
            seq.Scroll(5000);
            Assert.AreEqual(firstCoord, seq.First);
        }

        [Test]
        public void SliceSingleCoordinateSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(1);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(0, 0);

            Assert.AreEqual(1, slice.Count);
            Assert.AreEqual(generator.Sequence[0], slice[0]);
        }

        [Test]
        public void SliceSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(10000);
            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1000, 1100);

            Assert.AreEqual(101, slice.Count);

            for (Int32 i = 0; i < slice.Count; i++)
            {
                Assert.AreEqual(generator.Sequence[i + 1000], slice[i]);
            }
        }


        [Test]
        public void SliceSequenceWithPrepended()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 2, 0);

            generator.Sequence.Prepend(generator.PrependList);

            Assert.AreEqual(7, generator.Sequence.Count);

            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, 5);

            Assert.AreEqual(5, slice.Count);

            Assert.AreEqual(generator.PrependList[1], slice[0]);
            Assert.AreEqual(generator.MainList[0], slice[1]);
            Assert.AreEqual(generator.MainList[1], slice[2]);
            Assert.AreEqual(generator.MainList[2], slice[3]);
            Assert.AreEqual(generator.MainList[3], slice[4]);
        }

        [Test]
        public void SliceSequenceWithAppended()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 0, 2);

            generator.Sequence.Append(generator.AppendList);

            Assert.AreEqual(7, generator.Sequence.Count);

            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, 5);

            Assert.AreEqual(5, slice.Count);

            Assert.AreEqual(generator.MainList[1], slice[0]);
            Assert.AreEqual(generator.MainList[2], slice[1]);
            Assert.AreEqual(generator.MainList[3], slice[2]);
            Assert.AreEqual(generator.MainList[4], slice[3]);
            Assert.AreEqual(generator.AppendList[0], slice[4]);
        }

        [Test]
        public void SliceSequenceWithSkippedIndices()
        {
            SequenceGenerator generator = new SequenceGenerator(7);

            generator.Sequence.Remove(generator.MainList[2]);
            generator.Sequence.Remove(generator.MainList[4]);

            Assert.AreEqual(5, generator.Sequence.Count);

            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, 3);

            Assert.AreEqual(3, slice.Count);

            Assert.AreEqual(generator.MainList[1], slice[0]);
            Assert.AreEqual(generator.MainList[3], slice[1]);
            Assert.AreEqual(generator.MainList[5], slice[2]);
        }

        [Test]
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

            Assert.AreEqual(7, generator.Sequence.Count);

            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(1, 5); // count 5

            // slice should be: 7, 4, 2, 0,    6,
            //                 ------------   ---
            //                     main        p
            Assert.AreEqual(5, slice.Count);

            Assert.AreEqual(generator.AppendList[0], slice[0]);
            Assert.AreEqual(generator.MainList[4], slice[1]);
            Assert.AreEqual(generator.MainList[2], slice[2]);
            Assert.AreEqual(generator.MainList[0], slice[3]);
            Assert.AreEqual(generator.PrependList[1], slice[4]);
        }

        [Test]
        public void SliceSliceWithPrepended()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 2, 0);

            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(0, 4);
            slice.Prepend(generator.PrependList);

            Assert.AreEqual(7, slice.Count);

            ICoordinateSequence<Coordinate> slice2 = slice.Slice(1, 5);

            Assert.AreEqual(5, slice2.Count);

            Assert.AreEqual(generator.PrependList[1], slice2[0]);
            Assert.AreEqual(generator.MainList[0], slice2[1]);
            Assert.AreEqual(generator.MainList[1], slice2[2]);
            Assert.AreEqual(generator.MainList[2], slice2[3]);
            Assert.AreEqual(generator.MainList[3], slice2[4]);
        }

        [Test]
        public void SliceSliceWithAppended()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 0, 2);

            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(0, 4);
            slice.Append(generator.AppendList);

            Assert.AreEqual(7, slice.Count);

            ICoordinateSequence<Coordinate> slice2 = slice.Slice(1, 5);

            Assert.AreEqual(5, slice2.Count);

            Assert.AreEqual(generator.MainList[1], slice2[0]);
            Assert.AreEqual(generator.MainList[2], slice2[1]);
            Assert.AreEqual(generator.MainList[3], slice2[2]);
            Assert.AreEqual(generator.MainList[4], slice2[3]);
            Assert.AreEqual(generator.AppendList[0], slice2[4]);
        }

        [Test]
        public void SliceSliceWithSkippedIndices()
        {
            SequenceGenerator generator = new SequenceGenerator(7);

            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(0, 6);
            slice.Remove(generator.MainList[2]);
            slice.Remove(generator.MainList[4]);

            Assert.AreEqual(5, slice.Count);

            ICoordinateSequence<Coordinate> slice2 = slice.Slice(1, 3);

            Assert.AreEqual(3, slice2.Count);

            Assert.AreEqual(generator.MainList[1], slice2[0]);
            Assert.AreEqual(generator.MainList[3], slice2[1]);
            Assert.AreEqual(generator.MainList[5], slice2[2]);
        }

        [Test]
        public void SliceSliceWithPrependedAppendedSkippedIndices()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 2, 2);

            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(0, 4);
            slice.Remove(generator.MainList[1]);
            slice.Remove(generator.MainList[3]);
            slice.Prepend(generator.PrependList);
            slice.Append(generator.AppendList);

            Assert.AreEqual(7, slice.Count);

            ICoordinateSequence<Coordinate> slice2 = slice.Slice(1, 5);

            Assert.AreEqual(5, slice2.Count);

            Assert.AreEqual(generator.PrependList[1], slice2[0]);
            Assert.AreEqual(generator.MainList[0], slice2[1]);
            Assert.AreEqual(generator.MainList[2], slice2[2]);
            Assert.AreEqual(generator.MainList[4], slice2[3]);
            Assert.AreEqual(generator.AppendList[0], slice2[4]);
        }

        [Test]
        public void SliceReversedSliceWithPrependedAppendedSkippedIndices()
        {
            SequenceGenerator generator = new SequenceGenerator(5, 2, 2);

            ICoordinateSequence<Coordinate> slice = generator.Sequence.Slice(0, 4);
            slice.Remove(generator.MainList[1]);
            slice.Remove(generator.MainList[3]);
            slice.Prepend(generator.PrependList);
            slice.Append(generator.AppendList);
            slice.Reverse();

            Assert.AreEqual(7, slice.Count);

            ICoordinateSequence<Coordinate> slice2 = slice.Slice(1, 5);

            Assert.AreEqual(5, slice2.Count);

            Assert.AreEqual(generator.AppendList[0], slice2[0]);
            Assert.AreEqual(generator.MainList[4], slice2[1]);
            Assert.AreEqual(generator.MainList[2], slice2[2]);
            Assert.AreEqual(generator.MainList[0], slice2[3]);
            Assert.AreEqual(generator.PrependList[1], slice2[4]);
        }

        //[Test]
        //public void SlicingASequenceFreezesTheParentSequence()
        //{
        //    SequenceGenerator generator = new SequenceGenerator(10000);

        //    ICoordinateSequence<Coordinate> seq = generator.Sequence;
        //    Assert.IsFalse(seq.IsFrozen);
        //    Assert.IsFalse(seq.IsReadOnly);
        //    Assert.IsFalse(seq.IsFixedSize);

        //    seq.Slice(1000, 1100);

        //    Assert.IsTrue(seq.IsFrozen);
        //    Assert.IsTrue(seq.IsReadOnly);
        //    Assert.IsTrue(seq.IsFixedSize);
        //}

        [Test]
        public void SortIsCorrect()
        {
            SequenceGenerator generator = new SequenceGenerator(10000);

            ICoordinateSequence<Coordinate> seq = generator.Sequence;

            seq.Sort();

            for (Int32 i = 1; i < seq.Count; i++)
            {
                Assert.IsTrue(seq[i].CompareTo(seq[i - 1]) >= 0);
            }
        }

        [Test]
        public void SpliceIsCorrect()
        {
            SequenceGenerator generator = new SequenceGenerator(10000);
            CoordinateFactory coordFactory = generator.CoordinateFactory;
            ICoordinateSequence<Coordinate> seq = generator.Sequence;

            List<Coordinate> coordsToAdd
                = new List<Coordinate>(generator.GenerateCoordinates(100));

            // Prepend enumeration
            ICoordinateSequence<Coordinate> splice = seq.Splice(coordsToAdd, 5000, 5099);

            Assert.AreEqual(200, splice.Count);

            for (Int32 i = 0; i < splice.Count; i++)
            {
                if (i >= 100)
                {
                    Assert.AreEqual(seq[4900 + i], splice[i]);
                }
                else
                {
                    Assert.AreEqual(coordsToAdd[i], splice[i]);
                }
            }

            // Append enumeration
            splice = seq.Splice(9900, 9999, coordsToAdd);

            Assert.AreEqual(200, splice.Count);

            for (Int32 i = 0; i < splice.Count; i++)
            {
                if (i >= 100)
                {
                    Assert.AreEqual(coordsToAdd[i - 100], splice[i]);
                }
                else
                {
                    Assert.AreEqual(seq[9900 + i], splice[i]);
                }
            }

            // Prepend single
            splice = seq.Splice(coordFactory.Create(-1, -1), 0, 99);

            Assert.AreEqual(101, splice.Count);

            for (Int32 i = 0; i < splice.Count; i++)
            {
                if (i == 0)
                {
                    Assert.AreEqual(coordFactory.Create(-1, -1), splice[i]);
                }
                else
                {
                    Assert.AreEqual(seq[i - 1], splice[i]);
                }
            }

            // Append single
            splice = seq.Splice(1000, 1099, coordFactory.Create(-1, -1));

            Assert.AreEqual(101, splice.Count);

            for (Int32 i = 0; i < splice.Count; i++)
            {
                if (i >= 100)
                {
                    Assert.AreEqual(coordFactory.Create(-1, -1), splice[i]);
                }
                else
                {
                    Assert.AreEqual(seq[1000 + i], splice[i]);
                }
            }

            // Prepend single, append enumeration
            splice = seq.Splice(coordFactory.Create(-1, -1), 8000, 8099, coordsToAdd);

            Assert.AreEqual(201, splice.Count);

            for (Int32 i = 0; i < splice.Count; i++)
            {
                if (i == 0)
                {
                    Assert.AreEqual(coordFactory.Create(-1, -1), splice[i]);
                }
                else if (i <= 100)
                {
                    Assert.AreEqual(seq[8000 + i - 1], splice[i]);
                }
                else
                {
                    Assert.AreEqual(coordsToAdd[i - 101], splice[i]);
                }
            }

            // Prepend enumeration, append single
            splice = seq.Splice(coordsToAdd, 0, 0, coordFactory.Create(-1, -1));

            Assert.AreEqual(102, splice.Count);

            for (Int32 i = 0; i < splice.Count; i++)
            {
                if (i < 100)
                {
                    Assert.AreEqual(coordsToAdd[i], splice[i]);
                }
                else if (i == 100)
                {
                    Assert.AreEqual(seq[i - 100], splice[i]);
                }
                else
                {
                    Assert.AreEqual(coordFactory.Create(-1, -1), splice[i]);
                }
            }

            // Prepend single, append single
            splice = seq.Splice(coordFactory.Create(-1, -1), 0, 9999, coordFactory.Create(-1, -1));

            Assert.AreEqual(10002, splice.Count);

            for (Int32 i = 0; i < splice.Count; i++)
            {
                if (i > 0 && i < 10001)
                {
                    Assert.AreEqual(seq[i - 1], splice[i]);
                }
                else
                {
                    Assert.AreEqual(coordFactory.Create(-1, -1), splice[i]);
                }
            }

            // Prepend enumeration, append enumeration
            splice = seq.Splice(coordsToAdd, 9999, 9999, coordsToAdd);

            Assert.AreEqual(201, splice.Count);

            for (Int32 i = 0; i < splice.Count; i++)
            {
                if (i < 100 || i > 100)
                {
                    Assert.AreEqual(coordsToAdd[i > 100 ? i - 101 : i], splice[i]);
                }
                else
                {
                    Assert.AreEqual(seq[9999], splice[i]);
                }
            }
        }

        [Test]
        public void WithoutDuplicatePointsCorrect()
        {
            SequenceGenerator generator = new SequenceGenerator(2, 10000);
            CoordinateFactory coordFactory = generator.CoordinateFactory;
            ICoordinateSequence<Coordinate> seq = generator.Sequence;

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

            ICoordinateSequence<Coordinate> filtered = seq.WithoutDuplicatePoints();

            Assert.AreEqual(4, filtered.Count);

            Assert.IsTrue(filtered.Contains(coordFactory.Create(1, 1)));
            Assert.IsTrue(filtered.Contains(coordFactory.Create(1, 2)));
            Assert.IsTrue(filtered.Contains(coordFactory.Create(2, 1)));
            Assert.IsTrue(filtered.Contains(coordFactory.Create(2, 2)));
        }

        [Test]
        public void WithoutRepeatedPointsCorrect()
        {
            SequenceGenerator generator = new SequenceGenerator();

            ICoordinateSequence<Coordinate> seq = generator.NewSequence(
                generator.GenerateCoordinates(100000, 2), false);

            Coordinate last = new Coordinate();

            foreach (Coordinate coordinate in seq)
            {
                Assert.AreNotEqual(last, coordinate);
                last = coordinate;
            }
        }

        [Test]
        public void ChangingSequenceElementDoesntAffectOtherSequencesWithTheSameCoordinate()
        {
            CoordinateSequenceFactory factory
                = new CoordinateSequenceFactory();

            ICoordinateSequence<Coordinate> seq1
                = factory.Create(CoordinateDimensions.Two);
            ICoordinateSequence<Coordinate> seq2
                = factory.Create(CoordinateDimensions.Two);

            ICoordinateFactory<Coordinate> coordFactory = factory.CoordinateFactory;

            Random rnd = new MersenneTwister();

            for (Int32 i = 0; i < 100; i++)
            {
                Coordinate coord = coordFactory.Create(rnd.NextDouble(),
                                                       rnd.NextDouble());
                seq1.Add(coord);
                seq2.Add(coord);
                Assert.IsTrue(seq1[i].Equals(seq2[i]));
            }

            Coordinate c = seq1[10];
            Double x = c.X;
            Double y = c.Y;

            seq1[10] = coordFactory.Create(1234, 1234);

            Assert.AreEqual(x, seq2[10][Ordinates.X]);
            Assert.AreEqual(y, seq2[10][Ordinates.Y]);
        }

        //private IEnumerable<Coordinate> generateCoords(Int32 count,
        //                                                         Int32 max,
        //                                                         CoordinateFactory coordFactory)
        //{
        //    while (count-- > 0)
        //    {
        //        yield return coordFactory.Create(_rnd.Next(1, max + 1),
        //                                          _rnd.Next(1, max + 1));
        //    }
        //}
    }
}