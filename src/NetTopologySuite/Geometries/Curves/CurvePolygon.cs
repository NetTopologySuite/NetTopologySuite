// SPDX-License-Identifier: BSD-3-Clause
//
// AI assistance disclosure:
//   AI-drafted, human-reviewed and curated. Per the same convention used for the
//   companion JTS prototype at grootstebozewolf/jts#1, AI-generated portions are
//   dedicated to CC0-1.0; human curation falls under the NTS BSD-3-Clause grant.
//
//   Assisted-by: Claude (Fable 5)
//
// Status: PLAYGROUND -- in-tree port of the SQL/MM CurvePolygon prototype from
// the out-of-tree NetTopologySuite.Curve repository, following the maintainer
// discussion on the PR ("in-tree is the way to go"). This carries the F-CP
// structural contract of the SFA Curve Awareness epic (locationtech/jts#1195,
// Phase 1): rings are exposed as Curve, never collapsed to LinearRing.
// Not for merge into develop without further design discussion -- see the PR
// description.

using System;
using System.Collections.Generic;

namespace NetTopologySuite.Geometries.Curves
{
    /// <summary>
    /// A SQL/MM Spatial (ISO/IEC 13249-3) <c>CurvePolygon</c>: a planar surface like
    /// <see cref="Polygon"/>, but whose exterior and interior rings are
    /// <see cref="Curve"/>s -- <see cref="LinearRing"/>s, closed
    /// <see cref="CircularString"/>s or closed <see cref="CompoundCurve"/>s.
    /// </summary>
    /// <remarks>
    /// Because <c>CurvePolygon</c> extends <c>Surface&lt;Curve&gt;</c> rather than
    /// <c>Polygon</c>, the ring accessors are typed <see cref="Curve"/> and never
    /// collapse a curved ring to a flat <see cref="LinearRing"/> (the F-CP
    /// structural contract).
    /// <para/>
    /// This is a prototype.  <c>Area</c> and <c>Length</c> fall back to chord-based
    /// computations that treat the control points of curved rings as polylines;
    /// analytical arc geometry and linearization are tracked in the prototype
    /// roadmap.
    /// </remarks>
    [Serializable]
    public class CurvePolygon : Surface<Curve>, ILinearizable<Polygon>
    {
        /// <summary>The exterior ring.</summary>
        private readonly Curve _shell;

        /// <summary>The interior rings.</summary>
        private readonly Curve[] _holes;

