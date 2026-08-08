// SPDX-License-Identifier: BSD-3-Clause
// Dovetailed with GEOS MultiSurface (libgeos/geos curve hierarchy).
// Assisted-by: xAI Grok

using System;

namespace NetTopologySuite.Geometries.Curves
{
    /// <summary>
    /// A SQL/MM <c>MultiSurface</c>: a collection of surfaces
    /// (<see cref="Polygon"/>, <see cref="CurvePolygon"/>).
    /// Matches GEOS <c>geom::MultiSurface</c> / ISO WKB type 12.
    /// </summary>
    [Serializable]
    public class MultiSurface : GeometryCollection, IPolygonal
    {
        /// <summary>Empty multi-surface.</summary>
        public new static readonly MultiSurface Empty = new MultiSurface(null, DefaultFactory);

        /// <summary>
        /// Constructs a <see cref="MultiSurface"/>.
        /// </summary>
        /// <param name="surfaces">Member surfaces (must implement <see cref="ISurface"/>)</param>
        /// <param name="factory">Geometry factory</param>
        public MultiSurface(Geometry[] surfaces, GeometryFactory factory)
            : base(Validate(surfaces), factory)
        {
        }

        private static Geometry[] Validate(Geometry[] surfaces)
        {
            if (surfaces == null || surfaces.Length == 0)
                return Array.Empty<Geometry>();
            for (int i = 0; i < surfaces.Length; i++)
            {
                if (surfaces[i] == null)
                    throw new ArgumentException("MultiSurface members must not be null", nameof(surfaces));
                if (!(surfaces[i] is ISurface))
                    throw new ArgumentException(
                        "MultiSurface members must be surfaces (Polygon or CurvePolygon)", nameof(surfaces));
            }
            return surfaces;
        }

        /// <inheritdoc />
        protected override SortIndexValue SortIndex => SortIndexValue.MultiSurface;

        /// <inheritdoc />
        public override Dimension Dimension => Dimension.Surface;

        /// <inheritdoc />
        public override bool HasDimension(Dimension dim) => dim == Dimension.Surface;

        /// <inheritdoc />
        public override Dimension BoundaryDimension => Dimension.Curve;

        /// <inheritdoc />
        public override string GeometryType => TypeNameMultiSurface;

        /// <inheritdoc />
        public override OgcGeometryType OgcGeometryType => OgcGeometryType.MultiSurface;

        /// <inheritdoc />
        protected override Geometry ReverseInternal()
        {
            int n = NumGeometries;
            var rev = new Geometry[n];
            for (int i = 0; i < n; i++)
                rev[i] = GetGeometryN(i).Reverse();
            return new MultiSurface(rev, Factory);
        }

        /// <inheritdoc />
        protected override Geometry CopyInternal()
        {
            int n = NumGeometries;
            var copy = new Geometry[n];
            for (int i = 0; i < n; i++)
                copy[i] = GetGeometryN(i).Copy();
            return new MultiSurface(copy, Factory);
        }
    }
}
