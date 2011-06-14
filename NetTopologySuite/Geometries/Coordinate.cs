using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// A lightweight class used to store coordinates on the 2-dimensional Cartesian plane.
    /// It is distinct from <see cref="IPoint"/>, which is a subclass of <see cref="IGeometry"/>.
    /// Unlike objects of type <c>Point</c> (which contain additional
    /// information such as an envelope, a precision model, and spatial reference
    /// system information), a <c>Coordinate</c> only contains ordinate values
    /// and propertied.
    /// <c>Coordinate</c>s are two-dimensional points, with an additional Z-ordinate.
    /// NTS does not support any operations on the Z-ordinate except the basic accessor functions. 
    /// If a value is not specified, constructed coordinates will have a
    /// z-ordinate of <c>NaN</c>.  The standard comparison functions will ignore
    /// the z-ordinate.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class Coordinate : ICoordinate
    {
        private double _x; // = Double.NaN;
        private double _y; // = Double.NaN;
        private double _z; // = Double.NaN;

        /// <summary>
        /// X coordinate.
        /// </summary>
        public double X
        {
            get { return _x; }
            set { _x = value; }
        }

        /// <summary>
        /// Y coordinate.
        /// </summary>
        public double Y
        {
            get { return _y; }
            set { _y = value; }
        }

        /// <summary>
        /// Z coordinate.
        /// </summary>
        public double Z
        {
            get { return _z; }
            set { _z = value; }
        }

        /// <summary>
        /// The measure value
        /// </summary>
        public virtual double M
        {
            get { return Double.NaN; }
            set { }
        }

        /// <summary>
        /// Constructs a <c>Coordinate</c> at (x,y,z).
        /// </summary>
        /// <param name="x">X value.</param>
        /// <param name="y">Y value.</param>
        /// <param name="z">Z value.</param>
        public Coordinate(double x, double y, double z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        /// <summary>
        /// Gets/Sets the ordinate value for a given index
        /// </summary>
        /// <param name="index">The index of the ordinate</param>
        /// <returns>The ordinate value</returns>
        public double this[Ordinates index]
        {
            get
            {
                switch (index)
                {
                    case Ordinates.X:
                        return _x;
                    case Ordinates.Y:
                        return _y;
                    case Ordinates.Z:
                        return _z;
                    default:
                        return Double.NaN;
                }
            }
            set
            {
                switch (index)
                {
                    case Ordinates.X:
                        _x = value;
                        break;
                    case Ordinates.Y:
                        _y = value;
                        break;
                    case Ordinates.Z:
                        _z = value;
                        break;
                }
            }
        }

        /// <summary>
        ///  Constructs a <c>Coordinate</c> at (0,0,NaN).
        /// </summary>
        public Coordinate() : this(0.0, 0.0, Double.NaN) { }

        /// <summary>
        /// Constructs a <c>Coordinate</c> having the same (x,y,z) values as
        /// <c>other</c>.
        /// </summary>
        /// <param name="c"><c>Coordinate</c> to copy.</param>
        public Coordinate(ICoordinate c) : this(c.X, c.Y, c.Z) { }

        /// <summary>
        /// Constructs a <c>Coordinate</c> at (x,y,NaN).
        /// </summary>
        /// <param name="x">X value.</param>
        /// <param name="y">Y value.</param>
        public Coordinate(double x, double y) : this(x, y, Double.NaN) { }

        /// <summary>
        /// Gets/Sets <c>Coordinate</c>s (x,y,z) values.
        /// </summary>
        public ICoordinate CoordinateValue
        {
            get { return this; }
            set
            {
                _x = value.X;
                _y = value.Y;
                _z = value.Z;
            }
        }

        /// <summary>
        /// Returns whether the planar projections of the two <c>Coordinate</c>s are equal.
        ///</summary>
        /// <param name="other"><c>Coordinate</c> with which to do the 2D comparison.</param>
        /// <returns>
        /// <c>true</c> if the x- and y-coordinates are equal;
        /// the Z coordinates do not have to be equal.
        /// </returns>
        public bool Equals2D(ICoordinate other)
        {
            return X == other.X && Y == other.Y;
        }

        /// <summary>
        /// Returns <c>true</c> if <c>other</c> has the same values for the x and y ordinates.
        /// Since Coordinates are 2.5D, this routine ignores the z value when making the comparison.
        /// </summary>
        /// <param name="other"><c>Coordinate</c> with which to do the comparison.</param>
        /// <returns><c>true</c> if <c>other</c> is a <c>Coordinate</c> with the same values for the x and y ordinates.</returns>
        public override bool Equals(object other)
        {
            if (other == null)
                return false;
            if (!(other is ICoordinate))
                return false;
            return Equals((ICoordinate)other);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public Boolean Equals(ICoordinate other)
        {
            return Equals2D(other);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator ==(Coordinate obj1, ICoordinate obj2)
        {
            return Equals(obj1, obj2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator !=(Coordinate obj1, ICoordinate obj2)
        {
            return !(obj1 == obj2);
        }

        /// <summary>
        /// Compares this object with the specified object for order.
        /// Since Coordinates are 2.5D, this routine ignores the z value when making the comparison.
        /// Returns
        ///   -1  : this.x lowerthan other.x || ((this.x == other.x) AND (this.y lowerthan other.y))
        ///    0  : this.x == other.x AND this.y = other.y 
        ///    1  : this.x greaterthan other.x || ((this.x == other.x) AND (this.y greaterthan other.y)) 
        /// </summary>
        /// <param name="o"><c>Coordinate</c> with which this <c>Coordinate</c> is being compared.</param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as this <c>Coordinate</c>
        ///         is less than, equal to, or greater than the specified <c>Coordinate</c>.
        /// </returns>
        public int CompareTo(object o)
        {
            var other = (ICoordinate)o;
            return CompareTo(other);
        }

        /// <summary>
        /// Compares this object with the specified object for order.
        /// Since Coordinates are 2.5D, this routine ignores the z value when making the comparison.
        /// Returns
        ///   -1  : this.x lowerthan other.x || ((this.x == other.x) AND (this.y lowerthan other.y))
        ///    0  : this.x == other.x AND this.y = other.y 
        ///    1  : this.x greaterthan other.x || ((this.x == other.x) AND (this.y greaterthan other.y)) 
        /// </summary>
        /// <param name="other"><c>Coordinate</c> with which this <c>Coordinate</c> is being compared.</param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as this <c>Coordinate</c>
        ///         is less than, equal to, or greater than the specified <c>Coordinate</c>.
        /// </returns>
        public int CompareTo(ICoordinate other)
        {
            if (_x < other.X)
                return -1;
            if (_x > other.X)
                return 1;
            if (_y < other.Y)
                return -1;
            if (_y > other.Y)
                return 1;
            return 0;
        }

        /// <summary>
        /// Returns <c>true</c> if <c>other</c> has the same values for x, y and z.
        /// </summary>
        /// <param name="other"><c>Coordinate</c> with which to do the 3D comparison.</param>
        /// <returns><c>true</c> if <c>other</c> is a <c>Coordinate</c> with the same values for x, y and z.</returns>
        public bool Equals3D(ICoordinate other)
        {
            return (_x == other.X) && (_y == other.Y) &&
                ((_z == other.Z) || (Double.IsNaN(Z) && Double.IsNaN(other.Z)));
        }

        /// <summary>
        /// Returns a <c>string</c> of the form <I>(x,y,z)</I> .
        /// </summary>
        /// <returns><c>string</c> of the form <I>(x,y,z)</I></returns>
        public override string ToString()
        {
            return "(" + _x + ", " + _y + ", " + _z + ")";
        }

        /// <summary>
        /// Create a new object as copy of this instance.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new Coordinate(X, Y, Z);
        }

        /// <summary>
        /// Computes the 2-dimensional Euclidean distance to another location.
        /// The Z-ordinate is ignored.
        /// </summary>
        /// <param name="p"><c>Coordinate</c> with which to do the distance comparison.</param>
        /// <returns>the 2-dimensional Euclidean distance between the locations</returns>
        public double Distance(ICoordinate p)
        {
            var dx = _x - p.X;
            var dy = _y - p.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Gets a hashcode for this coordinate.
        /// </summary>
        /// <returns>A hashcode for this coordinate.</returns>
        public override int GetHashCode()
        {
            var result = 17;
            result = 37 * result + GetHashCode(X);
            result = 37 * result + GetHashCode(Y);
            return result;
        }

        /// <summary>
        /// Computes a hash code for a double value, using the algorithm from
        /// Joshua Bloch's book <i>Effective Java"</i>
        /// </summary>
        /// <param name="value">A hashcode for the double value</param>
        private static int GetHashCode(double value)
        {
            /*
             * From the java language specification, it says:
             * 
             * The value of n>>>s is n right-shifted s bit positions with zero-extension.
             * If n is positive, then the result is the same as that of n>>s; if n is
             * negative, the result is equal to that of the expression (n>>s)+(2<<~s) if
             * the type of the left-hand operand is int
             */
            var f = BitConverter.DoubleToInt64Bits(value);
            //if (f > 0)
                return (int)(f ^ (f >> 32));
            //return (int) (f ^ ((f >> 32) + (2 << ~32)));
            
        }

        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        /// <summary>
        /// Overloaded + operator.
        /// </summary>
        public static Coordinate operator +(Coordinate coord1, Coordinate coord2)
        {
            return new Coordinate(coord1.X + coord2.X, coord1.Y + coord2.Y, coord1.Z + coord2.Z);
        }

        /// <summary>
        /// Overloaded + operator.
        /// </summary>
        public static Coordinate operator +(Coordinate coord1, double d)
        {
            return new Coordinate(coord1.X + d, coord1.Y + d, coord1.Z + d);
        }

        /// <summary>
        /// Overloaded + operator.
        /// </summary>
        public static Coordinate operator +(double d, Coordinate coord1)
        {
            return coord1 + d;
        }

        /// <summary>
        /// Overloaded * operator.
        /// </summary>
        public static Coordinate operator *(Coordinate coord1, Coordinate coord2)
        {
            return new Coordinate(coord1.X * coord2.X, coord1.Y * coord2.Y, coord1.Z * coord2.Z);
        }

        /// <summary>
        /// Overloaded * operator.
        /// </summary>
        public static Coordinate operator *(Coordinate coord1, double d)
        {
            return new Coordinate(coord1.X * d, coord1.Y * d, coord1.Z * d);
        }

        /// <summary>
        /// Overloaded * operator.
        /// </summary>
        public static Coordinate operator *(double d, Coordinate coord1)
        {
            return coord1 * d;
        }

        /// <summary>
        /// Overloaded - operator.
        /// </summary>
        public static Coordinate operator -(Coordinate coord1, Coordinate coord2)
        {
            return new Coordinate(coord1.X - coord2.X, coord1.Y - coord2.Y, coord1.Z - coord2.Z);
        }

        /// <summary>
        /// Overloaded - operator.
        /// </summary>
        public static Coordinate operator -(Coordinate coord1, double d)
        {
            return new Coordinate(coord1.X - d, coord1.Y - d, coord1.Z - d);
        }

        /// <summary>
        /// Overloaded - operator.
        /// </summary>
        public static Coordinate operator -(double d, Coordinate coord1)
        {
            return coord1 - d;
        }

        /// <summary>
        /// Overloaded / operator.
        /// </summary>
        public static Coordinate operator /(Coordinate coord1, Coordinate coord2)
        {
            return new Coordinate(coord1.X / coord2.X, coord1.Y / coord2.Y, coord1.Z / coord2.Z);
        }

        /// <summary>
        /// Overloaded / operator.
        /// </summary>
        public static Coordinate operator /(Coordinate coord1, double d)
        {
            return new Coordinate(coord1.X / d, coord1.Y / d, coord1.Z / d);
        }

        /// <summary>
        /// Overloaded / operator.
        /// </summary>
        public static Coordinate operator /(double d, Coordinate coord1)
        {
            return coord1 / d;
        }

        /* END ADDED BY MPAUL42: monoGIS team */
    }
}
