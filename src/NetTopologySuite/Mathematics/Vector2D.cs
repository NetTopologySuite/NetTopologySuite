using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;
using ICloneable = System.ICloneable;

namespace NetTopologySuite.Mathematics
{
    /// <summary>
    /// A 2-dimensional mathematical vector represented by double-precision X and Y components.
    /// </summary>
    /// <author>mbdavis</author>
    public class Vector2D : ICloneable
    {
        /// <summary>
        /// Creates a new vector with all components set to Zero
        /// </summary>
        public static Vector2D Zero => new Vector2D();

        /// <summary>
        /// Creates a new vector with given X and Y components.
        /// </summary>
        /// <param name="x">The x component</param>
        /// <param name="y">The y component</param>
        /// <returns>A new vector</returns>
        public static Vector2D Create(double x, double y)
        {
            return new Vector2D(x, y);
        }

        /// <summary>
        /// Creates a new vector from an existing one.
        /// </summary>
        /// <param name="v">The vector to copy</param>
        /// <returns>A new vector</returns>
        public static Vector2D Create(Vector2D v)
        {
            return new Vector2D(v);
        }

        /// <summary>
        /// Creates a vector from a <see cref="Coordinate"/>.
        /// </summary>
        /// <param name="coord">The coordinate to copy</param>
        /// <returns>A new vector</returns>
        public static Vector2D Create(Coordinate coord)
        {
            return new Vector2D(coord);
        }

        /// <summary>Creates a vector with the direction and magnitude
        /// of the difference between the
        /// <paramref name="to"/> and <paramref name="from"/> <see cref="Coordinate"/>s.
        /// </summary>
        /// <param name="from">The origin coordinate</param>
        /// <param name="to">The destination coordinate</param>
        /// <returns>A new vector</returns>
        public static Vector2D Create(Coordinate from, Coordinate to)
        {
            return new Vector2D(from, to);
        }

        /// <summary>The X component of this vector</summary>
        private readonly double _x;

        ///<summary>The Y component of this vector.</summary>
        private readonly double _y;

        /// <summary>
        /// Creates an new vector instance
        /// </summary>
        public Vector2D() : this(0.0, 0.0)
        {
        }

        /// <summary>
        /// Creates a new vector instance using the provided <paramref name="x"/> and <paramref name="y"/> ordinates
        /// </summary>
        /// <param name="x">The x-ordinate value</param>
        /// <param name="y">The y-ordinate value</param>
        public Vector2D(double x, double y)
        {
            _x = x;
            _y = y;
        }

        /// <summary>
        /// Creates a new vector instance based on <paramref name="v"/>.
        /// </summary>
        /// <param name="v">The vector</param>
        public Vector2D(Vector2D v)
        {
            _x = v._x;
            _y = v._y;
        }

        /// <summary>Creates a new vector with the direction and magnitude
        /// of the difference between the
        /// <paramref name="to"/> and <paramref name="from"/> <see cref="Coordinate"/>s.
        /// </summary>
        /// <param name="from">The origin coordinate</param>
        /// <param name="to">The destination coordinate</param>
        public Vector2D(Coordinate from, Coordinate to)
        {
            _x = to.X - from.X;
            _y = to.Y - from.Y;
        }

        /// <summary>
        /// Creates a vector from a <see cref="Coordinate"/>.
        /// </summary>
        /// <param name="v">The coordinate</param>
        /// <returns>A new vector</returns>
        public Vector2D(Coordinate v)
        {
            _x = v.X;
            _y = v.Y;
        }

        /// <summary>
        /// Gets the x-ordinate value
        /// </summary>
        public double X => _x;

        /// <summary>
        /// Gets the y-ordinate value
        /// </summary>
        public double Y => _y;
        
