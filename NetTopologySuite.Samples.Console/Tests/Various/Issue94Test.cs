using NUnit.Framework;

namespace NetTopologySuite.Tests.Various
{
    using System;
    using GeoAPI.Geometries;
    using Geometries;

    [TestFixture]
    public class Issue94Test
    {
        private const string IgnoreString =
@"Martin Davis: This is a known issue.  It's not really a bug, it's a design limitation
caused by the fact that JTS[/NTS] uses double-precision floating point to
represent ordinate values.  This provides about 16 decimal digits of
precision.

The number in the bug report is 99999999999999982650000000000.0, which
has more than 16 digits of precision.   So this can't be represented
exactly in JTS[/NTS].

The only solution to this would be to use arbitrary precision
arithmetic.  This would be awkward and slow.

If this represents a real use case some other approach will need to be
taken to solve it.";

        [Test, Category("Issue94")]
        public void IntersectionWithLineCreatedWithSmallCoordinates()
        {
            PerformTest(100d);
        }

        [Test, Category("Issue94")]
        public void IntersectionWithLineCreatedWithNotSoSmallCoordinates()
        {
            PerformTest(999999999d);
        }

        [Test, Category("Issue94")]
        public void IntersectionWithLineCreatedWithBigCoordinates()
        {
            PerformTest(99999999999999d);
        }

        [Test, Category("Issue94"), Ignore(IgnoreString)]
        public void IntersectionWithLineCreatedWithBiggerCoordinates()
        {
            PerformTest(999999999999999d);
        }

        [Test, Category("Issue94"), Ignore(IgnoreString)]
        public void IntersectionWithLineCreatedWithLargeCoordinates()
        {
            // returns POINT (10 10.000000000000002) => Same as JTS
            PerformTest(99999999999999982650000000000d);
        }

        [Test, Category("Issue94"), Ignore(IgnoreString)]
        public void IntersectionWithLineCreatedWithLargestCoordinates()
        {
            // returns POINT (0 0) => Same as JTS
            PerformTest(Double.MaxValue);
        }

        private static void PerformTest(double value)
        {
            IGeometryFactory factory = GeometryFactory.Default;
            ILineString ls1 = factory.CreateLineString(new Coordinate[] { new Coordinate(0, 0), new Coordinate(50, 50) });
            ILineString ls2 = factory.CreateLineString(new Coordinate[] { new Coordinate(10, value), new Coordinate(10, -value) });
            IGeometry result = ls1.Intersection(ls2);
            IGeometry expected = factory.CreatePoint(new Coordinate(10, 10));
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}