using System;
using System.Collections.Generic;
using System.Text;

namespace GisSharpBlog.NetTopologySuite.Geometries.LightStructs
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public struct Coordinate : ICloneable, IComparable, IComparable<Coordinate>, IEquatable<Coordinate>
    {
        /// <summary>
        /// 
        /// </summary>
        public static Coordinate Null = new Coordinate(Double.NaN, Double.NaN, Double.NaN);       

        private double x;
        private double y;
        private double z;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Coordinate(double x, double y) : this(x, y, Double.NaN) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Coordinate(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// 
        /// </summary>
        public double X
        {
            get { return x; }
            set { x = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public double Y
        {
            get { return y; }
            set { y = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public double Z
        {
            get { return z; }
            set { z = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsNull
        {
            get
            {
                return X == Double.NaN || Y == Double.NaN;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsValid
        {
            get
            {
                return IsValid2D;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsValid2D
        {
            get
            {
                return X != Double.NaN && Y != Double.NaN;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsValid3D
        {
            get
            {
                return X != Double.NaN && Y != Double.NaN && Z != Double.NaN;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Is2D
        {
            get
            {
                return IsValid2D && !IsValid3D;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Is3D
        {
            get
            {
                return IsValid3D;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(x).Append(' ').Append(Y);
            if (Z != Double.NaN)
                sb.Append(' ').Append(Z);
            return sb.ToString();
        }

        /// <summary>
        /// Create a new object as copy of this instance.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new Coordinate(this.X, this.Y, this.Z);
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
            if (!(other is Coordinate))
                return false;
            return Equals((Coordinate) other);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public Boolean Equals(Coordinate other)
        {
            return Equals2D((Coordinate) other);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator ==(Coordinate obj1, Coordinate obj2)
        {
            return Object.Equals(obj1, obj2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator !=(Coordinate obj1, Coordinate obj2)
        {
            return !(obj1 == obj2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals2D(Coordinate other)
        {
            if (x != other.X)
                return false;
            if (y != other.Y)
                return false;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals3D(Coordinate other)
        {
            return (x == other.X) && (y == other.Y) && ((z == other.Z) ||
                    (Double.IsNaN(Z) && Double.IsNaN(other.Z)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public int CompareTo(object o)
        {
            Coordinate other = (Coordinate) o;
            return CompareTo(other);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Coordinate other)
        {
            if (x < other.X)
                return -1;
            if (x > other.X)
                return 1;
            if (y < other.Y)
                return -1;
            if (y > other.Y)
                return 1;
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        public override int GetHashCode()
        {
            int result = 17;
            result = 37 * result + GetHashCode(X);
            result = 37 * result + GetHashCode(Y);
            return result;
        }

        /// <summary>
        /// Return HashCode.
        /// </summary>
        /// <param name="x">Value from HashCode computation.</param>
        private static int GetHashCode(double value)
        {
            long f = BitConverter.DoubleToInt64Bits(value);
            return (int)(f ^ (f >> 32));
        }
    }
}
