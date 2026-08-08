// SPDX-License-Identifier: BSD-3-Clause
// Status: PRODUCTION (structure + WKT/WKB) — GEOS / ISO WKB type 11.
// Type + I/O only; member metric behaviour follows component types.
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
    }
}
