// SPDX-License-Identifier: BSD-3-Clause
// Fail-closed contract for curve metrics/analytics. Assisted-by: xAI Grok

using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Curves;
using NetTopologySuite.Operation.Distance;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Curves
{
    /// <summary>
    /// Pins the fail-closed contract: metric and analytic members on the five
    /// SQL/MM curve types throw <see cref="NotSupportedException"/> until
    /// arc-aware implementations land. Empty geometries keep trivial exact
    /// values; <c>GetHashCode</c> stays identity-safe.
    /// </summary>
    public class CurveFailClosedTest
    {
        private readonly GeometryFactory _factory = new GeometryFactory();

        private CircularString Semicircle()
        {
            var seq = _factory.CoordinateSequenceFactory.Create(new[]
            {
                new Coordinate(1, 0),
                new Coordinate(0, 1),
                new Coordinate(-1, 0)
            });
            return new CircularString(seq, _factory);
        }

        private CompoundCurve Compound()
        {
            var line = _factory.CreateLineString(new[] { new Coordinate(-2, 0), new Coordinate(1, 0) });
            return new CompoundCurve(new Curve[] { line, Semicircle() }, _factory);
        }

        private CurvePolygon CurvePoly()
        {
            var shell = new CircularString(_factory.CoordinateSequenceFactory.Create(new[]
            {
                new Coordinate(0, 0),
                new Coordinate(2, 2),
                new Coordinate(4, 0),
                new Coordinate(2, -2),
                new Coordinate(0, 0)
            }), _factory);
            return new CurvePolygon(shell, _factory);
        }

        private MultiCurve MultiC() => new MultiCurve(new Curve[] { Semicircle() }, _factory);

        private MultiSurface MultiS() => new MultiSurface(new Geometry[] { CurvePoly() }, _factory);

        private Geometry[] AllFive() => new Geometry[]
        {
            Semicircle(), Compound(), CurvePoly(), MultiC(), MultiS()
        };

        [Test]
        public void DistanceThrowsFromBothOperandPositions()
        {
            var arc = Semicircle();
            var point = _factory.CreatePoint(new Coordinate(0, 0));

            Assert.That(() => arc.Distance(point), Throws.TypeOf<NotSupportedException>());
            Assert.That(() => point.Distance(arc), Throws.TypeOf<NotSupportedException>());
            Assert.That(() => DistanceOp.NearestPoints(point, arc), Throws.TypeOf<NotSupportedException>());
            Assert.That(() => DistanceOp.IsWithinDistance(point, arc, 1.0), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void CentroidAndInteriorPointThrowForEachCurveType()
        {
            foreach (var g in AllFive())
            {
                Assert.That(() => g.Centroid, Throws.TypeOf<NotSupportedException>(), g.GeometryType + ".Centroid");
                Assert.That(() => g.InteriorPoint, Throws.TypeOf<NotSupportedException>(), g.GeometryType + ".InteriorPoint");
            }
        }

        [Test]
        public void CentroidAndInteriorPointThrowForCollectionWrappingCircularString()
        {
            var gc = _factory.CreateGeometryCollection(new Geometry[] { Semicircle() });
            Assert.That(() => gc.Centroid, Throws.TypeOf<NotSupportedException>());
            Assert.That(() => gc.InteriorPoint, Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void MultiCurveAndMultiSurfaceBoundaryThrowNotSupported()
        {
            Assert.That(() => MultiC().Boundary, Throws.TypeOf<NotSupportedException>());
            Assert.That(() => MultiS().Boundary, Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void MultiCurveLengthAndMultiSurfaceAreaThrow()
        {
            Assert.That(() => MultiC().Length, Throws.TypeOf<NotSupportedException>());
            Assert.That(() => MultiS().Area, Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void GetHashCodeDoesNotThrowForAnyCurveType()
        {
            foreach (var g in AllFive())
            {
                Assert.That(() => g.GetHashCode(), Throws.Nothing, g.GeometryType + ".GetHashCode");
            }
        }

        [Test]
        public void IntersectsThrowsViaEnvelopeGuard()
        {
            var point = _factory.CreatePoint(new Coordinate(0, 0));
            Assert.That(() => point.Intersects(Semicircle()), Throws.TypeOf<NotSupportedException>());
        }
    }
}
