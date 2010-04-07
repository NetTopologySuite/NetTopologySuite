using GeoAPI.DataStructures;
using NUnit.Framework;
namespace NetTopologySuite.Tests.NUnit.Index
{
    [TestFixture]
    public class IntervalTest
    {
        [Test]
        public void TestIntersectsBasic()
        {
            Assert.IsTrue(new Interval(5, 10).Intersects(new Interval(7, 12)));
            Assert.IsTrue(new Interval(7, 12).Intersects(new Interval(5, 10)));
            Assert.IsTrue(!new Interval(5, 10).Intersects(new Interval(11, 12)));
            Assert.IsTrue(!new Interval(11, 12).Intersects(new Interval(5, 10)));
            Assert.IsTrue(new Interval(5, 10).Intersects(new Interval(10, 12)));
            Assert.IsTrue(new Interval(10, 12).Intersects(new Interval(5, 10)));
        }

        [Test]
        public void TestIntersectsZeroWidthInterval()
        {
            Assert.IsTrue(new Interval(10, 10).Intersects(new Interval(7, 12)));
            Assert.IsTrue(new Interval(7, 12).Intersects(new Interval(10, 10)));
            Assert.IsTrue(!new Interval(10, 10).Intersects(new Interval(11, 12)));
            Assert.IsTrue(!new Interval(11, 12).Intersects(new Interval(10, 10)));
            Assert.IsTrue(new Interval(10, 10).Intersects(new Interval(10, 12)));
            Assert.IsTrue(new Interval(10, 12).Intersects(new Interval(10, 10)));
        }

        [Test]
        public void TestCopyConstructor()
        {
            Assert.AreEqual(new Interval(3, 4), new Interval(3, 4));
            Assert.AreEqual(new Interval(3, 4), new Interval(new Interval(3, 4)));
        }

        [Test]
        public void TestGetCentre()
        {
            Assert.AreEqual(6.5, new Interval(4, 9).Center, 1E-10);
        }

        [Test]
        public void TestExpandToInclude()
        {
            Assert.AreEqual(new Interval(3, 8), new Interval(3, 4)
                         .ExpandToInclude(new Interval(7, 8)));
            Assert.AreEqual(new Interval(3, 7), new Interval(3, 7)
                         .ExpandToInclude(new Interval(4, 5)));
            Assert.AreEqual(new Interval(3, 8), new Interval(3, 7)
                         .ExpandToInclude(new Interval(4, 8)));
        }
    }
}