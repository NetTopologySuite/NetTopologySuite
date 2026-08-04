// SPDX-License-Identifier: BSD-3-Clause
// AI-drafted, human-reviewed.  Assisted-by: Claude (Opus-4.7)

using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Curves;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Curves
{
    public class CircularStringTest
    {
        private readonly GeometryFactory _factory = new GeometryFactory();

        private CircularString Make(params (double x, double y)[] pts)
        {
            var coords = new Coordinate[pts.Length];
            for (int i = 0; i < pts.Length; i++) coords[i] = new Coordinate(pts[i].x, pts[i].y);
            var seq = _factory.CoordinateSequenceFactory.Create(coords);
            return new CircularString(seq, _factory);
        }

        [Test]
        public void EmptyCircularStringHasZeroPoints()
        {
            var cs = new CircularString(_factory.CoordinateSequenceFactory.Create(0, Ordinates.XY), _factory);
            Assert.That(cs.IsEmpty, Is.True);
            Assert.That(cs.NumPoints, Is.EqualTo(0));
            Assert.That(cs.NumArcs, Is.EqualTo(0));
            Assert.That(cs.IsClosed, Is.False);
        }

        [Test]
        public void SingleArcHasThreePoints()
        {
            var cs = Make((1, 0), (0, 1), (-1, 0));
            Assert.That(cs.NumPoints, Is.EqualTo(3));
            Assert.That(cs.NumArcs, Is.EqualTo(1));
            Assert.That(cs.IsEmpty, Is.False);
            Assert.That(cs.GeometryType, Is.EqualTo("CircularString"));
            Assert.That(cs.OgcGeometryType, Is.EqualTo(OgcGeometryType.CircularString));
        }

        [Test]
        public void TwoArcsHaveFivePoints()
        {
            var cs = Make((1, 0), (0, 1), (-1, 0), (0, -1), (1, 0));
            Assert.That(cs.NumPoints, Is.EqualTo(5));
            Assert.That(cs.NumArcs, Is.EqualTo(2));
            // The endpoint equals the startpoint, so it's closed.
            Assert.That(cs.IsClosed, Is.True);
        }

        [Test]
        public void NonEmptyMustHaveAtLeastThreePoints()
        {
            var seq = _factory.CoordinateSequenceFactory.Create(new[] {
                new Coordinate(0, 0), new Coordinate(1, 1)
            });
            Assert.Throws<ArgumentException>(() => new CircularString(seq, _factory));
        }

        [Test]
        public void PointCountMustBeOdd()
        {
            var seq = _factory.CoordinateSequenceFactory.Create(new[] {
                new Coordinate(0, 0), new Coordinate(1, 1),
                new Coordinate(2, 0), new Coordinate(3, 1)
            });
            Assert.Throws<ArgumentException>(() => new CircularString(seq, _factory));
        }

        [Test]
        public void StartAndEndPointsAreFirstAndLast()
        {
            var cs = Make((1, 2), (3, 4), (5, 6));
            Assert.That(cs.StartPoint.X, Is.EqualTo(1));
            Assert.That(cs.StartPoint.Y, Is.EqualTo(2));
            Assert.That(cs.EndPoint.X, Is.EqualTo(5));
            Assert.That(cs.EndPoint.Y, Is.EqualTo(6));
        }

        [Test]
        public void EnvelopeEnclosesAllControlPoints()
        {
            var cs = Make((0, 0), (5, 10), (10, 0));
            var env = cs.EnvelopeInternal;
            Assert.That(env.MinX, Is.EqualTo(0));
            Assert.That(env.MaxX, Is.EqualTo(10));
            Assert.That(env.MinY, Is.EqualTo(0));
            Assert.That(env.MaxY, Is.EqualTo(10));
        }

        [Test]
        public void CopyProducesEqualButDistinctObject()
        {
            var cs = Make((1, 0), (0, 1), (-1, 0));
            var copy = (CircularString)cs.Copy();
            Assert.That(copy.EqualsExact(cs), Is.True);
            Assert.That(ReferenceEquals(copy, cs), Is.False);
        }

        [Test]
        public void ReverseFlipsCoordinateOrder()
        {
            var cs = Make((1, 0), (0, 1), (-1, 0));
            var rev = (CircularString)cs.Reverse();
            Assert.That(rev.StartPoint.X, Is.EqualTo(-1));
            Assert.That(rev.EndPoint.X, Is.EqualTo(1));
            // Double reverse is identity.
            var revrev = (CircularString)rev.Reverse();
            Assert.That(revrev.EqualsExact(cs), Is.True);
        }

        [Test]
        public void EqualsExactDistinguishesCircularStringFromLineString()
        {
            var cs = Make((1, 0), (0, 1), (-1, 0));
            var ls = _factory.CreateLineString(cs.Coordinates);
            Assert.That(cs.EqualsExact(ls), Is.False,
                "CircularString and LineString with the same coordinates should not be EqualsExact.");
        }

        [Test]
        public void EmptyStartAndEndPointsAreNullLikeLineString()
        {
            var cs = new CircularString(_factory.CoordinateSequenceFactory.Create(0, Ordinates.XY), _factory);
            Assert.That(cs.StartPoint, Is.Null);
            Assert.That(cs.EndPoint, Is.Null);
        }

        [Test]
        public void OpenBoundaryIsEndpoints()
        {
            var cs = Make((1, 0), (0, 1), (-1, 0));
            var boundary = cs.Boundary;
            Assert.That(boundary.NumGeometries, Is.EqualTo(2));
            Assert.That(boundary.GetGeometryN(0).Coordinate, Is.EqualTo(new Coordinate(1, 0)));
            Assert.That(boundary.GetGeometryN(1).Coordinate, Is.EqualTo(new Coordinate(-1, 0)));
        }

        [Test]
        public void ClosedBoundaryIsEmpty()
        {
            var cs = Make((1, 0), (0, 1), (-1, 0), (0, -1), (1, 0));
            Assert.That(cs.IsClosed, Is.True);
            Assert.That(cs.Boundary.IsEmpty, Is.True);
        }

        [Test]
        public void EmptyBoundaryIsEmpty()
        {
            var cs = new CircularString(_factory.CoordinateSequenceFactory.Create(0, Ordinates.XY), _factory);
            Assert.That(cs.Boundary.IsEmpty, Is.True);
        }

        [Test]
        public void IsSimpleAndIsRingDoNotThrow()
        {
            var open = Make((1, 0), (0, 1), (-1, 0));
            Assert.That(() => open.IsSimple, Throws.Nothing);
            Assert.That(() => open.IsRing, Throws.Nothing);
            Assert.That(open.IsSimple, Is.True);
            Assert.That(open.IsRing, Is.False);

            var closed = Make((1, 0), (0, 1), (-1, 0), (0, -1), (1, 0));
            Assert.That(() => closed.IsSimple, Throws.Nothing);
            Assert.That(() => closed.IsRing, Throws.Nothing);
            Assert.That(closed.IsSimple, Is.True);
            Assert.That(closed.IsRing, Is.True);
        }

        [Test]
        public void LinearizeReturnsLineStringThroughControlPoints()
        {
            var cs = Make((1, 0), (0, 1), (-1, 0));
            ILinearizable<LineString> linearizable = cs;
            var line = linearizable.Linearize();

            Assert.That(line, Is.InstanceOf<LineString>());
            Assert.That(line, Is.Not.InstanceOf<CircularString>());
            Assert.That(line.NumPoints, Is.EqualTo(3));
            Assert.That(line.Coordinates, Is.EqualTo(cs.Coordinates));
            Assert.That(cs.Linearize(1.0).NumPoints, Is.EqualTo(3));
        }

        [Test]
        public void LinearizeEmptyReturnsEmptyLineString()
        {
            var cs = new CircularString(_factory.CoordinateSequenceFactory.Create(0, Ordinates.XY), _factory);
            Assert.That(cs.Linearize().IsEmpty, Is.True);
        }
    }
}
