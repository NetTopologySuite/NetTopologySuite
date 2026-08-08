// SPDX-License-Identifier: BSD-3-Clause
//
// AI assistance disclosure:
//   AI-drafted, human-reviewed and curated. Per the same convention used for the
//   companion JTS prototype at grootstebozewolf/jts#1, AI-generated portions are
//   dedicated to CC0-1.0; human curation falls under the NTS BSD-3-Clause grant.
//
//   Assisted-by: Claude (Fable 5)
//
// Status: PRODUCTION (structure + WKT/WKB) — GEOS 3.13-class foundation.
// Not GEOS-current metric parity (Length/Envelope/Distance); see CircularString
// and Category=Red CurveOracleRedTests.

using System;
using System.Collections.Generic;

namespace NetTopologySuite.Geometries.Curves
{
    /// <summary>
    /// A SQL/MM Spatial (ISO/IEC 13249-3) <c>CompoundCurve</c>: a single, contiguous
    /// curve composed of a sequence of <see cref="Curve"/> components -- either
    /// <see cref="LineString"/>s or <see cref="CircularString"/>s -- where each
    /// component starts at the end point of its predecessor.
    /// </summary>
    /// <remarks>
    /// Nested <c>CompoundCurve</c> components are rejected, keeping the component
    /// list flat (matching SQL/MM and common implementations).
    /// <para/>
    /// Like <see cref="CircularString"/>, foundation <c>Length</c> / envelopes use
    /// component chord (control) geometry until arc-aware measure lands.
    /// </remarks>
    [Serializable]
    public class CompoundCurve : Curve, ILinearizable<LineString>
    {
        /// <summary>The component curves.</summary>
        private readonly Curve[] _curves;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundCurve"/> class.
        /// </summary>
        /// <param name="curves">The component curves, in traversal order</param>
        /// <param name="factory">The geometry factory</param>
        /// <exception cref="ArgumentException">
        /// If a component is <c>null</c>, empty, or a <c>CompoundCurve</c> itself, or
        /// if a component does not start at the end point of its predecessor.
        /// </exception>
        public CompoundCurve(Curve[] curves, GeometryFactory factory) : base(factory)
        {
            if (curves == null || curves.Length == 0)
            {
                _curves = new Curve[0];
                return;
            }
            for (int i = 0; i < curves.Length; i++)
            {
                if (curves[i] == null)
                {
                    throw new ArgumentException(
                        "A CompoundCurve must not contain null components.", nameof(curves));
                }
                if (curves[i].IsEmpty)
                {
                    throw new ArgumentException(
                        "A CompoundCurve must not contain empty components.", nameof(curves));
                }
                if (curves[i] is CompoundCurve)
                {
                    throw new ArgumentException(
                        "A CompoundCurve must not contain nested CompoundCurve components.",
                        nameof(curves));
                }
                if (i > 0)
                {
                    var previousEnd = curves[i - 1].EndPoint.Coordinate;
                    var currentStart = curves[i].StartPoint.Coordinate;
                    if (!previousEnd.Equals2D(currentStart))
                    {
                        throw new ArgumentException(
                            "The components of a CompoundCurve must be contiguous: component " + i +
                            " starts at " + currentStart + " but its predecessor ends at " + previousEnd + ".",
                            nameof(curves));
                    }
                }
            }
            // Defensive copy: Normalize reverses and rewrites this array in place.
            _curves = new Curve[curves.Length];
            Array.Copy(curves, _curves, curves.Length);
        }

        /// <summary>
        /// Gets the component curves of this <c>CompoundCurve</c>.
        /// </summary>
        public IReadOnlyList<Curve> Curves => _curves;

        /// <inheritdoc cref="Geometry.IsEmpty"/>
        public override bool IsEmpty => _curves.Length == 0;

        /// <inheritdoc cref="Geometry.Coordinate"/>
        public override Coordinate Coordinate => IsEmpty ? null : _curves[0].Coordinate;

