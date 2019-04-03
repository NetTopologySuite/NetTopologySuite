using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Precision;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Algorithm
{
    /// <summary>
    /// An example of the usage of the <see cref="PerformanceTestRunner"/>.
    /// </summary>
    public class InteriorPointAreaPerfTest : PerformanceTestCase
    {
        private const int NumberOfIterations = 100;

        private static double OriginX = 100;
        private static double OriginY = 100;
        private static double Size = 100;
        private static int NumberOfArms = 20;
        private static double ArmRatio = 0.3;

        private IGeometry _sineStar;
        private IGeometry _sinePolyCrinkly;

        public InteriorPointAreaPerfTest()
            : base(nameof(InteriorPointAreaPerfTest))
        {
            this.RunSize = new[] { 10, 100, 1000, 10000, 100000, 1000000 };
            this.RunIterations = NumberOfIterations;
        }

        public override void SetUp()
        {
            Console.WriteLine("Interior Point Area perf test");
            Console.WriteLine($"SineStar: origin: ({OriginX}, {OriginY})  size: {Size}  # arms: {NumberOfArms}  arm ratio: {ArmRatio}");
            Console.WriteLine($"# Iterations: {NumberOfIterations}");
        }

        public override void StartRun(int npts)
        {
            _sineStar = SineStarFactory.Create(new Coordinate(OriginX, OriginY), Size, npts, NumberOfArms, ArmRatio);
            _sinePolyCrinkly = GeometryPrecisionReducer.Reduce(_sineStar, new PrecisionModel(Size));

            Console.WriteLine();
            Console.WriteLine($"Running with # pts {_sinePolyCrinkly.NumPoints}");
            ////if (npts <= 1000) Console.WriteLine(_sineStar);
        }

        public override void TestInternal()
        {
            PerformanceTestRunner.Run(typeof(InteriorPointAreaPerfTest));
        }

        public void RunTest1()
        {
            InteriorPointArea.GetInteriorPoint(_sinePolyCrinkly);
        }
    }
}
