using System;
using BenchmarkDotNet.Attributes;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Benchmark
{
    /// <summary>
    /// Compares creating a <see cref="LineString"/> from a slice of a larger coordinate array using
    /// <see cref="GeometryFactory.CreateLineString(ReadOnlySpan{Coordinate})"/> against the previous
    /// approach of materializing an intermediate array first.
    /// </summary>
    [MemoryDiagnoser]
    public class GeometryFactoryLineStringBenchmarks
    {
        private Coordinate[] _coordinates;
        private GeometryFactory _factory;

        [Params(10, 100, 1000)]
        public int PointCount { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _coordinates = new Coordinate[PointCount + 20];
            for (int i = 0; i < _coordinates.Length; i++)
            {
                _coordinates[i] = new Coordinate(i, i * 2);
            }

            _factory = NtsGeometryServices.Instance.CreateGeometryFactory();
        }

        [Benchmark(Baseline = true)]
        public int ExtractThenCreateLineString()
        {
            var slice = CoordinateArrays.Extract(_coordinates, 10, 10 + PointCount - 1);
            return _factory.CreateLineString(slice).NumPoints;
        }

        [Benchmark]
        public int CreateLineStringFromSpan()
        {
            return _factory.CreateLineString(new ReadOnlySpan<Coordinate>(_coordinates, 10, PointCount)).NumPoints;
        }
    }
}
