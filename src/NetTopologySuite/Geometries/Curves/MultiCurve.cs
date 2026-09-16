// SPDX-License-Identifier: BSD-3-Clause
// Status: PRODUCTION (structure + WKT/WKB) — GEOS / ISO WKB type 11.
// Metrics and analytic ops (Length, Area, Envelope, IsSimple, Distance,
// Centroid, InteriorPoint) fail closed with NotSupportedException until
// arc-aware implementations land in a follow-up PR; Linearize() is the
// explicit chord escape hatch.
// Assisted-by: xAI Grok

using System;

namespace NetTopologySuite.Geometries.Curves
{
    /// <summary>
    /// A SQL/MM <c>MultiCurve</c>: a collection of <see cref="Curve"/>s
    /// (<see cref="LineString"/>, <see cref="CircularString"/>, <see cref="CompoundCurve"/>).
    /// Matches GEOS <c>geom::MultiCurve</c> / ISO WKB type 11.
    /// </summary>
    [Serializable]
    public class MultiCurve : GeometryCollection, ILineal
    {
        /// <summary>Empty multi-curve.</summary>
        public new static readonly MultiCurve Empty = new MultiCurve(null, DefaultFactory);

        /// <summary>
        /// Constructs a <see cref="MultiCurve"/>.
        /// </summary>
        /// <param name="curves">Member curves, or null/empty for empty multi-curve</param>
        /// <param name="factory">Geometry factory</param>
        public MultiCurve(Curve[] curves, GeometryFactory factory)
            : base(ToGeometryArray(curves), factory)
        {
        }

        private static Geometry[] ToGeometryArray(Curve[] curves)
        {
            if (curves == null || curves.Length == 0)
                return Array.Empty<Geometry>();
            var geoms = new Geometry[curves.Length];
            for (int i = 0; i < curves.Length; i++)
            {
                if (curves[i] == null)
                    throw new ArgumentException("MultiCurve members must not be null", nameof(curves));
                geoms[i] = curves[i];
            }
            return geoms;
        }

        /// <inheritdoc />
        protected override SortIndexValue SortIndex => SortIndexValue.MultiCurve;

        /// <inheritdoc />
        public override Dimension Dimension => Dimension.Curve;

        /// <inheritdoc />
        public override bool HasDimension(Dimension dim) => dim == Dimension.Curve;

        /// <inheritdoc />
        public override Dimension BoundaryDimension
        {
            get
            {
                if (IsClosed)
                    return Dimension.False;
                return Dimension.Point;
            }
        }

        /// <inheritdoc />
        public override string GeometryType => TypeNameMultiCurve;

        /// <inheritdoc />
        public override OgcGeometryType OgcGeometryType => OgcGeometryType.MultiCurve;

        /// <summary>True if non-empty and every member curve is closed.</summary>
        public bool IsClosed
        {
            get
            {
                if (IsEmpty)
                    return false;
                for (int i = 0; i < NumGeometries; i++)
                {
                    if (!((Curve)GetGeometryN(i)).IsClosed)
                        return false;
                }
                return true;
            }
        }

        /// <inheritdoc />
        protected override Geometry ReverseInternal()
        {
            int n = NumGeometries;
            var rev = new Curve[n];
            for (int i = 0; i < n; i++)
                rev[i] = (Curve)GetGeometryN(i).Reverse();
            return new MultiCurve(rev, Factory);
        }

        /// <inheritdoc />
        protected override Geometry CopyInternal()
        {
            int n = NumGeometries;
            var copy = new Curve[n];
            for (int i = 0; i < n; i++)
                copy[i] = (Curve)GetGeometryN(i).Copy();
            return new MultiCurve(copy, Factory);
        }

        /// <summary>
        /// Arc-aware length is not implemented yet. Empty is 0; otherwise throws,
        /// including when every member is a <see cref="LineString"/>.
        /// </summary>
        public override double Length =>
            IsEmpty ? 0d : throw CurvedGeometry.NotYetSupported(this, "Length");

        /// <inheritdoc />
        protected override Envelope ComputeEnvelopeInternal()
        {
            if (IsEmpty) return new Envelope();
            throw CurvedGeometry.NotYetSupported(this, "Envelope");
        }

        /// <summary>
        /// Arc-aware boundary is not implemented yet.
        /// </summary>
        /// <remarks>
        /// The inherited <see cref="GeometryCollection.Boundary"/> asserts because
        /// <see cref="OgcGeometryType"/> is not <c>GeometryCollection</c>.
        /// </remarks>
        public override Geometry Boundary =>
            throw CurvedGeometry.NotYetSupported(this, "Boundary");

        /// <summary>
        /// Hashes a locally computed control-point envelope.
        /// </summary>
        /// <remarks>
        /// Base <see cref="Geometry.GetHashCode"/> reads <c>EnvelopeInternal</c>,
        /// which now throws for non-empty curve types. Hashing is identity, not a
        /// geometric answer; control points are EqualsExact-consistent.
        /// </remarks>
        public override int GetHashCode() => CurvedGeometry.HashControlEnvelope(this);
    }
}
