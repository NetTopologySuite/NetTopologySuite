using System;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixture]
    public class CoordinateArraysTest
    {
        [Test]
        public void TestPtNotInList1()
        {
            var list = CoordinateArrays.PointNotInList(
                new[] { new Coordinate(1, 1), new Coordinate(2, 2), new Coordinate(3, 3) },
                new[] { new Coordinate(1, 1), new Coordinate(1, 2), new Coordinate(1, 3) }
                );
            Assert.IsTrue(list.Equals2D(new Coordinate(2, 2)));
        }

        [Test]
        public void TestPtNotInList2()
        {
            var list = CoordinateArrays.PointNotInList(
                new[] { new Coordinate(1, 1), new Coordinate(2, 2), new Coordinate(3, 3) },
                new[] { new Coordinate(1, 1), new Coordinate(2, 2), new Coordinate(3, 3) }
                );
            Assert.IsTrue(list == null);
        }

        private static readonly Coordinate[] Coords1 = { new Coordinate(1, 1), new Coordinate(2, 2), new Coordinate(3, 3) };
        private static readonly Coordinate[] Empty = new Coordinate[0];

        [Test]
        public void TestEnvelope1()
        {
            Assert.AreEqual(CoordinateArrays.Envelope(Coords1), new Envelope(1, 3, 1, 3));
        }

        [Test]
        public void TestEnvelopeEmpty()
        {
            Assert.AreEqual(CoordinateArrays.Envelope(Empty), new Envelope());
        }

        [Test]
        public void TestIntersection_envelope1()
        {
            Assert.IsTrue(CoordinateArrays.Equals(
                CoordinateArrays.Intersection(Coords1, new Envelope(1, 2, 1, 2)),
                new[] { new Coordinate(1, 1), new Coordinate(2, 2) }
                ));
        }

        [Test]
        public void TestIntersection_envelopeDisjoint()
        {
            Assert.IsTrue(CoordinateArrays.Equals(
                CoordinateArrays.Intersection(Coords1, new Envelope(10, 20, 10, 20)), Empty)
                );
        }

        [Test]
        public void TestIntersection_empty_envelope()
        {
            Assert.IsTrue(CoordinateArrays.Equals(
                CoordinateArrays.Intersection(Empty, new Envelope(1, 2, 1, 2)), Empty)
                );
        }

        [Test]
        public void TestIntersection_coords_emptyEnvelope()
        {
            Assert.IsTrue(CoordinateArrays.Equals(
                CoordinateArrays.Intersection(Coords1, new Envelope()), Empty)
                );
        }

        [Test]
        public void TestScrollRing()
        {
            // arrange
            var sequence = CreateCircle(new Coordinate(10, 10), 9d);
            var scrolled = CreateCircle(new Coordinate(10, 10), 9d);

            // act
            CoordinateArrays.Scroll(scrolled, 12);

            // assert
            int io = 12;
            for (int isc = 0; isc < scrolled.Length - 1; isc++)
            {
                CheckCoordinateAt(sequence, io, scrolled, isc);
                io++;
                io %= scrolled.Length - 1;
            }
            CheckCoordinateAt(scrolled, 0, scrolled, scrolled.Length - 1);
        }

        [Test]
        public void TestScroll()
        {
            // arrange
            var sequence = CreateCircularString(new Coordinate(20, 20), 7d,
              0.1, 22);
            var scrolled = CreateCircularString(new Coordinate(20, 20), 7d,
              0.1, 22); ;

            // act
            CoordinateArrays.Scroll(scrolled, 12);

            // assert
            int io = 12;
            for (int isc = 0; isc < scrolled.Length - 1; isc++)
            {
                CheckCoordinateAt(sequence, io, scrolled, isc);
                io++;
                io %= scrolled.Length;
            }
        }

        [Test]
        public void TestEnforceConsistency()
        {
            var array = new Coordinate[]{ new CoordinateZ(1.0, 1.0, 2.0), new CoordinateM(2.0,2.0,1.0) };
            var array2 = new Coordinate[]{ new Coordinate(1.0, 1.0), new Coordinate(2.0,2.0) };
            // process into array with dimension 4 and measures 1
            CoordinateArrays.EnforceConsistency(array);
            Assert.AreEqual(4, CoordinateArrays.Dimension(array));
            Assert.AreEqual(1, CoordinateArrays.Measures(array));
            Assert.AreEqual(2d, array[0].Z);
            Assert.AreEqual(double.NaN, array[0].M);
            Assert.AreEqual(double.NaN, array[1].Z);
            Assert.AreEqual(1d, array[1].M);

            CoordinateArrays.EnforceConsistency(array2);

            var @fixed = CoordinateArrays.EnforceConsistency(array2, 2, 0);
            Assert.AreSame(@fixed, array2) ; // no processing required

            @fixed = CoordinateArrays.EnforceConsistency(array, 3, 0);
            Assert.AreNotSame(@fixed, array); // copied into new array
            Assert.AreNotSame(array[0], @fixed[0]) ; // processing needed to CoordinateXYZ
            Assert.AreNotSame(array[1], @fixed[1]) ; // processing needed to CoordinateXYZ
        }

        private static void CheckCoordinateAt(Coordinate[] seq1, int pos1,
                                              Coordinate[] seq2, int pos2)
        {
            Coordinate c1 = seq1[pos1], c2 = seq2[pos2];

            Assert.That(c1.X, Is.EqualTo(c2.X), "unexpected x-ordinate at pos " + pos2);
            Assert.That(c1.Y, Is.EqualTo(c2.Y), "unexpected y-ordinate at pos " + pos2);
        }

        private static Coordinate[] CreateCircle(Coordinate center, double radius)
        {
            // Get a complete circular string
            var res = CreateCircularString(center, radius, 0d, 49);

            // ensure it is closed
            res[48] = res[0].Copy();

            return res;
        }
        private static Coordinate[] CreateCircularString(Coordinate center, double radius, double startAngle,
                                                         int numPoints)
        {
            const int numSegmentsCircle = 48;
            const double angleCircle = 2 * Math.PI;
            const double angleStep = angleCircle / numSegmentsCircle;

            var sequence = new Coordinate[numPoints];
            var pm = new PrecisionModel(1000);
            double angle = startAngle;
            for (int i = 0; i < numPoints; i++)
            {
                double dx = Math.Cos(angle) * radius;
                double dy = Math.Sin(angle) * radius;
                sequence[i] = new Coordinate(pm.MakePrecise(center.X + dx), pm.MakePrecise(center.Y + dy));

                angle += angleStep;
                angle %= angleCircle;
            }

            return sequence;
        }

    }
}
