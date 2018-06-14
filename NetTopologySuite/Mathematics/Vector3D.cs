using System;
using System.Globalization;
using GeoAPI.Geometries;

namespace NetTopologySuite.Mathematics
{
    /// <summary>
    /// Represents a vector in 3-dimensional Cartesian space.
    /// </summary>
    /// <author>Martin Davis</author>
    public class Vector3D
    {

// ReSharper disable InconsistentNaming
        /// <summary>
        /// Computes the dot product of the 3D vectors AB and CD.
        /// </summary>
        /// <param name="A">A coordinate</param>
        /// <param name="B">A coordinate</param>
        /// <param name="C">A coordinate</param>
        /// <param name="D">A coordinate</param>
        /// <returns>The dot product</returns>
        public static double Dot(Coordinate A, Coordinate B, Coordinate C, Coordinate D)
        {
            double ABx = B.X - A.X;
            double ABy = B.Y - A.Y;
            double ABz = B.Z - A.Z;
            double CDx = D.X - C.X;
            double CDy = D.Y - C.Y;
            double CDz = D.Z - C.Z;
            return ABx * CDx + ABy * CDy + ABz * CDz;
        }
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Creates a new vector with given <paramref name="x"/>, <paramref name="y"/> and <paramref name="z"/> components.
        /// </summary>
        /// <param name="x">The x component</param>
        /// <param name="y">The y component</param>
        /// <param name="z">The z component</param>
        /// <returns>A new vector</returns>
        public static Vector3D Create(double x, double y, double z)
        {
            return new Vector3D(x, y, z);
        }

        /// <summary>
        /// Creates a new vector from a <see cref="Coordinate"/>.
        /// </summary>
        /// <param name="coord">The coordinate to copy</param>
        /// <returns>A new vector</returns>
        public static Vector3D Create(Coordinate coord)
        {
            return new Vector3D(coord);
        }

        public Vector3D(Coordinate v)
        {
            _x = v.X;
            _y = v.Y;
            _z = v.Z;
        }

        /// <summary>
        /// Computes the 3D dot-product of two <see cref="Coordinate"/>s
        /// </summary>
        /// <param name="v1">The 1st vector</param>
        /// <param name="v2">The 2nd vector</param>
        /// <returns>The dot product of the (coordinate) vectors</returns>
        public static double Dot(Coordinate v1, Coordinate v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        private readonly double _x;
        private readonly double _y;
        private readonly double _z;

        /// <summary>
        /// Creates a vector, that is the difference of <paramref name="to"/> and <paramref name="from"/>
        /// </summary>
        /// <param name="from">The origin coordinate</param>
        /// <param name="to">The destination coordinate</param>
        public Vector3D(Coordinate from, Coordinate to)
        {
            _x = to.X - from.X;
            _y = to.Y - from.Y;
            _z = to.Z - from.Z;
        }

        /// <summary>
        /// Creates a vector with the ordinates <paramref name="x"/>, <paramref name="y"/> and <paramref name="z"/>
        /// </summary>
        /// <param name="x">The x-ordinate</param>
        /// <param name="y">The y-ordinate</param>
        /// <param name="z">The z-ordinate</param>
        public Vector3D(double x, double y, double z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        /// <summary>
        /// Gets a value indicating the x-ordinate
        /// </summary>
        public double X => _x;

        /// <summary>
        /// Gets a value indicating the y-ordinate
        /// </summary>
        public double Y => _y;

        /// <summary>
        /// Gets a value indicating the z-ordinate
        /// </summary>
        public double Z => _z;

        /// <summary>
        /// Computes the dot-product of this <see cref="Vector3D"/> and <paramref name="v"/>
        /// </summary>
        /// <paramref name="v">The 2nd vector</paramref>
        /// <returns>The dot product of the vectors</returns>
        public double Dot(Vector3D v)
        {
            return _x * v._x + _y * v._y + _z * v._z;
        }

        /// <summary>
        /// Function to compute the length of this vector
        /// </summary>
        /// <returns>The length of this vector</returns>
        public double Length()
        {
            return Math.Sqrt(_x * _x + _y * _y + _z * _z);
        }

        /// <summary>
        /// Function to compute the length of vector <paramref name="v"/>.
        /// </summary>
        /// <param name="v">A coordinate, treated as vector</param>
        /// <returns>The length of <paramref name="v"/></returns>
        public static double Length(Coordinate v)
        {
            return Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        }

        /// <summary>
        /// Function to compute a normalized form of this vector
        /// </summary>
        /// <returns>A normalized form of this vector</returns>
        public Vector3D Normalize()
        {
            double length = Length();
            if (length > 0.0)
                return Divide(Length());
            return Create(0.0, 0.0, 0.0);
        }

        /// <summary>
        /// Function to devide all dimensions of this vector by <paramref name="d"/>.
        /// </summary>
        /// <param name="d">The divisor</param>
        /// <returns>A new (divided) vector</returns>
        private Vector3D Divide(double d)
        {
            return Create(_x / d, _y / d, _z / d);
        }

        /// <summary>
        /// Function to compute a normalized form of vector <paramref name="v"/>.
        /// </summary>
        /// <param name="v">A coordinate vector</param>
        /// <returns>A normalized form of <paramref name="v"/></returns>
        public static Coordinate Normalize(Coordinate v)
        {
            double len = Length(v);
            return new Coordinate(v.X / len, v.Y / len, v.Z / len);
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            return string.Format(NumberFormatInfo.InvariantInfo, "[{0}, {1}, {2}]", _x, _y, _z);
        }

    }
}