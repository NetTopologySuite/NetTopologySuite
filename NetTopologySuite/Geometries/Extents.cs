using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Defines a rectangular region of the 2D coordinate plane.
    /// It is often used to represent the bounding box of a <see cref="Geometry{TCoordinate}"/>,
    /// e.g. the minimum and maximum x and y values of the <c>Coordinate</c>s.
    /// Note that Envelopes support infinite or half-infinite regions, by using the values of
    /// <c>Double.PositiveInfinity</c> and <c>Double.NegativeInfinity</c>.
    /// When Envelope objects are created or initialized,
    /// the supplies extent values are automatically sorted into the correct order.    
    /// </summary>
    [Serializable]
    public class Extents<TCoordinate> : IExtents<TCoordinate>
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<TCoordinate>, IConvertible
    {
        /// <summary>
        /// Test the point q to see whether it intersects the Envelope
        /// defined by p1-p2.
        /// </summary>
        /// <param name="p1">One extremal point of the envelope.</param>
        /// <param name="p2">Another extremal point of the envelope.</param>
        /// <param name="q">Point to test for intersection.</param>
        /// <returns><see langword="true"/> if q intersects the envelope p1-p2.</returns>
        public static Boolean Intersects(ICoordinate p1, ICoordinate p2, ICoordinate q)
        {
            if (((q.X >= (p1.X < p2.X ? p1.X : p2.X)) && (q.X <= (p1.X > p2.X ? p1.X : p2.X))) &&
                ((q.Y >= (p1.Y < p2.Y ? p1.Y : p2.Y)) && (q.Y <= (p1.Y > p2.Y ? p1.Y : p2.Y))))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Test the envelope defined by p1-p2 for intersection
        /// with the envelope defined by q1-q2
        /// </summary>
        /// <param name="p1">One extremal point of the envelope Point.</param>
        /// <param name="p2">Another extremal point of the envelope Point.</param>
        /// <param name="q1">One extremal point of the envelope Q.</param>
        /// <param name="q2">Another extremal point of the envelope Q.</param>
        /// <returns><see langword="true"/> if Q intersects Point</returns>
        public static Boolean Intersects(ICoordinate p1, ICoordinate p2, ICoordinate q1, ICoordinate q2)
        {
            Double minq = Math.Min(q1.X, q2.X);
            Double maxq = Math.Max(q1.X, q2.X);
            Double minp = Math.Min(p1.X, p2.X);
            Double maxp = Math.Max(p1.X, p2.X);

            if (minp > maxq)
            {
                return false;
            }

            if (maxp < minq)
            {
                return false;
            }

            minq = Math.Min(q1.Y, q2.Y);
            maxq = Math.Max(q1.Y, q2.Y);
            minp = Math.Min(p1.Y, p2.Y);
            maxp = Math.Max(p1.Y, p2.Y);

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

        /*
        *  the minimum x-coordinate
        */
        private Double minx;

        /*
        *  the maximum x-coordinate
        */
        private Double maxx;

        /*
        * the minimum y-coordinate
        */
        private Double miny;

        /*
        *  the maximum y-coordinate
        */
        private Double maxy;

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
            if (x1 < x2)
            {
                minx = x1;
                maxx = x2;
            }
            else
            {
                minx = x2;
                maxx = x1;
            }

            if (y1 < y2)
            {
                miny = y1;
                maxy = y2;
            }
            else
            {
                miny = y2;
                maxy = y1;
            }
        }

        /// <summary>
        /// Initialize an <see cref="Extents{TCoordinate}"/> for a region defined by two Coordinates.
        /// </summary>
        /// <param name="p1">The first Coordinate.</param>
        /// <param name="p2">The second Coordinate.</param>
        public void Init(ICoordinate p1, ICoordinate p2)
        {
            Init(p1.X, p2.X, p1.Y, p2.Y);
        }

        /// <summary>
        /// Initialize an <see cref="Extents{TCoordinate}"/> for a region defined by a single Coordinate.
        /// </summary>
        /// <param name="p">The Coordinate.</param>
        public void Init(ICoordinate p)
        {
            Init(p.X, p.X, p.Y, p.Y);
        }

        /// <summary>
        /// Initialize an <see cref="Extents{TCoordinate}"/> from an existing Envelope.
        /// </summary>
        /// <param name="env">The Envelope to initialize from.</param>
        public void Init(IExtents env)
        {
            minx = env.MinX;
            maxx = env.MaxX;
            miny = env.MinY;
            maxy = env.MaxY;
        }

        /// <summary>
        /// Makes this <see cref="Extents{TCoordinate}"/> a "null" envelope..
        /// </summary>
        public void SetToEmpty()
        {
            minx = 0;
            maxx = -1;
            miny = 0;
            maxy = -1;
        }

        /// <summary>
        /// Returns <see langword="true"/> if this <see cref="Extents{TCoordinate}"/> 
        /// is an empty envelope.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if this <see cref="Extents{TCoordinate}"/> is uninitialized
        /// or is the envelope of the empty point.
        /// </returns>
        public Boolean IsNull
        {
            get { return maxx < minx; }
        }

        /// <summary>
        /// Returns the difference between the maximum and minimum x values.
        /// </summary>
        /// <returns>max x - min x, or 0 if this is a null <see cref="Extents{TCoordinate}"/>.</returns>
        public Double Width
        {
            get
            {
                if (IsNull)
                {
                    return 0;
                }

                return maxx - minx;
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
                if (IsNull)
                {
                    return 0;
                }

                return maxy - miny;
            }
        }

        /// <summary>
        /// Returns the <see cref="Extents{TCoordinate}"/>s minimum x-value. min x > max x
        /// indicates that this is a null <see cref="Extents{TCoordinate}"/>.
        /// </summary>
        /// <returns>The minimum x-coordinate.</returns>
        public Double MinX
        {
            get { return minx; }
        }

        /// <summary>
        /// Returns the <see cref="Extents{TCoordinate}"/>s maximum x-value. min x > max x
        /// indicates that this is a null <see cref="Extents{TCoordinate}"/>.
        /// </summary>
        /// <returns>The maximum x-coordinate.</returns>
        public Double MaxX
        {
            get { return maxx; }
        }

        /// <summary>
        /// Returns the <see cref="Extents{TCoordinate}"/>s minimum y-value. min y > max y
        /// indicates that this is a null <see cref="Extents{TCoordinate}"/>.
        /// </summary>
        /// <returns>The minimum y-coordinate.</returns>
        public Double MinY
        {
            get { return miny; }
        }

        /// <summary>
        /// Returns the <see cref="Extents{TCoordinate}"/>s maximum y-value. min y > max y
        /// indicates that this is a null <see cref="Extents{TCoordinate}"/>.
        /// </summary>
        /// <returns>The maximum y-coordinate.</returns>
        public Double MaxY
        {
            get { return maxy; }
        }

        /// <summary>
        /// Expands this envelope by a given distance in all directions.
        /// Both positive and negative distances are supported.
        /// </summary>
        /// <param name="distance">The distance to expand the envelope.</param>
        public void ExpandBy(Double distance)
        {
            ExpandBy(distance, distance);
        }

        /// <summary>
        /// Expands this envelope by a given distance in all directions.
        /// Both positive and negative distances are supported.
        /// </summary>
        /// <param name="deltaX">The distance to expand the envelope along the the X axis.</param>
        /// <param name="deltaY">The distance to expand the envelope along the the Y axis.</param>
        public void ExpandBy(Double deltaX, Double deltaY)
        {
            if (IsNull)
            {
                return;
            }

            minx -= deltaX;
            maxx += deltaX;
            miny -= deltaY;
            maxy += deltaY;

            // check for envelope disappearing
            if (minx > maxx || miny > maxy)
            {
                SetToEmpty();
            }
        }

        /// <summary>
        /// Enlarges the boundary of the <see cref="Extents{TCoordinate}"/> so that it contains (p).
        /// Does nothing if (p) is already on or within the boundaries.
        /// </summary>
        /// <param name="p">The Coordinate.</param>
        public void ExpandToInclude(ICoordinate p)
        {
            ExpandToInclude(p.X, p.Y);
        }

        /// <summary>
        /// Enlarges the boundary of the <see cref="Extents{TCoordinate}"/> so that it contains
        /// (x,y). Does nothing if (x,y) is already on or within the boundaries.
        /// </summary>
        /// <param name="x">The value to lower the minimum x to or to raise the maximum x to.</param>
        /// <param name="y">The value to lower the minimum y to or to raise the maximum y to.</param>
        public void ExpandToInclude(Double x, Double y)
        {
            if (IsNull)
            {
                minx = x;
                maxx = x;
                miny = y;
                maxy = y;
            }
            else
            {
                if (x < minx)
                {
                    minx = x;
                }

                if (x > maxx)
                {
                    maxx = x;
                }

                if (y < miny)
                {
                    miny = y;
                }

                if (y > maxy)
                {
                    maxy = y;
                }
            }
        }

        /// <summary>
        /// Enlarges the boundary of the <see cref="Extents{TCoordinate}"/> so that it contains
        /// <c>other</c>. Does nothing if <c>other</c> is wholly on or
        /// within the boundaries.
        /// </summary>
        /// <param name="other">the <see cref="Extents{TCoordinate}"/> to merge with.</param>        
        public void ExpandToInclude(IExtents other)
        {
            if (other.IsNull)
            {
                return;
            }

            if (IsNull)
            {
                minx = other.MinX;
                maxx = other.MaxX;
                miny = other.MinY;
                maxy = other.MaxY;
            }
            else
            {
                if (other.MinX < minx)
                {
                    minx = other.MinX;
                }

                if (other.MaxX > maxx)
                {
                    maxx = other.MaxX;
                }

                if (other.MinY < miny)
                {
                    miny = other.MinY;
                }

                if (other.MaxY > maxy)
                {
                    maxy = other.MaxY;
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
            if (IsNull)
            {
                return;
            }

            Init(MinX + transX, MaxX + transX, MinY + transY, MaxY + transY);
        }

        /// <summary>
        /// Computes the coordinate of the centre of this envelope (as long as it is non-null).
        /// </summary>
        /// <returns>
        /// The centre coordinate of this envelope, 
        /// or <see langword="null" /> if the envelope is null.
        /// </returns>.
        public ICoordinate Centre
        {
            get
            {
                if (IsNull)
                {
                    return null;
                }

                return new Coordinate((MinX + MaxX) / 2.0, (MinY + MaxY) / 2.0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        public IExtents Intersection(IExtents env)
        {
            if (IsNull || env.IsNull || !Intersects(env))
            {
                return new Extents();
            }

            return new Extents(Math.Max(MinX, env.MinX),
                                Math.Min(MaxX, env.MaxX),
                                Math.Max(MinY, env.MinY),
                                Math.Min(MaxY, env.MaxY));
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
        public Boolean Intersects(IExtents other)
        {
            if (IsNull || other.IsNull)
            {
                return false;
            }

            return !(other.MinX > maxx || other.MaxX < minx || other.MinY > maxy || other.MaxY < miny);
        }

        /// <summary>
        /// Use Intersects instead. In the future, Overlaps may be
        /// changed to be a true overlap check; that is, whether the intersection is
        /// two-dimensional.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        [Obsolete("Use Intersects instead")]
        public Boolean Overlaps(IExtents other)
        {
            return Intersects(other);
        }

        /// <summary>
        /// Use Intersects instead.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        [Obsolete("Use Intersects instead")]
        public Boolean Overlaps(ICoordinate p)
        {
            return Intersects(p);
        }

        /// <summary>
        /// Use Intersects instead.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        [Obsolete("Use Intersects instead")]
        public Boolean Overlaps(Double x, Double y)
        {
            return Intersects(x, y);
        }

        /// <summary>  
        /// Check if the point <c>p</c> overlaps (lies inside) the region of this <see cref="Extents{TCoordinate}"/>.
        /// </summary>
        /// <param name="p"> the <c>Coordinate</c> to be tested.</param>
        /// <returns><see langword="true"/> if the point overlaps this <see cref="Extents{TCoordinate}"/>.</returns>
        public Boolean Intersects(ICoordinate p)
        {
            return Intersects(p.X, p.Y);
        }

        /// <summary>  
        /// Check if the point <c>(x, y)</c> overlaps (lies inside) the region of this <see cref="Extents{TCoordinate}"/>.
        /// </summary>
        /// <param name="x"> the x-ordinate of the point.</param>
        /// <param name="y"> the y-ordinate of the point.</param>
        /// <returns><see langword="true"/> if the point overlaps this <see cref="Extents{TCoordinate}"/>.</returns>
        public Boolean Intersects(Double x, Double y)
        {
            return !(x > maxx || x < minx || y > maxy || y < miny);
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
        public Boolean Contains(ICoordinate p)
        {
            return Contains(p.X, p.Y);
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
            return x >= minx && x <= maxx && y >= miny && y <= maxy;
        }

        /// <summary>  
        /// Returns <see langword="true"/> if the <c>Envelope other</c>
        /// lies wholely inside this <see cref="Extents{TCoordinate}"/> (inclusive of the boundary).
        /// </summary>
        /// <param name="other"> the <see cref="Extents{TCoordinate}"/> which this <see cref="Extents{TCoordinate}"/> is being checked for containing.</param>
        /// <returns><see langword="true"/> if <c>other</c> is contained in this <see cref="Extents{TCoordinate}"/>.</returns>
        public Boolean Contains(IExtents other)
        {
            if (IsNull || other.IsNull)
            {
                return false;
            }

            return other.MinX >= minx && other.MaxX <= maxx &&
                   other.MinY >= miny && other.MaxY <= maxy;
        }

        /// <summary> 
        /// Computes the distance between this and another
        /// <see cref="Extents{TCoordinate}"/>.
        /// The distance between overlapping Envelopes is 0.  Otherwise, the
        /// distance is the Euclidean distance between the closest points.
        /// </summary>
        /// <returns>The distance between this and another <see cref="Extents{TCoordinate}"/>.</returns>
        public Double Distance(IExtents env)
        {
            if (Intersects(env))
            {
                return 0;
            }

            Double dx = 0.0;

            if (maxx < env.MinX)
            {
                dx = env.MinX - maxx;
            }

            if (minx > env.MaxX)
            {
                dx = minx - env.MaxX;
            }

            Double dy = 0.0;

            if (maxy < env.MinY)
            {
                dy = env.MinY - maxy;
            }

            if (miny > env.MaxY)
            {
                dy = miny - env.MaxY;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override Boolean Equals(object other)
        {
            if (other == null)
            {
                return false;
            }

            if (!(other is Extents))
            {
                return false;
            }

            return Equals((IExtents)other);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public Boolean Equals(IExtents other)
        {
            if (IsNull)
            {
                return other.IsNull;
            }

            return maxx == other.MaxX && maxy == other.MaxY &&
                   minx == other.MinX && miny == other.MinY;
        }

        public Int32 CompareTo(object other)
        {
            return CompareTo((IExtents)other);
        }

        public Int32 CompareTo(IExtents other)
        {
            if (IsNull && other.IsNull)
            {
                return 0;
            }
            else if (!IsNull && other.IsNull)
            {
                return 1;
            }
            else if (IsNull && !other.IsNull)
            {
                return -1;
            }

            if (Area > other.Area)
            {
                return 1;
            }

            if (Area < other.Area)
            {
                return -1;
            }

            return 0;
        }

        public override Int32 GetHashCode()
        {
            Int32 result = 17;
            result = 37 * result + GetHashCode(minx);
            result = 37 * result + GetHashCode(maxx);
            result = 37 * result + GetHashCode(miny);
            result = 37 * result + GetHashCode(maxy);
            return result;
        }

        /// <summary>
        /// Return HashCode.
        /// </summary>
        /// <param name="x">Value from HashCode computation.</param>
        private static Int32 GetHashCode(Double value)
        {
            long f = BitConverter.DoubleToInt64Bits(value);
            return (Int32)(f ^ (f >> 32));
        }

        public static Boolean operator ==(Extents obj1, Extents obj2)
        {
            return Equals(obj1, obj2);
        }

        public static Boolean operator !=(Extents obj1, Extents obj2)
        {
            return !(obj1 == obj2);
        }

        public override string ToString()
        {
            return "Env[" + minx + " : " + maxx + ", " + miny + " : " + maxy + "]";
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
                area = area * (maxx - minx);
                area = area * (maxy - miny);
                return area;
            }
        }

        /// <summary>
        /// Creates a deep copy of the current envelope.
        /// </summary>
        /// <returns></returns>
        public IExtents Clone()
        {
            return new Extents(minx, maxx, miny, maxy);
        }

        /// <summary>
        /// Calculates the union of the current box and the given point.
        /// </summary>
        public IExtents Union(IPoint point)
        {
            return Union(point.Coordinate);
        }

        /// <summary>
        /// Calculates the union of the current box and the given coordinate.
        /// </summary>
        public IExtents Union(ICoordinate coord)
        {
            Extents env = (Extents)Clone();
            env.ExpandToInclude(coord);
            return env;
        }

        /// <summary>
        /// Calculates the union of the current box and the given box.
        /// </summary>
        public IExtents Union(IExtents box)
        {
            if (box.IsNull)
            {
                return this;
            }

            if (IsNull)
            {
                return box;
            }

            return new Extents(Math.Min(minx, box.MinX),
                                Math.Max(maxx, box.MaxX),
                                Math.Min(miny, box.MinY),
                                Math.Max(maxy, box.MaxY));
        }

        /// <summary>
        /// Moves the envelope to the indicated coordinate.
        /// </summary>
        /// <param name="centre">The new centre coordinate.</param>
        public void SetCentre(ICoordinate centre)
        {
            SetCentre(centre, Width, Height);
        }

        /// <summary>
        /// Moves the envelope to the indicated point.
        /// </summary>
        /// <param name="centre">The new centre point.</param>
        public void SetCentre(IPoint centre)
        {
            SetCentre(centre.Coordinate, Width, Height);
        }

        /// <summary>
        /// Resizes the envelope to the indicated point.
        /// </summary>
        /// <param name="width">The new width.</param>
        /// <param name="height">The new height.</param>
        public void SetCentre(Double width, Double height)
        {
            SetCentre(Centre, width, height);
        }

        /// <summary>
        /// Moves and resizes the current envelope.
        /// </summary>
        /// <param name="centre">The new centre point.</param>
        /// <param name="width">The new width.</param>
        /// <param name="height">The new height.</param>
        public void SetCentre(IPoint centre, Double width, Double height)
        {
            SetCentre(centre.Coordinate, width, height);
        }

        /// <summary>
        /// Moves and resizes the current envelope.
        /// </summary>
        /// <param name="centre">The new centre coordinate.</param>
        /// <param name="width">The new width.</param>
        /// <param name="height">The new height.</param>
        public void SetCentre(ICoordinate centre, Double width, Double height)
        {
            minx = centre.X - (width / 2);
            maxx = centre.X + (width / 2);
            miny = centre.Y - (height / 2);
            maxy = centre.Y + (height / 2);
        }

        /// <summary>
        /// Zoom the box. 
        /// Possible values are e.g. 50 (to zoom in a 50%) or -50 (to zoom out a 50%).
        /// </summary>
        /// <param name="perCent"> 
        /// Negative do Envelope smaller.
        /// Positive do Envelope bigger.
        /// </param>
        /// <example> 
        ///  perCent = -50 compact the envelope a 50% (make it smaller).
        ///  perCent = 200 enlarge envelope by 2.
        /// </example>
        public void Zoom(Double perCent)
        {
            Double w = (Width * perCent / 100);
            Double h = (Height * perCent / 100);
            SetCentre(w, h);
        }

        /* END ADDED BY MPAUL42: monoGIS team */
    }
}