        /// <summary>
        /// Initializes a new instance of the <see cref="CurvePolygon"/> class with no
        /// interior rings.
        /// </summary>
        /// <param name="shell">The exterior ring, a closed <c>Curve</c> (or null for an empty polygon)</param>
        /// <param name="factory">The geometry factory</param>
        public CurvePolygon(Curve shell, GeometryFactory factory)
            : this(shell, null, factory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurvePolygon"/> class.
        /// </summary>
        /// <param name="shell">The exterior ring, a closed <c>Curve</c> (or null for an empty polygon)</param>
        /// <param name="holes">The interior rings, closed <c>Curve</c>s</param>
        /// <param name="factory">The geometry factory</param>
        /// <exception cref="ArgumentException">
        /// If a ring is not closed, a hole is <c>null</c>, or the shell is empty while
        /// holes are not.
        /// </exception>
        public CurvePolygon(Curve shell, Curve[] holes, GeometryFactory factory) : base(factory)
        {
            if (shell == null)
            {
                shell = new CircularString(factory.CoordinateSequenceFactory.Create(0, Ordinates.XY), factory);
            }
            if (holes == null || holes.Length == 0)
            {
                holes = new Curve[0];
            }
            else
            {
                // Defensive copy so callers cannot mutate the ring list after construction.
                var holesCopy = new Curve[holes.Length];
                Array.Copy(holes, holesCopy, holes.Length);
                holes = holesCopy;
            }
            foreach (var hole in holes)
            {
                if (hole == null)
                {
                    throw new ArgumentException(
                        "A CurvePolygon must not contain null holes.", nameof(holes));
                }
            }
            if (shell.IsEmpty && HasNonEmptyElements(holes))
            {
                throw new ArgumentException("shell is empty but holes are not", nameof(holes));
            }
            if (!shell.IsEmpty && !shell.IsClosed)
            {
                throw new ArgumentException(
                    "The shell of a CurvePolygon must be closed.", nameof(shell));
            }
            foreach (var hole in holes)
            {
                if (!hole.IsEmpty && !hole.IsClosed)
                {
                    throw new ArgumentException(
                        "The holes of a CurvePolygon must be closed.", nameof(holes));
                }
            }
            _shell = shell;
            _holes = holes;
        }

        private static bool HasNonEmptyElements(Curve[] curves)
        {
            foreach (var curve in curves)
            {
                if (!curve.IsEmpty) return true;
            }
            return false;
        }

        /// <inheritdoc cref="Surface{T}.ExteriorRing"/>
        public override Curve ExteriorRing => _shell;

        /// <inheritdoc cref="Surface{T}.NumInteriorRings"/>
        public override int NumInteriorRings => _holes.Length;

        /// <inheritdoc cref="Surface{T}.GetInteriorRingN"/>
        public override Curve GetInteriorRingN(int index) => _holes[index];

        /// <inheritdoc cref="Geometry.GeometryType"/>
        public override string GeometryType => "CurvePolygon";

        /// <inheritdoc cref="Geometry.OgcGeometryType"/>
        public override OgcGeometryType OgcGeometryType => OgcGeometryType.CurvePolygon;

        /// <inheritdoc cref="Geometry.IsEmpty"/>
        public override bool IsEmpty => _shell.IsEmpty;

        /// <inheritdoc cref="Geometry.Coordinate"/>
        public override Coordinate Coordinate => _shell.Coordinate;

        /// <inheritdoc cref="Geometry.Coordinates"/>
        public override Coordinate[] Coordinates
        {
            get
            {
                if (IsEmpty) return new Coordinate[0];
                var coordinates = new List<Coordinate>(_shell.Coordinates);
                foreach (var hole in _holes)
                {
                    coordinates.AddRange(hole.Coordinates);
                }
                return coordinates.ToArray();
            }
        }

        /// <inheritdoc cref="Geometry.NumPoints"/>
        public override int NumPoints
        {
            get
            {
                int numPoints = _shell.NumPoints;
                foreach (var hole in _holes)
                {
                    numPoints += hole.NumPoints;
                }
                return numPoints;
            }
        }

        /// <inheritdoc/>
        public override double[] GetOrdinates(Ordinate ordinate)
        {
            if (IsEmpty) return new double[0];
            var ordinates = new List<double>(_shell.GetOrdinates(ordinate));
            foreach (var hole in _holes)
            {
                ordinates.AddRange(hole.GetOrdinates(ordinate));
            }
            return ordinates.ToArray();
        }

        /// <summary>
        /// Area approximated by treating the control points of the rings as polyline
        /// rings (chord-based, consistent with <see cref="CircularString"/>'s
        /// chord-based <c>Length</c>).  For curved rings the true area differs by the
        /// circular segments; analytical arc area is tracked in the prototype roadmap.
        /// </summary>
        public override double Area
        {
            get
            {
                if (IsEmpty) return 0d;
                double area = Algorithm.Area.OfRing(_shell.Coordinates);
                foreach (var hole in _holes)
                {
                    area -= Algorithm.Area.OfRing(hole.Coordinates);
                }
                return area;
            }
        }

        /// <summary>
        /// Perimeter approximated by summing ring lengths (chord-based for curved
        /// rings).
        /// </summary>
        public override double Length
        {
            get
            {
                double length = _shell.Length;
                foreach (var hole in _holes)
                {
                    length += hole.Length;
                }
                return length;
            }
        }

        /// <summary>
        /// The rings of this surface.  Returned as a <see cref="GeometryCollection"/>
        /// of <see cref="Curve"/>s for now -- a dedicated <c>MultiCurve</c> type is
        /// part of the prototype roadmap.
        /// </summary>
        public override Geometry Boundary
        {
            get
            {
                if (IsEmpty)
                {
                    return Factory.CreateGeometryCollection();
                }
                var rings = new Geometry[1 + _holes.Length];
                rings[0] = _shell;
                for (int i = 0; i < _holes.Length; i++)
                {
                    rings[i + 1] = _holes[i];
                }
                return Factory.CreateGeometryCollection(rings);
            }
        }

        /// <inheritdoc/>
        protected override Envelope ComputeEnvelopeInternal() => _shell.EnvelopeInternal;

        /// <inheritdoc/>
        public override bool EqualsExact(Geometry other, double tolerance)
        {
            if (!IsEquivalentClass(other)) return false;
            var o = (CurvePolygon)other;
            if (!_shell.EqualsExact(o._shell, tolerance)) return false;
            if (_holes.Length != o._holes.Length) return false;
            for (int i = 0; i < _holes.Length; i++)
            {
                if (!_holes[i].EqualsExact(o._holes[i], tolerance))
                    return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public override void Apply(ICoordinateFilter filter)
        {
            _shell.Apply(filter);
            foreach (var hole in _holes) hole.Apply(filter);
        }

        /// <inheritdoc/>
        public override void Apply(ICoordinateSequenceFilter filter)
        {
            _shell.Apply(filter);
            if (!filter.Done)
            {
                foreach (var hole in _holes)
                {
                    hole.Apply(filter);
                    if (filter.Done) break;
                }
            }
            if (filter.GeometryChanged) GeometryChanged();
        }

        /// <inheritdoc/>
        public override void Apply(IEntireCoordinateSequenceFilter filter)
        {
            _shell.Apply(filter);
            if (!filter.Done)
            {
                foreach (var hole in _holes)
                {
                    hole.Apply(filter);
                    if (filter.Done) break;
                }
            }
            if (filter.GeometryChanged) GeometryChanged();
        }

        /// <inheritdoc/>
        public override void Apply(IGeometryFilter filter) => filter.Filter(this);

        /// <inheritdoc/>
        public override void Apply(IGeometryComponentFilter filter)
        {
            filter.Filter(this);
            _shell.Apply(filter);
            foreach (var hole in _holes) hole.Apply(filter);
        }

        /// <inheritdoc/>
        protected override Geometry CopyInternal()
        {
            var holes = new Curve[_holes.Length];
            for (int i = 0; i < _holes.Length; i++)
            {
                holes[i] = (Curve)_holes[i].Copy();
            }
            return new CurvePolygon((Curve)_shell.Copy(), holes, Factory);
        }

        /// <summary>
        /// Normalization of ring orientation and hole ordering requires orientation
        /// of curved rings, which is part of the prototype roadmap.  This prototype
        /// implementation does nothing.
        /// </summary>
        public override void Normalize()
        {
        }

        /// <inheritdoc/>
        protected override Geometry ReverseInternal()
        {
            var holes = new Curve[_holes.Length];
            for (int i = 0; i < _holes.Length; i++)
            {
                holes[i] = (Curve)_holes[i].Reverse();
            }
            return new CurvePolygon((Curve)_shell.Reverse(), holes, Factory);
        }

        /// <inheritdoc/>
        protected override bool IsEquivalentClass(Geometry other) => other is CurvePolygon;

        /// <summary>
        /// CompareTo for two CurvePolygons uses lex order of the shell coordinates,
        /// then the hole count, then lex order of the hole coordinates (type-blind
        /// across ring kinds).
        /// </summary>
        protected internal override int CompareToSameClass(object o)
        {
            var other = (CurvePolygon)o;
            int c = CompareCoordinates(_shell.Coordinates, other._shell.Coordinates);
            if (c != 0) return c;
            c = _holes.Length.CompareTo(other._holes.Length);
            if (c != 0) return c;
            for (int i = 0; i < _holes.Length; i++)
            {
                c = CompareCoordinates(_holes[i].Coordinates, other._holes[i].Coordinates);
                if (c != 0) return c;
            }
            return 0;
        }

        /// <inheritdoc/>
        protected internal override int CompareToSameClass(object o, IComparer<CoordinateSequence> comp)
        {
            var other = (CurvePolygon)o;
            var factory = Factory.CoordinateSequenceFactory;
            return comp.Compare(factory.Create(Coordinates), factory.Create(other.Coordinates));
        }

        private static int CompareCoordinates(Coordinate[] a, Coordinate[] b)
        {
            int n = Math.Min(a.Length, b.Length);
            for (int i = 0; i < n; i++)
            {
                int c = a[i].CompareTo(b[i]);
                if (c != 0) return c;
            }
            return a.Length.CompareTo(b.Length);
        }

        /// <inheritdoc/>
        protected override SortIndexValue SortIndex => SortIndexValue.CurvePolygon;

        /// <summary>
        /// Returns a chord approximation of this curve polygon as a linear
        /// <see cref="Polygon"/> whose rings are linearized control polylines.
        /// </summary>
        public Polygon Linearize() => Linearize(double.NaN);

        /// <summary>
        /// Returns a chord approximation of this curve polygon as a linear
        /// <see cref="Polygon"/>.
        /// </summary>
        /// <param name="arcSegmentLength">
        /// Passed through to curved rings when they support densification;
        /// currently reserved (control chords).
        /// </param>
        public Polygon Linearize(double arcSegmentLength)
        {
            if (IsEmpty)
            {
                return Factory.CreatePolygon();
            }

            var shell = LinearizeRing(_shell, arcSegmentLength);
            LinearRing[] holes = null;
            if (_holes.Length > 0)
            {
                holes = new LinearRing[_holes.Length];
                for (int i = 0; i < _holes.Length; i++)
                {
                    holes[i] = LinearizeRing(_holes[i], arcSegmentLength);
                }
            }
            return Factory.CreatePolygon(shell, holes);
        }

        private LinearRing LinearizeRing(Curve ring, double arcSegmentLength)
        {
            LineString line;
            switch (ring)
            {
                case CircularString circularString:
                    line = circularString.Linearize(arcSegmentLength);
                    break;
                case CompoundCurve compoundCurve:
                    line = compoundCurve.Linearize(arcSegmentLength);
                    break;
                case LinearRing linearRing:
                    return linearRing;
                case LineString lineString:
                    line = lineString;
                    break;
                default:
                    line = Factory.CreateLineString(ring.Coordinates);
                    break;
            }
            return Factory.CreateLinearRing(line.CoordinateSequence);
        }
    }
}
