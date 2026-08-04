// SPDX-License-Identifier: BSD-3-Clause
//
// AI assistance disclosure:
//   AI-drafted, human-reviewed and curated. Per the same convention used for the
//   companion JTS prototype at grootstebozewolf/jts#1, AI-generated portions are
//   dedicated to CC0-1.0; human curation falls under the NTS BSD-3-Clause grant.
//
//   Assisted-by: Claude (Opus-4.7)
//
// Status: PLAYGROUND -- prototype of OGC SFA-CA CircularString on top of the
// foundational Curve/Surface abstractions already on enhancement/curved.
// Not for merge into develop without further design discussion -- see the PR
// description for scope, decisions, and open questions.

using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm;

namespace NetTopologySuite.Geometries.Curves
{
    /// <summary>
    /// An OGC SFA-CA <c>CircularString</c>: a sequence of circular arc segments,
    /// each defined by three consecutive coordinates (start, point on arc, end).
    /// </summary>
    /// <remarks>
    /// A <c>CircularString</c> with <c>2n + 1</c> coordinates encodes <c>n</c> arcs,
    /// with adjacent arcs sharing endpoints.  An empty <c>CircularString</c> has zero
    /// coordinates.
    /// <para/>
    /// This is a prototype.  In particular, <c>Length</c> and <c>Boundary</c> currently
    /// fall back to chord-based computations (treating the control points as a polyline)
    /// rather than analytical arc geometry; the inherited <see cref="Curve.IsClosed"/>
    /// semantics still apply (start equals end).
    /// </remarks>
    [Serializable]
    public class CircularString : Curve, ILinearizable<LineString>
    {
        /// <summary>The control points of the arcs.</summary>
        private readonly CoordinateSequence _points;

        /// <summary>
        /// Initializes a new instance of the <see cref="CircularString"/> class.
        /// </summary>
        /// <param name="points">The coordinate sequence of control points</param>
        /// <param name="factory">The geometry factory</param>
        /// <exception cref="ArgumentException">
        /// If the sequence has fewer than 3 points or an even number of points
        /// (must be 0 or odd and at least 3).
        /// </exception>
        public CircularString(CoordinateSequence points, GeometryFactory factory) : base(factory)
        {
            if (points == null)
            {
                points = factory.CoordinateSequenceFactory.Create(0, Ordinates.XY);
            }
            if (points.Count != 0)
            {
                if (points.Count < 3)
                {
                    throw new ArgumentException(
                        "A non-empty CircularString must have at least 3 control points " +
                        "(start, on-arc, end of the first arc).", nameof(points));
                }
                if (points.Count % 2 == 0)
                {
                    throw new ArgumentException(
                        "A CircularString must have an odd number of control points " +
                        "(2n + 1 points encode n arcs).", nameof(points));
                }
            }
            _points = points;
        }

        /// <summary>The control points of this <c>CircularString</c>.</summary>
        public CoordinateSequence CoordinateSequence => _points;

        /// <summary>The number of arc segments encoded by this <c>CircularString</c>.</summary>
        public int NumArcs => _points.Count == 0 ? 0 : (_points.Count - 1) / 2;

        /// <inheritdoc cref="Geometry.NumPoints"/>
        public override int NumPoints => _points.Count;

        /// <inheritdoc cref="Geometry.IsEmpty"/>
        public override bool IsEmpty => _points.Count == 0;

        /// <inheritdoc cref="Geometry.Coordinate"/>
        public override Coordinate Coordinate => IsEmpty ? null : _points.GetCoordinate(0);

        /// <inheritdoc cref="Geometry.Coordinates"/>
        public override Coordinate[] Coordinates => _points.ToCoordinateArray();

        /// <inheritdoc/>
        public override double[] GetOrdinates(Ordinate ordinate)
        {
            if (IsEmpty) return new double[0];
            var ordinateFlag = (Ordinates)(1 << (int)ordinate);
            if ((_points.Ordinates & ordinateFlag) != ordinateFlag)
            {
                var nulls = new double[_points.Count];
                for (int i = 0; i < nulls.Length; i++) nulls[i] = Coordinate.NullOrdinate;
                return nulls;
            }
            var vals = new double[_points.Count];
            for (int i = 0; i < _points.Count; i++) vals[i] = _points.GetOrdinate(i, (int)ordinate);
            return vals;
        }

        /// <inheritdoc cref="Curve.StartPoint"/>
        public override Point StartPoint =>
            IsEmpty ? null : Factory.CreatePoint(_points.GetCoordinate(0));

        /// <inheritdoc cref="Curve.EndPoint"/>
        public override Point EndPoint =>
            IsEmpty ? null : Factory.CreatePoint(_points.GetCoordinate(_points.Count - 1));

        /// <inheritdoc cref="Curve.IsClosed"/>
        public override bool IsClosed
        {
            get
            {
                if (IsEmpty) return false;
                return _points.GetCoordinate(0).Equals2D(_points.GetCoordinate(_points.Count - 1));
            }
        }

        /// <inheritdoc cref="Geometry.GeometryType"/>
        public override string GeometryType => "CircularString";

        /// <inheritdoc cref="Geometry.OgcGeometryType"/>
        public override OgcGeometryType OgcGeometryType => OgcGeometryType.CircularString;

        /// <summary>
        /// Length approximated by summing arc-chord lengths.  Analytical arc-length
        /// computation is tracked in the prototype roadmap.
        /// </summary>
        public override double Length => Algorithm.Length.OfLine(_points);

