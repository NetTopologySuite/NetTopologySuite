using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixture]
    public class SimpleRayCrossingStressTest
    {
        PrecisionModel pmFixed_1 = new PrecisionModel(1.0);

        [Test]
        public void TestGrid()
        {
            // Use fixed PM to try and get at least some points hitting the boundary
            GeometryFactory geomFactory = new GeometryFactory(pmFixed_1);
            //		GeometryFactory geomFactory = new GeometryFactory();
		
            PerturbedGridPolygonBuilder gridBuilder = new PerturbedGridPolygonBuilder(geomFactory);
            gridBuilder.NumLines = 20;
            gridBuilder.LineWidth = 10.0;
            IGeometry area = gridBuilder.Geometry;
		
            SimpleRayCrossingPointInAreaLocator pia = new SimpleRayCrossingPointInAreaLocator(area); 

            PointInAreaStressTester gridTester = new PointInAreaStressTester(geomFactory, area);
            gridTester.NumPoints = 100000;
            gridTester.TestPointInAreaLocator = pia;
		
            bool isCorrect = gridTester.Run();
            Assert.IsTrue(isCorrect);
        }
	
        class SimpleRayCrossingPointInAreaLocator : IPointOnGeometryLocator
        {
            private IGeometry geom;
		
            public SimpleRayCrossingPointInAreaLocator(IGeometry geom)
            {
                this.geom = geom;
            }
		
            public Location Locate(Coordinate p)
            {
                RayCrossingCounter rcc = new RayCrossingCounter(p);
                RayCrossingSegmentFilter filter = new RayCrossingSegmentFilter(rcc);
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
		
                public void Filter(ICoordinateSequence seq, int i)
                {
                    if (i == 0) return;
                    seq.GetCoordinate(i - 1, p0);
                    seq.GetCoordinate(i, p1);
                    rcc.CountSegment(p0, p1);
                }

                public bool Done
                {
                    get { return rcc.IsOnSegment; }
                }

                public bool GeometryChanged
                {
                    get { return false; }
                }
            }
        }
    }
}


