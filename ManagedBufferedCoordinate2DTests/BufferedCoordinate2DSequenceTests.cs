using System;
using System.Collections;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures.Collections.Generic;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NetTopologySuite.Coordinates;
using NPack;
using NUnit.Framework;
using Rhino.Mocks;

namespace ManagedBufferedCoordinate2DTests
{
    using IBufferedCoordFactory = ICoordinateFactory<BufferedCoordinate2D>;
    using IBufferedCoordSequence = ICoordinateSequence<BufferedCoordinate2D>;
    using IBufferedCoordSequenceFactory = ICoordinateSequenceFactory<BufferedCoordinate2D>;

    [TestFixture]
    public class BufferedCoordinate2DSequenceTests
    {
        // Tests to write:
        //  Indexer tests
        //      IList indexer yields same result as implicit indexer implementation
        //      IList indexer setter
        //  AppendPrependVariants
        //      Good subset of sequences/slices being appended/prepended to with removed coordinates
        //      Reverse and remove coordinates from complex slices being appended/prepended to sequences/slices

        private static readonly Int32 BigMaxLimit = Int32.MaxValue - 2;
        private readonly Random _rnd = new MersenneTwister();
        private delegate IEnumerator<BufferedCoordinate2D> BufferedCoordinate2DEnumeratorDelegate();

        [Test]
        public void CreatingCoordinateSequenceSucceeds()
        {
            BufferedCoordinate2DSequenceFactory factory
                = new BufferedCoordinate2DSequenceFactory();
            IBufferedCoordSequence seq = factory.Create(CoordinateDimensions.Two);
        }

        [Test]
        public void CreatingCoordinateSequenceWithSpecificSizeSucceeds()
        {
            BufferedCoordinate2DSequenceFactory factory
                = new BufferedCoordinate2DSequenceFactory();
            IBufferedCoordSequence seq = factory.Create(200, CoordinateDimensions.Two);

            Assert.AreEqual(200, seq.Count);
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void CreatingCoordinateSequenceWith3DCoordinateFails()
        {
            BufferedCoordinate2DSequenceFactory factory
                = new BufferedCoordinate2DSequenceFactory();
            IBufferedCoordSequence seq = factory.Create(CoordinateDimensions.Three);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CreatingCoordinateSequenceWithNegativeSpecificSizeFails()
        {
            BufferedCoordinate2DSequenceFactory factory
                = new BufferedCoordinate2DSequenceFactory();
            IBufferedCoordSequence seq = factory.Create(-1, CoordinateDimensions.Two);
        }

        [Test]
        public void CreatingCoordinateSequenceWithAnEnumerationOfCoordinatesSucceeds()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory factory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq
                = factory.Create(generateCoords(9999, BigMaxLimit, coordFactory));

            Assert.AreEqual(9999, seq.Count);
        }

        [Test]
        public void CreatingCoordinateSequenceWithoutRepeatedCoordinatesSucceeds()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory factory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            List<BufferedCoordinate2D> coords
                = new List<BufferedCoordinate2D>(generateCoords(100, 1, coordFactory));

            Assert.AreEqual(100, coords.Count);

            IBufferedCoordSequence seq1 = factory.Create(coords, true);

            for (Int32 i = 0; i < 100; i++)
            {
                Assert.AreEqual(seq1[i], coords[i]);
            }

            Assert.AreEqual(100, seq1.Count);

            IBufferedCoordSequence seq2
                = factory.Create(coords, false);

            Assert.AreEqual(1, seq2.Count);
        }

        [Test]
        public void CreatingSequenceAsUniqueSucceeds()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory factory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            List<BufferedCoordinate2D> coords
                = new List<BufferedCoordinate2D>(generateCoords(1000, 5, coordFactory));

            IBufferedCoordSequence seq = factory.Create(coords, false);

            seq = seq.WithoutDuplicatePoints();

            CollectionAssert.AllItemsAreUnique(seq);
        }

        [Test]
        public void SequenceToArraySucceeds()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory factory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = factory.Create(generateCoords(1000, 500000, coordFactory));

            BufferedCoordinate2D[] coordsArray = seq.ToArray();

            Assert.AreEqual(seq.Count, coordsArray.Length);

