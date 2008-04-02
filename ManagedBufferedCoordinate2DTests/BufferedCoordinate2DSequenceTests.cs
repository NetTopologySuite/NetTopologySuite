using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures.Collections.Generic;
using NetTopologySuite.Coordinates;
using NPack;
using NUnit.Framework;

namespace ManagedBufferedCoordinate2DTests
{
    using IBufferedCoordFactory = ICoordinateFactory<BufferedCoordinate2D>;
    using IBufferedCoordSequence = ICoordinateSequence<BufferedCoordinate2D>;
    using IBufferedCoordSequenceFactory = ICoordinateSequenceFactory<BufferedCoordinate2D>;

    [TestFixture]
    public class BufferedCoordinate2DSequenceTests
    {
        private Random _rnd = new MersenneTwister();
        private BufferedCoordinate2DFactory _coordFactory
            = new BufferedCoordinate2DFactory();

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
            BufferedCoordinate2DSequenceFactory factory
                = new BufferedCoordinate2DSequenceFactory();

            IBufferedCoordSequence seq
                = factory.Create(generateCoords(9999, Int32.MaxValue - 1));

            Assert.AreEqual(9999, seq.Count);
        }

        [Test]
        public void CreatingCoordinateSequenceWithoutRepeatedCoordinatesSucceeds()
        {
            BufferedCoordinate2DSequenceFactory factory
                = new BufferedCoordinate2DSequenceFactory();

            List<BufferedCoordinate2D> coords
                = new List<BufferedCoordinate2D>(generateCoords(100, 1));

            Assert.AreEqual(100, coords.Count);

            IBufferedCoordSequence seq1 = factory.Create(coords, true);

            for (Int32 i = 0; i < 100; i++)
            {
                Assert.IsTrue(seq1[i].ValueEquals(coords[i]));
            }

            Assert.AreEqual(100, seq1.Count);

            IBufferedCoordSequence seq2
                = factory.Create(coords, false);

            Assert.AreEqual(1, seq2.Count);
        }

        [Test]
        public void CreatingSequenceAsUniqueSucceeds()
        {
            BufferedCoordinate2DSequenceFactory factory
                = new BufferedCoordinate2DSequenceFactory();

            List<BufferedCoordinate2D> coords
                = new List<BufferedCoordinate2D>(generateCoords(1000, 5));

            IBufferedCoordSequence seq = factory.Create(coords, false);

            seq = seq.WithoutDuplicatePoints();

            CollectionAssert.AllItemsAreUnique(seq);
        }

        [Test]
        public void SequenceToArraySucceeds()
        {
            BufferedCoordinate2DSequenceFactory factory
                = new BufferedCoordinate2DSequenceFactory();

            IBufferedCoordSequence seq = factory.Create(generateCoords(1000, 500000));

            BufferedCoordinate2D[] coordsArray = seq.ToArray();

            Assert.AreEqual(seq.Count, coordsArray.Length);

            for (Int32 i = 0; i < seq.Count; i++)
            {
                Assert.IsTrue(seq[i].Equals(coordsArray[i]));
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
            BufferedCoordinate2DSequenceFactory factory
                = new BufferedCoordinate2DSequenceFactory();

            IBufferedCoordSequence seq = factory.Create(CoordinateDimensions.Two);

            List<BufferedCoordinate2D> coordsToAdd
                = new List<BufferedCoordinate2D>(generateCoords(1000, 500000));

            seq.AddRange(coordsToAdd);

            Assert.AreEqual(coordsToAdd.Count, seq.Count);

            for (Int32 i = 0; i < coordsToAdd.Count; i++)
            {
                Assert.IsTrue(coordsToAdd[i].ValueEquals(seq[i]));
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
            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory();

            IBufferedCoordSequence seq
                = seqFactory.Create(generateCoords(1000, 5000));

            ISet<BufferedCoordinate2D> set = seq.AsSet();

            Assert.IsNotNull(set);
        }

        [Test]
        public void CloneSucceeds()
        {
            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory();

            IBufferedCoordSequence seq1
                = seqFactory.Create(generateCoords(1000, 5000));

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
                = new List<BufferedCoordinate2D>(generateCoords(1000, 500000));

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
            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory();

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(1000, 200));

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
            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory();

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(1000, 200));

            Assert.AreEqual(1000, seq.Count);
        }

