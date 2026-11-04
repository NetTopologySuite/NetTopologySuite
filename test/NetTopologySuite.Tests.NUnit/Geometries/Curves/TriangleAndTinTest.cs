// SPDX-License-Identifier: BSD-3-Clause
// AI-drafted, human-reviewed.  Assisted-by: Claude (Opus-4.7)

using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Curves;
using NUnit.Framework;
using Triangle = NetTopologySuite.Geometries.Curves.Triangle;

namespace NetTopologySuite.Tests.NUnit.Geometries.Curves
{
    public class TriangleAndTinTest
    {
        private readonly GeometryFactory _factory = new GeometryFactory();

        private LinearRing TriRing(double ax, double ay, double bx, double by, double cx, double cy)
        {
            var coords = new[]
            {
                new Coordinate(ax, ay),
                new Coordinate(bx, by),
                new Coordinate(cx, cy),
                new Coordinate(ax, ay),
            };
            return _factory.CreateLinearRing(coords);
        }

        [Test]
        public void TriangleAcceptsFourCoordRing()
        {
            var t = new Triangle(TriRing(0, 0, 10, 0, 5, 8), _factory);
            Assert.That(t.NumPoints, Is.EqualTo(4));
            Assert.That(t.GeometryType, Is.EqualTo("Triangle"));
            Assert.That(t.OgcGeometryType, Is.EqualTo(OgcGeometryType.Triangle));
            Assert.That(t.NumInteriorRings, Is.EqualTo(0));
        }

        [Test]
        public void TriangleAreaIsHalfBaseTimesHeight()
        {
            var t = new Triangle(TriRing(0, 0, 10, 0, 0, 8), _factory);
            Assert.That(t.Area, Is.EqualTo(40.0).Within(1e-9));
        }

        [Test]
        public void TriangleSignedAreaIsPositiveForCCW()
        {
            var t = new Triangle(TriRing(0, 0, 10, 0, 5, 8), _factory);
            Assert.That(t.SignedDoubleArea, Is.GreaterThan(0));
        }

        [Test]
        public void TriangleSignedAreaIsNegativeForCW()
        {
            var t = new Triangle(TriRing(0, 0, 5, 8, 10, 0), _factory);
            Assert.That(t.SignedDoubleArea, Is.LessThan(0));
        }

        [Test]
        public void TriangleSignedAreaIsZeroForCollinear()
        {
            var t = new Triangle(TriRing(0, 0, 1, 1, 2, 2), _factory);
            Assert.That(t.SignedDoubleArea, Is.EqualTo(0).Within(1e-9));
            Assert.That(t.Area, Is.EqualTo(0).Within(1e-9));
        }

        [Test]
        public void TriangleRejectsNon4PointRing()
        {
            var ring = _factory.CreateLinearRing(new[]
            {
                new Coordinate(0, 0), new Coordinate(10, 0),
                new Coordinate(10, 10), new Coordinate(0, 10),
                new Coordinate(0, 0)
            });
            Assert.Throws<ArgumentException>(() => new Triangle(ring, _factory));
        }

        [Test]
        public void TriangleHasNoInteriorRings()
        {
            var t = new Triangle(TriRing(0, 0, 10, 0, 5, 8), _factory);
            Assert.Throws<ArgumentOutOfRangeException>(() => t.GetInteriorRingN(0));
        }

        [Test]
        public void TriangleCopyIsEqualButDistinct()
        {
            var t = new Triangle(TriRing(0, 0, 10, 0, 5, 8), _factory);
            var copy = (Triangle)t.Copy();
            Assert.That(copy.EqualsExact(t), Is.True);
            Assert.That(ReferenceEquals(copy, t), Is.False);
        }

        [Test]
        public void TriangleReverseFlipsVertexOrder()
        {
            var t = new Triangle(TriRing(0, 0, 10, 0, 5, 8), _factory);
            var rev = (Triangle)t.Reverse();
            Assert.That(rev.SignedDoubleArea, Is.EqualTo(-t.SignedDoubleArea).Within(1e-9));
        }

        [Test]
        public void TinHoldsTriangles()
        {
            var t1 = new Triangle(TriRing(0, 0, 10, 0, 5, 8), _factory);
            var t2 = new Triangle(TriRing(10, 0, 20, 0, 15, 8), _factory);
            var tin = new Tin(new[] { t1, t2 }, _factory);
            Assert.That(tin.NumGeometries, Is.EqualTo(2));
            Assert.That(tin.GeometryType, Is.EqualTo("TIN"));
            Assert.That(tin.OgcGeometryType, Is.EqualTo(OgcGeometryType.TIN));
        }

        [Test]
        public void TinAreaIsSumOfTriangleAreas()
        {
            var t1 = new Triangle(TriRing(0, 0, 10, 0, 0, 8), _factory);
            var t2 = new Triangle(TriRing(10, 0, 20, 0, 10, 4), _factory);
            var tin = new Tin(new[] { t1, t2 }, _factory);
            Assert.That(tin.Area, Is.EqualTo(40.0 + 20.0).Within(1e-9));
        }

        [Test]
        public void EmptyTinHasZeroGeometries()
        {
            var tin = new Tin(new Triangle[0], _factory);
            Assert.That(tin.NumGeometries, Is.EqualTo(0));
            Assert.That(tin.Area, Is.EqualTo(0.0));
        }

        [Test]
        public void TinGetTriangleNReturnsTypedElement()
        {
            var t1 = new Triangle(TriRing(0, 0, 10, 0, 5, 8), _factory);
            var tin = new Tin(new[] { t1 }, _factory);
            var got = tin.GetTriangleN(0);
            Assert.That(got, Is.SameAs(t1));
        }

        [Test]
        public void TinCopyDeepCopiesTriangles()
        {
            var t1 = new Triangle(TriRing(0, 0, 10, 0, 5, 8), _factory);
            var tin = new Tin(new[] { t1 }, _factory);
            var copy = (Tin)tin.Copy();
            Assert.That(copy.NumGeometries, Is.EqualTo(1));
            Assert.That(copy.GetTriangleN(0).EqualsExact(t1), Is.True);
            Assert.That(ReferenceEquals(copy.GetTriangleN(0), t1), Is.False);
        }
    }
}
