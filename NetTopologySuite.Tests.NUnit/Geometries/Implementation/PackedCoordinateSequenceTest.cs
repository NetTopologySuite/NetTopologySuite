using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Implementation
{
    //Tests are exposed by CoordinateSequenceTestBase type
    public class PackedCoordinateSequenceTest : CoordinateSequenceTestBase
    {
        public PackedCoordinateSequenceTest()
        {
            base.csFactory = new PackedCoordinateSequenceFactory();
        }

        // TODO: This test was overriden because the base class implementation asserts that the Z ordinate is NaN, but in NTS/JTS 1.9 the result of PackedCoordinateSequence.GetCoordinate method is 0.0
        // This should be removed once the new Coordinate.NULL_ORDINATE result is migrated from JTS
        public override void Test2DZOrdinate()
        {
            Coordinate[] coords = base.CreateArray(SIZE);

            ICoordinateSequence seq = csFactory.Create(SIZE, 2);
            for (int i = 0; i < seq.Count; i++)
            {
                seq.SetOrdinate(i, Ordinate.X, coords[i].X);
                seq.SetOrdinate(i, Ordinate.Y, coords[i].Y);
            }

            for (int i = 0; i < seq.Count; i++)
            {
                Coordinate p = seq.GetCoordinate(i);
                Assert.IsTrue(p.Z == 0.0);
            }
        }
    }
}