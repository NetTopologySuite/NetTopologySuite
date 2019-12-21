using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;

namespace FilterBench
{
    [GcServer(true)]
    public class Program
    {
        private LineString _pts;

        static void Main()
        {
            var p = new Program();

            var distinctResults = new HashSet<(double, double, double, double)>();
            foreach (string seqType in new[] { nameof(CoordinateArraySequence), nameof(PackedDoubleCoordinateSequence), nameof(PackedFloatCoordinateSequence), nameof(VectorCoordinateSequence) })
            {
                p.SequenceType = seqType;
                p.Init();

                distinctResults.Add(p.Agnostic_OneByOne());
                distinctResults.Add(p.Agnostic_Entire());
                distinctResults.Add(p.Fast_OneByOne());
                distinctResults.Add(p.Fast_Entire());
            }

            if (distinctResults.Count > 1)
            {
                throw new Exception("Results differ.");
            }

            BenchmarkRunner.Run<Program>();
        }

        [GlobalSetup]
        public void Init()
        {
            int dimension = 4;
            int measures = 1;
            CoordinateSequenceFactory seqFactory = null;
            switch (SequenceType)
            {
                case nameof(CoordinateArraySequence):
                    seqFactory = CoordinateArraySequenceFactory.Instance;
                    break;

                case nameof(PackedDoubleCoordinateSequence):
                    seqFactory = PackedCoordinateSequenceFactory.DoubleFactory;
                    break;

                case nameof(PackedFloatCoordinateSequence):
                    seqFactory = PackedCoordinateSequenceFactory.FloatFactory;
                    break;

                case nameof(VectorCoordinateSequence):
                    seqFactory = new VectorCoordinateSequenceFactory();
                    break;
            }

            var seq = seqFactory.Create(10_000_000, dimension, measures);
            for (int i = 0; i < seq.Count; i++)
            {
                for (int j = 0; j < dimension; j++)
                {
                    seq.SetOrdinate(i, j, i + 1 + (((double)j) * seq.Count + i));
                }
            }

            var fac = new GeometryFactory(seqFactory);
            _pts = fac.CreateLineString(seq);
        }

        [Params(nameof(CoordinateArraySequence), nameof(PackedDoubleCoordinateSequence), nameof(PackedFloatCoordinateSequence), nameof(VectorCoordinateSequence))]
        public string SequenceType;

        [Benchmark(Baseline = true)]
        public (double x, double y, double z, double m) Agnostic_OneByOne()
        {
            var filter = new AgnosticOneByOneFilter();
            _pts.Apply(filter);
            return (filter.AverageX, filter.AverageY, filter.AverageZ, filter.AverageM);
        }

        [Benchmark]
        public (double x, double y, double z, double m) Agnostic_Entire()
        {
            var filter = new AgnosticEntireFilter();
            _pts.Apply(filter);
            return (filter.AverageX, filter.AverageY, filter.AverageZ, filter.AverageM);
        }

        [Benchmark]
        public (double x, double y, double z, double m) Fast_OneByOne()
        {
            var filter = new FastFilter();
            _pts.Apply((ICoordinateSequenceFilter)filter);
            return (filter.AverageX, filter.AverageY, filter.AverageZ, filter.AverageM);
        }

        [Benchmark]
        public (double x, double y, double z, double m) Fast_Entire()
        {
            var filter = new FastFilter();
            _pts.Apply((IEntireCoordinateSequenceFilter)filter);
            return (filter.AverageX, filter.AverageY, filter.AverageZ, filter.AverageM);
        }

        private sealed class AgnosticOneByOneFilter : ICoordinateSequenceFilter
        {
            private double _sumX;

            private double _sumY;

            private double _sumZ;

            private double _sumM;

            private int _cnt;

            public bool Done => false;

            public bool GeometryChanged => false;

            public double AverageX => _sumX / _cnt;

            public double AverageY => _sumY / _cnt;

            public double AverageZ => _sumZ / _cnt;

