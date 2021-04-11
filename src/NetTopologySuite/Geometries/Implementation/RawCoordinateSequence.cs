using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace NetTopologySuite.Geometries.Implementation
{
    /// <summary>
    /// An implementation of <see cref="CoordinateSequence"/> that packs its contents in a way that
    /// can be customized by the creator.
    /// </summary>
    [Serializable]
    public sealed class RawCoordinateSequence : CoordinateSequence, ISerializable
    {
        private readonly (Memory<double> Array, int DimensionCount)[] _rawData;

        private readonly (int RawDataIndex, int DimensionIndex)[] _dimensionMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="RawCoordinateSequence"/> class.
        /// </summary>
        /// <param name="rawData">
        /// Contains the raw data for this sequence.
        /// </param>
        /// <param name="dimensionMap">
        /// Contains a pair of indexes to tell us, for each dimension, where to find its data in
        /// <paramref name="rawData"/>.
        /// </param>
        /// <param name="measures">
        /// The value for <see cref="CoordinateSequence.Measures"/>.
        /// </param>
        public RawCoordinateSequence(Memory<double>[] rawData, (int RawDataIndex, int DimensionIndex)[] dimensionMap, int measures)
            : base(GetCountIfValid(rawData, dimensionMap), dimensionMap.Length, measures)
        {
            _rawData = new (Memory<double> Array, int DimensionCount)[rawData.Length];
            if (Count == 0)
            {
                foreach ((int rawDataIndex, int dimensionIndex) in dimensionMap)
                {
                    ref int dimensionCount = ref _rawData[rawDataIndex].DimensionCount;
                    if (dimensionCount <= dimensionIndex)
                    {
                        dimensionCount = dimensionIndex + 1;
                    }
                }

                for (int i = 0; i < rawData.Length; i++)
                {
                    _rawData[i].Array = rawData[i];
                }
            }
            else
            {
                for (int i = 0; i < rawData.Length; i++)
                {
                    _rawData[i].Array = rawData[i];
                    _rawData[i].DimensionCount = rawData[i].Length / Count;
                }
            }

            _dimensionMap = dimensionMap;
        }

        private RawCoordinateSequence(int count, int dimension, int measures, (Memory<double> Array, int DimensionCount)[] rawData, (int RawDataIndex, int DimensionIndex)[] dimensionMap)
            : base(count, dimension, measures)
        {
            _rawData = rawData;
            _dimensionMap = dimensionMap;
        }

        private RawCoordinateSequence(SerializationInfo info, StreamingContext context)
            : this(
                info.GetInt32("count"),
                info.GetInt32("dimension"),
                info.GetInt32("measures"),
                Array.ConvertAll(((double[] Array, int DimensionCount)[])info.GetValue("rawData", typeof((double[] Array, int DimensionCount)[])), tup => (tup.Array.AsMemory(), tup.DimensionCount)),
                ((int RawDataIndex, int DimensionIndex)[])info.GetValue("dimensionMap", typeof((int RawDataIndex, int DimensionIndex)[])))
        {
        }

        /// <summary>
        /// Gets the underlying <see cref="Memory{T}"/> for the ordinates at the given index, along
        /// with a "stride" value that represents how many slots there are between elements.
        /// </summary>
        /// <param name="ordinateIndex">
        /// The index of the ordinate whose values to get, from
        /// <see cref="CoordinateSequence.TryGetOrdinateIndex"/>.
        /// </param>
        /// <returns>
        /// The underlying <see cref="Memory{T}"/> and stride.
        /// </returns>
        /// <remarks>
        /// Assuming <see cref="CoordinateSequence.Count"/> is nonzero, the first element of the
        /// returned array holds the first coordinate's value for the requested ordinate, and the
        /// last element of the returned array holds the last coordinate's value for the requested
        /// ordinate.
        /// </remarks>
        public (Memory<double> Array, int Stride) GetRawCoordinatesAndStride(int ordinateIndex)
        {
            if ((uint)ordinateIndex >= (uint)_dimensionMap.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(ordinateIndex), ordinateIndex, $"Must be less than Dimension (use {nameof(TryGetOrdinateIndex)} to get the value to use for this if you are unsure).");
            }

            (int sourceIndex, int offset) = _dimensionMap[ordinateIndex];
            (var array, int stride) = _rawData[sourceIndex];
            if (!array.IsEmpty)
            {
                array = array.Slice(offset, array.Length - stride + 1);
            }

            return (array, stride);
        }

        /// <inheritdoc />
        public override double GetOrdinate(int index, int ordinateIndex)
        {
            return ValueRef(index, ordinateIndex);
        }

        /// <inheritdoc />
        public override void SetOrdinate(int index, int ordinateIndex, double value)
        {
            ValueRef(index, ordinateIndex) = value;
        }

        /// <inheritdoc />
        public override CoordinateSequence Copy()
        {
            var newRawData = _rawData.AsSpan().ToArray();

            for (int i = 0; i < newRawData.Length; i++)
            {
                newRawData[i].Array = newRawData[i].Array.ToArray();
            }

            var newDimensionMap = _dimensionMap.AsSpan().ToArray();
            return new RawCoordinateSequence(Count, Dimension, Measures, newRawData, newDimensionMap);
        }

        /// <inheritdoc />
        public override CoordinateSequence Reversed()
        {
            var result = (RawCoordinateSequence)Copy();

            foreach ((var array, int chunkSize) in result._rawData)
            {
                var span = array.Span;
                switch (chunkSize)
                {
                    case 1:
                        span.Reverse();
                        break;

                    case 2:
                        MemoryMarshal.Cast<double, (double, double)>(span).Reverse();
                        break;

                    case 3:
                        MemoryMarshal.Cast<double, (double, double, double)>(span).Reverse();
                        break;

                    case 4:
                        MemoryMarshal.Cast<double, (double, double, double, double)>(span).Reverse();
                        break;

                    default:
                        // those are the common sizes... we could keep going, but for the sake of
                        // keeping this smallish, just reverse the raw array and then reverse each
                        // individual coordinate's section within the array.
                        span.Reverse();
                        while (!span.IsEmpty)
                        {
                            span.Slice(0, chunkSize).Reverse();
                            span = span.Slice(chunkSize);
                        }

                        break;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public override Envelope ExpandEnvelope(Envelope env)
        {
            if (env is null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            (var xsMem, int strideX) = GetRawCoordinatesAndStride(0);
            (var ysMem, int strideY) = GetRawCoordinatesAndStride(1);
            var xs = xsMem.Span;
            var ys = ysMem.Span;
            for (int x = 0, y = 0; x < xs.Length; x += strideX, y += strideY)
            {
                env.ExpandToInclude(xs[x], ys[y]);
            }

            return env;
        }

        /// <inheritdoc />
        public override Coordinate[] ToCoordinateArray()
        {
            if (Count == 0)
            {
                return Array.Empty<Coordinate>();
            }

            var raw = new (ReadOnlyMemory<double> Memory, int Stride)[Dimension];
            var rawArrays = new (double[] Array, int Offset, int Stride)[Dimension];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = GetRawCoordinatesAndStride(i);

                if (rawArrays is null)
                {
                    continue;
                }

                if (MemoryMarshal.TryGetArray(raw[i].Memory, out var arraySegment))
                {
                    rawArrays[i].Array = arraySegment.Array;
                    rawArrays[i].Offset = arraySegment.Offset;
                    rawArrays[i].Stride = raw[i].Stride;
                }
                else
                {
                    rawArrays = null;
                }
            }

            var result = new Coordinate[Count];
            if (rawArrays != null)
            {
                raw = null;
                for (int i = 0; i < result.Length; i++)
                {
                    var coord = result[i] = CreateCoordinate();
                    for (int j = 0; j < rawArrays.Length; j++)
                    {
                        ref var nxt = ref rawArrays[j];
                        coord[j] = nxt.Array[nxt.Offset];
                        nxt.Offset += nxt.Stride;
                    }
                }
            }
            else
            {
                // xs and ys can be special
                var xs = raw[0].Memory.Span;
                int strideX = raw[0].Stride;
                var ys = raw[1].Memory.Span;
                int strideY = raw[1].Stride;

                int i = 0;
                while (true)
                {
                    var coord = result[i] = CreateCoordinate();
                    coord.X = xs[0];
                    coord.Y = ys[0];

                    for (int j = 2; j < raw.Length; j++)
                    {
                        coord[j] = raw[j].Memory.Span[0];
                    }

                    if (++i == result.Length)
                    {
                        break;
                    }

                    xs = xs.Slice(strideX);
                    ys = ys.Slice(strideY);
                    for (int j = 2; j < raw.Length; j++)
                    {
                        ref var nxt = ref raw[j];
                        nxt.Memory = nxt.Memory.Slice(nxt.Stride);
                    }
                }
            }

            return result;
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("count", Count);
            info.AddValue("dimension", Dimension);
            info.AddValue("measures", Measures);
            info.AddValue("rawData", Array.ConvertAll(_rawData, tup => (tup.Array.ToArray(), tup.DimensionCount)));
            info.AddValue("dimensionMap", _dimensionMap);
        }

        private static int GetCountIfValid(Memory<double>[] rawData, (int RawDataIndex, int DimensionIndex)[] dimensionMap)
        {
            if (rawData is null)
            {
                throw new ArgumentNullException(nameof(rawData));
            }

            if (dimensionMap is null)
            {
                throw new ArgumentNullException(nameof(dimensionMap));
            }

            int dimensionCount = dimensionMap.Length;
            if (dimensionCount == 0)
            {
                // base class requires at least 2 spatial dimensions, so it'll throw for us.
                return 0;
            }

            int valueCount = 0;
            foreach (var array in rawData)
            {
                valueCount += array.Length;
            }

            int count = Math.DivRem(valueCount, dimensionCount, out int remainder);
            if (remainder != 0)
            {
                throw new ArgumentException("The sum of all array sizes must be an even multiple of the number of dimensions.");
            }

            if (count == 0)
            {
                // validation for empty is a lot different, because we can't start from the number
                // of slots in the arrays of rawData.
                ValidateEmpty(rawData, dimensionMap);
                return count;
            }

            var scratchIntBuffer = rawData.Length < 20
                ? stackalloc int[rawData.Length * 2]
                : new int[rawData.Length * 2];

            // for each array in rawData, calculate two values: the first value tells us the number
            // of dimensions in the arrays before it, and the second value tells us the number of
            // dimensions in that array itself.
            var dimensionsBySourceArray = MemoryMarshal.Cast<int, (int DimensionOffset, int DimensionCount)>(scratchIntBuffer);

            int dimensionsSoFar = 0;
            for (int i = 0; i < rawData.Length; i++)
            {
                dimensionsBySourceArray[i].DimensionOffset = dimensionsSoFar;
                dimensionsSoFar += dimensionsBySourceArray[i].DimensionCount = rawData[i].Length / count;
            }

            if (dimensionsSoFar != dimensionCount)
            {
                throw new ArgumentException("Inferred dimension count from raw data does not match the number of entries in dimension map.");
            }

            // now check to ensure that each dimension is actually represented in one (and only one)
            // slot in rawData.  those counts that we just computed will let us check each dimension
            // very quickly, using just a single array lookup and some comparisons.
            var slotIsUsed = dimensionCount < 20
                ? stackalloc bool[dimensionCount]
                : new bool[dimensionCount];
            slotIsUsed.Clear();
            int usedSlotCount = 0;

            foreach ((int rawDataIndex, int dimensionIndex) in dimensionMap)
            {
                if ((uint)rawDataIndex >= (uint)dimensionsBySourceArray.Length)
                {
                    throw new ArgumentException("Raw data index in dimension map must be less than the length of raw data.");
                }

                (int dimensionOffsetInSourceArray, int dimensionCountInSourceArray) = dimensionsBySourceArray[rawDataIndex];
                if ((uint)dimensionIndex >= (uint)dimensionCountInSourceArray)
                {
                    throw new ArgumentException("Dimension index in dimension map must be a positive value that's less than the number of dimensions in the corresponding raw data slot.");
                }

                int slotIndex = dimensionOffsetInSourceArray + dimensionIndex;
                if (slotIsUsed[slotIndex])
                {
                    throw new ArgumentException("Dimension map contains duplicate values.", nameof(dimensionMap));
                }

                slotIsUsed[slotIndex] = true;
                ++usedSlotCount;
            }

            if (usedSlotCount != dimensionCount)
            {
                throw new ArgumentException("Dimension map does not cover all slots in raw data.");
            }

            return count;
        }

        private static void ValidateEmpty(Memory<double>[] rawData, (int RawDataIndex, int DimensionIndex)[] dimensionMap)
        {
            int dimensionCount = dimensionMap.Length;

            var slotUsed = rawData.Length * dimensionCount < 400
                ? stackalloc bool[rawData.Length * dimensionCount]
                : new bool[rawData.Length * dimensionCount];
            slotUsed.Clear();

            foreach ((int rawDataIndex, int dimensionIndex) in dimensionMap)
            {
                if ((uint)rawDataIndex >= (uint)rawData.Length)
                {
                    throw new ArgumentException("Raw data index in dimension map must be less than the length of raw data.");
                }

                int slotIndex = (rawDataIndex * dimensionCount) + dimensionIndex;
                if (slotUsed[slotIndex])
                {
                    throw new ArgumentException("Dimension map contains duplicate values.", nameof(dimensionMap));
                }

                slotUsed[slotIndex] = true;
            }

            int inferredDimensionCount = 0;
            for (int i = 0; i < rawData.Length; i++)
            {
                int baseSlot = dimensionCount * i;
                for (int j = 0; j < dimensionCount; j++)
                {
                    if (slotUsed[baseSlot + j])
                    {
                        ++inferredDimensionCount;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (inferredDimensionCount != dimensionCount)
            {
                throw new ArgumentException("Dimension map does not cover all slots in raw data.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref double ValueRef(int index, int ordinateIndex)
        {
            (int sourceIndex, int offset) = _dimensionMap[ordinateIndex];
            (var array, int stride) = _rawData[sourceIndex];
            return ref array.Span[(index * stride) + offset];
        }
    }
}