        [Test]
        public void CountIsCorrectAfterAddOperations()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(1000, 100));

            seq.Add(coordFactory.Create(789, 456));

            Assert.AreEqual(1001, seq.Count);

            seq.AddRange(generateCoords(100, 100));

            Assert.AreEqual(1101, seq.Count);
        }

        [Test]
        public void CountIsCorrectAfterRemoveOperations()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(1000, 100));

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

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(1000, 100));

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

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(1000, 100));

            IBufferedCoordSequence splice = seq.Splice(coordFactory.Create(-1, -1), 0, 9);

            Assert.AreEqual(11, splice.Count);

            splice = seq.Splice(generateCoords(10, 100), 990, 999);

            Assert.AreEqual(20, splice.Count);

            splice = seq.Splice(generateCoords(10, 100), 990, 999, coordFactory.Create(0, 0));

            Assert.AreEqual(21, splice.Count);

            splice = seq.Splice(generateCoords(10, 100), 990, 999, generateCoords(10, 100));

            Assert.AreEqual(30, splice.Count);

            splice = seq.Splice(coordFactory.Create(0, 0), 990, 999, coordFactory.Create(0, 0));

            Assert.AreEqual(22, splice.Count);

            splice = seq.Splice(990, 999, coordFactory.Create(0, 0));

            Assert.AreEqual(21, splice.Count);

            splice = seq.Splice(990, 999, generateCoords(10, 100));

            Assert.AreEqual(30, splice.Count);
        }

        [Test]
        public void EqualsTestsEqualityCorrectly()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            List<BufferedCoordinate2D> coordsToAdd
                = new List<BufferedCoordinate2D>(generateCoords(1000, 1000));

            IBufferedCoordSequence seq1 = seqFactory.Create(coordsToAdd);
            IBufferedCoordSequence seq2 = seqFactory.Create(coordsToAdd);

            Assert.IsTrue(seq1.Equals(seq2));
        }

        [Test]
        [Ignore("Not Implemented")]
        public void ExpandExtentsSucceeds()
        {

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

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(10, Int32.MaxValue - 2));

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
        public void IndexerSucceeds()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            List<BufferedCoordinate2D> coordList =
                new List<BufferedCoordinate2D>(generateCoords(10, Int32.MaxValue - 2));
            IBufferedCoordSequence seq = seqFactory.Create(coordList);

            for (int i = 0; i < seq.Count; i++)
            {
                Assert.IsTrue(coordList[i].ValueEquals(seq[i]));
            }
        }

        [Test]
        public void IndexerOnReversedSucceeds()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            List<BufferedCoordinate2D> coordList =
                new List<BufferedCoordinate2D>(generateCoords(10, Int32.MaxValue - 2));
            IBufferedCoordSequence seq = seqFactory.Create(coordList);
            seq.Reverse();

            for (int i = 0; i < seq.Count; i++)
            {
                Assert.IsTrue(coordList[coordList.Count - i - 1].ValueEquals(seq[i]));
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CallingIndexerWithNegativeNumberFails()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(10, Int32.MaxValue - 2));

            BufferedCoordinate2D coord = seq[-1];
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CallingIndexerWithNumberEqualToCountFails()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(10, Int32.MaxValue - 2));

            BufferedCoordinate2D coord = seq[seq.Count];
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CallingIndexerWithNumberGreaterThanCountFails()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(10, Int32.MaxValue - 2));

            BufferedCoordinate2D coord = seq[Int32.MaxValue];
        }


        [Test]
        public void IndexerOnSliceIsCorrect()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(10, Int32.MaxValue - 2));

            IBufferedCoordSequence slice = seq.Slice(5, 9);

            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(seq[i + 5], slice[i]);
            }

            IBufferedCoordSequence slice2 = slice.Slice(2, 4);

            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(slice[i + 2], slice2[i]);
            }
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

            foreach (BufferedCoordinate2D coordinate in generateCoords(10, Int32.MaxValue - 2, coordFactory))
            {
                Int32 index = count % 2 == 0 ? 0 : count - 1;
                seq.Insert(index, coordinate);
                count++;
                Assert.AreEqual(coordinate, seq[index]);
            }
        }

        [Test]
        public void IsFixedSizeIsCorrect()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(10, Int32.MaxValue - 1));

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

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(10, Int32.MaxValue - 1));

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

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(10, Int32.MaxValue - 1));

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

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(10, Int32.MaxValue - 1));

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

            seq.AddRange(generateCoords(10000, Int32.MaxValue - 1));

            Int32 count = 10000;
            while (seq.Count > 0)
            {
                seq.Remove(seq.Last);
                Assert.LessOrEqual(0, --count);
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
            seq.AddRange(generateCoords(10000, Int32.MaxValue - 1));

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
                = new List<BufferedCoordinate2D>(generateCoords(10000, Int32.MaxValue - 1));
            seq.AddRange(coordsToTest);
            seq.Reverse();

            Assert.AreEqual(coordsToTest.Count, seq.Count);

            Int32 count = coordsToTest.Count;
            for (Int32 i = 0; i < count; i++)
            {
                Assert.IsTrue(coordsToTest[i].ValueEquals(seq[count - i - 1]));
            }
        }

        [Test]
        public void ReversedIsCorrect()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(10000, Int32.MaxValue - 1));
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

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(10000, Int32.MaxValue - 1));
            BufferedCoordinate2D firstCoord = seq.First;
            BufferedCoordinate2D midCoord = seq[5000];
            seq.Scroll(midCoord);
            Assert.AreEqual(midCoord, seq.First);
            seq.Scroll(5000);
            Assert.AreEqual(firstCoord, seq.First);
        }

        [Test]
        public void SliceSucceeds()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create();
            seq.Add(coordFactory.Create(0, 0));
            IBufferedCoordSequence slice = seq.Slice(0, 0);
            Assert.AreEqual(1, slice.Count);

            List<BufferedCoordinate2D> coordsToTest
                = new List<BufferedCoordinate2D>(generateCoords(10000, Int32.MaxValue - 1));

            seq = seqFactory.Create(coordsToTest);

            slice = seq.Slice(1000, 1100);

            Assert.AreEqual(101, slice.Count);

            for (Int32 i = 0; i < slice.Count; i++)
            {
                Assert.IsTrue(coordsToTest[i + 1000].ValueEquals(slice[i]));
            }
        }

        [Test]
        public void SlicingASequenceFreezesTheParentSequence()
        {
            BufferedCoordinate2DFactory coordFactory
                = new BufferedCoordinate2DFactory();

            BufferedCoordinate2DSequenceFactory seqFactory
                = new BufferedCoordinate2DSequenceFactory(coordFactory);

            IBufferedCoordSequence seq = seqFactory.Create(generateCoords(10000, Int32.MaxValue - 1));
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
                                                                          Int32.MaxValue - 1,
                                                                          coordFactory));

            seq.Sort();

            for (Int32 i = 1; i < seq.Count; i++)
            {
                Assert.GreaterOrEqual(0, seq[i].CompareTo(seq[i - 1]));
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
                                                                          Int32.MaxValue - 1,
                                                                          coordFactory));

            List<BufferedCoordinate2D> coordsToAdd
                = new List<BufferedCoordinate2D>(generateCoords(100,
                                                 Int32.MaxValue - 1,
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

        private IEnumerable<BufferedCoordinate2D> generateCoords(Int32 count, Int32 max)
        {
            return generateCoords(count, max, _coordFactory);
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
