using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries.Implementation
{
    //#if !SILVERLIGHT
    [Serializable]
    //#endif
    public class DotSpatialAffineCoordinateSequence : ICoordinateSequence
    {
        private readonly double[] _xy;
        private readonly double[] _z;
        private double[] _m;
        private readonly Ordinates _ordinates;

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
            for (int i = 0; i < coordinates.Count; i++)
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
                    _z[i] = double.NaN;
            }

            if ((ordinates & Ordinates.M) != 0)
            {
                _m = new double[size];
                for (var i = 0; i < size; i++)
                    _m[i] = double.NaN;
            }
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
                Buffer.BlockCopy(dsCoordSeq.XY, 0, XY, 0, 16 * dsCoordSeq.Count);
                if (dsCoordSeq.Z != null)
                {
                    _z = new double[dsCoordSeq.Count];
                    Buffer.BlockCopy(dsCoordSeq.Z, 0, Z, 0, 8 * dsCoordSeq.Count);
                }
                return;
            }

            _xy = new double[2 * coordSeq.Count];
            if (coordSeq.Dimension > 2) _z = new double[coordSeq.Count];

            var j = 0;
            for (var i = 0; i < coordSeq.Count; i++)
            {
                XY[j++] = coordSeq.GetX(i);
                XY[j++] = coordSeq.GetY(i);
                if (Z != null)
                    Z[i] = coordSeq.GetOrdinate(i, Ordinate.X);
            }
        }

        public DotSpatialAffineCoordinateSequence(double[] xy, double[] z)
        {
            _xy = xy;
            _z = z;
        }

        public DotSpatialAffineCoordinateSequence(double[] xy, double[] z, double[] m)
            : this(xy, z)
        {
            _m = m;
        }

        public object Clone()
        {
            return new DotSpatialAffineCoordinateSequence(
                (double[])XY.Clone(), (double[])Z.Clone());
        }

        public Coordinate GetCoordinate(int i)
        {
            var j = 2 * i;
            return _z != null
                ? new Coordinate(_xy[j++], _xy[j], _z[i])
                : new Coordinate(_xy[j++], _xy[j]);
        }

        public Coordinate GetCoordinateCopy(int i)
        {
            return new Coordinate(XY[2 * i], XY[2 * i + 1], Z != null ? Z[i] : double.NaN);
        }

        public void GetCoordinate(int index, Coordinate coord)
        {
            coord.X = XY[2 * index];
            coord.Y = XY[2 * index + 1];
            coord.Z = Z != null ? Z[index] : double.NaN;
        }

        public double GetX(int index)
        {
            return XY[2 * index];
        }

        public double GetY(int index)
        {
            return XY[2 * index + 1];
        }

        public double GetOrdinate(int index, Ordinate ordinate)
        {
            switch (ordinate)
            {
                case Ordinate.X:
                    return XY[index * 2];
                case Ordinate.Y:
                    return XY[index * 2 + 1];
                case Ordinate.Z:
                    return Z != null ? Z[index] : double.NaN;
                /*case Ordinate.M:*/
                default:
                    throw new NotSupportedException();
            }
        }

        public void SetOrdinate(int index, Ordinate ordinate, double value)
        {
            switch (ordinate)
            {
                case Ordinate.X:
                    XY[index * 2] = value;
                    break;
                case Ordinate.Y:
                    XY[index * 2 + 1] = value;
                    break;
                case Ordinate.Z:
                    if (Z != null)
                        Z[index] = value;
                    break;
                /*case Ordinate.M:*/
                default:
                    throw new NotSupportedException();
            }
        }

        public Coordinate[] ToCoordinateArray()
        {
            var j = 0;
            var count = Count;
            var ret = new Coordinate[count];
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
            return ret;
        }

        public Envelope ExpandEnvelope(Envelope env)
        {
            var j = 0;
            for (var i = 0; i < Count; i++)
                env.ExpandToInclude(XY[j++], XY[j++]);
            return env;
        }

        public int Dimension
        {
            get { return Z == null ? 2 : 3; }
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
        public double[] XY
        {
            get { return _xy; }
        }

        /// <summary>
        /// Gets the vector with z-ordinate values
        /// </summary>
        public double[] Z
        {
            get { return _z; }
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