        /// <summary>
        /// The coordinates of the components, concatenated in traversal order with
        /// the shared join point of adjacent components emitted only once.
        /// </summary>
        public override Coordinate[] Coordinates
        {
            get
            {
                var coordinates = new List<Coordinate>();
                for (int i = 0; i < _curves.Length; i++)
                {
                    var componentCoordinates = _curves[i].Coordinates;
                    for (int j = i == 0 ? 0 : 1; j < componentCoordinates.Length; j++)
                    {
                        coordinates.Add(componentCoordinates[j]);
                    }
                }
                return coordinates.ToArray();
            }
        }

        /// <inheritdoc cref="Geometry.NumPoints"/>
        public override int NumPoints
        {
            get
            {
                if (IsEmpty) return 0;
                int numPoints = _curves[0].NumPoints;
                for (int i = 1; i < _curves.Length; i++)
                {
                    numPoints += _curves[i].NumPoints - 1;
                }
                return numPoints;
            }
        }

        /// <inheritdoc/>
        public override double[] GetOrdinates(Ordinate ordinate)
        {
            if (IsEmpty) return new double[0];
            var ordinates = new List<double>();
            for (int i = 0; i < _curves.Length; i++)
            {
                double[] componentOrdinates = _curves[i].GetOrdinates(ordinate);
                for (int j = i == 0 ? 0 : 1; j < componentOrdinates.Length; j++)
                {
                    ordinates.Add(componentOrdinates[j]);
                }
            }
            return ordinates.ToArray();
        }

        /// <inheritdoc cref="Curve.StartPoint"/>
        public override Point StartPoint =>
            IsEmpty ? null : _curves[0].StartPoint;

        /// <inheritdoc cref="Curve.EndPoint"/>
        public override Point EndPoint =>
            IsEmpty ? null : _curves[_curves.Length - 1].EndPoint;

        /// <inheritdoc cref="Curve.IsClosed"/>
        public override bool IsClosed
        {
            get
            {
                if (IsEmpty) return false;
                return StartPoint.Coordinate.Equals2D(EndPoint.Coordinate);
            }
        }

        /// <inheritdoc cref="Geometry.GeometryType"/>
        public override string GeometryType => "CompoundCurve";

        /// <inheritdoc cref="Geometry.OgcGeometryType"/>
        public override OgcGeometryType OgcGeometryType => OgcGeometryType.CompoundCurve;

        /// <summary>
        /// Length approximated by summing component lengths.  For
        /// <see cref="CircularString"/> components this is the chord-based fallback;
        /// analytical arc-length is gated by oracle RED tests (arc-aware measure).
        /// </summary>
        public override double Length
        {
            get
            {
                double length = 0d;
                for (int i = 0; i < _curves.Length; i++)
                {
                    length += _curves[i].Length;
                }
                return length;
            }
        }

        /// <summary>
        /// The boundary of a curve per the Mod-2 rule: empty when the curve is empty
        /// or closed, otherwise the two end points.
        /// </summary>
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
            var env = new Envelope();
            for (int i = 0; i < _curves.Length; i++)
            {
                env.ExpandToInclude(_curves[i].EnvelopeInternal);
            }
            return env;
        }

