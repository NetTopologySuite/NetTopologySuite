// SPDX-License-Identifier: BSD-3-Clause
// AI-drafted, human-reviewed.  Assisted-by: Claude (Fable 5)
//
// The "FCP_*" tests are the in-tree port of CurvePolygonStructuralSpec from the
// out-of-tree NetTopologySuite.Curve repository. They pin the F-CP (Structural
// CurvePolygon) contract of the SFA Curve Awareness epic (locationtech/jts#1195,
// Phase 1): rings are exposed as Curve and never collapse to LinearRing on
// access, copy, or WKT round-trip. Per NTS convention these stay in the
// codebase indefinitely as a regression net. The out-of-tree sub-TAG FCP-TL
// (linearization) depends on facilities this prototype does not carry yet; it
// comes back with the linearization follow-up.

using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Curves;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Curves
{
    [Category("CurveAwareness")]
    public class CurvePolygonTest
    {
        private readonly GeometryFactory _factory = new GeometryFactory();

        private CircularString Arc(params (double x, double y)[] pts)
        {
            var coords = new Coordinate[pts.Length];
            for (int i = 0; i < pts.Length; i++) coords[i] = new Coordinate(pts[i].x, pts[i].y);
            return new CircularString(_factory.CoordinateSequenceFactory.Create(coords), _factory);
        }

        private LinearRing Ring(params (double x, double y)[] pts)
        {
            var coords = new Coordinate[pts.Length];
            for (int i = 0; i < pts.Length; i++) coords[i] = new Coordinate(pts[i].x, pts[i].y);
            return _factory.CreateLinearRing(coords);
        }

        private LineString Line(params (double x, double y)[] pts)
        {
            var coords = new Coordinate[pts.Length];
            for (int i = 0; i < pts.Length; i++) coords[i] = new Coordinate(pts[i].x, pts[i].y);
            return _factory.CreateLineString(coords);
        }

        /// <summary>A compound shell made of two semi-circles, plus one linear hole.</summary>
        private CurvePolygon CompoundShellWithLinearHole()
        {
            var shell = new CompoundCurve(
                new Curve[] { Arc((0, 0), (5, 5), (10, 0)), Arc((10, 0), (5, -5), (0, 0)) },
                _factory);
            var hole = Ring((2, -1), (8, -1), (8, 1), (2, 1), (2, -1));
            return new CurvePolygon(shell, new Curve[] { hole }, _factory);
        }

        /// <summary>A CurvePolygon with a CircularString hole inside a flat shell.</summary>
        private CurvePolygon FlatShellWithArcHole()
        {
            var shell = Ring((0, 0), (100, 0), (100, 100), (0, 100), (0, 0));
            var hole = Arc((40, 50), (50, 60), (60, 50), (50, 40), (40, 50));
            return new CurvePolygon(shell, new Curve[] { hole }, _factory);
        }

        // ============================================================
        // F-CP structural contract (ported from CurvePolygonStructuralSpec)
        // ============================================================

        [Test]
        public void FCP_S_compound_shell_exposed_as_CompoundCurve()
        {
            var cp = CompoundShellWithLinearHole();
            Assert.That(cp.ExteriorRing, Is.InstanceOf<CompoundCurve>(),
                "FCP-S: compound shell should be exposed as CompoundCurve, got "
                + cp.ExteriorRing.GetType().Name);
        }

        [Test]
        public void FCP_S_arc_shell_exposed_as_CircularString()
        {
            var cp = new CurvePolygon(Arc((0, 0), (10, 0), (5, 5), (0, 5), (0, 0)), _factory);
            Assert.That(cp.ExteriorRing, Is.InstanceOf<CircularString>(),
                "FCP-S: arc shell should be exposed as CircularString, got "
                + cp.ExteriorRing.GetType().Name);
        }

        [Test]
        public void FCP_MEM_compound_shell_members_retain_subtypes()
        {
            var cp = CompoundShellWithLinearHole();
            var cc = (CompoundCurve)cp.ExteriorRing;
            Assert.That(cc.Curves.Count, Is.EqualTo(2),
                "FCP-MEM: compound shell should have two members");
            Assert.That(cc.Curves[0], Is.InstanceOf<CircularString>(),
                "FCP-MEM: member 0 should be a CircularString");
            Assert.That(cc.Curves[1], Is.InstanceOf<CircularString>(),
                "FCP-MEM: member 1 should be a CircularString");
        }

        [Test]
        public void FCP_H_arc_hole_exposed_as_CircularString()
        {
            var cp = FlatShellWithArcHole();
            Assert.That(cp.NumInteriorRings, Is.EqualTo(1),
                "FCP-H: CurvePolygon has one interior ring");
            Assert.That(cp.GetInteriorRingN(0), Is.InstanceOf<CircularString>(),
                "FCP-H: arc hole should be a CircularString, got "
                + cp.GetInteriorRingN(0).GetType().Name);
        }

        [Test]
        public void FCP_CP_copy_preserves_shell_subtype()
        {
            var cp = CompoundShellWithLinearHole();
            var copy = (CurvePolygon)cp.Copy();

            Assert.That(copy.ExteriorRing, Is.InstanceOf<CompoundCurve>(),
                "FCP-CP: copied shell must also be a CompoundCurve, got "
                + copy.ExteriorRing.GetType().Name);
            Assert.That(copy.ExteriorRing, Is.Not.SameAs(cp.ExteriorRing),
                "FCP-CP: copy must be a deep copy of the shell");
            Assert.That(copy.EqualsExact(cp), Is.True);
        }

        [Test]
        public void FCP_CP_copy_preserves_arc_hole_subtype()
        {
            var cp = FlatShellWithArcHole();
            var copy = (CurvePolygon)cp.Copy();

            Assert.That(copy.GetInteriorRingN(0), Is.InstanceOf<CircularString>(),
                "FCP-CP: copied hole must remain a CircularString, got "
                + copy.GetInteriorRingN(0).GetType().Name);
        }

        [Test]
        public void FCP_WKT_roundtrip_preserves_compound_shell()
        {
            var cp = CompoundShellWithLinearHole();
            string emitted = new WKTWriter().Write(cp);

            Assert.That(emitted.ToUpperInvariant(), Does.Contain("COMPOUNDCURVE"),
                "FCP-WKT: emitted WKT must contain the COMPOUNDCURVE tag, got: " + emitted);

            var roundTripped = (CurvePolygon)new WKTReader().Read(emitted);
            Assert.That(roundTripped.ExteriorRing, Is.InstanceOf<CompoundCurve>(),
                "FCP-WKT: round-tripped shell must remain a CompoundCurve, got "
                + roundTripped.ExteriorRing.GetType().Name);
        }

        [Test]
        public void FCP_WKT_roundtrip_preserves_arc_shell()
        {
            var cp = new CurvePolygon(Arc((0, 0), (10, 0), (5, 5), (0, 5), (0, 0)), _factory);
            string emitted = new WKTWriter().Write(cp);

            Assert.That(emitted.ToUpperInvariant(), Does.Contain("CIRCULARSTRING"),
                "FCP-WKT: emitted WKT must contain the CIRCULARSTRING tag, got: " + emitted);

            var roundTripped = (CurvePolygon)new WKTReader().Read(emitted);
            Assert.That(roundTripped.ExteriorRing, Is.InstanceOf<CircularString>(),
                "FCP-WKT: round-tripped shell must remain a CircularString, got "
                + roundTripped.ExteriorRing.GetType().Name);
        }

        [Test]
        public void FCP_DOVE_ring_accessors_are_typed_Curve()
        {
            var exteriorRingType = typeof(CurvePolygon).GetProperty(nameof(CurvePolygon.ExteriorRing))?.PropertyType;
            Assert.That(exteriorRingType, Is.Not.Null,
                "FCP-DOVE: CurvePolygon must expose an ExteriorRing accessor");
            Assert.That(exteriorRingType, Is.EqualTo(typeof(Curve)),
                "FCP-DOVE: CurvePolygon.ExteriorRing must be typed as Curve "
                + "(the polymorphic base of LinearRing / LineString / CircularString / "
                + "CompoundCurve). If this returns LinearRing again, the JTS-side "
                + "FCP-DOVE A/B/C dovetail decision needs to be made here too.");
        }

        // ============================================================
        // Construction and basic behavior
        // ============================================================

        [Test]
        public void EmptyCurvePolygonHasZeroPoints()
        {
            var cp = new CurvePolygon(null, _factory);
            Assert.That(cp.IsEmpty, Is.True);
            Assert.That(cp.NumPoints, Is.EqualTo(0));
            Assert.That(cp.NumInteriorRings, Is.EqualTo(0));
            Assert.That(cp.Area, Is.EqualTo(0));
            Assert.That(cp.GeometryType, Is.EqualTo("CurvePolygon"));
            Assert.That(cp.OgcGeometryType, Is.EqualTo(OgcGeometryType.CurvePolygon));
        }

        [Test]
        public void RejectsUnclosedShell()
        {
            Assert.Throws<ArgumentException>(() =>
                new CurvePolygon(Arc((0, 0), (1, 1), (2, 0)), _factory));
        }

        [Test]
        public void RejectsUnclosedHole()
        {
            var shell = Ring((0, 0), (10, 0), (10, 10), (0, 10), (0, 0));
            Assert.Throws<ArgumentException>(() =>
                new CurvePolygon(shell, new Curve[] { Line((1, 1), (2, 2)) }, _factory));
        }

        [Test]
        public void RejectsEmptyShellWithHoles()
        {
            var hole = Ring((2, 2), (4, 2), (4, 4), (2, 4), (2, 2));
            Assert.Throws<ArgumentException>(() =>
                new CurvePolygon(null, new Curve[] { hole }, _factory));
        }

        [Test]
        public void RejectsNullHoles()
        {
            var shell = Ring((0, 0), (10, 0), (10, 10), (0, 10), (0, 0));
            Assert.Throws<ArgumentException>(() =>
                new CurvePolygon(shell, new Curve[] { null }, _factory));
        }

        [Test]
        public void AreaAndLengthFailClosed()
        {
            var shell = Ring((0, 0), (10, 0), (10, 10), (0, 10), (0, 0));
            var hole = Ring((2, 2), (4, 2), (4, 4), (2, 4), (2, 2));
            var cp = new CurvePolygon(shell, new Curve[] { hole }, _factory);

            Assert.That(() => cp.Area, Throws.TypeOf<NotSupportedException>(),
                "Unconditional cut: all-linear CurvePolygon still throws.");
            Assert.That(() => cp.Length, Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void EnvelopeFailsClosedUntilArcAware()
        {
            var cp = CompoundShellWithLinearHole();
            Assert.That(() => cp.EnvelopeInternal, Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void NormalizeFailsClosedWhenNotEmpty()
        {
            var cp = new CurvePolygon(Ring((0, 0), (10, 0), (10, 10), (0, 10), (0, 0)), _factory);
            Assert.That(() => cp.Normalize(), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void LinearizeWithToleranceFailsClosed()
        {
            var cp = FlatShellWithArcHole();
            Assert.That(() => cp.Linearize(1.0), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void BoundaryExposesAllRings()
        {
            var cp = FlatShellWithArcHole();
            var boundary = cp.Boundary;
            Assert.That(boundary.NumGeometries, Is.EqualTo(2));
            Assert.That(boundary.GetGeometryN(0), Is.InstanceOf<LinearRing>());
            Assert.That(boundary.GetGeometryN(1), Is.InstanceOf<CircularString>());
        }

        [Test]
        public void ReversePreservesRingSubtypes()
        {
            var cp = CompoundShellWithLinearHole();
            var rev = (CurvePolygon)cp.Reverse();

            Assert.That(rev.ExteriorRing, Is.InstanceOf<CompoundCurve>());
            Assert.That(rev.NumInteriorRings, Is.EqualTo(1));

            // Double reverse is identity.
            var revrev = (CurvePolygon)rev.Reverse();
            Assert.That(revrev.EqualsExact(cp), Is.True);
        }

        [Test]
        public void EqualsExactDistinguishesCurvePolygonFromPolygon()
        {
            var shellCoords = new[] { (0d, 0d), (10d, 0d), (10d, 10d), (0d, 10d), (0d, 0d) };
            var cp = new CurvePolygon(Ring(shellCoords), _factory);
            var polygon = _factory.CreatePolygon(Ring(shellCoords));
            Assert.That(cp.EqualsExact(polygon), Is.False,
                "CurvePolygon and Polygon with the same shell should not be EqualsExact.");
        }

        [Test]
        public void LinearizeReturnsLinearPolygonWithLinearRings()
        {
            var cp = FlatShellWithArcHole();
            ILinearizable<Polygon> linearizable = cp;
            var polygon = linearizable.Linearize();

            Assert.That(polygon, Is.InstanceOf<Polygon>());
            Assert.That(polygon, Is.Not.InstanceOf<CurvePolygon>());
            Assert.That(polygon.ExteriorRing, Is.InstanceOf<LinearRing>());
            Assert.That(polygon.NumInteriorRings, Is.EqualTo(1));
            Assert.That(polygon.GetInteriorRingN(0), Is.InstanceOf<LinearRing>());
            Assert.That(polygon.GetInteriorRingN(0).NumPoints, Is.EqualTo(5));
        }
    }
}
