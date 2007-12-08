using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Operation;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>  
    /// Basic implementation of <c>LineString</c>.
    /// </summary>  
    [Serializable]
    public class LineString<TCoordinate> : Geometry<TCoordinate>, ILineString<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        /// <summary>
        /// Represents an empty <c>LineString</c>.
        /// </summary>
        public static readonly ILineString Empty = new GeometryFactory<TCoordinate>().CreateLineString();

        // The points of this LineString.
        private readonly ICoordinateSequence<TCoordinate> _points;

        /// <param name="points">
        /// The points of the linestring, or <see langword="null" />
        /// to create the empty point. Consecutive points may not be equal.
        /// </param>
        public LineString(ICoordinateSequence<TCoordinate> points, IGeometryFactory<TCoordinate> factory)
            : base(factory)
        {
            if (points == null)
            {
                points = factory.CoordinateSequenceFactory.Create(4, 2);
            }

            if (points.Count == 1)
            {
                throw new ArgumentException("point array must contain 0 or >1 elements", "points");
            }

            _points = points;
        }

        public override IList<TCoordinate> Coordinates
        {
            get
            {
                throw new NotImplementedException();
            }
        }

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
            get { return _points.Count == 0; }
        }

        public override Int32 PointCount
        {
            get { return _points.Count; }
        }

        public IPoint<TCoordinate> GetPoint(Int32 index)
        {
            return Factory.CreatePoint(_points[index]);
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

                TCoordinate first = Slice.GetFirst(Coordinates);
                TCoordinate last = Slice.GetLast(Coordinates);

                return first.Equals(last);
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
            get { return CGAlgorithms<TCoordinate>.Length(_points); }
        }

        public override Boolean IsSimple
        {
            get { return (new IsSimpleOp<TCoordinate>()).IsSimple(this); }
        }

        public override IGeometry<TCoordinate> Boundary
        {
            get
            {
                if (IsEmpty)
                {
                    return Factory.CreateGeometryCollection();
                }

                if (IsClosed)
                {
                    return Factory.CreateMultiPoint();
                }

                return Factory.CreateMultiPoint(StartPoint, EndPoint);
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
            ICoordinateSequence<TCoordinate> seq = _points.Clone() as ICoordinateSequence<TCoordinate>;
            seq.Reverse();
            return Factory.CreateLineString(seq);
        }

        /// <summary>
        /// Returns true if the given point is a vertex of this <see cref="LineString{TCoordinate}"/>.
        /// </summary>
        /// <param name="pt">The <c>Coordinate</c> to check.</param>
        /// <returns><see langword="true"/> if <c>pt</c> is one of this <c>LineString</c>'s vertices.</returns>
        public Boolean IsCoordinate(TCoordinate pt)
        {
            foreach (TCoordinate coordinate in _points)
            {
                if(coordinate.Equals(pt))
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
                return new Extents<TCoordinate>();
            }

            // Convert to array, then access array directly, to avoid the function-call overhead
            // of calling Getter millions of times. ToArray may be inefficient for
            // non-BasicCoordinateSequence CoordinateSequences. [Jon Aquino]
            TCoordinate[] coordinates = points.ToCoordinateArray();

            Double minx = coordinates[0].X;
            Double miny = coordinates[0].Y;
            Double maxx = coordinates[0].X;
            Double maxy = coordinates[0].Y;

            for (Int32 i = 1; i < coordinates.Length; i++)
            {
                minx = minx < coordinates[i].X ? minx : coordinates[i].X;
                maxx = maxx > coordinates[i].X ? maxx : coordinates[i].X;
                miny = miny < coordinates[i].Y ? miny : coordinates[i].Y;
                maxy = maxy > coordinates[i].Y ? maxy : coordinates[i].Y;
            }

            return new Extents<TCoordinate>(minx, maxx, miny, maxy);
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

            for (Int32 i = 0; i < _points.Count; i++)
            {
                if (!Equal(Coordinates[i], otherLineString.Coordinates[i], tolerance))
                {
                    return false;
                }
            }

            return true;
        }

        //public override void Apply(ICoordinateFilter filter)
        //{
        //    for (Int32 i = 0; i < points.Count; i++)
        //    {
        //        filter.Filter(points.GetCoordinate(i));
        //    }
        //}

        //public override void Apply(IGeometryFilter filter)
        //{
        //    filter.Filter(this);
        //}

        public override void Apply(IGeometryComponentFilter<TCoordinate> filter)
        {
            filter.Filter(this);
        }

        public override IGeometry<TCoordinate> Clone()
        {
            return Factory.CreateLineString(Coordinates);
        }

        /// <summary> 
        /// Normalizes a <c>LineString</c>.  A normalized linestring
        /// has the first point which is not equal to it's reflected point
        /// less than the reflected point.
        /// </summary>
        public override void Normalize()
        {
            for (Int32 i = 0; i < _points.Count / 2; i++)
            {
                Int32 j = _points.Count - 1 - i;

                // skip equal points on both ends
                if (!_points[i].Equals(_points[j]))
                {
                    if (_points[i].CompareTo(_points[j]) > 0)
                    {
                        CoordinateArrays.Reverse(Coordinates);
                    }

                    return;
                }
            }
        }

        protected internal override Int32 CompareToSameClass(IGeometry<TCoordinate> other)
        {
            LineString<TCoordinate> line = other as LineString<TCoordinate>;

            Debug.Assert(line != null);

            // MD - optimized implementation
            Int32 i = 0;
            Int32 j = 0;

            while (i < _points.Count && j < line._points.Count)
            {
                Int32 comparison = _points[i].CompareTo(line._points[j]);

                if (comparison != 0)
                {
                    return comparison;
                }

                i++;
                j++;
            }

            if (i < _points.Count)
            {
                return 1;
            }

            if (j < line._points.Count)
            {
                return -1;
            }

            return 0;
        }

        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        /// <summary>
        /// Initializes a new instance of the <see cref="LineString{TCoordinate}"/> class.
        /// </summary>        
        /// <remarks>
        /// For create this <see cref="Geometry{TCoordinate}"/> is used a standard <see cref="GeometryFactory{TCoordinate}"/> 
        /// with <see cref="PrecisionModel{TCoordinate}" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        /// <param name="points">The coordinates used for create this <see cref="LineString{TCoordinate}" />.</param>
        public LineString(IEnumerable<TCoordinate> points) :
            this(DefaultFactory.CoordinateSequenceFactory.Create(points), DefaultFactory) { }

        public TCoordinate this[Int32 index]
        {
            get { return _points[index]; }
            set
            {
                _points.SetOrdinate(index, Ordinates.X, value.X);
                _points.SetOrdinate(index, Ordinates.Y, value.Y);
                _points.SetOrdinate(index, Ordinates.Z, value.Z);
            }
        }

        public Int32 Count
        {
            get { return _points.Count; }
        }

        /// <summary>
        /// Returns the value of the angle between the <see cref="StartPoint" />
        /// and the <see cref="EndPoint" />.
        /// </summary>
        public Double Angle
        {
            get
            {
                TCoordinate startPoint = Slice.GetFirst(_points);
                TCoordinate endPoint = Slice.GetLast(_points);

                Double startX = startPoint[Ordinates.X], startY = startPoint[Ordinates.Y];
                Double endX = endPoint[Ordinates.X], endY = endPoint[Ordinates.Y];

                Double deltaX = endPoint[Ordinates.X] - startPoint[Ordinates.X];
                Double deltaY = endPoint[Ordinates.Y] - startPoint[Ordinates.Y];
                Double length = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                Double angleRadians = Math.Asin(Math.Abs(endY - startY) / length);
                Double angle = (angleRadians * 180) / Math.PI;

                if (((startX < endX) && (startY > endY)) ||
                    ((startX > endX) && (startY < endY)))
                {
                    angle = 360 - angle;
                }

                return angle;
            }
        }

        /* END ADDED BY MPAUL42: monoGIS team */

        #region ILineString Members

        IPoint ILineString.GetPoint(Int32 index)
        {
            return GetPoint(index);
        }

        ILineString ILineString.Reverse()
        {
            return Reverse();
        }

        #endregion

        #region ICurve Members

        IPoint ICurve.StartPoint
        {
            get { return StartPoint; }
        }

        IPoint ICurve.EndPoint
        {
            get { return EndPoint; }
        }

        #endregion
    }
}