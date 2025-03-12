using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Defines a rectangular region of the 2D coordinate plane.
    /// </summary>
    /// <remarks>
    /// It is often used to represent the bounding box of a <c>Geometry</c>,
    /// e.g. the minimum and maximum x and y values of the <c>Coordinate</c>s.
    /// Note that Envelopes support infinite or half-infinite regions, by using the values of
    /// <c>Double.PositiveInfinity</c> and <c>Double.NegativeInfinity</c>.
    /// When Envelope objects are created or initialized,
    /// the supplied extent values are automatically sorted into the correct order.
    /// </remarks>
    [Serializable]
#pragma warning disable 612,618
    public class Envelope : IComparable<Envelope>, IIntersectable<Envelope>, IExpandable<Envelope>
#pragma warning restore 612,618
    {
        /// <summary>
        /// Test the point q to see whether it intersects the Envelope
        /// defined by p1-p2.
        /// </summary>
        /// <param name="p1">One extremal point of the envelope.</param>
        /// <param name="p2">Another extremal point of the envelope.</param>
        /// <param name="q">Point to test for intersection.</param>
        /// <returns><c>true</c> if q intersects the envelope p1-p2.</returns>
        public static bool Intersects(Coordinate p1, Coordinate p2, Coordinate q)
        {
            return ((q.X >= (p1.X < p2.X ? p1.X : p2.X)) && (q.X <= (p1.X > p2.X ? p1.X : p2.X))) &&
                   ((q.Y >= (p1.Y < p2.Y ? p1.Y : p2.Y)) && (q.Y <= (p1.Y > p2.Y ? p1.Y : p2.Y)));
        }

        /// <summary>
        /// Tests whether the envelope defined by p1-p2
        /// and the envelope defined by q1-q2
        /// intersect.
        /// </summary>
        /// <param name="p1">One extremal point of the envelope Point.</param>
        /// <param name="p2">Another extremal point of the envelope Point.</param>
        /// <param name="q1">One extremal point of the envelope Q.</param>
        /// <param name="q2">Another extremal point of the envelope Q.</param>
        /// <returns><c>true</c> if Q intersects Point</returns>
        public static bool Intersects(Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2)
        {
            double minP = Math.Min(p1.X, p2.X);
            double maxQ = Math.Max(q1.X, q2.X);
            if (minP > maxQ)
                return false;

            double minQ = Math.Min(q1.X, q2.X);
            double maxP = Math.Max(p1.X, p2.X);
            if (maxP < minQ)
                return false;

            minP = Math.Min(p1.Y, p2.Y);
            maxQ = Math.Max(q1.Y, q2.Y);
            if (minP > maxQ)
                return false;

            minQ = Math.Min(q1.Y, q2.Y);
            maxP = Math.Max(p1.Y, p2.Y);
            if (maxP < minQ)
                return false;

            return true;
        }

        /// <summary>
        /// The minimum x-coordinate
        /// </summary>
        private double _minX;

        /// <summary>
        /// The maximum x-coordinate
        /// </summary>
        private double _maxX;

        /// <summary>
        /// The minimum y-coordinate
        /// </summary>
        private double _minY;

        /// <summary>
        /// The maximum y-coordinate
        /// </summary>
        private double _maxY;

        /// <summary>
        /// Creates a null <c>Envelope</c>.
        /// </summary>
        public Envelope()
        {
            Init();
        }

        /// <summary>
        /// Creates an <c>Envelope</c> for a region defined by maximum and minimum values.
        /// </summary>
        /// <param name="x1">The first x-value.</param>
        /// <param name="x2">The second x-value.</param>
        /// <param name="y1">The first y-value.</param>
        /// <param name="y2">The second y-value.</param>
        public Envelope(double x1, double x2, double y1, double y2)
        {
            Init(x1, x2, y1, y2);
        }

        /// <summary>
        /// Creates an <c>Envelope</c> for a region defined by two Coordinates.
        /// </summary>
        /// <param name="p1">The first Coordinate.</param>
        /// <param name="p2">The second Coordinate.</param>
        public Envelope(Coordinate p1, Coordinate p2)
        {
            Init(p1.X, p2.X, p1.Y, p2.Y);
        }

        /// <summary>
        /// Creates an <c>Envelope</c> for a region defined by a single Coordinate.
        /// </summary>
        /// <param name="p">The Coordinate.</param>
        public Envelope(Coordinate p)
        {
            Init(p.X, p.X, p.Y, p.Y);
        }

        /// <summary>
        /// Creates an <c>Envelope</c> for a region defined by an enumeration of <c>Coordinate</c>s.
        /// </summary>
        /// <param name="pts">The <c>Coordinates</c>.</param>
        public Envelope(IEnumerable<Coordinate> pts) : this()
        {
            if (pts == null)
                return;

            foreach (var pt in pts)
                ExpandToInclude(pt);
        }

        /// <summary>
        /// Creates an <c>Envelope</c> for a region defined by a <c>CoordinateSequence</c>s.
        /// </summary>
        /// <param name="sequence">The <c>CoordinateSequence</c>.</param>
        public Envelope(CoordinateSequence sequence) : this()
        {
            sequence.ExpandEnvelope(this);
        }

        /// <summary>
        /// Create an <c>Envelope</c> from an existing Envelope.
        /// </summary>
        /// <param name="env">The Envelope to initialize from.</param>
        public Envelope(Envelope env)
        {
            Init(env);
        }

        /// <summary>
        /// Initialize to a null <c>Envelope</c>.
        /// </summary>
        public void Init()
        {
            SetToNull();
        }

        /// <summary>
        /// Initialize an <c>Envelope</c> for a region defined by maximum and minimum values.
        /// </summary>
        /// <param name="x1">The first x-value.</param>
        /// <param name="x2">The second x-value.</param>
        /// <param name="y1">The first y-value.</param>
        /// <param name="y2">The second y-value.</param>
        public void Init(double x1, double x2, double y1, double y2)
        {
            if (double.IsNaN(x1) || double.IsNaN(x2) || double.IsNaN(y1) || double.IsNaN(y2))
                throw new ArgumentException("An ordinate value is double.NaN");

            if (x1 < x2)
            {
                _minX = x1;
                _maxX = x2;
            }
            else
            {
                _minX = x2;
                _maxX = x1;
            }

            if (y1 < y2)
            {
                _minY = y1;
                _maxY = y2;
            }
            else
            {
                _minY = y2;
                _maxY = y1;
            }
        }

        /// <summary>
        /// Initialize an <c>Envelope</c> for a region defined by two Coordinates.
        /// </summary>
        /// <param name="p1">The first Coordinate.</param>
        /// <param name="p2">The second Coordinate.</param>
        public void Init(Coordinate p1, Coordinate p2)
        {
            Init(p1.X, p2.X, p1.Y, p2.Y);
        }

        /// <summary>
        /// Initialize an <c>Envelope</c> for a region defined by a single Coordinate.
        /// </summary>
        /// <param name="p">The Coordinate.</param>
        public void Init(Coordinate p)
        {
            Init(p.X, p.X, p.Y, p.Y);
        }

        /// <summary>
        /// Initialize an <c>Envelope</c> from an existing Envelope.
        /// </summary>
        /// <param name="env">The Envelope to initialize from.</param>
        public void Init(Envelope env)
        {
            _minX = env.MinX;
            _maxX = env.MaxX;
            _minY = env.MinY;
            _maxY = env.MaxY;
        }

        /// <summary>
        /// Makes this <c>Envelope</c> a "null" envelope..
        /// </summary>
        public void SetToNull()
        {
            _minX = 0;
            _maxX = -1;
            _minY = 0;
            _maxY = -1;
        }

        /// <summary>
        /// Returns <c>true</c> if this <c>Envelope</c> is a "null" envelope.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this <c>Envelope</c> is uninitialized
        /// or is the envelope of the empty point.
        /// </returns>
        public bool IsNull
        {
            get
            {
                return _maxX < _minX;
            }
        }

        /// <summary>
        /// Returns the difference between the maximum and minimum x values.
        /// </summary>
        /// <returns>max x - min x, or 0 if this is a null <c>Envelope</c>.</returns>
        public double Width
        {
            get
            {
                if (IsNull)
                    return 0;
                return _maxX - _minX;
            }
        }

        /// <summary>
        /// Returns the difference between the maximum and minimum y values.
        /// </summary>
        /// <returns>max y - min y, or 0 if this is a null <c>Envelope</c>.</returns>
        public double Height
        {
            get
            {
                if (IsNull)
                    return 0;
                return _maxY - _minY;
            }
        }

        /// <summary>
        /// Gets the length of the diameter (diagonal) of the envelope.
        /// </summary>
        /// <returns>The diameter length</returns>
        public double Diameter
        {
            get
            {
                if (IsNull)
                    return 0;

                double w = Width;
                double h = Height;
                return Math.Sqrt(w * w + h * h);
            }
        }

        /// <summary>
        /// Returns the <c>Envelope</c>s minimum x-value. min x > max x
        /// indicates that this is a null <c>Envelope</c>.
        /// </summary>
        /// <returns>The minimum x-coordinate.</returns>
        public double MinX
        {
            get { return _minX; }
        }

        /// <summary>
        /// Returns the <c>Envelope</c>s maximum x-value. min x > max x
        /// indicates that this is a null <c>Envelope</c>.
        /// </summary>
        /// <returns>The maximum x-coordinate.</returns>
        public double MaxX
        {
            get { return _maxX; }
        }

        /// <summary>
        /// Returns the <c>Envelope</c>s minimum y-value. min y > max y
        /// indicates that this is a null <c>Envelope</c>.
        /// </summary>
        /// <returns>The minimum y-coordinate.</returns>
        public double MinY
        {
            get { return _minY; }
        }

        /// <summary>
        /// Returns the <c>Envelope</c>s maximum y-value. min y > max y
        /// indicates that this is a null <c>Envelope</c>.
        /// </summary>
        /// <returns>The maximum y-coordinate.</returns>
        public double MaxY
        {
            get { return _maxY; }
        }

        /// <summary>
        /// Gets the area of this envelope.
        /// </summary>
        /// <returns>The area of the envelope, or 0.0 if envelope is null</returns>
        public double Area
        {
            get
            {
                return Width * Height;
            }
        }

        /// <summary>
        /// Expands this envelope by a given distance in all directions.
        /// Both positive and negative distances are supported.
        /// </summary>
        /// <param name="distance">The distance to expand the envelope.</param>
        public void ExpandBy(double distance)
        {
            ExpandBy(distance, distance);
        }

        /// <summary>
        /// Expands this envelope by a given distance in all directions.
        /// Both positive and negative distances are supported.
        /// </summary>
        /// <param name="deltaX">The distance to expand the envelope along the the X axis.</param>
        /// <param name="deltaY">The distance to expand the envelope along the the Y axis.</param>
        public void ExpandBy(double deltaX, double deltaY)
        {
            if (IsNull)
                return;

            _minX -= deltaX;
            _maxX += deltaX;
            _minY -= deltaY;
            _maxY += deltaY;

            // check for envelope disappearing
            if (_minX > _maxX || _minY > _maxY)
                SetToNull();
        }

        /// <summary>
        /// Gets the minimum extent of this envelope across both dimensions.
        /// </summary>
        /// <returns></returns>
        public double MinExtent
        {
            get
            {
                if (IsNull) return 0.0;
                double w = Width;
                double h = Height;
                if (w < h) return w;
                return h;
            }
        }

        /// <summary>
        /// Gets the maximum extent of this envelope across both dimensions.
        /// </summary>
        /// <returns></returns>
        public double MaxExtent
        {
            get
            {
                if (IsNull) return 0.0;
                double w = Width;
                double h = Height;
                if (w > h) return w;
                return h;
            }
        }

        /// <summary>
        /// Enlarges this <c>Envelope</c> so that it contains
        /// the given <see cref="Coordinate"/>.
        /// Has no effect if the point is already on or within the envelope.
        /// </summary>
        /// <param name="p">The Coordinate.</param>
        public void ExpandToInclude(Coordinate p)
        {
            ExpandToInclude(p.X, p.Y);
        }

        /// <summary>
        /// Enlarges this <c>Envelope</c> so that it contains
        /// the given <see cref="Coordinate"/>.
        /// </summary>
        /// <remarks>Has no effect if the point is already on or within the envelope.</remarks>
        /// <param name="x">The value to lower the minimum x to or to raise the maximum x to.</param>
        /// <param name="y">The value to lower the minimum y to or to raise the maximum y to.</param>
        public void ExpandToInclude(double x, double y)
        {
            if (double.IsNaN(x) || double.IsNaN(y))
                return;

            if (IsNull)
            {
                _minX = x;
                _maxX = x;
                _minY = y;
                _maxY = y;
            }
            else
            {
                if (x < _minX)
                    _minX = x;
                if (x > _maxX)
                    _maxX = x;
                if (y < _minY)
                    _minY = y;
                if (y > _maxY)
                    _maxY = y;
            }
        }

        /// <summary>
        /// Enlarges this <c>Envelope</c> so that it contains
        /// the <c>other</c> Envelope.
        /// Has no effect if <c>other</c> is wholly on or
        /// within the envelope.
        /// </summary>
        /// <param name="other">the <c>Envelope</c> to expand to include.</param>
        public void ExpandToInclude(Envelope other)
        {
            if (other.IsNull)
                return;
            if (IsNull)
            {
                _minX = other.MinX;
                _maxX = other.MaxX;
                _minY = other.MinY;
                _maxY = other.MaxY;
            }
            else
            {
                if (other.MinX < _minX)
                    _minX = other.MinX;
                if (other.MaxX > _maxX)
                    _maxX = other.MaxX;
                if (other.MinY < _minY)
                    _minY = other.MinY;
                if (other.MaxY > _maxY)
                    _maxY = other.MaxY;
            }
        }

        /// <summary>
        /// Enlarges this <c>Envelope</c> so that it contains
        /// the <c>other</c> Envelope.
        /// Has no effect if <c>other</c> is wholly on or
        /// within the envelope.
        /// </summary>
        /// <param name="other">the <c>Envelope</c> to expand to include.</param>
        public Envelope ExpandedBy(Envelope other)
        {
            if (other.IsNull)
                return this;
            if (IsNull)
                return other;

            double minX = (other._minX < _minX) ? other._minX : _minX;
            double maxX = (other._maxX > _maxX) ? other._maxX : _maxX;
            double minY = (other._minY < _minY) ? other._minY : _minY;
            double maxY = (other._maxY > _maxY) ? other._maxY : _maxY;
            return new Envelope(minX, maxX, minY, maxY);
        }
        /// <summary>
        /// Translates this envelope by given amounts in the X and Y direction.
        /// </summary>
        /// <param name="transX">The amount to translate along the X axis.</param>
        /// <param name="transY">The amount to translate along the Y axis.</param>
        public void Translate(double transX, double transY)
        {
            if (IsNull)
                return;
            Init(MinX + transX, MaxX + transX, MinY + transY, MaxY + transY);
        }

        /// <summary>
        /// Computes the coordinate of the centre of this envelope (as long as it is non-null).
        /// </summary>
        /// <returns>
        /// The centre coordinate of this envelope,
        /// or <c>null</c> if the envelope is null.
        /// </returns>.
        public Coordinate Centre
        {
            get
            {
                return IsNull ? null : new Coordinate((MinX + MaxX) / 2.0, (MinY + MaxY) / 2.0);
            }
        }

        /// <summary>
        /// Computes the intersection of two <see cref="Envelope"/>s.
        /// </summary>
        /// <param name="env">The envelope to intersect with</param>
        /// <returns>
        /// A new Envelope representing the intersection of the envelopes (this will be
        /// the null envelope if either argument is null, or they do not intersect
        /// </returns>
        public Envelope Intersection(Envelope env)
        {
            if (IsNull || env.IsNull || !Intersects(env))
                return new Envelope();

            return new Envelope(Math.Max(MinX, env.MinX),
                                Math.Min(MaxX, env.MaxX),
                                Math.Max(MinY, env.MinY),
                                Math.Min(MaxY, env.MaxY));
        }

        /// <summary>
        /// Check if the region defined by <c>other</c>
        /// intersects the region of this <c>Envelope</c>.
        /// <para/>
        /// A null envelope never intersects.
        /// </summary>
        /// <param name="other">The <c>Envelope</c> which this <c>Envelope</c> is
        /// being checked for intersecting.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <c>Envelope</c>s intersect.
        /// </returns>
        public bool Intersects(Envelope other)
        {
            if (IsNull || other.IsNull)
                return false;
            return !(other.MinX > _maxX || other.MaxX < _minX || other.MinY > _maxY || other.MaxY < _minY);
        }

        /// <summary>
        /// Check if the point <c>p</c> overlaps (lies inside) the region of this <c>Envelope</c>.
        /// </summary>
        /// <param name="p"> the <c>Coordinate</c> to be tested.</param>
        /// <returns><c>true</c> if the point overlaps this <c>Envelope</c>.</returns>
        public bool Intersects(Coordinate p)
        {
            return Intersects(p.X, p.Y);
        }

        /// <summary>
        /// Check if the point <c>(x, y)</c> overlaps (lies inside) the region of this <c>Envelope</c>.
        /// </summary>
        /// <param name="x"> the x-ordinate of the point.</param>
        /// <param name="y"> the y-ordinate of the point.</param>
        /// <returns><c>true</c> if the point overlaps this <c>Envelope</c>.</returns>
        public bool Intersects(double x, double y)
        {
            return !(x > _maxX || x < _minX || y > _maxY || y < _minY);
        }

        /// <summary>
        /// Tests if the extent defined by two extremal points
        /// intersects the extent of this <c>Envelope</c>.
        /// </summary>
        /// <param name="a">A point</param>
        /// <param name="b">Another point</param>
        /// <returns><c>true</c> if the extents intersect</returns>
        public bool Intersects(Coordinate a, Coordinate b)
        {
            if (IsNull) return false;

            double envMinX = (a.X < b.X) ? a.X : b.X;
            if (envMinX > _maxX) return false;

            double envMaxX = (a.X > b.X) ? a.X : b.X;
            if (envMaxX < _minX) return false;

            double envMinY = (a.Y < b.Y) ? a.Y : b.Y;
            if (envMinY > _maxY) return false;

            double envMaxY = (a.Y > b.Y) ? a.Y : b.Y;
            if (envMaxY < _minY) return false;

            return true;
        }

        /// <summary>
        /// Tests if the region defined by <c>other</c>
        /// is disjoint from the region of this <c>Envelope</c>.
        /// </summary>
        /// <param name="other">The <c>Envelope</c> being checked for disjointness</param>
        /// <returns><c>true</c> if the <c>Envelope</c>s are disjoint</returns>
        /// <seealso cref="Intersects(NetTopologySuite.Geometries.Envelope)"/>
        public bool Disjoint(Envelope other)
        {
            return !Intersects(other);
        }

        ///<summary>
        /// Tests if the <c>Envelope other</c> lies wholely inside this <c>Envelope</c> (inclusive of the boundary).
        ///</summary>
        /// <remarks>
        /// Note that this is <b>not</b> the same definition as the SFS <i>contains</i>,
        /// which would exclude the envelope boundary.
        /// </remarks>
        /// <para>The <c>Envelope</c> to check</para>
        /// <returns>true if <c>other</c> is contained in this <c>Envelope</c></returns>
        /// <see cref="Covers(Envelope)"/>
        public bool Contains(Envelope other)
        {
            return Covers(other);
        }

        ///<summary>
        /// Tests if the given point lies in or on the envelope.
        ///</summary>
        /// <remarks>
        /// Note that this is <b>not</b> the same definition as the SFS <i>contains</i>,
        /// which would exclude the envelope boundary.
        /// </remarks>
        /// <param name="p">the point which this <c>Envelope</c> is being checked for containing</param>
        /// <returns><c>true</c> if the point lies in the interior or on the boundary of this <c>Envelope</c>. </returns>
        /// <see cref="Covers(Coordinate)"/>
        public bool Contains(Coordinate p)
        {
            return Covers(p);
        }

        ///<summary>
        /// Tests if the given point lies in or on the envelope.
        ///</summary>
        /// <remarks>
        /// Note that this is <b>not</b> the same definition as the SFS <i>contains</i>, which would exclude the envelope boundary.
        /// </remarks>
        /// <param name="x">the x-coordinate of the point which this <c>Envelope</c> is being checked for containing</param>
        /// <param name="y">the y-coordinate of the point which this <c>Envelope</c> is being checked for containing</param>
        /// <returns>
        /// <c>true</c> if <c>(x, y)</c> lies in the interior or on the boundary of this <c>Envelope</c>.
        /// </returns>
        /// <see cref="Covers(double, double)"/>
        public bool Contains(double x, double y)
        {
            return Covers(x, y);
        }

        /// <summary>
        /// Tests if an envelope is properly contained in this one.
        /// The envelope is properly contained if it is contained
        /// by this one but not equal to it.
        /// </summary>
        /// <param name="other">The envelope to test</param>
        /// <returns><c>true</c> if the envelope is properly contained</returns>
        public bool ContainsProperly(Envelope other)
        {
            if (Equals(other))
                return false;
            return Covers(other);
        }

        ///<summary>
        /// Tests if the given point lies in or on the envelope.
        ///</summary>
        /// <param name="x">the x-coordinate of the point which this <c>Envelope</c> is being checked for containing</param>
        /// <param name="y">the y-coordinate of the point which this <c>Envelope</c> is being checked for containing</param>
        /// <returns> <c>true</c> if <c>(x, y)</c> lies in the interior or on the boundary of this <c>Envelope</c>.</returns>
        public bool Covers(double x, double y)
        {
            if (IsNull) return false;
            return x >= _minX &&
                x <= _maxX &&
                y >= _minY &&
                y <= _maxY;
        }

        ///<summary>
        /// Tests if the given point lies in or on the envelope.
        ///</summary>
        /// <param name="p">the point which this <c>Envelope</c> is being checked for containing</param>
        /// <returns><c>true</c> if the point lies in the interior or on the boundary of this <c>Envelope</c>.</returns>
        public bool Covers(Coordinate p)
        {
            return Covers(p.X, p.Y);
        }

        ///<summary>
        /// Tests if the <c>Envelope other</c> lies wholely inside this <c>Envelope</c> (inclusive of the boundary).
        ///</summary>
        /// <param name="other">the <c>Envelope</c> to check</param>
        /// <returns>true if this <c>Envelope</c> covers the <c>other</c></returns>
        public bool Covers(Envelope other)
        {
            if (IsNull || other.IsNull)
                return false;
            return other.MinX >= _minX &&
                other.MaxX <= _maxX &&
                other.MinY >= _minY &&
                other.MaxY <= _maxY;
        }

        /// <summary>
        /// Computes the distance between this and another
        /// <c>Envelope</c>.
        /// The distance between overlapping Envelopes is 0.  Otherwise, the
        /// distance is the Euclidean distance between the closest points.
        /// </summary>
        /// <returns>The distance between this and another <c>Envelope</c>.</returns>
        public double Distance(Envelope env)
        {
            if (Intersects(env))
                return 0;

            double dx = 0.0;

            if (_maxX < env.MinX)
                dx = env.MinX - _maxX;
            else if (_minX > env.MaxX)
                dx = _minX - env.MaxX;

            double dy = 0.0;

            if (_maxY < env.MinY)
                dy = env.MinY - _maxY;
            else if (_minY > env.MaxY)
                dy = _minY - env.MaxY;

            // if either is zero, the envelopes overlap either vertically or horizontally
            if (dx == 0.0)
                return dy;
            if (dy == 0.0)
                return dx;

            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <inheritdoc />
        public override bool Equals(object o)
        {
            if (o is Envelope other)
                return Equals(other);
            return false;
        }

        /// <inheritdoc cref="M:System.IEquatable{Envelope}.Equals(Envelope)"/>
        public bool Equals(Envelope other)
        {
            if (IsNull)
                return other.IsNull;

            return _maxX == other.MaxX && _maxY == other.MaxY &&
                   _minX == other.MinX && _minY == other.MinY;
        }

        /// <summary>
        /// Compares two envelopes using lexicographic ordering.
        /// The ordering comparison is based on the usual numerical
        /// comparison between the sequence of ordinates.
        /// Null envelopes are less than all non-null envelopes.
        /// </summary>
        /// <param name="o">An envelope</param>
        public int CompareTo(object o)
        {
            if (o is Envelope other)
                return CompareTo(other);
            return 1;
        }

        /// <summary>
        /// Compares two envelopes using lexicographic ordering.
        /// The ordering comparison is based on the usual numerical
        /// comparison between the sequence of ordinates.
        /// Null envelopes are less than all non-null envelopes.
        /// </summary>
        /// <param name="env">An envelope</param>
        public int CompareTo(Envelope env)
        {
            env = env ?? new Envelope();

            // compare nulls if present
            if (IsNull)
            {
                if (env.IsNull) return 0;
                return -1;
            }
            else
            {
                if (env.IsNull) return 1;
            }

            // compare based on numerical ordering of ordinates
            if (MinX < env.MinX) return -1;
            if (MinX > env.MinX) return 1;
            if (MinY < env.MinY) return -1;
            if (MinY > env.MinY) return 1;
            if (MaxX < env.MaxX) return -1;
            if (MaxX > env.MaxX) return 1;
            if (MaxY < env.MaxY) return -1;
            if (MaxY > env.MaxY) return 1;
            return 0;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int result = 17;
            // ReSharper disable NonReadonlyFieldInGetHashCode
            result = 37 * result + _minX.GetHashCode();
            result = 37 * result + _maxX.GetHashCode();
            result = 37 * result + _minY.GetHashCode();
            result = 37 * result + _maxY.GetHashCode();
            // ReSharper restore NonReadonlyFieldInGetHashCode
            return result;
        }

        /// <summary>
        /// Function to get a textual representation of this envelope
        /// </summary>
        /// <returns>A textual representation of this envelope</returns>
        public override string ToString()
        {
            var sb = new StringBuilder("Env[");
            if (IsNull)
            {
                sb.Append("Null]");
            }
            else
            {
                sb.AppendFormat(NumberFormatInfo.InvariantInfo, "{0:R} : {1:R}, ", _minX, _maxX);
                sb.AppendFormat(NumberFormatInfo.InvariantInfo, "{0:R} : {1:R}]", _minY, _maxY);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Creates a deep copy of the current envelope.
        /// </summary>
        /// <returns></returns>
        public Envelope Copy()
        {
            if (IsNull)
            {
                // #179: This will create a new 'NULL' envelope
                return new Envelope();
            }
            return new Envelope(_minX, _maxX, _minY, _maxY);
        }

        /// <summary>
        /// Method to parse an envelope from its <see cref="ToString"/> value
        /// </summary>
        /// <param name="envelope">The envelope string</param>
        /// <returns>The envelope</returns>
        public static Envelope Parse(string envelope)
        {
            if (string.IsNullOrEmpty(envelope))
                throw new ArgumentNullException(nameof(envelope));
            if (!(envelope.StartsWith("Env[") && envelope.EndsWith("]")))
                throw new ArgumentException("Not a valid envelope string", nameof(envelope));

            // test for null
            envelope = envelope.Substring(4, envelope.Length - 5);
            if (envelope == "Null")
                return new Envelope();

            // Parse values
            double[] ordinatesValues = new double[4];
            string[] ordinateLabel = new[] { "x", "y" };
            int j = 0;

            // split into ranges
            string[] parts = envelope.Split(',');
            if (parts.Length != 2)
                throw new ArgumentException("Does not provide two ranges", nameof(envelope));

            foreach (string part in parts)
            {
                // Split int min/max
                string[] ordinates = part.Split(':');
                if (ordinates.Length != 2)
                    throw new ArgumentException("Does not provide just min and max values", nameof(envelope));

                if (!double.TryParse(ordinates[0].Trim(), NumberStyles.Number, NumberFormatInfo.InvariantInfo, out ordinatesValues[2 * j]))
                    throw new ArgumentException($"Could not parse min {ordinateLabel[j]}-Ordinate", nameof(envelope));
                if (!double.TryParse(ordinates[1].Trim(), NumberStyles.Number, NumberFormatInfo.InvariantInfo, out ordinatesValues[2 * j + 1]))
                    throw new ArgumentException($"Could not parse max {ordinateLabel[j]}-Ordinate", nameof(envelope));
                j++;
            }

            return new Envelope(ordinatesValues[0], ordinatesValues[1],
                                ordinatesValues[2], ordinatesValues[3]);
        }
    }
}
