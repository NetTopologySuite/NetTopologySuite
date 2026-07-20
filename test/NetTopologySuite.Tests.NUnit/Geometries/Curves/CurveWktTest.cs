// SPDX-License-Identifier: BSD-3-Clause
// AI-drafted, human-reviewed.  Assisted-by: Claude (Fable 5)

using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Curves;
using NetTopologySuite.IO;
using NUnit.Framework;

// The OGC Triangle geometry, not the coordinate utility class.
using Triangle = NetTopologySuite.Geometries.Curves.Triangle;

namespace NetTopologySuite.Tests.NUnit.Geometries.Curves
{
    /// <summary>
    /// WKT round-trip tests for the curve prototype types (CIRCULARSTRING,
    /// COMPOUNDCURVE, CURVEPOLYGON, TRIANGLE, TIN).
    /// </summary>
    public class CurveWktTest
    {
        [TestCase("CIRCULARSTRING (0 0, 1 1, 2 0)", typeof(CircularString))]
        [TestCase("CIRCULARSTRING (0 0, 1 1, 2 0, 3 -1, 4 0)", typeof(CircularString))]
        [TestCase("CIRCULARSTRING EMPTY", typeof(CircularString))]
        [TestCase("COMPOUNDCURVE ((0 0, 1 0), CIRCULARSTRING (1 0, 2 1, 3 0), (3 0, 4 0))", typeof(CompoundCurve))]
        [TestCase("COMPOUNDCURVE (CIRCULARSTRING (0 0, 1 1, 2 0), (2 0, 3 0))", typeof(CompoundCurve))]
        [TestCase("COMPOUNDCURVE EMPTY", typeof(CompoundCurve))]
        [TestCase("CURVEPOLYGON (CIRCULARSTRING (0 0, 2 2, 4 0, 2 -2, 0 0))", typeof(CurvePolygon))]
        [TestCase("CURVEPOLYGON (COMPOUNDCURVE (CIRCULARSTRING (0 0, 5 5, 10 0), (10 0, 0 0)), (2 1, 8 1, 8 2, 2 2, 2 1))", typeof(CurvePolygon))]
        [TestCase("CURVEPOLYGON ((0 0, 100 0, 100 100, 0 100, 0 0), CIRCULARSTRING (40 50, 50 60, 60 50, 50 40, 40 50))", typeof(CurvePolygon))]
        [TestCase("CURVEPOLYGON EMPTY", typeof(CurvePolygon))]
        [TestCase("TRIANGLE ((0 0, 1 0, 0 1, 0 0))", typeof(Triangle))]
        [TestCase("TRIANGLE EMPTY", typeof(Triangle))]
        [TestCase("TIN (((0 0, 1 0, 0 1, 0 0)), ((1 0, 1 1, 0 1, 1 0)))", typeof(Tin))]
        [TestCase("TIN EMPTY", typeof(Tin))]
        [TestCase("GEOMETRYCOLLECTION (CIRCULARSTRING (0 0, 1 1, 2 0), POINT (5 5))", typeof(GeometryCollection))]
        public void WktRoundTripIsStable(string wkt, Type expectedType)
        {
            var reader = new WKTReader();

            var geometry = reader.Read(wkt);
            Assert.That(geometry, Is.InstanceOf(expectedType));

            string written = geometry.AsText();
            Assert.That(written, Is.EqualTo(wkt));

            var reparsed = reader.Read(written);
            Assert.That(reparsed.EqualsExact(geometry), Is.True,
                "Round-tripped geometry should be EqualsExact to the original.");
        }

        [Test]
        public void ReadPreservesComponentAndRingSubtypes()
        {
            var reader = new WKTReader();

            var cc = (CompoundCurve)reader.Read("COMPOUNDCURVE ((0 0, 1 0), CIRCULARSTRING (1 0, 2 1, 3 0))");
            Assert.That(cc.Curves[0], Is.InstanceOf<LineString>());
            Assert.That(cc.Curves[1], Is.InstanceOf<CircularString>());

            var cp = (CurvePolygon)reader.Read(
                "CURVEPOLYGON ((0 0, 100 0, 100 100, 0 100, 0 0), CIRCULARSTRING (40 50, 50 60, 60 50, 50 40, 40 50))");
            Assert.That(cp.ExteriorRing, Is.InstanceOf<LineString>());
            Assert.That(cp.GetInteriorRingN(0), Is.InstanceOf<CircularString>());
        }

        [Test]
        public void ReadSupportsZOrdinates()
        {
            var geometry = new WKTReader().Read("CIRCULARSTRING Z(0 0 5, 1 1 5, 2 0 5)");

            var circularString = (CircularString)geometry;
            Assert.That(circularString.CoordinateSequence.GetZ(0), Is.EqualTo(5));
            Assert.That(new WKTWriter(3).Write(geometry), Is.EqualTo("CIRCULARSTRING Z(0 0 5, 1 1 5, 2 0 5)"));
        }

        [Test]
        public void ReadRejectsNestedCompoundCurves()
        {
            Assert.Throws<ParseException>(() =>
                new WKTReader().Read("COMPOUNDCURVE (COMPOUNDCURVE ((0 0, 1 0)))"));
        }

        [Test]
        public void ReadRejectsTriangleWithInteriorRing()
        {
            Assert.Throws<ParseException>(() =>
                new WKTReader().Read("TRIANGLE ((0 0, 10 0, 0 10, 0 0), (1 1, 2 1, 1 2, 1 1))"));
        }

        [Test]
        public void ReadRejectsUnexpectedCurveComponent()
        {
            Assert.Throws<ParseException>(() =>
                new WKTReader().Read("CURVEPOLYGON (POINT (0 0))"));
        }

        [Test]
        public void ReadRejectsEvenCircularStringPointCount()
        {
            Assert.Throws<ArgumentException>(() =>
                new WKTReader().Read("CIRCULARSTRING (0 0, 1 1)"));
        }

        [Test]
        public void ReadRejectsNonContiguousCompoundCurve()
        {
            Assert.Throws<ArgumentException>(() =>
                new WKTReader().Read("COMPOUNDCURVE ((0 0, 1 0), (2 0, 3 0))"));
        }
    }
}
