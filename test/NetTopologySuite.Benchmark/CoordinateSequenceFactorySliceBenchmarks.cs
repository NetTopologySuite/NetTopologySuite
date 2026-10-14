using System;
using BenchmarkDotNet.Attributes;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;

namespace NetTopologySuite.Benchmark
{
    /// <summary>
    /// Compares building a <see cref="CoordinateSequence"/> from a slice of a larger array using
    /// the new <see cref="CoordinateSequenceFactory.Create(ReadOnlySpan{Coordinate})"/> overload
    /// against the previous approach of materializing an intermediate array first
    /// (<see cref="CoordinateArrays.Extract(Coordinate[], int, int)"/>).
    /// </summary>
    [MemoryDiagnoser]
    public class CoordinateSequenceFactorySliceBenchmarks
    {
        private Coordinate[] _coordinates;

        [Params(10, 100, 1000)]
        public int SliceLength { get; set; }

        [ParamsSource(nameof(Factories))]
        public CoordinateSequenceFactory Factory { get; set; }

        public static CoordinateSequenceFactory[] Factories { get; } =
        {
            CoordinateArraySequenceFactory.Instance,
            PackedCoordinateSequenceFactory.DoubleFactory,
            DotSpatialAffineCoordinateSequenceFactory.Instance,
        };

        [GlobalSetup]
        public void Setup()
        {
            _coordinates = new Coordinate[SliceLength + 20];
            for (int i = 0; i < _coordinates.Length; i++)
            {
                _coordinates[i] = new Coordinate(i, i * 2);
            }
        }

        [Benchmark(Baseline = true)]
        public int ExtractThenCreate()
        {
            var extracted = CoordinateArrays.Extract(_coordinates, 10, 10 + SliceLength - 1);
            return Factory.Create(extracted).Count;
        }

        [Benchmark]
        public int CreateFromSpan()
        {
            var span = new ReadOnlySpan<Coordinate>(_coordinates, 10, SliceLength);
            return Factory.Create(span).Count;
        }
    }
}
