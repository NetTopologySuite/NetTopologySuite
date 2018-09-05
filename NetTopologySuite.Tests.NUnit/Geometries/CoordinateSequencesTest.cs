using System;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixtureAttribute]
    public class CoordinateSequencesTest
    {

        private static readonly double[][] ordinateValues = {
            new[]{75.76,77.43},new[]{41.35,90.75},new[]{73.74,41.67},new[]{20.87,86.49},new[]{17.49,93.59},new[]{67.75,80.63},
            new[]{63.01,52.57},new[]{32.9,44.44}, new[]{79.36,29.8}, new[]{38.17,88.0}, new[]{19.31,49.71},new[]{57.03,19.28},
            new[]{63.76,77.35},new[]{45.26,85.15},new[]{51.71,50.38},new[]{92.16,19.85},new[]{64.18,27.7}, new[]{64.74,65.1},
            new[]{80.07,13.55},new[]{55.54,94.07}};
        
        [TestAttribute]
        public void TestCopyToLargerDim()
        {
            var csFactory = new PackedCoordinateSequenceFactory();
            var cs2D = CreateTestSequence(csFactory, 10, 2);
            var cs3D = csFactory.Create(10, 3);
            CoordinateSequences.Copy(cs2D, 0, cs3D, 0, cs3D.Count);
            Assert.IsTrue(CoordinateSequences.IsEqual(cs2D, cs3D));
        }

        [TestAttribute]
        public void TestCopyToSmallerDim()
        {
            var csFactory = new PackedCoordinateSequenceFactory();
            var cs3D = CreateTestSequence(csFactory, 10, 3);
            var cs2D = csFactory.Create(10, 2);
            CoordinateSequences.Copy(cs3D, 0, cs2D, 0, cs2D.Count);
            Assert.IsTrue(CoordinateSequences.IsEqual(cs2D, cs3D));
        }


        [TestAttribute]
        public void TestScrollRing()
        {
            Console.WriteLine("Testing scrolling of closed ring");
            DoTestScrollRing(CoordinateArraySequenceFactory.Instance, 2);
            DoTestScrollRing(CoordinateArraySequenceFactory.Instance, 3);
            DoTestScrollRing(PackedCoordinateSequenceFactory.DoubleFactory, 2);
            DoTestScrollRing(PackedCoordinateSequenceFactory.DoubleFactory, 4);
            DoTestScrollRing(PackedCoordinateSequenceFactory.FloatFactory, 2);
            DoTestScrollRing(PackedCoordinateSequenceFactory.FloatFactory, 4);
        }

        [TestAttribute]
        public void TestScroll()
        {
            Console.WriteLine("Testing scrolling of circular string");
            DoTestScroll(CoordinateArraySequenceFactory.Instance, 2);
            DoTestScroll(CoordinateArraySequenceFactory.Instance, 3);
            DoTestScroll(PackedCoordinateSequenceFactory.DoubleFactory, 2);
            DoTestScroll(PackedCoordinateSequenceFactory.DoubleFactory, 4);
            DoTestScroll(PackedCoordinateSequenceFactory.FloatFactory, 2);
            DoTestScroll(PackedCoordinateSequenceFactory.FloatFactory, 4);
        }

        [TestAttribute]
        public void TestIndexOf()
        {
            Console.WriteLine("Testing indexOf");
            DoTestIndexOf(CoordinateArraySequenceFactory.Instance, 2);
            DoTestIndexOf(PackedCoordinateSequenceFactory.DoubleFactory, 5);
            DoTestIndexOf(PackedCoordinateSequenceFactory.FloatFactory, 7);
        }

        [TestAttribute]
        public void TestMinCoordinateIndex()
        {
            Console.WriteLine("Testing minCoordinateIndex");
            DoTestMinCoordinateIndex(CoordinateArraySequenceFactory.Instance, 2);
            DoTestMinCoordinateIndex(PackedCoordinateSequenceFactory.DoubleFactory, 5);
            DoTestMinCoordinateIndex(PackedCoordinateSequenceFactory.FloatFactory, 7);
        }

        [TestAttribute]
        public void TestIsRing()
        {
            Console.WriteLine("Testing isRing");
            DoTestIsRing(CoordinateArraySequenceFactory.Instance, 2);
            DoTestIsRing(PackedCoordinateSequenceFactory.DoubleFactory, 5);
            DoTestIsRing(PackedCoordinateSequenceFactory.FloatFactory, 7);
        }

        [TestAttribute]
        public void TestCopy()
        {
            Console.WriteLine("Testing copy");
            DoTestCopy(CoordinateArraySequenceFactory.Instance, 2);
            DoTestCopy(PackedCoordinateSequenceFactory.DoubleFactory, 5);
            DoTestCopy(PackedCoordinateSequenceFactory.FloatFactory, 7);
        }

        [TestAttribute]
        public void TestReverse()
        {
            Console.WriteLine("Testing reverse");
            DoTestReverse(CoordinateArraySequenceFactory.Instance, 2);
            DoTestReverse(PackedCoordinateSequenceFactory.DoubleFactory, 5);
            DoTestReverse(PackedCoordinateSequenceFactory.FloatFactory, 7);
        }

        /**
         * Method used to create a {@link this.ordinateValues}.
         * Usage: remove first 't' and run as unit test.
         * Note: When parameters are changed, some unit tests may need to be
         * changed, too. <p>
         * This is especially true for the (@link testMinCoordinateIndex) test,
         * which assumes that the coordinates in the sequence are all within an
         * envelope of [Env(10, 100, 10, 100)].
         * </p>.
         *
         * @deprecated only use to update {@link this.ordinateValues}
         */
        [TestAttribute, Ignore("")]
        [Obsolete]
        public void TestCreateRandomOrdinates()
        {
            var sequence = CreateRandomTestSequence(CoordinateArraySequenceFactory.Instance, 20,
                    2, new Random(7),
                    new Envelope(10, 100, 10, 100), new PrecisionModel(100));

            var ordinates = new StringBuilder("\tprivate static readonly double[][] ordinateValues = {");
            for (int i = 0; i < sequence.Count; i++)
            {
                if (i % 6 == 0) ordinates.Append("\n\t\t");
                ordinates.Append("new[]{");
                ordinates.Append(sequence.GetOrdinate(i, Ordinate.X));
                ordinates.Append(',');
                ordinates.Append(sequence.GetOrdinate(i, Ordinate.Y));
                if (i < sequence.Count - 1) ordinates.Append("},"); else ordinates.Append('}');
            }
            ordinates.Append("};");

            Console.WriteLine(ordinates.ToString());
            Assert.IsTrue(true);
        }

        private static ICoordinateSequence CreateSequenceFromOrdinates(ICoordinateSequenceFactory csFactory, int dim)
        {
            var sequence = csFactory.Create(ordinateValues.Length, dim);
            for (int i = 0; i < ordinateValues.Length; i++)
            {
                sequence.SetOrdinate(i, Ordinate.X, ordinateValues[i][0]);
                sequence.SetOrdinate(i, Ordinate.Y, ordinateValues[i][1]);
            }
            return FillNonPlanarDimensions(sequence);
        }

        private static ICoordinateSequence CreateTestSequence(ICoordinateSequenceFactory csFactory, int size, int dim)
        {
            var cs = csFactory.Create(size, dim);
            // initialize with a data signature where coords look like [1, 10, 100, ...]
            for (int i = 0; i < size; i++)
                for (int d = 0; d < dim; d++)
                    cs.SetOrdinate(i, (Ordinate) d, i*Math.Pow(10, d));
            return cs;
        }


        /**
         * Deprecated only use to update in conjunction with <see cref="TestCreateRandomOrdinates" />
         */
        [Obsolete]
        private static ICoordinateSequence CreateRandomTestSequence(ICoordinateSequenceFactory csFactory, int size, int dim,
                                                         Random rnd, Envelope range, PrecisionModel pm)
        {
            var cs = csFactory.Create(size, dim);
            for (int i = 0; i < size; i++)
            {
                cs.SetOrdinate(i, Ordinate.X, pm.MakePrecise(range.Width * rnd.NextDouble() + range.MinX));
                cs.SetOrdinate(i, Ordinate.Y, pm.MakePrecise(range.Height * rnd.NextDouble() + range.MinY));
            }

            return FillNonPlanarDimensions(cs);
        }

        private static void DoTestReverse(ICoordinateSequenceFactory factory, int dimension)
        {

            // arrange
            var sequence = CreateSequenceFromOrdinates(factory, dimension);
            var reversed = sequence.Copy();

            // act
            CoordinateSequences.Reverse(reversed);

            // assert
            for (int i = 0; i < sequence.Count; i++)
                CheckCoordinateAt(sequence, i, reversed, sequence.Count - i - 1, dimension);
        }

        private static void DoTestCopy(ICoordinateSequenceFactory factory, int dimension)
        {

            // arrange
            var sequence = CreateSequenceFromOrdinates(factory, dimension);
            if (sequence.Count <= 7)
            {
                Console.WriteLine("sequence has a size of " + sequence.Count + ". Execution of this test needs a sequence " +
                        "with more than 6 coordinates.");
                return;
            }

            var fullCopy = factory.Create(sequence.Count, dimension);
            var partialCopy = factory.Create(sequence.Count - 5, dimension);

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

        private static void DoTestIsRing(ICoordinateSequenceFactory factory, int dimension)
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

        private static void DoTestIndexOf(ICoordinateSequenceFactory factory, int dimension)
        {
            // arrange
            var sequence = CreateSequenceFromOrdinates(factory, dimension);

            // act & assert
            var coordinates = sequence.ToCoordinateArray();
            for (int i = 0; i < sequence.Count; i++)
                Assert.AreEqual(i, CoordinateSequences.IndexOf(coordinates[i], sequence));

        }

        private static void DoTestMinCoordinateIndex(ICoordinateSequenceFactory factory, int dimension)
        {

            var sequence = CreateSequenceFromOrdinates(factory, dimension);
            if (sequence.Count <= 6)
            {
                Console.WriteLine("sequence has a size of " + sequence.Count + ". Execution of this test needs a sequence " +
                        "with more than 5 coordinates.");
                return;
            }

            int minIndex = sequence.Count / 2;
            sequence.SetOrdinate(minIndex, (Ordinate)0, 5);
            sequence.SetOrdinate(minIndex, (Ordinate)1, 5);

            Assert.AreEqual(minIndex, CoordinateSequences.MinCoordinateIndex(sequence));
            Assert.AreEqual(minIndex, CoordinateSequences.MinCoordinateIndex(sequence, 2, sequence.Count - 2));

        }

        private static void DoTestScroll(ICoordinateSequenceFactory factory, int dimension)
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

        private static void DoTestScrollRing(ICoordinateSequenceFactory factory, int dimension)
        {

            // arrange
            //Console.WriteLine("Testing '" + factory.getClass().getSimpleName() + "' with dim=" +dimension );
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

        private static void CheckCoordinateAt(ICoordinateSequence seq1, int pos1,
                                              ICoordinateSequence seq2, int pos2, int dim)
        {
            Assert.AreEqual(seq1.GetOrdinate(pos1, Ordinate.X), seq2.GetOrdinate(pos2, Ordinate.X),
                "unexpected x-ordinate at pos " + pos2);
            Assert.AreEqual(seq1.GetOrdinate(pos1, Ordinate.Y), seq2.GetOrdinate(pos2, Ordinate.Y),
                "unexpected y-ordinate at pos " + pos2);

            // check additional ordinates
            for (int j = 2; j < dim; j++)
            {
                Assert.AreEqual(seq1.GetOrdinate(pos1, (Ordinate)j), seq2.GetOrdinate(pos2, (Ordinate)j),
                    "unexpected " + j + "-ordinate at pos " + pos2);
            }
        }

        private static ICoordinateSequence CreateAlmostRing(ICoordinateSequenceFactory factory, int dimension, int num)
        {

            if (num > 4) num = 4;

            var sequence = factory.Create(num, dimension);
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

        private static ICoordinateSequence FillNonPlanarDimensions(ICoordinateSequence seq)
        {

            if (seq.Dimension < 3)
                return seq;

            for (int i = 0; i < seq.Count; i++)
                for (int j = 2; j < seq.Dimension; j++)
                    seq.SetOrdinate(i, (Ordinate)j, i * Math.Pow(10, j - 1));

            return seq;
        }

        private static ICoordinateSequence CreateCircle(ICoordinateSequenceFactory factory, int dimension,
                                                       Coordinate center, double radius)
        {
            // Get a complete circular string
            var res = CreateCircularString(factory, dimension, center, radius, 0d, 49);

            // ensure it is closed
            for (int i = 0; i < dimension; i++)
                res.SetOrdinate(48, (Ordinate)i, res.GetOrdinate(0, (Ordinate)i));

            return res;
        }
        private static ICoordinateSequence CreateCircularString(ICoordinateSequenceFactory factory, int dimension,
                                                               Coordinate center, double radius, double startAngle,
                                                               int numPoints)
        {
            const int numSegmentsCircle = 48;
            const double angleCircle = 2 * Math.PI;
            const double angleStep = angleCircle / numSegmentsCircle;

            var sequence = factory.Create(numPoints, dimension);
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
                    sequence.SetOrdinate(i, (Ordinate)j, Math.Pow(10, j - 1) * i);

                angle += angleStep;
                angle %= angleCircle;
            }

            return sequence;
        }
    }
}
