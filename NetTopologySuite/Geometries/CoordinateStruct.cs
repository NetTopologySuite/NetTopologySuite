using System;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    [Serializable]
    public struct CoordinateStruct : ICoordinate
    {
        private Double x;
        private Double y;
        private Double z;

        public CoordinateStruct(Double x, Double y, Double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public CoordinateStruct(Double x, Double y) : this(x, y, Double.NaN) {}

        public CoordinateStruct(ICoordinate c) : this(c.X, c.Y, c.Z) {}

        public Double X
        {
            get { return x; }
            set { x = value; }
        }

        public Double Y
        {
            get { return y; }
            set { y = value; }
        }

        public Double Z
        {
            get { return z; }
            set { z = value; }
        }

        public ICoordinate CoordinateValue
        {
            get { return this; }
            set
            {
                x = value.X;
                y = value.Y;
                z = value.Z;
            }
        }

        public Double Distance(ICoordinate p)
        {
            Double dx = x - p.X;
            Double dy = y - p.Y;
            return Math.Sqrt(dx*dx + dy*dy);
        }

        public Boolean Equals2D(ICoordinate other)
        {
            return (x == other.X) && (y == other.Y);
        }

        public Boolean Equals3D(ICoordinate other)
        {
            return (x == other.X) && (y == other.Y) &&
                   ((z == other.Z) || (Double.IsNaN(Z) && Double.IsNaN(other.Z)));
        }

        public object Clone()
        {
            return new CoordinateStruct(X, Y, Z);
        }

        public Int32 CompareTo(object obj)
        {
            ICoordinate other = (ICoordinate) obj;
            return CompareTo(other);
        }

        public Int32 CompareTo(ICoordinate other)
        {
            if (x < other.X)
            {
                return -1;
            }
            if (x > other.X)
            {
                return 1;
            }
            if (y < other.Y)
            {
                return -1;
            }
            if (y > other.Y)
            {
                return 1;
            }
            return 0;
        }

        public Boolean Equals(ICoordinate other)
        {
            return Equals2D(other);
        }

        public override Boolean Equals(object other)
        {
            if (other == null)
            {
                return false;
            }

            if (!(other is CoordinateStruct))
            {
                return false;
            }

            return Equals((CoordinateStruct) other);
        }

        public static Boolean operator ==(CoordinateStruct obj1, CoordinateStruct obj2)
        {
            return Equals(obj1, obj2);
        }

        public static Boolean operator !=(CoordinateStruct obj1, CoordinateStruct obj2)
        {
            return !(obj1 == obj2);
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ", " + z + ")";
        }

        public override Int32 GetHashCode()
        {
            Int32 result = 17;
            result = 37*result + GetHashCode(X);
            result = 37*result + GetHashCode(Y);
            return result;
        }

        private static Int32 GetHashCode(Double value)
        {
            long f = BitConverter.DoubleToInt64Bits(value);
            return (Int32) (f ^ (f >> 32));
        }
    }
}