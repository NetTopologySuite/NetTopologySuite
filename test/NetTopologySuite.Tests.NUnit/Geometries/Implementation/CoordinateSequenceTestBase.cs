#nullable disable
using NetTopologySuite.Geometries;
using NetTopologySuite.Tests.NUnit.Utilities;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Implementation
{
    /// <summary>
    /// General test cases for CoordinateSequences.
    /// Subclasses can set the factory to test different kinds of CoordinateSequences.
    /// </summary>
    [TestFixture]
    public abstract class CoordinateSequenceTestBase
    {
        protected const int Size = 100;

        protected abstract CoordinateSequenceFactory CsFactory { get; }

        [Test]
        public void TestZeroLength()
        {
            var seq = CsFactory.Create(0, 3);
            Assert.IsTrue(seq.Count == 0);

            var seq2 = CsFactory.Create((Coordinate[])null);
            Assert.IsTrue(seq2.Count == 0);
        }

        [Test]
        public void TestCreateBySizeAndModify()
        {
            var coords = CreateArray(Size);

            var seq = CsFactory.Create(Size, 3);
            for (int i = 0; i < seq.Count; i++)
            {
                seq.SetOrdinate(i, Ordinate.X, coords[i].X);
                seq.SetOrdinate(i, Ordinate.Y, coords[i].Y);
                seq.SetOrdinate(i, Ordinate.Z, coords[i].Z);
            }

            Assert.IsTrue(IsEqual(seq, coords));
        }

        // TODO: This test was marked as virtual to allow PackedCoordinateSequenceTest to override the assert value
        // The method should not be marked as virtual, and should be altered when the correct PackedCoordinateSequence.GetCoordinate result is migrated to NTS
        [Test]
        public virtual void Test2DZOrdinate()
        {
            var coords = CreateArray(Size);

            var seq = CsFactory.Create(Size, 2);
            for (int i = 0; i < seq.Count; i++)
            {
                seq.SetOrdinate(i, Ordinate.X, coords[i].X);
                seq.SetOrdinate(i, Ordinate.Y, coords[i].Y);
            }

            for (int i = 0; i < seq.Count; i++)
            {
                var p = seq.GetCoordinate(i);
                Assert.IsTrue(double.IsNaN(p.Z));
            }
        }

        [Test]
        public void TestCreateByInit()
        {
            var coords = CreateArray(Size);
            var seq = CsFactory.Create(coords);
            Assert.IsTrue(IsEqual(seq, coords));
        }

        [Test]
        public void TestCreateByInitAndCopy()
        {
            var coords = CreateArray(Size);
            var seq = CsFactory.Create(coords);
            var seq2 = CsFactory.Create(seq);
            Assert.IsTrue(IsEqual(seq2, coords));
        }

        [Test]
        public void testSerializable() {
            var coords = CreateArray(Size);
            var seq = CsFactory.Create(coords);
            // throws exception if not serializable
            byte[] data = SerializationUtility.Serialize(seq);
            // check round-trip gives same data
            var seq2 = SerializationUtility.Deserialize<CoordinateSequence>(data);
            Assert.IsTrue(IsEqual(seq2, coords));
        }

        [TestCase(2, 0)] // XY
        [TestCase(3, 0)] // XYZ
        [TestCase(3, 1)] // XYM
        [TestCase(4, 1)] // XYZM
        public void NamedAndOrdinateGettersShouldBeConsistent(int dimension, int measures)
        {
            var seq = CsFactory.Create(1, dimension, measures);
            for (int dim = 0; dim < dimension; dim++)
            {
                seq.SetOrdinate(0, dim, 10 + dim);
            }

            // we just set all the ordinate values to 10 plus their index.
            double expectedX = 10;
            double expectedY = 11;
            double expectedZ = seq.HasZ ? 12 : Coordinate.NullOrdinate;
            double expectedM = seq.HasM ? seq.HasZ ? 13 : 12 : Coordinate.NullOrdinate;

            var c = seq.GetCoordinate(0);

            // X
            Assert.That(seq.GetX(0), Is.EqualTo(expectedX));
            Assert.That(seq.GetOrdinate(0, Ordinate.X), Is.EqualTo(expectedX));
            Assert.That(c.X, Is.EqualTo(expectedX));
            Assert.That(c[Ordinate.X], Is.EqualTo(expectedX));

            // Y
            Assert.That(seq.GetY(0), Is.EqualTo(expectedY));
            Assert.That(seq.GetOrdinate(0, Ordinate.Y), Is.EqualTo(expectedY));
            Assert.That(c.Y, Is.EqualTo(expectedY));
            Assert.That(c[Ordinate.Y], Is.EqualTo(expectedY));

            // Z
            Assert.That(seq.GetZ(0), Is.EqualTo(expectedZ));
            Assert.That(seq.GetOrdinate(0, Ordinate.Z), Is.EqualTo(expectedZ));
            Assert.That(c.Z, Is.EqualTo(expectedZ));
            Assert.That(c[Ordinate.Z], Is.EqualTo(expectedZ));

            // M
            Assert.That(seq.GetM(0), Is.EqualTo(expectedM));
            Assert.That(seq.GetOrdinate(0, Ordinate.M), Is.EqualTo(expectedM));
            Assert.That(c.M, Is.EqualTo(expectedM));
            Assert.That(c[Ordinate.M], Is.EqualTo(expectedM));
        }

        // TODO: This private method was marked as protected to allow PackedCoordinateSequenceTest to override Test2DZOrdinate
        // The method should not be marked as protected, and should be altered when the correct PackedCoordinateSequence.GetCoordinate result is migrated to NTS
        protected Coordinate[] CreateArray(int size)
        {
            var coords = new Coordinate[size];
            for (int i = 0; i < size; i++)
            {
                double baseUnits = 2 * 1;
                coords[i] = new CoordinateZ(baseUnits, baseUnits + 1, baseUnits + 2);
            }
            return coords;
        }

        protected bool IsAllCoordsEqual(CoordinateSequence seq, Coordinate coord)
        {
            for (int i = 0; i < seq.Count; i++)
            {
                if (!coord.Equals(seq.GetCoordinate(i)))
                    return false;

                if (coord.X != seq.GetOrdinate(i, 0))
                    return false;
                if (coord.Y != seq.GetOrdinate(i, 1))
                    return false;
                if (seq.HasZ)
                {
                    if (coord.Z != seq.GetZ(i))
                        return false;
                }
                if (seq.HasM)
                {
                    if (coord.M != seq.GetM(i))
                        return false;
                }
                if (seq.Dimension > 2)
                {
                    if (coord[2] != seq.GetOrdinate(i, 2))
                        return false;
                }
                if (seq.Dimension > 3)
                {
                    if (coord[3] != seq.GetOrdinate(i, 3))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Tests for equality using all supported accessors,
        /// to provides test coverage for them.
        /// </summary>
        /// <param name="seq"></param>
        /// <param name="coords"></param>
        /// <returns></returns>
        protected bool IsEqual(CoordinateSequence seq, Coordinate[] coords)
        {
            if (seq.Count != coords.Length)
                return false;

            // carefully get coordinate of the same type as the sequence
            var p = seq.CreateCoordinate();

            for (int i = 0; i < seq.Count; i++)
            {
                if (!coords[i].Equals(seq.GetCoordinate(i)))
                    return false;

                // Ordinate named getters
                if (!coords[i].X.Equals(seq.GetX(i)))
                    return false;
                if (!coords[i].Y.Equals(seq.GetY(i)))
                    return false;
                if (seq.HasZ)
                {
                    if (!coords[i].Z.Equals(seq.GetZ(i)))
                        return false;
                }
                if (seq.HasM)
                {
                    if (!coords[i].M.Equals(seq.GetM(i)))
                        return false;
                }

                // Ordinate indexed getters
                if (!coords[i].X.Equals(seq.GetOrdinate(i, 0)))
                    return false;
                if (!coords[i].Y.Equals(seq.GetOrdinate(i, 1)))
                    return false;
                if (seq.Dimension > 2)
                {
                    if (!coords[i][2].Equals(seq.GetOrdinate(i, 2)))
                        return false;
                }
                if (seq.Dimension > 3)
                {
                    if (!coords[i][3].Equals(seq.GetOrdinate(i, 3)))
                        return false;
                }

                // Coordinate getter
                seq.GetCoordinate(i, p);
                if (!coords[i].X.Equals(p.X))
                    return false;
                if (!coords[i].Y.Equals(p.Y))
                    return false;
                if (seq.HasZ)
                {
                    if (!coords[i].Z.Equals(p.Z))
                        return false;
                }
                if (seq.HasM)
                {
                    if (!coords[i].M.Equals(p.M))
                        return false;
                }
            }
            return true;
        }
    }
}

