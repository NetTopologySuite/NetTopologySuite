using NetTopologySuite.Noding;
using NUnit.Framework;
using Coordinate = NetTopologySuite.Coordinates.Simple.Coordinate;

namespace NetTopologySuite.Tests.NUnit.Noding
{
    [TestFixture]
    public class SegmentPointComparatorTest
    {
        [Test]
        public void TestOctant0()
        {
            CheckNodePosition(Octants.Zero, 1, 1, 2, 2, -1);
            CheckNodePosition(Octants.Zero, 1, 0, 1, 1, -1);
        }

        private static void CheckNodePosition(Octants octant,
            double x0, double y0,
            double x1, double y1,
          int expectedPositionValue
          )
        {
            int posValue = SegmentPointComparator.Compare<Coordinate>(octant,
                GeometryUtils.CoordFac.Create(x0, y0),
                GeometryUtils.CoordFac.Create(x1, y1)
                );
            Assert.IsTrue(posValue == expectedPositionValue);
        }
    }
}