            public double AverageM => _sumM / _cnt;

            public void Filter(CoordinateSequence seq, int i)
            {
                _sumX += seq.GetX(i);
                _sumY += seq.GetY(i);
                _sumZ += seq.GetZ(i);
                _sumM += seq.GetM(i);
                ++_cnt;
            }
        }

        private sealed class AgnosticEntireFilter : IEntireCoordinateSequenceFilter
        {
            private double _sumX;

            private double _sumY;

            private double _sumZ;

            private double _sumM;

            private int _cnt;

            public bool Done => false;

            public bool GeometryChanged => false;

            public double AverageX => _sumX / _cnt;

            public double AverageY => _sumY / _cnt;

            public double AverageZ => _sumZ / _cnt;

            public double AverageM => _sumM / _cnt;

            public void Filter(CoordinateSequence seq)
            {
                for (int i = 0; i < seq.Count; i++)
                {
                    _sumX += seq.GetX(i);
                    _sumY += seq.GetY(i);
                    _sumZ += seq.GetZ(i);
                    _sumM += seq.GetM(i);
                    ++_cnt;
                }
            }
        }

        private sealed class FastFilter : ICoordinateSequenceFilter, IEntireCoordinateSequenceFilter
        {
            private double _sumX;

            private double _sumY;

            private double _sumZ;

            private double _sumM;

            private int _cnt;

            public bool Done => false;

            public bool GeometryChanged => false;

            public double AverageX => _sumX / _cnt;

            public double AverageY => _sumY / _cnt;

            public double AverageZ => _sumZ / _cnt;

            public double AverageM => _sumM / _cnt;

            public void Filter(CoordinateSequence seq, int i)
            {
                if (i == 0)
                {
                    Filter(seq);
                }
            }

            public void Filter(CoordinateSequence seq)
            {
                _cnt += seq.Count;
                switch (seq)
                {
                    case CoordinateArraySequence coordinateArraySequence:
                        Filter(coordinateArraySequence);
                        break;

                    case PackedDoubleCoordinateSequence packedDoubleCoordinateSequence:
                        Filter(packedDoubleCoordinateSequence);
                        break;

                    case PackedFloatCoordinateSequence packedFloatCoordinateSequence:
                        Filter(packedFloatCoordinateSequence);
                        break;

                    case VectorCoordinateSequence vectorCoordinateSequence:
                        Filter(vectorCoordinateSequence);
                        break;
                }
            }

            private void Filter(CoordinateArraySequence seq)
            {
                double sumX = 0;
                double sumY = 0;
                double sumZ = 0;
                double sumM = 0;
                foreach (var c in seq.ToCoordinateArray())
                {
                    sumX += c.X;
                    sumY += c.Y;
                    sumZ += c.Z;
                    sumM += c.M;
                }

                _sumX += sumX;
                _sumY += sumY;
                _sumZ += sumZ;
                _sumM += sumM;
            }

            private void Filter(PackedDoubleCoordinateSequence seq)
            {
                double sumX = 0;
                double sumY = 0;
                double sumZ = 0;
                double sumM = 0;

                int dim = seq.Dimension;
                int zOffset = seq.HasZ ? seq.ZOrdinateIndex : 0;
                int mOffset = seq.HasM ? seq.MOrdinateIndex : 0;

                double[] raw = seq.GetRawCoordinates();
                for (int i = 0; i < raw.Length; i += dim)
                {
                    sumX += raw[i + 0];
                    sumY += raw[i + 1];
                    sumZ += raw[i + zOffset];
                    sumM += raw[i + mOffset];
                }

                _sumX += sumX;
                _sumY += sumY;
                _sumZ = seq.HasZ ? _sumZ + sumZ : Coordinate.NullOrdinate;
                _sumM = seq.HasM ? _sumM + sumM : Coordinate.NullOrdinate;
            }