            for (Int32 i = 0; i < seq.Count; i++)
            {
                Assert.AreEqual(seq[i], coordsArray[i]);
            }
        }

        [Test]
        public void AddingABufferedCoordinate2DFromVariousFactoriesSucceeds()
        {
            BufferedCoordinate2DFactory coordFactory1
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DFactory coordFactory2
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory1);

            IBufferedCoordSequence seq1 = seqFactory.Create(CoordinateDimensions.Two);

            seq1.Add(coordFactory1.Create(10, 20));
            seq1.Add(coordFactory1.Create(20, 30));

            seq1.Add(coordFactory2.Create(20, 30));
            seq1.Add(coordFactory2.Create(30, 40));

            Assert.AreEqual(3, coordFactory1.VectorBuffer.Count);
            Assert.AreEqual(2, coordFactory2.VectorBuffer.Count);

            Assert.AreEqual(4, seq1.Count);

            Assert.IsTrue(seq1.HasRepeatedCoordinates);
        }

        [Test]
        public void AddingARangeOfBufferedCoordinate2DInstancesSucceeds()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory factory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = factory.Create(CoordinateDimensions.Two);

            List<BufferedCoordinate2D> coordsToAdd
                = new List<BufferedCoordinate2D>(generateCoords(1000, 500000, coordFactory));

            seq.AddRange(coordsToAdd);

            Assert.AreEqual(coordsToAdd.Count, seq.Count);

            for (Int32 i = 0; i < coordsToAdd.Count; i++)
            {
                Assert.AreEqual(coordsToAdd[i], seq[i]);
            }
        }

        [Test]
        public void AddingARangeOfBufferedCoordinate2DInstancesWithoutRepeatsInReverseSucceeds()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            List<BufferedCoordinate2D> coordsToAdd = new List<BufferedCoordinate2D>();
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
        public void AddingABufferedCoordinate2DSequenceFromTheSameFactorySucceeds()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

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

            Assert.AreEqual(seq1.Count, seq2.Count);

            for (Int32 i = 0; i < seq1.Count; i++)
            {
                Assert.IsTrue(seq1[i].Equals(seq2[i]));
            }
        }

        [Test]
        public void AddingABufferedCoordinate2DSequenceFromADifferentFactorySucceeds()
        {
            BufferedCoordinate2DFactory coordFactory1
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory1
                = new BufferedCoordinate2DSequenceFactory(coordFactory1);

            IBufferedCoordSequence seq1
                = seqFactory1.Create(CoordinateDimensions.Two);

            seq1.Add(coordFactory1.Create(10, 20));
            seq1.Add(coordFactory1.Create(11, 21));
            seq1.Add(coordFactory1.Create(22, 32));
            seq1.Add(coordFactory1.Create(23, 33));
            seq1.Add(coordFactory1.Create(34, 44));
            seq1.Add(coordFactory1.Create(35, 45));

            BufferedCoordinate2DSequenceFactory seqFactory2
                = new BufferedCoordinate2DSequenceFactory();

            IBufferedCoordSequence seq2
                = seqFactory2.Create(CoordinateDimensions.Two);

            seq2.AddSequence(seq1);

            Assert.AreEqual(seq1.Count, seq2.Count);

            for (Int32 i = 0; i < seq1.Count; i++)
            {
                Assert.IsFalse(seq1[i].Equals(seq2[i]));
                Assert.IsTrue(seq1[i].ValueEquals(seq2[i]));
            }
        }

        [Test]
        public void ReturningASetFromAsSetSucceeds()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq
                = seqFactory.Create(generateCoords(1000, 5000, coordFactory));

            ISet<BufferedCoordinate2D> set = seq.AsSet();

            Assert.IsNotNull(set);
        }

        [Test]
        public void CloneSucceeds()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory factory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq1
                = factory.Create(generateCoords(1000, 5000, coordFactory));

            IBufferedCoordSequence seq2 = seq1.Clone();

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
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

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
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            // Create a ring which is closed by definition
            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

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

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ClosingARingOnASequenceWithFewerThan3PointsFails()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 0));
            seq.Add(coordFactory.Create(0, 1));

            seq.CloseRing();
        }

        [Test]
        public void ClosingARingOnASlicedSequenceSucceeds()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

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

            Assert.AreEqual(5, slice1.Count);
            Assert.AreEqual(slice1.First, slice1.Last);

            slice1.CloseRing();

            Assert.AreEqual(5, slice1.Count);
            Assert.AreEqual(slice1.First, slice1.Last);

            // This sequence is not a ring
            IBufferedCoordSequence slice2 = seq.Slice(2, 5);

            Assert.AreEqual(4, slice2.Count);
            Assert.AreNotEqual(slice2.First, slice2.Last);

            slice2.CloseRing();

            Assert.AreEqual(5, slice2.Count);
            Assert.AreEqual(slice2.First, slice2.Last);
        }

        [Test]
        public void ContainsABufferedCoordinate2DFromTheSameBufferSucceeds()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            List<BufferedCoordinate2D> coordsToAdd
                = new List<BufferedCoordinate2D>
                    (generateCoords(1000, 500000, coordFactory));

            IBufferedCoordSequence seq = seqFactory.Create(coordsToAdd);

            foreach (BufferedCoordinate2D coordinate2D in coordsToAdd)
            {
                Assert.IsTrue(seq.Contains(coordinate2D));
            }
        }

        [Test]
        public void ContainsABufferedCoordinate2DFromADifferentBufferFails()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            // these coordinates come from a different buffer
            List<BufferedCoordinate2D> coordsToAdd
                = new List<BufferedCoordinate2D>(generateCoords(1000, 500000, coordFactory));

            IBufferedCoordSequence seq = seqFactory.Create(coordsToAdd);

            foreach (BufferedCoordinate2D coordinate2D in coordsToAdd)
            {
                Assert.IsFalse(seq.Contains(coordinate2D));
            }
        }

        [Test]
        public void CompareToComputesLexicographicOrderingCorrectly()
        {
            BufferedCoordinate2DFactory coordFactory1
                = new BufferedCoordinate2DFactory();
            BufferedCoordinate2DSequenceFactory seqFactory1
                = new BufferedCoordinate2DSequenceFactory(coordFactory1);

            BufferedCoordinate2DFactory coordFactory2
                = new BufferedCoordinate2DFactory();
            BufferedCoordinate2DSequenceFactory seqFactory2
                = new BufferedCoordinate2DSequenceFactory(coordFactory2);

            IBufferedCoordSequence seq1 = seqFactory1.Create(CoordinateDimensions.Two);
            seq1.Add(coordFactory1.Create(1, 2));
            seq1.Add(coordFactory1.Create(3, 4));
            seq1.Add(coordFactory1.Create(5, 6));

            IBufferedCoordSequence seq2 = seqFactory2.Create(CoordinateDimensions.Two);
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

            Assert.AreEqual(1, seq1.CompareTo(seq2));
        }

        [Test]
        public void CopyToArraySucceeds()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(1000, 200, coordFactory));

            BufferedCoordinate2D[] coordArray = new BufferedCoordinate2D[2000];
            seq.CopyTo(coordArray, 0);

            for (Int32 i = 0; i < seq.Count; i++)
            {
                Assert.AreEqual(seq[i], coordArray[i]);
            }

            seq.CopyTo(coordArray, 1000);

            Int32 end = coordArray.Length - 1;

            for (Int32 i = 0; i < seq.Count; i++)
            {
                Assert.AreEqual(seq[i], coordArray[i + 1000]);
            }
        }

        [Test]
        public void CountIsCorrectOnCreation()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(1000, 200, coordFactory));

            Assert.AreEqual(1000, seq.Count);
        }

        [Test]
        public void CountIsCorrectAfterAddOperations()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(1000, 100, coordFactory));

            seq.Add(coordFactory.Create(789, 456));

            Assert.AreEqual(1001, seq.Count);

            seq.AddRange(generateCoords(100, 100, coordFactory));

            Assert.AreEqual(1101, seq.Count);
        }

        [Test]
        public void CountIsCorrectAfterRemoveOperations()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(1000, 100, coordFactory));

            Boolean didRemove = seq.Remove(coordFactory.Create(-1, -1));
            Assert.IsFalse(didRemove);
            Assert.AreEqual(1000, seq.Count);

            BufferedCoordinate2D coord = seq[4];
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
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(1000, 100, coordFactory));

            IBufferedCoordSequence slice = seq.Slice(0, 9);

            Assert.AreEqual(10, slice.Count);

            slice = seq.Slice(990, 999);

            Assert.AreEqual(10, slice.Count);
        }

        [Test]
        public void CountIsCorrectAfterSpliceOperation()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(1000, 100, coordFactory));

            IBufferedCoordSequence splice = seq.Splice(coordFactory.Create(-1, -1), 0, 9);

            Assert.AreEqual(11, splice.Count);

            splice = seq.Splice(generateCoords(10, 100, coordFactory), 990, 999);

            Assert.AreEqual(20, splice.Count);

            splice = seq.Splice(generateCoords(10, 100, coordFactory), 990, 999, coordFactory.Create(0, 0));

            Assert.AreEqual(21, splice.Count);

            splice = seq.Splice(generateCoords(10, 100, coordFactory), 990, 999, generateCoords(10, 100, coordFactory));

            Assert.AreEqual(30, splice.Count);

            splice = seq.Splice(coordFactory.Create(0, 0), 990, 999, coordFactory.Create(0, 0));

            Assert.AreEqual(12, splice.Count);

            splice = seq.Splice(990, 999, coordFactory.Create(0, 0));

            Assert.AreEqual(11, splice.Count);

            splice = seq.Splice(990, 999, generateCoords(10, 100, coordFactory));

            Assert.AreEqual(20, splice.Count);
        }

        [Test]
        public void EqualsTestsEqualityCorrectly()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            List<BufferedCoordinate2D> coordsToAdd
                = new List<BufferedCoordinate2D>(generateCoords(1000, 1000, coordFactory));

            IBufferedCoordSequence seq1 = seqFactory.Create(coordsToAdd);
            IBufferedCoordSequence seq2 = seqFactory.Create(coordsToAdd);

            Assert.IsTrue(seq1.Equals(seq2));
        }


        [Test]
        public void EnumeratorBasicOperationSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 10);

            Assert.AreEqual(generator.MainList.Count, generator.Sequence.Count);

            IEnumerator<BufferedCoordinate2D> enumerator = generator.Sequence.GetEnumerator();
            foreach (BufferedCoordinate2D coordinate in generator.MainList)
            {
                Assert.IsTrue(enumerator.MoveNext());
                Assert.AreEqual(coordinate, enumerator.Current);
            }
            Assert.IsFalse(enumerator.MoveNext());
        }

        [Test]
        public void EnumeratorOnReversedSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 10);

            generator.Sequence.Reverse();

            IEnumerator<BufferedCoordinate2D> enumerator = generator.Sequence.GetEnumerator();

            for (Int32 i = 0; i < generator.Sequence.Count; i++)
            {
                Assert.IsTrue(enumerator.MoveNext());
                BufferedCoordinate2D expected 
                    = generator.MainList[generator.MainList.Count - i - 1];
                Assert.AreEqual(expected, enumerator.Current);
            }

            Assert.IsFalse(enumerator.MoveNext());
        }


        [Test]
        public void EnumeratorOnSliceIsCorrect()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 10);

            IBufferedCoordSequence slice = generator.Sequence.Slice(5, 9);
                
            IEnumerator<BufferedCoordinate2D> enumerator = slice.GetEnumerator();

            for (Int32 i = 0; i < 5; i++)
            {
                Assert.IsTrue(enumerator.MoveNext());
                Assert.AreEqual(generator.Sequence[i + 5], enumerator.Current);
            }

            Assert.IsFalse(enumerator.MoveNext());

            IBufferedCoordSequence slice2 = slice.Slice(2, 4);

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
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 2, 0);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 4);

            slice.Prepend(generator.PrependList);

            Assert.AreEqual(generator.MainList.Count + generator.PrependList.Count, slice.Count);

            IEnumerator<BufferedCoordinate2D> enumerator = slice.GetEnumerator();

            foreach (BufferedCoordinate2D expected in generator.PrependList)
            {
                Assert.IsTrue(enumerator.MoveNext());
                Assert.AreEqual(expected, enumerator.Current);
            }

            foreach (BufferedCoordinate2D expected in generator.MainList)
            {
                Assert.IsTrue(enumerator.MoveNext());
                Assert.AreEqual(expected, enumerator.Current);
            }

            Assert.IsFalse(enumerator.MoveNext());
        }


        [Test]
        public void EnumeratorWithAppendedList()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 0, 2);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 4);

            slice.Append(generator.AppendList);

            Assert.AreEqual(generator.MainList.Count + generator.AppendList.Count, slice.Count);

            IEnumerator<BufferedCoordinate2D> enumerator = slice.GetEnumerator();

            foreach (BufferedCoordinate2D expected in generator.MainList)
            {
                Assert.IsTrue(enumerator.MoveNext());
                Assert.AreEqual(expected, enumerator.Current);
            }

            foreach (BufferedCoordinate2D expected in generator.AppendList)
            {
                Assert.IsTrue(enumerator.MoveNext());
                Assert.AreEqual(expected, enumerator.Current);
            }

            Assert.IsFalse(enumerator.MoveNext());
        }


        [Test]
        public void EnumeratorWithSkippedIndexList()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 4);

            slice.Remove(generator.MainList[1]);
            slice.Remove(generator.MainList[3]);

            Assert.AreEqual(generator.MainList.Count - 2, slice.Count);

            IEnumerator<BufferedCoordinate2D> enumerator = slice.GetEnumerator();

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
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 2, 2);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 4);
            slice.Remove(generator.MainList[1]);
            slice.Remove(generator.MainList[3]);
            slice.Prepend(generator.PrependList);
            slice.Append(generator.AppendList);
            slice.Reverse();

            Int32 expectedCount = generator.MainList.Count - 2 + 
                                  generator.PrependList.Count +
                                  generator.AppendList.Count;

            Assert.AreEqual(expectedCount, slice.Count);

            IEnumerator<BufferedCoordinate2D> enumerator = slice.GetEnumerator();

            for (Int32 i = generator.AppendList.Count - 1; i >= 0; i--)
            {
                Assert.IsTrue(enumerator.MoveNext());
                BufferedCoordinate2D expectedCoord = generator.AppendList[i];
                Assert.AreEqual(expectedCoord, enumerator.Current);
            }

            for (Int32 i = generator.MainList.Count - 1; i >= 0; i--)
            {
                if (i != 1 && i != 3)
                {
                    Assert.IsTrue(enumerator.MoveNext());
                    BufferedCoordinate2D expectedCoord = generator.MainList[i];
                    Assert.AreEqual(expectedCoord, enumerator.Current);
                }
            }

            for (Int32 i = generator.PrependList.Count - 1; i >= 0; i--)
            {
                Assert.IsTrue(enumerator.MoveNext());
                BufferedCoordinate2D expectedCoord = generator.PrependList[i];
                Assert.AreEqual(expectedCoord, enumerator.Current);
            }

            Assert.IsFalse(enumerator.MoveNext());
        }

        [Test]
        public void ExpandExtentsSucceeds()
        {
            BufferedCoordinate2DFactory coordinateFactory = new BufferedCoordinate2DFactory();
            BufferedCoordinate2DSequenceFactory sequenceFactory = new BufferedCoordinate2DSequenceFactory(coordinateFactory);
            GeometryFactory<BufferedCoordinate2D> geometryFactory = new GeometryFactory<BufferedCoordinate2D>(sequenceFactory);

            IBufferedCoordSequence sequence = sequenceFactory.Create(CoordinateDimensions.Two);
            sequence.Add(coordinateFactory.Create(1, 15));
            sequence.Add(coordinateFactory.Create(15, 1));

            IExtents<BufferedCoordinate2D> extents = sequence.ExpandExtents(geometryFactory.CreateExtents());

            Assert.AreEqual(1, extents.Min.X);
            Assert.AreEqual(1, extents.Min.Y);
            Assert.AreEqual(15, extents.Max.X);
            Assert.AreEqual(15, extents.Max.Y);
        }

        [Test]
        public void FirstReturnsTheFirstCoordinate()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 0));

            Assert.AreEqual(seq.First, seq[0]);
        }

        [Test]
        public void GetEnumeratorSucceeds()
        {
            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory();

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            IEnumerator<BufferedCoordinate2D> enumerator = seq.GetEnumerator();

            Assert.IsNotNull(enumerator);
        }

        [Test]
        public void HasRepeatedCoordinatesSucceeds()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

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
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

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
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(10, BigMaxLimit, coordFactory));

            BufferedCoordinate2D coord;

            for (Int32 i = 0; i < 10; i++)
            {
                coord = seq[i];
                Assert.AreEqual(i, seq.IndexOf(coord));
            }

            coord = coordFactory.Create(Int32.MaxValue, Int32.MaxValue);
            Assert.AreEqual(-1, seq.IndexOf(coord));

            coord = seq[0];
            seq.Clear();

            Assert.AreEqual(-1, seq.IndexOf(coord));
        }

        [Test]
        public void IListIndexerYieldsSameResultAsIndexer()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 10);

            Assert.AreEqual(generator.MainList.Count, generator.Sequence.Count);

            for (Int32 i = 0; i < generator.Sequence.Count; i++)
            {
                BufferedCoordinate2D implicitResult = generator.Sequence[i];
                Assert.AreEqual(generator.MainList[i], implicitResult);
                Object iListResult = (generator.Sequence as IList)[i];
                Assert.IsInstanceOfType(typeof(BufferedCoordinate2D), iListResult);
                Assert.AreEqual(implicitResult, (BufferedCoordinate2D)iListResult);
            }
        }

        [Test]
        public void IndexerSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 10);

            Assert.AreEqual(generator.MainList.Count, generator.Sequence.Count);

            for (Int32 i = 0; i < generator.Sequence.Count; i++)
            {
                Assert.AreEqual(generator.MainList[i], generator.Sequence[i]);
            }
        }

        [Test]
        public void IndexerOnReversedSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 10);

            generator.Sequence.Reverse();

            Assert.AreEqual(generator.MainList.Count, generator.Sequence.Count);

            for (Int32 i = 0; i < generator.Sequence.Count; i++)
            {
                Assert.AreEqual(generator.MainList[generator.MainList.Count - i - 1], generator.Sequence[i]);
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CallingIndexerWithNegativeNumberFails()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 10);

            BufferedCoordinate2D coord = generator.Sequence[-1];
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CallingIndexerWithNumberEqualToCountFails()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 10);

            BufferedCoordinate2D coord = generator.Sequence[generator.Sequence.Count];
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CallingIndexerWithNumberGreaterThanCountFails()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 10);

            BufferedCoordinate2D coord = generator.Sequence[Int32.MaxValue];
        }


        [Test]
        public void IndexerOnSliceIsCorrect()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 10);

            IBufferedCoordSequence slice = generator.Sequence.Slice(5, 9);

            for (Int32 i = 0; i < 5; i++)
            {
                Assert.AreEqual(generator.Sequence[i + 5], slice[i]);
            }

            IBufferedCoordSequence slice2 = slice.Slice(2, 4);

            for (Int32 i = 0; i < 3; i++)
            {
                Assert.AreEqual(slice[i + 2], slice2[i]);
            }
        }

        [Test]
        public void IndexerWithPrependedList()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 2, 0);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 4);
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
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 0, 2);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 4);
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
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 4);
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
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 2, 2);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 4);
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
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create();

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
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create();

            Int32 count = 0;

            IEnumerable<BufferedCoordinate2D> coords 
                = generateCoords(10, BigMaxLimit, coordFactory);

            foreach (BufferedCoordinate2D expected in coords)
            {
                Int32 index = count % 2 == 0 ? 0 : count - 1;
                seq.Insert(index, expected);
                count++;
                BufferedCoordinate2D actual = seq[index];
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void IsFixedSizeIsCorrect()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(10, BigMaxLimit, coordFactory));

            Assert.IsFalse(seq.IsFixedSize);

            seq.Freeze();

            Assert.IsTrue(seq.IsFixedSize);
        }

        [Test]
        public void IsReadOnlyIsCorrect()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create();

            Assert.IsFalse(seq.IsReadOnly);

            seq.Freeze();

            Assert.IsTrue(seq.IsReadOnly);
        }

        [Test]
        public void IsFrozenIsCorrect()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(10, BigMaxLimit, coordFactory));

            Assert.IsFalse(seq.IsFrozen);

            seq.Freeze();

            Assert.IsTrue(seq.IsFrozen);
        }

        [Test]
        public void LastIsTheLastCoordinate()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(10, BigMaxLimit, coordFactory));

            Assert.AreEqual(seq[seq.Count - 1], seq.Last);

            seq = seqFactory.Create();

            Assert.AreEqual(0, seq.Count);
            Assert.AreEqual(new BufferedCoordinate2D(), seq.Last);
        }

        [Test]
        public void LastIndexIsCorrect()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(10, BigMaxLimit, coordFactory));

            Assert.AreEqual(seq.Count - 1, seq.LastIndex);

            seq = seqFactory.Create();

            Assert.AreEqual(0, seq.Count);
            Assert.AreEqual(-1, seq.LastIndex);
        }


        [Test]
        public void MaximumIsCorrectSingleItemSequence()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 0));

            Assert.AreEqual(coordFactory.Create(0, 0), seq.Maximum);
        }

        [Test]
        public void MaximumIsCorrectFirstInMultiItemSequence()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(0, 0));
            seq.Add(coordFactory.Create(0, 1));

            Assert.AreEqual(coordFactory.Create(1, 1), seq.Maximum);
        }

        [Test]
        public void MaximumIsCorrectLastInMultiItemSequence()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 1));
            seq.Add(coordFactory.Create(0, 0));
            seq.Add(coordFactory.Create(1, 1));

            Assert.AreEqual(coordFactory.Create(1, 1), seq.Maximum);
        }

        [Test]
        public void MaximumIsCorrectMiddleOfMultiItemSequence()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 1));
            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(0, 0));

            Assert.AreEqual(coordFactory.Create(1, 1), seq.Maximum);
        }

        [Test]
        public void MaximumIsCorrectAfterMaxInSequenceChanges()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

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
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create();

            Assert.AreEqual(new BufferedCoordinate2D(), seq.Maximum);

            seq.Add(coordFactory.Create(0, 0));
        }


        [Test]
        public void MinimumIsCorrectSingleItemSequence()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 0));

            Assert.AreEqual(coordFactory.Create(0, 0), seq.Minimum);
        }

        [Test]
        public void MinimumIsCorrectFirstInMultiItemSequence()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 0));
            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(0, 1));

            Assert.AreEqual(coordFactory.Create(0, 0), seq.Minimum);
        }

        [Test]
        public void MinimumIsCorrectLastInMultiItemSequence()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 1));
            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(0, 0));

            Assert.AreEqual(coordFactory.Create(0, 0), seq.Minimum);
        }

        [Test]
        public void MinimumIsCorrectMiddleOfMultiItemSequence()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

            seq.Add(coordFactory.Create(0, 1));
            seq.Add(coordFactory.Create(0, 0));
            seq.Add(coordFactory.Create(1, 1));

            Assert.AreEqual(coordFactory.Create(0, 0), seq.Minimum);
        }

        [Test]
        public void MinimumIsCorrectAfterMinInSequenceChanges()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(CoordinateDimensions.Two);

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
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create();

            Assert.AreEqual(new BufferedCoordinate2D(), seq.Minimum);
        }

        [Test]
        public void MergeSucceeds()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq1 = seqFactory.Create();
            IBufferedCoordSequence seq2 = seqFactory.Create();

            IBufferedCoordSequence merged = seq1.Merge(seq2);
            Assert.Fail("Need to complete");
        }

        [Test]
        public void PrependCoordinateToNewSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5);

            BufferedCoordinate2D coord = generator.RandomCoordinate(BigMaxLimit);

            generator.Sequence.Prepend(coord);

            Assert.AreEqual(coord, generator.Sequence[0]);
            Assert.AreEqual(generator.MainList[0], generator.Sequence[1]);
        }

        [Test]
        public void PrependCoordinateToNewSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, 3);

            BufferedCoordinate2D coord = generator.RandomCoordinate(BigMaxLimit);

            slice.Prepend(coord);

            Assert.AreEqual(coord, slice[0]);
            Assert.AreEqual(generator.MainList[1], slice[1]);
        }

        [Test]
        public void PrependCoordinateToPrependedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5);

            BufferedCoordinate2D coord1 = generator.RandomCoordinate(BigMaxLimit);
            BufferedCoordinate2D coord0 = generator.RandomCoordinate(BigMaxLimit);

            generator.Sequence.Prepend(coord1);

            Assert.AreEqual(coord1, generator.Sequence[0]);
            // prepend pushes back the indexes, so the first index in MainList will 
            // be the second in Sequence
            Assert.AreEqual(generator.MainList[0], generator.Sequence[1]);

            generator.Sequence.Prepend(coord0);

            Assert.AreEqual(coord0, generator.Sequence[0]);
            Assert.AreEqual(coord1, generator.Sequence[1]);
            Assert.AreEqual(generator.MainList[0], generator.Sequence[2]);
        }

        [Test]
        public void PrependCoordinateToPrependedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, 3);

            BufferedCoordinate2D coord1 = generator.RandomCoordinate(BigMaxLimit);
            BufferedCoordinate2D coord0 = generator.RandomCoordinate(BigMaxLimit);

            slice.Prepend(coord1);

            Assert.AreEqual(coord1, slice[0]);
            Assert.AreEqual(generator.MainList[1], slice[1]);

            slice.Prepend(coord0);

            Assert.AreEqual(coord0, slice[0]);
            Assert.AreEqual(coord1, slice[1]);
            Assert.AreEqual(generator.MainList[1], slice[2]);
        }

        [Test]
        public void PrependEnumerationToNewSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);

            BufferedCoordinate2DEnumeratorDelegate enumeratorDelegate
                = delegate { return generator.PrependList.GetEnumerator(); };

            MockRepository mocks = new MockRepository();
            IEnumerable<BufferedCoordinate2D> prependList
                = mocks.CreateMock<IEnumerable<BufferedCoordinate2D>>();
            Expect.Call(prependList.GetEnumerator())
                .Repeat.Any()
                .Do(enumeratorDelegate);
            mocks.ReplayAll();

            generator.Sequence.Prepend(prependList);

            Int32 i = 0;
            for (; i < generator.PrependList.Count; i++)
            {
                Assert.AreEqual(generator.PrependList[i], generator.Sequence[i]);
            }
            Assert.AreEqual(generator.MainList[0], generator.Sequence[i]);

            mocks.VerifyAll();
        }

        [Test]
        public void PrependEnumerationToNewSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, 3);

            BufferedCoordinate2DEnumeratorDelegate enumeratorDelegate
                = delegate { return generator.PrependList.GetEnumerator(); };

            MockRepository mocks = new MockRepository();
            IEnumerable<BufferedCoordinate2D> prependList
                = mocks.CreateMock<IEnumerable<BufferedCoordinate2D>>();
            Expect.Call(prependList.GetEnumerator())
                .Repeat.Any()
                .Do(enumeratorDelegate);
            mocks.ReplayAll();

            slice.Prepend(prependList);

            Int32 i = 0;
            for (; i < generator.PrependList.Count; i++)
            {
                Assert.AreEqual(generator.PrependList[i], slice[i]);
            }
            Assert.AreEqual(generator.MainList[1], slice[i]);

            mocks.VerifyAll();
        }

        [Test]
        public void PrependEnumerationToPrependedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);

            BufferedCoordinate2D prependedCoordinate = generator.CoordinateFactory.Create(1, 1);
            generator.Sequence.Prepend(prependedCoordinate);

            BufferedCoordinate2DEnumeratorDelegate enumeratorDelegate
                = delegate { return generator.PrependList.GetEnumerator(); };

            MockRepository mocks = new MockRepository();
            IEnumerable<BufferedCoordinate2D> prependList
                = mocks.CreateMock<IEnumerable<BufferedCoordinate2D>>();
            Expect.Call(prependList.GetEnumerator())
                .Repeat.Any()
                .Do(enumeratorDelegate);
            mocks.ReplayAll();

            generator.Sequence.Prepend(prependList);

            Int32 i = 0;
            for (; i < generator.PrependList.Count; i++)
            {
                Assert.AreEqual(generator.PrependList[i], generator.Sequence[i]);
            }
            Assert.AreEqual(prependedCoordinate, generator.Sequence[i]);
            Assert.AreEqual(generator.MainList[0], generator.Sequence[i + 1]);

            mocks.VerifyAll();
        }

        [Test]
        public void PrependEnumerationToPrependedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, 3);

            BufferedCoordinate2D prependedCoordinate = generator.CoordinateFactory.Create(1, 1);
            slice.Prepend(prependedCoordinate);

            BufferedCoordinate2DEnumeratorDelegate enumeratorDelegate
                = delegate { return generator.PrependList.GetEnumerator(); };

            MockRepository mocks = new MockRepository();
            IEnumerable<BufferedCoordinate2D> prependList
                = mocks.CreateMock<IEnumerable<BufferedCoordinate2D>>();
            Expect.Call(prependList.GetEnumerator())
                .Repeat.Any()
                .Do(enumeratorDelegate);
            mocks.ReplayAll();

            slice.Prepend(prependList);

            Int32 i = 0;
            for (; i < generator.PrependList.Count; i++)
            {
                Assert.AreEqual(generator.PrependList[i], slice[i]);
            }
            Assert.AreEqual(prependedCoordinate, slice[i]);
            Assert.AreEqual(generator.MainList[1], slice[i + 1]);

            mocks.VerifyAll();
        }

        [Test]
        public void PrependSequenceToNewSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            ICoordinateSequence<BufferedCoordinate2D> prependSeq 
                = generator.SequenceFactory.Create(generator.PrependList);

            generator.Sequence.Prepend(prependSeq);

            Int32 i = 0;
            BufferedCoordinate2D expected;
            BufferedCoordinate2D actual;

            for (; i < generator.PrependList.Count; i++)
            {
                expected = generator.PrependList[i];
                actual = generator.Sequence[i];
                Assert.AreEqual(expected, actual);
            }

            expected = generator.MainList[0];
            actual = generator.Sequence[i];
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void PrependSequenceToNewSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, 3);

            ICoordinateSequence<BufferedCoordinate2D> prependSeq = generator.SequenceFactory.Create(generator.PrependList);

            slice.Prepend(prependSeq);

            Int32 i = 0;
            for (; i < generator.PrependList.Count; i++)
            {
                Assert.AreEqual(generator.PrependList[i], slice[i]);
            }
            Assert.AreEqual(generator.MainList[1], slice[i]);
        }

        [Test]
        public void PrependSequenceToPrependedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);

            BufferedCoordinate2D prependedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            generator.Sequence.Prepend(prependedCoordinate);

            ICoordinateSequence<BufferedCoordinate2D> prependSeq 
                = generator.SequenceFactory.Create(generator.PrependList);

            generator.Sequence.Prepend(prependSeq);

            Int32 i = 0;

            for (; i < generator.PrependList.Count; i++)
            {
                Assert.AreEqual(generator.PrependList[i], generator.Sequence[i]);
            }

            Assert.AreEqual(prependedCoordinate, generator.Sequence[i]);
            Assert.AreEqual(generator.MainList[0], generator.Sequence[i + 1]);
        }

        [Test]
        public void PrependSequenceToPrependedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, 3);

            BufferedCoordinate2D prependedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            slice.Prepend(prependedCoordinate);

            ICoordinateSequence<BufferedCoordinate2D> prependSeq = generator.SequenceFactory.Create(generator.PrependList);

            slice.Prepend(prependSeq);

            Int32 i = 0;
            for (; i < generator.PrependList.Count; i++)
            {
                Assert.AreEqual(generator.PrependList[i], slice[i]);
            }
            Assert.AreEqual(prependedCoordinate, slice[i]);
            Assert.AreEqual(generator.MainList[1], slice[i + 1]);
        }

        [Test]
        public void PrependComplexSliceToPrependedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);

            BufferedCoordinate2D prependedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            generator.Sequence.Prepend(prependedCoordinate);

            IBufferedCoordSequence prependSlice
                = generator.SequenceFactory.Create(generator.PrependList)
                .Slice(0, 2);
            BufferedCoordinate2D preSliceCoordinate = generator.RandomCoordinate(BigMaxLimit);
            BufferedCoordinate2D postSliceCoordinate = generator.RandomCoordinate(BigMaxLimit);
            prependSlice.Prepend(preSliceCoordinate);
            prependSlice.Append(postSliceCoordinate);

            generator.Sequence.Prepend(prependSlice);

            Assert.AreEqual(preSliceCoordinate, generator.Sequence[0]);
            Int32 i = 1;
            for (; i <= generator.PrependList.Count; i++)
            {
                Assert.AreEqual(generator.PrependList[i - 1], generator.Sequence[i]);
            }
            Assert.AreEqual(postSliceCoordinate, generator.Sequence[i]);
            Assert.AreEqual(prependedCoordinate, generator.Sequence[i + 1]);
            Assert.AreEqual(generator.MainList[0], generator.Sequence[i + 2]);
        }

        [Test]
        public void PrependComplexSliceToPrependedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            IBufferedCoordSequence target = generator.Sequence.Slice(1, 3);

            BufferedCoordinate2D prependedCoordinate 
                = generator.RandomCoordinate(BigMaxLimit);

            target.Prepend(prependedCoordinate);

            IBufferedCoordSequence prependSlice = generator.SequenceFactory
                                                    .Create(generator.PrependList)
                                                    .Slice(0, 2);

            BufferedCoordinate2D preSliceCoordinate = generator.RandomCoordinate(BigMaxLimit);
            BufferedCoordinate2D postSliceCoordinate = generator.RandomCoordinate(BigMaxLimit);
            prependSlice.Prepend(preSliceCoordinate);
            prependSlice.Append(postSliceCoordinate);

            Assert.AreEqual(preSliceCoordinate, prependSlice.First);
            Assert.AreEqual(postSliceCoordinate, prependSlice.Last);

            target.Prepend(prependSlice);

            Assert.AreEqual(preSliceCoordinate, target[0]);

            Int32 i = 1;

            for (; i <= generator.PrependList.Count; i++)
            {
                Assert.AreEqual(generator.PrependList[i - 1], target[i]);
            }

            Assert.AreEqual(postSliceCoordinate, target[i]);
            Assert.AreEqual(prependedCoordinate, target[i + 1]);
            // CHECK
            // should expected be generator.MainList[1], since 
            // the target slice is defined as .Slice(1, 3)?
            Assert.AreEqual(generator.MainList[0], target[i + 2]);
        }

        [Test]
        public void PrependCoordinateToNewReversedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5);
            generator.Sequence.Reverse();

            BufferedCoordinate2D coord = generator.RandomCoordinate(BigMaxLimit);

            generator.Sequence.Prepend(coord);

            Assert.AreEqual(coord, generator.Sequence[0]);
            Assert.AreEqual(generator.MainList[generator.MainList.Count - 1], generator.Sequence[1]);
        }

        [Test]
        public void PrependCoordinateToNewReversedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, 3);
            slice.Reverse();

            BufferedCoordinate2D coord = generator.RandomCoordinate(BigMaxLimit);

            slice.Prepend(coord);

            Assert.AreEqual(coord, slice[0]);
            Assert.AreEqual(generator.MainList[generator.MainList.Count - 2], slice[1]);
        }

        [Test]
        public void PrependCoordinateToPrependedReversedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5);

            BufferedCoordinate2D coord1 = generator.RandomCoordinate(BigMaxLimit);
            BufferedCoordinate2D coord0 = generator.RandomCoordinate(BigMaxLimit);

            generator.Sequence.Reverse();
            generator.Sequence.Prepend(coord1);

            Assert.AreEqual(coord1, generator.Sequence[0]);
            Assert.AreEqual(generator.MainList[generator.MainList.Count - 1], generator.Sequence[1]);

            generator.Sequence.Prepend(coord0);

            Assert.AreEqual(coord0, generator.Sequence[0]);
            Assert.AreEqual(coord1, generator.Sequence[1]);
            Assert.AreEqual(generator.MainList[generator.MainList.Count - 1], generator.Sequence[2]);
        }

        [Test]
        public void PrependCoordinateToReversedPrependedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5);

            BufferedCoordinate2D coord1 = generator.RandomCoordinate(BigMaxLimit);
            BufferedCoordinate2D coord0 = generator.RandomCoordinate(BigMaxLimit);

            generator.Sequence.Prepend(coord1);

            BufferedCoordinate2D expected;
            BufferedCoordinate2D actual;

            expected = coord1;
            actual = generator.Sequence[0];
            Assert.AreEqual(expected, actual);

            expected = generator.MainList[0];
            actual = generator.Sequence[1];
            Assert.AreEqual(expected, actual);

            generator.Sequence.Reverse();

            generator.Sequence.Prepend(coord0);

            Assert.AreEqual(coord0, generator.Sequence[0]);

            // CHECK
            // should the actual be generator.Sequence[1]?
            expected = generator.MainList[generator.MainList.Count - 1];
            actual = generator.Sequence[2];
            Assert.AreEqual(expected, actual);

            Assert.AreEqual(coord1, generator.Sequence.Last);
        }

        [Test]
        public void PrependCoordinateToPrependedReversedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, 3);

            BufferedCoordinate2D coord1 = generator.RandomCoordinate(BigMaxLimit);
            BufferedCoordinate2D coord0 = generator.RandomCoordinate(BigMaxLimit);

            slice.Reverse();
            slice.Prepend(coord1);

            Assert.AreEqual(coord1, slice[0]);
            Assert.AreEqual(generator.MainList[generator.MainList.Count - 2], slice[1]);

            slice.Prepend(coord0);

            Assert.AreEqual(coord0, slice[0]);
            Assert.AreEqual(coord1, slice[1]);
            Assert.AreEqual(generator.MainList[generator.MainList.Count - 2], slice[2]);
        }

        [Test]
        public void PrependCoordinateToReversedPrependedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, 3);

            BufferedCoordinate2D coord1 = generator.RandomCoordinate(BigMaxLimit);
            BufferedCoordinate2D coord0 = generator.RandomCoordinate(BigMaxLimit);

            slice.Prepend(coord1);

            Assert.AreEqual(coord1, slice[0]);
            Assert.AreEqual(generator.MainList[1], slice[1]);
            slice.Reverse();

            slice.Prepend(coord0);

            Assert.AreEqual(coord0, slice[0]);
            Assert.AreEqual(generator.MainList[generator.MainList.Count - 2], slice[2]);
            Assert.AreEqual(coord1, slice.Last);
        }

        [Test]
        public void PrependEnumerationToNewReversedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            generator.Sequence.Reverse();

            BufferedCoordinate2DEnumeratorDelegate enumeratorDelegate
                = delegate { return generator.PrependList.GetEnumerator(); };

            MockRepository mocks = new MockRepository();
            IEnumerable<BufferedCoordinate2D> prependList
                = mocks.CreateMock<IEnumerable<BufferedCoordinate2D>>();
            Expect.Call(prependList.GetEnumerator())
                .Repeat.Any()
                .Do(enumeratorDelegate);
            mocks.ReplayAll();

            generator.Sequence.Prepend(prependList);

            Int32 i = 0;

            for (; i < generator.PrependList.Count; i++)
            {
                Assert.AreEqual(generator.PrependList[i], generator.Sequence[i]);
            }

            Assert.AreEqual(generator.MainList[generator.MainList.Count - 1], generator.Sequence[i]);

            mocks.VerifyAll();
        }

        [Test]
        public void PrependEnumerationToNewReversedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, 3);
            slice.Reverse();

            BufferedCoordinate2DEnumeratorDelegate enumeratorDelegate
                = delegate { return generator.PrependList.GetEnumerator(); };

            MockRepository mocks = new MockRepository();
            IEnumerable<BufferedCoordinate2D> prependList
                = mocks.CreateMock<IEnumerable<BufferedCoordinate2D>>();
            Expect.Call(prependList.GetEnumerator())
                .Repeat.Any()
                .Do(enumeratorDelegate);
            mocks.ReplayAll();

            slice.Prepend(prependList);

            Int32 i = 0;

            for (; i < generator.PrependList.Count; i++)
            {
                Assert.AreEqual(generator.PrependList[i], slice[i]);
            }

            Assert.AreEqual(generator.MainList[generator.MainList.Count - 2], slice[i]);

            mocks.VerifyAll();
        }

        [Test]
        public void PrependEnumerationToPrependedReversedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            generator.Sequence.Reverse();

            BufferedCoordinate2D prependedCoordinate = generator.CoordinateFactory.Create(1, 1);
            generator.Sequence.Prepend(prependedCoordinate);

            BufferedCoordinate2DEnumeratorDelegate enumeratorDelegate
                = delegate { return generator.PrependList.GetEnumerator(); };

            MockRepository mocks = new MockRepository();
            IEnumerable<BufferedCoordinate2D> prependList
                = mocks.CreateMock<IEnumerable<BufferedCoordinate2D>>();
            Expect.Call(prependList.GetEnumerator())
                .Repeat.Any()
                .Do(enumeratorDelegate);
            mocks.ReplayAll();

            generator.Sequence.Prepend(prependList);

            Int32 i = 0;

            for (; i < generator.PrependList.Count; i++)
            {
                Assert.AreEqual(generator.PrependList[i], generator.Sequence[i]);
            }

            Assert.AreEqual(prependedCoordinate, generator.Sequence[i]);
            Assert.AreEqual(generator.MainList[generator.MainList.Count - 1], generator.Sequence[i + 1]);

            mocks.VerifyAll();
        }

        [Test]
        public void PrependEnumerationToReversedPrependedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);

            BufferedCoordinate2D prependedCoordinate = generator.CoordinateFactory.Create(1, 1);
            generator.Sequence.Prepend(prependedCoordinate);
            generator.Sequence.Reverse();

            BufferedCoordinate2DEnumeratorDelegate enumeratorDelegate
                = delegate { return generator.PrependList.GetEnumerator(); };

            MockRepository mocks = new MockRepository();
            IEnumerable<BufferedCoordinate2D> prependList
                = mocks.CreateMock<IEnumerable<BufferedCoordinate2D>>();
            Expect.Call(prependList.GetEnumerator())
                .Repeat.Any()
                .Do(enumeratorDelegate);
            mocks.ReplayAll();

            generator.Sequence.Prepend(prependList);

            Int32 i = 0;

            for (; i < generator.PrependList.Count; i++)
            {
                Assert.AreEqual(generator.PrependList[i], generator.Sequence[i]);
            }

            Assert.AreEqual(generator.MainList[generator.MainList.Count - 1], generator.Sequence[i + 1]);
            Assert.AreEqual(prependedCoordinate, generator.Sequence.Last);

            mocks.VerifyAll();
        }

        [Test]
        public void PrependEnumerationToPrependedReversedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, 3);
            slice.Reverse();

            BufferedCoordinate2D prependedCoordinate = generator.CoordinateFactory.Create(1, 1);
            slice.Prepend(prependedCoordinate);

            BufferedCoordinate2DEnumeratorDelegate enumeratorDelegate
                = delegate { return generator.PrependList.GetEnumerator(); };

            MockRepository mocks = new MockRepository();
            IEnumerable<BufferedCoordinate2D> prependList
                = mocks.CreateMock<IEnumerable<BufferedCoordinate2D>>();
            Expect.Call(prependList.GetEnumerator())
                .Repeat.Any()
                .Do(enumeratorDelegate);
            mocks.ReplayAll();

            slice.Prepend(prependList);

            Int32 i = 0;

            for (; i < generator.PrependList.Count; i++)
            {
                Assert.AreEqual(generator.PrependList[i], slice[i]);
            }

            Assert.AreEqual(prependedCoordinate, slice[i]);
            Assert.AreEqual(generator.MainList[generator.MainList.Count - 2], slice[i + 1]);

            mocks.VerifyAll();
        }

        [Test]
        public void PrependEnumerationToReversedPrependedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, 3);

            BufferedCoordinate2D prependedCoordinate = generator.CoordinateFactory.Create(1, 1);
            slice.Prepend(prependedCoordinate);
            slice.Reverse();

            BufferedCoordinate2DEnumeratorDelegate enumeratorDelegate
                = delegate { return generator.PrependList.GetEnumerator(); };

            MockRepository mocks = new MockRepository();
            IEnumerable<BufferedCoordinate2D> prependList
                = mocks.CreateMock<IEnumerable<BufferedCoordinate2D>>();
            Expect.Call(prependList.GetEnumerator())
                .Repeat.Any()
                .Do(enumeratorDelegate);
            mocks.ReplayAll();

            slice.Prepend(prependList);

            Int32 i = 0;

            for (; i < generator.PrependList.Count; i++)
            {
                Assert.AreEqual(generator.PrependList[i], slice[i]);
            }

            Assert.AreEqual(generator.MainList[generator.MainList.Count - 2], slice[i]);
            Assert.AreEqual(prependedCoordinate, slice.Last);

            mocks.VerifyAll();
        }

        [Test]
        public void PrependSequenceToNewReversedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            ICoordinateSequence<BufferedCoordinate2D> prependSeq = generator.SequenceFactory.Create(generator.PrependList);

            generator.Sequence.Reverse();
            generator.Sequence.Prepend(prependSeq);

            Int32 i = 0;

            for (; i < generator.PrependList.Count; i++)
            {
                Assert.AreEqual(generator.PrependList[i], generator.Sequence[i]);
            }

            Assert.AreEqual(generator.MainList[generator.MainList.Count - 1], generator.Sequence[i]);
        }

        [Test]
        public void PrependSequenceToNewReversedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, 3);
            slice.Reverse();

            ICoordinateSequence<BufferedCoordinate2D> prependSeq = generator.SequenceFactory.Create(generator.PrependList);

            slice.Prepend(prependSeq);

            Int32 i = 0;

            for (; i < generator.PrependList.Count; i++)
            {
                Assert.AreEqual(generator.PrependList[i], slice[i]);
            }

            Assert.AreEqual(generator.MainList[generator.MainList.Count - 2], slice[i]);
        }

        [Test]
        public void PrependSequenceToPrependedReversedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            generator.Sequence.Reverse();

            BufferedCoordinate2D prependedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            generator.Sequence.Prepend(prependedCoordinate);

            ICoordinateSequence<BufferedCoordinate2D> prependSeq 
                = generator.SequenceFactory.Create(generator.PrependList);

            generator.Sequence.Prepend(prependSeq);

            Int32 i = 0;

            for (; i < generator.PrependList.Count; i++)
            {
                Assert.AreEqual(generator.PrependList[i], generator.Sequence[i]);
            }

            Assert.AreEqual(prependedCoordinate, generator.Sequence[i]);
            Assert.AreEqual(generator.MainList[generator.MainList.Count - 1], generator.Sequence[i + 1]);
        }

        [Test]
        public void PrependSequenceToReversedPrependedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);

            BufferedCoordinate2D prependedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            generator.Sequence.Prepend(prependedCoordinate);
            generator.Sequence.Reverse();

            ICoordinateSequence<BufferedCoordinate2D> prependSeq = generator.SequenceFactory.Create(generator.PrependList);

            generator.Sequence.Prepend(prependSeq);

            Int32 i = 0;

            for (; i < generator.PrependList.Count; i++)
            {
                Assert.AreEqual(generator.PrependList[i], generator.Sequence[i]);
            }

            Assert.AreEqual(generator.MainList[generator.MainList.Count - 1], generator.Sequence[i]);
            Assert.AreEqual(prependedCoordinate, generator.Sequence.Last);
        }

        [Test]
        public void PrependSequenceToPrependedReversedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, 3);
            slice.Reverse();

            BufferedCoordinate2D prependedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            slice.Prepend(prependedCoordinate);

            ICoordinateSequence<BufferedCoordinate2D> prependSeq = generator.SequenceFactory.Create(generator.PrependList);

            slice.Prepend(prependSeq);

            Int32 i = 0;

            for (; i < generator.PrependList.Count; i++)
            {
                Assert.AreEqual(generator.PrependList[i], slice[i]);
            }

            Assert.AreEqual(prependedCoordinate, slice[i]);
            Assert.AreEqual(generator.MainList[generator.MainList.Count - 2], slice[i + 1]);
        }

        [Test]
        public void PrependSequenceToReversedPrependedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, 3);

            BufferedCoordinate2D prependedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            slice.Prepend(prependedCoordinate);
            slice.Reverse();

            ICoordinateSequence<BufferedCoordinate2D> prependSeq = generator.SequenceFactory.Create(generator.PrependList);

            slice.Prepend(prependSeq);

            Int32 i = 0;

            for (; i < generator.PrependList.Count; i++)
            {
                Assert.AreEqual(generator.PrependList[i], slice[i]);
            }

            Assert.AreEqual(generator.MainList[generator.MainList.Count - 2], slice[i]);
            Assert.AreEqual(prependedCoordinate, slice.Last);
        }

        [Test]
        public void PrependComplexSliceToPrependedReversedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            generator.Sequence.Reverse();

            BufferedCoordinate2D prependedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            generator.Sequence.Prepend(prependedCoordinate);

            IBufferedCoordSequence prependSlice
                = generator.SequenceFactory.Create(generator.PrependList)
                .Slice(0, 2);
            BufferedCoordinate2D preSliceCoordinate = generator.RandomCoordinate(BigMaxLimit);
            BufferedCoordinate2D postSliceCoordinate = generator.RandomCoordinate(BigMaxLimit);
            prependSlice.Prepend(preSliceCoordinate);
            prependSlice.Append(postSliceCoordinate);

            generator.Sequence.Prepend(prependSlice);

            Assert.AreEqual(preSliceCoordinate, generator.Sequence[0]);

            Int32 i = 1;

            for (; i <= generator.PrependList.Count; i++)
            {
                Assert.AreEqual(generator.PrependList[i - 1], generator.Sequence[i]);
            }

            Assert.AreEqual(postSliceCoordinate, generator.Sequence[i]);
            Assert.AreEqual(prependedCoordinate, generator.Sequence[i + 1]);
            Assert.AreEqual(generator.MainList[generator.MainList.Count - 1], generator.Sequence[i + 2]);
        }

        [Test]
        public void PrependComplexSliceToReversedPrependedSequence()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);

            BufferedCoordinate2D prependedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            generator.Sequence.Prepend(prependedCoordinate);
            generator.Sequence.Reverse();

            IBufferedCoordSequence prependSlice
                = generator.SequenceFactory.Create(generator.PrependList)
                .Slice(0, 2);
            BufferedCoordinate2D preSliceCoordinate = generator.RandomCoordinate(BigMaxLimit);
            BufferedCoordinate2D postSliceCoordinate = generator.RandomCoordinate(BigMaxLimit);
            prependSlice.Prepend(preSliceCoordinate);
            prependSlice.Append(postSliceCoordinate);

            generator.Sequence.Prepend(prependSlice);

            Assert.AreEqual(preSliceCoordinate, generator.Sequence[0]);

            Int32 i = 1;

            for (; i <= generator.PrependList.Count; i++)
            {
                Assert.AreEqual(generator.PrependList[i - 1], generator.Sequence[i]);
            }

            Assert.AreEqual(postSliceCoordinate, generator.Sequence[i]);
            Assert.AreEqual(generator.MainList[generator.MainList.Count - 1], generator.Sequence[i + 1]);
            Assert.AreEqual(prependedCoordinate, generator.Sequence.Last);
        }

        [Test]
        public void PrependComplexSliceToPrependedReversedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, 3);
            slice.Reverse();

            BufferedCoordinate2D prependedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            slice.Prepend(prependedCoordinate);

            IBufferedCoordSequence prependSlice
                = generator.SequenceFactory.Create(generator.PrependList)
                  .Slice(0, 2);
            BufferedCoordinate2D preSliceCoordinate = generator.RandomCoordinate(BigMaxLimit);
            BufferedCoordinate2D postSliceCoordinate = generator.RandomCoordinate(BigMaxLimit);
            prependSlice.Prepend(preSliceCoordinate);
            prependSlice.Append(postSliceCoordinate);

            slice.Prepend(prependSlice);

            Assert.AreEqual(preSliceCoordinate, slice[0]);

            Int32 i = 1;

            for (; i <= generator.PrependList.Count; i++)
            {
                Assert.AreEqual(generator.PrependList[i], slice[i]);
            }

            Assert.AreEqual(postSliceCoordinate, slice[i]);
            Assert.AreEqual(prependedCoordinate, slice[i + 1]);
            Assert.AreEqual(generator.MainList[generator.MainList.Count - 2], slice[i + 2]);
        }

        [Test]
        public void PrependComplexSliceToReversedPrependedSlice()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 3, 0);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, 3);

            BufferedCoordinate2D prependedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            slice.Prepend(prependedCoordinate);
            slice.Reverse();

            IBufferedCoordSequence prependSlice
                = generator.SequenceFactory.Create(generator.PrependList)
                  .Slice(0, 2);
            BufferedCoordinate2D preSliceCoordinate = generator.RandomCoordinate(BigMaxLimit);
            BufferedCoordinate2D postSliceCoordinate = generator.RandomCoordinate(BigMaxLimit);
            prependSlice.Prepend(preSliceCoordinate);
            prependSlice.Append(postSliceCoordinate);

            slice.Prepend(prependSlice);

            Assert.AreEqual(preSliceCoordinate, slice[0]);

            Int32 i = 1;

            for (; i <= generator.PrependList.Count; i++)
            {
                Assert.AreEqual(generator.PrependList[i], slice[i]);
            }

            Assert.AreEqual(postSliceCoordinate, slice[i]);
            Assert.AreEqual(generator.MainList[generator.MainList.Count - 2], slice[i + 1]);
            Assert.AreEqual(prependedCoordinate, slice.Last);
        }

        [Test]
        public void AppendCoordinateToNewSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);

            BufferedCoordinate2D coord = generator.RandomCoordinate(BigMaxLimit);

            generator.Sequence.Append(coord);

            Assert.AreEqual(generator.MainList[mainLength - 1], generator.Sequence[mainLength - 1]);
            Assert.AreEqual(coord, generator.Sequence[mainLength]);
        }

        [Test]
        public void AppendCoordinateToNewSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, mainLength - 2);

            BufferedCoordinate2D coord = generator.RandomCoordinate(BigMaxLimit);

            slice.Append(coord);

            Assert.AreEqual(generator.MainList[mainLength - 2], slice[sliceLength - 1]);
            Assert.AreEqual(coord, slice[sliceLength]);
        }

        [Test]
        public void AppendCoordinateToAppendedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);

            BufferedCoordinate2D coord1 = generator.RandomCoordinate(BigMaxLimit);
            BufferedCoordinate2D coord0 = generator.RandomCoordinate(BigMaxLimit);

            generator.Sequence.Append(coord1);

            Assert.AreEqual(generator.MainList[mainLength - 1], generator.Sequence[mainLength - 1]);
            Assert.AreEqual(coord1, generator.Sequence[mainLength]);

            generator.Sequence.Append(coord0);

            Assert.AreEqual(generator.MainList[mainLength - 1], generator.Sequence[mainLength - 1]);
            Assert.AreEqual(coord1, generator.Sequence[mainLength]);
            Assert.AreEqual(coord0, generator.Sequence[mainLength + 1]);
        }

        [Test]
        public void AppendCoordinateToAppendedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, mainLength - 2);

            BufferedCoordinate2D coord1 = generator.RandomCoordinate(BigMaxLimit);
            BufferedCoordinate2D coord0 = generator.RandomCoordinate(BigMaxLimit);

            slice.Append(coord1);

            Assert.AreEqual(generator.MainList[mainLength - 2], slice[sliceLength - 1]);
            Assert.AreEqual(coord1, slice[sliceLength]);

            slice.Append(coord0);

            Assert.AreEqual(generator.MainList[mainLength - 2], slice[sliceLength - 1]);
            Assert.AreEqual(coord1, slice[sliceLength]);
            Assert.AreEqual(coord0, slice[sliceLength + 1]);
        }

        [Test]
        public void AppendEnumerationToNewSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);

            BufferedCoordinate2DEnumeratorDelegate enumeratorDelegate
                = delegate { return generator.AppendList.GetEnumerator(); };

            MockRepository mocks = new MockRepository();
            IEnumerable<BufferedCoordinate2D> appendList
                = mocks.CreateMock<IEnumerable<BufferedCoordinate2D>>();
            Expect.Call(appendList.GetEnumerator())
                .Repeat.Any()
                .Do(enumeratorDelegate);
            mocks.ReplayAll();

            generator.Sequence.Append(appendList);

            Assert.AreEqual(generator.MainList[mainLength - 1], generator.Sequence[mainLength - 1]);
            
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], generator.Sequence[mainLength + i]);
            }

            mocks.VerifyAll();
        }

        [Test]
        public void AppendEnumerationToNewSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            BufferedCoordinate2DEnumeratorDelegate enumeratorDelegate
                = delegate { return generator.AppendList.GetEnumerator(); };

            MockRepository mocks = new MockRepository();
            IEnumerable<BufferedCoordinate2D> appendList
                = mocks.CreateMock<IEnumerable<BufferedCoordinate2D>>();
            Expect.Call(appendList.GetEnumerator())
                .Repeat.Any()
                .Do(enumeratorDelegate);
            mocks.ReplayAll();

            slice.Append(appendList);

            Assert.AreEqual(generator.MainList[sliceLength], slice[sliceLength - 1]);
            
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + i]);
            }

            mocks.VerifyAll();
        }

        [Test]
        public void AppendEnumerationToAppendedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);

            BufferedCoordinate2D appendedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            generator.Sequence.Append(appendedCoordinate);

            BufferedCoordinate2DEnumeratorDelegate enumeratorDelegate
                = delegate { return generator.AppendList.GetEnumerator(); };

            MockRepository mocks = new MockRepository();
            IEnumerable<BufferedCoordinate2D> appendList
                = mocks.CreateMock<IEnumerable<BufferedCoordinate2D>>();
            Expect.Call(appendList.GetEnumerator())
                .Repeat.Any()
                .Do(enumeratorDelegate);
            mocks.ReplayAll();

            generator.Sequence.Append(appendList);

            Assert.AreEqual(generator.MainList[mainLength - 1], generator.Sequence[mainLength - 1]);
            Assert.AreEqual(appendedCoordinate, generator.Sequence[mainLength]);
            
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], generator.Sequence[mainLength + 1 + i]);
            }

            mocks.VerifyAll();
        }

        [Test]
        public void AppendEnumerationToAppendedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            BufferedCoordinate2D appendedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            slice.Append(appendedCoordinate);

            BufferedCoordinate2DEnumeratorDelegate enumeratorDelegate
                = delegate { return generator.AppendList.GetEnumerator(); };

            MockRepository mocks = new MockRepository();
            IEnumerable<BufferedCoordinate2D> appendList
                = mocks.CreateMock<IEnumerable<BufferedCoordinate2D>>();
            Expect.Call(appendList.GetEnumerator())
                .Repeat.Any()
                .Do(enumeratorDelegate);
            mocks.ReplayAll();

            slice.Append(appendList);

            Assert.AreEqual(generator.MainList[mainLength - 2], slice[sliceLength - 1]);
            Assert.AreEqual(appendedCoordinate, slice[sliceLength]);
            
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + 1 + i]);
            }

            mocks.VerifyAll();
        }

        [Test]
        public void AppendSequenceToNewSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            ICoordinateSequence<BufferedCoordinate2D> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            generator.Sequence.Append(appendSeq);

            Assert.AreEqual(generator.MainList[mainLength - 1], generator.Sequence[mainLength - 1]);
            
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], generator.Sequence[mainLength + i]);
            }
        }

        [Test]
        public void AppendSequenceToNewSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            ICoordinateSequence<BufferedCoordinate2D> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            slice.Append(appendSeq);

            Assert.AreEqual(generator.MainList[sliceLength], slice[sliceLength - 1]);
            
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + i]);
            }
        }

        [Test]
        public void AppendSequenceToAppendedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);

            BufferedCoordinate2D appendedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            generator.Sequence.Append(appendedCoordinate);

            ICoordinateSequence<BufferedCoordinate2D> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            generator.Sequence.Append(appendSeq);

            Assert.AreEqual(generator.MainList[mainLength - 1], generator.Sequence[mainLength - 1]);
            Assert.AreEqual(appendedCoordinate, generator.Sequence[mainLength]);
            
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], generator.Sequence[mainLength + 1 + i]);
            }
        }

        [Test]
        public void AppendSequenceToAppendedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            BufferedCoordinate2D appendedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            slice.Append(appendedCoordinate);

            ICoordinateSequence<BufferedCoordinate2D> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            slice.Append(appendSeq);

            Assert.AreEqual(generator.MainList[sliceLength], slice[sliceLength - 1]);
            Assert.AreEqual(appendedCoordinate, slice[sliceLength]);
            
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + 1 + i]);
            }
        }

        [Test]
        public void AppendComplexSliceToAppendedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);

            BufferedCoordinate2D appendedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            generator.Sequence.Append(appendedCoordinate);

            IBufferedCoordSequence appendSlice
                = generator.SequenceFactory.Create(generator.AppendList)
                  .Slice(0, 2);
            BufferedCoordinate2D preSliceCoordinate = generator.RandomCoordinate(BigMaxLimit);
            BufferedCoordinate2D postSliceCoordinate = generator.RandomCoordinate(BigMaxLimit);
            appendSlice.Prepend(preSliceCoordinate);
            appendSlice.Append(postSliceCoordinate);

            generator.Sequence.Append(appendSlice);

            Assert.AreEqual(generator.MainList[mainLength - 1], generator.Sequence[mainLength - 1]);
            Assert.AreEqual(appendedCoordinate, generator.Sequence[mainLength]);
            Assert.AreEqual(preSliceCoordinate, generator.Sequence[mainLength + 1]);
            
            for (Int32 i = 0; i <= generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], generator.Sequence[mainLength + 2 + i]);
            }
            
            Assert.AreEqual(postSliceCoordinate, generator.Sequence[mainLength + 2 + generator.AppendList.Count]);
        }

        [Test]
        public void AppendComplexSliceToAppendedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            BufferedCoordinate2D appendedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            slice.Append(appendedCoordinate);

            IBufferedCoordSequence appendSlice
                = generator.SequenceFactory.Create(generator.AppendList)
                  .Slice(0, 2);
            BufferedCoordinate2D preSliceCoordinate = generator.RandomCoordinate(BigMaxLimit);
            BufferedCoordinate2D postSliceCoordinate = generator.RandomCoordinate(BigMaxLimit);
            appendSlice.Prepend(preSliceCoordinate);
            appendSlice.Append(postSliceCoordinate);

            slice.Append(appendSlice);

            Assert.AreEqual(generator.MainList[sliceLength], slice[sliceLength - 1]);
            Assert.AreEqual(appendedCoordinate, slice[sliceLength]);
            Assert.AreEqual(preSliceCoordinate, slice[sliceLength + 1]);
            
            for (Int32 i = 0; i <= generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + 2 + i]);
            }
           
            Assert.AreEqual(postSliceCoordinate, slice[sliceLength + 2 + generator.AppendList.Count]);
        }

        [Test]
        public void AppendCoordinateToNewReversedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);
            generator.Sequence.Reverse();

            BufferedCoordinate2D coord = generator.RandomCoordinate(BigMaxLimit);

            generator.Sequence.Append(coord);

            Assert.AreEqual(generator.MainList[0], generator.Sequence[mainLength - 1]);
            Assert.AreEqual(coord, generator.Sequence[mainLength]);
        }

        [Test]
        public void AppendCoordinateToNewReversedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);
            slice.Reverse();

            BufferedCoordinate2D coord = generator.RandomCoordinate(BigMaxLimit);

            slice.Append(coord);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 1]);
            Assert.AreEqual(coord, slice[sliceLength]);
        }

        [Test]
        public void AppendCoordinateToAppendedReversedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);

            BufferedCoordinate2D coord1 = generator.RandomCoordinate(BigMaxLimit);
            BufferedCoordinate2D coord0 = generator.RandomCoordinate(BigMaxLimit);

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
        public void AppendCoordinateToReversedAppendedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);

            BufferedCoordinate2D coord1 = generator.RandomCoordinate(BigMaxLimit);
            BufferedCoordinate2D coord0 = generator.RandomCoordinate(BigMaxLimit);

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
        public void AppendCoordinateToAppendedReversedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            BufferedCoordinate2D coord1 = generator.RandomCoordinate(BigMaxLimit);
            BufferedCoordinate2D coord0 = generator.RandomCoordinate(BigMaxLimit);

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
        public void AppendCoordinateToReversedAppendedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            BufferedCoordinate2D coord1 = generator.RandomCoordinate(BigMaxLimit);
            BufferedCoordinate2D coord0 = generator.RandomCoordinate(BigMaxLimit);

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
        public void AppendEnumerationToNewReversedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            generator.Sequence.Reverse();
            Assert.AreEqual(generator.MainList[0], generator.Sequence[mainLength - 1]);

            BufferedCoordinate2DEnumeratorDelegate enumeratorDelegate
                = delegate { return generator.AppendList.GetEnumerator(); };

            MockRepository mocks = new MockRepository();
            IEnumerable<BufferedCoordinate2D> appendList
                = mocks.CreateMock<IEnumerable<BufferedCoordinate2D>>();
            Expect.Call(appendList.GetEnumerator())
                .Repeat.Any()
                .Do(enumeratorDelegate);
            mocks.ReplayAll();

            generator.Sequence.Append(appendList);

            Assert.AreEqual(generator.MainList[0], generator.Sequence[mainLength - 1]);
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], generator.Sequence[mainLength + i]);
            }

            mocks.VerifyAll();
        }

        [Test]
        public void AppendEnumerationToNewReversedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);
            slice.Reverse();

            BufferedCoordinate2DEnumeratorDelegate enumeratorDelegate
                = delegate { return generator.AppendList.GetEnumerator(); };

            MockRepository mocks = new MockRepository();
            IEnumerable<BufferedCoordinate2D> appendList
                = mocks.CreateMock<IEnumerable<BufferedCoordinate2D>>();
            Expect.Call(appendList.GetEnumerator())
                .Repeat.Any()
                .Do(enumeratorDelegate);
            mocks.ReplayAll();

            slice.Append(appendList);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 1]);
           
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + i]);
            }

            mocks.VerifyAll();
        }

        [Test]
        public void AppendEnumerationToAppendedReversedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            generator.Sequence.Reverse();

            BufferedCoordinate2D appendedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            generator.Sequence.Append(appendedCoordinate);

            BufferedCoordinate2DEnumeratorDelegate enumeratorDelegate
                = delegate { return generator.AppendList.GetEnumerator(); };

            MockRepository mocks = new MockRepository();
            IEnumerable<BufferedCoordinate2D> appendList
                = mocks.CreateMock<IEnumerable<BufferedCoordinate2D>>();
            Expect.Call(appendList.GetEnumerator())
                .Repeat.Any()
                .Do(enumeratorDelegate);
            mocks.ReplayAll();

            generator.Sequence.Append(appendList);

            Assert.AreEqual(generator.MainList[0], generator.Sequence[mainLength - 1]);
            Assert.AreEqual(appendedCoordinate, generator.Sequence[mainLength]);
           
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], generator.Sequence[mainLength + 1 + i]);
            }

            mocks.VerifyAll();
        }

        [Test]
        public void AppendEnumerationToReversedAppendedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);

            BufferedCoordinate2D appendedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            generator.Sequence.Append(appendedCoordinate);
            generator.Sequence.Reverse();

            BufferedCoordinate2DEnumeratorDelegate enumeratorDelegate
                = delegate { return generator.AppendList.GetEnumerator(); };

            MockRepository mocks = new MockRepository();
            IEnumerable<BufferedCoordinate2D> appendList
                = mocks.CreateMock<IEnumerable<BufferedCoordinate2D>>();
            Expect.Call(appendList.GetEnumerator())
                .Repeat.Any()
                .Do(enumeratorDelegate);
            mocks.ReplayAll();

            generator.Sequence.Append(appendList);

            Assert.AreEqual(generator.MainList[0], generator.Sequence[mainLength]);
          
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], generator.Sequence[mainLength + 1 + i]);
            }
          
            Assert.AreEqual(appendedCoordinate, generator.Sequence[0]);

            mocks.VerifyAll();
        }

        [Test]
        public void AppendEnumerationToAppendedReversedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);
            slice.Reverse();

            BufferedCoordinate2D appendedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            slice.Append(appendedCoordinate);

            BufferedCoordinate2DEnumeratorDelegate enumeratorDelegate
                = delegate { return generator.AppendList.GetEnumerator(); };

            MockRepository mocks = new MockRepository();
            IEnumerable<BufferedCoordinate2D> appendList
                = mocks.CreateMock<IEnumerable<BufferedCoordinate2D>>();
            Expect.Call(appendList.GetEnumerator())
                .Repeat.Any()
                .Do(enumeratorDelegate);
            mocks.ReplayAll();

            slice.Append(appendList);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 1]);
            Assert.AreEqual(appendedCoordinate, slice[sliceLength]);
           
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + 1 + i]);
            }

            mocks.VerifyAll();
        }

        [Test]
        public void AppendEnumerationToReversedAppendedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            BufferedCoordinate2D appendedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            slice.Append(appendedCoordinate);
            slice.Reverse();

            BufferedCoordinate2DEnumeratorDelegate enumeratorDelegate
                = delegate { return generator.AppendList.GetEnumerator(); };

            MockRepository mocks = new MockRepository();
            IEnumerable<BufferedCoordinate2D> appendList
                = mocks.CreateMock<IEnumerable<BufferedCoordinate2D>>();
            Expect.Call(appendList.GetEnumerator())
                .Repeat.Any()
                .Do(enumeratorDelegate);
            mocks.ReplayAll();

            slice.Append(appendList);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength]);
          
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + 1 + i]);
            }
          
            Assert.AreEqual(appendedCoordinate, slice[0]);

            mocks.VerifyAll();
        }

        [Test]
        public void AppendSequenceToNewReversedSequence()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            ICoordinateSequence<BufferedCoordinate2D> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            generator.Sequence.Reverse();
            generator.Sequence.Append(appendSeq);

            Assert.AreEqual(generator.MainList[0], generator.Sequence[mainLength - 1]);
         
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], generator.Sequence[mainLength + i]);
            }
        }

        [Test]
        public void AppendSequenceToNewReversedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);
            slice.Reverse();

            ICoordinateSequence<BufferedCoordinate2D> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            slice.Append(appendSeq);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 1]);
          
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + i]);
            }
        }

        [Test]
        public void AppendSequenceToAppendedReversedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            generator.Sequence.Reverse();

            BufferedCoordinate2D appendedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            generator.Sequence.Append(appendedCoordinate);

            ICoordinateSequence<BufferedCoordinate2D> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            generator.Sequence.Append(appendSeq);

            Assert.AreEqual(generator.MainList[0], generator.Sequence[mainLength - 1]);
            Assert.AreEqual(appendedCoordinate, generator.Sequence[mainLength]);
         
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], generator.Sequence[mainLength + 1 + i]);
            }
        }

        [Test]
        public void AppendSequenceToReversedAppendedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);

            BufferedCoordinate2D appendedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            generator.Sequence.Append(appendedCoordinate);
            generator.Sequence.Reverse();

            ICoordinateSequence<BufferedCoordinate2D> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            generator.Sequence.Append(appendSeq);

            Assert.AreEqual(generator.MainList[0], generator.Sequence[mainLength]);
        
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], generator.Sequence[mainLength + 1 + i]);
            }
           
            Assert.AreEqual(appendedCoordinate, generator.Sequence[0]);
        }

        [Test]
        public void AppendSequenceToAppendedReversedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);
            slice.Reverse();

            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 1]);

            BufferedCoordinate2D appendedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            slice.Append(appendedCoordinate);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 1]);
            Assert.AreEqual(appendedCoordinate, slice[sliceLength]);

            ICoordinateSequence<BufferedCoordinate2D> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            slice.Append(appendSeq);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 1]);
            Assert.AreEqual(appendedCoordinate, slice[sliceLength]);
            
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + 1 + i]);
            }
        }

        [Test]
        public void AppendSequenceToReversedAppendedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            BufferedCoordinate2D appendedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            slice.Append(appendedCoordinate);
            slice.Reverse();

            Assert.AreEqual(generator.MainList[1], slice[sliceLength]);
            Assert.AreEqual(appendedCoordinate, slice[0]);

            ICoordinateSequence<BufferedCoordinate2D> appendSeq = generator.SequenceFactory.Create(generator.AppendList);

            slice.Append(appendSeq);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength]);
           
            for (Int32 i = 0; i < generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + 1 + i]);
            }
            
            Assert.AreEqual(appendedCoordinate, slice[0]);
        }

        [Test]
        public void AppendComplexSliceToAppendedReversedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            generator.Sequence.Reverse();

            BufferedCoordinate2D appendedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            generator.Sequence.Append(appendedCoordinate);

            Assert.AreEqual(generator.MainList[0], generator.Sequence[mainLength - 1]);
            Assert.AreEqual(appendedCoordinate, generator.Sequence[mainLength]);

            IBufferedCoordSequence appendSlice
                = generator.SequenceFactory.Create(generator.AppendList)
                .Slice(0, 2);
            BufferedCoordinate2D preSliceCoordinate = generator.RandomCoordinate(BigMaxLimit);
            BufferedCoordinate2D postSliceCoordinate = generator.RandomCoordinate(BigMaxLimit);
            appendSlice.Prepend(preSliceCoordinate);
            appendSlice.Append(postSliceCoordinate);

            generator.Sequence.Append(appendSlice);

            Assert.AreEqual(generator.MainList[0], generator.Sequence[mainLength - 1]);
            Assert.AreEqual(appendedCoordinate, generator.Sequence[mainLength]);

            Assert.AreEqual(preSliceCoordinate, generator.Sequence[mainLength + 1]);
           
            for (Int32 i = 0; i <= generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], generator.Sequence[mainLength + 2 + i]);
            }
           
            Assert.AreEqual(postSliceCoordinate, generator.Sequence[mainLength + 2 + generator.AppendList.Count]);
        }

        [Test]
        public void AppendComplexSliceToReversedAppendedSequence()
        {
            Int32 mainLength = 5;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);

            BufferedCoordinate2D appendedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            generator.Sequence.Append(appendedCoordinate);
            generator.Sequence.Reverse();

            Assert.AreEqual(generator.MainList[0], generator.Sequence[mainLength]);
            Assert.AreEqual(appendedCoordinate, generator.Sequence[0]);

            IBufferedCoordSequence appendSlice
                = generator.SequenceFactory.Create(generator.AppendList)
                .Slice(0, 2);
            BufferedCoordinate2D preSliceCoordinate = generator.RandomCoordinate(BigMaxLimit);
            BufferedCoordinate2D postSliceCoordinate = generator.RandomCoordinate(BigMaxLimit);
            appendSlice.Prepend(preSliceCoordinate);
            appendSlice.Append(postSliceCoordinate);

            generator.Sequence.Append(appendSlice);

            Assert.AreEqual(generator.MainList[0], generator.Sequence[mainLength]);

            Assert.AreEqual(preSliceCoordinate, generator.Sequence[mainLength + 1]);
          
            for (Int32 i = 0; i <= generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], generator.Sequence[mainLength + 2 + i]);
            }
           
            Assert.AreEqual(postSliceCoordinate, generator.Sequence[mainLength + 2 + generator.AppendList.Count]);

            Assert.AreEqual(appendedCoordinate, generator.Sequence[0]);
        }

        [Test]
        public void AppendComplexSliceToAppendedReversedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);
            slice.Reverse();

            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 1]);

            BufferedCoordinate2D appendedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            slice.Append(appendedCoordinate);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 1]);
            Assert.AreEqual(appendedCoordinate, slice[sliceLength]);

            IBufferedCoordSequence appendSlice
                = generator.SequenceFactory.Create(generator.AppendList)
                .Slice(0, 2);
            BufferedCoordinate2D preSliceCoordinate = generator.RandomCoordinate(BigMaxLimit);
            BufferedCoordinate2D postSliceCoordinate = generator.RandomCoordinate(BigMaxLimit);
            appendSlice.Prepend(preSliceCoordinate);
            appendSlice.Append(postSliceCoordinate);

            slice.Append(appendSlice);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength - 1]);
            Assert.AreEqual(appendedCoordinate, slice[sliceLength]);

            Assert.AreEqual(preSliceCoordinate, slice[sliceLength + 1]);
           
            for (Int32 i = 0; i <= generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + 2 + i]);
            }
            
            Assert.AreEqual(postSliceCoordinate, slice[sliceLength + 2 + generator.AppendList.Count]);
        }

        [Test]
        public void AppendComplexSliceToReversedAppendedSlice()
        {
            Int32 mainLength = 5;
            Int32 sliceLength = mainLength - 2;
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, 0, 3);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            BufferedCoordinate2D appendedCoordinate = generator.RandomCoordinate(BigMaxLimit);
            slice.Append(appendedCoordinate);
            slice.Reverse();

            Assert.AreEqual(generator.MainList[1], slice[sliceLength]);
            Assert.AreEqual(appendedCoordinate, slice[0]);

            IBufferedCoordSequence appendSlice
                = generator.SequenceFactory.Create(generator.AppendList)
                  .Slice(0, 2);
            BufferedCoordinate2D preSliceCoordinate = generator.RandomCoordinate(BigMaxLimit);
            BufferedCoordinate2D postSliceCoordinate = generator.RandomCoordinate(BigMaxLimit);
            appendSlice.Prepend(preSliceCoordinate);
            appendSlice.Append(postSliceCoordinate);

            slice.Append(appendSlice);

            Assert.AreEqual(generator.MainList[1], slice[sliceLength]);

            Assert.AreEqual(preSliceCoordinate, slice[sliceLength + 1]);
            
            for (Int32 i = 0; i <= generator.AppendList.Count; i++)
            {
                Assert.AreEqual(generator.AppendList[i], slice[sliceLength + 2 + i]);
            }
           
            Assert.AreEqual(postSliceCoordinate, slice[sliceLength + 2 + generator.AppendList.Count]);

            Assert.AreEqual(appendedCoordinate, slice[0]);
        }

        [Test]
        public void RemoveSucceeds()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create();

            Assert.IsFalse(seq.Remove(new BufferedCoordinate2D()));

            seq.Add(coordFactory.Create());
            Assert.IsTrue(seq.Remove(new BufferedCoordinate2D()));
            Assert.AreEqual(0, seq.Count);

            seq.Add(coordFactory.Create(0, 0));
            Assert.IsTrue(seq.Remove(coordFactory.Create(0, 0)));
            Assert.AreEqual(0, seq.Count);

            seq.AddRange(generateCoords(10000, BigMaxLimit, coordFactory));

            Int32 count = 10000;
          
            while (seq.Count > 0)
            {
                seq.Remove(seq.Last);
                Assert.LessOrEqual(0, --count);
            }
        }

        [Test]
        public void RemoveFromComplexSliceSucceeds()
        {
            Int32 mainLength = 202;
            Int32 sliceLength = mainLength - 2;
            Int32 xpendLength = 50;
            Int32 segmentBufferLength = 10;

            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, xpendLength, xpendLength);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            slice.Prepend(generator.PrependList);
            slice.Append(generator.AppendList);

            Assert.AreEqual(sliceLength + xpendLength + xpendLength, slice.Count);
            {
                Int32 i = 0;
             
                foreach (BufferedCoordinate2D coord in generator.PrependList)
                {
                    Assert.AreEqual(coord, slice[i++]);
                }
              
                for (Int32 j = 1; j <= sliceLength; j++)
                {
                    Assert.AreEqual(generator.MainList[j], slice[i++]);
                }
               
                foreach (BufferedCoordinate2D coord in generator.AppendList)
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

            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, mainLength, xpendLength, xpendLength);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1, sliceLength);

            slice.Prepend(generator.PrependList);
            slice.Append(generator.AppendList);
            slice.Reverse();

            Assert.AreEqual(sliceLength + xpendLength + xpendLength, slice.Count);
            {
                Int32 i = slice.Count - 1;
            
                foreach (BufferedCoordinate2D coord in generator.PrependList)
                {
                    Assert.AreEqual(coord, slice[i--]);
                }
              
                for (Int32 j = 1; j <= sliceLength; j++)
                {
                    Assert.AreEqual(generator.MainList[j], slice[i--]);
                }
               
                foreach (BufferedCoordinate2D coord in generator.AppendList)
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
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create();
            seq.AddRange(generateCoords(10000, BigMaxLimit, coordFactory));

            Int32 count = 10000;
            
            while (seq.Count > 0)
            {
                seq.RemoveAt(_rnd.Next(0, count));
                Assert.LessOrEqual(0, --count);
            }
        }

        [Test]
        public void ReverseSucceeds()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create();
            List<BufferedCoordinate2D> coordsToTest
                = new List<BufferedCoordinate2D>(generateCoords(10000, BigMaxLimit, coordFactory));
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
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(10000, BigMaxLimit, coordFactory));
            IBufferedCoordSequence reversed = seq.Reversed;

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
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(10000, BigMaxLimit, coordFactory));
            BufferedCoordinate2D firstCoord = seq.First;
            BufferedCoordinate2D midCoord = seq[5000];
            seq.Scroll(midCoord);
            Assert.AreEqual(midCoord, seq.First);
            seq.Scroll(5000);
            Assert.AreEqual(firstCoord, seq.First);
        }

        [Test]
        public void SliceSingleCoordinateSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 1);
            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 0);

            Assert.AreEqual(1, slice.Count);
            Assert.AreEqual(generator.Sequence[0], slice[0]);
        }

        [Test]
        public void SliceSucceeds()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 10000);
            IBufferedCoordSequence slice = generator.Sequence.Slice(1000, 1100);

            Assert.AreEqual(101, slice.Count);

            for (Int32 i = 0; i < slice.Count; i++)
            {
                Assert.AreEqual(generator.Sequence[i + 1000], slice[i]);
            }
        }


        [Test]
        public void SliceSequenceWithPrepended()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 2, 0);

            generator.Sequence.Prepend(generator.PrependList);

            Assert.AreEqual(7, generator.Sequence.Count);

            IBufferedCoordSequence slice = generator.Sequence.Slice(1, 5);

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
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 0, 2);

            generator.Sequence.Append(generator.AppendList);

            Assert.AreEqual(7, generator.Sequence.Count);

            IBufferedCoordSequence slice = generator.Sequence.Slice(1, 5);

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
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 7);

            generator.Sequence.Remove(generator.MainList[2]);
            generator.Sequence.Remove(generator.MainList[4]);

            Assert.AreEqual(5, generator.Sequence.Count);

            IBufferedCoordSequence slice = generator.Sequence.Slice(1, 3);

            Assert.AreEqual(3, slice.Count);

            Assert.AreEqual(generator.MainList[1], slice[0]);
            Assert.AreEqual(generator.MainList[3], slice[1]);
            Assert.AreEqual(generator.MainList[5], slice[2]);
        }

        [Test]
        public void SliceReversedSequenceWithPrependedAppendedSkippedIndices()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 2, 2);

            generator.Sequence.Remove(generator.MainList[1]);
            generator.Sequence.Remove(generator.MainList[3]);
            generator.Sequence.Prepend(generator.PrependList);
            generator.Sequence.Append(generator.AppendList);
            generator.Sequence.Reverse();

            Assert.AreEqual(7, generator.Sequence.Count);

            IBufferedCoordSequence slice = generator.Sequence.Slice(1, 5);

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
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 2, 0);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 4);
            slice.Prepend(generator.PrependList);

            Assert.AreEqual(7, slice.Count);

            IBufferedCoordSequence slice2 = slice.Slice(1, 5);

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
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 0, 2);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 4);
            slice.Append(generator.AppendList);

            Assert.AreEqual(7, slice.Count);

            IBufferedCoordSequence slice2 = slice.Slice(1, 5);

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
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 7);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 6);
            slice.Remove(generator.MainList[2]);
            slice.Remove(generator.MainList[4]);

            Assert.AreEqual(5, slice.Count);

            IBufferedCoordSequence slice2 = slice.Slice(1, 3);

            Assert.AreEqual(3, slice2.Count);

            Assert.AreEqual(generator.MainList[1], slice2[0]);
            Assert.AreEqual(generator.MainList[3], slice2[1]);
            Assert.AreEqual(generator.MainList[5], slice2[2]);
        }

        [Test]
        public void SliceSliceWithPrependedAppendedSkippedIndices()
        {
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 2, 2);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 4);
            slice.Remove(generator.MainList[1]);
            slice.Remove(generator.MainList[3]);
            slice.Prepend(generator.PrependList);
            slice.Append(generator.AppendList);

            Assert.AreEqual(7, slice.Count);

            IBufferedCoordSequence slice2 = slice.Slice(1, 5);

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
            SequenceGenerator generator = new SequenceGenerator(BigMaxLimit, 5, 2, 2);

            IBufferedCoordSequence slice = generator.Sequence.Slice(0, 4);
            slice.Remove(generator.MainList[1]);
            slice.Remove(generator.MainList[3]);
            slice.Prepend(generator.PrependList);
            slice.Append(generator.AppendList);
            slice.Reverse();

            Assert.AreEqual(7, slice.Count);

            IBufferedCoordSequence slice2 = slice.Slice(1, 5);

            Assert.AreEqual(5, slice2.Count);

            Assert.AreEqual(generator.AppendList[0], slice2[0]);
            Assert.AreEqual(generator.MainList[4], slice2[1]);
            Assert.AreEqual(generator.MainList[2], slice2[2]);
            Assert.AreEqual(generator.MainList[0], slice2[3]);
            Assert.AreEqual(generator.PrependList[1], slice2[4]);
        }

        [Test]
        public void SlicingASequenceFreezesTheParentSequence()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IEnumerable<BufferedCoordinate2D> coords 
                = generateCoords(10000, BigMaxLimit, coordFactory);
            IBufferedCoordSequence seq = seqFactory.Create(coords);
            Assert.IsFalse(seq.IsFrozen);
            Assert.IsFalse(seq.IsReadOnly);
            Assert.IsFalse(seq.IsFixedSize);

            IBufferedCoordSequence slice = seq.Slice(1000, 1100);
            Assert.IsTrue(seq.IsFrozen);
            Assert.IsTrue(seq.IsReadOnly);
            Assert.IsTrue(seq.IsFixedSize);
        }

        [Test]
        public void SortIsCorrect()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(10000,
                                                                          BigMaxLimit,
                                                                          coordFactory));

            seq.Sort();

            for (Int32 i = 1; i < seq.Count; i++)
            {
                Assert.GreaterOrEqual(seq[i].CompareTo(seq[i - 1]), 0);
            }
        }

        [Test]
        public void SpliceIsCorrect()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(10000,
                                                                          BigMaxLimit,
                                                                          coordFactory));

            List<BufferedCoordinate2D> coordsToAdd
                = new List<BufferedCoordinate2D>(generateCoords(100,
                                                 BigMaxLimit,
                                                 coordFactory));

            // Prepend enumeration
            IBufferedCoordSequence splice = seq.Splice(coordsToAdd, 5000, 5099);

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
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(100000, 2, coordFactory));
            seq.Add(coordFactory.Create(1, 1));
            seq.Add(coordFactory.Create(1, 2));
            seq.Add(coordFactory.Create(2, 1));
            seq.Add(coordFactory.Create(2, 2));

            IBufferedCoordSequence filtered = seq.WithoutDuplicatePoints();

            Assert.AreEqual(4, filtered.Count);

            Assert.IsTrue(filtered.Contains(coordFactory.Create(1, 1)));
            Assert.IsTrue(filtered.Contains(coordFactory.Create(1, 2)));
            Assert.IsTrue(filtered.Contains(coordFactory.Create(2, 1)));
            Assert.IsTrue(filtered.Contains(coordFactory.Create(2, 2)));
        }

        [Test]
        public void WithoutRepeatedPointsCorrect()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(100000,
                                                                          2,
                                                                          coordFactory));

            BufferedCoordinate2D last = new BufferedCoordinate2D();

            foreach (BufferedCoordinate2D coordinate in seq)
            {
                Assert.AreNotEqual(last, coordinate);
                last = coordinate;
            }
        }

        [Test]
        public void ChangingSequenceElementDoesntAffectOtherSequencesWithTheSameCoordinate()
        {
            BufferedCoordinate2DSequenceFactory factory
                = new BufferedCoordinate2DSequenceFactory();

            IBufferedCoordSequence seq1
                = factory.Create(CoordinateDimensions.Two);
            IBufferedCoordSequence seq2
                = factory.Create(CoordinateDimensions.Two);

            ICoordinateFactory<BufferedCoordinate2D> coordFactory = factory.CoordinateFactory;

            Random rnd = new MersenneTwister();

            for (Int32 i = 0; i < 100; i++)
            {
                BufferedCoordinate2D coord = coordFactory.Create(rnd.NextDouble(),
                                                                 rnd.NextDouble());
                seq1.Add(coord);
                seq2.Add(coord);
                Assert.IsTrue(seq1[i].Equals(seq2[i]));
            }

            BufferedCoordinate2D c = seq1[10];
            Double x = c.X;
            Double y = c.Y;

            seq1[10] = coordFactory.Create(1234, 1234);

            Assert.AreEqual(x, seq2[10][Ordinates.X]);
            Assert.AreEqual(y, seq2[10][Ordinates.Y]);
        }


        private IEnumerable<BufferedCoordinate2D> generateCoords(Int32 count,
                                                                 Int32 max,
                                                                 BufferedCoordinate2DFactory coordFactory)
        {
            while (count-- > 0)
            {
                yield return coordFactory.Create(_rnd.Next(1, max + 1),
                                                  _rnd.Next(1, max + 1));
            }
        }
    }
}
