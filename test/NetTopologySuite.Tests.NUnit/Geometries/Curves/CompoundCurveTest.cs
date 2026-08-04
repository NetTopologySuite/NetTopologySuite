// SPDX-License-Identifier: BSD-3-Clause
// AI-drafted, human-reviewed.  Assisted-by: Claude (Fable 5)

using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Curves;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Curves
{
    public class CompoundCurveTest
    {
        private readonly GeometryFactory _factory = new GeometryFactory();

        private CircularString Arc(params (double x, double y)[] pts)
        {
            var coords = new Coordinate[pts.Length];
            for (int i = 0; i < pts.Length; i++) coords[i] = new Coordinate(pts[i].x, pts[i].y);
            return new CircularString(_factory.CoordinateSequenceFactory.Create(coords), _factory);
        }

        private LineString Line(params (double x, double y)[] pts)
        {
            var coords = new Coordinate[pts.Length];
            for (int i = 0; i < pts.Length; i++) coords[i] = new Coordinate(pts[i].x, pts[i].y);
            return _factory.CreateLineString(coords);
        }

        [Test]
        public void EmptyCompoundCurveHasZeroPoints()
        {
            var cc = new CompoundCurve(null, _factory);
            Assert.That(cc.IsEmpty, Is.True);
            Assert.That(cc.NumPoints, Is.EqualTo(0));
            Assert.That(cc.Curves.Count, Is.EqualTo(0));
            Assert.That(cc.IsClosed, Is.False);
            Assert.That(cc.GeometryType, Is.EqualTo("CompoundCurve"));
            Assert.That(cc.OgcGeometryType, Is.EqualTo(OgcGeometryType.CompoundCurve));
        }

        [Test]
        public void LineArcLineSharesJoinPoints()
        {
            var cc = new CompoundCurve(
                new Curve[] { Line((0, 0), (1, 0)), Arc((1, 0), (2, 1), (3, 0)), Line((3, 0), (4, 0)) },
                _factory);

            Assert.That(cc.Curves.Count, Is.EqualTo(3));
            // 2 + 3 + 2 control points, with the two join points emitted once.
            Assert.That(cc.NumPoints, Is.EqualTo(5));
            Assert.That(cc.Coordinates.Length, Is.EqualTo(5));
            Assert.That(cc.StartPoint.Coordinate, Is.EqualTo(new Coordinate(0, 0)));
            Assert.That(cc.EndPoint.Coordinate, Is.EqualTo(new Coordinate(4, 0)));
            Assert.That(cc.IsClosed, Is.False);
        }

        [Test]
        public void MembersRetainSubtypes()
        {
            var cc = new CompoundCurve(
                new Curve[] { Arc((0, 0), (1, 1), (2, 0)), Line((2, 0), (3, 0)) },
                _factory);

            Assert.That(cc.Curves[0], Is.InstanceOf<CircularString>());
            Assert.That(cc.Curves[1], Is.InstanceOf<LineString>());
        }

        [Test]
        public void ClosedCompoundCurveIsClosedAndHasEmptyBoundary()
        {
            var cc = new CompoundCurve(
                new Curve[] { Arc((0, 0), (1, 1), (2, 0)), Line((2, 0), (0, 0)) },
                _factory);

            Assert.That(cc.IsClosed, Is.True);
            Assert.That(cc.Boundary.IsEmpty, Is.True);
        }

        [Test]
        public void OpenCompoundCurveBoundaryIsEndpoints()
        {
            var cc = new CompoundCurve(
                new Curve[] { Line((0, 0), (1, 0)), Arc((1, 0), (2, 1), (3, 0)) },
                _factory);

            var boundary = cc.Boundary;
            Assert.That(boundary.NumGeometries, Is.EqualTo(2));
            Assert.That(boundary.GetGeometryN(0).Coordinate, Is.EqualTo(new Coordinate(0, 0)));
            Assert.That(boundary.GetGeometryN(1).Coordinate, Is.EqualTo(new Coordinate(3, 0)));
        }

        [Test]
        public void LengthIsSumOfComponentLengths()
        {
            var line = Line((0, 0), (1, 0));
            var arc = Arc((1, 0), (2, 1), (3, 0));
            var cc = new CompoundCurve(new Curve[] { line, arc }, _factory);

            Assert.That(cc.Length, Is.EqualTo(line.Length + arc.Length).Within(1e-12));
        }

        [Test]
        public void EnvelopeIsUnionOfComponentEnvelopes()
        {
            var cc = new CompoundCurve(
                new Curve[] { Line((0, 0), (1, 0)), Arc((1, 0), (2, 5), (3, 0)) },
                _factory);

            var env = cc.EnvelopeInternal;
            Assert.That(env.MinX, Is.EqualTo(0));
            Assert.That(env.MaxX, Is.EqualTo(3));
            Assert.That(env.MinY, Is.EqualTo(0));
            Assert.That(env.MaxY, Is.EqualTo(5));
        }

        [Test]
        public void RejectsNullComponents()
        {
            Assert.Throws<ArgumentException>(() =>
                new CompoundCurve(new Curve[] { null }, _factory));
        }

        [Test]
        public void RejectsEmptyComponents()
        {
            Assert.Throws<ArgumentException>(() =>
                new CompoundCurve(new Curve[] { _factory.CreateLineString() }, _factory));
        }

        [Test]
        public void RejectsNonContiguousComponents()
        {
            Assert.Throws<ArgumentException>(() =>
                new CompoundCurve(
                    new Curve[] { Line((0, 0), (1, 0)), Line((2, 0), (3, 0)) },
                    _factory));
        }

        [Test]
        public void RejectsNestedCompoundCurves()
        {
            var inner = new CompoundCurve(new Curve[] { Line((0, 0), (1, 0)) }, _factory);
            Assert.Throws<ArgumentException>(() =>
                new CompoundCurve(new Curve[] { inner }, _factory));
        }

        [Test]
        public void EmptyStartAndEndPointsAreNullLikeLineString()
        {
            var cc = new CompoundCurve(null, _factory);
            Assert.That(cc.StartPoint, Is.Null);
            Assert.That(cc.EndPoint, Is.Null);
        }

        [Test]
        public void NormalizeDoesNotMutateCallerComponentArray()
        {
            var components = new Curve[]
            {
                Line((4, 0), (3, 0)),
                Arc((3, 0), (2, 1), (1, 0))
            };
            var originalFirstStart = components[0].StartPoint.Coordinate.Copy();

            var cc = new CompoundCurve(components, _factory);
            cc.Normalize();

            Assert.That(components[0].StartPoint.Coordinate, Is.EqualTo(originalFirstStart),
                "Normalize must not reverse the caller's component array in place.");
            Assert.That(cc.StartPoint.Coordinate.CompareTo(cc.EndPoint.Coordinate) <= 0);
        }

        [Test]
        public void IsSimpleAndIsRingDoNotThrow()
        {
            var open = new CompoundCurve(
                new Curve[] { Line((0, 0), (1, 0)), Arc((1, 0), (2, 1), (3, 0)) },
                _factory);
            Assert.That(() => open.IsSimple, Throws.Nothing);
            Assert.That(() => open.IsRing, Throws.Nothing);
            Assert.That(open.IsRing, Is.False);

            var closed = new CompoundCurve(
                new Curve[] { Arc((0, 0), (1, 1), (2, 0)), Line((2, 0), (0, 0)) },
                _factory);
            Assert.That(() => closed.IsSimple, Throws.Nothing);
            Assert.That(() => closed.IsRing, Throws.Nothing);
            Assert.That(closed.IsClosed, Is.True);
        }

        [Test]
        public void CopyProducesEqualButDistinctObjectAndPreservesSubtypes()
        {
            var cc = new CompoundCurve(
                new Curve[] { Arc((0, 0), (1, 1), (2, 0)), Line((2, 0), (3, 0)) },
                _factory);
            var copy = (CompoundCurve)cc.Copy();

            Assert.That(copy.EqualsExact(cc), Is.True);
            Assert.That(ReferenceEquals(copy, cc), Is.False);
            Assert.That(copy.Curves[0], Is.InstanceOf<CircularString>());
            Assert.That(ReferenceEquals(copy.Curves[0], cc.Curves[0]), Is.False);
        }

        [Test]
        public void ReverseFlipsComponentOrderAndDirection()
        {
            var cc = new CompoundCurve(
                new Curve[] { Arc((0, 0), (1, 1), (2, 0)), Line((2, 0), (3, 0)) },
                _factory);
            var rev = (CompoundCurve)cc.Reverse();

            Assert.That(rev.Curves[0], Is.InstanceOf<LineString>());
            Assert.That(rev.Curves[1], Is.InstanceOf<CircularString>());
            Assert.That(rev.StartPoint.Coordinate, Is.EqualTo(new Coordinate(3, 0)));
            Assert.That(rev.EndPoint.Coordinate, Is.EqualTo(new Coordinate(0, 0)));

            // Double reverse is identity.
            var revrev = (CompoundCurve)rev.Reverse();
            Assert.That(revrev.EqualsExact(cc), Is.True);
        }

        [Test]
        public void EqualsExactDistinguishesCompoundCurveFromLineString()
        {
            var cc = new CompoundCurve(new Curve[] { Line((0, 0), (1, 0), (2, 0)) }, _factory);
            var ls = Line((0, 0), (1, 0), (2, 0));
            Assert.That(cc.EqualsExact(ls), Is.False,
                "CompoundCurve and LineString with the same coordinates should not be EqualsExact.");
        }

        [Test]
        public void LinearizeReturnsLineStringCollapsingJoinPoints()
        {
            var cc = new CompoundCurve(
                new Curve[] { Line((0, 0), (1, 0)), Arc((1, 0), (2, 1), (3, 0)), Line((3, 0), (4, 0)) },
                _factory);
            ILinearizable<LineString> linearizable = cc;
            var line = linearizable.Linearize();

            Assert.That(line, Is.InstanceOf<LineString>());
            Assert.That(line, Is.Not.InstanceOf<CompoundCurve>());
            Assert.That(line.NumPoints, Is.EqualTo(5));
            Assert.That(line.StartPoint.Coordinate, Is.EqualTo(new Coordinate(0, 0)));
            Assert.That(line.EndPoint.Coordinate, Is.EqualTo(new Coordinate(4, 0)));
        }
    }
}
