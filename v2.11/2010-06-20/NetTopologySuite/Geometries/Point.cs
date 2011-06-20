using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Basic implementation of <see cref="IPoint{TCoordinate}"/>,
    /// <see cref="IPoint2D"/> and <see cref="IPoint3D"/>.
    /// </summary>
    /// <typeparam name="TCoordinate">Type of coordinate to use.</typeparam>
    [Serializable]
    public class Point<TCoordinate> : Geometry<TCoordinate>,
                                      IPoint<TCoordinate>,
                                      IPoint3D
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        ///// <summary>
        ///// Represents an empty <see cref="Point{TCoordinate}"/>.
        ///// </summary>
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

        #region IPoint<TCoordinate> Members

        public override ICoordinateSequence<TCoordinate> Coordinates
        {
            get
            {
                if (!IsEmpty)
                    return Factory.CoordinateSequenceFactory.Create(Coordinate);
                return null;
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

        public TCoordinate Coordinate
        {
            get { return _coordinate; }
        }

        public override OgcGeometryType GeometryType
        {
            get { return OgcGeometryType.Point; }
        }

        public override IEnumerable<TCoordinate> GetVertexes(ITransformMatrix<NPack.DoubleComponent> transform)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<TCoordinate> GetVertexes()
        {
            yield return _coordinate;
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

        public override Boolean EqualsExact(IGeometry<TCoordinate> g, Tolerance tolerance)
        {
            if (g == null) throw new ArgumentNullException("g");

            if (!IsEquivalentClass(g))
            {
                return false;
            }

            if (IsEmpty && g.IsEmpty)
            {
                return true;
            }

            IPoint<TCoordinate> point = g as IPoint<TCoordinate>;
            Debug.Assert(point != null);

            return tolerance.Equal(0, _coordinate.Distance(point.Coordinate));
        }

        public override IGeometry<TCoordinate> Clone()
        {
            return Factory.CreatePoint(Coordinate);
        }

        public override void Normalize()
        {
        }

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
                                                          "Ordinate value doesn't " +
                                                          "exist in this point");
                }
            }
        }

        ICoordinate IPoint.Coordinate
        {
            get { return Coordinate; }
        }

        public Int32 OrdinateCount
        {
            get { return _coordinate.ComponentCount; }
        }

        IPoint IAddable<Double, IPoint>.Add(Double b)
        {
            throw new NotImplementedException();
        }

        IPoint ISubtractable<Double, IPoint>.Subtract(Double b)
        {
            throw new NotImplementedException();
        }

        IPoint IComputable<Double, IPoint>.Set(Double value)
        {
            throw new NotImplementedException();
        }

        IPoint IComputable<IPoint>.Set(Double value)
        {
            throw new NotImplementedException();
        }

        IPoint IComputable<IPoint>.Abs()
        {
            throw new NotImplementedException();
        }

        IPoint INegatable<IPoint>.Negative()
        {
            throw new NotImplementedException();
        }

        IPoint ISubtractable<IPoint>.Subtract(IPoint b)
        {
            throw new NotImplementedException();
        }

        IPoint IHasZero<IPoint>.Zero
        {
            get
            {
                Double[] ordinates = new Double[(Int32)Dimension];
                return Factory.CreatePoint(Factory.CoordinateFactory.Create(ordinates));
            }
        }

        IPoint IAddable<IPoint>.Add(IPoint b)
        {
            throw new NotImplementedException();
        }

        IPoint IDivisible<IPoint>.Divide(IPoint b)
        {
            throw new NotImplementedException();
        }

        IPoint IHasOne<IPoint>.One
        {
            get { throw new NotImplementedException(); }
        }

        IPoint IMultipliable<IPoint>.Multiply(IPoint b)
        {
            throw new NotImplementedException();
        }

        Boolean IBooleanComparable<IPoint>.GreaterThan(IPoint value)
        {
            throw new NotImplementedException();
        }

        Boolean IBooleanComparable<IPoint>.GreaterThanOrEqualTo(IPoint value)
        {
            throw new NotImplementedException();
        }

        Boolean IBooleanComparable<IPoint>.LessThan(IPoint value)
        {
            throw new NotImplementedException();
        }

        Boolean IBooleanComparable<IPoint>.LessThanOrEqualTo(IPoint value)
        {
            throw new NotImplementedException();
        }

        IPoint IExponential<IPoint>.Exp()
        {
            throw new NotSupportedException();
        }

        IPoint IExponential<IPoint>.Log()
        {
            throw new NotSupportedException();
        }

        IPoint IExponential<IPoint>.Log(Double newBase)
        {
            throw new NotSupportedException();
        }

        IPoint IExponential<IPoint>.Power(Double exponent)
        {
            throw new NotSupportedException();
        }

        IPoint IExponential<IPoint>.Sqrt()
        {
            throw new NotImplementedException();
        }

        IPoint IMultipliable<Double, IPoint>.Multiply(Double b)
        {
            // TODO: need to disambiguate the interface call here...
            TCoordinate p = ((IMultipliable<Double, TCoordinate>)_coordinate).Multiply(b);
            return Factory.CreatePoint(p);
        }

        IPoint IDivisible<Double, IPoint>.Divide(Double b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IPoint3D Members

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

        public Double Z
        {
            get
            {
                if (Coordinates<TCoordinate>.IsEmpty(Coordinate))
                {
                    throw new InvalidOperationException("Z called on empty Point");
                }

                return Coordinate[Ordinates.Z];
            }
        }

        #endregion

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