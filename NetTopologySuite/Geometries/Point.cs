using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Basic implementation of <c>Point</c>.
    /// </summary>
    [Serializable]
    public class Point<TCoordinate> : Geometry<TCoordinate>, IPoint<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        private static readonly TCoordinate emptyCoordinate = default(TCoordinate);

        /// <summary>
        /// Represents an empty <see cref="Point{TCoordinate}"/>.
        /// </summary>
        public static readonly IPoint<TCoordinate> Empty = new GeometryFactory<TCoordinate>().CreatePoint(emptyCoordinate);

        private readonly TCoordinate _coordinate;

        /// <summary>
        /// Initializes a new instance of the <see cref="Point{TCoordinate}"/> class.
        /// </summary>
        /// <param name="coordinate">The coordinate used for create this <see cref="Point{TCoordinate}" />.</param>
        /// <remarks>
        /// For create this <see cref="Geometry{TCoordinate}"/> is used a standard <see cref="GeometryFactory{TCoordinate}"/> 
        /// with <see cref="PrecisionModel{TCoordinate}" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public Point(TCoordinate coordinate) :
            this(coordinate, GeometryFactory<TCoordinate>.Default) { }

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

        protected override Extents<TCoordinate> ComputeExtentsInternal()
        {
            if (IsEmpty)
            {
                return new Extents<TCoordinate>();
            }

            return new Extents<TCoordinate>(Coordinate, Coordinate);
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

        public override void Apply(IGeometryComponentFilter<TCoordinate> filter)
        {
            filter.Filter(this);
        }

        public override IGeometry<TCoordinate> Clone()
        {
            return Factory.CreatePoint(Coordinate);
        }

        public override void Normalize() { }

        protected internal override Int32 CompareToSameClass(IGeometry<TCoordinate> other)
        {
            IPoint<TCoordinate> point = other as IPoint<TCoordinate>;

            if (point == null)
            {
                throw new ArgumentException("Parameter must be of type IPoint<TCoordinate>.");
            }

            return Coordinate.CompareTo(point.Coordinate);
        }

        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        /// <summary>
        /// Initializes a new instance of the <see cref="Point{TCoordinate}"/> class.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        /// /// <remarks>
        /// For create this <see cref="Geometry{TCoordinate}"/> is used a standard <see cref="GeometryFactory{TCoordinate}"/> 
        /// with <see cref="IPrecisionModel{TCoordinate}" /> <c> set to </c> <see cref="PrecisionModelType.Floating"/>.
        /// </remarks>
        public Point(Double x, Double y, Double z) :
            this(DefaultFactory.CoordinateFactory.Create3D(x, y, z), DefaultFactory) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Point{TCoordinate}"/> class.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// /// <remarks>
        /// For create this <see cref="Geometry{TCoordinate}"/> is used a standard <see cref="GeometryFactory{TCoordinate}"/> 
        /// with <see cref="IPrecisionModel{TCoordinate}" /> <c> set to </c> <see cref="PrecisionModelType.Floating"/>.
        /// </remarks>
        public Point(Double x, Double y)
            : this(DefaultFactory.CoordinateFactory.Create(x, y), DefaultFactory) { }

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
                                                          "Ordinate value doesn't exist in this point");
                }
            }
        }

        ICoordinate IPoint.Coordinate
        {
            get { return Coordinate; }
        }

        #endregion
    }
}