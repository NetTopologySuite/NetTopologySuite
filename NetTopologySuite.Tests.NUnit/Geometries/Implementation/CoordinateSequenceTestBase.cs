using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

/*
 * General test cases for CoordinateSequences.
 * Subclasses can set the factory to test different kinds of CoordinateSequences.
 */
namespace NetTopologySuite.Tests.NUnit.Geometries.Implementation
{
    [TestFixture]
    public abstract class CoordinateSequenceTestBase
    {
        public static int SIZE = 100;

        protected ICoordinateSequenceFactory csFactory;

        [Test]
        public void TestZeroLength()
        {
            ICoordinateSequence seq = csFactory.Create(0, 3);
            Assert.IsTrue(seq.Count == 0);

            ICoordinateSequence seq2 = csFactory.Create((Coordinate[])null);
            Assert.IsTrue(seq2.Count == 0);
        }

        [Test]
        public void TestCreateBySizeAndModify()
        {
            Coordinate[] coords = CreateArray(SIZE);

            ICoordinateSequence seq = csFactory.Create(SIZE, 3);
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
            Coordinate[] coords = CreateArray(SIZE);

            ICoordinateSequence seq = csFactory.Create(SIZE, 2);
            for (int i = 0; i < seq.Count; i++)
            {
                seq.SetOrdinate(i, Ordinate.X, coords[i].X);
                seq.SetOrdinate(i, Ordinate.Y, coords[i].Y);
            }

            for (int i = 0; i < seq.Count; i++)
            {
                Coordinate p = seq.GetCoordinate(i);
                Assert.IsTrue(Double.IsNaN(p.Z));
            }
        }

        [Test]
        public void TestCreateByInit()
        {
            Coordinate[] coords = CreateArray(SIZE);
            ICoordinateSequence seq = csFactory.Create(coords);
            Assert.IsTrue(IsEqual(seq, coords));
        }

        [Test]
        public void TestCreateByInitAndCopy()
        {
            Coordinate[] coords = CreateArray(SIZE);
            ICoordinateSequence seq = csFactory.Create(coords);
            ICoordinateSequence seq2 = csFactory.Create(seq);
            Assert.IsTrue(IsEqual(seq2, coords));
        }

        // TODO: This private method was marked as protected to allow PackedCoordinateSequenceTest to override Test2DZOrdinate
        // The method should not be marked as protected, and should be altered when the correct PackedCoordinateSequence.GetCoordinate result is migrated to NTS
        protected Coordinate[] CreateArray(int size)
        {
            Coordinate[] coords = new Coordinate[size];
            for (int i = 0; i < size; i++) {
                double baseUnits = 2 * 1;
                coords[i] = new Coordinate(baseUnits, baseUnits + 1, baseUnits + 2);
            }
            return coords;
        }

        bool IsAllCoordsEqual(ICoordinateSequence seq, Coordinate coord)
        {
            for (int i = 0; i < seq.Count; i++)
            {
                if (!coord.Equals(seq.GetCoordinate(i))) return false;

                if (coord.X != seq.GetOrdinate(i, Ordinate.X)) return false;
                if (coord.Y != seq.GetOrdinate(i, Ordinate.Y)) return false;
                if (coord.Z != seq.GetOrdinate(i, Ordinate.Z)) return false;
            }
            return true;
        }

        /*
        * Tests for equality using all supported accessors,
        * to provides test coverage for them.
        *
        * @param seq
        * @param coords
        * @return
        */
        bool IsEqual(ICoordinateSequence seq, Coordinate[] coords)
        {
            if (seq.Count != coords.Length) return false;

            Coordinate p = new Coordinate();

            for (int i = 0; i < seq.Count; i++)
            {
                if (!coords[i].Equals(seq.GetCoordinate(i))) return false;

                // Ordinate named getters
                if (coords[i].X != seq.GetX(i)) return false;
                if (coords[i].Y != seq.GetY(i)) return false;

                // Ordinate indexed getters
                if (coords[i].X != seq.GetOrdinate(i, Ordinate.X)) return false;
                if (coords[i].Y != seq.GetOrdinate(i, Ordinate.Y)) return false;
                if (coords[i].Z != seq.GetOrdinate(i, Ordinate.Z)) return false;

                // Coordinate getter
                seq.GetCoordinate(i, p);
                if (coords[i].X != p.X) return false;
                if (coords[i].Y != p.Y) return false;
                //TODO: Remove commented line below when NTS supports Z ordinates in CoordinateArraySequence.GetCoordinate and PackedCoordinateSequence.GetCoordinate
                //if (coords[i].Z != p.Z) return false;

            }
            return true;
        }
    }
}

