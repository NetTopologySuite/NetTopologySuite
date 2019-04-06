using System;
using System.Collections.Generic;

namespace NetTopologySuite.Geometries.Implementation
{
    /// <summary>
    /// A coordinate sequence that follows the dotspatial shape range
    /// </summary>
    [Serializable]
    public class DotSpatialAffineCoordinateSequence :
        ICoordinateSequence
        //IMeasuredCoordinateSequence
    {

        private readonly double[] _xy;
        private readonly double[] _z;
        private readonly double[] _m;

        private readonly Ordinates _ordinates;

        [NonSerialized]
        private WeakReference _coordinateArrayRef;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="coordinates">The coordinates</param>
        /// <param name="ordinates"></param>
        public DotSpatialAffineCoordinateSequence(IList<Coordinate> coordinates, Ordinates ordinates)
        {
            // Set ordinates
            _ordinates = ordinates;

            if (coordinates == null)
            {
                _xy = new double[0];
                return;
            }
            _xy = new double[2 * coordinates.Count];
            int j = 0;
            for (int i = 0; i < coordinates.Count; i++)
            {
                XY[j++] = coordinates[i].X;
                XY[j++] = coordinates[i].Y;
            }

            if ((ordinates & Ordinates.Z) == Ordinates.Z)
            {
                _z = new double[coordinates.Count];
                for (int i = 0; i < coordinates.Count; i++)
                    Z[i] = coordinates[i].Z;
            }

            if ((ordinates & Ordinates.M) == Ordinates.M)
            {
                _m = new double[coordinates.Count];
                for (int i = 0; i < coordinates.Count; i++)
                    M[i] = Coordinate.NullOrdinate;
            }

            _ordinates = ordinates;
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
            for (int i = 0; i < size; i++)
                _z[i] = double.NaN;

            _m = new double[size];
            for (int i = 0; i < size; i++)
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
            if ((ordinates & Ordinates.Z) == Ordinates.Z)
            {
                _z = new double[size];
                for (int i = 0; i < size; i++)
                    _z[i] = Coordinate.NullOrdinate;
            }

            if ((ordinates & Ordinates.M) == Ordinates.M)
            {
                _m = new double[size];
                for (int i = 0; i < size; i++)
                    _m[i] = Coordinate.NullOrdinate;
            }
        }

        /// <summary>
        /// Creates a sequence based on the given coordinate sequence.
        /// </summary>
        /// <param name="coordSeq">The coordinate sequence.</param>
        /// <param name="ordinates">The ordinates to copy</param>
        public DotSpatialAffineCoordinateSequence(ICoordinateSequence coordSeq, Ordinates ordinates)
        {
            int count = coordSeq.Count;

            _xy = new double[2 * count];
            if ((ordinates & Ordinates.Z) == Ordinates.Z)
                _z = new double[count];
            if ((ordinates & Ordinates.M) == Ordinates.M)
                _m = new double[count];

            var dsCoordSeq = coordSeq as DotSpatialAffineCoordinateSequence;
            if (dsCoordSeq != null)
            {
                double[] nullOrdinateArray = null;
                Buffer.BlockCopy(dsCoordSeq._xy, 0, _xy, 0, 16 * count);
                if ((ordinates & Ordinates.Z) == Ordinates.Z)
                {
                    double[] copyOrdinateArray = dsCoordSeq.Z != null
                        ? dsCoordSeq.Z
                        : nullOrdinateArray = NullOrdinateArray(count);
                    Buffer.BlockCopy(copyOrdinateArray, 0, _z, 0, 8 * count);
                }

                if ((ordinates & Ordinates.M) == Ordinates.M)
                {
                    double[] copyOrdinateArray = dsCoordSeq.M != null
                        ? dsCoordSeq.M
                        : nullOrdinateArray ?? NullOrdinateArray(count);
                    Buffer.BlockCopy(copyOrdinateArray, 0, _m, 0, 8*count);
                }

                _ordinates = ordinates;
                return;
            }

            int j = 0;
            for (int i = 0; i < coordSeq.Count; i++)
            {
                _xy[j++] = coordSeq.GetX(i);
                _xy[j++] = coordSeq.GetY(i);
                if (_z != null) _z[i] = coordSeq.GetOrdinate(i, Ordinate.Z);
                if (_m != null) _m[i] = coordSeq.GetOrdinate(i, Ordinate.M);
            }
        }

        private static double[] NullOrdinateArray(int size)
        {
            double[] res = new double[size];
            for (int i = 0; i < size; i++)
                res[i] = Coordinate.NullOrdinate;
            return res;
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

            _ordinates = Ordinates.XY;
            if (_z != null) _ordinates |= Ordinates.Z;
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

        [Obsolete]
        public object Clone()
        {
            return Copy();

        }

        /// <inheritdoc cref="ICoordinateSequence.Copy"/>
        public ICoordinateSequence Copy()
        {
            return new DotSpatialAffineCoordinateSequence(this, Ordinates);
        }

        /// <inheritdoc />
        public Coordinate CreateCoordinate() => Coordinates.Create(Dimension, Measures);

        public Coordinate GetCoordinate(int i)
        {
            int j = 2 * i;
            return _z == null
                ? _m == null
                    ? new Coordinate(_xy[j++], _xy[j])
                    : new CoordinateM(_xy[j++], _xy[j], _m[i])
                : _m == null
                    ? new CoordinateZ(_xy[j++], _xy[j], _z[i])
                    : new CoordinateZM(_xy[j++], _xy[j], _z[i], _m[i]);
        }

        public Coordinate GetCoordinateCopy(int i)
        {
            return GetCoordinate(i);
        }

        public void GetCoordinate(int index, Coordinate coord)
        {
            coord.X = _xy[2 * index];
            coord.Y = _xy[2 * index + 1];
            if (HasZ)
            {
                coord.Z = _z[index];
            }
            if (HasM)
            {
                coord.M = _m[index];
            }
        }

        public double GetX(int index)
        {
            return _xy[2 * index];
        }

        public double GetY(int index)
        {
            return _xy[2 * index + 1];
        }

        /// <inheritdoc />
        public double GetZ(int index)
        {
            return _z?[index] ?? Coordinate.NullOrdinate;
        }

        /// <inheritdoc />
        public double GetM(int index)
        {
            return _m?[index] ?? Coordinate.NullOrdinate;
        }

        public double GetOrdinate(int index, Ordinate ordinate)
        {
            switch (ordinate)
            {
                case Ordinate.X:
                    return _xy[index * 2];
                case Ordinate.Y:
                    return _xy[index * 2 + 1];
                case Ordinate.Ordinate2 when HasZ:
                    return _z[index];
                case Ordinate.Ordinate2:
                case Ordinate.M:
                    return _m?[index] ?? Coordinate.NullOrdinate;
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
                case Ordinate.Ordinate2 when HasZ:
                    if (_z != null) _z[index] = value;
                    break;
                case Ordinate.Ordinate2:
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
            var localCoordinateArrayRef = _coordinateArrayRef;
            if (localCoordinateArrayRef != null && localCoordinateArrayRef.IsAlive)
            {
                var arr = (Coordinate[])localCoordinateArrayRef.Target;
                if (arr != null)
                    return arr;

                _coordinateArrayRef = null;
                return null;
            }
            return null;
        }

        public Coordinate[] ToCoordinateArray()
        {
            var ret = GetCachedCoords();
            if (ret != null) return ret;

            int j = 0;
            int count = Count;
            ret = new Coordinate[count];
            if (_z != null)
            {
                for (int i = 0; i < count; i++)
                    ret[i] = new CoordinateZ(_xy[j++], _xy[j++], _z[i]);
            }
            else
            {
                for (int i = 0; i < count; i++)
                    ret[i] = new Coordinate(_xy[j++], _xy[j++]);
            }

            _coordinateArrayRef = new WeakReference(ret);
            return ret;
        }

        public Envelope ExpandEnvelope(Envelope env)
        {
            int j = 0;
            for (int i = 0; i < Count; i++)
                env.ExpandToInclude(_xy[j++], _xy[j++]);
            return env;
        }

        /// <summary>
        /// Creates a reversed version of this coordinate sequence with cloned <see cref="Coordinate"/>s
        /// </summary>
        /// <returns>A reversed version of this sequence</returns>
        public ICoordinateSequence Reversed()
        {
            double[] xy = new double[_xy.Length];

            double[] z = null, m = null;
            if (_z != null) z = new double[_z.Length];
            if (_m != null) m = new double[_m.Length];

            int j = 2* Count;
            int k = Count;
            for (int i = 0; i < Count; i++)
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
                int res = 2;
                if (_z != null) res++;
                if (_m != null) res++;
                return res;
            }
        }

        public int Measures => _m == null ? 0 : 1;

        public bool HasZ => _z != null;

        public bool HasM => _m != null;

        public Ordinates Ordinates => _ordinates;

        public int Count => XY.Length / 2;

        /// <summary>
        /// Gets the vector with x- and y-ordinate values;
        /// </summary>
        /// <remarks>If you modify the values of this vector externally, you need to call <see cref="ReleaseCoordinateArray"/>!</remarks>
        public double[] XY => _xy;

        /// <summary>
        /// Gets the vector with z-ordinate values
        /// </summary>
        /// <remarks>If you modify the values of this vector externally, you need to call <see cref="ReleaseCoordinateArray"/>!</remarks>
        public double[] Z => _z;

        /// <summary>
        /// Gets the vector with measure values
        /// </summary>
        /// <remarks>If you modify the values of this vector externally, you need to call <see cref="ReleaseCoordinateArray"/>!</remarks>
        public double[] M => _m;

        /// <summary>
        /// Releases the weak reference to the weak referenced coordinate array
        /// </summary>
        /// <remarks>This is necessary if you modify the values of the <see cref="XY"/>, <see cref="Z"/>, <see cref="M"/> arrays externally.</remarks>
        public void ReleaseCoordinateArray()
        {
            _coordinateArrayRef = null;
        }
    }
}
