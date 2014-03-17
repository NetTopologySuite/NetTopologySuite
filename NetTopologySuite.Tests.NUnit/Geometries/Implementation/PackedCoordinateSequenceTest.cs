using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Implementation
{
    //Tests are exposed by CoordinateSequenceTestBase type
    public class PackedCoordinateSequenceTest : CoordinateSequenceTestBase
    {
        protected override ICoordinateSequenceFactory CsFactory
        {
            get { return new PackedCoordinateSequenceFactory(); }
        }

        // TODO: This test was overriden because the base class implementation asserts that the Z ordinate is NaN, 
        ///but in NTS/JTS 1.9 the result of PackedCoordinateSequence.GetCoordinate method is 0.0
        // This should be removed once the new Coordinate.NULL_ORDINATE result is migrated from JTS
        [TestAttribute]
        [Ignore("test fail")]
        public override void Test2DZOrdinate()
        {
            Coordinate[] coords = CreateArray(Size);

            ICoordinateSequence seq = CsFactory.Create(Size, 2);
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

        [TestAttribute]
        public void TestMultiPointDim4()
        {
            GeometryFactory gf = new GeometryFactory(new PackedCoordinateSequenceFactory());
            ICoordinateSequence mpSeq = gf.CoordinateSequenceFactory.Create(1, Ordinates.XYZM);
            mpSeq.SetOrdinate(0, Ordinate.X, 50);
            mpSeq.SetOrdinate(0, Ordinate.Y, -2);
            mpSeq.SetOrdinate(0, Ordinate.Z, 10);
            mpSeq.SetOrdinate(0, Ordinate.M, 20);

            IMultiPoint mp = gf.CreateMultiPoint(mpSeq);
            ICoordinateSequence pSeq = ((Point)mp.GetGeometryN(0)).CoordinateSequence;
            Assert.AreEqual(4, pSeq.Dimension);
            Assert.AreEqual(Ordinates.XYZM, pSeq.Ordinates);
            for (int i = 0; i < 4; i++)
                Assert.AreEqual(mpSeq.GetOrdinate(0, (Ordinate)i), pSeq.GetOrdinate(0, (Ordinate)i));
        }
    }
}