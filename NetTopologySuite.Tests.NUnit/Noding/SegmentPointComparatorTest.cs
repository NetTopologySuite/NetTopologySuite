using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Noding;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Noding
{
    /// <summary>
    /// Test IntersectionSegment#compareNodePosition
    /// </summary>
    [TestFixtureAttribute]
    public class SegmentPointComparatorTest
    {
        [TestAttribute]
        public void TestOctant0()
        {
            checkNodePosition(Octants.Zero, 1, 1, 2, 2, -1);
            checkNodePosition(Octants.Zero, 1, 0, 1, 1, -1);
        }

        private void checkNodePosition(Octants octant,
            double x0, double y0,
            double x1, double y1,
            int expectedPositionValue
            )
        {
            int posValue = SegmentPointComparator.Compare(octant,
                new Coordinate(x0, y0),
                new Coordinate(x1, y1)
                );
            Assert.IsTrue(posValue == expectedPositionValue);
        }
    }
}
