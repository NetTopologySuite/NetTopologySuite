using System;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Models a collection of <c>Point</c>s.
    /// </summary>
    [Serializable]
    public class MultiPoint : GeometryCollection, IPuntal
    {
        /// <summary>
        /// Represents an empty <c>MultiPoint</c>.
        /// </summary>
        public new static readonly MultiPoint Empty = new GeometryFactory().CreateMultiPoint(new Point[] { });

        /// <summary>
        /// Constructs a <c>MultiPoint</c>.
        /// </summary>
        /// <param name="points">
        /// The <c>Point</c>s for this <c>MultiPoint</c>
        /// , or <c>null</c> or an empty array to create the empty point.
        /// Elements may be empty <c>Point</c>s, but not <c>null</c>s.
        /// </param>
        /// <param name="factory"></param>
        public MultiPoint(Point[] points, GeometryFactory factory) : base(points, factory) { }

        /// <summary>
        /// Constructs a <c>MultiPoint</c>.
        /// </summary>
        /// <param name="points">
        /// The <c>Point</c>s for this <c>MultiPoint</c>
        /// , or <c>null</c> or an empty array to create the empty point.
        /// Elements may be empty <c>Point</c>s, but not <c>null</c>s.
        /// </param>
        /// <remarks>
        /// For create this <see cref="Geometry"/> is used a standard <see cref="GeometryFactory"/>
        /// with <see cref="PrecisionModel" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public MultiPoint(Point[] points) : this(points, DefaultFactory) { }

        /// <inheritdoc cref="Geometry.CopyInternal"/>>
        protected override Geometry CopyInternal()

        {
            var points = new Point[NumGeometries];
            for (int i = 0; i < points.Length; i++)
                points[i] = (Point)GetGeometryN(i).Copy();

            return new MultiPoint(points, Factory);
        }

        /// <summary>
        /// Gets a value to sort the geometry
        /// </summary>
        /// <remarks>
        /// NOTE:<br/>
        /// For JTS v1.17 this property's getter has been renamed to <c>getTypeCode()</c>.
        /// In order not to break binary compatibility we did not follow.
        /// </remarks>
        protected override SortIndexValue SortIndex => SortIndexValue.MultiPoint;

        /// <summary>
        ///
        /// </summary>
        public override Dimension Dimension => Dimension.Point;

        /// <summary>
        ///
        /// </summary>
        public override Dimension BoundaryDimension => Dimension.False;

        /// <summary>
        /// Returns the name of this object's interface.
        /// </summary>
        /// <returns>"MultiPoint"</returns>
        public override string GeometryType => Geometry.TypeNameMultiPoint;

        public override OgcGeometryType OgcGeometryType => OgcGeometryType.MultiPoint;

        /// <summary>
       /// Gets the boundary of this geometry.
       /// Zero-dimensional geometries have no boundary by definition,
       /// so an empty GeometryCollection is returned.
       /// </summary>
       public override Geometry Boundary => Factory.CreateGeometryCollection();

        ///// <summary>
        /////
        ///// </summary>
        //public override bool IsSimple
        //{
        //    get
        //    {
        //        return (new IsSimpleOp()).IsSimple(this);
        //    }
        //}

        /// <inheritdoc cref="ReverseInternal"/>
        protected override Geometry ReverseInternal()
        {
            var points = new Point[Geometries.Length];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = (Point)Geometries[i].Copy();
            }
            return new MultiPoint(points, Factory);
        }

        /// <inheritdoc cref="Geometry.IsValid"/>
        // Note: this is left here for API compatibility!
        public override bool IsValid => base.IsValid;

        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public override bool EqualsExact(Geometry other, double tolerance)
        {
            if (!IsEquivalentClass(other))
                return false;
            return base.EqualsExact(other, tolerance);
        }

        /// <summary>
        /// Returns the <c>Coordinate</c> at the given position.
        /// </summary>
        /// <param name="n">The index of the <c>Coordinate</c> to retrieve, beginning at 0.
        /// </param>
        /// <returns>The <c>n</c>th <c>Coordinate</c>.</returns>
        protected Coordinate GetCoordinate(int n)
        {
            return Geometries[n].Coordinate;
        }
    }
}
