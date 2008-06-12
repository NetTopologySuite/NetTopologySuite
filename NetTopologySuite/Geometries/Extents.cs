using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using NPack;
using NPack.Interfaces;
using GeoAPI.DataStructures;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Defines a rectangular, orthogonal region of the 2D coordinate plane.
    /// </summary>
    /// <remarks>
    /// An <see cref="Extents{TCoordinate}"/> is often used to represent the 
    /// bounding box of a <see cref="Geometry{TCoordinate}"/>,
    /// e.g. the minimum and maximum x and y values of all the coordinates.
    /// Extents support infinite or half-infinite regions, by using the values of
    /// <see cref="Double.PositiveInfinity"/> and <see cref="Double.NegativeInfinity"/>.
    /// When Extents objects are created or initialized,
    /// the supplies extent values are automatically sorted into the correct order. 
    /// </remarks>
    [Serializable]
    public class Extents<TCoordinate> : IExtents<TCoordinate>, IExtents2D
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComputable<Double, TCoordinate>,
                                IComparable<TCoordinate>, IConvertible
    {
        private readonly IGeometryFactory<TCoordinate> _geoFactory;
        private TCoordinate _min;
        private TCoordinate _max;

        /// <summary>
        /// Test the point q to see whether it intersects the Envelope
        /// defined by p1-p2.
        /// </summary>
        /// <param name="p1">One extremal point of the envelope.</param>
        /// <param name="p2">The other extremal point of the envelope.</param>
        /// <param name="q">Point to test for intersection.</param>
        /// <returns><see langword="true"/> if q intersects the envelope p1-p2.</returns>
        public static Boolean Intersects(TCoordinate p1, TCoordinate p2, TCoordinate q)
        {
            Double qX = q[Ordinates.X], qY = q[Ordinates.Y];
            Double p1X = p1[Ordinates.X], p1Y = p1[Ordinates.Y];
            Double p2X = p1[Ordinates.X], p2Y = p1[Ordinates.Y];

            if (((qX >= (p1X < p2X ? p1X : p2X)) && (qX <= (p1X > p2X ? p1X : p2X))) &&
                ((qY >= (p1Y < p2Y ? p1Y : p2Y)) && (qY <= (p1Y > p2Y ? p1Y : p2Y))))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Test the envelope defined by p1-p2 for intersection
        /// with the envelope defined by q1-q2.
        /// </summary>
        /// <param name="p1">One extremal point of the envelope Point.</param>
        /// <param name="p2">Another extremal point of the envelope Point.</param>
        /// <param name="q1">One extremal point of the envelope Q.</param>
        /// <param name="q2">Another extremal point of the envelope Q.</param>
        /// <returns><see langword="true"/> if Q intersects Point</returns>
        public static Boolean Intersects(TCoordinate p1, TCoordinate p2, TCoordinate q1, TCoordinate q2)
        {
            Double p1X = p1[Ordinates.X], p1Y = p1[Ordinates.Y];
            Double p2X = p1[Ordinates.X], p2Y = p1[Ordinates.Y];
            Double q1X = p1[Ordinates.X], q1Y = p1[Ordinates.Y];
            Double q2X = p1[Ordinates.X], q2Y = p1[Ordinates.Y];

            Double minq = Math.Min(q1X, q2X);
            Double maxq = Math.Max(q1X, q2X);
            Double minp = Math.Min(p1X, p2X);
            Double maxp = Math.Max(p1X, p2X);

            if (minp > maxq)
            {
                return false;
            }

            if (maxp < minq)
            {
                return false;
            }

            minq = Math.Min(q1Y, q2Y);
            maxq = Math.Max(q1Y, q2Y);
            minp = Math.Min(p1Y, p2Y);
            maxp = Math.Max(p1Y, p2Y);

            if (minp > maxq)
            {
                return false;
            }

            if (maxp < minq)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a null <see cref="Extents{TCoordinate}"/>.
        /// </summary>
        internal Extents(IGeometryFactory<TCoordinate> factory)
        {
            _geoFactory = factory;
            Init();
        }

        /// <summary>
        /// Creates an <see cref="Extents{TCoordinate}"/> for a region defined by maximum and minimum values.
        /// </summary>
        /// <param name="x1">The first x-value.</param>
        /// <param name="x2">The second x-value.</param>
        /// <param name="y1">The first y-value.</param>
        /// <param name="y2">The second y-value.</param>
        internal Extents(IGeometryFactory<TCoordinate> factory, Double x1, Double x2, Double y1, Double y2)
            : this(factory)
        {
            Init(x1, x2, y1, y2);
        }

        /// <summary>
        /// Creates an <see cref="Extents{TCoordinate}"/> for a region defined by two Coordinates.
        /// </summary>
        /// <param name="p1">The first Coordinate.</param>
        /// <param name="p2">The second Coordinate.</param>
        internal Extents(IGeometryFactory<TCoordinate> factory, TCoordinate p1, TCoordinate p2)
            : this(factory)
        {
            Init(p1, p2);
        }

        /// <summary>
        /// Creates an <see cref="Extents{TCoordinate}"/> for a region defined by a single Coordinate.
        /// </summary>
        /// <param name="p">The Coordinate.</param>
        internal Extents(IGeometryFactory<TCoordinate> factory, TCoordinate p)
            : this(factory)
        {
            Init(p);
        }

        /// <summary>
        /// Create an <see cref="Extents{TCoordinate}"/> from an existing Envelope.
        /// </summary>
        /// <param name="extents">The Envelope to initialize from.</param>
        internal Extents(IGeometryFactory<TCoordinate> factory, IExtents<TCoordinate> extents)
            : this(factory)
        {
            Init(extents);
        }

        /// <summary>
        /// Initialize to a null <see cref="Extents{TCoordinate}"/>.
        /// </summary>
        public void Init()
        {
            SetToEmpty();
        }

        /// <summary>
        /// Initialize an <see cref="Extents{TCoordinate}"/> for a region defined by maximum and minimum values.
        /// </summary>
        /// <param name="x1">The first x-value.</param>
        /// <param name="x2">The second x-value.</param>
        /// <param name="y1">The first y-value.</param>
        /// <param name="y2">The second y-value.</param>
        public void Init(Double x1, Double x2, Double y1, Double y2)
        {
            Double minX, maxX, minY, maxY;

            if (x1 < x2)
            {
                minX = x1;
                maxX = x2;
            }
            else
            {
                minX = x2;
                maxX = x1;
            }

            if (y1 < y2)
            {
                minY = y1;
                maxY = y2;
            }
            else
            {
                minY = y2;
                maxY = y1;
            }

            ICoordinateFactory<TCoordinate> coordFactory = _geoFactory.CoordinateFactory;
            _min = coordFactory.Create(minX, minY);
            _max = coordFactory.Create(maxX, maxY);
        }

        /// <summary>
        /// Initialize an <see cref="Extents{TCoordinate}"/> 
        /// for a region defined by two <typeparamref name="TCoordinate"/>s.
        /// </summary>
        /// <param name="p1">The first coordinate.</param>
        /// <param name="p2">The second coordinate.</param>
        public void Init(TCoordinate p1, TCoordinate p2)
        {
            Init(p1[Ordinates.X], p2[Ordinates.X], p1[Ordinates.Y], p2[Ordinates.Y]);
        }

        /// <summary>
        /// Initialize an <see cref="Extents{TCoordinate}"/> for a region defined by a single Coordinate.
        /// </summary>
        /// <param name="p">The Coordinate.</param>
        public void Init(TCoordinate p)
        {
            if (Coordinates<TCoordinate>.IsEmpty(p))
            {
                return;
            }

            Double x = p[Ordinates.X], y = p[Ordinates.Y];
            Init(x, x, y, y);
        }

        /// <summary>
        /// Initialize an <see cref="Extents{TCoordinate}"/> from an existing 
        /// <see cref="IExtents{TCoordinate}"/>.
        /// </summary>
        /// <param name="extents">The Envelope to initialize from.</param>
        public void Init(IExtents<TCoordinate> extents)
        {
            _min = extents.Min;
            _max = extents.Max;
        }

        /// <summary>
        /// Makes this <see cref="Extents{TCoordinate}"/> a "null" envelope..
        /// </summary>
        public void SetToEmpty()
        {
            _min = default(TCoordinate);
            _max = default(TCoordinate);
        }

        /// <summary>
        /// Returns <see langword="true"/> if this <see cref="Extents{TCoordinate}"/> 
        /// is an empty envelope.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if this <see cref="Extents{TCoordinate}"/> is uninitialized
        /// or is the envelope of the empty point.
        /// </returns>
        public Boolean IsEmpty
        {
            get
            {
                return Coordinates<TCoordinate>.IsEmpty(_min)
                    || Coordinates<TCoordinate>.IsEmpty(_max);
            }
        }

        /// <summary>
        /// Returns the difference between the maximum and minimum x values.
        /// </summary>
        /// <returns>max x - min x, or 0 if this is a null <see cref="Extents{TCoordinate}"/>.</returns>
        public Double Width
        {
            get
            {
                if (IsEmpty)
                {
                    return 0;
                }

                return Math.Abs(_max[Ordinates.X] - _min[Ordinates.X]);
            }
        }

        /// <summary>
        /// Returns the difference between the maximum and minimum y values.
        /// </summary>
        /// <returns>max y - min y, or 0 if this is a null <see cref="Extents{TCoordinate}"/>.</returns>
        public Double Height
        {
            get
            {
                if (IsEmpty)
                {
                    return 0;
                }

                return Math.Abs(_max[Ordinates.Y] - _min[Ordinates.Y]);
            }
        }

        /// <summary>
        /// Enlarges the boundary of the <see cref="Extents{TCoordinate}"/> so that it contains (p).
        /// Does nothing if (p) is already on or within the boundaries.
        /// </summary>
        /// <param name="p">The Coordinate.</param>
        public void ExpandToInclude(TCoordinate p)
        {
            ExpandToInclude(p[Ordinates.X], p[Ordinates.Y]);
        }

        /// <summary>
        /// Enlarges the boundary of the <see cref="Extents{TCoordinate}"/> 
        /// so that it contains (x, y). Does nothing if (x, y) is already on 
        /// or within the boundaries.
        /// </summary>
        /// <param name="x">
        /// The value to lower the minimum x to or to raise the maximum x to.
        /// </param>
        /// <param name="y">
        /// The value to lower the minimum y to or to raise the maximum y to.
        /// </param>
        public void ExpandToInclude(Double x, Double y)
        {
            TCoordinate coordinate = _geoFactory.CoordinateFactory.Create(x, y);

            if (IsEmpty)
            {
                _min = coordinate;
                _max = coordinate;
            }
            else
            {
                if (_min.GreaterThan(coordinate))
                {
                    _min = coordinate;
                }

                if (_max.LessThan(coordinate))
                {
                    _max = coordinate;
                }
            }
        }
    
        public void ExpandToInclude(IExtents<TCoordinate> other)
        {
            if (other == null || other.IsEmpty)
            {
                return;
            }

            if (IsEmpty)
            {
                _min = other.Min;
                _max = other.Max;
            }
            else
            {
                TCoordinate otherMin = other.Min;
                TCoordinate otherMax = other.Max;

                Double xMin = Math.Min(_min[Ordinates.X], otherMin[Ordinates.X]);
                Double xMax = Math.Max(_max[Ordinates.X], otherMax[Ordinates.X]);
                Double yMin = Math.Min(_min[Ordinates.Y], otherMin[Ordinates.Y]);
                Double yMax = Math.Max(_max[Ordinates.Y], otherMax[Ordinates.Y]);

                ICoordinateFactory<TCoordinate> coordFactory = _geoFactory.CoordinateFactory;
                _min = coordFactory.Create(xMin, yMin);
                _max = coordFactory.Create(xMax, yMax);
            }
        }

        public IGeometryFactory<TCoordinate> Factory
        {
            get { return _geoFactory; }
        }

        /// <summary>
        /// Translates this envelope by given amounts in the X and Y direction.
        /// </summary>
        /// <param name="transX">The amount to translate along the X axis.</param>
        /// <param name="transY">The amount to translate along the Y axis.</param>
        public void Translate(Double transX, Double transY)
        {
            if (IsEmpty)
            {
                return;
            }

            ICoordinateFactory<TCoordinate> coordFactory = _geoFactory.CoordinateFactory;
            _min = coordFactory.Create(_min[Ordinates.X] + transX, _min[Ordinates.Y] + transY);
            _max = coordFactory.Create(_max[Ordinates.X] + transX, _max[Ordinates.Y] + transY);
        }

        public TCoordinate Center
        {
            get
            {
                return IsEmpty
                           ? default(TCoordinate)
                           : _geoFactory.CoordinateFactory.Create(
                                 (Min[Ordinates.X] + Max[Ordinates.X]) / 2.0,
                                 (Min[Ordinates.Y] + Max[Ordinates.Y]) / 2.0);
            }
        } 

        public IExtents<TCoordinate> Intersection(IExtents<TCoordinate> extents)
        {
            if (IsEmpty || extents.IsEmpty || !Intersects(extents))
            {
                return _geoFactory.CreateExtents();
            }

            // TODO: 3D unsafe
            return new Extents<TCoordinate>(Factory,
                                            Math.Max(Min[Ordinates.X], extents.Min[Ordinates.X]),
                                            Math.Min(Max[Ordinates.X], extents.Max[Ordinates.X]),
                                            Math.Max(Min[Ordinates.Y], extents.Min[Ordinates.Y]),
                                            Math.Min(Max[Ordinates.Y], extents.Max[Ordinates.Y]));
        }

        public Boolean Intersects(IExtents<TCoordinate> other)
        {
            if (IsEmpty || other.IsEmpty)
            {
                return false;
            }

            // TODO: 3D unsafe
            return !(other.Min[Ordinates.X] > _max[Ordinates.X] ||
                     other.Max[Ordinates.X] < _min[Ordinates.X] ||
                     other.Min[Ordinates.Y] > _max[Ordinates.Y] ||
                     other.Max[Ordinates.Y] < _min[Ordinates.Y]);
        }

        public Boolean Intersects(TCoordinate coordinate)
        {
            if (IsEmpty || Coordinates<TCoordinate>.IsEmpty(coordinate))
            {
                return false;
            }

            // TODO: 3D unsafe
            return Intersects(coordinate[Ordinates.X], coordinate[Ordinates.Y]);
        }

        /// <summary>  
        /// Check if the point <c>(x, y)</c> overlaps (lies inside) the region 
        /// of this <see cref="Extents{TCoordinate}"/>.
        /// </summary>
        /// <param name="x">The x-ordinate of the point.</param>
        /// <param name="y">The y-ordinate of the point.</param>
        /// <returns>
        /// <see langword="true"/> if the point overlaps this <see cref="Extents{TCoordinate}"/>.
        /// </returns>
        public Boolean Intersects(Double x, Double y)
        {
            if (IsEmpty || Double.IsNaN(x) || Double.IsNaN(y))
            {
                return false;
            }

            return !(x > _max[Ordinates.X] ||
                     x < _min[Ordinates.X] ||
                     y > _max[Ordinates.Y] ||
                     y < _min[Ordinates.Y]);
        }

        /// <summary>
        /// Use Intersects instead. In the future, Overlaps may be
        /// changed to be a true overlap check; that is, whether the intersection is
        /// two-dimensional.
        /// </summary>
        [Obsolete("Use Intersects instead")]
        public Boolean Overlaps(IExtents<TCoordinate> other)
        {
            return Intersects(other);
        }

        /// <summary>
        /// Use Intersects instead.
        /// </summary>
        [Obsolete("Use Intersects instead")]
        public Boolean Overlaps(TCoordinate p)
        {
            return Intersects(p);
        }

        /// <summary>
        /// Use Intersects instead.
        /// </summary>
        [Obsolete("Use Intersects instead")]
        public Boolean Overlaps(Double x, Double y)
        {
            return Intersects(x, y);
        }
                
        public Boolean Contains(TCoordinate p)
        {
            return Contains(p[Ordinates.X], p[Ordinates.Y]);
        }

        /// <summary>  
        /// Returns <see langword="true"/> if the given point lies in or on the envelope.
        /// </summary>
        /// <param name="x"> the x-coordinate of the point which this <see cref="Extents{TCoordinate}"/> is
        /// being checked for containing.</param>
        /// <param name="y"> the y-coordinate of the point which this <see cref="Extents{TCoordinate}"/> is
        /// being checked for containing.</param>
        /// <returns><see langword="true"/> if <c>(x, y)</c> lies in the interior or
        /// on the boundary of this <see cref="Extents{TCoordinate}"/>.</returns>
        public Boolean Contains(Double x, Double y)
        {
            return x >= _min[Ordinates.X] &&
                   x <= _max[Ordinates.X] &&
                   y >= _min[Ordinates.Y] &&
                   y <= _max[Ordinates.Y];
        }

        /// <summary>  
        /// Returns <see langword="true"/> if the <c>Envelope other</c>
        /// lies wholely inside this <see cref="Extents{TCoordinate}"/> (inclusive of the boundary).
        /// </summary>
        /// <param name="other"> the <see cref="Extents{TCoordinate}"/> which this <see cref="Extents{TCoordinate}"/> is being checked for containing.</param>
        /// <returns><see langword="true"/> if <c>other</c> is contained in this <see cref="Extents{TCoordinate}"/>.</returns>
        public Boolean Contains(IExtents<TCoordinate> other)
        {
            if (IsEmpty || other.IsEmpty)
            {
                return false;
            }

            // TODO: 3D unsafe
            return other.Min[Ordinates.X] >= _min[Ordinates.X] &&
                   other.Max[Ordinates.X] <= _max[Ordinates.X] &&
                   other.Min[Ordinates.Y] >= _min[Ordinates.Y] &&
                   other.Max[Ordinates.Y] <= _max[Ordinates.Y];
        }

        /// <summary> 
        /// Computes the distance between this and another
        /// <see cref="Extents{TCoordinate}"/>.
        /// The distance between overlapping Envelopes is 0.  Otherwise, the
        /// distance is the Euclidean distance between the closest points.
        /// </summary>
        /// <returns>The distance between this and another <see cref="Extents{TCoordinate}"/>.</returns>
        public Double Distance(IExtents<TCoordinate> extents)
        {
            if (Intersects(extents))
            {
                return 0;
            }

            Double dx = 0.0;

            // TODO: 3D unsafe
            if (_max[Ordinates.X] < extents.Min[Ordinates.X])
            {
                dx = extents.Min[Ordinates.X] - _max[Ordinates.X];
            }

            if (_min[Ordinates.X] > extents.Max[Ordinates.X])
            {
                dx = _min[Ordinates.X] - extents.Max[Ordinates.X];
            }

            Double dy = 0.0;

            if (_max[Ordinates.Y] < extents.Min[Ordinates.Y])
            {
                dy = extents.Min[Ordinates.Y] - _max[Ordinates.Y];
            }

            if (_min[Ordinates.Y] > extents.Max[Ordinates.Y])
            {
                dy = _min[Ordinates.Y] - extents.Max[Ordinates.Y];
            }

            // if either is zero, the envelopes overlap either vertically or horizontally
            if (dx == 0.0)
            {
                return dy;
            }

            if (dy == 0.0)
            {
                return dx;
            }

            return Math.Sqrt(dx * dx + dy * dy);
        }

        public override Boolean Equals(object other)
        {
            return Equals(other as IExtents<TCoordinate>);
        }

        public Boolean Equals(IExtents<TCoordinate> other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (IsEmpty)
            {
                return other.IsEmpty;
            }

            return _max.Equals(other.Max) &&
                   _min.Equals(other.Min);
        }

        public Int32 CompareTo(object other)
        {
            return CompareTo(other as IExtents<TCoordinate>);
        }

        public Int32 CompareTo(IExtents<TCoordinate> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            Boolean isEmpty = IsEmpty, otherEmpty = other.IsEmpty;

            if (isEmpty && otherEmpty)
            {
                return 0;
            }
            
            if (isEmpty || otherEmpty)
            {
                return isEmpty ? -1 : 1;
            }

            Double area = Area, otherArea = other.GetSize(Ordinates.X, Ordinates.Y);

            if (area > otherArea)
            {
                return 1;
            }

            if (area < otherArea)
            {
                return -1;
            }

            return 0;
        }

        public override Int32 GetHashCode()
        {
            Int32 result = 861101 ^ _min.GetHashCode() ^ _max.GetHashCode();
            return result;
        }

        /// <summary>
        /// Compares two <see cref="Extents{TCoordinate}"/> instances for value equality.
        /// </summary>
        /// <param name="left">The left <see cref="Extents{TCoordinate}"/> instance.</param>
        /// <param name="right">The right <see cref="Extents{TCoordinate}"/> instance.</param>
        /// <returns>
        /// <see langword="true"/> if the <typeparamref name="TCoordinate"/> values of the 
        /// <see cref="Extents{TCoordinate}"/> are equal.
        /// </returns>
        public static Boolean operator ==(Extents<TCoordinate> left, Extents<TCoordinate> right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            return !ReferenceEquals(left, null)
                       ? left.Equals(right)
                       : right.Equals(left);
        }

        /// <summary>
        /// Compares two <see cref="Extents{TCoordinate}"/> instances for value inequality.
        /// </summary>
        /// <param name="left">The left <see cref="Extents{TCoordinate}"/> instance.</param>
        /// <param name="right">The right <see cref="Extents{TCoordinate}"/> instance.</param>
        /// <returns>
        /// <see langword="true"/> if the <typeparamref name="TCoordinate"/> values of the 
        /// <see cref="Extents{TCoordinate}"/> are not equal.
        /// </returns>
        public static Boolean operator !=(Extents<TCoordinate> left, Extents<TCoordinate> right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            // TODO: 3D unsafe
            return "Extents [" + _min[Ordinates.X] + " - " + _max[Ordinates.X] +
                   ", " + _min[Ordinates.Y] + " - " + _max[Ordinates.Y] + "]";
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        object ICloneable.Clone()
        {
            return Clone();
        }

        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        /// <summary>
        /// Returns the area of the envelope.
        /// </summary>
        public Double Area
        {
            get
            {
                // TODO: 3D unsafe
                Double area = Math.Abs(_max[Ordinates.X] - _min[Ordinates.X]) * 
                              Math.Abs(_max[Ordinates.Y] - _min[Ordinates.Y]);
                return area;
            }
        }

        /// <summary>
        /// Creates a deep copy of the current envelope.
        /// </summary>
        /// <returns></returns>
        public IExtents<TCoordinate> Clone()
        {
            if (IsEmpty)
            {
                return new Extents<TCoordinate>(Factory);
            }

            ICoordinateFactory<TCoordinate> coordFactory = Factory.CoordinateFactory;
            TCoordinate cloneMin = coordFactory.Create(_min);
            TCoordinate cloneMax = coordFactory.Create(_max);
            return new Extents<TCoordinate>(Factory, cloneMin, cloneMax);
        }

        /* END ADDED BY MPAUL42: monoGIS team */

        #region IExtents<TCoordinate> Members

        public Boolean Borders(TCoordinate coordinate)
        {
            throw new NotImplementedException();
        }

        public Boolean Borders(IExtents<TCoordinate> other)
        {
            throw new NotImplementedException();
        }

        public Boolean Borders(TCoordinate coordinate, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public Boolean Borders(IExtents<TCoordinate> other, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public Boolean Contains(TCoordinate coordinate, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public Boolean Contains(IExtents<TCoordinate> other, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public void ExpandToInclude(params TCoordinate[] coordinate)
        {
            foreach (TCoordinate tCoordinate in coordinate)
            {
                ExpandToInclude(tCoordinate);
            }
        }

        public void ExpandToInclude(IEnumerable<TCoordinate> coordinates)
        {
            // TODO: 3D unsafe
            Double originalXMin;
            Double originalYMin;
            Double originalXMax;
            Double originalYMax;

            if (IsEmpty)
            {
                originalXMin = Double.MaxValue;
                originalYMin = Double.MaxValue;
                originalXMax = Double.MinValue;
                originalYMax = Double.MinValue;
            }
            else
            {
                originalXMin = _min[Ordinates.X];
                originalYMin = _min[Ordinates.Y];
                originalXMax = _max[Ordinates.X];
                originalYMax = _max[Ordinates.Y];
            }

            Double xMin = originalXMin;
            Double yMin = originalYMin;
            Double xMax = originalXMax;
            Double yMax = originalYMax;

            foreach (TCoordinate coordinate in coordinates)
            {
                Double x = coordinate[Ordinates.X];
                Double y = coordinate[Ordinates.Y];

                xMin = Math.Min(x, xMin);
                xMax = Math.Max(x, xMax);
                yMin = Math.Min(y, yMin);
                yMax = Math.Max(y, yMax);
            }

            ICoordinateFactory<TCoordinate> coordFactory = _geoFactory.CoordinateFactory;

            if (xMin < originalXMin || yMin < originalYMin)
            {
                _min = coordFactory.Create(xMin, yMin);
            }

            if (xMax > originalXMax || yMax > originalYMax)
            {
                _max = coordFactory.Create(xMax, yMax);
            }
        }

        public void ExpandToInclude(IGeometry<TCoordinate> geometry)
        {
            if (geometry == null)
            {
                return;
            }

            ExpandToInclude(geometry.GetVertexes());
        }

        public Double GetIntersectingArea(IGeometry<TCoordinate> geometry)
        {
            throw new NotImplementedException();
        }

        public IExtents<TCoordinate> Intersection(IGeometry<TCoordinate> geometry)
        {
            if (geometry == null || geometry.IsEmpty)
            {
                return _geoFactory.CreateExtents();
            }

            return Intersection(geometry.Extents);
        }

        public Boolean Intersects(TCoordinate coordinate, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public Boolean Intersects(IExtents<TCoordinate> other, Tolerance tolerance)
        {
            if (other == null || other.IsEmpty)
            {
                return false;
            }

            TCoordinate minA = Min;
            TCoordinate maxA = Max;
            TCoordinate minB = other.Min;
            TCoordinate maxB = other.Max;

            return !(minA.GreaterThan(maxB) ||
                     minB.GreaterThan(maxA) ||
                     maxA.LessThan(minB) ||
                     maxB.LessThan(minA));
        }

        public TCoordinate Max
        {
            get { return _geoFactory.CoordinateFactory.Create(_max); }
        }

        public TCoordinate Min
        {
            get { return _geoFactory.CoordinateFactory.Create(_min); }
        }

        public Boolean Overlaps(TCoordinate coordinate, Tolerance tolerance)
        {
            return Intersects(coordinate, tolerance);
        }

        public Boolean Overlaps(IExtents<TCoordinate> other, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IExtents<TCoordinate>> Split(TCoordinate coordinate)
        {
            throw new NotImplementedException();
        }

        public IGeometry<TCoordinate> ToGeometry()
        {
            if (IsEmpty)
            {
                return _geoFactory.CreatePoint();
            }

            ICoordinateFactory<TCoordinate> coordFactory = _geoFactory.CoordinateFactory;
            ILinearRing<TCoordinate> shell = _geoFactory.CreateLinearRing(new TCoordinate[]
                          {
                              coordFactory.Create(_min),
                              coordFactory.Create(_min[Ordinates.X], _max[Ordinates.Y]),
                              coordFactory.Create(_max[Ordinates.X], _max[Ordinates.Y]),
                              coordFactory.Create(_max[Ordinates.X], _min[Ordinates.Y]),
                              coordFactory.Create(_min)
                          });
            return _geoFactory.CreatePolygon(shell);
        }

        public Boolean Touches(TCoordinate coordinate)
        {
            throw new NotImplementedException();
        }

        public Boolean Touches(IExtents<TCoordinate> other)
        {
            throw new NotImplementedException();
        }

        public Boolean Touches(TCoordinate coordinate, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public Boolean Touches(IExtents<TCoordinate> other, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates the union of the current box and the given point.
        /// </summary>
        public IExtents<TCoordinate> Union(IPoint<TCoordinate> point)
        {
            return Union(point.Coordinate);
        }

        /// <summary>
        /// Calculates the union of the current box and the given coordinate.
        /// </summary>
        public IExtents<TCoordinate> Union(TCoordinate coord)
        {
            IExtents<TCoordinate> extents = Clone();
            extents.ExpandToInclude(coord);
            return extents;
        }

        /// <summary>
        /// Calculates the union of the current box and the given box.
        /// </summary>
        public IExtents<TCoordinate> Union(IExtents<TCoordinate> box)
        {
            if (box.IsEmpty)
            {
                return this;
            }

            if (IsEmpty)
            {
                return box;
            }

            return new Extents<TCoordinate>(
                                Factory,
                                Math.Min(_min[Ordinates.X], box.Min[Ordinates.X]),
                                Math.Max(_max[Ordinates.X], box.Max[Ordinates.X]),
                                Math.Min(_min[Ordinates.Y], box.Min[Ordinates.Y]),
                                Math.Max(_max[Ordinates.Y], box.Max[Ordinates.Y]));
        }

        public Boolean Within(TCoordinate coordinate)
        {
            throw new NotImplementedException();
        }

        public Boolean Within(IExtents<TCoordinate> other)
        {
            throw new NotImplementedException();
        }

        public Boolean Within(TCoordinate coordinate, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public Boolean Within(IExtents<TCoordinate> other, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IExtents Members

        Boolean IExtents.Borders(IExtents other)
        {
            return Borders(convert(other));
        }

        Boolean IExtents.Borders(IExtents other, Tolerance tolerance)
        {
            return Borders(convert(other), tolerance);
        }

        ICoordinate IExtents.Center
        {
            get { return Center; }
        }

        public Boolean Contains(params Double[] coordinate)
        {
            throw new NotImplementedException();
        }

        Boolean IContainable<IExtents>.Contains(IExtents other)
        {
            if (other == null || other.IsEmpty)
            {
                return false;
            }

            return Contains(convert(other));
        }

        public Boolean Contains(ICoordinate other)
        {
            throw new NotImplementedException();
        }

        public Boolean Contains(Tolerance tolerance, params Double[] coordinate)
        {
            throw new NotImplementedException();
        }

        Boolean IExtents.Contains(IExtents other, Tolerance tolerance)
        {
            return Contains(convert(other), tolerance);
        }

        Boolean IExtents.Contains(ICoordinate other, Tolerance tolerance)
        {
            return Contains(_geoFactory.CoordinateFactory.Create(other), tolerance);
        }

        Double IExtents.Distance(IExtents extents)
        {
            return Distance(convert(extents));
        }

        public void ExpandToInclude(params Double[] coordinate)
        {
            if (coordinate == null) throw new ArgumentNullException("coordinate");

            ICoordinateFactory<TCoordinate> coordFactory = _geoFactory.CoordinateFactory;

            if (coordinate.Length % 3 == 0)
            {
                throw new NotImplementedException();
            }
            else if (coordinate.Length % 2 == 0)
            {
                for (Int32 i = 0; i < coordinate.Length; i *= 2)
                {
                    Double x = coordinate[i];
                    Double y = coordinate[i + 1];
                    TCoordinate coord = coordFactory.Create(x, y);
                    ExpandToInclude(coord);
                }
            }
            else
            {
                throw new ArgumentException(
                    "Invalid number of coordinate components: " + coordinate.Length);
            }
        }

        void IExtents.ExpandToInclude(ICoordinateSequence sequence)
        {
            ICoordinateSequenceFactory<TCoordinate> coordSeqFac
                = _geoFactory.CoordinateSequenceFactory;
            ICoordinateSequence<TCoordinate> converted
                = GenericInterfaceConverter<TCoordinate>.Convert(sequence, coordSeqFac);
            ExpandToInclude(converted);
        }

        void IExtents.ExpandToInclude(IExtents other)
        {
            ExpandToInclude(convert(other));
        }

        void IExtents.ExpandToInclude(IGeometry other)
        {
            if (other == null || other.IsEmpty)
            {
                return;
            }

            (this as IExtents).ExpandToInclude(other.Extents);
        }

        IExtents IExtents.Intersection(IExtents extents)
        {
            return Intersection(convert(extents));
        }

        public Boolean Intersects(params Double[] coordinate)
        {
            throw new NotImplementedException();
        }

        Boolean IIntersectable<IExtents>.Intersects(IExtents other)
        {
            return Intersects(convert(other));
        }

        public Boolean Intersects(Tolerance tolerance, params Double[] coordinate)
        {
            throw new NotImplementedException();
        }

        Boolean IExtents.Intersects(IExtents other, Tolerance tolerance)
        {
            return Intersects(convert(other), tolerance);
        }

        ICoordinate IExtents.Max
        {
            get { return _max; }
        }

        ICoordinate IExtents.Min
        {
            get { return _min; }
        }

        public Double GetMax(Ordinates ordinate)
        {
            return _max[ordinate];
        }

        public Double GetMin(Ordinates ordinate)
        {
            return _min[ordinate];
        }

        public Double GetSize(Ordinates axis)
        {
            return Math.Abs(_max[axis] - _min[axis]);
        }

        public Double GetSize(Ordinates axis1, Ordinates axis2)
        {
            return Math.Abs(_max[axis1] - _min[axis1]) *
                   Math.Abs(_max[axis2] - _min[axis2]);
        }

        public Double GetSize(Ordinates axis1, Ordinates axis2, Ordinates axis3)
        {
            return Math.Abs(_max[axis1] - _min[axis1]) *
                   Math.Abs(_max[axis2] - _min[axis2]) *
                   Math.Abs(_max[axis3] - _min[axis3]);
        }

        public Double GetSize(params Ordinates[] axes)
        {
            if (axes.Length == 0)
            {
                throw new ArgumentException("No dimension specified.");
            }

            Double size = GetSize(axes[0]);

            for (Int32 i = 0; i < axes.Length; i++)
            {
                size *= GetSize(axes[i]);
            }

            return size;
        }

        public Boolean Overlaps(params Double[] coordinate)
        {
            return Overlaps(_geoFactory.CoordinateFactory.Create(coordinate));
        }

        Boolean IExtents.Overlaps(ICoordinate other)
        {
            return Overlaps(_geoFactory.CoordinateFactory.Create(other));
        }

        Boolean IExtents.Overlaps(IExtents other)
        {
            return (this as IExtents).Intersects(other);
        }

        public void Scale(params Double[] vector)
        {
            if (IsEmpty)
            {
                return;
            }

            throw new NotImplementedException();
        }

        public void Scale(Double factor)
        {
            if (IsEmpty)
            {
                return;
            }

            _min = _min.Multiply(factor);
        }

        public void Scale(Double factor, Ordinates axis)
        {
            if (IsEmpty)
            {
                return;
            }

            throw new NotImplementedException();
        }

        IGeometry IExtents.ToGeometry()
        {
            return ToGeometry();
        }

        public void Translate(params Double[] vector)
        {
            if (IsEmpty)
            {
                return;
            }

            throw new NotImplementedException();
        }

        public void Transform(ITransformMatrix<DoubleComponent> transformMatrix)
        {
            _min = (TCoordinate)transformMatrix.TransformVector(_min);
            _max = (TCoordinate)transformMatrix.TransformVector(_max);
        }

        IExtents IExtents.Union(IPoint point)
        {
            return point == null
                ? Clone()
                : _geoFactory.CreateExtents(this, point.Extents);
        }

        IExtents IExtents.Union(IExtents box)
        {
            return box == null
                ? Clone()
                : _geoFactory.CreateExtents(this, box);
        }

        public Boolean Touches(IExtents a) 
        {
            throw new NotImplementedException();
        }

        public Boolean Within(IExtents a) 
        {
            throw new NotImplementedException();
        }

        public void TranslateRelativeToWidth(params Double[] vector)
        {
            Double xShift;
            Double yShift;

            switch (vector.Length)
            {
                case 0:
                    return;
                case 1:
                    xShift = yShift = vector[0] * Width;
                    break;
                default:
                    xShift = vector[0] * Width;
                    yShift = vector[1] * Width;
                    break;
            }

            Translate(xShift, yShift);
        }


        IGeometryFactory IExtents.Factory
        {
            get { return _geoFactory; }
        }

        #endregion

        #region IEquatable<IExtents> Members

        public Boolean Equals(IExtents other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IExtents2D Members

        public Double XMin
        {
            get { return _min[Ordinates.X]; }
        }

        public Double XMax
        {
            get { return _max[Ordinates.X]; }
        }

        public Double YMin
        {
            get { return _min[Ordinates.Y]; }
        }

        public Double YMax
        {
            get { return _max[Ordinates.Y]; }
        }

        #endregion

        #region Private helper members
        private IExtents<TCoordinate> convert(IExtents extents)
        {
            if (extents == null)
            {
                return null;
            }

            IExtents<TCoordinate> converted = extents as IExtents<TCoordinate>;

            if (converted != null)
            {
                return converted;
            }

            ICoordinateFactory<TCoordinate> coordFactory = _geoFactory.CoordinateFactory;

            return _geoFactory.CreateExtents(coordFactory.Create(extents.Min),
                                             coordFactory.Create(extents.Max));
        }
        #endregion

        #region Obsolete commented out code

        //public Double MinX
        //{
        //    get { return _minX; }
        //}

        //public Double MaxX
        //{
        //    get { return _maxX; }
        //}

        //public Double MinY
        //{
        //    get { return _minY; }
        //}

        //public Double MaxY
        //{
        //    get { return _maxY; }
        //}

        ///// <summary>
        ///// Expands this envelope by a given distance in all directions.
        ///// Both positive and negative distances are supported.
        ///// </summary>
        ///// <param name="distance">The distance to expand the envelope.</param>
        //public void ExpandBy(Double distance)
        //{
        //    ExpandBy(distance, distance);
        //}

        ///// <summary>
        ///// Expands this envelope by a given distance in all directions.
        ///// Both positive and negative distances are supported.
        ///// </summary>
        ///// <param name="deltaX">The distance to expand the envelope along the the X axis.</param>
        ///// <param name="deltaY">The distance to expand the envelope along the the Y axis.</param>
        //public void ExpandBy(Double deltaX, Double deltaY)
        //{
        //    if (IsEmpty)
        //    {
        //        return;
        //    }

        //    _minX -= deltaX;
        //    _maxX += deltaX;
        //    _minY -= deltaY;
        //    _maxY += deltaY;

        //    // check for envelope disappearing
        //    if (_minX > _maxX || _minY > _maxY)
        //    {
        //        SetToEmpty();
        //    }
        //}

        ///// <summary>
        ///// Moves the envelope to the indicated coordinate.
        ///// </summary>
        ///// <param name="center">The new center coordinate.</param>
        //public void SetCentre(ICoordinate center)
        //{
        //    SetCentre(center, Width, Height);
        //}

        ///// <summary>
        ///// Moves the envelope to the indicated point.
        ///// </summary>
        ///// <param name="center">The new center point.</param>
        //public void SetCentre(IPoint center)
        //{
        //    SetCentre(center.Coordinate, Width, Height);
        //}

        ///// <summary>
        ///// Resizes the envelope to the indicated point.
        ///// </summary>
        ///// <param name="width">The new width.</param>
        ///// <param name="height">The new height.</param>
        //public void SetCentre(Double width, Double height)
        //{
        //    SetCentre(Center, width, height);
        //}

        ///// <summary>
        ///// Moves and resizes the current envelope.
        ///// </summary>
        ///// <param name="center">The new center point.</param>
        ///// <param name="width">The new width.</param>
        ///// <param name="height">The new height.</param>
        //public void SetCentre(IPoint center, Double width, Double height)
        //{
        //    SetCentre(center.Coordinate, width, height);
        //}

        ///// <summary>
        ///// Moves and resizes the current envelope.
        ///// </summary>
        ///// <param name="center">The new center coordinate.</param>
        ///// <param name="width">The new width.</param>
        ///// <param name="height">The new height.</param>
        //public void SetCentre(ICoordinate center, Double width, Double height)
        //{
        //    _minX = center.X - (width / 2);
        //    _maxX = center.X + (width / 2);
        //    _minY = center.Y - (height / 2);
        //    _maxY = center.Y + (height / 2);
        //}

        ///// <summary>
        ///// Zoom the box. 
        ///// Possible values are e.g. 50 (to zoom in a 50%) or -50 (to zoom out a 50%).
        ///// </summary>
        ///// <param name="perCent"> 
        ///// Negative do Envelope smaller.
        ///// Positive do Envelope bigger.
        ///// </param>
        ///// <example> 
        /////  perCent = -50 compact the envelope a 50% (make it smaller).
        /////  perCent = 200 enlarge envelope by 2.
        ///// </example>
        //public void Zoom(Double perCent)
        //{
        //    Double w = (Width * perCent / 100);
        //    Double h = (Height * perCent / 100);
        //    SetCenter(w, h);
        //}
        #endregion
    }
}