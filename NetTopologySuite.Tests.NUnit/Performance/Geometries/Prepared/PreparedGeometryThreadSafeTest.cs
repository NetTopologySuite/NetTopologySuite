using System;
using System.Threading;
using GeoAPI.Geometries;
using GeoAPI.Geometries.Prepared;
using NUnit.Framework;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;

namespace NetTopologySuite.Tests.NUnit.Performance.Geometries.Prepared
{
/**
 * 
 * 
 * @author Martin Davis
 *
 */
    /// <summary>
    /// Tests for race conditons in the PreparedGeometry classes.
    /// </summary>
    /// <author>Martin Davis</author>
    public class PreparedGeometryThreadSafeTest : ThreadTestCase
    {
        private int nPts = 1000;
        private readonly IGeometryFactory _factory = new GeometryFactory(new PrecisionModel(1.0));
        private int _numberOfTests = 100;
        private IPreparedGeometry _preparedGeometry;
        private IGeometry _geometry;

        public override void Setup()
        {
            var sinePoly = CreateSineStar(new Coordinate(0, 0), 100000.0, nPts);
            _preparedGeometry = PreparedGeometryFactory.Prepare(sinePoly);
            _geometry = CreateSineStar(new Coordinate(10, 10), 100000.0, 100);
            
            //This is mandatory for the test to be executed at all
            //pg.Intersects will fail on thread by creating the IndexedPointInAreaLocator
            var result = _preparedGeometry.Intersects(_geometry);
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
            get { return new[] {_preparedGeometry, _geometry.Clone() }; }
        }

        private static readonly object LockWriteLine = new object();
        public override ParameterizedThreadStart GetRunnable(int threadIndex)
        {
            return delegate(object parameter)
                {
                    var pa = (object[]) parameter;
                    var pg = (IPreparedGeometry) pa[0];
                    var g = (IGeometry) pa[1];

                    while (_numberOfTests > 0)
                    {
                        Interlocked.Decrement(ref _numberOfTests);
                        var intersects = pg.Intersects(g);
                        //var intersects = _numberOfTests >= 0;
                        lock (LockWriteLine)
                            Console.WriteLine("{0} {1} {2}", _numberOfTests, threadIndex, intersects);
                    }
                };
        }

        [Test]
        public void TestIntersectsThreadSafe()
        {
            ThreadTestRunner.Run(new PreparedGeometryThreadSafeTest());
        }
    }
}