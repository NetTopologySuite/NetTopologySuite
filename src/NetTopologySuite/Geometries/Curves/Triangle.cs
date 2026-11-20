// SPDX-License-Identifier: BSD-3-Clause
//
// AI assistance disclosure: AI-drafted, human-reviewed.
//   Assisted-by: Claude (Opus-4.7)
//
// Status: PRODUCTION (structure + WKT/WKB) — OGC SFA Triangle on Surface<T>.

using System;
using System.Collections.Generic;

namespace NetTopologySuite.Geometries.Curves
{
    /// <summary>
    /// An OGC SFA <c>Triangle</c>: a <see cref="Surface{T}"/> whose exterior ring has
    /// exactly four coordinates (three distinct vertices plus a closing repeat) and
    /// no interior rings.
    /// </summary>
    /// <remarks>
    /// This is a sibling of the existing <see cref="Geometries.Triangle"/> utility
    /// class -- they live in different namespaces.  The utility class provides
    /// <c>InCentre</c> / <c>IsAcute</c> / <c>PerpendicularBisector</c> on raw
    /// <see cref="Coordinate"/>s without participating in the geometry hierarchy;
    /// this class is a full <see cref="Geometry"/> that takes part in WKT, WKB,
    /// envelopes, predicates, etc.
    /// </remarks>
    [Serializable]
    public class Triangle : Surface<LinearRing>
    {
        private readonly LinearRing _shell;

        /// <summary>
        /// Initializes a new instance of the <see cref="Triangle"/> class from a
        /// closed 4-coordinate shell.
        /// </summary>
        /// <param name="shell">A linear ring with exactly four coordinates (or empty)</param>
        /// <param name="factory">The geometry factory</param>
        /// <exception cref="ArgumentException">
        /// If the shell is non-empty and does not have exactly four coordinates.
        /// </exception>
        public Triangle(LinearRing shell, GeometryFactory factory) : base(factory)
        {
            shell = shell ?? factory.CreateLinearRing((CoordinateSequence)null);
            if (!shell.IsEmpty && shell.NumPoints != 4)
            {
                throw new ArgumentException(
                    "A Triangle must have a 4-coordinate shell (3 distinct vertices " +
                    "plus a closing repeat), got " + shell.NumPoints + " points.",
                    nameof(shell));
            }
            _shell = shell;
        }

        /// <inheritdoc cref="Geometry.GeometryType"/>
        public override string GeometryType => "Triangle";

        /// <inheritdoc cref="Geometry.OgcGeometryType"/>
        public override OgcGeometryType OgcGeometryType => OgcGeometryType.Triangle;

        /// <inheritdoc cref="Geometry.IsEmpty"/>
        public override bool IsEmpty => _shell.IsEmpty;

        /// <inheritdoc cref="Geometry.NumPoints"/>
        public override int NumPoints => _shell.NumPoints;

        /// <inheritdoc cref="Geometry.Coordinate"/>
        public override Coordinate Coordinate => _shell.Coordinate;

        /// <inheritdoc cref="Geometry.Coordinates"/>
        public override Coordinate[] Coordinates => _shell.Coordinates;

        /// <inheritdoc/>
        public override double[] GetOrdinates(Ordinate ordinate) => _shell.GetOrdinates(ordinate);

        /// <inheritdoc cref="Surface{T}.ExteriorRing"/>
        public override LinearRing ExteriorRing => _shell;

        /// <inheritdoc cref="Surface{T}.NumInteriorRings"/>
        public override int NumInteriorRings => 0;

        /// <inheritdoc cref="Surface{T}.GetInteriorRingN"/>
        public override LinearRing GetInteriorRingN(int index)
        {
            throw new ArgumentOutOfRangeException(nameof(index),
                "A Triangle has no interior rings.");
        }

        /// <summary>
        /// Signed twice-area of the triangle (positive for CCW vertices, negative
        /// for CW, zero for a degenerate triangle).
        /// </summary>
        public double SignedDoubleArea
        {
            get
            {
                if (IsEmpty) return 0.0;
                var c = _shell.Coordinates;
                return (c[1].X - c[0].X) * (c[2].Y - c[0].Y)
                     - (c[2].X - c[0].X) * (c[1].Y - c[0].Y);
            }
        }

        /// <inheritdoc cref="Geometry.Area"/>
        public override double Area => Math.Abs(SignedDoubleArea) / 2.0;

        /// <inheritdoc cref="Geometry.Length"/>
        public override double Length => _shell.Length;

        /// <inheritdoc cref="Geometry.Boundary"/>
        public override Geometry Boundary => IsEmpty
            ? (Geometry)Factory.CreateMultiLineString()
            : Factory.CreateLineString(_shell.CoordinateSequence.Copy());

        /// <inheritdoc/>
        protected override Envelope ComputeEnvelopeInternal() => _shell.EnvelopeInternal;

        /// <inheritdoc/>
        public override bool EqualsExact(Geometry other, double tolerance)
        {
            if (!IsEquivalentClass(other)) return false;
            var o = (Triangle)other;
            return _shell.EqualsExact(o._shell, tolerance);
        }

        /// <inheritdoc/>
        public override void Apply(ICoordinateFilter filter) => _shell.Apply(filter);

        /// <inheritdoc/>
        public override void Apply(ICoordinateSequenceFilter filter)
        {
            _shell.Apply(filter);
            if (filter.GeometryChanged) GeometryChanged();
        }

        /// <inheritdoc/>
        public override void Apply(IEntireCoordinateSequenceFilter filter)
        {
            _shell.Apply(filter);
            if (filter.GeometryChanged) GeometryChanged();
        }

        /// <inheritdoc/>
        public override void Apply(IGeometryFilter filter) => filter.Filter(this);

        /// <inheritdoc/>
        public override void Apply(IGeometryComponentFilter filter)
        {
            filter.Filter(this);
            _shell.Apply(filter);
        }

        /// <inheritdoc/>
        protected override Geometry CopyInternal() => new Triangle((LinearRing)_shell.Copy(), Factory);

        /// <inheritdoc/>
        public override void Normalize() => _shell.Normalize();

        /// <inheritdoc/>
        protected override Geometry ReverseInternal() =>
            new Triangle((LinearRing)_shell.Reverse(), Factory);

        /// <inheritdoc/>
        protected override bool IsEquivalentClass(Geometry other) => other is Triangle;

        /// <inheritdoc/>
        protected internal override int CompareToSameClass(object o)
        {
            return _shell.CompareTo(((Triangle)o)._shell);
        }

        /// <inheritdoc/>
        protected internal override int CompareToSameClass(object o, IComparer<CoordinateSequence> comp)
        {
            return comp.Compare(_shell.CoordinateSequence, ((Triangle)o)._shell.CoordinateSequence);
        }

        /// <inheritdoc/>
        protected override SortIndexValue SortIndex => SortIndexValue.Triangle;
    }
}