        /// <summary>
        /// The boundary of a curve per the Mod-2 rule: empty when the curve is empty
        /// or closed, otherwise the two end points.
        /// </summary>
        /// <remarks>
        /// <c>BoundaryOp</c> only special-cases <see cref="LineString"/> and
        /// <see cref="MultiLineString"/>; calling it for curve types recurses into
        /// <see cref="Geometry.Boundary"/> and stack-overflows.  Mirror
        /// <see cref="CompoundCurve.Boundary"/> until BoundaryOp learns about curves.
        /// </remarks>
        public override Geometry Boundary
        {
            get
            {
                if (IsEmpty || IsClosed)
                {
                    return Factory.CreateMultiPoint();
                }
                return Factory.CreateMultiPoint(new[] { StartPoint, EndPoint });
            }
        }

        /// <inheritdoc/>
        protected override Envelope ComputeEnvelopeInternal()
        {
            if (IsEmpty) return new Envelope();
            // Conservative: enclose all control points. The true analytical
            // envelope of an arc may extend beyond control points by up to the
            // sagitta of the arc; a follow-up will tighten this.
            var env = new Envelope();
            for (int i = 0; i < _points.Count; i++)
                env.ExpandToInclude(_points.GetCoordinate(i));
            return env;
        }

        /// <inheritdoc/>
        public override bool EqualsExact(Geometry other, double tolerance)
        {
            if (!IsEquivalentClass(other)) return false;
            var o = (CircularString)other;
            if (_points.Count != o._points.Count) return false;
            var cec = Factory.CoordinateEqualityComparer;
            for (int i = 0; i < _points.Count; i++)
            {
                if (!cec.Equals(_points.GetCoordinate(i), o._points.GetCoordinate(i), tolerance))
                    return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public override void Apply(ICoordinateFilter filter)
        {
            for (int i = 0; i < _points.Count; i++) filter.Filter(_points.GetCoordinate(i));
        }

        /// <inheritdoc/>
        public override void Apply(ICoordinateSequenceFilter filter)
        {
            if (_points.Count == 0) return;
            for (int i = 0; i < _points.Count; i++)
            {
                filter.Filter(_points, i);
                if (filter.Done) break;
            }
            if (filter.GeometryChanged) GeometryChanged();
        }

        /// <inheritdoc/>
        public override void Apply(IEntireCoordinateSequenceFilter filter)
        {
            filter.Filter(_points);
            if (filter.GeometryChanged) GeometryChanged();
        }

        /// <inheritdoc/>
        public override void Apply(IGeometryFilter filter) => filter.Filter(this);

        /// <inheritdoc/>
        public override void Apply(IGeometryComponentFilter filter) => filter.Filter(this);

        /// <inheritdoc/>
        protected override Geometry CopyInternal() => new CircularString(_points.Copy(), Factory);

        /// <inheritdoc/>
        public override void Normalize()
        {
            // Two normalization choices for an arc string: (a) keep direction,
            // (b) flip endpoints when start > end in lex order. Mirror LineString:
            if (IsEmpty) return;
            var lex = _points.GetCoordinate(0).CompareTo(_points.GetCoordinate(_points.Count - 1));
            if (lex > 0) CoordinateSequences.Reverse(_points);
        }

        /// <inheritdoc/>
        protected override Geometry ReverseInternal()
        {
            var rev = _points.Copy();
            CoordinateSequences.Reverse(rev);
            return new CircularString(rev, Factory);
        }

        /// <inheritdoc/>
        protected override bool IsEquivalentClass(Geometry other) => other is CircularString;

        /// <summary>
        /// CompareTo for two CircularStrings uses coordinate-sequence lex order.
        /// </summary>
        protected internal override int CompareToSameClass(object o)
        {
            var other = (CircularString)o;
            int n = Math.Min(_points.Count, other._points.Count);
            for (int i = 0; i < n; i++)
            {
                int c = _points.GetCoordinate(i).CompareTo(other._points.GetCoordinate(i));
                if (c != 0) return c;
            }
            return _points.Count.CompareTo(other._points.Count);
        }

        /// <inheritdoc/>
        protected internal override int CompareToSameClass(object o, IComparer<CoordinateSequence> comp)
        {
            return comp.Compare(_points, ((CircularString)o)._points);
        }

        /// <inheritdoc/>
        protected override SortIndexValue SortIndex => SortIndexValue.CircularString;

        /// <summary>
        /// Returns a chord approximation of this circular string as a
        /// <see cref="LineString"/> through the control points.
        /// </summary>
        /// <remarks>
        /// Arc-aware densification is deferred; this escape hatch lets algorithms
        /// fall through to linear geometry without treating control polylines as
        /// the only model of the type itself.
        /// </remarks>
        public LineString Linearize() => Linearize(double.NaN);

        /// <summary>
        /// Returns a chord approximation of this circular string as a
        /// <see cref="LineString"/>.
        /// </summary>
        /// <param name="arcSegmentLength">
        /// Reserved for maximum chord length along each arc. Currently unused;
        /// control-point chords are always returned.
        /// </param>
        public LineString Linearize(double arcSegmentLength)
        {
            if (IsEmpty)
            {
                return Factory.CreateLineString();
            }
            return Factory.CreateLineString(_points.Copy());
        }
    }
}