        /// <inheritdoc/>
        public override bool EqualsExact(Geometry other, double tolerance)
        {
            if (!IsEquivalentClass(other)) return false;
            var o = (CompoundCurve)other;
            if (_curves.Length != o._curves.Length) return false;
            for (int i = 0; i < _curves.Length; i++)
            {
                if (!_curves[i].EqualsExact(o._curves[i], tolerance))
                    return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public override void Apply(ICoordinateFilter filter)
        {
            for (int i = 0; i < _curves.Length; i++) _curves[i].Apply(filter);
        }

        /// <inheritdoc/>
        public override void Apply(ICoordinateSequenceFilter filter)
        {
            for (int i = 0; i < _curves.Length; i++)
            {
                _curves[i].Apply(filter);
                if (filter.Done) break;
            }
            if (filter.GeometryChanged) GeometryChanged();
        }

        /// <inheritdoc/>
        public override void Apply(IEntireCoordinateSequenceFilter filter)
        {
            for (int i = 0; i < _curves.Length; i++)
            {
                _curves[i].Apply(filter);
                if (filter.Done) break;
            }
            if (filter.GeometryChanged) GeometryChanged();
        }

        /// <inheritdoc/>
        public override void Apply(IGeometryFilter filter) => filter.Filter(this);

        /// <inheritdoc/>
        public override void Apply(IGeometryComponentFilter filter)
        {
            filter.Filter(this);
            for (int i = 0; i < _curves.Length; i++) _curves[i].Apply(filter);
        }

        /// <inheritdoc/>
        protected override Geometry CopyInternal()
        {
            var curves = new Curve[_curves.Length];
            for (int i = 0; i < _curves.Length; i++)
            {
                curves[i] = (Curve)_curves[i].Copy();
            }
            return new CompoundCurve(curves, Factory);
        }

        /// <inheritdoc/>
        public override void Normalize()
        {
            // Mirror the CircularString choice: flip traversal direction when the
            // start point is lexicographically greater than the end point.
            if (IsEmpty) return;
            if (StartPoint.Coordinate.CompareTo(EndPoint.Coordinate) > 0)
            {
                Array.Reverse(_curves);
                for (int i = 0; i < _curves.Length; i++)
                {
                    _curves[i] = (Curve)_curves[i].Reverse();
                }
                GeometryChanged();
            }
        }

        /// <inheritdoc/>
        protected override Geometry ReverseInternal()
        {
            var curves = new Curve[_curves.Length];
            for (int i = 0; i < _curves.Length; i++)
            {
                curves[i] = (Curve)_curves[_curves.Length - 1 - i].Reverse();
            }
            return new CompoundCurve(curves, Factory);
        }

        /// <inheritdoc/>
        protected override bool IsEquivalentClass(Geometry other) => other is CompoundCurve;

        /// <summary>
        /// CompareTo for two CompoundCurves uses lex order of the concatenated
        /// coordinates (type-blind across component kinds).
        /// </summary>
        protected internal override int CompareToSameClass(object o)
        {
            var other = (CompoundCurve)o;
            var a = Coordinates;
            var b = other.Coordinates;
            int n = Math.Min(a.Length, b.Length);
            for (int i = 0; i < n; i++)
            {
                int c = a[i].CompareTo(b[i]);
                if (c != 0) return c;
            }
            return a.Length.CompareTo(b.Length);
        }

        /// <inheritdoc/>
        protected internal override int CompareToSameClass(object o, IComparer<CoordinateSequence> comp)
        {
            var other = (CompoundCurve)o;
            var factory = Factory.CoordinateSequenceFactory;
            return comp.Compare(factory.Create(Coordinates), factory.Create(other.Coordinates));
        }

        /// <inheritdoc/>
        protected override SortIndexValue SortIndex => SortIndexValue.CompoundCurve;

        /// <summary>
        /// Returns a chord approximation of this compound curve as a single
        /// <see cref="LineString"/>, concatenating linearized components and
        /// collapsing shared join points.
        /// </summary>
        public LineString Linearize() => Linearize(double.NaN);

        /// <summary>
        /// Returns a chord approximation of this compound curve as a single
        /// <see cref="LineString"/>.
        /// </summary>
        /// <param name="arcSegmentLength">
        /// Passed through to <see cref="CircularString"/> components when they
        /// support densification; currently reserved (control chords).
        /// </param>
        public LineString Linearize(double arcSegmentLength)
        {
            if (IsEmpty)
            {
                return Factory.CreateLineString();
            }

            var coordinates = new List<Coordinate>();
            for (int i = 0; i < _curves.Length; i++)
            {
                LineString componentLine = LinearizeComponent(_curves[i], arcSegmentLength);
                var componentCoordinates = componentLine.Coordinates;
                for (int j = i == 0 ? 0 : 1; j < componentCoordinates.Length; j++)
                {
                    coordinates.Add(componentCoordinates[j]);
                }
            }
            return Factory.CreateLineString(coordinates.ToArray());
        }

        private static LineString LinearizeComponent(Curve component, double arcSegmentLength)
        {
            switch (component)
            {
                case CircularString circularString:
                    return circularString.Linearize(arcSegmentLength);
                case LineString lineString:
                    return lineString;
                default:
                    // Defensive: constructor rejects nested CompoundCurves; other
                    // Curve subtypes should linearize via control coordinates.
                    return component.Factory.CreateLineString(component.Coordinates);
            }
        }
    }
}
