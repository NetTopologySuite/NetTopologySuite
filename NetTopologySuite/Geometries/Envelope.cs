using System;
using System.Runtime.InteropServices;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Defines a rectangular region of the 2D coordinate plane.
    /// It is often used to represent the bounding box of a <c>Geometry</c>,
    /// e.g. the minimum and maximum x and y values of the <c>Coordinate</c>s.
    /// Note that Envelopes support infinite or half-infinite regions, by using the values of
    /// <c>Double.PositiveInfinity</c> and <c>Double.NegativeInfinity</c>.
    /// When Envelope objects are created or initialized,
    /// the supplies extent values are automatically sorted into the correct order.    
    /// </summary>
    [Serializable]
    public class Envelope : ICloneable
    {
        /// <summary>
        /// Return HashCode.
        /// </summary>
        public override int GetHashCode()
        {
            int result = 17;            
            result = 37 * result + Coordinate.GetHashCode(minx);
            result = 37 * result + Coordinate.GetHashCode(maxx);
            result = 37 * result + Coordinate.GetHashCode(miny);
            result = 37 * result + Coordinate.GetHashCode(maxy);
            return result;
        }

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
            if  (((q.X >= (p1.X < p2.X ? p1.X : p2.X))  && (q.X <= (p1.X > p2.X ? p1.X : p2.X))) &&
                 ((q.Y >= (p1.Y < p2.Y ? p1.Y : p2.Y))  && (q.Y <= (p1.Y > p2.Y ? p1.Y : p2.Y))))            
                return true;                        
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
        /// <returns><c>true</c> if Q intersects Point</returns>
        public static bool Intersects(Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2)
        {
            double minq = Math.Min(q1.X, q2.X);
            double maxq = Math.Max(q1.X, q2.X);
            double minp = Math.Min(p1.X, p2.X);
            double maxp = Math.Max(p1.X, p2.X);
            if(minp > maxq) return false;
            if(maxp < minq) return false;

            minq = Math.Min(q1.Y, q2.Y);
            maxq = Math.Max(q1.Y, q2.Y);
            minp = Math.Min(p1.Y, p2.Y);
            maxp = Math.Max(p1.Y, p2.Y);
            if( minp > maxq ) return false;
            if( maxp < minq ) return false;

            return true;
        }

        /*
        *  the minimum x-coordinate
        */
        private double minx;

        /*
        *  the maximum x-coordinate
        */
        private double maxx;

        /*
        * the minimum y-coordinate
        */
        private double miny;

        /*
        *  the maximum y-coordinate
        */
        private double maxy;

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
            Init(p1, p2);
        }

        /// <summary>
        /// Creates an <c>Envelope</c> for a region defined by a single Coordinate.
        /// </summary>
        /// <param name="p">The Coordinate.</param>
        public Envelope(Coordinate p)
        {
            Init(p);
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
        public virtual void Init()
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
        public virtual void Init(double x1, double x2, double y1, double y2)
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
        /// Initialize an <c>Envelope</c> for a region defined by two Coordinates.
        /// </summary>
        /// <param name="p1">The first Coordinate.</param>
        /// <param name="p2">The second Coordinate.</param>
        public virtual void Init(Coordinate p1, Coordinate p2)
        {
            Init(p1.X, p2.X, p1.Y, p2.Y);
        }

        /// <summary>
        /// Initialize an <c>Envelope</c> for a region defined by a single Coordinate.
        /// </summary>
        /// <param name="p">The Coordinate.</param>
        public virtual void Init(Coordinate p)
        {
            Init(p.X, p.X, p.Y, p.Y);
        }

        /// <summary>
        /// Initialize an <c>Envelope</c> from an existing Envelope.
        /// </summary>
        /// <param name="env">The Envelope to initialize from.</param>
        public virtual void Init(Envelope env)
        {
            this.minx = env.minx;
            this.maxx = env.maxx;
            this.miny = env.miny;
            this.maxy = env.maxy;
        }

        /// <summary>
        /// Makes this <c>Envelope</c> a "null" envelope..
        /// </summary>
        public virtual void SetToNull()
        {
            minx = 0;
            maxx = -1;
            miny = 0;
            maxy = -1;
        }

        /// <summary>
        /// Returns <c>true</c> if this <c>Envelope</c> is a "null" envelope.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this <c>Envelope</c> is uninitialized
        /// or is the envelope of the empty point.
        /// </returns>
        public virtual bool IsNull
        {
            get
            {
                return maxx < minx;
            }
        }

        /// <summary>
        /// Returns the difference between the maximum and minimum x values.
        /// </summary>
        /// <returns>max x - min x, or 0 if this is a null <c>Envelope</c>.</returns>
        public virtual double Width
        {
            get
            {
                if (IsNull)
                    return 0;                
                return maxx - minx;
            }
        }

        /// <summary>
        /// Returns the difference between the maximum and minimum y values.
        /// </summary>
        /// <returns>max y - min y, or 0 if this is a null <c>Envelope</c>.</returns>
        public virtual double Height
        {
            get
            {
                if (IsNull)
                    return 0;
                return maxy - miny;
            }
        }

        /// <summary>
        /// Returns the <c>Envelope</c>s minimum x-value. min x > max x
        /// indicates that this is a null <c>Envelope</c>.
        /// </summary>
        /// <returns>The minimum x-coordinate.</returns>
        public virtual double MinX
        {
            get
            {
                return minx;
            }
        }

        /// <summary>
        /// Returns the <c>Envelope</c>s maximum x-value. min x > max x
        /// indicates that this is a null <c>Envelope</c>.
        /// </summary>
        /// <returns>The maximum x-coordinate.</returns>
        public virtual double MaxX
        {
            get
            {
                return maxx;
            }
        }

        /// <summary>
        /// Returns the <c>Envelope</c>s minimum y-value. min y > max y
        /// indicates that this is a null <c>Envelope</c>.
        /// </summary>
        /// <returns>The minimum y-coordinate.</returns>
        public virtual double MinY
        {
            get
            {
                return miny;
            }
        }

        /// <summary>
        /// Returns the <c>Envelope</c>s maximum y-value. min y > max y
        /// indicates that this is a null <c>Envelope</c>.
        /// </summary>
        /// <returns>The maximum y-coordinate.</returns>
        public virtual double MaxY
        {
            get
            {
                return maxy;
            }
        }

        /// <summary>
        /// Expands this envelope by a given distance in all directions.
        /// Both positive and negative distances are supported.
        /// </summary>
        /// <param name="distance">The distance to expand the envelope.</param>
        public void expandBy(double distance)
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

            minx -= deltaX;
            maxx += deltaX;
            miny -= deltaY;
            maxy += deltaY;

            // check for envelope disappearing
            if (minx > maxx || miny > maxy)
                SetToNull();
        }

        /// <summary>
        /// Enlarges the boundary of the <c>Envelope</c> so that it contains (p).
        /// Does nothing if (p) is already on or within the boundaries.
        /// </summary>
        /// <param name="p">The Coordinate.</param>
        public virtual void ExpandToInclude(Coordinate p)
        {
            ExpandToInclude(p.X, p.Y);
        }

        /// <summary>
        /// Enlarges the boundary of the <c>Envelope</c> so that it contains
        /// (x,y). Does nothing if (x,y) is already on or within the boundaries.
        /// </summary>
        /// <param name="x">The value to lower the minimum x to or to raise the maximum x to.</param>
        /// <param name="y">The value to lower the minimum y to or to raise the maximum y to.</param>
        public virtual void ExpandToInclude(double x, double y)
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
                if (x < minx) minx = x;                
                if (x > maxx) maxx = x;
                if (y < miny) miny = y;
                if (y > maxy) maxy = y;
            }
        }

        /// <summary>
        /// Enlarges the boundary of the <c>Envelope</c> so that it contains
        /// <c>other</c>. Does nothing if <c>other</c> is wholly on or
        /// within the boundaries.
        /// </summary>
        /// <param name="other">the <c>Envelope</c> to merge with.</param>        
        public virtual void ExpandToInclude(Envelope other)
        {
            if (other.IsNull)
                return;            
            if (IsNull)
            {
                minx = other.MinX;
                maxx = other.MaxX;
                miny = other.MinY;
                maxy = other.MaxY;
            }
            else
            {
                if (other.minx < minx)
                    minx = other.minx;                
                if (other.maxx > maxx)
                    maxx = other.maxx;
                if (other.miny < miny)
                    miny = other.miny;
                if (other.maxy > maxy)
                    maxy = other.maxy;
            }
        }

        /// <summary>
        /// Translates this envelope by given amounts in the X and Y direction.
        /// </summary>
        /// <param name="transX">The amount to translate along the X axis.</param>
        /// <param name="transY">The amount to translate along the Y axis.</param>
        public virtual void Translate(double transX, double transY)
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
        public virtual Coordinate Centre
        {
            get
            {
                if (IsNull) return null;
                return new Coordinate((MinX + MaxX) / 2.0, (MinY + MaxY) / 2.0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        public Envelope Intersection(Envelope env)
        {
            if (IsNull || env.IsNull || !Intersects(env)) 
                return new Envelope();

            return new Envelope( Math.Max(MinX, env.MinX) ,
                                 Math.Min(MaxX, env.MaxX) ,
                                 Math.Max(MinY, env.MinY) ,
                                 Math.Min(MaxY, env.MaxY) );
        }        

        /// <summary> 
        /// Check if the region defined by <c>other</c>
        /// overlaps (intersects) the region of this <c>Envelope</c>.
        /// </summary>
        /// <param name="other"> the <c>Envelope</c> which this <c>Envelope</c> is
        /// being checked for overlapping.
        /// </param>
        /// <returns>        
        /// <c>true</c> if the <c>Envelope</c>s overlap.
        /// </returns>
        public virtual bool Intersects(Envelope other)
        {
            if (IsNull || other.IsNull)
                return false;            
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
        public virtual bool Overlaps(Envelope other)
        {
            return Intersects(other);
        }        

        /// <summary>
        /// Use Intersects instead.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        [Obsolete("Use Intersects instead")]
        public virtual bool Overlaps(Coordinate p)
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
        public virtual bool Overlaps(double x, double y)
        {
            return Intersects(x, y);
        }

        /// <summary>  
        /// Check if the point <c>p</c> overlaps (lies inside) the region of this <c>Envelope</c>.
        /// </summary>
        /// <param name="p"> the <c>Coordinate</c> to be tested.</param>
        /// <returns><c>true</c> if the point overlaps this <c>Envelope</c>.</returns>
        public virtual bool Intersects(Coordinate p)
        {
            return Intersects(p.X, p.Y);
        }

        /// <summary>  
        /// Check if the point <c>(x, y)</c> overlaps (lies inside) the region of this <c>Envelope</c>.
        /// </summary>
        /// <param name="x"> the x-ordinate of the point.</param>
        /// <param name="y"> the y-ordinate of the point.</param>
        /// <returns><c>true</c> if the point overlaps this <c>Envelope</c>.</returns>
        public virtual bool Intersects(double x, double y)
        {
            return !(x > maxx || x < minx || y > maxy || y < miny);
        }

        /// <summary>  
        /// Returns <c>true</c> if the given point lies in or on the envelope.
        /// </summary>
        /// <param name="p"> the point which this <c>Envelope</c> is
        /// being checked for containing.</param>
        /// <returns>    
        /// <c>true</c> if the point lies in the interior or
        /// on the boundary of this <c>Envelope</c>.
        /// </returns>                
        public virtual bool Contains(Coordinate p)
        {
            return Contains(p.X, p.Y);
        }

        /// <summary>  
        /// Returns <c>true</c> if the given point lies in or on the envelope.
        /// </summary>
        /// <param name="x"> the x-coordinate of the point which this <c>Envelope</c> is
        /// being checked for containing.</param>
        /// <param name="y"> the y-coordinate of the point which this <c>Envelope</c> is
        /// being checked for containing.</param>
        /// <returns><c>true</c> if <c>(x, y)</c> lies in the interior or
        /// on the boundary of this <c>Envelope</c>.</returns>
        public virtual bool Contains(double x, double y)
        {
            return x >= minx && x <= maxx && y >= miny && y <= maxy;
        }

        /// <summary>  
        /// Returns <c>true</c> if the <c>Envelope other</c>
        /// lies wholely inside this <c>Envelope</c> (inclusive of the boundary).
        /// </summary>
        /// <param name="other"> the <c>Envelope</c> which this <c>Envelope</c> is being checked for containing.</param>
        /// <returns><c>true</c> if <c>other</c> is contained in this <c>Envelope</c>.</returns>
        public virtual bool Contains(Envelope other)
        {
            if (IsNull || other.IsNull)
                return false;            
            return  other.MinX >= minx && other.MaxX <= maxx && 
                other.MinY >= miny && other.MaxY <= maxy;
        }

        /// <summary> 
        /// Computes the distance between this and another
        /// <c>Envelope</c>.
        /// The distance between overlapping Envelopes is 0.  Otherwise, the
        /// distance is the Euclidean distance between the closest points.
        /// </summary>
        /// <returns>The distance between this and another <c>Envelope</c>.</returns>
        public virtual double Distance(Envelope env)
        {
            if (Intersects(env))
                return 0;

            double dx = 0.0;

            if (maxx < env.minx)
                dx = env.minx - maxx;
            if (minx > env.maxx)
                dx = minx - env.maxx;

            double dy = 0.0;

            if (maxy < env.miny)
                dy = env.miny - maxy;
            if (miny > env.maxy)
                dy = miny - env.maxy;

            // if either is zero, the envelopes overlap either vertically or horizontally
            if (dx == 0.0) return dy;
            if (dy == 0.0) return dx;

            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(object other)
        {
            if (other == null)
                return false;

            if (!(other is Envelope)) 
                return false;            

            Envelope otherEnvelope = (Envelope)other;
            if (IsNull) 
                return otherEnvelope.IsNull;

            return  maxx == otherEnvelope.MaxX && maxy == otherEnvelope.MaxY &&
                    minx == otherEnvelope.MinX && miny == otherEnvelope.MinY;
        }
        
        /// <summary>
        /// See <see cref="Equals"/>
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator ==(Envelope obj1, Envelope obj2)
        {
            return Object.Equals(obj1, obj2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator !=(Envelope obj1, Envelope obj2)
        {
            return !(obj1 == obj2);
        }     

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Env[" + minx + " : " + maxx + ", " + miny + " : " + maxy + "]";
        }

        #region ICloneable Members

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion

        /* BEGIN ADDED BY MPAUL42: monoGIS team */
        
        /// <summary>
        /// Returns the area of the envelope.
        /// </summary>
        public virtual double Area
        {
            get
            {
                double area = 1;
                area = area * (maxx - minx);
                area = area * (maxy - miny);
                return area;
            }
        }
  
        /// <summary>
        /// Creates a deep copy of the current envelope.
        /// </summary>
        /// <returns></returns>
        public virtual Envelope Clone()
        {
            return new Envelope(minx, maxx, miny, maxy);
        }

        /// <summary>
        /// Calculates the union of the current box and the given point.
        /// </summary>
        public virtual Envelope Union(Point point)
        {
            return Union(point.Coordinate);
        }

        /// <summary>
        /// Calculates the union of the current box and the given coordinate.
        /// </summary>
        public virtual Envelope Union(Coordinate coord)
        {
            Envelope env = this.Clone();
            env.ExpandToInclude(coord);
            return env;
        }

        /// <summary>
        /// Calculates the union of the current box and the given box.
        /// </summary>
        public virtual Envelope Union(Envelope box)
        {
            if (box.IsNull)
                return this;
            if (this.IsNull)
                return box;

            return new Envelope( Math.Min(minx, box.MinX) ,
                                 Math.Max(maxx, box.MaxX) ,
                                 Math.Min(miny, box.MinY) ,
                                 Math.Max(maxy, box.MaxY) );
        }

        /// <summary>
        /// Moves the envelope to the indicated coordinate.
        /// </summary>
        /// <param name="centre">The new centre coordinate.</param>
        public virtual void SetCentre(Coordinate centre)
        {
            SetCentre(centre, Width, Height);
        }

        /// <summary>
        /// Moves the envelope to the indicated point.
        /// </summary>
        /// <param name="centre">The new centre point.</param>
        public virtual void SetCentre(Point centre)
        {
            SetCentre(centre.Coordinate, Width, Height);
        }

        /// <summary>
        /// Resizes the envelope to the indicated point.
        /// </summary>
        /// <param name="width">The new width.</param>
        /// <param name="height">The new height.</param>
        public virtual void SetCentre(double width, double height)
        {
            SetCentre(Centre, width, height);
        }

        /// <summary>
        /// Moves and resizes the current envelope.
        /// </summary>
        /// <param name="centre">The new centre point.</param>
        /// <param name="width">The new width.</param>
        /// <param name="height">The new height.</param>
        public virtual void SetCentre(Point centre, double width, double height)
        {
            SetCentre(centre.Coordinate, width, height);
        }

        /// <summary>
        /// Moves and resizes the current envelope.
        /// </summary>
        /// <param name="centre">The new centre coordinate.</param>
        /// <param name="width">The new width.</param>
        /// <param name="height">The new height.</param>
        public virtual void SetCentre(Coordinate centre, double width, double height)
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
        public virtual void Zoom(double perCent)
        {
            double w = (this.Width * perCent / 100);
            double h = (this.Height * perCent / 100);
            SetCentre(w, h);
        }
        
        /* END ADDED BY MPAUL42: monoGIS team */

    }
}
