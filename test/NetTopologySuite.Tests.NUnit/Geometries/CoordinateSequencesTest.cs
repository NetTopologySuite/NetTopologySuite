using System;
using System.Collections;
using System.Reflection.PortableExecutable;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixture]
    public class CoordinateSequencesTest
    {

        private static readonly double[][] ordinateValues = {
            new[]{75.76,77.43},new[]{41.35,90.75},new[]{73.74,41.67},new[]{20.87,86.49},new[]{17.49,93.59},new[]{67.75,80.63},
            new[]{63.01,52.57},new[]{32.9,44.44}, new[]{79.36,29.8}, new[]{38.17,88.0}, new[]{19.31,49.71},new[]{57.03,19.28},
            new[]{63.76,77.35},new[]{45.26,85.15},new[]{51.71,50.38},new[]{92.16,19.85},new[]{64.18,27.7}, new[]{64.74,65.1},
            new[]{80.07,13.55},new[]{55.54,94.07}};

        [Test]
        public void TestCopyToLargerDim()
        {
            var csFactory = new PackedCoordinateSequenceFactory();
            var cs2D = CreateTestSequence(csFactory, 10, 2, 0);
            var cs3D = csFactory.Create(10, 3, 0);
            CoordinateSequences.Copy(cs2D, 0, cs3D, 0, cs3D.Count);
            Assert.IsTrue(CoordinateSequences.IsEqual(cs2D, cs3D));
        }

        [Test]
        public void TestCopyToSmallerDim()
        {
            var csFactory = new PackedCoordinateSequenceFactory();
            var cs3D = CreateTestSequence(csFactory, 10, 3, 0);
            var cs2D = csFactory.Create(10, 2, 0);
            CoordinateSequences.Copy(cs3D, 0, cs2D, 0, cs2D.Count);
            Assert.IsTrue(CoordinateSequences.IsEqual(cs2D, cs3D));
        }


        [Test]
        public void TestScrollRing()
        {
            TestContext.WriteLine("Testing scrolling of closed ring");
            DoTestScrollRing(CoordinateArraySequenceFactory.Instance, 2);
            DoTestScrollRing(CoordinateArraySequenceFactory.Instance, 3);
            DoTestScrollRing(PackedCoordinateSequenceFactory.DoubleFactory, 2);
            DoTestScrollRing(PackedCoordinateSequenceFactory.DoubleFactory, 4);
            DoTestScrollRing(PackedCoordinateSequenceFactory.FloatFactory, 2);
            DoTestScrollRing(PackedCoordinateSequenceFactory.FloatFactory, 4);
            DoTestScrollRing(DotSpatialAffineCoordinateSequenceFactory.Instance, 2);
            DoTestScrollRing(DotSpatialAffineCoordinateSequenceFactory.Instance, 4);
        }

        [Test]
        public void TestScroll()
        {
            TestContext.WriteLine("Testing scrolling of circular string");
            DoTestScroll(CoordinateArraySequenceFactory.Instance, 2);
            DoTestScroll(CoordinateArraySequenceFactory.Instance, 3);
            DoTestScroll(PackedCoordinateSequenceFactory.DoubleFactory, 2);
            DoTestScroll(PackedCoordinateSequenceFactory.DoubleFactory, 4);
            DoTestScroll(PackedCoordinateSequenceFactory.FloatFactory, 2);
            DoTestScroll(PackedCoordinateSequenceFactory.FloatFactory, 4);
            DoTestScroll(DotSpatialAffineCoordinateSequenceFactory.Instance, 2);
            DoTestScroll(DotSpatialAffineCoordinateSequenceFactory.Instance, 4);
        }

        [Test]
        public void TestIndexOf()
        {
            TestContext.WriteLine("Testing indexOf");
            DoTestIndexOf(CoordinateArraySequenceFactory.Instance, 2);
            DoTestIndexOf(PackedCoordinateSequenceFactory.DoubleFactory, 5);
            DoTestIndexOf(PackedCoordinateSequenceFactory.FloatFactory, 7);
            DoTestIndexOf(DotSpatialAffineCoordinateSequenceFactory.Instance, 4);
        }

        [Test]
        public void TestMinCoordinateIndex()
        {
            TestContext.WriteLine("Testing minCoordinateIndex");
            DoTestMinCoordinateIndex(CoordinateArraySequenceFactory.Instance, 2);
            DoTestMinCoordinateIndex(PackedCoordinateSequenceFactory.DoubleFactory, 5);
            DoTestMinCoordinateIndex(PackedCoordinateSequenceFactory.FloatFactory, 7);
            DoTestMinCoordinateIndex(DotSpatialAffineCoordinateSequenceFactory.Instance, 4);
        }

        [Test]
        public void TestIsRing()
        {
            TestContext.WriteLine("Testing isRing");
            DoTestIsRing(CoordinateArraySequenceFactory.Instance, 2);
            DoTestIsRing(PackedCoordinateSequenceFactory.DoubleFactory, 5);
            DoTestIsRing(PackedCoordinateSequenceFactory.FloatFactory, 7);
            DoTestIsRing(DotSpatialAffineCoordinateSequenceFactory.Instance, 4);
        }

        [Test]
        public void TestCopy()
        {
            TestContext.WriteLine("Testing copy");
            DoTestCopy(CoordinateArraySequenceFactory.Instance, 2);
            DoTestCopy(PackedCoordinateSequenceFactory.DoubleFactory, 5);
            DoTestCopy(PackedCoordinateSequenceFactory.FloatFactory, 7);
            DoTestCopy(DotSpatialAffineCoordinateSequenceFactory.Instance, 4);
        }

        [TestCaseSource(nameof(CsFactories))]
        public void TestCopyDifferentDim(CoordinateSequenceFactory csFactory)
        {
            TestContext.WriteLine($"Testing copy with different dimensions using {csFactory}");
            DoTestCopyDifferentDim(csFactory);
        }


        [TestCaseSource(nameof(CsFactories))]
        public void TestIsEqual(CoordinateSequenceFactory csFactory)
        {
            TestContext.WriteLine("Testing equality");
            int dim1 = 2, dim2 = 4, measures1 = 0, measures2 = 1;
            if (csFactory is PackedCoordinateSequenceFactory) {
                dim1 = 7; measures1 = 3;
                dim2 = 9; measures2 = 2;
            }
            var seq1 = CreateTestSequenceM(csFactory, 20, dim1, measures1);
            var seq2 = CreateTestSequenceM(csFactory, 20, dim2, measures2);

            Assert.That(CoordinateSequences.IsEqual(seq1, seq2), Is.True);
            Assert.That(CoordinateSequences.IsEqualAt(seq1, 4, seq2, 4), Is.True);
            Assert.That(CoordinateSequences.IsEqualAt(seq1, 5, seq2, 4), Is.False);
        }

        [Test]
        public void TestReverse()
        {
            TestContext.WriteLine("Testing reverse");
            DoTestReverse(CoordinateArraySequenceFactory.Instance, 2);
            DoTestReverse(PackedCoordinateSequenceFactory.DoubleFactory, 5);
            DoTestReverse(PackedCoordinateSequenceFactory.FloatFactory, 7);
        }

        private static CoordinateSequence CreateSequenceFromOrdinates(CoordinateSequenceFactory csFactory, int dim, int measures)
        {
            var sequence = csFactory.Create(ordinateValues.Length, dim, measures);
            for (int i = 0; i < ordinateValues.Length; i++)
            {
                sequence.SetOrdinate(i, Ordinate.X, ordinateValues[i][0]);
                sequence.SetOrdinate(i, Ordinate.Y, ordinateValues[i][1]);
            }
            return FillNonPlanarDimensions(sequence);
        }

        private static CoordinateSequence CreateTestSequence(CoordinateSequenceFactory csFactory, int size, int dim, int measures)
        {
            var cs = csFactory.Create(size, dim, measures);
            // initialize with a data signature where coords look like [1, 10, 100, ...]
            for (int i = 0; i < size; i++)
                for (int d = 0; d < dim; d++)
                    cs.SetOrdinate(i, d, i*Math.Pow(10, d));
            return cs;
        }

        private static CoordinateSequence CreateTestSequenceM(CoordinateSequenceFactory csFactory, int size, int dim,
            int measures)
        {
            var cs = csFactory.Create(size, dim, measures);
            // initialize with a data signature where coords look like [1, 10, 100, ...]
            int spatial = dim - measures;
            for (int i = 0; i < size; i++) {
                for (int d = 0; d < spatial; d++)
                    cs.SetOrdinate(i, d, i * Math.Pow(10, d));
                for (int d = 0; d < measures; d++)
                    cs.SetOrdinate(i, spatial + d, i * 100 + d + 1);
            }
        
            return cs;
        }

        private static void DoTestReverse(CoordinateSequenceFactory factory, int dimension)
        {

            // arrange
            var sequence = CreateSequenceFromOrdinates(factory, dimension, 0);
            var reversed = sequence.Copy();

            // act
            CoordinateSequences.Reverse(reversed);

            // assert
            for (int i = 0; i < sequence.Count; i++)
                CheckCoordinateAt(sequence, i, reversed, sequence.Count - i - 1, dimension);
        }

        private static void DoTestCopy(CoordinateSequenceFactory factory, int dimension)
        {

            // arrange
            var sequence = CreateSequenceFromOrdinates(factory, dimension, 0);
            if (sequence.Count <= 7)
            {
                TestContext.WriteLine("sequence has a size of " + sequence.Count + ". Execution of this test needs a sequence " +
                        "with more than 6 coordinates.");
                return;
            }

            var fullCopy = factory.Create(sequence.Count, dimension, 0);
            var partialCopy = factory.Create(sequence.Count - 5, dimension, 0);

            // act
            CoordinateSequences.Copy(sequence, 0, fullCopy, 0, sequence.Count);
            CoordinateSequences.Copy(sequence, 2, partialCopy, 0, partialCopy.Count);

            // assert
            for (int i = 0; i < fullCopy.Count; i++)
                CheckCoordinateAt(sequence, i, fullCopy, i, dimension);
            for (int i = 0; i < partialCopy.Count; i++)
                CheckCoordinateAt(sequence, 2 + i, partialCopy, i, dimension);

            // ToDo test if dimensions don't match
        }

        private static void DoTestCopyDifferentDim(CoordinateSequenceFactory factory)
        {

            // arrange
            var sequence = CreateSequenceFromOrdinates(factory, 4, 1);
            if (sequence.Count <= 7)
            {
                TestContext.WriteLine("sequence has a size of " + sequence.Count + ". Execution of this test needs a sequence " +
                                      "with more than 6 coordinates.");
                return;
            }

            var fullCopy = factory.Create(sequence.Count, 3, 1);
            var partialCopy = factory.Create(sequence.Count - 5, 3, 1);

            // act
            CoordinateSequences.Copy(sequence, 0, fullCopy, 0, sequence.Count);
            CoordinateSequences.Copy(sequence, 2, partialCopy, 0, partialCopy.Count);

            // assert
            for (int i = 0; i < fullCopy.Count; i++)
            {
                CheckCoordinateAt(sequence, i, fullCopy, i, Ordinates.XYM);
            }

            for (int i = 0; i < partialCopy.Count; i++)
            {
                CheckCoordinateAt(sequence, 2 + i, partialCopy, i, Ordinates.XYM);
            }

        }

        private static void DoTestIsRing(CoordinateSequenceFactory factory, int dimension)
        {

            // arrange
            var ring = CreateCircle(factory, dimension, new Coordinate(), 5);
            var noRing = CreateCircularString(factory, dimension, new Coordinate(), 5,
                    0.1, 22);
            var empty = CreateAlmostRing(factory, dimension, 0);
            var incomplete1 = CreateAlmostRing(factory, dimension, 1);
            var incomplete2 = CreateAlmostRing(factory, dimension, 2);
            var incomplete3 = CreateAlmostRing(factory, dimension, 3);
            var incomplete4a = CreateAlmostRing(factory, dimension, 4);
            var incomplete4b = CoordinateSequences.EnsureValidRing(factory, incomplete4a);

            // act
            bool isRingRing = CoordinateSequences.IsRing(ring);
            bool isRingNoRing = CoordinateSequences.IsRing(noRing);
            bool isRingEmpty = CoordinateSequences.IsRing(empty);
            bool isRingIncomplete1 = CoordinateSequences.IsRing(incomplete1);
            bool isRingIncomplete2 = CoordinateSequences.IsRing(incomplete2);
            bool isRingIncomplete3 = CoordinateSequences.IsRing(incomplete3);
            bool isRingIncomplete4a = CoordinateSequences.IsRing(incomplete4a);
            bool isRingIncomplete4b = CoordinateSequences.IsRing(incomplete4b);

            // assert
            Assert.IsTrue(isRingRing);
            Assert.IsTrue(!isRingNoRing);
            Assert.IsTrue(isRingEmpty);
            Assert.IsTrue(!isRingIncomplete1);
            Assert.IsTrue(!isRingIncomplete2);
            Assert.IsTrue(!isRingIncomplete3);
            Assert.IsTrue(!isRingIncomplete4a);
            Assert.IsTrue(isRingIncomplete4b);
        }

        private static void DoTestIndexOf(CoordinateSequenceFactory factory, int dimension)
        {
            // arrange
            var sequence = CreateSequenceFromOrdinates(factory, dimension, 0);

            // act & assert
            var coordinates = sequence.ToCoordinateArray();
            for (int i = 0; i < sequence.Count; i++)
                Assert.AreEqual(i, CoordinateSequences.IndexOf(coordinates[i], sequence));

        }

        private static void DoTestMinCoordinateIndex(CoordinateSequenceFactory factory, int dimension)
        {

            var sequence = CreateSequenceFromOrdinates(factory, dimension, 0);
            if (sequence.Count <= 6)
            {
                TestContext.WriteLine("sequence has a size of " + sequence.Count + ". Execution of this test needs a sequence " +
                        "with more than 5 coordinates.");
                return;
            }

            int minIndex = sequence.Count / 2;
            sequence.SetOrdinate(minIndex, 0, 5);
            sequence.SetOrdinate(minIndex, 1, 5);

            Assert.AreEqual(minIndex, CoordinateSequences.MinCoordinateIndex(sequence));
            Assert.AreEqual(minIndex, CoordinateSequences.MinCoordinateIndex(sequence, 2, sequence.Count - 2));

        }

        private static void DoTestScroll(CoordinateSequenceFactory factory, int dimension)
        {

            // arrange
            var sequence = CreateCircularString(factory, dimension, new Coordinate(20, 20), 7d,
                    0.1, 22);
            var scrolled = sequence.Copy();

            // act
            CoordinateSequences.Scroll(scrolled, 12);

            // assert
            int io = 12;
            for (int @is = 0; @is < scrolled.Count - 1; @is++)
            {
                CheckCoordinateAt(sequence, io, scrolled, @is, dimension);
                io++;
                io %= scrolled.Count;
            }
        }

        private static void DoTestScrollRing(CoordinateSequenceFactory factory, int dimension)
        {

            // arrange
            //TestContext.WriteLine("Testing '" + factory.getClass().getSimpleName() + "' with dim=" +dimension );
            var sequence = CreateCircle(factory, dimension, new Coordinate(10, 10), 9d);
            var scrolled = sequence.Copy();

            // act
            CoordinateSequences.Scroll(scrolled, 12);

            // assert
            int io = 12;
            for (int @is = 0; @is < scrolled.Count - 1; @is++)
            {
                CheckCoordinateAt(sequence, io, scrolled, @is, dimension);
                io++;
                io %= scrolled.Count - 1;
            }
            CheckCoordinateAt(scrolled, 0, scrolled, scrolled.Count - 1, dimension);
        }

        private static void CheckCoordinateAt(CoordinateSequence seq1, int pos1,
                                              CoordinateSequence seq2, int pos2, int dim)
        {
            Assert.AreEqual(seq1.GetOrdinate(pos1, Ordinate.X), seq2.GetOrdinate(pos2, Ordinate.X),
                "unexpected x-ordinate at pos " + pos2);
            Assert.AreEqual(seq1.GetOrdinate(pos1, Ordinate.Y), seq2.GetOrdinate(pos2, Ordinate.Y),
                "unexpected y-ordinate at pos " + pos2);

            // check additional ordinates
            for (int j = 2; j < dim; j++)
            {
                Assert.AreEqual(seq1.GetOrdinate(pos1, j), seq2.GetOrdinate(pos2, j),
                    "unexpected " + j + "-ordinate at pos " + pos2);
            }
        }

        private static void CheckCoordinateAt(CoordinateSequence seq1, int pos1,
            CoordinateSequence seq2, int pos2, Ordinates toCheck)
        {
            Assert.AreEqual(seq1.GetOrdinate(pos1, Ordinate.X), seq2.GetOrdinate(pos2, Ordinate.X),
                "unexpected x-ordinate at pos " + pos2);
            Assert.AreEqual(seq1.GetOrdinate(pos1, Ordinate.Y), seq2.GetOrdinate(pos2, Ordinate.Y),
                "unexpected y-ordinate at pos " + pos2);

            // check additional ordinates
            for (var j = Ordinate.Spatial3; j <= Ordinate.Measure16; j++)
            {
                var coFlag = (Ordinates)(1 << (int)j);
                if ((coFlag & toCheck) == coFlag)
                {
                    Assert.AreEqual(seq1.GetOrdinate(pos1, j), seq2.GetOrdinate(pos2, j),
                        "unexpected " + j + "-ordinate at pos " + pos2);
                }
            }
        }
        private static CoordinateSequence CreateAlmostRing(CoordinateSequenceFactory factory, int dimension, int num)
        {

            if (num > 4) num = 4;

            var sequence = factory.Create(num, dimension, 0);
            if (num == 0) return FillNonPlanarDimensions(sequence);

            sequence.SetOrdinate(0, Ordinate.X, 10);
            sequence.SetOrdinate(0, Ordinate.Y, 10);
            if (num == 1) return FillNonPlanarDimensions(sequence);

            sequence.SetOrdinate(1, Ordinate.X, 20);
            sequence.SetOrdinate(1, Ordinate.Y, 10);
            if (num == 2) return FillNonPlanarDimensions(sequence);

            sequence.SetOrdinate(2, Ordinate.X, 20);
            sequence.SetOrdinate(2, Ordinate.Y, 20);
            if (num == 3) return FillNonPlanarDimensions(sequence);

            sequence.SetOrdinate(3, Ordinate.X, 10.00001);
            sequence.SetOrdinate(3, Ordinate.Y,  9.99999);
            return FillNonPlanarDimensions(sequence);

        }

        private static CoordinateSequence FillNonPlanarDimensions(CoordinateSequence seq)
        {

            if (seq.Dimension < 3)
                return seq;

            for (int i = 0; i < seq.Count; i++)
                for (int j = 2; j < seq.Dimension; j++)
                    seq.SetOrdinate(i, j, i * Math.Pow(10, j - 1));

            return seq;
        }

        private static CoordinateSequence CreateCircle(CoordinateSequenceFactory factory, int dimension,
                                                       Coordinate center, double radius)
        {
            // Get a complete circular string
            var res = CreateCircularString(factory, dimension, center, radius, 0d, 49);

            // ensure it is closed
            for (int i = 0; i < dimension; i++)
                res.SetOrdinate(48, (Ordinate)i, res.GetOrdinate(0, (Ordinate)i));

            return res;
        }
        private static CoordinateSequence CreateCircularString(CoordinateSequenceFactory factory, int dimension,
                                                               Coordinate center, double radius, double startAngle,
                                                               int numPoints)
        {
            const int numSegmentsCircle = 48;
            const double angleCircle = 2 * Math.PI;
            const double angleStep = angleCircle / numSegmentsCircle;

            var sequence = factory.Create(numPoints, dimension, 0);
            var pm = new PrecisionModel(100);
            double angle = startAngle;
            for (int i = 0; i < numPoints; i++)
            {
                double dx = Math.Cos(angle) * radius;
                sequence.SetOrdinate(i, Ordinate.X, pm.MakePrecise(center.X + dx));
                double dy = Math.Sin(angle) * radius;
                sequence.SetOrdinate(i, Ordinate.Y, pm.MakePrecise(center.Y + dy));

                // set other ordinate values to predictable values
                for (int j = 2; j < dimension; j++)
                    sequence.SetOrdinate(i, j, Math.Pow(10, j - 1) * i);

                angle += angleStep;
                angle %= angleCircle;
            }

            return sequence;
        }

        public static IEnumerable CsFactories
        {
            get
            {
                yield return CoordinateArraySequenceFactory.Instance;
                yield return PackedCoordinateSequenceFactory.DoubleFactory;
                yield return PackedCoordinateSequenceFactory.FloatFactory;
                yield return DotSpatialAffineCoordinateSequenceFactory.Instance;
            }
        }
    }
}
