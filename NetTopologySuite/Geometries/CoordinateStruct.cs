using System;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    [Serializable]    
    public struct CoordinateStruct : ICoordinate 
    {
        private double x;
        private double y;
        private double z;

        public CoordinateStruct(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public CoordinateStruct(double x, double y) : this(x, y, Double.NaN) { }

        public CoordinateStruct(ICoordinate c) : this(c.X, c.Y, c.Z) { }

        public double X
        {
            get { return x;  }
            set { x = value; }
        }

        public double Y
        {
            get { return y;  }
            set { y = value; }
        }

        public double Z
        {
            get { return z;  }
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

        public double Distance(ICoordinate p)
        {
            double dx = x - p.X;
            double dy = y - p.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public bool Equals2D(ICoordinate other)
        {
            return (x == other.X) && (y == other.Y);
        }

        public bool Equals3D(ICoordinate other)
        {
            return (x == other.X) && (y == other.Y) && 
                ((z == other.Z) || (Double.IsNaN(Z) && Double.IsNaN(other.Z)));
        }       
        
        public object Clone()
        {
            return new CoordinateStruct(this.X, this.Y, this.Z);
        }
        
        public int CompareTo(object obj)
        {
            ICoordinate other = (ICoordinate) obj;
            return CompareTo(other);
        }

        public int CompareTo(ICoordinate other)
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

        public bool Equals(ICoordinate other)
        {
            return Equals2D(other);
        }

        public override bool Equals(object other)
        {
            if (other == null)
                return false;
            if (!(other is CoordinateStruct))
                return false;
            return Equals((CoordinateStruct) other);
        }

        public static bool operator ==(CoordinateStruct obj1, CoordinateStruct obj2)
        {
            return Equals(obj1, obj2);
        }

        public static bool operator !=(CoordinateStruct obj1, CoordinateStruct obj2)
        {
            return !(obj1 == obj2);
        }  

        public override string ToString()
        {
            return "(" + x + ", " + y + ", " + z + ")";
        }

        public override int GetHashCode()
        {
            int result = 17;
            result = 37 * result + GetHashCode(X);
            result = 37 * result + GetHashCode(Y);
            return result;
        }

        private static int GetHashCode(double value)
        {
            long f = BitConverter.DoubleToInt64Bits(value);
            return (int)(f ^ (f >> 32));
        }
    }
}