            private void Filter(PackedFloatCoordinateSequence seq)
            {
                double sumX = 0;
                double sumY = 0;
                double sumZ = 0;
                double sumM = 0;

                int dim = seq.Dimension;
                int zOffset = seq.HasZ ? seq.ZOrdinateIndex : 0;
                int mOffset = seq.HasM ? seq.MOrdinateIndex : 0;

                float[] raw = seq.GetRawCoordinates();
                for (int i = 0; i < raw.Length; i += dim)
                {
                    sumX += raw[i + 0];
                    sumY += raw[i + 1];
                    sumZ += raw[i + zOffset];
                    sumM += raw[i + mOffset];
                }

                _sumX += sumX;
                _sumY += sumY;
                _sumZ = seq.HasZ ? _sumZ + sumZ : Coordinate.NullOrdinate;
                _sumM = seq.HasM ? _sumM + sumM : Coordinate.NullOrdinate;
            }

            private void Filter(VectorCoordinateSequence seq)
            {
                if (seq.TryGetVals(Ordinate.X, out var xs))
                {
                    _sumX += ComputeVectorSum(xs);
                }

                if (seq.TryGetVals(Ordinate.Y, out var ys))
                {
                    _sumY += ComputeVectorSum(ys);
                }

                if (seq.TryGetVals(Ordinate.Z, out var zs))
                {
                    _sumZ += ComputeVectorSum(zs);
                }

                if (seq.TryGetVals(Ordinate.M, out var ms))
                {
                    _sumM += ComputeVectorSum(ms);
                }
            }

            private static unsafe double ComputeVectorSum(ReadOnlySpan<double> vals)
            {
                if (!Avx2.IsSupported)
                {
                    throw new PlatformNotSupportedException("I only bothered testing this on my machine at home...");
                }

                var sums = Vector256<double>.Zero;

                int endIndex = vals.Length - (vals.Length % Vector256<double>.Count);
                fixed (double* d = vals)
                {
                    for (double* cur = d, end = d + endIndex; cur < end; cur += Vector256<double>.Count)
                    {
                        sums = Avx.Add(sums, Avx.LoadVector256(cur));
                    }

                    double sum = 0;
                    for (int i = 0; i < Vector256<double>.Count; i++)
                    {
                        sum += sums.GetElement(i);
                    }

                    for (int i = endIndex; i < vals.Length; i++)
                    {
                        sum += vals[i];
                    }

                    return sum;
                }
            }
        }

        private sealed class VectorCoordinateSequence : CoordinateSequence
        {
            private readonly double[][] _coords;

            public VectorCoordinateSequence(int count, int dimension, int measures)
                : base(count, dimension, measures)
            {
                _coords = new double[dimension][];
                for (int i = 0; i < dimension; i++)
                {
                    _coords[i] = new double[count];
                }
            }

            public bool TryGetVals(int ordinateIndex, out Span<double> vals)
            {
                if ((uint)ordinateIndex < (uint)_coords.Length)
                {
                    vals = _coords[ordinateIndex];
                    return true;
                }

                vals = default;
                return false;
            }

            public bool TryGetVals(Ordinate ordinate, out Span<double> vals)
            {
                if (TryGetOrdinateIndex(ordinate, out int ordinateIndex))
                {
                    vals = _coords[ordinateIndex];
                    return true;
                }

                vals = default;
                return false;
            }

            public override CoordinateSequence Copy()
            {
                var result = new VectorCoordinateSequence(Count, Dimension, Measures);

                for (int i = 0; i < Dimension; i++)
                {
                    _coords[i].AsSpan().CopyTo(result._coords[i]);
                }

                return result;
            }

            public override double GetOrdinate(int index, int ordinateIndex) => _coords[ordinateIndex][index];

            public override void SetOrdinate(int index, int ordinateIndex, double value) => _coords[ordinateIndex][index] = value;
        }

        private sealed class VectorCoordinateSequenceFactory : CoordinateSequenceFactory
        {
            public override CoordinateSequence Create(int size, int dimension, int measures)
            {
                return new VectorCoordinateSequence(size, dimension, measures);
            }
        }
    }
}
