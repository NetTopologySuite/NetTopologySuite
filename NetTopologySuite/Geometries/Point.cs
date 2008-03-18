using System;
using GeoAPI.Coordinates;
using GeoAPI.CoordinateSystems;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Basic implementation of <see cref="IPoint"/>.
    /// </summary>
    [Serializable]
    public class Point<TCoordinate> : Geometry<TCoordinate>, IPoint<TCoordinate>, IPoint2D
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        /// <summary>
        /// Represents an empty <see cref="Point{TCoordinate}"/>.
        /// </summary>
        //public static readonly IPoint<TCoordinate> Empty = new GeometryFactory<TCoordinate>().CreatePoint(emptyCoordinate);

        private readonly TCoordinate _coordinate;

        ///// <summary>
        ///// Initializes a new instance of the <see cref="Point{TCoordinate}"/> class.
        ///// </summary>
        ///// <param name="coordinate">The coordinate used for create this <see cref="Point{TCoordinate}" />.</param>
        ///// <remarks>
        ///// For create this <see cref="Geometry{TCoordinate}"/> is used a standard <see cref="GeometryFactory{TCoordinate}"/> 
        ///// with <see cref="PrecisionModel{TCoordinate}" /> <c> == </c> <see cref="PrecisionModelType.Floating"/>.
        ///// </remarks>
        //public Point(TCoordinate coordinate) :
        //    this(coordinate, GeometryFactory<TCoordinate>.Default) { }

        /// <summary>
        /// Constructs a <see cref="Point{TCoordinate}"/> with the given coordinate.
        /// </summary>
        /// <param name="coordinate">
        /// Contains the single coordinate on which to base this <see cref="Point{TCoordinate}"/>,
        /// or <see langword="null" /> to create the empty point.
        /// </param>
        public Point(TCoordinate coordinate, IGeometryFactory<TCoordinate> factory)
            : base(factory)
        {
            _coordinate = coordinate;
        }

        public override ICoordinateSequence<TCoordinate> Coordinates
        {
            get
            {
                return Factory.CoordinateSequenceFactory.Create(Coordinate);
            }
        }

        public override Int32 PointCount
        {
            get { return IsEmpty ? 0 : 1; }
        }

        public override Boolean IsEmpty
        {
            get { return Coordinates<TCoordinate>.IsEmpty(Coordinate); }
        }

        public override Boolean IsSimple
        {
            get { return true; }
        }

        public override Boolean IsValid
        {
            get { return true; }
        }

        public override Dimensions Dimension
        {
            get { return Dimensions.Point; }
        }

        public override Dimensions BoundaryDimension
        {
            get { return Dimensions.False; }
        }

        public Double X
        {
            get
            {
                if (Coordinates<TCoordinate>.IsEmpty(Coordinate))
                {
                    throw new InvalidOperationException("X called on empty Point");
                }

                return Coordinate[Ordinates.X];
            }
        }

        public Double Y
        {
            get
            {
                if (Coordinates<TCoordinate>.IsEmpty(Coordinate))
                {
                    throw new InvalidOperationException("Y called on empty Point");
                }

                return Coordinate[Ordinates.Y];
            }
        }

        public TCoordinate Coordinate
        {
            get { return _coordinate; }
        }

        public override OgcGeometryType GeometryType
        {
            get { return OgcGeometryType.Point; }
        }

        public override IGeometry<TCoordinate> Boundary
        {
            get { return Factory.CreateGeometryCollection(null); }
        }

        public override Boolean Equals(IGeometry<TCoordinate> other, Tolerance tolerance)
        {
            if (!IsEquivalentClass(other))
            {
                return false;
            }

            IPoint<TCoordinate> otherPoint = other as IPoint<TCoordinate>;

            if (otherPoint == null)
            {
                return false;
            }

            if (IsEmpty && other.IsEmpty)
            {
                return true;
            }

            return Equal(otherPoint.Coordinate, Coordinate, tolerance);
        }

        //public override void Apply(ICoordinateFilter<TCoordinate> filter)
        //{
        //    if (IsEmpty)
        //    {
        //        return;
        //    }

        //    filter.Filter(Coordinate);
        //}

        //public override void Apply(IGeometryFilter<TCoordinate> filter)
        //{
        //    filter.Filter(this);
        //}

        //public override void Apply(IGeometryComponentFilter<TCoordinate> filter)
        //{
        //    filter.Filter(this);
        //}

        public override IGeometry<TCoordinate> Clone()
        {
            return Factory.CreatePoint(Coordinate);
        }

        public override void Normalize() { }

        protected override Extents<TCoordinate> ComputeExtentsInternal()
        {
            if (IsEmpty)
            {
                return new Extents<TCoordinate>(Factory);
            }

            return new Extents<TCoordinate>(Factory, Coordinate, Coordinate);
        }

        protected internal override Int32 CompareToSameClass(IGeometry<TCoordinate> other)
        {
            if (other == null)
            {
                return 1;
            }

            IPoint<TCoordinate> point = other as IPoint<TCoordinate>;

            if (point == null)
            {
                throw new ArgumentException(
                    "Parameter must be of type IPoint<TCoordinate>.");
            }

            return Coordinate.CompareTo(point.Coordinate);
        }

        #region IPoint Members

        public Double this[Ordinates ordinate]
        {
            get
            {
                if (Coordinates<TCoordinate>.IsEmpty(Coordinate))
                {
                    throw new InvalidOperationException("Point is empty.");
                }

                if (Coordinate.ContainsOrdinate(ordinate))
                {
                    return Coordinate[ordinate];
                }
                else
                {
                    throw new ArgumentOutOfRangeException("ordinate", ordinate,
                                                          "Ordinate value doesn't "+
                                                          "exist in this point");
                }
            }
        }

        ICoordinate IPoint.Coordinate
        {
            get { return Coordinate; }
        }

        #endregion

        #region IPoint Members


        public int OrdinateCount
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IGeometry Members

        IPoint IGeometry.Centroid
        {
            get { return Centroid; }
        }

        IGeometry IGeometry.Envelope
        {
            get { return Envelope; }
        }

        IExtents IGeometry.Extents
        {
            get { return Extents; }
        }

        IGeometryFactory IGeometry.Factory
        {
            get { return Factory; }
        }

        IPrecisionModel IGeometry.PrecisionModel
        {
            get { return PrecisionModel; }
        }

        ICoordinateSystem IGeometry.SpatialReference
        {
            get { return SpatialReference; }
        }

        #endregion

        #region ISpatialOperator Members

        public double Distance(IGeometry g)
        {
            throw new NotImplementedException();
        }

        public new IGeometry Buffer(double distance)
        {
            throw new NotImplementedException();
        }

        IGeometry ISpatialOperator.Buffer(double distance, int quadrantSegments)
        {
            throw new NotImplementedException();
        }

        IGeometry ISpatialOperator.Buffer(double distance, GeoAPI.Operations.Buffer.BufferStyle endCapStyle)
        {
            throw new NotImplementedException();
        }

        IGeometry ISpatialOperator.Buffer(double distance, int quadrantSegments, GeoAPI.Operations.Buffer.BufferStyle endCapStyle)
        {
            throw new NotImplementedException();
        }

        public IGeometry Intersection(IGeometry other)
        {
            throw new NotImplementedException();
        }

        public IGeometry Union(IGeometry other)
        {
            throw new NotImplementedException();
        }

        public IGeometry Difference(IGeometry other)
        {
            throw new NotImplementedException();
        }

        public IGeometry SymmetricDifference(IGeometry other)
        {
            throw new NotImplementedException();
        }

        public new IGeometry ConvexHull()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ISpatialRelation Members

        public Boolean Equals(IGeometry g, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public Boolean Touches(IGeometry g)
        {
            throw new NotImplementedException();
        }

        public Boolean Touches(IGeometry g, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public Boolean Contains(IGeometry g)
        {
            throw new NotImplementedException();
        }

        public Boolean Contains(IGeometry g, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public Boolean Within(IGeometry g)
        {
            throw new NotImplementedException();
        }

        public Boolean Within(IGeometry g, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public Boolean Disjoint(IGeometry g)
        {
            throw new NotImplementedException();
        }

        public Boolean Disjoint(IGeometry g, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public Boolean Crosses(IGeometry g)
        {
            throw new NotImplementedException();
        }

        public Boolean Crosses(IGeometry g, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public Boolean Overlaps(IGeometry g)
        {
            throw new NotImplementedException();
        }

        public Boolean Overlaps(IGeometry g, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public Boolean Intersects(IGeometry g)
        {
            throw new NotImplementedException();
        }

        public Boolean Intersects(IGeometry g, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public Boolean IsWithinDistance(IGeometry g, double distance)
        {
            throw new NotImplementedException();
        }

        public Boolean IsWithinDistance(IGeometry g, double distance, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public Boolean IsCoveredBy(IGeometry g)
        {
            throw new NotImplementedException();
        }

        public Boolean IsCoveredBy(IGeometry g, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public Boolean Covers(IGeometry g)
        {
            throw new NotImplementedException();
        }

        public Boolean Covers(IGeometry g, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public Boolean Relate(IGeometry g, IntersectionMatrix intersectionPattern)
        {
            throw new NotImplementedException();
        }

        public Boolean Relate(IGeometry g, IntersectionMatrix intersectionPattern, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public Boolean Relate(IGeometry g, string intersectionPattern)
        {
            throw new NotImplementedException();
        }

        public Boolean Relate(IGeometry g, string intersectionPattern, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public IntersectionMatrix Relate(IGeometry g)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICloneable Members

        object ICloneable.Clone()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComputable<double,IPoint> Members

        public IPoint Set(double value)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComputable<IPoint> Members

        public IPoint Abs()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region INegatable<IPoint> Members

        public IPoint Negative()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ISubtractable<IPoint> Members

        public IPoint Subtract(IPoint b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IHasZero<IPoint> Members

        public IPoint Zero
        {
            get
            {
                Double[] ordinates = new Double[(Int32)Dimension];
                return Factory.CreatePoint(Factory.CoordinateFactory.Create(ordinates));
            }
        }

        #endregion

        #region IAddable<IPoint> Members

        public IPoint Add(IPoint b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDivisible<IPoint> Members

        public IPoint Divide(IPoint b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IHasOne<IPoint> Members

        public IPoint One
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IMultipliable<IPoint> Members

        public IPoint Multiply(IPoint b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IBooleanComparable<IPoint> Members

        public Boolean GreaterThan(IPoint value)
        {
            throw new NotImplementedException();
        }

        public Boolean GreaterThanOrEqualTo(IPoint value)
        {
            throw new NotImplementedException();
        }

        public Boolean LessThan(IPoint value)
        {
            throw new NotImplementedException();
        }

        public Boolean LessThanOrEqualTo(IPoint value)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IExponential<IPoint> Members

        IPoint IExponential<IPoint>.Exp()
        {
            throw new NotSupportedException();
        }

        IPoint IExponential<IPoint>.Log()
        {
            throw new NotImplementedException();
        }

        IPoint IExponential<IPoint>.Log(Double newBase)
        {
            throw new NotImplementedException();
        }

        IPoint IExponential<IPoint>.Power(Double exponent)
        {
            throw new NotImplementedException();
        }

        IPoint IExponential<IPoint>.Sqrt()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IMultipliable<Double, IPoint> Members

        public IPoint Multiply(Double b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDivisible<Double, IPoint> Members

        public IPoint Divide(Double b)
        {
            throw new NotImplementedException();
        }

        #endregion

        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        ///// <summary>
        ///// Initializes a new instance of the <see cref="Point{TCoordinate}"/> class.
        ///// </summary>
        ///// <param name="x">The x coordinate.</param>
        ///// <param name="y">The y coordinate.</param>
        ///// <param name="z">The z coordinate.</param>
        ///// /// <remarks>
        ///// For create this <see cref="Geometry{TCoordinate}"/> is used a standard <see cref="GeometryFactory{TCoordinate}"/> 
        ///// with <see cref="IPrecisionModel{TCoordinate}" /> <c> set to </c> <see cref="PrecisionModelType.Floating"/>.
        ///// </remarks>
        //public Point(Double x, Double y, Double z) :
        //    this(DefaultFactory.CoordinateFactory.Create3D(x, y, z), DefaultFactory) { }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="Point{TCoordinate}"/> class.
        ///// </summary>
        ///// <param name="x">The x coordinate.</param>
        ///// <param name="y">The y coordinate.</param>
        ///// /// <remarks>
        ///// For create this <see cref="Geometry{TCoordinate}"/> is used a standard <see cref="GeometryFactory{TCoordinate}"/> 
        ///// with <see cref="IPrecisionModel{TCoordinate}" /> <c> set to </c> <see cref="PrecisionModelType.Floating"/>.
        ///// </remarks>
        //public Point(Double x, Double y)
        //    : this(DefaultFactory.CoordinateFactory.Create(x, y), DefaultFactory) { }

        //public Double Z
        //{
        //    get
        //    {
        //        if (CoordinateHelper.IsEmpty(Coordinate))
        //        {
        //            throw new InvalidOperationException("Z called on empty Point");
        //        }

        //        if (!Coordinate.ContainsOrdinate(Ordinates.Z))
        //        {
        //            return 0;
        //        }
        //        else
        //        {
        //            return Coordinate[Ordinates.Z];
        //        }
        //    }
        //}

        /* END ADDED BY MPAUL42: monoGIS team */
    }
}