using System;
using BenchmarkDotNet.Attributes;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;

namespace NetTopologySuite.Benchmark
{
    /// <summary>
    /// A factory that does not override <see cref="CoordinateSequenceFactory.Create(ReadOnlySpan{Coordinate})"/>,
    /// so it exercises the generic <c>SetOrdinate</c>-based loop in the base class -- the code path used
    /// before the built-in factories gained specialized span overrides.
    /// </summary>
    internal sealed class GenericFallbackCoordinateSequenceFactory : CoordinateSequenceFactory
    {
        private readonly PackedCoordinateSequenceFactory _inner = PackedCoordinateSequenceFactory.DoubleFactory;

        public override CoordinateSequence Create(int size, int dimension, int measures) =>
            _inner.Create(size, dimension, measures);
    }

    /// <summary>
    /// Compares the generic base-class <c>Create(ReadOnlySpan&lt;Coordinate&gt;)</c> implementation
    /// against <see cref="PackedCoordinateSequenceFactory"/>'s specialized override, for equivalent input.
    /// </summary>
    [MemoryDiagnoser]
    public class CoordinateSequenceFactoryOverrideBenchmarks
    {
        private Coordinate[] _coordinates;
        private readonly GenericFallbackCoordinateSequenceFactory _generic = new GenericFallbackCoordinateSequenceFactory();
        private readonly PackedCoordinateSequenceFactory _packed = PackedCoordinateSequenceFactory.DoubleFactory;

        [Params(10, 100, 1000)]
        public int Count { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _coordinates = new Coordinate[Count];
            for (int i = 0; i < Count; i++)
            {
                _coordinates[i] = new Coordinate(i, i * 2);
            }
        }

        [Benchmark(Baseline = true)]
        public int GenericBaseImplementation() => _generic.Create((ReadOnlySpan<Coordinate>)_coordinates).Count;

        [Benchmark]
        public int SpecializedPackedOverride() => _packed.Create((ReadOnlySpan<Coordinate>)_coordinates).Count;
    }
}
