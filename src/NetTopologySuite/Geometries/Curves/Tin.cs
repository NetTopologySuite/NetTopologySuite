// SPDX-License-Identifier: BSD-3-Clause
//
// AI assistance disclosure: AI-drafted, human-reviewed.
//   Assisted-by: Claude (Opus-4.7)
//
// Status: PLAYGROUND -- prototype of OGC SFA-CA Triangulated Irregular
// Network (TIN), modeled as a GeometryCollection of Triangle (also defined in
// this Curves namespace).  Not for merge.

using System;

namespace NetTopologySuite.Geometries.Curves
{
    /// <summary>
    /// An OGC SFA-CA Triangulated Irregular Network: a homogeneous collection of
    /// <see cref="Triangle"/>s.  Conceptually a piecewise-linear surface, but
    /// represented here as a <see cref="GeometryCollection"/> for tooling
    /// compatibility on the prototype branch.
    /// </summary>
    /// <remarks>
    /// All elements are required to be <see cref="Triangle"/> instances; the
    /// constructor enforces this.  Adjacency, shared-edge consistency, and
    /// orientation invariants are not currently checked -- those are downstream
    /// validity properties that should accompany an eventual
    /// <c>IsValid</c>-style predicate.
    /// </remarks>
    [Serializable]
    public class Tin : GeometryCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Tin"/> class.
        /// </summary>
        /// <param name="triangles">The constituent triangles (may be empty or null)</param>
        /// <param name="factory">The geometry factory</param>
        public Tin(Triangle[] triangles, GeometryFactory factory)
            : base(triangles ?? new Triangle[0], factory)
        {
            // The base ctor accepts Geometry[]; the array contravariance is fine
            // here because Triangle inherits from Geometry. The element-type
            // constraint is encoded in the (Triangle[]) parameter signature, so
            // downstream code can't slip a non-Triangle in through this entry
            // point. The Geometry[] form on the base class is still reachable
            // via the inherited APIs; an IsValid-style invariant check would
            // catch that case.
        }

        /// <inheritdoc cref="Geometry.GeometryType"/>
        public override string GeometryType => "TIN";

        /// <inheritdoc cref="Geometry.OgcGeometryType"/>
        public override OgcGeometryType OgcGeometryType => OgcGeometryType.TIN;

        /// <summary>The dimension of a TIN is that of a surface (2).</summary>
        public override Dimension Dimension => Dimension.Surface;

        /// <summary>The boundary of a TIN is curvilinear (the perimeter of the union).</summary>
        public override Dimension BoundaryDimension => Dimension.Curve;

        /// <summary>
        /// Gets the triangle at the given index.
        /// </summary>
        public Triangle GetTriangleN(int index) => (Triangle)GetGeometryN(index);

        /// <inheritdoc/>
        protected override Geometry CopyInternal()
        {
            var copies = new Triangle[NumGeometries];
            for (int i = 0; i < NumGeometries; i++)
            {
                copies[i] = (Triangle)GetGeometryN(i).Copy();
            }
            return new Tin(copies, Factory);
        }

        /// <inheritdoc/>
        protected override bool IsEquivalentClass(Geometry other) => other is Tin;

        /// <summary>
        /// The total area of the TIN, computed as the sum of triangle areas.
        /// Adjacent triangles that overlap will double-count; the prototype
        /// assumes a well-formed TIN.
        /// </summary>
        public override double Area
        {
            get
            {
                double total = 0.0;
                for (int i = 0; i < NumGeometries; i++)
                {
                    total += ((Triangle)GetGeometryN(i)).Area;
                }
                return total;
            }
        }

        /// <inheritdoc/>
        protected override SortIndexValue SortIndex => SortIndexValue.Tin;
    }
}
