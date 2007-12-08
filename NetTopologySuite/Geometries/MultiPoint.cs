using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Operation;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>  
    /// Models a collection of <c>Point</c>s.
    /// </summary>
    [Serializable]
    public class MultiPoint<TCoordinate> : GeometryCollection<TCoordinate>, IMultiPoint<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>, 
                            IComputable<TCoordinate>, IConvertible
    {
        /// <summary>
        /// Represents an empty <c>MultiPoint</c>.
        /// </summary>
        public new static readonly IMultiPoint Empty = new GeometryFactory<TCoordinate>().CreateMultiPoint();

        /// <summary>
        /// Constructs an empty <see cref="MultiPoint{TCoordinate}"/>.
        /// </summary>
        public MultiPoint(IGeometryFactory<TCoordinate> factory)
            : base(factory) { }

        /// <summary>
        /// Constructs a <c>MultiPoint</c>.
        /// </summary>
        /// <param name="points">
        /// The <c>Point</c>s for this <c>MultiPoint</c>
        /// , or <see langword="null" /> or an empty array to create the empty point.
        /// Elements may be empty <c>Point</c>s, but not <see langword="null" />s.
        /// </param>
        public MultiPoint(IEnumerable<IPoint<TCoordinate>> points, IGeometryFactory<TCoordinate> factory)
            : base(EnumerableConverter.Upcast<IGeometry<TCoordinate>, IPoint<TCoordinate>>(points), factory) { }

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
        /// with <see cref="IPrecisionModel{TCoordinate}" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public MultiPoint(IEnumerable<IPoint<TCoordinate>> points) : this(points, DefaultFactory) {}

        public override Dimensions Dimension
        {
            get { return Dimensions.Point; }
        }

        public override Dimensions BoundaryDimension
        {
            get { return Dimensions.False; }
        }

        public override OgcGeometryType GeometryType
        {
            get { return OgcGeometryType.MultiPoint; }
        }

        public override IGeometry<TCoordinate> Boundary
        {
            get { return Factory.CreateGeometryCollection(null); }
        }

        public override Boolean IsSimple
        {
            get { return (new IsSimpleOp<TCoordinate>()).IsSimple(this); }
        }

        public override Boolean IsValid
        {
            get { return true; }
        }

        public override Boolean Equals(IGeometry<TCoordinate> other, Tolerance tolerance)
        {
            if (!IsEquivalentClass(other))
            {
                return false;
            }

            return base.Equals(other, tolerance);
        }

        /// <summary>
        /// Returns the <c>Coordinate</c> at the given position.
        /// </summary>
        /// <param name="n">The index of the <c>Coordinate</c> to retrieve, beginning at 0.
        /// </param>
        /// <returns>The <c>n</c>th <c>Coordinate</c>.</returns>
        protected TCoordinate GetCoordinate(Int32 n)
        {
            return this[n].Coordinate;
        }

        public new IEnumerator<IPoint<TCoordinate>> GetEnumerator()
        {
            foreach (IPoint<TCoordinate> point in this)
            {
                yield return point;
            }
        }

        #region IMultiPoint<TCoordinate> Members

        public new IPoint<TCoordinate> this[Int32 index]
        {
            get { return base[index] as IPoint<TCoordinate>; }
            set { base[index] = value; }
        }

        #endregion

        #region IMultiPoint Members

        IPoint IMultiPoint.this[Int32 index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        #endregion

        IEnumerator<IPoint> IEnumerable<IPoint>.GetEnumerator()
        {
            foreach (IPoint point in this)
            {
                yield return point;
            }
        }
    }
}