using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Models a collection of <c>Point</c>s.
    /// </summary>
#if HAS_SYSTEM_SERIALIZABLEATTRIBUTE
    [Serializable]
#endif
    public class MultiPoint : GeometryCollection, GeoAPI.Geometries.IMultiPoint
    {
        /// <summary>
        /// Represents an empty <c>MultiPoint</c>.
        /// </summary>
        public new static readonly GeoAPI.Geometries.IMultiPoint Empty = new GeometryFactory().CreateMultiPoint(new GeoAPI.Geometries.IPoint[] { });

        /// <summary>
        /// Constructs a <c>MultiPoint</c>.
        /// </summary>
        /// <param name="points">
        /// The <c>Point</c>s for this <c>MultiPoint</c>
        /// , or <c>null</c> or an empty array to create the empty point.
        /// Elements may be empty <c>Point</c>s, but not <c>null</c>s.
        /// </param>
        /// <param name="factory"></param>
        public MultiPoint(GeoAPI.Geometries.IPoint[] points, GeoAPI.Geometries.IGeometryFactory factory) : base(points, factory) { }

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
        /// with <see cref="PrecisionModel" /> <c> == </c> <see cref="GeoAPI.Geometries.PrecisionModels.Floating"/>.
        /// </remarks>
        public MultiPoint(GeoAPI.Geometries.IPoint[] points) : this(points, DefaultFactory) { }

        /// <inheritdoc cref="Geometry.CopyInternal"/>>
        protected override IGeometry CopyInternal()

        {
            var points = new IPoint[NumGeometries];
            for (int i = 0; i < points.Length; i++)
                points[i] = (IPoint)GetGeometryN(i).Copy();

            return new MultiPoint(points, Factory);
        }

        /// <summary>
        /// Gets a value to sort the geometry
        /// </summary>
        protected override SortIndexValue SortIndex => SortIndexValue.MultiPoint;

        /// <summary>
        ///
        /// </summary>
        public override GeoAPI.Geometries.Dimension Dimension => GeoAPI.Geometries.Dimension.Point;

        /// <summary>
        ///
        /// </summary>
        public override GeoAPI.Geometries.Dimension BoundaryDimension => GeoAPI.Geometries.Dimension.False;

        /// <summary>
        /// Returns the name of this object's interface.
        /// </summary>
        /// <returns>"MultiPoint"</returns>
        public override string GeometryType => "MultiPoint";

        public override GeoAPI.Geometries.OgcGeometryType OgcGeometryType => GeoAPI.Geometries.OgcGeometryType.MultiPoint;

        ///<summary>
       /// Gets the boundary of this geometry.
       /// Zero-dimensional geometries have no boundary by definition,
       /// so an empty GeometryCollection is returned.
       /// </summary>
       public override GeoAPI.Geometries.IGeometry Boundary => Factory.CreateGeometryCollection();

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

        /// <summary>
        ///
        /// </summary>
        public override bool IsValid => true;

        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public override bool EqualsExact(GeoAPI.Geometries.IGeometry other, double tolerance)
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
        protected GeoAPI.Geometries.Coordinate GetCoordinate(int n)
        {
            return Geometries[n].Coordinate;
        }
    }
}
