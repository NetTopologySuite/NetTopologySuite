#nullable disable
using NetTopologySuite.Algorithm;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
using NetTopologySuite.Tests.NUnit.Algorithm;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Algorithm
{
    [TestFixture]
    public class SimpleRayCrossingStressTest
    {
        PrecisionModel pmFixed_1 = new PrecisionModel(1.0);

        [Test]
        [Category("Stress")]
        public void TestGrid()
        {
            // Use fixed PM to try and get at least some points hitting the boundary
            var geomFactory = new GeometryFactory(pmFixed_1);
            // GeometryFactory geomFactory = new GeometryFactory();

            var gridBuilder = new PerturbedGridPolygonBuilder(geomFactory);
            gridBuilder.NumLines = 20;
            gridBuilder.LineWidth = 10.0;
            var area = gridBuilder.Geometry;

            var pia = new SimpleRayCrossingPointInAreaLocator(area);

            var gridTester = new PointInAreaStressTester(geomFactory, area);
            gridTester.NumPoints = 100000;
            gridTester.TestPointInAreaLocator = pia;

            bool isCorrect = gridTester.Run();
            Assert.IsTrue(isCorrect);
        }

        class SimpleRayCrossingPointInAreaLocator : IPointOnGeometryLocator
        {
            private Geometry geom;

            public SimpleRayCrossingPointInAreaLocator(Geometry geom)
            {
                this.geom = geom;
            }

            public Location Locate(Coordinate p)
            {
                var rcc = new RayCrossingCounter(p);
                var filter = new RayCrossingSegmentFilter(rcc);
                geom.Apply(filter);
                return rcc.Location;
            }

            class RayCrossingSegmentFilter : ICoordinateSequenceFilter
            {
                private RayCrossingCounter rcc;
                private Coordinate p0 = new Coordinate();
                private Coordinate p1 = new Coordinate();

                public RayCrossingSegmentFilter(RayCrossingCounter rcc)
                {
                    this.rcc = rcc;
                }

                public void Filter(CoordinateSequence seq, int i)
                {
                    if (i == 0) return;
                    seq.GetCoordinate(i - 1, p0);
                    seq.GetCoordinate(i, p1);
                    rcc.CountSegment(p0, p1);
                }

                public bool Done => rcc.IsOnSegment;

                public bool GeometryChanged => false;
            }
        }
    }
}

