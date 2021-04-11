using System;
using System.Collections.Generic;

namespace NetTopologySuite.Geometries.Implementation
{
    /// <summary>
    /// Factory for creating <see cref="RawCoordinateSequence"/> instances.
    /// </summary>
    public sealed class RawCoordinateSequenceFactory : CoordinateSequenceFactory
    {
        private readonly Ordinates _ordinatesInGroups;

        private readonly Ordinates[] _ordinateGroups;

        /// <summary>
        /// Initializes a new instance of the <see cref="RawCoordinateSequenceFactory"/> class.
        /// </summary>
        /// <param name="ordinateGroups">
        /// A sequence of zero or more <see cref="Ordinates"/> flags representing ordinate values
        /// that should be allocated together.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="ordinateGroups"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when a given flag appears in more than one element of
        /// <paramref name="ordinateGroups"/>.
        /// </exception>
        /// <remarks>
        /// Any flags not represented in <paramref name="ordinateGroups"/>, and any spatial or
        /// measure dimensions beyond the 16th, will be allocated together, SoA-style.
        /// <para/>
        /// Elements without any bits set will be silently ignored.
        /// </remarks>
        public RawCoordinateSequenceFactory(IEnumerable<Ordinates> ordinateGroups)
        {
            if (ordinateGroups is null)
            {
                throw new ArgumentNullException(nameof(ordinateGroups));
            }

            var seenOrdinates = Ordinates.None;
            var ordinateGroupsList = new List<Ordinates>();
            foreach (var ordinateGroup in ordinateGroups)
            {
                if ((ordinateGroup & seenOrdinates) != Ordinates.None)
                {
                    throw new ArgumentException("Each ordinate may show up in at most one group.", nameof(ordinateGroups));
                }

                seenOrdinates |= ordinateGroup;

                if (OrdinatesUtility.OrdinatesToDimension(ordinateGroup) < 2)
                {
                    // it would have been equally correct to omit this
                    continue;
                }

                _ordinatesInGroups |= ordinateGroup;
                ordinateGroupsList.Add(ordinateGroup);
            }

            _ordinateGroups = ordinateGroupsList.ToArray();
        }

        /// <summary>
        /// Creates a new <see cref="RawCoordinateSequence"/> that uses the given arrays for reading
        /// and writing X and Y data ignoring the <see cref="Ordinates"/> flags that were passed
        /// into the constructor for this factory instance.
        /// </summary>
        /// <param name="x">
        /// An array of X values, laid out as
        /// <c>[x0, x1, x2, ..., xn]</c>.
        /// </param>
        /// <param name="y">
        /// An array of Y values, laid out as
        /// <c>[y0, y1, y2, ..., yn]</c>.
        /// </param>
        /// <returns>
        /// A <see cref="RawCoordinateSequence"/> instance that's backed by the given arrays.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the input arrays do not contain data for the same number of coordinates.
        /// </exception>
        public static RawCoordinateSequence CreateXY(Memory<double> x, Memory<double> y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("Arrays must contain data for the same number of coordinates.");
            }

            Memory<double>[] rawData = { x, y };
            (int RawDataIndex, int DimensionIndex)[] dimensionMap = { (0, 0), (1, 0) };
            return new RawCoordinateSequence(rawData, dimensionMap, measures: 0);
        }

        /// <summary>
        /// Creates a new <see cref="RawCoordinateSequence"/> that uses the given array for reading
        /// and writing X and Y data ignoring the <see cref="Ordinates"/> flags that were passed
        /// into the constructor for this factory instance.
        /// </summary>
        /// <param name="xy">
        /// An array of X and Y values, laid out as
        /// <c>[x0, y0, x1, y1, x2, y2, ..., xn, yn]</c>.
        /// </param>
        /// <returns>
        /// A <see cref="RawCoordinateSequence"/> instance that's backed by the given array.
        /// </returns>
        /// <remarks>
        /// The resulting instance is essentially a <see cref="PackedDoubleCoordinateSequence"/>
        /// with slightly more overhead, so the main reason to prefer this over that one would be if
        /// you <b>really</b> need to avoid copying the data to fit it into that format.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the length of <paramref name="xy"/> is not a multiple of 2.
        /// </exception>
        public static RawCoordinateSequence CreateXY(Memory<double> xy)
        {
            if (xy.Length % 2 != 0)
            {
                throw new ArgumentException("Length must be a multiple of 2.", nameof(xy));
            }

            Memory<double>[] rawData = { xy };
            (int RawDataIndex, int DimensionIndex)[] dimensionMap = { (0, 0), (0, 1) };
            return new RawCoordinateSequence(rawData, dimensionMap, measures: 0);
        }

