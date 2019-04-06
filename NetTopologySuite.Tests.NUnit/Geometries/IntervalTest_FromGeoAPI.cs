using GeoAPI.DataStructures;
using NUnit.Framework;

namespace GeoAPI.Tests.DataStructures
{
    [TestFixture]
    public class IntervalTest
    {
        [Test]
        public void TestIntersectsBasic()
        {
            Assert.IsTrue(Interval.Create(5, 10).Overlaps(Interval.Create(7, 12)));
            Assert.IsTrue(Interval.Create(7, 12).Overlaps(Interval.Create(5, 10)));
            Assert.IsTrue(!Interval.Create(5, 10).Overlaps(Interval.Create(11, 12)));
            Assert.IsTrue(!Interval.Create(11, 12).Overlaps(Interval.Create(5, 10)));
            Assert.IsTrue(Interval.Create(5, 10).Overlaps(Interval.Create(10, 12)));
            Assert.IsTrue(Interval.Create(10, 12).Overlaps(Interval.Create(5, 10)));
        }

        [Test]
        public void TestIntersectsZeroWidthInterval()
        {
            Assert.IsTrue(Interval.Create(10).Overlaps(Interval.Create(7, 12)));
            Assert.IsTrue(Interval.Create(7, 12).Overlaps(Interval.Create(10)));
            Assert.IsTrue(!Interval.Create(10).Overlaps(Interval.Create(11, 12)));
            Assert.IsTrue(!Interval.Create(11, 12).Overlaps(Interval.Create(10)));
            Assert.IsTrue(Interval.Create(10).Overlaps(Interval.Create(10, 12)));
            Assert.IsTrue(Interval.Create(10, 12).Overlaps(Interval.Create(10)));
        }

        [Test]
        public void TestCopyConstructor()
        {
            Assert.IsTrue(IntervalsAreEqual(Interval.Create(3, 4), Interval.Create(3, 4)));
            Assert.IsTrue(IntervalsAreEqual(Interval.Create(3, 4), Interval.Create(Interval.Create(3, 4))));
        }

        [Test]
        public void TestCentre()
        {
            Assert.AreEqual(6.5, Interval.Create(4, 9).Centre, 1E-10);
        }

        [Test]
        public void TestExpandToInclude()
        {
            var expected = Interval.Create(3, 8);
            var actual = Interval.Create(3, 4);
            actual = actual.ExpandedByInterval(Interval.Create(7, 8));

            Assert.AreEqual(expected.Min, actual.Min);
            Assert.AreEqual(expected.Max, actual.Max);

            expected = Interval.Create(3, 7);
            actual = Interval.Create(3, 7);
            actual = actual.ExpandedByInterval(Interval.Create(4, 5));

            Assert.AreEqual(expected.Min, actual.Min);
            Assert.AreEqual(expected.Max, actual.Max);

            expected = Interval.Create(3, 8);
            actual = Interval.Create(3, 7);
            actual = actual.ExpandedByInterval(Interval.Create(4, 8));

            Assert.AreEqual(expected.Min, actual.Min);
            Assert.AreEqual(expected.Max, actual.Max);
        }

        // Added a method for comparing intervals, because the Equals method had not been overriden on Interval
        private static bool IntervalsAreEqual(Interval expected, Interval actual)
        {
            return expected.Min == actual.Min && expected.Max == actual.Max && expected.Width == actual.Width;
        }
    }
}