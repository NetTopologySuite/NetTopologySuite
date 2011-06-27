using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries
{
//#if !SILVERLIGHT    
    [Serializable]
//#endif
    public class DotSpatialAffineCoordinateSequence : ICoordinateSequence
    {
        private readonly double[] _xy;
        private readonly double[] _z;
        //private double[] _m;
        
        public DotSpatialAffineCoordinateSequence(ICoordinate[] coordinates) 
        {
            if (coordinates == null)
            {
                _xy = new double[0];
                return;
            }
            _xy = new double[2 * coordinates.Length];
            _z = new double[coordinates.Length];

            int j = 0;
            for (int i = 0; i < coordinates.Length; i++)
            {
                _xy[j++] = coordinates[i].X;
                _xy[j++] = coordinates[i].Y;
                _z[i] = coordinates[i].Z;
            }
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
            for(var i = 0; i < size; i++)
                _z[i] = double.NaN;
        }

        /// <summary>
        /// Constructs a sequence based on the given coordinate sequence.
        /// </summary>
        /// <param name="coordSeq">The coordinate sequence.</param>      
        public DotSpatialAffineCoordinateSequence(ICoordinateSequence coordSeq)
        {
            var dsCoordSeq = coordSeq as DotSpatialAffineCoordinateSequence;
            if (dsCoordSeq != null)
            {
                _xy = new double[2 * dsCoordSeq.Count];
                Buffer.BlockCopy(dsCoordSeq._xy, 0, _xy, 0, 16*dsCoordSeq.Count);
                if (dsCoordSeq._z != null)
                {
                    _z = new double[dsCoordSeq.Count];
                    Buffer.BlockCopy(dsCoordSeq._z, 0, _z, 0, 8 * dsCoordSeq.Count);
                }
                return;
            }

            _xy = new double[2 * coordSeq.Count];
            if (coordSeq.Dimension > 2) _z = new double[coordSeq.Count];

            var j = 0;
            for (var i = 0; i < coordSeq.Count; i++ )
            {
                _xy[j++] = coordSeq.GetX(i);
                _xy[j++] = coordSeq.GetY(i);
                if (_z != null)
                    _z[i] = coordSeq.GetOrdinate(i, Ordinate.X);
            }
        }

        public DotSpatialAffineCoordinateSequence(double[] xy, double[] z)
        {
            _xy = xy;
            _z = z;
        }
        
        public object Clone()
        {
            return new DotSpatialAffineCoordinateSequence(
                (double[]) _xy.Clone(), (double[]) _z.Clone());
        }

        public ICoordinate GetCoordinate(int i)
        {
            return new DotSpatialAffineCoordinate(this, i);
        }

        public ICoordinate GetCoordinateCopy(int i)
        {
            return new Coordinate(_xy[2*i], _xy[2*i+1], _z != null ? _z[i] : double.NaN );
        }

        public void GetCoordinate(int index, ICoordinate coord)
        {
            coord.X = _xy[2*index];
            coord.Y = _xy[2*index + 1];
            coord.Z = _z != null ? _z[index] : double.NaN;
        }

        public double GetX(int index)
        {
            return _xy[2*index];
        }

        public double GetY(int index)
        {
            return _xy[2*index + 1];
        }

        public double GetOrdinate(int index, Ordinate ordinate)
        {
            switch (ordinate)
            {
                case Ordinate.X:
                    return _xy[index*2];
                case Ordinate.Y:
                    return _xy[index*2+1];
                case Ordinate.Z:
                    return _z != null ? _z[index] : double.NaN;
                /*case Ordinates.M:*/
                default:
                    throw new NotSupportedException();
            }
        }

        public void SetOrdinate(int index, Ordinate ordinate, double value)
        {
            switch (ordinate)
            {
                case Ordinate.X:
                    _xy[index*2] = value;
                    break;
                case Ordinate.Y:
                    _xy[index*2 + 1] = value;
                    break;
                case Ordinate.Z:
                    if (_z != null)
                        _z[index] = value;
                    break;
                    /*case Ordinates.M:*/
                default:
                    throw new NotSupportedException();
            }
        }

        public ICoordinate[] ToCoordinateArray()
        {
            //var j = 0;
            var count = Count;
            var ret = new ICoordinate[count];
            for (var i = 0; i < count; i++)
            {
                ret[i] = new DotSpatialAffineCoordinate(this, i);
                /*
                ret[i] = new Coordinate(_xy[j++], _xy[j++]);
                if (_z != null)
                    ret[i].Z = _z[i];
                 */
            }
            return ret;
        }

        public IEnvelope ExpandEnvelope(IEnvelope env)
        {
            var j = 0;
            for (var i = 0; i < Count; i++ ) 
                env.ExpandToInclude( _xy[j++], _xy[j++]);            
            return env;
        }

        public int Dimension
        {
            get { return _z == null ? 2 : 3; }
        }

        public int Count
        {
            get { return _xy.Length / 2; }
        }
    }

#if !SILVERLIGHT
    [Serializable]
#endif
    public class DotSpatialAffineCoordinate : ICoordinate
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

        public double Distance(ICoordinate p)
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
        public Boolean Equals(ICoordinate other)
        {
            return Equals2D(other);
        }

        public bool Equals3D(ICoordinate other)
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

        public ICoordinate CoordinateValue
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
}
