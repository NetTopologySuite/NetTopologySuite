using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixture]
    public class NonRobustLineIntersectorTest
    {
        private NonRobustLineIntersector li = new NonRobustLineIntersector();

        [Test]
        [Ignore("The JTS testNegativeZero test was being ignored")]
        public void TestNegativeZero() {
        //MD suggests we ignore this issue for now.
        //    li.computeIntersection(new Coordinate(220, 260), new Coordinate(220, 0),
        //        new Coordinate(220, 0), new Coordinate(100, 0));
        //    assertEquals((new Coordinate(220, 0)).ToString(), li.getIntersection(0).ToString());
        }
        [Test]
        [Ignore("The JTS testGetIntersectionNum test was being ignored")]
        public void TestGetIntersectionNum() {
        //MD: NonRobustLineIntersector may have different semantics for
        //getIntersectionNumber
        //    li.computeIntersection(new Coordinate(220, 0), new Coordinate(110, 0),
        //        new Coordinate(0, 0), new Coordinate(110, 0));
        //    assertEquals(1, li.getIntersectionNum());
        }
    }
}