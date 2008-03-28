using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                Assert.IsTrue(coordsToAdd[i * 2].ValueEquals(seq[3 - i]));
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
        public void ReturingASetFromAsSetSucceeds()
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

            Assert.AreEqual(4, slice1.Count);
            Assert.AreNotEqual(slice1.First, slice1.Last);

            slice1.CloseRing();

            Assert.AreEqual(5, slice1.Count);
            Assert.AreEqual(slice1.First, slice1.Last);
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

        }

        [Test]
        public void IncreasingDirectionIsCorrect()
        {

        }

        [Test]
        public void IndexOfSucceeds()
        {

        }

        [Test]
        public void InsertSucceeds()
        {

        }

        [Test]
        public void IndexerSucceeds()
        {

        }

        [Test]
        public void IndexerOnSliceIsCorrect()
        {

        }

        [Test]
        public void Indexer2Succeeds()
        {

        }

        [Test]
        public void IsFixedSizeIsCorrect()
        {

        }

        [Test]
        public void IsReadOnlyIsCorrect()
        {

        }

        [Test]
        public void IsFrozenIsCorrect()
        {

        }

        [Test]
        public void LastIsTheLastCoordinate()
        {

        }

        [Test]
        public void LastIndexIsCorrect()
        {

        }

        [Test]
        public void MaximumIsCorrect()
        {

        }

        [Test]
        public void MinimumIsCorrect()
        {

        }

        [Test]
        public void MergeSucceeds()
        {

        }

        [Test]
        public void RemoveSucceeds()
        {

        }

        [Test]
        public void RemoveAtSucceeds()
        {

        }

        [Test]
        public void ReverseSucceeds()
        {

        }

        [Test]
        public void ReversedIsCorrect()
        {

        }

        [Test]
        public void ScrollSucceeds()
        {

        }

        [Test]
        public void SliceSucceeds()
        {

        }

        [Test]
        public void SlicingASequenceFreezesTheParentSequence()
        {

        }

        [Test]
        public void SortIsCorrect()
        {

        }

        [Test]
        public void SpliceIsCorrect()
        {

        }

        [Test]
        public void WithoutDuplicatePointsCorrect()
        {

        }

        [Test]
        public void WithoutRepeatedPointsCorrect()
        {

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
