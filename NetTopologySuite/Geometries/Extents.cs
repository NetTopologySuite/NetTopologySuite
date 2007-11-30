using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Defines a rectangular region of the 2D coordinate plane.
    /// It is often used to represent the bounding box of a <see cref="Geometry{TCoordinate}"/>,
    /// e.g. the minimum and maximum x and y values of the Coordinates.
    /// Note that Extents support infinite or half-infinite regions, by using the values of
    /// <see cref="Double.PositiveInfinity"/> and <see cref="Double.NegativeInfinity"/>.
    /// When Extents objects are created or initialized,
    /// the supplies extent values are automatically sorted into the correct order.    
    /// </summary>
    [Serializable]
    public class Extents<TCoordinate> : IExtents<TCoordinate>
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<TCoordinate>, IConvertible
    {
        private TCoordinate _min;
        private TCoordinate _max;

        /// <summary>
        /// Test the point q to see whether it intersects the Envelope
        /// defined by p1-p2.
        /// </summary>
        /// <param name="p1">One extremal point of the envelope.</param>
        /// <param name="p2">Another extremal point of the envelope.</param>
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
        public Extents()
        {
            Init();
        }

        /// <summary>
        /// Creates an <see cref="Extents{TCoordinate}"/> for a region defined by maximum and minimum values.
        /// </summary>
        /// <param name="x1">The first x-value.</param>
        /// <param name="x2">The second x-value.</param>
        /// <param name="y1">The first y-value.</param>
        /// <param name="y2">The second y-value.</param>
        public Extents(Double x1, Double x2, Double y1, Double y2)
        {
            Init(x1, x2, y1, y2);
        }

        /// <summary>
        /// Creates an <see cref="Extents{TCoordinate}"/> for a region defined by two Coordinates.
        /// </summary>
        /// <param name="p1">The first Coordinate.</param>
        /// <param name="p2">The second Coordinate.</param>
        public Extents(TCoordinate p1, TCoordinate p2)
        {
            Init(p1, p2);
        }

        /// <summary>
        /// Creates an <see cref="Extents{TCoordinate}"/> for a region defined by a single Coordinate.
        /// </summary>
        /// <param name="p">The Coordinate.</param>
        public Extents(TCoordinate p)
        {
            Init(p);
        }

        /// <summary>
        /// Create an <see cref="Extents{TCoordinate}"/> from an existing Envelope.
        /// </summary>
        /// <param name="env">The Envelope to initialize from.</param>
        public Extents(IExtents<TCoordinate> env)
        {
            Init(env);
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

            _min = new TCoordinate(minX, maxX);
            _max = new TCoordinate(minY, maxY);
        }

        /// <summary>
        /// Initialize an <see cref="Extents{TCoordinate}"/> for a region defined by two Coordinates.
        /// </summary>
        /// <param name="p1">The first Coordinate.</param>
        /// <param name="p2">The second Coordinate.</param>
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
            get { return CoordinateHelper.IsEmpty(_min) || CoordinateHelper.IsEmpty(_max); }
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

                return _max[Ordinates.X] - _min[Ordinates.X];
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

                return _max[Ordinates.Y] - _min[Ordinates.Y];
            }
        }

        ///// <summary>
        ///// Returns the <see cref="Extents{TCoordinate}"/>s minimum x-value. min x > max x
        ///// indicates that this is a null <see cref="Extents{TCoordinate}"/>.
        ///// </summary>
        ///// <returns>The minimum x-coordinate.</returns>
        //public Double MinX
        //{
        //    get { return _minX; }
        //}

        ///// <summary>
        ///// Returns the <see cref="Extents{TCoordinate}"/>s maximum x-value. min x > max x
        ///// indicates that this is a null <see cref="Extents{TCoordinate}"/>.
        ///// </summary>
        ///// <returns>The maximum x-coordinate.</returns>
        //public Double MaxX
        //{
        //    get { return _maxX; }
        //}

        ///// <summary>
        ///// Returns the <see cref="Extents{TCoordinate}"/>s minimum y-value. min y > max y
        ///// indicates that this is a null <see cref="Extents{TCoordinate}"/>.
        ///// </summary>
        ///// <returns>The minimum y-coordinate.</returns>
        //public Double MinY
        //{
        //    get { return _minY; }
        //}

        ///// <summary>
        ///// Returns the <see cref="Extents{TCoordinate}"/>s maximum y-value. min y > max y
        ///// indicates that this is a null <see cref="Extents{TCoordinate}"/>.
        ///// </summary>
        ///// <returns>The maximum y-coordinate.</returns>
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
            TCoordinate coordinate = new TCoordinate(x, y);

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

        /// <summary>
        /// Enlarges the boundary of the <see cref="Extents{TCoordinate}"/> so that it contains
        /// <c>other</c>. Does nothing if <c>other</c> is wholly on or
        /// within the boundaries.
        /// </summary>
        /// <param name="other">the <see cref="Extents{TCoordinate}"/> to merge with.</param>        
        public void ExpandToInclude(IExtents<TCoordinate> other)
        {
            if (other.IsEmpty)
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
                if (other.Min.LessThan(_min))
                {
                    _min = other.Min;
                }

                if (other.Max.GreaterThan(_max))
                {
                    _max = other.Max;
                }
            }
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

            _min = new TCoordinate(_min[Ordinates.X] + transX, _min[Ordinates.Y] + transY);
            _max = new TCoordinate(_max[Ordinates.X] + transX, _max[Ordinates.Y] + transY);
        }

        /// <summary>
        /// Computes the coordinate of the center of this envelope 
        /// (as long as it is non-null).
        /// </summary>
        /// <returns>
        /// The center coordinate of this envelope, 
        /// or <see langword="null" /> if the envelope is null.
        /// </returns>.
        public TCoordinate Center
        {
            get
            {
                if (IsEmpty)
                {
                    return default(TCoordinate);
                }

                return new TCoordinate(
                    (Min[Ordinates.X] + Max[Ordinates.X]) / 2.0, 
                    (Min[Ordinates.Y] + Max[Ordinates.Y]) / 2.0);
            }
        }

        public IExtents<TCoordinate> Intersection(IExtents<TCoordinate> extents)
        {
            if (IsEmpty || extents.IsEmpty || !Intersects(extents))
            {
                return new Extents<TCoordinate>();
            }

            Double minX = Min[Ordinates.X];
            Double minY = Min[Ordinates.X];
            Double maxX = Max[Ordinates.X];
            Double maxY = Max[Ordinates.Y];
            

            return new Extents<TCoordinate>(Math.Max(minX, extents.Min[Ordinates.X]),
                                Math.Min(maxX, extents.Max[Ordinates.X]),
                                Math.Max(minY, extents.Min[Ordinates.Y]),
                                Math.Min(maxY, extents.Max[Ordinates.Y]));
        }

        /// <summary> 
        /// Check if the region defined by <c>other</c>
        /// overlaps (intersects) the region of this <see cref="Extents{TCoordinate}"/>.
        /// </summary>
        /// <param name="other"> the <see cref="Extents{TCoordinate}"/> which this <see cref="Extents{TCoordinate}"/> is
        /// being checked for overlapping.
        /// </param>
        /// <returns>        
        /// <see langword="true"/> if the <see cref="Extents{TCoordinate}"/>s overlap.
        /// </returns>
        public Boolean Intersects(IExtents<TCoordinate> other)
        {
            if (IsEmpty || other.IsEmpty)
            {
                return false;
            }

            return !(other.Min[Ordinates.X] > _max[Ordinates.X] || 
                other.Max[Ordinates.X] < _min[Ordinates.X] ||
                other.Min[Ordinates.Y] > _max[Ordinates.Y] ||
                other.Max[Ordinates.Y] < _min[Ordinates.Y]);
        }

        /// <summary>  
        /// Check if the point <paramref name="p"/> overlaps (lies inside) 
        /// the region of this <see cref="Extents{TCoordinate}"/>.
        /// </summary>
        /// <param name="p">The <typeparamref name="TCoordinate"/> to be tested.</param>
        /// <returns>
        /// <see langword="true"/> if the point overlaps this <see cref="Extents{TCoordinate}"/>.
        /// </returns>
        public Boolean Intersects(TCoordinate p)
        {
            return Intersects(p[Ordinates.X], p[Ordinates.Y]);
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

        /// <summary>  
        /// Returns <see langword="true"/> if the given point lies in or on the envelope.
        /// </summary>
        /// <param name="p"> the point which this <see cref="Extents{TCoordinate}"/> is
        /// being checked for containing.</param>
        /// <returns>    
        /// <see langword="true"/> if the point lies in the interior or
        /// on the boundary of this <see cref="Extents{TCoordinate}"/>.
        /// </returns>                
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

            return  other.Min[Ordinates.X] >= _min[Ordinates.X] && 
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

            return _max[Ordinates.X] == other.Max[Ordinates.X] && 
                   _max[Ordinates.Y] == other.Max[Ordinates.Y] &&
                   _min[Ordinates.X] == other.Min[Ordinates.X] &&
                   _min[Ordinates.Y] == other.Min[Ordinates.Y];
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
            else if (isEmpty || otherEmpty)
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
            Int32 result = 861101;
            result ^= _min.GetHashCode();
            result ^= _max.GetHashCode();
            return result;
        }

        public static Boolean operator ==(Extents<TCoordinate> left, Extents<TCoordinate> right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (!ReferenceEquals(left, null))
            {
                return left.Equals(right);
            }
            else
            {
                return right.Equals(left);
            }
        }

        public static Boolean operator !=(Extents<TCoordinate> left, Extents<TCoordinate> right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return "Env[" + _min[Ordinates.X] + " : " + _max[Ordinates.X] +
                ", " + _min[Ordinates.Y] + " : " + _max[Ordinates.Y] + "]";
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
                Double area = 1;
                area = area * (_max[Ordinates.X] - _min[Ordinates.X]);
                area = area * (_max[Ordinates.Y] - _min[Ordinates.Y]);
                return area;
            }
        }

        /// <summary>
        /// Creates a deep copy of the current envelope.
        /// </summary>
        /// <returns></returns>
        public IExtents<TCoordinate> Clone()
        {
            return new Extents<TCoordinate>(_min, _max);
        }

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
            throw new NotImplementedException();
        }

        public void ExpandToInclude(IEnumerable<TCoordinate> coordinates)
        {
            throw new NotImplementedException();
        }

        public void ExpandToInclude(IGeometry<TCoordinate> geometry)
        {
            throw new NotImplementedException();
        }

        public Double GetIntersectingArea(IGeometry<TCoordinate> geometry)
        {
            throw new NotImplementedException();
        }

        public IExtents<TCoordinate> Intersection(IGeometry<TCoordinate> extents)
        {
            throw new NotImplementedException();
        }

        public Boolean Intersects(TCoordinate coordinate, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public Boolean Intersects(IExtents<TCoordinate> other, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public TCoordinate Max
        {
            get { throw new NotImplementedException(); }
        }

        public TCoordinate Min
        {
            get { throw new NotImplementedException(); }
        }

        public Boolean Overlaps(TCoordinate coordinate, Tolerance tolerance)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public bool Touches(TCoordinate coordinate)
        {
            throw new NotImplementedException();
        }

        public bool Touches(IExtents<TCoordinate> other)
        {
            throw new NotImplementedException();
        }

        public bool Touches(TCoordinate coordinate, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public bool Touches(IExtents<TCoordinate> other, Tolerance tolerance)
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

            return new Extents<TCoordinate>(Math.Min(_min[Ordinates.X], box.Min[Ordinates.X]),
                                Math.Max(_max[Ordinates.X], box.Max[Ordinates.X]),
                                Math.Min(_min[Ordinates.Y], box.Min[Ordinates.Y]),
                                Math.Max(_max[Ordinates.Y], box.Max[Ordinates.Y]));
        }

        public bool Within(TCoordinate coordinate)
        {
            throw new NotImplementedException();
        }

        public bool Within(IExtents<TCoordinate> other)
        {
            throw new NotImplementedException();
        }

        public bool Within(TCoordinate coordinate, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public bool Within(IExtents<TCoordinate> other, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IExtents Members

        public Boolean Borders(IExtents other)
        {
            throw new NotImplementedException();
        }

        public Boolean Borders(IExtents other, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        ICoordinate IExtents.Center
        {
            get { throw new NotImplementedException(); }
        }

        public Boolean Contains(params Double[] coordinate)
        {
            throw new NotImplementedException();
        }

        public Boolean Contains(IExtents other)
        {
            throw new NotImplementedException();
        }

        public Boolean Contains(ICoordinate other)
        {
            throw new NotImplementedException();
        }

        public Boolean Contains(Tolerance tolerance, params Double[] coordinate)
        {
            throw new NotImplementedException();
        }

        public Boolean Contains(IExtents other, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public Boolean Contains(ICoordinate other, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public Double Distance(IExtents extents)
        {
            throw new NotImplementedException();
        }

        public void ExpandToInclude(params Double[] coordinate)
        {
            throw new NotImplementedException();
        }

        public void ExpandToInclude(IExtents other)
        {
            throw new NotImplementedException();
        }

        public void ExpandToInclude(IGeometry other)
        {
            throw new NotImplementedException();
        }

        public IExtents Intersection(IExtents extents)
        {
            throw new NotImplementedException();
        }

        public Boolean Intersects(params Double[] coordinate)
        {
            throw new NotImplementedException();
        }

        public Boolean Intersects(IExtents other)
        {
            throw new NotImplementedException();
        }

        public Boolean Intersects(Tolerance tolerance, params Double[] coordinate)
        {
            throw new NotImplementedException();
        }

        public Boolean Intersects(IExtents other, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        ICoordinate IExtents.Max
        {
            get { throw new NotImplementedException(); }
        }

        ICoordinate IExtents.Min
        {
            get { throw new NotImplementedException(); }
        }

        public Double GetMax(Ordinates ordinate)
        {
            throw new NotImplementedException();
        }

        public Double GetMin(Ordinates ordinate)
        {
            throw new NotImplementedException();
        }

        public Double GetSize(Ordinates axis)
        {
            throw new NotImplementedException();
        }

        public Double GetSize(Ordinates axis1, Ordinates axis2)
        {
            throw new NotImplementedException();
        }

        public Double GetSize(Ordinates axis1, Ordinates axis2, Ordinates axis3)
        {
            throw new NotImplementedException();
        }

        public Double GetSize(params Ordinates[] axes)
        {
            throw new NotImplementedException();
        }

        public Boolean Overlaps(params Double[] coordinate)
        {
            throw new NotImplementedException();
        }

        public Boolean Overlaps(ICoordinate other)
        {
            throw new NotImplementedException();
        }

        public Boolean Overlaps(IExtents other)
        {
            throw new NotImplementedException();
        }

        public void Scale(params Double[] vector)
        {
            throw new NotImplementedException();
        }

        public void Scale(Double factor)
        {
            throw new NotImplementedException();
        }

        public void Scale(Double factor, Ordinates axis)
        {
            throw new NotImplementedException();
        }

        IGeometry IExtents.ToGeometry()
        {
            throw new NotImplementedException();
        }

        public void Translate(params Double[] vector)
        {
            throw new NotImplementedException();
        }

        public void Transform(IMatrix<NPack.DoubleComponent> transformMatrix)
        {
            throw new NotImplementedException();
        }

        public IExtents Union(IPoint point)
        {
            throw new NotImplementedException();
        }

        public IExtents Union(IExtents box)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEquatable<IExtents> Members

        public Boolean Equals(IExtents other)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}