using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Implementation
{
    //Tests are exposed by CoordinateSequenceTestBase type
    public class PackedCoordinateSequenceTest : CoordinateSequenceTestBase
    {
        protected override ICoordinateSequenceFactory CsFactory => new PackedCoordinateSequenceFactory();

        [TestAttribute]
        public void TestMultiPointDim4()
        {
            var gf = new GeometryFactory(new PackedCoordinateSequenceFactory());
            var mpSeq = gf.CoordinateSequenceFactory.Create(1, Ordinates.XYZM);
            mpSeq.SetOrdinate(0, Ordinate.X, 50);
            mpSeq.SetOrdinate(0, Ordinate.Y, -2);
            mpSeq.SetOrdinate(0, Ordinate.Z, 10);
            mpSeq.SetOrdinate(0, Ordinate.M, 20);

            var mp = gf.CreateMultiPoint(mpSeq);
            var pSeq = ((Point)mp.GetGeometryN(0)).CoordinateSequence;
            Assert.AreEqual(4, pSeq.Dimension);
            Assert.AreEqual(Ordinates.XYZM, pSeq.Ordinates);
            for (int i = 0; i < 4; i++)
                Assert.AreEqual(mpSeq.GetOrdinate(0, (Ordinate)i), pSeq.GetOrdinate(0, (Ordinate)i));
        }

        [Test, Sequential]
        public void TestOrdinates(
            [Values(2, 3, 4)] int dimension,
            [Values(Ordinates.XY, Ordinates.XYZ, Ordinates.XYZM)] Ordinates ordinates)
        {
            var factory = new PackedCoordinateSequenceFactory(
                PackedCoordinateSequenceFactory.PackedType.Double,
                dimension);

            Assert.AreEqual(ordinates, factory.Ordinates);
        }
    }
}