        /// <summary>
        /// Creates a new <see cref="RawCoordinateSequence"/> that uses the given arrays for reading
        /// and writing X, Y, and Z data ignoring the <see cref="Ordinates"/> flags that were passed
        /// into the constructor for this factory instance.
        /// </summary>
        /// <param name="x">
        /// An array of X values, laid out as
        /// <c>[x0, x1, x2, ..., xn]</c>.
        /// </param>
        /// <param name="y">
        /// An array of Y values, laid out as
        /// <c>[y0, y1, y2, ..., yn]</c>.
        /// </param>
        /// <param name="z">
        /// An array of Z values, laid out as
        /// <c>[z0, z1, z2, ..., zn]</c>.
        /// </param>
        /// <returns>
        /// A <see cref="RawCoordinateSequence"/> instance that's backed by the given arrays.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the input arrays do not contain data for the same number of coordinates.
        /// </exception>
        public static RawCoordinateSequence CreateXYZ(Memory<double> x, Memory<double> y, Memory<double> z)
        {
            if (x.Length != y.Length || x.Length != z.Length)
            {
                throw new ArgumentException("Arrays must contain data for the same number of coordinates.");
            }

            Memory<double>[] rawData = { x, y, z };
            (int RawDataIndex, int DimensionIndex)[] dimensionMap = { (0, 0), (1, 0), (2, 0) };
            return new RawCoordinateSequence(rawData, dimensionMap, measures: 0);
        }

        /// <summary>
        /// Creates a new <see cref="RawCoordinateSequence"/> that uses the given array for reading
        /// and writing X, Y, and Z data ignoring the <see cref="Ordinates"/> flags that were passed
        /// into the constructor for this factory instance.
        /// </summary>
        /// <param name="xy">
        /// An array of X and Y values, laid out as
        /// <c>[x0, y0, x1, y1, x2, y2, ..., xn, yn]</c>.
        /// </param>
        /// <param name="z">
        /// An array of Z values, laid out as
        /// <c>[z0, z1, z2, ..., zn]</c>.
        /// </param>
        /// <returns>
        /// A <see cref="RawCoordinateSequence"/> instance that's backed by the given array.
        /// </returns>
        /// <remarks>
        /// The resulting instance is essentially a <see cref="DotSpatialAffineCoordinateSequence"/>
        /// with slightly more overhead, so the main reason to prefer this over that one would be if
        /// you <b>really</b> need to avoid copying the data to fit it into that format.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the length of <paramref name="xy"/> is not a multiple of 2, or when the
        /// input arrays do not contain data for the same number of coordinates.
        /// </exception>
        public static RawCoordinateSequence CreateXYZ(Memory<double> xy, Memory<double> z)
        {
            if (xy.Length % 2 != 0)
            {
                throw new ArgumentException("Length must be a multiple of 2.", nameof(xy));
            }

            if (xy.Length != z.Length * 2)
            {
                throw new ArgumentException("Arrays must contain data for the same number of coordinates.");
            }

            Memory<double>[] rawData = { xy, z };
            (int RawDataIndex, int DimensionIndex)[] dimensionMap = { (0, 0), (0, 1), (1, 0) };
            return new RawCoordinateSequence(rawData, dimensionMap, measures: 0);
        }

        /// <summary>
        /// Creates a new <see cref="RawCoordinateSequence"/> that uses the given array for reading
        /// and writing X, Y, and Z data ignoring the <see cref="Ordinates"/> flags that were passed
        /// into the constructor for this factory instance.
        /// </summary>
        /// <param name="xyz">
        /// An array of X, Y, and Z values, laid out as
        /// <c>[x0, y0, z0, x1, y1, z1, x2, y2, z2, ..., xn, yn, zn]</c>.
        /// </param>
        /// <returns>
        /// A <see cref="RawCoordinateSequence"/> instance that's backed by the given array.
        /// </returns>
        /// <remarks>
        /// The resulting instance is essentially a <see cref="PackedDoubleCoordinateSequence"/>
        /// with slightly more overhead, so the main reason to prefer this over that one would be if
        /// you <b>really</b> need to avoid copying the data to fit it into that format.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the length of <paramref name="xyz"/> is not a multiple of 3.
        /// </exception>
        public static RawCoordinateSequence CreateXYZ(Memory<double> xyz)
        {
            if (xyz.Length % 3 != 0)
            {
                throw new ArgumentException("Length must be a multiple of 3.", nameof(xyz));
            }

            Memory<double>[] rawData = { xyz };
            (int RawDataIndex, int DimensionIndex)[] dimensionMap = { (0, 0), (0, 1), (0, 2) };
            return new RawCoordinateSequence(rawData, dimensionMap, measures: 0);
        }

