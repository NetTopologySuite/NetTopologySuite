using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries.Implementation
{
    /// <summary>
    /// A coordinate sequence that follows the dotspatial shape range
    /// </summary>
#if HAS_SYSTEM_SERIALIZABLEATTRIBUTE
    [Serializable]
#endif
    public class DotSpatialAffineCoordinateSequence : 
        ICoordinateSequence
        //IMeasuredCoordinateSequence
    {
#if HAS_SYSTEM_SERIALIZABLEATTRIBUTE
        [NonSerialized]
#endif
        private WeakReference _coordinateArrayRef;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="coordinates">The coordinates</param>
        /// <param name="ordinates"></param>
        public DotSpatialAffineCoordinateSequence(IList<Coordinate> coordinates, Ordinates ordinates)
        {
            // Set ordinates
            Ordinates = ordinates;

            if (coordinates == null)
            {
                XY = new double[0];
                return;
            }
            XY = new double[2 * coordinates.Count];
            var j = 0;
            for (var i = 0; i < coordinates.Count; i++)
            {
                XY[j++] = coordinates[i].X;
                XY[j++] = coordinates[i].Y;
            }

            if ((ordinates & Ordinates.Z) == Ordinates.Z)
            {
                Z = new double[coordinates.Count];
                for (var i = 0; i < coordinates.Count; i++)
                    Z[i] = coordinates[i].Z;
            }

            if ((ordinates & Ordinates.M) == Ordinates.M)
            {
                M = new double[coordinates.Count];
                for (var i = 0; i < coordinates.Count; i++)
                    M[i] = Coordinate.NullOrdinate;
            }

            Ordinates = ordinates;
        }

        /// <summary>
        /// Constructs a sequence of a given size, populated with new Coordinates.
        /// </summary>
        /// <param name="size">The size of the sequence to create.</param>
        /// <param name="dimension">The number of dimensions.</param>
        public DotSpatialAffineCoordinateSequence(int size, int dimension)
        {
            XY = new double[2 * size];
            if (dimension <= 2) return;

            Z = new double[size];
            for (var i = 0; i < size; i++)
                Z[i] = double.NaN;

            M = new double[size];
            for (var i = 0; i < size; i++)
                M[i] = double.NaN;
        }

        /// <summary>
        /// Constructs a sequence of a given size, populated with new Coordinates.
        /// </summary>
        /// <param name="size">The size of the sequence to create.</param>
        /// <param name="ordinates">The kind of ordinates.</param>
        public DotSpatialAffineCoordinateSequence(int size, Ordinates ordinates)
        {
            XY = new double[2 * size];
            Ordinates = ordinates;
            if ((ordinates & Ordinates.Z) == Ordinates.Z)
            {
                Z = new double[size];
                for (var i = 0; i < size; i++)
                    Z[i] = Coordinate.NullOrdinate;
            }

            if ((ordinates & Ordinates.M) == Ordinates.M)
            {
                M = new double[size];
                for (var i = 0; i < size; i++)
                    M[i] = Coordinate.NullOrdinate;
            }
        }

        /// <summary>
        /// Creates a sequence based on the given coordinate sequence.
        /// </summary>
        /// <param name="coordSeq">The coordinate sequence.</param>
        /// <param name="ordinates">The ordinates to copy</param>
        public DotSpatialAffineCoordinateSequence(ICoordinateSequence coordSeq, Ordinates ordinates)
        {
            var count = coordSeq.Count;

            XY = new double[2 * count];
            if ((ordinates & Ordinates.Z) == Ordinates.Z)
                Z = new double[count];
            if ((ordinates & Ordinates.M) == Ordinates.M)
                M = new double[count];

            var dsCoordSeq = coordSeq as DotSpatialAffineCoordinateSequence;
            if (dsCoordSeq != null)
            {
                double[] nullOrdinateArray = null;
                Buffer.BlockCopy(dsCoordSeq.XY, 0, XY, 0, 16 * count);
                if ((ordinates & Ordinates.Z) == Ordinates.Z)
                {
                    var copyOrdinateArray = dsCoordSeq.Z != null
                        ? dsCoordSeq.Z
                        : nullOrdinateArray = NullOrdinateArray(count);
                    Buffer.BlockCopy(copyOrdinateArray, 0, Z, 0, 8 * count);
                }

                if ((ordinates & Ordinates.M) == Ordinates.M)
                {
                    var copyOrdinateArray = dsCoordSeq.M != null
                        ? dsCoordSeq.M
                        : nullOrdinateArray ?? NullOrdinateArray(count);
                    Buffer.BlockCopy(copyOrdinateArray, 0, M, 0, 8*count);
                }

                Ordinates = ordinates;
                return;
            }

            var j = 0;
            for (var i = 0; i < coordSeq.Count; i++)
            {
                XY[j++] = coordSeq.GetX(i);
                XY[j++] = coordSeq.GetY(i);
                if (Z != null) Z[i] = coordSeq.GetOrdinate(i, Ordinate.Z);
                if (M != null) M[i] = coordSeq.GetOrdinate(i, Ordinate.M);
            }
        }

        private static double[] NullOrdinateArray(int size)
        {
            var res = new double[size];
            for (var i = 0; i < size; i++)
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
            XY = xy;
            Z = z;

            Ordinates = Ordinates.XY;
            if (Z != null) Ordinates |= Ordinates.Z;
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
            M = m;
            Ordinates = Ordinates.XYZM;
        }

        [Obsolete]
        public Object Clone()
        {
            return Copy();

        }

        /// <inheritdoc cref="ICoordinateSequence.Copy"/>
        public ICoordinateSequence Copy()
        {
            return new DotSpatialAffineCoordinateSequence(this, Ordinates);
        }

        public Coordinate GetCoordinate(int i)
        {
            var j = 2 * i;
            return Z == null
                ? new Coordinate(XY[j++], XY[j])
                : new Coordinate(XY[j++], XY[j], Z[i]);
        }

        public Coordinate GetCoordinateCopy(int i)
        {
            return GetCoordinate(i);
        }

        public void GetCoordinate(int index, Coordinate coord)
        {
            coord.X = XY[2 * index];
            coord.Y = XY[2 * index + 1];
            coord.Z = Z != null ? Z[index] : Coordinate.NullOrdinate;
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
                    return Z != null ? Z[index] : Coordinate.NullOrdinate;
                case Ordinate.M:
                    return M != null ? M[index] : Coordinate.NullOrdinate;
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
                    if (Z != null) Z[index] = value;
                    break;
                case Ordinate.M:
                    if (M != null) M[index] = value;
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
            
            var j = 0;
            var count = Count;
            ret = new Coordinate[count];
            if (Z != null)
            {
                for (var i = 0; i < count; i++)
                    ret[i] = new Coordinate(XY[j++], XY[j++], Z[i]);
            }
            else
            {
                for (var i = 0; i < count; i++)
                    ret[i] = new Coordinate(XY[j++], XY[j++]);
            }

            _coordinateArrayRef = new WeakReference(ret);
            return ret;
        }

        public Envelope ExpandEnvelope(Envelope env)
        {
            var j = 0;
            for (var i = 0; i < Count; i++)
                env.ExpandToInclude(XY[j++], XY[j++]);
            return env;
        }

        /// <summary>
        /// Creates a reversed version of this coordinate sequence with cloned <see cref="Coordinate"/>s
        /// </summary>
        /// <returns>A reversed version of this sequence</returns>
        public ICoordinateSequence Reversed()
        {
            var xy = new double[XY.Length];

            double[] z = null, m = null;
            if (Z != null) z = new double[Z.Length];
            if (M != null) m = new double[M.Length];
            
            var j = 2* Count;
            var k = Count;
            for (var i = 0; i < Count; i++)
            {
                xy[--j] = XY[2 * i + 1];
                xy[--j] = XY[2 * i];
                k--;
                if (Z != null) z[k] = Z[i];
                if (M != null) m[k] = M[i];
            }
            return new DotSpatialAffineCoordinateSequence(xy, z, m);
        }

        public int Dimension
        {
            get
            {
                var res = 2;
                if (Z != null) res++;
                if (M != null) res++;
                return res;
            }
        }

        public Ordinates Ordinates { get; }

        public int Count => XY.Length / 2;

        /// <summary>
        /// Gets the vector with x- and y-ordinate values;
        /// </summary>
        /// <remarks>If you modify the values of this vector externally, you need to call <see cref="ReleaseCoordinateArray"/>!</remarks>
        public double[] XY { get; }

        /// <summary>
        /// Gets the vector with z-ordinate values
        /// </summary>
        /// <remarks>If you modify the values of this vector externally, you need to call <see cref="ReleaseCoordinateArray"/>!</remarks>
        public double[] Z { get; }

        /// <summary>
        /// Gets the vector with measure values
        /// </summary>
        /// <remarks>If you modify the values of this vector externally, you need to call <see cref="ReleaseCoordinateArray"/>!</remarks>
        public double[] M { get; }

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