using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    //Tests are exposed by AbstractPointInRingTest type
    public class RayCrossingCounterTest : AbstractPointInRingTest
    {
        private WKTReader reader = new WKTReader();

        protected override void RunPtInRing(Location expectedLoc, Coordinate pt, string wkt)
        {
            var geom = reader.Read(wkt);
            Assert.AreEqual(expectedLoc, RayCrossingCounter.LocatePointInRing(pt, geom.Coordinates));
            var poly = geom as Polygon;
            if (poly == null)
                return;

            Assert.AreEqual(expectedLoc, RayCrossingCounter.LocatePointInRing(pt, poly.ExteriorRing.CoordinateSequence));
        }

        [Test]
        public void TestRunPtInRing4d()
        {
            var cs = new PackedCoordinateSequenceFactory(PackedCoordinateSequenceFactory.PackedType.Double)
                .Create(new double[]{
                    0.0, 0.0, 0.0, 0.0,
                    10.0, 0.0, 0.0, 0.0,
                    5.0, 10.0, 0.0, 0.0,
                    0.0, 0.0, 0.0, 0.0
                }, 4, 1);
            Assert.AreEqual(Location.Interior, RayCrossingCounter.LocatePointInRing(new Coordinate(5.0, 2.0), cs));
        }


    }

}