        /// <summary>
        /// Creates a new <see cref="RawCoordinateSequence"/> that uses the given arrays for reading
        /// and writing X, Y, and M data ignoring the <see cref="Ordinates"/> flags that were passed
        /// into the constructor for this factory instance.
        /// </summary>
        /// <param name="x">
        /// An array of X values, laid out as
        /// <c>[x0, x1, x2, ..., xn]</c>.
        /// </param>
        /// <param name="y">
        /// An array of Y values, laid out as
        /// <c>[y0, y1, y2, ..., yn]</c>.
        /// </param>
        /// <param name="m">
        /// An array of M values, laid out as
        /// <c>[m0, m1, m2, ..., mn]</c>.
        /// </param>
        /// <returns>
        /// A <see cref="RawCoordinateSequence"/> instance that's backed by the given arrays.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the input arrays do not contain data for the same number of coordinates.
        /// </exception>
        public static RawCoordinateSequence CreateXYM(Memory<double> x, Memory<double> y, Memory<double> m)
        {
            if (x.Length != y.Length || x.Length != m.Length)
            {
                throw new ArgumentException("Arrays must contain data for the same number of coordinates.");
            }

            Memory<double>[] rawData = { x, y, m };
            (int RawDataIndex, int DimensionIndex)[] dimensionMap = { (0, 0), (1, 0), (2, 0) };
            return new RawCoordinateSequence(rawData, dimensionMap, measures: 1);
        }

        /// <summary>
        /// Creates a new <see cref="RawCoordinateSequence"/> that uses the given array for reading
        /// and writing X, Y, and M data ignoring the <see cref="Ordinates"/> flags that were passed
        /// into the constructor for this factory instance.
        /// </summary>
        /// <param name="xy">
        /// An array of X and Y values, laid out as
        /// <c>[x0, y0, x1, y1, x2, y2, ..., xn, yn]</c>.
        /// </param>
        /// <param name="m">
        /// An array of M values, laid out as
        /// <c>[m0, m1, m2, ..., mn]</c>.
        /// </param>
        /// <returns>
        /// A <see cref="RawCoordinateSequence"/> instance that's backed by the given array.
        /// </returns>
        /// <remarks>
        /// The resulting instance is essentially a <see cref="DotSpatialAffineCoordinateSequence"/>
        /// with slightly more overhead, so the main reason to prefer this over that one would be if
        /// you <b>really</b> need to avoid copying the data to fit it into that format.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the length of <paramref name="xy"/> is not a multiple of 2, or when the
        /// input arrays do not contain data for the same number of coordinates.
        /// </exception>
        public static RawCoordinateSequence CreateXYM(Memory<double> xy, Memory<double> m)
        {
            if (xy.Length % 2 != 0)
            {
                throw new ArgumentException("Length must be a multiple of 2.", nameof(xy));
            }

            if (xy.Length != m.Length * 2)
            {
                throw new ArgumentException("Arrays must contain data for the same number of coordinates.");
            }

            Memory<double>[] rawData = { xy, m };
            (int RawDataIndex, int DimensionIndex)[] dimensionMap = { (0, 0), (0, 1), (1, 0) };
            return new RawCoordinateSequence(rawData, dimensionMap, measures: 1);
        }

        /// <summary>
        /// Creates a new <see cref="RawCoordinateSequence"/> that uses the given array for reading
        /// and writing X, Y, and M data ignoring the <see cref="Ordinates"/> flags that were passed
        /// into the constructor for this factory instance.
        /// </summary>
        /// <param name="xym">
        /// An array of X, Y, and M values, laid out as
        /// <c>[x0, y0, m0, x1, y1, m1, x2, y2, m2, ..., xn, yn, mn]</c>.
        /// </param>
        /// <returns>
        /// A <see cref="RawCoordinateSequence"/> instance that's backed by the given array.
        /// </returns>
        /// <remarks>
        /// The resulting instance is essentially a <see cref="PackedDoubleCoordinateSequence"/>
        /// with slightly more overhead, so the main reason to prefer this over that one would be if
        /// you <b>really</b> need to avoid copying the data to fit it into that format.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the length of <paramref name="xym"/> is not a multiple of 3.
        /// </exception>
        public static RawCoordinateSequence CreateXYM(Memory<double> xym)
        {
            if (xym.Length % 3 != 0)
            {
                throw new ArgumentException("Length must be a multiple of 3.", nameof(xym));
            }

            Memory<double>[] rawData = { xym };
            (int RawDataIndex, int DimensionIndex)[] dimensionMap = { (0, 0), (0, 1), (0, 2) };
            return new RawCoordinateSequence(rawData, dimensionMap, measures: 1);
        }

