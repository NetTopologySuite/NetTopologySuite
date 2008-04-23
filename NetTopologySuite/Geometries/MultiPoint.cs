using System;
using System.Collections;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Operation;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>  
    /// Models a collection of <see cref="IPoint{TCoordinate}"/>s.
    /// </summary>
    [Serializable]
    public class MultiPoint<TCoordinate> : GeometryCollection<TCoordinate>, 
                                           IMultiPoint<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible, 
                            IComputable<Double, TCoordinate>
    {
        ///// <summary>
        ///// Represents an empty <c>MultiPoint</c>.
        ///// </summary>
        //public new static readonly IMultiPoint Empty = new GeometryFactory<TCoordinate>().CreateMultiPoint();

        /// <summary>
        /// Constructs an empty <see cref="MultiPoint{TCoordinate}"/>.
        /// </summary>
        public MultiPoint(IGeometryFactory<TCoordinate> factory)
            : base(factory) { }

        /// <summary>
        /// Constructs a <see cref="MultiPoint{TCoordinate}"/>.
        /// </summary>
        /// <param name="points">
        /// The <c>Point</c>s for this <see cref="MultiPoint{TCoordinate}"/>, 
        /// or <see langword="null" /> or an empty array to create the empty point.
        /// Elements may be empty <see cref="Point{TCoordinate}"/>s, but not <see langword="null" />s.
        /// </param>
        public MultiPoint(IEnumerable<IPoint<TCoordinate>> points, 
                          IGeometryFactory<TCoordinate> factory)
            : base(Enumerable.Upcast<IGeometry<TCoordinate>, IPoint<TCoordinate>>(points), factory) { }

        /// <summary>
        /// Constructs a <see cref="MultiPoint{TCoordinate}"/>.
        /// </summary>
        /// <param name="points">
        /// The <see cref="Point{TCoordinate}"/>s for this <see cref="MultiPoint{TCoordinate}"/>, 
        /// or <see langword="null" /> or an empty array to create the empty point.
        /// Elements may be empty <see cref="Point{TCoordinate}"/>s, 
        /// but not <see langword="null" />s.
        /// </param>
        /// <remarks>
        /// For create this <see cref="Geometry{TCoordinate}"/> 
        /// is used a standard <see cref="GeometryFactory{TCoordinate}"/> 
        /// with <see cref="IPrecisionModel{TCoordinate}" /> <c> == </c> <see cref="PrecisionModelType.Floating"/>.
        /// </remarks>
        public MultiPoint(IEnumerable<IPoint<TCoordinate>> points) 
            : this(points, ExtractGeometryFactory(Enumerable.Upcast<IGeometry<TCoordinate>, IPoint<TCoordinate>>(points))) {}

        //public MultiPoint(IEnumerable<TCoordinate> points)
        //    : this(DefaultFactory.CreateMultiPoint(points), DefaultFactory) { }

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
            foreach (IPoint<TCoordinate> point in GeometriesInternal)
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
            foreach (IPoint point in GeometriesInternal)
            {
                yield return point;
            }
        }

        protected override void CheckItemType(IGeometry<TCoordinate> item)
        {
            if (!(item is IPoint<TCoordinate>))
            {
                throw new InvalidOperationException(String.Format(
                                                        "Cannot add geometry of type {0} " +
                                                        "to a MultiPoint",
                                                        item.GetType()));
            }
        }
    }
}