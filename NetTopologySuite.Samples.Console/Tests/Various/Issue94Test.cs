namespace NetTopologySuite.Tests.Various
{
    using System;
    using GeoAPI.Geometries;
    using Geometries;
    using NUnit.Framework;

    [TestFixture]
    public class Issue94Test
    {
        [Test]
        public void IntersectionWithLineCreatedWithSmallCoordinates()
        {
            PerformTest(100d);         
        }

        [Test]
        public void IntersectionWithLineCreatedWithNotSoSmallCoordinates()
        {
            PerformTest(999999999d);
        }

        [Test]
        public void IntersectionWithLineCreatedWithBigCoordinates()
        {
            PerformTest(99999999999999d);
        }

        [Test]
        public void IntersectionWithLineCreatedWithBiggerCoordinates()
        {
            PerformTest(999999999999999d);
        }

        [Test]
        public void IntersectionWithLineCreatedWithLargeCoordinates()
        {
            // returns POINT (10 10.000000000000002) => Same as JTS
            PerformTest(99999999999999982650000000000d);            
        }

        [Test]
        public void IntersectionWithLineCreatedWithLargestCoordinates()
        {
            // returns POINT (0 0) => Same as JTS
            PerformTest(Double.MaxValue);
        }

        private static void PerformTest(double value) 
        {
            IGeometryFactory factory = GeometryFactory.Default;
            ILineString ls1 = factory.CreateLineString(new ICoordinate[] { new Coordinate(0, 0), new Coordinate(50, 50) });            
            ILineString ls2 = factory.CreateLineString(new ICoordinate[] { new Coordinate(10, value), new Coordinate(10, -value) });
            IGeometry result = ls1.Intersection(ls2);
            IGeometry expected = factory.CreatePoint(new Coordinate(10, 10));
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
