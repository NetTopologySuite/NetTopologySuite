using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Operation;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>  
    /// Models a collection of <c>Point</c>s.
    /// </summary>
    [Serializable]
    public class MultiPoint : GeometryCollection, IMultiPoint
    {
        /// <summary>
        /// Represents an empty <c>MultiPoint</c>.
        /// </summary>
        public new static readonly IMultiPoint Empty = new GeometryFactory().CreateMultiPoint(new IPoint[] {});

        /// <summary>
        /// Constructs a <c>MultiPoint</c>.
        /// </summary>
        /// <param name="points">
        /// The <c>Point</c>s for this <c>MultiPoint</c>
        /// , or <see langword="null" /> or an empty array to create the empty point.
        /// Elements may be empty <c>Point</c>s, but not <see langword="null" />s.
        /// </param>
        public MultiPoint(IPoint[] points, IGeometryFactory factory) : base(points, factory) {}

        /// <summary>
        /// Constructs a <c>MultiPoint</c>.
        /// </summary>
        /// <param name="points">
        /// The <c>Point</c>s for this <c>MultiPoint</c>
        /// , or <see langword="null" /> or an empty array to create the empty point.
        /// Elements may be empty <c>Point</c>s, but not <see langword="null" />s.
        /// </param>
        /// <remarks>
        /// For create this <see cref="Geometry{TCoordinate}"/> is used a standard <see cref="GeometryFactory{TCoordinate}"/> 
        /// with <see cref="PrecisionModel" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public MultiPoint(IPoint[] points) : this(points, DefaultFactory) {}

        public override Dimensions Dimension
        {
            get { return Dimensions.Point; }
        }

        public override Dimensions BoundaryDimension
        {
            get { return Dimensions.False; }
        }

        public override string GeometryType
        {
            get { return "MultiPoint"; }
        }

        public override IGeometry Boundary
        {
            get { return Factory.CreateGeometryCollection(null); }
        }

        public override Boolean IsSimple
        {
            get { return (new IsSimpleOp()).IsSimple(this); }
        }

        public override Boolean IsValid
        {
            get { return true; }
        }
        public override Boolean EqualsExact(IGeometry other, Double tolerance)
        {
            if (!IsEquivalentClass(other))
            {
                return false;
            }

            return base.EqualsExact(other, tolerance);
        }

        /// <summary>
        /// Returns the <c>Coordinate</c> at the given position.
        /// </summary>
        /// <param name="n">The index of the <c>Coordinate</c> to retrieve, beginning at 0.
        /// </param>
        /// <returns>The <c>n</c>th <c>Coordinate</c>.</returns>
        protected ICoordinate GetCoordinate(Int32 n)
        {
            return geometries[n].Coordinate;
        }
    }
}