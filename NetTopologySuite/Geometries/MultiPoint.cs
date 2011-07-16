using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using NetTopologySuite.Operation;
using NPack.Interfaces;

#if DOTNET35
using System.Linq;
#endif

namespace NetTopologySuite.Geometries
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
            : base(factory)
        {
        }

        /// <summary>
        /// Constructs a <see cref="MultiPoint{TCoordinate}"/>.
        /// </summary>
        /// <param name="points">
        /// The <see cref="IPoint{TCoordinate}"/>s for this <see cref="MultiPoint{TCoordinate}"/>, 
        /// or <see langword="null" /> or an empty array to create the empty point.
        /// Elements may be empty <see cref="IPoint{TCoordinate}"/>s, 
        /// but not <see langword="null" />s.
        /// </param>
        /// <param name="factory">
        /// The <see cref="IGeometryFactory{TCoordinate}"/> used to create the 
        /// <see cref="IPoint{TCoordinate}"/>.
        /// </param>
        public MultiPoint(IEnumerable<IPoint<TCoordinate>> points,
                          IGeometryFactory<TCoordinate> factory)
            : base(Caster.Upcast<IGeometry<TCoordinate>, IPoint<TCoordinate>>(points), factory)
        {
        }

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
        /// with <see cref="IPrecisionModel{TCoordinate}" /> <c> == </c> <see cref="PrecisionModelType.DoubleFloating"/>.
        /// </remarks>
        public MultiPoint(IEnumerable<IPoint<TCoordinate>> points)
            : this(points, ExtractGeometryFactory(Caster.Upcast<IGeometry<TCoordinate>, IPoint<TCoordinate>>(points)))
        {
        }

        //public MultiPoint(IEnumerable<TCoordinate> points)
        //    : this(DefaultFactory.CreateMultiPoint(points), DefaultFactory) { }

        #region IMultiPoint<TCoordinate> Members

        public override Dimensions Dimension
        {
            get { return Dimensions.Point; }
        }

        public override Dimensions BoundaryDimension
        {
            get { return Dimensions.False; }
        }

        public override IGeometry<TCoordinate> Clone()
        {
            List<IPoint<TCoordinate>> points = new List<IPoint<TCoordinate>>();

            foreach (IPoint<TCoordinate> point in this)
            {
                points.Add(point.Clone() as IPoint<TCoordinate>);
            }

            return Factory.CreateMultiPoint(points);
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
            return IsEquivalentClass(other) && base.Equals(other, tolerance);
        }

        public new IEnumerator<IPoint<TCoordinate>> GetEnumerator()
        {
            foreach (IPoint<TCoordinate> point in GeometriesInternal)
            {
                yield return point;
            }
        }

        ///<summary>
        ///</summary>
        ///<param name="index"></param>
        public new IPoint<TCoordinate> this[Int32 index]
        {
            get { return base[index] as IPoint<TCoordinate>; }
            set { base[index] = value; }
        }

        IPoint IMultiPoint.this[Int32 index]
        {
            get { return this[index]; }
            set { this[index] = value as IPoint<TCoordinate>; }
        }

        IEnumerator<IPoint> IEnumerable<IPoint>.GetEnumerator()
        {
            foreach (IPoint point in GeometriesInternal)
                yield return point;
        }

        #endregion

        /// <summary>
        /// Returns the <typeparamref name="TCoordinate"/> at the given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">
        /// The index of the <typeparamref name="TCoordinate"/> 
        /// to retrieve, beginning at 0.
        /// </param>
        /// <returns>The <typeparamref name="TCoordinate"/> at <paramref name="index"/>.</returns>
        protected TCoordinate GetCoordinate(Int32 index)
        {
            return this[index].Coordinate;
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