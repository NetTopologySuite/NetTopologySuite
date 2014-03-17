using System;
using System.Threading;
using GeoAPI.Geometries;
using GeoAPI.Geometries.Prepared;
using NUnit.Framework;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;

namespace NetTopologySuite.Tests.NUnit.Performance.Geometries.Prepared
{

    /// <summary>
    /// Tests for race conditons in the PreparedGeometry classes.
    /// </summary>
    /// <author>Martin Davis</author>
    public class PreparedGeometryThreadSafeTest : ThreadTestCase
    {
        private int nPts = 100000;
        private readonly IGeometryFactory _factory = new GeometryFactory(new PrecisionModel(1.0));
        private int _numberOfTests = 20;
        private IPreparedGeometry _preparedGeometry;

        public override void Setup()
        {
            var sinePoly = CreateSineStar(new Coordinate(0, 0), 100000.0, nPts);
            _preparedGeometry = PreparedGeometryFactory.Prepare(sinePoly);
            
            WaitHandles = new WaitHandle[ThreadTestRunner.DefaultThreadCount];
        }

        private IGeometry CreateSineStar(Coordinate origin, double size, int numberOfPoints)
        {
            var gsf = new NetTopologySuite.Geometries.Utilities.SineStarFactory(_factory)
                {Centre = origin, Size = size, NumPoints = numberOfPoints, ArmLengthRatio = 0.1, NumArms = 20};
            var poly = gsf.CreateSineStar();
            return poly;
        }

        public override object Argument
        {
            get { return _preparedGeometry; }
        }

        private static readonly Random Rnd = new Random();
        public override ParameterizedThreadStart GetRunnable(int threadIndex)
        {
            WaitHandles[threadIndex] = new AutoResetEvent(false);
            return delegate(object parameter)
                {
                    var pg = (IPreparedGeometry) parameter;

                    for (var i = 0; i < 20; i++)
                    {
                        var g = CreateSineStar(new Coordinate(Rnd.Next(-10, 10), Rnd.Next(-10, 10)), 100000.0, Rnd.Next(75, 110));
                        var intersects = pg.Intersects(g);
                        Console.WriteLine("ThreadId {0} Test {1} Result {2}", threadIndex, i, intersects);
                    }

                    ((AutoResetEvent) WaitHandles[threadIndex]).Set();
                };
        }

        [TestAttribute]
        public void TestIntersectsThreadSafe()
        {
            ThreadTestRunner.Run(new PreparedGeometryThreadSafeTest());
        }
    }
}