using System;
using GeoAPI;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Mathematics
{
    /// <summary>
    ///     A 2-dimensional mathematical vector represented by double-precision X and Y components.
    /// </summary>
    /// <author>mbdavis</author>
    public class Vector2D : ICloneable
    {
        /**
         * The X component of this vector.
         */

        /**
         * The Y component of this vector.
         */

        /// <summary>
        ///     Creates an new vector instance
        /// </summary>
        public Vector2D() : this(0.0, 0.0)
        {
        }

        /// <summary>
        ///     Creates a new vector instance using the provided <paramref name="x" /> and <paramref name="y" /> ordinates
        /// </summary>
        /// <param name="x">The x-ordinate value</param>
        /// <param name="y">The y-ordinate value</param>
        public Vector2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        ///     Creates a new vector instance based on <paramref name="v" />.
        /// </summary>
        /// <param name="v">The vector</param>
        public Vector2D(Vector2D v)
        {
            X = v.X;
            Y = v.Y;
        }

        /// <summary>
        ///     Creates a new vector with the direction and magnitude
        ///     of the difference between the
        ///     <paramref name="to" /> and <paramref name="from" /> <see cref="Coordinate" />s.
        /// </summary>
        /// <param name="from">The origin coordinate</param>
        /// <param name="to">The destination coordinate</param>
        public Vector2D(Coordinate from, Coordinate to)
        {
            X = to.X - from.X;
            Y = to.Y - from.Y;
        }

        /// <summary>
        ///     Creates a vector from a <see cref="Coordinate" />.
        /// </summary>
        /// <param name="v">The coordinate</param>
        /// <returns>A new vector</returns>
        public Vector2D(Coordinate v)
        {
            X = v.X;
            Y = v.Y;
        }

        /// <summary>
        ///     Gets the x-ordinate value
        /// </summary>
        public double X { get; }

        /// <summary>
        ///     Gets the y-ordinate value
        /// </summary>
        public double Y { get; }

        /// <summary>
        ///     Gets the ordinate values by index
        /// </summary>
        /// <param name="index">The index</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">Thrown if index &lt; 0 or &gt 1</exception>
        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }


        /// <summary>
        ///     Creates a copy of this vector
        /// </summary>
        /// <returns>A copy of this vector</returns>
        public object Clone()
        {
            return new Vector2D(this);
        }

        /// <summary>
        ///     Creates a new vector with given X and Y components.
        /// </summary>
        /// <param name="x">The x component</param>
        /// <param name="y">The y component</param>
        /// <returns>A new vector</returns>
        public static Vector2D Create(double x, double y)
        {
            return new Vector2D(x, y);
        }

        /// <summary>
        ///     Creates a new vector from an existing one.
        /// </summary>
        /// <param name="v">The vector to copy</param>
        /// <returns>A new vector</returns>
        public static Vector2D Create(Vector2D v)
        {
            return new Vector2D(v);
        }


        /// <summary>
        ///     Creates a vector from a <see cref="Coordinate" />.
        /// </summary>
        /// <param name="coord">The coordinate to copy</param>
        /// <returns>A new vector</returns>
        public static Vector2D Create(Coordinate coord)
        {
            return new Vector2D(coord);
        }

        /// <summary>
        ///     Creates a vector with the direction and magnitude
        ///     of the difference between the
        ///     <paramref name="to" /> and <paramref name="from" /> <see cref="Coordinate" />s.
        /// </summary>
        /// <param name="from">The origin coordinate</param>
        /// <param name="to">The destination coordinate</param>
        /// <returns>A new vector</returns>
        public static Vector2D Create(Coordinate from, Coordinate to)
        {
            return new Vector2D(from, to);
        }

        /// <summary>
        ///     Adds <paramref name="v" /> to this vector instance.
        /// </summary>
        /// <param name="v">The vector to add</param>
        /// <returns>The result vector</returns>
        public Vector2D Add(Vector2D v)
        {
            return Create(X + v.X, Y + v.Y);
        }

        /// <summary>
        ///     Subtracts <paramref name="v" /> from this vector instance
        /// </summary>
        /// <param name="v">The vector to subtract</param>
        /// <returns>The result vector</returns>
        public Vector2D Subtract(Vector2D v)
        {
            return Create(X - v.X, Y - v.Y);
        }

        /// <summary>
        ///     Multiplies the vector by a scalar value.
        /// </summary>
        /// <param name="d">The value to multiply by</param>
        /// <returns>A new vector with the value v * d</returns>
        public Vector2D Multiply(double d)
        {
            return Create(X*d, Y*d);
        }

        /// <summary>
        ///     Divides the vector by a scalar value.
        /// </summary>
        /// <param name="d">The value to divide by</param>
        /// <returns>A new vector with the value v / d</returns>
        public Vector2D Divide(double d)
        {
            return Create(X/d, Y/d);
        }

        /// <summary>
        ///     Negates this vector
        /// </summary>
        /// <returns>A new vector with [-_x, -_y]</returns>
        public Vector2D Negate()
        {
            return Create(-X, -Y);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public double Length()
        {
            return Math.Sqrt(X*X + Y*Y);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public double LengthSquared()
        {
            return X*X + Y*Y;
        }

        /// <summary>
        ///     Normalizes the vector
        /// </summary>
        /// <returns>A new normalized vector</returns>
        public Vector2D Normalize()
        {
            var length = Length();
            if (length > 0.0)
                return Divide(length);
            return Create(0.0, 0.0);
        }

        /// <summary>
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Vector2D Average(Vector2D v)
        {
            return WeightedSum(v, 0.5);
        }

        /// <summary>
        ///     Computes the weighted sum of this vector
        ///     with another vector,
        ///     with this vector contributing a fraction
        ///     of <tt>frac</tt> to the total.
        ///     <para />
        ///     In other words,
        ///     <pre>
        ///         sum = frac * this + (1 - frac) * v
        ///     </pre>
        /// </summary>
        /// <param name="v">The vector to sum</param>
        /// <param name="frac">The fraction of the total contributed by this vector</param>
        /// <returns>The weighted sum of the two vectors</returns>
        public Vector2D WeightedSum(Vector2D v, double frac)
        {
            return Create(
                frac*X + (1.0 - frac)*v.X,
                frac*Y + (1.0 - frac)*v.Y);
        }

        /// <summary>
        ///     Computes the distance between this vector and another one.
        /// </summary>
        /// <param name="v">A vector</param>
        /// <returns>The distance between the vectors</returns>
        public double Distance(Vector2D v)
        {
            var delx = v.X - X;
            var dely = v.Y - Y;
            return Math.Sqrt(delx*delx + dely*dely);
        }

        /// <summary>
        ///     Computes the dot-product of two vectors
        /// </summary>
        /// <param name="v">A vector</param>
        /// <returns>The dot product of the vectors</returns>
        public double Dot(Vector2D v)
        {
            return X*v.X + Y*v.Y;
        }

        /// <summary>
        ///     Computes the angle this vector describes to the horizontal axis
        /// </summary>
        /// <returns>The angle</returns>
        public double Angle()
        {
            return Math.Atan2(Y, X);
        }

        /// <summary>
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public double Angle(Vector2D v)
        {
            return AngleUtility.Diff(v.Angle(), Angle());
        }

        /// <summary>
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public double AngleTo(Vector2D v)
        {
            var a1 = Angle();
            var a2 = v.Angle();
            var angDel = a2 - a1;

            // normalize, maintaining orientation
            if (angDel <= -Math.PI)
                return angDel + AngleUtility.PiTimes2;
            if (angDel > Math.PI)
                return angDel - AngleUtility.PiTimes2;
            return angDel;
        }

        /// <summary>
        ///     Rotates this vector by <paramref name="angle" />
        /// </summary>
        /// <param name="angle">The angle</param>
        /// <returns>The rotated vector</returns>
        public Vector2D Rotate(double angle)
        {
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);
            return Create(
                X*cos - Y*sin,
                X*sin + Y*cos
            );
        }

        /// <summary>
        ///     Rotates a vector by a given number of quarter-circles (i.e. multiples of 90
        ///     degrees or Pi/2 radians). A positive number rotates counter-clockwise, a
        ///     negative number rotates clockwise. Under this operation the magnitude of
        ///     the vector and the absolute values of the ordinates do not change, only
        ///     their sign and ordinate index.
        /// </summary>
        /// <param name="numQuarters">The number of quarter-circles to rotate by</param>
        /// <returns>The rotated vector.</returns>
        public Vector2D RotateByQuarterCircle(int numQuarters)
        {
            var nQuad = numQuarters%4;
            if ((numQuarters < 0) && (nQuad != 0))
                nQuad = nQuad + 4;
            switch (nQuad)
            {
                case 0:
                    return Create(X, Y);
                case 1:
                    return Create(-Y, X);
                case 2:
                    return Create(-X, -Y);
                case 3:
                    return Create(Y, -X);
            }
            Assert.ShouldNeverReachHere();
            return null;
        }

        /// <summary>
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool IsParallel(Vector2D v)
        {
            return 0d == RobustDeterminant.SignOfDet2x2(X, Y, v.X, v.Y);
        }

        /// <summary>
        ///     Gets a <see cref="Coordinate" /> made of this vector translated by <paramref name="coord" />.
        /// </summary>
        /// <param name="coord">The translation coordinate</param>
        /// <returns>A coordinate</returns>
        public Coordinate Translate(Coordinate coord)
        {
            return new Coordinate(X + coord.X, Y + coord.Y);
        }

        /// <summary>
        ///     Gets a <see cref="Coordinate" /> from this vector
        /// </summary>
        /// <returns>A coordinate</returns>
        public Coordinate ToCoordinate()
        {
            return new Coordinate(X, Y);
        }

        /// <summary>
        ///     Gets a string representation of this vector
        /// </summary>
        /// <returns>A string representing this vector</returns>
        public override string ToString()
        {
            return "[" + X + ", " + Y + "]";
        }

        /// <summary>
        ///     Tests if a vector <paramref name="o" /> has the same values for the x and y components.
        /// </summary>
        /// <param name="o">A <see cref="Vector2D" /> with which to do the comparison.</param>
        /// <returns>
        ///     true if <paramref name="o" /> is a <see cref="T:NetTopologySuite.Mathematics.Vector2D" />with the same values
        ///     for the X and Y components.
        /// </returns>
        public override bool Equals(object o)
        {
            if (!(o is Vector2D)) return false;
            var v = (Vector2D) o;
            return (X == v.X) && (Y == v.Y);
        }

        /// <summary>
        ///     Gets a hashcode for this vector.
        /// </summary>
        /// <returns>A hashcode for this vector</returns>
        public override int GetHashCode()
        {
            // Algorithm from Effective Java by Joshua Bloch
            var result = 17;
            result = 37*result + Coordinate.GetHashCode(X);
            result = 37*result + Coordinate.GetHashCode(Y);
            return result;
        }
    }
}