        /// <summary>
        /// Creates a new <see cref="RawCoordinateSequence"/> that uses the given arrays for reading
        /// and writing X, Y, Z, and M data ignoring the <see cref="Ordinates"/> flags that were passed
        /// into the constructor for this factory instance.
        /// </summary>
        /// <param name="x">
        /// An array of X values, laid out as
        /// <c>[x0, x1, x2, ..., xn]</c>.
        /// </param>
        /// <param name="y">
        /// An array of Y values, laid out as
        /// <c>[y0, y1, y2, ..., yn]</c>.
        /// </param>
        /// <param name="z">
        /// An array of Z values, laid out as
        /// <c>[z0, z1, z2, ..., zn]</c>.
        /// </param>
        /// <param name="m">
        /// An array of M values, laid out as
        /// <c>[m0, m1, m2, ..., mn]</c>.
        /// </param>
        /// <returns>
        /// A <see cref="RawCoordinateSequence"/> instance that's backed by the given arrays.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the input arrays do not contain data for the same number of coordinates.
        /// </exception>
        public static RawCoordinateSequence CreateXYZM(Memory<double> x, Memory<double> y, Memory<double> z, Memory<double> m)
        {
            if (x.Length != y.Length || x.Length != z.Length || x.Length != m.Length)
            {
                throw new ArgumentException("Arrays must contain data for the same number of coordinates.");
            }

            Memory<double>[] rawData = { x, y, z, m };
            (int RawDataIndex, int DimensionIndex)[] dimensionMap = { (0, 0), (1, 0), (2, 0), (3, 0) };
            return new RawCoordinateSequence(rawData, dimensionMap, measures: 1);
        }

        /// <summary>
        /// Creates a new <see cref="RawCoordinateSequence"/> that uses the given array for reading
        /// and writing X, Y, Z, and M data ignoring the <see cref="Ordinates"/> flags that were passed
        /// into the constructor for this factory instance.
        /// </summary>
        /// <param name="xy">
        /// An array of X and Y values, laid out as
        /// <c>[x0, y0, x1, y1, x2, y2, ..., xn, yn]</c>.
        /// </param>
        /// <param name="z">
        /// An array of Z values, laid out as
        /// <c>[z0, z1, z2, ..., zn]</c>.
        /// </param>
        /// <param name="m">
        /// An array of M values, laid out as
        /// <c>[m0, m1, m2, ..., mn]</c>.
        /// </param>
        /// <returns>
        /// A <see cref="RawCoordinateSequence"/> instance that's backed by the given array.
        /// </returns>
        /// <remarks>
        /// The resulting instance is essentially a <see cref="DotSpatialAffineCoordinateSequence"/>
        /// with slightly more overhead, so the main reason to prefer this over that one would be if
        /// you <b>really</b> need to avoid copying the data to fit it into that format.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the length of <paramref name="xy"/> is not a multiple of 2, or when the
        /// input arrays do not contain data for the same number of coordinates.
        /// </exception>
        public static RawCoordinateSequence CreateXYZM(Memory<double> xy, Memory<double> z, Memory<double> m)
        {
            if (xy.Length % 2 != 0)
            {
                throw new ArgumentException("Length must be a multiple of 2.", nameof(xy));
            }

            if (xy.Length != z.Length * 2 || z.Length != m.Length)
            {
                throw new ArgumentException("Arrays must contain data for the same number of coordinates.");
            }

            Memory<double>[] rawData = { xy, z, m };
            (int RawDataIndex, int DimensionIndex)[] dimensionMap = { (0, 0), (0, 1), (1, 0), (2, 0) };
            return new RawCoordinateSequence(rawData, dimensionMap, measures: 1);
        }

