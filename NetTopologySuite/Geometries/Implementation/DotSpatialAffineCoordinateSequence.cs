using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries.Implementation
{
    /// <summary>
    /// A coordinate sequence that follows the dotspatial shape range
    /// </summary>
#if !(PCL || SILVERLIGHT)
    [System.Serializable]
#else
    [System.Runtime.Serialization.DataContract]
#endif
    public class DotSpatialAffineCoordinateSequence : 
        ICoordinateSequence
        //IMeasuredCoordinateSequence
    {
        
#if (PCL || SILVERLIGHT)
        [System.Runtime.Serialization.DataMember(IsRequired = true, Name="XY")]
#endif
        private readonly double[] _xy;
#if (PCL || SILVERLIGHT)
        [System.Runtime.Serialization.DataMember(Name = "Z")]
#endif
        private readonly double[] _z;
#if (PCL || SILVERLIGHT)
        [System.Runtime.Serialization.DataMember(Name = "M")]
#endif
        private readonly double[] _m;
        
        private readonly Ordinates _ordinates;
        
#if !(PCL || SILVERLIGHT)
        [System.NonSerialized]
#else
        [System.Runtime.Serialization.IgnoreDataMember]
#endif
        private WeakReference _coordinateArrayRef;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="coordinates">The</param>
        public DotSpatialAffineCoordinateSequence(IList<Coordinate> coordinates)
        {
            if (coordinates == null)
            {
                _xy = new double[0];
                return;
            }
            _xy = new double[2 * coordinates.Count];
            _z = new double[coordinates.Count];

            var j = 0;
            for (var i = 0; i < coordinates.Count; i++)
            {
                XY[j++] = coordinates[i].X;
                XY[j++] = coordinates[i].Y;
                Z[i] = coordinates[i].Z;
            }

            _ordinates = Ordinates.XYZ;
        }

        /// <summary>
        /// Constructs a sequence of a given size, populated with new Coordinates.
        /// </summary>
        /// <param name="size">The size of the sequence to create.</param>
        /// <param name="dimension">The number of dimensions.</param>
        public DotSpatialAffineCoordinateSequence(int size, int dimension)
        {
            _xy = new double[2 * size];
            if (dimension <= 2) return;

            _z = new double[size];
            for (var i = 0; i < size; i++)
                _z[i] = double.NaN;

            _m = new double[size];
            for (var i = 0; i < size; i++)
                _m[i] = double.NaN;
        }

        /// <summary>
        /// Constructs a sequence of a given size, populated with new Coordinates.
        /// </summary>
        /// <param name="size">The size of the sequence to create.</param>
        /// <param name="ordinates">The kind of ordinates.</param>
        public DotSpatialAffineCoordinateSequence(int size, Ordinates ordinates)
        {
            _xy = new double[2 * size];
            _ordinates = ordinates;
            if ((ordinates & Ordinates.Z) != 0)
            {
                _z = new double[size];
                for (var i = 0; i < size; i++)
                    _z[i] = Coordinate.NullOrdinate;
            }

            if ((ordinates & Ordinates.M) != 0)
            {
                _m = new double[size];
                for (var i = 0; i < size; i++)
                    _m[i] = Coordinate.NullOrdinate;
            }
        }

        /// <summary>
        /// Creates a sequence based on the given coordinate sequence.
        /// </summary>
        /// <param name="coordSeq">The coordinate sequence.</param>
        public DotSpatialAffineCoordinateSequence(ICoordinateSequence coordSeq)
        {
            var dsCoordSeq = coordSeq as DotSpatialAffineCoordinateSequence;
            var count = coordSeq.Count;
            if (dsCoordSeq != null)
            {
                _xy = new double[2 * count];
                Buffer.BlockCopy(dsCoordSeq._xy, 0, _xy, 0, 16 * count);
                if (dsCoordSeq.Z != null)
                {
                    _z = new double[dsCoordSeq.Count];
                    Buffer.BlockCopy(dsCoordSeq._z, 0, _z, 0, 8 * count);
                }

                if (dsCoordSeq.M != null)
                {
                    _m = new double[dsCoordSeq.Count];
                    Buffer.BlockCopy(dsCoordSeq._m, 0, _m, 0, 8 * count);
                }

                _ordinates = dsCoordSeq._ordinates;
                return;
            }

            _xy = new double[2 * coordSeq.Count];
            if ((coordSeq.Ordinates & Ordinates.Z) != 0)
                _z = new double[coordSeq.Count];

            if ((coordSeq.Ordinates & Ordinates.M) != 0)
                _m = new double[coordSeq.Count];

            var j = 0;
            for (var i = 0; i < coordSeq.Count; i++)
            {
                _xy[j++] = coordSeq.GetX(i);
                _xy[j++] = coordSeq.GetY(i);
                if (_z != null) _z[i] = coordSeq.GetOrdinate(i, Ordinate.Z);
                if (_m != null) _m[i] = coordSeq.GetOrdinate(i, Ordinate.M);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xy"></param>
        /// <param name="z"></param>
        public DotSpatialAffineCoordinateSequence(double[] xy, double[] z)
        {
            _xy = xy;
            _z = z;
            _ordinates = Ordinates.XYZ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xy"></param>
        /// <param name="z"></param>
        /// <param name="m"></param>
        public DotSpatialAffineCoordinateSequence(double[] xy, double[] z, double[] m)
            : this(xy, z)
        {
            _m = m;
            _ordinates = Ordinates.XYZM;
        }

        public object Clone()
        {
            return new DotSpatialAffineCoordinateSequence(this);
        }

        public Coordinate GetCoordinate(int i)
        {
            var j = 2 * i;
            return _z == null
                ? new Coordinate(_xy[j++], _xy[j])
                : new Coordinate(_xy[j++], _xy[j], _z[i]);
        }

        public Coordinate GetCoordinateCopy(int i)
        {
            return GetCoordinate(i);
        }

        public void GetCoordinate(int index, Coordinate coord)
        {
            coord.X = _xy[2 * index];
            coord.Y = _xy[2 * index + 1];
            coord.Z = _z != null ? _z[index] : Coordinate.NullOrdinate;
        }

        public double GetX(int index)
        {
            return _xy[2 * index];
        }

        public double GetY(int index)
        {
            return _xy[2 * index + 1];
        }

        public double GetOrdinate(int index, Ordinate ordinate)
        {
            switch (ordinate)
            {
                case Ordinate.X:
                    return _xy[index * 2];
                case Ordinate.Y:
                    return _xy[index * 2 + 1];
                case Ordinate.Z:
                    return _z != null ? _z[index] : Coordinate.NullOrdinate;
                case Ordinate.M:
                    return _m != null ? _m[index] : Coordinate.NullOrdinate;
                default:
                    throw new NotSupportedException();
            }
        }

        public void SetOrdinate(int index, Ordinate ordinate, double value)
        {
            switch (ordinate)
            {
                case Ordinate.X:
                    _xy[index * 2] = value;
                    break;
                case Ordinate.Y:
                    _xy[index * 2 + 1] = value;
                    break;
                case Ordinate.Z:
                    if (_z != null) _z[index] = value;
                    break;
                case Ordinate.M:
                    if (_m != null) _m[index] = value;
                    break;
                default:
                    throw new NotSupportedException();
            }
            _coordinateArrayRef = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Coordinate[] GetCachedCoords()
        {
            if (_coordinateArrayRef != null)
            {
                var arr = (Coordinate[])_coordinateArrayRef.Target;
                if (arr != null)
                    return arr;

                _coordinateArrayRef= null;
                return null;
            }
            return null;
        }


        public Coordinate[] ToCoordinateArray()
        {
            var ret = GetCachedCoords();
            if (ret != null) return ret;
            
            var j = 0;
            var count = Count;
            ret = new Coordinate[count];
            if (_z != null)
            {
                for (var i = 0; i < count; i++)
                    ret[i] = new Coordinate(_xy[j++], _xy[j++], _z[i]);
            }
            else
            {
                for (var i = 0; i < count; i++)
                    ret[i] = new Coordinate(_xy[j++], _xy[j++]);
            }

            _coordinateArrayRef = new WeakReference(ret);
            return ret;
        }

        public Envelope ExpandEnvelope(Envelope env)
        {
            var j = 0;
            for (var i = 0; i < Count; i++)
                env.ExpandToInclude(_xy[j++], _xy[j++]);
            return env;
        }

        /// <summary>
        /// Creates a reversed version of this coordinate sequence with cloned <see cref="Coordinate"/>s
        /// </summary>
        /// <returns>A reversed version of this sequence</returns>
        public ICoordinateSequence Reversed()
        {
            var xy = new double[_xy.Length];

            double[] z = null, m = null;
            if (_z != null) z = new double[_z.Length];
            if (_m != null) m = new double[_m.Length];
            
            var j = 2* Count;
            var k = Count;
            for (var i = 0; i < Count; i++)
            {
                xy[--j] = _xy[2 * i + 1];
                xy[--j] = _xy[2 * i];
                k--;
                if (_z != null) z[k] = _z[i];
                if (_m != null) m[k] = _m[i];
            }
            return new DotSpatialAffineCoordinateSequence(xy, z, m);
        }

        public int Dimension
        {
            get
            {
                var res = 2;
                if (_z != null) res++;
                if (_m != null) res++;
                return res;
            }
        }

        public Ordinates Ordinates
        {
            get { return _ordinates; }
        }

        public int Count
        {
            get { return XY.Length / 2; }
        }

        /// <summary>
        /// Gets the vector with x- and y-ordinate values;
        /// </summary>
        /// <remarks>If you modify the values of this vector externally, you need to call <see cref="ReleaseCoordinateArray"/>!</remarks>
        public double[] XY
        {
            get { return _xy; }
        }

        /// <summary>
        /// Gets the vector with z-ordinate values
        /// </summary>
        /// <remarks>If you modify the values of this vector externally, you need to call <see cref="ReleaseCoordinateArray"/>!</remarks>
        public double[] Z
        {
            get { return _z; }
        }

        /// <summary>
        /// Gets the vector with measure values
        /// </summary>
        /// <remarks>If you modify the values of this vector externally, you need to call <see cref="ReleaseCoordinateArray"/>!</remarks>
        public double[] M
        {
            get { return _m; }
        }

        /// <summary>
        /// Releases the weak reference to the weak referenced coordinate array
        /// </summary>
        /// <remarks>This is necessary if you modify the values of the <see cref="XY"/>, <see cref="Z"/>, <see cref="M"/> arrays externally.</remarks>
        public void ReleaseCoordinateArray()
        {
            _coordinateArrayRef = null;
        }
    }

    /*
#if !SILVERLIGHT
    [Serializable]
#endif
    public class DotSpatialAffineCoordinate : Coordinate
    {
        private readonly DotSpatialAffineCoordinateSequence _sequence;
        private readonly Int32 _index;

        internal DotSpatialAffineCoordinate(DotSpatialAffineCoordinateSequence sequence, int index)
        {
            _sequence = sequence;
            _index = index;
        }

        public object Clone()
        {
            return new DotSpatialAffineCoordinate((DotSpatialAffineCoordinateSequence) _sequence.Clone(), _index);
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
            var other = (Coordinate)o;
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
        public int CompareTo(Coordinate other)
        {
            if (X < other.X)
                return -1;
            if (X > other.X)
                return 1;
            if (Y < other.Y)
                return -1;
            if (Y > other.Y)
                return 1;
            return 0;
        }

        public double Distance(Coordinate p)
        {
            var dx = X - p.X;
            var dy = Y - p.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Returns whether the planar projections of the two <c>Coordinate</c>s are equal.
        ///</summary>
        /// <param name="other"><c>Coordinate</c> with which to do the 2D comparison.</param>
        /// <returns>
        /// <c>true</c> if the x- and y-coordinates are equal;
        /// the Z coordinates do not have to be equal.
        /// </returns>
        public bool Equals2D(Coordinate other)
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
            if (!(other is Coordinate))
                return false;
            return Equals((Coordinate)other);
        }

        /// <summary>
        ///
        /// </summary>
        public override int GetHashCode()
        {
            var result = 17;
            result = 37 * result + GetHashCode(X);
            result = 37 * result + GetHashCode(Y);
            return result;
        }

        /// <summary>
        /// Return HashCode.
        /// </summary>
        /// <param name="value">Value from HashCode computation.</param>
        private static int GetHashCode(double value)
        {
            var f = BitConverter.DoubleToInt64Bits(value);
            return (int)(f ^ (f >> 32));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public Boolean Equals(Coordinate other)
        {
            return Equals2D(other);
        }

        public bool Equals3D(Coordinate other)
        {
            throw new NotImplementedException();
        }

        public double X
        {
            get { return _sequence.GetX(_index); }
            set { _sequence.SetOrdinate(_index, Ordinate.X, value); }
        }

        public double Y
        {
            get { return _sequence.GetY(_index); }
            set { _sequence.SetOrdinate(_index, Ordinate.Y, value); }
        }

        public double Z
        {
            get { return _sequence.GetOrdinate(_index, Ordinate.Z); }
            set { _sequence.SetOrdinate(_index, Ordinate.Z, value); }
        }

        public double M
        {
            get { return _sequence.GetOrdinate(_index, Ordinate.M); }
            set { _sequence.SetOrdinate(_index, Ordinate.Z, value); }
        }

        public Coordinate CoordinateValue
        {
            get { return this; }
            set
            {
                X = value.X;
                Y = value.Y;
                Z = value.Z;
            }
        }

        public double this[Ordinate index]
        {
            get { return _sequence.GetOrdinate(_index, index); }
            set { _sequence.SetOrdinate(_index, index, value); }
        }
    }
     */
}