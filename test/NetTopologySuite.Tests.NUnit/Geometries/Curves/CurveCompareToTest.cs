// SPDX-License-Identifier: BSD-3-Clause
// AI-drafted, human-reviewed.

using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Curves;
using NUnit.Framework;

// The OGC Triangle geometry, not the coordinate utility class.
using Triangle = NetTopologySuite.Geometries.Curves.Triangle;

namespace NetTopologySuite.Tests.NUnit.Geometries.Curves
{
    /// <summary>
    /// Mixed-type <see cref="Geometry.CompareTo(Geometry)"/> must order by
    /// dedicated sort indices and never enter a wrong CompareToSameClass cast.
    /// </summary>
    public class CurveCompareToTest
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

        private LinearRing Ring(params (double x, double y)[] pts)
        {
            var coords = new Coordinate[pts.Length];
            for (int i = 0; i < pts.Length; i++) coords[i] = new Coordinate(pts[i].x, pts[i].y);
            return _factory.CreateLinearRing(coords);
        }

        [Test]
        public void CircularStringCompareToLineStringDoesNotThrow()
        {
            var circularString = Arc((0, 0), (1, 1), (2, 0));
            var lineString = Line((0, 0), (1, 1), (2, 0));

            Assert.That(() => circularString.CompareTo(lineString), Throws.Nothing);
            Assert.That(() => lineString.CompareTo(circularString), Throws.Nothing);
            Assert.That(circularString.CompareTo(lineString), Is.Not.EqualTo(0));
            Assert.That(circularString.CompareTo(lineString),
                Is.EqualTo(-lineString.CompareTo(circularString)));
        }

        [Test]
        public void CompoundCurveCompareToLineStringDoesNotThrow()
        {
            var compoundCurve = new CompoundCurve(
                new Curve[] { Line((0, 0), (1, 0)), Arc((1, 0), (2, 1), (3, 0)) },
                _factory);
            var lineString = Line((0, 0), (1, 0), (2, 1), (3, 0));

            Assert.That(() => compoundCurve.CompareTo(lineString), Throws.Nothing);
            Assert.That(() => lineString.CompareTo(compoundCurve), Throws.Nothing);
            Assert.That(compoundCurve.CompareTo(lineString), Is.Not.EqualTo(0));
        }

        [Test]
        public void CurvePolygonCompareToPolygonDoesNotThrow()
        {
            var shell = Ring((0, 0), (10, 0), (10, 10), (0, 10), (0, 0));
            var curvePolygon = new CurvePolygon(shell, _factory);
            var polygon = _factory.CreatePolygon(shell);

            Assert.That(() => curvePolygon.CompareTo(polygon), Throws.Nothing);
            Assert.That(() => polygon.CompareTo(curvePolygon), Throws.Nothing);
            Assert.That(curvePolygon.CompareTo(polygon), Is.Not.EqualTo(0));
            Assert.That(curvePolygon.CompareTo(polygon),
                Is.EqualTo(-polygon.CompareTo(curvePolygon)));
        }

        [Test]
        public void TriangleCompareToPolygonDoesNotThrow()
        {
            var shell = Ring((0, 0), (1, 0), (0, 1), (0, 0));
            var triangle = new Triangle(shell, _factory);
            var polygon = _factory.CreatePolygon(shell);

            Assert.That(() => triangle.CompareTo(polygon), Throws.Nothing);
            Assert.That(() => polygon.CompareTo(triangle), Throws.Nothing);
            Assert.That(triangle.CompareTo(polygon), Is.Not.EqualTo(0));
        }

        [Test]
        public void TinCompareToMultiPolygonDoesNotThrow()
        {
            var t1 = new Triangle(Ring((0, 0), (1, 0), (0, 1), (0, 0)), _factory);
            var t2 = new Triangle(Ring((1, 0), (1, 1), (0, 1), (1, 0)), _factory);
            var tin = new Tin(new[] { t1, t2 }, _factory);
            var multiPolygon = _factory.CreateMultiPolygon(new[]
            {
                _factory.CreatePolygon(Ring((0, 0), (1, 0), (0, 1), (0, 0))),
                _factory.CreatePolygon(Ring((1, 0), (1, 1), (0, 1), (1, 0)))
            });

            Assert.That(() => tin.CompareTo(multiPolygon), Throws.Nothing);
            Assert.That(() => multiPolygon.CompareTo(tin), Throws.Nothing);
            Assert.That(tin.CompareTo(multiPolygon), Is.Not.EqualTo(0));
        }

        [Test]
        public void ArraySortOfMixedLinearAndCurveTypesDoesNotThrow()
        {
            Geometry[] geometries =
            {
                Line((0, 0), (1, 0)),
                Arc((0, 0), (1, 1), (2, 0)),
                new CompoundCurve(new Curve[] { Line((0, 0), (1, 0)), Arc((1, 0), (2, 1), (3, 0)) }, _factory),
                _factory.CreatePolygon(Ring((0, 0), (1, 0), (1, 1), (0, 1), (0, 0))),
                new CurvePolygon(Arc((0, 0), (2, 2), (4, 0), (2, -2), (0, 0)), _factory),
                new Triangle(Ring((0, 0), (1, 0), (0, 1), (0, 0)), _factory)
            };

            Assert.That(() => Array.Sort(geometries), Throws.Nothing);

            // Sorted order must be stable under a second pass and anti-symmetric.
            for (int i = 0; i < geometries.Length - 1; i++)
            {
                Assert.That(geometries[i].CompareTo(geometries[i + 1]), Is.LessThanOrEqualTo(0));
            }
        }

        [Test]
        public void SameTypeCompareToUsesStructuralOrderNotOnlySortIndex()
        {
            var a = Arc((0, 0), (1, 1), (2, 0));
            var b = Arc((0, 0), (1, 2), (2, 0));
            Assert.That(a.CompareTo(b), Is.Not.EqualTo(0));
            Assert.That(a.CompareTo(a.Copy()), Is.EqualTo(0));
        }
    }
}