        /// <summary>
        /// Creates a new <see cref="RawCoordinateSequence"/> that uses the given array for reading
        /// and writing X, Y, Z, and M data ignoring the <see cref="Ordinates"/> flags that were passed
        /// into the constructor for this factory instance.
        /// </summary>
        /// <param name="xyzm">
        /// An array of X, Y, Z, and M values, laid out as
        /// <c>[x0, y0, z0, m0, x1, y1, z1, m1, x2, y2, z2, m2, ..., xn, yn, zn, mn]</c>.
        /// </param>
        /// <returns>
        /// A <see cref="RawCoordinateSequence"/> instance that's backed by the given array.
        /// </returns>
        /// <remarks>
        /// The resulting instance is essentially a <see cref="PackedDoubleCoordinateSequence"/>
        /// with slightly more overhead, so the main reason to prefer this over that one would be if
        /// you <b>really</b> need to avoid copying the data to fit it into that format.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the length of <paramref name="xyzm"/> is not a multiple of 4.
        /// </exception>
        public static RawCoordinateSequence CreateXYZM(Memory<double> xyzm)
        {
            if (xyzm.Length % 4 != 0)
            {
                throw new ArgumentException("Length must be a multiple of 4.", nameof(xyzm));
            }

            Memory<double>[] rawData = { xyzm };
            (int RawDataIndex, int DimensionIndex)[] dimensionMap = { (0, 0), (0, 1), (0, 2), (0, 3) };
            return new RawCoordinateSequence(rawData, dimensionMap, measures: 1);
        }

        /// <inheritdoc />
        public override CoordinateSequence Create(int size, int dimension, int measures)
        {
            int spatial = dimension - measures;
            var ordinatesInGroups = _ordinatesInGroups;
            var ordinatesInResult = Ordinates.None;
            double[] underlyingData = new double[size * dimension];
            var rawDataList = new List<Memory<double>>(dimension);
            var remainingRawData = underlyingData.AsMemory();
            var dimensionMap = new (int RawDataIndex, int DimensionIndex)[dimension];

            for (int i = 0; i < spatial; i++)
            {
                if (i <= 16)
                {
                    var flag = (Ordinates)((int)Ordinates.Spatial1 << i);
                    ordinatesInResult |= flag;
                    if ((ordinatesInGroups & flag) != Ordinates.None)
                    {
                        continue;
                    }
                }

                dimensionMap[i].RawDataIndex = rawDataList.Count;
                rawDataList.Add(remainingRawData.Slice(0, size));
                remainingRawData = remainingRawData.Slice(size);
            }

            for (int i = 0; i < measures; i++)
            {
                if (i <= 16)
                {
                    var flag = (Ordinates)((int)Ordinates.Measure1 << i);
                    ordinatesInResult |= flag;
                    if ((ordinatesInGroups & flag) != Ordinates.None)
                    {
                        continue;
                    }
                }

                dimensionMap[spatial + i].RawDataIndex = rawDataList.Count;
                rawDataList.Add(remainingRawData.Slice(0, size));
                remainingRawData = remainingRawData.Slice(size);
            }

            if ((ordinatesInResult & ordinatesInGroups) == Ordinates.None)
            {
                return new RawCoordinateSequence(rawDataList.ToArray(), dimensionMap, measures);
            }

            foreach (var overallOrdinateGroup in _ordinateGroups)
            {
                var ordinateGroup = overallOrdinateGroup & ordinatesInResult;
                if (ordinateGroup == Ordinates.None)
                {
                    continue;
                }

                int dimCountForGroup = 0;
                for (int i = 0; i < spatial && i < 16; i++)
                {
                    if ((ordinateGroup & (Ordinates)((int)Ordinates.Spatial1 << i)) == Ordinates.None)
                    {
                        continue;
                    }

                    dimensionMap[i].RawDataIndex = rawDataList.Count;
                    dimensionMap[i].DimensionIndex = dimCountForGroup++;
                }

                for (int i = 0; i < measures && i < 16; i++)
                {
                    if ((ordinateGroup & (Ordinates)((int)Ordinates.Measure1 << i)) == Ordinates.None)
                    {
                        continue;
                    }

                    dimensionMap[spatial + i].RawDataIndex = rawDataList.Count;
                    dimensionMap[spatial + i].DimensionIndex = dimCountForGroup++;
                }

                rawDataList.Add(remainingRawData.Slice(0, size * dimCountForGroup));
                remainingRawData = remainingRawData.Slice(size * dimCountForGroup);
            }

            return new RawCoordinateSequence(rawDataList.ToArray(), dimensionMap, measures);
        }
    }
}
