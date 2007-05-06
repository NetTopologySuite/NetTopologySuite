using System;
using System.Collections.Generic;
using System.Text;

namespace SharpMap.Geometries.LightStructs
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class Envelope : ICloneable, IEquatable<Envelope>
    {        
        private double minx;
        private double maxx;

        private double miny;
        private double maxy;

        /// <summary>
        /// Creates a <c>null</c> <see cref="Envelope">Envelope</see>.
        /// </summary>
        public Envelope()
        {
            Init();
        }

        /// <summary>
        /// Creates an <see cref="Envelope">Envelope</see> for a region defined by maximum and minimum values.
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
        /// Creates an <see cref="Envelope">Envelope</see> for a region 
        /// defined by two <see cref="Coordinate">Coordinates</see>.
        /// </summary>
        /// <param name="p1">The first coordinate.</param>
        /// <param name="p2">The second coordinate.</param>
        public Envelope(Coordinate p1, Coordinate p2)
        {
            Init(p1, p2);
        }

        /// <summary>
        /// Creates an <see cref="Envelope">Envelope</see> for a region 
        /// defined by a single <see cref="Coordinate">Coordinate</see>.
        /// </summary>
        /// <param name="p">The coordinate.</param>
        public Envelope(Coordinate p)
        {
            Init(p);
        }

        /// <summary>
        /// Create an <see cref="Envelope">Envelope</see> from an existing Envelope.
        /// </summary>
        /// <param name="env">The envelope to initialize from.</param>
        public Envelope(Envelope env)
        {
            Init(env);
        }

        /// <summary>
        /// Initialize to a <c>null</c> <see cref="Envelope">Envelope</see>.
        /// </summary>
        public virtual void Init()
        {
            SetToNull();
        }

        /// <summary>
        /// Initialize an <see cref="Envelope">Envelope</see> for a region defined by maximum and minimum values.
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
        /// Initialize an <see cref="Envelope">Envelope</see> for a region defined by two Coordinates.
        /// </summary>
        /// <param name="p1">The first coordinate.</param>
        /// <param name="p2">The second coordinate.</param>
        public virtual void Init(Coordinate p1, Coordinate p2)
        {
            Init(p1.X, p2.X, p1.Y, p2.Y);
        }

        /// <summary>
        /// Initialize an <see cref="Envelope">Envelope</see> for a region defined by a single Coordinate.
        /// </summary>
        /// <param name="p">The coordinate.</param>
        public virtual void Init(Coordinate p)
        {
            Init(p.X, p.X, p.Y, p.Y);
        }

        /// <summary>
        /// Initialize an <see cref="Envelope">Envelope</see> from an existing <see cref="Envelope">Envelope</see>.
        /// </summary>
        /// <param name="env">The envelope to initialize from.</param>
        public virtual void Init(Envelope env)
        {
            this.minx = env.minx;
            this.maxx = env.maxx;
            this.miny = env.miny;
            this.maxy = env.maxy;
        }

        /// <summary>
        /// Makes this <see cref="Envelope">Envelope</see> a "null" envelope.
        /// </summary>
        public virtual void SetToNull()
        {
            minx = 0;
            maxx = -1;
            miny = 0;
            maxy = -1;
        }

        /// <summary>
        /// Returns <c>true</c> if this <see cref="Envelope">Envelope</see> is a "null" envelope.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this <see cref="Envelope">Envelope</see> is uninitialized
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
        /// Returns the <see cref="Envelope">Envelope</see>s minimum x-value. min x > max x
        /// indicates that this is a null <see cref="Envelope">Envelope</see>.
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
        /// Returns the <see cref="Envelope">Envelope</see>s maximum x-value. min x > max x
        /// indicates that this is a null <see cref="Envelope">Envelope</see>.
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
        /// Returns the <see cref="Envelope">Envelope</see>s minimum y-value. min y > max y
        /// indicates that this is a null <see cref="Envelope">Envelope</see>.
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
        /// Returns the <see cref="Envelope">Envelope</see>s maximum y-value. min y > max y
        /// indicates that this is a null <see cref="Envelope">Envelope</see>.
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
        /// Enlarges the boundary of the <see cref="Envelope">Envelope</see> so that it contains (p).
        /// Does nothing if (p) is already on or within the boundaries.
        /// </summary>
        /// <param name="p">The coordinate.</param>
        public virtual void ExpandToInclude(Coordinate p)
        {
            ExpandToInclude(p.X, p.Y);
        }

        /// <summary>
        /// Enlarges the boundary of the <see cref="Envelope">Envelope</see> so that it contains
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
        /// Enlarges the boundary of the <see cref="Envelope">Envelope</see> so that it contains
        /// <c>other</c>. Does nothing if <c>other</c> is wholly on or
        /// within the boundaries.
        /// </summary>
        /// <param name="other">the <see cref="Envelope">Envelope</see> to merge with.</param>        
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
                if (IsNull) return new Coordinate();
                return new Coordinate((MinX + MaxX) / 2.0, (MinY + MaxY) / 2.0);
            }
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
            return Equals((Envelope) other);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Envelope other)
        {
            if (IsNull)
                return other.IsNull;

            return maxx == other.MaxX && maxy == other.MaxY &&
                   minx == other.MinX && miny == other.MinY;
        }

        /// <summary>
        /// 
        /// </summary>
        public override int GetHashCode()
        {
            int result = 17;
            result = 37 * result + GetHashCode(minx);
            result = 37 * result + GetHashCode(maxx);
            result = 37 * result + GetHashCode(miny);
            result = 37 * result + GetHashCode(maxy);
            return result;
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
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        object ICloneable.Clone()
        {
            return Clone();
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
        /// Return HashCode.
        /// </summary>
        /// <param name="x">Value from HashCode computation.</param>
        private static int GetHashCode(double value)
        {
            long f = BitConverter.DoubleToInt64Bits(value);
            return (int) (f ^ (f >> 32));
        }
    }
}
