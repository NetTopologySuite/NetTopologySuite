using System;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.Units;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Operation;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>  
    /// Basic implementation of <c>LineString</c>.
    /// </summary>  
    [Serializable]
    public class LineString<TCoordinate> : MultiCoordinateGeometry<TCoordinate>,
                                           ILineString<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        ///// <summary>
        ///// Represents an empty <c>LineString</c>.
        ///// </summary>
        //public static readonly ILineString Empty = new GeometryFactory<TCoordinate>().CreateLineString();

        /// <summary>
        /// Creates a new <see cref="LineString{TCoordinate}"/> instance from the 
        /// given <paramref name="points"/>.
        /// </summary>
        /// <param name="points">
        /// The points of the linestring, or <see langword="null" />
        /// to create the empty point. Consecutive points may not be equal.
        /// </param>
        /// <param name="factory">
        /// The <see cref="IGeometryFactory{TCoordinate}"/> instance used to generate
        /// </param>
        public LineString(ICoordinateSequence<TCoordinate> points, IGeometryFactory<TCoordinate> factory)
            : base(factory)
        {
            if (points == null)
            {
                // 3D_UNSAFE
                points = factory.CoordinateSequenceFactory.Create(CoordinateDimensions.Two);
            }

            if (points.Count == 1)
            {
                throw new ArgumentException("point array must contain 0 or >1 elements", "points");
            }

            CoordinatesInternal = points;
        }

        public TCoordinate this[Int32 index]
        {
            get { return CoordinatesInternal[index]; }
            set { CoordinatesInternal[index] = value; }
        }

        public Int32 Count
        {
            get { return CoordinatesInternal.Count; }
        }

        /// <summary>
        /// Returns the value of the angle between the <see cref="StartPoint" />
        /// and the <see cref="EndPoint" />.
        /// </summary>
        public Degrees Angle
        {
            get
            {
                TCoordinate startPoint = CoordinatesInternal.First;
                TCoordinate endPoint = CoordinatesInternal.Last;

                Double startX = startPoint[Ordinates.X], startY = startPoint[Ordinates.Y];
                Double endX = endPoint[Ordinates.X], endY = endPoint[Ordinates.Y];

                Double deltaX = endPoint[Ordinates.X] - startPoint[Ordinates.X];
                Double deltaY = endPoint[Ordinates.Y] - startPoint[Ordinates.Y];
                Double length = Math.Sqrt(deltaX*deltaX + deltaY*deltaY);
                Radians radians = (Radians) Math.Asin(Math.Abs(endY - startY)/length);
                Degrees angle = (Degrees) radians;

                if (((startX < endX) && (startY > endY)) ||
                    ((startX > endX) && (startY < endY)))
                {
                    angle = (Degrees) 360 - angle;
                }

                return angle;
            }
        }

        #region ILineString<TCoordinate> Members

        public override Dimensions Dimension
        {
            get { return Dimensions.Curve; }
        }

        public override Dimensions BoundaryDimension
        {
            get
            {
                if (IsClosed)
                {
                    return Dimensions.False;
                }

                return Dimensions.Point;
            }
        }

        public override Boolean IsEmpty
        {
            get { return CoordinatesInternal.Count == 0; }
        }

        public override Int32 PointCount
        {
            get { return CoordinatesInternal.Count; }
        }

        public IPoint<TCoordinate> GetPoint(Int32 index)
        {
            return Factory.CreatePoint(CoordinatesInternal[index]);
        }

        public IPoint<TCoordinate> StartPoint
        {
            get
            {
                if (IsEmpty)
                {
                    return null;
                }

                return GetPoint(0);
            }
        }

        public IPoint<TCoordinate> EndPoint
        {
            get
            {
                if (IsEmpty)
                {
                    return null;
                }

                return GetPoint(PointCount - 1);
            }
        }

        public virtual Boolean IsClosed
        {
            get
            {
                if (IsEmpty)
                {
                    return false;
                }
                return CoordinatesInternal.First.Equals(CoordinatesInternal.Last);

                //TCoordinate first = Slice.GetFirst(Coordinates);
                //TCoordinate last = Slice.GetLast(Coordinates);

                //return first.Equals(last);
            }
        }

        public Boolean IsRing
        {
            get { return IsClosed && IsSimple; }
        }

        public override OgcGeometryType GeometryType
        {
            get { return OgcGeometryType.LineString; }
        }

        /// <summary>  
        /// Returns the length of this <c>LineString</c>
        /// </summary>
        /// <returns>The length of the polygon.</returns>
        public override Double Length
        {
            get { return CGAlgorithms<TCoordinate>.Length(CoordinatesInternal); }
        }

        public override Boolean IsSimple
        {
            get { return (new IsSimpleOp<TCoordinate>()).IsSimple(this); }
        }

        public override IGeometry<TCoordinate> Boundary
        {
            get
            {
                return new BoundaryOp<TCoordinate>(this).GetBoundary();
                //if (IsEmpty)
                //{
                //    return Factory.CreateGeometryCollection();
                //}

                //if (IsClosed)
                //{
                //    return Factory.CreateMultiPoint();
                //}

                //return Factory.CreateMultiPoint(StartPoint, EndPoint);
            }
        }

        /// <summary>
        /// Creates an <see cref="ILineString{TCoordinate}" /> whose coordinates 
        /// are in the reverse order of this objects.
        /// </summary>
        /// <returns>
        /// An <see cref="ILineString{TCoordinate}" /> with coordinates 
        /// in the reverse order.
        /// </returns>
        public ILineString<TCoordinate> Reverse()
        {
            ICoordinateSequence<TCoordinate> seq = CoordinatesInternal.Clone();
            Debug.Assert(seq != null);
            seq.Reverse();
            return Factory.CreateLineString(seq);
        }

        public override Boolean Equals(IGeometry<TCoordinate> other, Tolerance tolerance)
        {
            if (!IsEquivalentClass(other))
            {
                return false;
            }

            ILineString<TCoordinate> otherLineString = other as ILineString<TCoordinate>;

            if (ReferenceEquals(otherLineString, null))
            {
                return false;
            }

            if (PointCount != otherLineString.PointCount)
            {
                return false;
            }

            for (Int32 i = 0; i < CoordinatesInternal.Count; i++)
            {
                if (!Equal(Coordinates[i], otherLineString.Coordinates[i], tolerance))
                {
                    return false;
                }
            }

            return true;
        }

        /*
         * [codekaizen 2008-01-14] removed when replaced visitor patterns with
         *                         enumeration / query patterns
         */

        //public override void Apply(ICoordinateFilter filter)
        //{
        //    for (Int32 i = 0; i < points.TotalItemCount; i++)
        //    {
        //        filter.Filter(points.GetCoordinate(i));
        //    }
        //}

        //public override void Apply(IGeometryFilter filter)
        //{
        //    filter.Filter(this);
        //}

        //public override void Apply(IGeometryComponentFilter<TCoordinate> filter)
        //{
        //    filter.Filter(this);
        //}

        public override IGeometry<TCoordinate> Clone()
        {
            return Factory.CreateLineString(Coordinates);
        }

        /// <summary> 
        /// Normalizes a <see cref="LineString{TCoordinate}"/>.  
        /// A normalized <see cref="LineString{TCoordinate}"/> 
        /// has the first point which 
        /// is not equal to it's reflected point
        /// less than the reflected point.
        /// </summary>
        public override void Normalize()
        {
            for (Int32 i = 0; i < CoordinatesInternal.Count/2; i++)
            {
                Int32 j = CoordinatesInternal.Count - 1 - i;

                // skip equal points on both ends
                if (!CoordinatesInternal[i].Equals(CoordinatesInternal[j]))
                {
                    if (CoordinatesInternal[i].CompareTo(CoordinatesInternal[j]) > 0)
                    {
                        Coordinates.Reverse();
                    }

                    return;
                }
            }
        }

        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        ///// <summary>
        ///// Initializes a new instance of the <see cref="LineString{TCoordinate}"/> class.
        ///// </summary>        
        ///// <remarks>
        ///// For create this <see cref="Geometry{TCoordinate}"/> is used a standard <see cref="GeometryFactory{TCoordinate}"/> 
        ///// with <see cref="PrecisionModel{TCoordinate}" /> <c> == </c> <see cref="PrecisionModelType.Floating"/>.
        ///// </remarks>
        ///// <param name="points">The coordinates used for create this <see cref="LineString{TCoordinate}" />.</param>
        //public LineString(IEnumerable<TCoordinate> points) :
        //    this(DefaultFactory.CoordinateSequenceFactory.Create(points), DefaultFactory) { }

        /* END ADDED BY MPAUL42: monoGIS team */

        IPoint ILineString.GetPoint(Int32 index)
        {
            return GetPoint(index);
        }

        ILineString ILineString.Reverse()
        {
            return Reverse();
        }

        IPoint ICurve.StartPoint
        {
            get { return StartPoint; }
        }

        IPoint ICurve.EndPoint
        {
            get { return EndPoint; }
        }

        #endregion

        /// <summary>
        /// Returns true if the given point is a vertex of this <see cref="LineString{TCoordinate}"/>.
        /// </summary>
        /// <param name="pt">The <c>Coordinate</c> to check.</param>
        /// <returns><see langword="true"/> if <c>pt</c> is one of this <c>LineString</c>'s vertices.</returns>
        public Boolean IsCoordinate(TCoordinate pt)
        {
            foreach (TCoordinate coordinate in CoordinatesInternal)
            {
                if (coordinate.Equals(pt))
                {
                    return true;
                }
            }

            return false;
        }

        protected override Extents<TCoordinate> ComputeExtentsInternal()
        {
            if (IsEmpty)
            {
                return new Extents<TCoordinate>(Factory);
            }

            ICoordinateSequence<TCoordinate> coordinates = CoordinatesInternal;

            Extents<TCoordinate> e = new Extents<TCoordinate>(Factory);

            coordinates.ExpandExtents(e);

            return e;
        }

        protected internal override Int32 CompareToSameClass(IGeometry<TCoordinate> other)
        {
            if (other == null)
            {
                return 1;
            }

            LineString<TCoordinate> line = other as LineString<TCoordinate>;

            if (line == null)
            {
                throw new NotSupportedException(
                    "Comparison to ILineString types other than LineString<TCoordinate> " +
                    "not currently supported.");
            }

            return CoordinatesInternal.CompareTo(line.CoordinatesInternal);
            //// MD - optimized implementation
            //Int32 i = 0;
            //Int32 j = 0;

            //while (i < CoordinatesInternal.Count && j < line.CoordinatesInternal.Count)
            //{
            //    Int32 comparison = CoordinatesInternal[i].CompareTo(line.CoordinatesInternal[j]);

            //    if (comparison != 0)
            //    {
            //        return comparison;
            //    }

            //    i++;
            //    j++;
            //}

            //if (i < CoordinatesInternal.Count)
            //{
            //    return 1;
            //}

            //if (j < line.CoordinatesInternal.Count)
            //{
            //    return -1;
            //}

            //return 0;
        }

        protected override Boolean IsEquivalentClass(IGeometry other)
        {
            return other is ILineString<TCoordinate>;
        }
    }
}