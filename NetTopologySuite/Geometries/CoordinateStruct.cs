using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries
{
//#if!SILVERLIGHT
    [Serializable]
//#endif
    public struct CoordinateStruct : ICoordinate, ICloneable
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

        public CoordinateStruct(Coordinate c) : this(c.X, c.Y, c.Z) { }

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

        public double M
        {
            get { return Double.NaN; }
            set {  }
        }

        public Coordinate CoordinateValue
        {
            get { return ((ICoordinate)this).ToCoordinate(); }
            set
            {
                x = value.X;
                y = value.Y;
                z = value.Z;
            }
        }

        public double this[Ordinate index]
        {
            get
            {
                switch (index)
                {
                    case Ordinate.X:
                        return x;
                    case Ordinate.Y:
                        return y;
                    case Ordinate.Z:
                        return z;
                    default:
                        return Double.NaN;
                }
            }
            set
            {
                switch (index)
                {
                    case Ordinate.X:
                        x = value;
                        break;
                    case Ordinate.Y:
                        y = value;
                        break;
                    case Ordinate.Z:
                        z = value;
                        break;
                }
            }
        }

        public double Distance(Coordinate p)
        {
            double dx = x - p.X;
            double dy = y - p.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public bool Equals2D(Coordinate other)
        {
            return (x == other.X) && (y == other.Y);
        }

        public bool Equals3D(Coordinate other)
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
            Coordinate other = (Coordinate) obj;
            return CompareTo(other);
        }

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

        public bool Equals(Coordinate other)
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

        ICoordinate ICoordinate.CoordinateValue
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        double ICoordinate.this[Ordinate index]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        double ICoordinate.Distance(ICoordinate other)
        {
            throw new NotImplementedException();
        }

        bool ICoordinate.Equals2D(ICoordinate other)
        {
            throw new NotImplementedException();
        }

        bool ICoordinate.Equals3D(ICoordinate other)
        {
            throw new NotImplementedException();
        }

        Coordinate ICoordinate.ToCoordinate()
        {
            return new Coordinate(x, y, z);
        }

        object ICloneable.Clone()
        {
            return new CoordinateStruct(x, y, z);
        }

        int IComparable.CompareTo(object obj)
        {
            throw new NotImplementedException();
        }

        int IComparable<ICoordinate>.CompareTo(ICoordinate other)
        {
            throw new NotImplementedException();
        }

        bool IEquatable<ICoordinate>.Equals(ICoordinate other)
        {
            throw new NotImplementedException();
        }
    }
}
