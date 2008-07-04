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
        public static new readonly IMultiPoint Empty = new GeometryFactory().CreateMultiPoint(new IPoint[] { });
        
        /// <summary>
        /// Constructs a <c>MultiPoint</c>.
        /// </summary>
        /// <param name="points">
        /// The <c>Point</c>s for this <c>MultiPoint</c>
        /// , or <c>null</c> or an empty array to create the empty point.
        /// Elements may be empty <c>Point</c>s, but not <c>null</c>s.
        /// </param>
        /// <param name="factory"></param>
        public MultiPoint(IPoint[] points, IGeometryFactory factory) : base(points, factory) { }

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
        public MultiPoint(IPoint[] points) : this(points, DefaultFactory) { }  

        /// <summary>
        /// 
        /// </summary>
        public override Dimensions Dimension 
        {
            get
            {
                return Dimensions.Point;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override Dimensions BoundaryDimension
        {
            get
            {
                return Dimensions.False;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string GeometryType
        {
            get
            {
                return "MultiPoint";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override IGeometry Boundary
        {
            get
            {
                return Factory.CreateGeometryCollection(null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsSimple
        {
            get
            {
                return (new IsSimpleOp()).IsSimple(this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsValid
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public override bool EqualsExact(IGeometry other, double tolerance) 
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
        protected ICoordinate GetCoordinate(int n) 
        {
            return geometries[n].Coordinate;
        }
    }
}