        /// <summary>
        /// Gets the ordinate values by index
        /// </summary>
        /// <param name="index">The index</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">Thrown if index &lt; 0 or &gt; 1</exception>
        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _x;
                    case 1:
                        return _y;
                    default:
                        throw new System.ArgumentOutOfRangeException("index");
                }
            }
        }

        /// <summary>
        /// Adds <paramref name="v"/> to this vector instance.
        /// </summary>
        /// <param name="v">The vector to add</param>
        /// <returns>The result vector</returns>
        public Vector2D Add(Vector2D v)
        {
            return Create(_x + v._x, _y + v._y);
        }

        /// <summary>
        /// Subtracts <paramref name="v"/> from this vector instance
        /// </summary>
        /// <param name="v">The vector to subtract</param>
        /// <returns>The result vector</returns>
        public Vector2D Subtract(Vector2D v)
        {
            return Create(_x - v._x, _y - v._y);
        }

        /// <summary>
        /// Multiplies the vector by a scalar value.
        /// </summary>
        /// <param name="d">The value to multiply by</param>
        /// <returns>A new vector with the value v * d</returns>
        public Vector2D Multiply(double d)
        {
            return Create(_x * d, _y * d);
        }

        /// <summary>
        /// Divides the vector by a scalar value.
        /// </summary>
        /// <param name="d">The value to divide by</param>
        /// <returns>A new vector with the value v / d</returns>
        public Vector2D Divide(double d)
        {
            return Create(_x / d, _y / d);
        }

        /// <summary>
        /// Negates this vector
        /// </summary>
        /// <returns>A new vector with [-_x, -_y]</returns>
        public Vector2D Negate()
        {
            return Create(-_x, -_y);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public double Length()
        {
            return System.Math.Sqrt(_x * _x + _y * _y);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public double LengthSquared()
        {
            return _x * _x + _y * _y;
        }

        /// <summary>
        /// Normalizes the vector
        /// </summary>
        /// <returns>A new normalized vector</returns>
        public Vector2D Normalize()
        {
            double length = Length();
            if (length > 0.0)
                return Divide(length);
            return Create(0.0, 0.0);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Vector2D Average(Vector2D v)
        {
            return WeightedSum(v, 0.5);
        }

        /// <summary>
        /// Computes the weighted sum of this vector
        /// with another vector,
        /// with this vector contributing a fraction
        /// of <tt>frac</tt> to the total.
        /// <para/>
        /// In other words,
        /// <pre>
        /// sum = frac * this + (1 - frac) * v
        /// </pre>
        /// </summary>
        /// <param name="v">The vector to sum</param>
        /// <param name="frac">The fraction of the total contributed by this vector</param>
        /// <returns>The weighted sum of the two vectors</returns>
        public Vector2D WeightedSum(Vector2D v, double frac)
        {
            return Create(
                    frac * _x + (1.0 - frac) * v._x,
                    frac * _y + (1.0 - frac) * v._y);
        }

        /// <summary>
        /// Computes the distance between this vector and another one.
        /// </summary>
        /// <param name="v">A vector</param>
        /// <returns>The distance between the vectors</returns>
        public double Distance(Vector2D v)
        {
            double delx = v._x - _x;
            double dely = v._y - _y;
            return System.Math.Sqrt(delx * delx + dely * dely);
        }

        /// <summary>
        /// Computes the dot-product of two vectors
        /// </summary>
        /// <param name="v">A vector</param>
        /// <returns>The dot product of the vectors</returns>
        public double Dot(Vector2D v)
        {
            return _x * v._x + _y * v._y;
        }

        /// <summary>
        /// Computes the angle this vector describes to the horizontal axis
        /// </summary>
        /// <returns>The angle</returns>
        public double Angle()
        {
            return System.Math.Atan2(_y, _x);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public double Angle(Vector2D v)
        {
            return AngleUtility.Diff(v.Angle(), Angle());
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public double AngleTo(Vector2D v)
        {
            double a1 = Angle();
            double a2 = v.Angle();
            double angDel = a2 - a1;

            // normalize, maintaining orientation
            if (angDel <= -System.Math.PI)
                return angDel + AngleUtility.PiTimes2;
            if (angDel > System.Math.PI)
                return angDel - AngleUtility.PiTimes2;
            return angDel;
        }

        /// <summary>
        /// Rotates this vector by <paramref name="angle"/>
        /// </summary>
        /// <param name="angle">The angle</param>
        /// <returns>The rotated vector</returns>
        public Vector2D Rotate(double angle)
        {
            double cos = System.Math.Cos(angle);
            double sin = System.Math.Sin(angle);
            return Create(
                    _x * cos - _y * sin,
                    _x * sin + _y * cos
                    );
        }

        /// <summary>
        /// Rotates a vector by a given number of quarter-circles (i.e. multiples of 90
        /// degrees or Pi/2 radians). A positive number rotates counter-clockwise, a
        /// negative number rotates clockwise. Under this operation the magnitude of
        /// the vector and the absolute values of the ordinates do not change, only
        /// their sign and ordinate index.
        /// </summary>
        /// <param name="numQuarters">The number of quarter-circles to rotate by</param>
        /// <returns>The rotated vector.</returns>
        public Vector2D RotateByQuarterCircle(int numQuarters)
        {
            int nQuad = numQuarters % 4;
            if (numQuarters < 0 && nQuad != 0)
            {
                nQuad = nQuad + 4;
            }
            switch (nQuad)
            {
                case 0:
                    return Create(_x, _y);
                case 1:
                    return Create(-_y, _x);
                case 2:
                    return Create(-_x, -_y);
                case 3:
                    return Create(_y, -_x);
            }
            Assert.ShouldNeverReachHere();
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool IsParallel(Vector2D v)
        {
            return 0 == CGAlgorithmsDD.SignOfDet2x2(_x, _y, v._x, v._y);
        }

        /// <summary>
        /// Gets a <see cref="Coordinate"/> made of this vector translated by <paramref name="coord"/>.
        /// </summary>
        /// <param name="coord">The translation coordinate</param>
        /// <returns>A coordinate</returns>
        public Coordinate Translate(Coordinate coord)
        {
            return new Coordinate(_x + coord.X, _y + coord.Y);
        }

        /// <summary>
        /// Gets a <see cref="Coordinate"/> from this vector
        /// </summary>
        /// <returns>A coordinate</returns>
        public Coordinate ToCoordinate()
        {
            return new Coordinate(_x, _y);
        }

        /// <summary>
        /// Creates a copy of this vector
        /// </summary>
        /// <returns>A copy of this vector</returns>
        public object Clone()
        {
            return new Vector2D(this);
        }

        /// <summary>
        /// Gets a string representation of this vector
        /// </summary>
        /// <returns>A string representing this vector</returns>
        public override string ToString()
        {
            return "[" + _x + ", " + _y + "]";
        }

        /// <summary>
        /// Tests if a vector <paramref name="o"/> has the same values for the x and y components.
        /// </summary>
        /// <param name="o">A <see cref="Vector2D"/> with which to do the comparison.</param>
        /// <returns>true if <paramref name="o"/> is a <see cref="T:NetTopologySuite.Mathematics.Vector2D"/>with the same values for the X and Y components.</returns>
        public override bool Equals(object o)
        {
            if (!(o is Vector2D)) {
                return false;
            }
            var v = (Vector2D) o;
            return _x == v._x && _y == v._y;
        }

        /// <summary>
        /// Gets a hashcode for this vector.
        /// </summary>
        /// <returns>A hashcode for this vector</returns>
        public override int GetHashCode()
        {
            // Algorithm from Effective Java by Joshua Bloch
            int result = 17;
            result = 37 * result + _x.GetHashCode();
            result = 37 * result + _y.GetHashCode();
            return result;
        }
        
        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="left">The first vector to add.</param>
        /// <param name="right">The second vector to add.</param>
        /// <returns>The sum of the two vectors.</returns>
        public static Vector2D operator +(Vector2D left, Vector2D right)
        {
            return new Vector2D(left.X + right.X, left.Y + right.Y);
        }

        /// <summary>
        /// Modulates a vector with another by performing component-wise multiplication"/>.
        /// </summary>
        /// <param name="left">The first vector to multiply.</param>
        /// <param name="right">The second vector to multiply.</param>
        /// <returns>The multiplication of the two vectors.</returns>
        public static Vector2D operator *(Vector2D left, Vector2D right)
        {
            return new Vector2D(left.X * right.X, left.Y * right.Y);
        }
        
        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="left">The first vector to subtract.</param>
        /// <param name="right">The second vector to subtract.</param>
        /// <returns>The difference of the two vectors.</returns>
        public static Vector2D operator -(Vector2D left, Vector2D right)
        {
            return new Vector2D(left.X - right.X, left.Y - right.Y);
        }

        /// <summary>
        /// Reverses the direction of a given vector.
        /// </summary>
        /// <param name="value">The vector to negate.</param>
        /// <returns>A vector facing in the opposite direction.</returns>
        public static Vector2D operator -(Vector2D value)
        {
            return new Vector2D(-value.X, -value.Y);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name="value">The vector to scale.</param>
        /// <param name="scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static Vector2D operator *(double scale, Vector2D value)
        {
            return new Vector2D(value.X * scale, value.Y * scale);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name="value">The vector to scale.</param>
        /// <param name="scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static Vector2D operator *(Vector2D value, double scale)
        {
            return new Vector2D(value.X * scale, value.Y * scale);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name="value">The vector to scale.</param>
        /// <param name="scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static Vector2D operator /(Vector2D value, double scale)
        {
            return new Vector2D(value.X / scale, value.Y / scale);
        }

        /// <summary>
        /// Scales a vector by the given value.
        /// </summary>
        /// <param name="value">The vector to scale.</param>
        /// <param name="scale">The amount by which to scale the vector.</param>
        /// <returns>The scaled vector.</returns>
        public static Vector2D operator /(Vector2D value, Vector2D scale)
        {
            return new Vector2D(value.X / scale.X, value.Y / scale.Y);
        }

        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has the same value as
        /// <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Vector2D left, Vector2D right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// Tests for inequality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has a different value
        /// than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Vector2D left, Vector2D right)
        {
            if (left is null) return !(right is null);
            return !left.Equals(right);
        }
    }
}
