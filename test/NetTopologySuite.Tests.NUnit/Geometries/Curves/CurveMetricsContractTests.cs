// SPDX-License-Identifier: BSD-3-Clause
//
// Intentional-fail hooks for arc-aware Distance / Length / Envelope on the
// SQL/MM curve foundation. These pin expected contracts and stay red until
// arc-aware metrics land; today the members throw NotSupportedException
// instead of returning control-polyline / control-bbox stubs.
//
// Assisted-by: xAI Grok

using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Curves;
using NetTopologySuite.Operation.Distance;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Curves
{
    /// <summary>
    /// Intentional-fail contract tests for arc-aware curve metrics.
    /// Assert SQL/MM / GEOS-quality Distance, Length, and Envelope behaviour;
    /// they stay red until arc-aware metrics land (today they fail with
    /// <see cref="NotSupportedException"/> rather than wrong chord values).
    /// Excluded from default CI via <c>FailureCase</c> (same pattern as other known-fail fixtures).
    /// </summary>
    [Category("FailureCase")]
    [Category("Red")]
    [Category("Curves.MetricsContract")]
    public class CurveMetricsContractTests
    {
        private readonly GeometryFactory _factory = new GeometryFactory();

        /// <summary>
        /// Unit upper semicircle: controls (1,0), (0,1), (-1,0). Radius 1, centre origin.
        /// </summary>
        private CircularString UnitSemicircle()
        {
            var seq = _factory.CoordinateSequenceFactory.Create(new[]
            {
                new Coordinate(1, 0),
                new Coordinate(0, 1),
                new Coordinate(-1, 0)
            });
            return new CircularString(seq, _factory);
        }

        /// <summary>
        /// Unit circle arc through angles −30°, 10°, 50° (radians via cos/sin).
        /// True max X on the arc is 1 (angle 0°), which is not a control point.
        /// </summary>
        private CircularString UnitArcPastAxis()
        {
            static double Rad(double deg) => deg * Math.PI / 180.0;
            var seq = _factory.CoordinateSequenceFactory.Create(new[]
            {
                new Coordinate(Math.Cos(Rad(-30)), Math.Sin(Rad(-30))),
                new Coordinate(Math.Cos(Rad(10)), Math.Sin(Rad(10))),
                new Coordinate(Math.Cos(Rad(50)), Math.Sin(Rad(50)))
            });
            return new CircularString(seq, _factory);
        }

        /// <summary>
        /// P0 — Distance to a curve must be finite and arc-correct.
        /// Today <see cref="DistanceOp"/> fails closed with
        /// <see cref="NotSupportedException"/> (expected distance at centre is 1).
        /// </summary>
        [Test]
        public void Red_Distance_PointToCircularString_CentreOfUnitSemicircle_IsRadius()
        {
            var arc = UnitSemicircle();
            var centre = _factory.CreatePoint(new Coordinate(0, 0));

            double d = DistanceOp.Distance(centre, arc);

            Assert.That(double.IsFinite(d), Is.True,
                "Distance must not leave the MaxValue sentinel for curve inputs.");
            Assert.That(d, Is.EqualTo(1.0).Within(1e-9),
                "Expected arc-aware distance: centre of unit semicircle is at distance r = 1.");
        }

        /// <summary>
        /// P0 companion — endpoint query is zero on the true arc.
        /// </summary>
        [Test]
        public void Red_Distance_PointToCircularString_Endpoint_IsZero()
        {
            var arc = UnitSemicircle();
            var end = _factory.CreatePoint(new Coordinate(1, 0));

            double d = DistanceOp.Distance(end, arc);

            Assert.That(double.IsFinite(d), Is.True);
            Assert.That(d, Is.EqualTo(0.0).Within(1e-12));
        }

        /// <summary>
        /// P1 — Length is arc measure r·θ, not the control-polyline.
        /// Unit semicircle: expected length is π.
        /// </summary>
        [Test]
        public void Red_Length_UnitSemicircle_IsPi()
        {
            var arc = UnitSemicircle();

            Assert.That(arc.Length, Is.EqualTo(Math.PI).Within(1e-9),
                "Expected arc-aware length: unit semicircle is π, not 2√2 control chords.");
        }

        /// <summary>
        /// P1 — Envelope must cover the true arc, not only control points.
        /// </summary>
        [Test]
        public void Red_Envelope_IncludesAxisExtremeBeyondControls()
        {
            var arc = UnitArcPastAxis();
            var env = arc.EnvelopeInternal;

            Assert.That(env.MaxX, Is.GreaterThanOrEqualTo(1.0 - 1e-12),
                "Unit arc spanning −30°…50° reaches x=1 at angle 0°; " +
                "control-only bbox stops at cos(10°) ≈ 0.985.");
        }
    }
}
