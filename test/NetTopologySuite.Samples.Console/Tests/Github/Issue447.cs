using System;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    [Description("https://github.com/NetTopologySuite/NetTopologySuite/issues/447")]
    public class Issue447
    {
        private static readonly CoordinateSequenceFactory CsFactory =
            NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory;
        
        [Test]
        public void Test()
        {
            var i = new NtsGeometryServices(CsFactory, PrecisionModel.Floating.Value, 0,
                GeometryOverlay.Legacy, new PerOrdinateEqualityComparer());
            var f = i.CreateGeometryFactory();
            var pt1 = f.CreatePoint(new Coordinate(1, 2));
            var pt2 = f.CreatePoint(new CoordinateZ(1, 2, 3));
            var pt3 = f.CreatePoint(new CoordinateM(1, 2, 4));
            var pt4 = f.CreatePoint(new CoordinateZM(1, 2, 3, 4));
            var pt5 = f.CreatePoint(new CoordinateZM(1, 2, double.NaN, 4));
            var pt6 = f.CreatePoint(new Coordinate(1, 2));

            Assert.IsFalse(pt1 == pt2);
            Assert.IsFalse(pt1 == pt3);
            Assert.IsFalse(pt1 == pt4);
            Assert.IsFalse(pt1 == pt5);
            Assert.IsTrue(pt1 == pt6);
            Assert.AreNotEqual(pt1, pt2);
            Assert.AreNotEqual(pt1, pt3);
            Assert.AreNotEqual(pt1, pt4);
            Assert.AreNotEqual(pt2, pt3);
            Assert.AreNotEqual(pt2, pt4);
            Assert.AreNotEqual(pt3, pt4);
            Assert.AreNotEqual(pt4, pt5);
            Assert.AreEqual(pt1, pt6);

            Assert.IsTrue(pt1 == f.CreatePoint(new CoordinateZ(1,2)));
            Assert.IsTrue(pt1 == f.CreatePoint(new CoordinateM(1, 2)));
            Assert.IsTrue(pt1 == f.CreatePoint(new CoordinateZM(1, 2)));

        }

        [Test]
        public void TestWithTolerance()
        {
            var i = new NtsGeometryServices(CsFactory, PrecisionModel.Floating.Value, 0,
                GeometryOverlay.Legacy, new PerOrdinateEqualityComparer());
            var f = i.CreateGeometryFactory();
            const double tolerance  = 0.001;
            const double allowed = tolerance;

            var pt = f.CreatePoint(new CoordinateZM(1, 2, 3, 4));
            Assert.That(pt == f.CreatePoint(new CoordinateZM(1 + allowed, 2, 3, 4)), Is.False);
            Assert.That(pt.EqualsExact(f.CreatePoint(new CoordinateZM(1 + allowed, 2, 3, 4)), tolerance), Is.True);
            Assert.That(pt.EqualsExact(f.CreatePoint(new CoordinateZM(Math.BitIncrement(1 + allowed), 2, 3, 4)), tolerance), Is.False);

            Assert.That(pt == f.CreatePoint(new CoordinateZM(1 + allowed, 2, 3, 4)), Is.False);
            Assert.That(pt.EqualsExact(f.CreatePoint(new CoordinateZM(1, 2 + allowed, 3, 4)), tolerance), Is.True);
            Assert.That(pt.EqualsExact(f.CreatePoint(new CoordinateZM(1, Math.BitIncrement(2 + allowed), 3, 4)), tolerance), Is.False);

            Assert.That(pt == f.CreatePoint(new CoordinateZM(1 + allowed, 2, 3, 4)), Is.False);
            Assert.That(pt.EqualsExact(f.CreatePoint(new CoordinateZM(1, 2, 3 - allowed, 4)), tolerance), Is.True);
            Assert.That(pt.EqualsExact(f.CreatePoint(new CoordinateZM(1, 2, Math.BitDecrement(3 - allowed), 4)), tolerance), Is.False);

            Assert.That(pt == f.CreatePoint(new CoordinateZM(1 + allowed, 2, 3, 4)), Is.False);
            Assert.That(pt.EqualsExact(f.CreatePoint(new CoordinateZM(1, 2, 3, 4 - allowed)), tolerance), Is.True);
            Assert.That(pt.EqualsExact(f.CreatePoint(new CoordinateZM(1, 2, 3, Math.BitDecrement(4 - allowed))), tolerance), Is.False);
        }

        [Test]
        public void Test2()
        {
            var f = NtsGeometryServices.Instance.CreateGeometryFactory();
            var pt1 = f.CreatePoint(new NetTopologySuite.Geometries.Coordinate(10, 10));
            var pt2 = f.CreatePoint(new NetTopologySuite.Geometries.CoordinateZ(10, 10));

            Assert.IsTrue(pt1 == pt2); 
            Assert.IsTrue(pt1.Equals((object)pt2));

        }
    }
}
