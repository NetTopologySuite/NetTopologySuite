using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NetTopologySuite.Geometries.Implementation
{
    /// <summary>
    /// A coordinate sequence that follows the dotspatial shape range
    /// </summary>
    [Serializable]
    public class DotSpatialAffineCoordinateSequence :
        CoordinateSequence
        //IMeasuredCoordinateSequence
    {
        private readonly double[] _xy;
        private readonly double[] _z;
        private readonly double[] _m;

        [NonSerialized]
        private WeakReference<Coordinate[]> _coordinateArrayRef;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="coordinates">The coordinates</param>
        /// <param name="ordinates"></param>
        public DotSpatialAffineCoordinateSequence(IReadOnlyCollection<Coordinate> coordinates, Ordinates ordinates)
            : base(coordinates?.Count ?? 0, OrdinatesUtility.OrdinatesToDimension(ordinates & Ordinates.XYZM), OrdinatesUtility.OrdinatesToMeasures(ordinates & Ordinates.XYZM))
        {
            coordinates = coordinates ?? Array.Empty<Coordinate>();

            _xy = new double[2 * coordinates.Count];
            if (HasZ)
            {
                _z = new double[coordinates.Count];
            }

            if (HasM)
            {
                _m = new double[coordinates.Count];
            }

            var xy = MemoryMarshal.Cast<double, XYStruct>(_xy);
            using (var coordinatesEnumerator = coordinates.GetEnumerator())
            {
                for (int i = 0; i < xy.Length; i++)
                {
                    if (!coordinatesEnumerator.MoveNext())
                    {
                        ThrowForInconsistentCount();
                    }

                    var coordinate = coordinatesEnumerator.Current;

                    xy[i].X = coordinate.X;
                    xy[i].Y = coordinate.Y;
                    if (_z != null)
                    {
                        _z[i] = coordinate.Z;
                    }

                    if (_m != null)
                    {
                        _m[i] = coordinate.M;
                    }
                }

                if (coordinatesEnumerator.MoveNext())
                {
                    ThrowForInconsistentCount();
                }
            }

            void ThrowForInconsistentCount() => throw new ArgumentException("IReadOnlyCollection<T>.Count is inconsistent with IEnumerable<T> implementation.", nameof(coordinates));
        }

        /// <summary>
        /// Constructs a sequence of a given size, populated with new Coordinates.
        /// </summary>
        /// <param name="size">The size of the sequence to create.</param>
        /// <param name="dimension">The number of dimensions.</param>
        /// <param name="measures">The number of measures.</param>
        public DotSpatialAffineCoordinateSequence(int size, int dimension, int measures)
            : base(size, ClipDimension(dimension, measures > 1 ? 1 : measures), measures > 1 ? 1 : measures)
        {
            _xy = new double[2 * size];

            if (HasZ)
            {
                _z = NullOrdinateArray(size);
            }

            if (HasM)
            {
                _m = NullOrdinateArray(size);
            }
        }

        /// <summary>
        /// Constructs a sequence of a given size, populated with new Coordinates.
        /// </summary>
        /// <param name="size">The size of the sequence to create.</param>
        /// <param name="ordinates">The kind of ordinates.</param>
        public DotSpatialAffineCoordinateSequence(int size, Ordinates ordinates)
            : base(size, OrdinatesUtility.OrdinatesToDimension(ordinates & Ordinates.XYZM), OrdinatesUtility.OrdinatesToMeasures(ordinates & Ordinates.XYZM))
        {
            _xy = new double[2 * size];

            if (HasZ)
            {
                _z = NullOrdinateArray(size);
            }

            if (HasM)
            {
                _m = NullOrdinateArray(size);
            }
        }

        /// <summary>
        /// Creates a sequence based on the given coordinate sequence.
        /// </summary>
        /// <param name="coordSeq">The coordinate sequence.</param>
        /// <param name="ordinates">The ordinates to copy</param>
        public DotSpatialAffineCoordinateSequence(CoordinateSequence coordSeq, Ordinates ordinates)
            : base(coordSeq?.Count ?? 0, OrdinatesUtility.OrdinatesToDimension(ordinates & Ordinates.XYZM), OrdinatesUtility.OrdinatesToMeasures(ordinates & Ordinates.XYZM))
        {
            if (coordSeq is DotSpatialAffineCoordinateSequence dsCoordSeq)
            {
                _xy = (double[])dsCoordSeq._xy.Clone();

                if (HasZ)
                {
                    _z = ((double[])dsCoordSeq._z?.Clone()) ?? NullOrdinateArray(Count);
                }

                if (HasM)
                {
                    _m = ((double[])dsCoordSeq._m?.Clone()) ?? NullOrdinateArray(Count);
                }
            }
            else
            {
                _xy = new double[2 * Count];
                if (HasZ)
                {
                    _z = new double[Count];
                }

                if (HasM)
                {
                    _m = new double[Count];
                }

                var xy = MemoryMarshal.Cast<double, XYStruct>(_xy);
                for (int i = 0; i < xy.Length; i++)
                {
                    xy[i].X = coordSeq.GetX(i);
                    xy[i].Y = coordSeq.GetY(i);
                    if (_z != null)
                    {
                        _z[i] = coordSeq.GetZ(i);
                    }

                    if (_m != null)
                    {
                        _m[i] = coordSeq.GetM(i);
                    }
                }
            }
        }

        private static double[] NullOrdinateArray(int size)
        {
            double[] res = new double[size];
            new Span<double>(res).Fill(Coordinate.NullOrdinate);
            return res;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="xy"></param>
        /// <param name="z"></param>
        /// <param name="m"></param>
        public DotSpatialAffineCoordinateSequence(double[] xy, double[] z, double[] m)
            : base(xy?.Length / 2 ?? 0, 2 + (z is null ? 0 : 1) + (m is null ? 0 : 1), m is null ? 0 : 1)
        {
            _xy = xy ?? Array.Empty<double>();
            _z = z;
            _m = m;
        }

        /// <inheritdoc cref="CoordinateSequence.Copy"/>
        public override CoordinateSequence Copy()
        {
            return new DotSpatialAffineCoordinateSequence(this, Ordinates);
        }

        /// <inheritdoc cref="CoordinateSequence.GetCoordinateCopy(int)"/>
        public override Coordinate GetCoordinateCopy(int i)
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

        /// <inheritdoc cref="CoordinateSequence.GetCoordinate(int, Coordinate)"/>
        public override void GetCoordinate(int index, Coordinate coord)
        {
            coord.X = _xy[2 * index];
            coord.Y = _xy[2 * index + 1];
            if (_z != null)
            {
                coord.Z = _z[index];
            }

            if (_m != null)
            {
                coord.M = _m[index];
            }
        }

        /// <inheritdoc />
        public override double GetX(int index)
        {
            return _xy[2 * index];
        }

        /// <inheritdoc />
        public override double GetY(int index)
        {
            return _xy[2 * index + 1];
        }

        /// <inheritdoc />
        public override double GetZ(int index)
        {
            return _z?[index] ?? Coordinate.NullOrdinate;
        }

        /// <inheritdoc />
        public override double GetM(int index)
        {
            return _m?[index] ?? Coordinate.NullOrdinate;
        }

        /// <inheritdoc />
        public override void SetX(int index, double value)
        {
            _xy[2 * index] = value;
        }

        /// <inheritdoc />
        public override void SetY(int index, double value)
        {
            _xy[2 * index + 1] = value;
        }

        /// <inheritdoc />
        public override void SetZ(int index, double value)
        {
            if (_z != null)
            {
                _z[index] = value;
            }
        }

        /// <inheritdoc />
        public override void SetM(int index, double value)
        {
            if (_m != null)
            {
                _m[index] = value;
            }
        }

        /// <inheritdoc cref="CoordinateSequence.GetOrdinate(int, int)"/>
        public override double GetOrdinate(int index, int ordinateIndex)
        {
            // if (ordinateIndex == 0 || ordinateIndex == 1)
            if (ordinateIndex >> 1 == 0)
            {
                return _xy[index * 2 + ordinateIndex];
            }

            if (ordinateIndex == ZOrdinateIndex)
            {
                return GetZ(index);
            }

            if (ordinateIndex == MOrdinateIndex)
            {
                return GetM(index);
            }

            return Coordinate.NullOrdinate;
        }

        /// <inheritdoc cref="CoordinateSequence.SetOrdinate(int, int, double)"/>
        public override void SetOrdinate(int index, int ordinateIndex, double value)
        {
            // if (ordinateIndex == 0 || ordinateIndex == 1)
            if (ordinateIndex >> 1 == 0)
            {
                _xy[index * 2 + ordinateIndex] = value;
            }
            else if (ordinateIndex == ZOrdinateIndex)
            {
                if (_z == null)
                {
                    return;
                }

                _z[index] = value;
            }
            else if (ordinateIndex == MOrdinateIndex)
            {
                if (_m == null)
                {
                    return;
                }

                _m[index] = value;
            }
            else
            {
                return;
            }

            _coordinateArrayRef = null;
        }

        private Coordinate[] GetCachedCoords()
        {
            Coordinate[] array = null;
            _coordinateArrayRef?.TryGetTarget(out array);
            return array;
        }

        /// <inheritdoc cref="CoordinateSequence.ToCoordinateArray()"/>
        public override Coordinate[] ToCoordinateArray()
        {
            var ret = GetCachedCoords();
            if (ret != null)
            {
                return ret;
            }

            ret = new Coordinate[Count];
            if (_z != null)
            {
                if (_m != null)
                {
                    for (int i = 0, j = 0; i < ret.Length; i++)
                    {
                        ret[i] = new CoordinateZM(_xy[j++], _xy[j++], _z[i], _m[i]);
                    }
                }
                else
                {
                    for (int i = 0, j = 0; i < ret.Length; i++)
                    {
                        ret[i] = new CoordinateZ(_xy[j++], _xy[j++], _z[i]);
                    }
                }
            }
            else
            {
                if (_m != null)
                {
                    for (int i = 0, j = 0; i < ret.Length; i++)
                    {
                        ret[i] = new CoordinateM(_xy[j++], _xy[j++], _m[i]);
                    }
                }
                else
                {
                    for (int i = 0, j = 0; i < ret.Length; i++)
                    {
                        ret[i] = new Coordinate(_xy[j++], _xy[j++]);
                    }
                }
            }

            _coordinateArrayRef = new WeakReference<Coordinate[]>(ret);
            return ret;
        }

        /// <inheritdoc cref="CoordinateSequence.ExpandEnvelope(Envelope)"/>
        public override Envelope ExpandEnvelope(Envelope env)
        {
            var xy = MemoryMarshal.Cast<double, XYStruct>(_xy);
            for (int i = 0; i < xy.Length; i++)
            {
                env.ExpandToInclude(xy[i].X, xy[i].Y);
            }

            return env;
        }

        /// <summary>
        /// Creates a reversed version of this coordinate sequence with cloned <see cref="Coordinate"/>s
        /// </summary>
        /// <returns>A reversed version of this sequence</returns>
        public override CoordinateSequence Reversed()
        {
            double[] xy = (double[])_xy.Clone();
            MemoryMarshal.Cast<double, XYStruct>(xy).Reverse();

            double[] z = (double[])_z?.Clone();
            new Span<double>(z).Reverse();

            double[] m = (double[])_m?.Clone();
            new Span<double>(m).Reverse();

            return new DotSpatialAffineCoordinateSequence(xy, z, m);
        }

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

        private static int ClipDimension(int dimension, int measures)
        {
            if (measures < 0 || dimension < 0 || dimension - measures < 2)
            {
                // base will throw anyway.
                return dimension;
            }

            if (dimension - measures > 3)
            {
                dimension = 3 + measures;
            }

            return dimension;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct XYStruct
        {
            public double X;
            public double Y;
        }
    }
}
