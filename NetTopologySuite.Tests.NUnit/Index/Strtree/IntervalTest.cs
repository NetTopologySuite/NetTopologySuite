/*
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Index
{
    using Interval = GeoAPI.Geometries.Interval;

    [TestAttribute]
    public class IntervalTest
    {
        [TestAttribute]
        public void TestIntersectsBasic()
        {
            Assert.IsTrue(new Interval(5, 10).Overlaps(new Interval(7, 12)));
            Assert.IsTrue(new Interval(7, 12).Overlaps(new Interval(5, 10)));
            Assert.IsTrue(!new Interval(5, 10).Overlaps(new Interval(11, 12)));
            Assert.IsTrue(!new Interval(11, 12).Overlaps(new Interval(5, 10)));
            Assert.IsTrue(new Interval(5, 10).Overlaps(new Interval(10, 12)));
            Assert.IsTrue(new Interval(10, 12).Overlaps(new Interval(5, 10)));
        }

        [TestAttribute]
        public void TestIntersectsZeroWidthInterval()
        {
            Assert.IsTrue(new Interval(10, 10).Overlaps(new Interval(7, 12)));
            Assert.IsTrue(new Interval(7, 12).Overlaps(new Interval(10, 10)));
            Assert.IsTrue(!new Interval(10, 10).Overlaps(new Interval(11, 12)));
            Assert.IsTrue(!new Interval(11, 12).Overlaps(new Interval(10, 10)));
            Assert.IsTrue(new Interval(10, 10).Overlaps(new Interval(10, 12)));
            Assert.IsTrue(new Interval(10, 12).Overlaps(new Interval(10, 10)));
        }

        [TestAttribute]
        public void TestCopyConstructor()
        {
            Assert.IsTrue(IntervalsAreEqual(new Interval(3, 4), new Interval(3, 4)));
            Assert.IsTrue(IntervalsAreEqual(new Interval(3, 4), new Interval(new Interval(3, 4))));
        }

        [TestAttribute]
        public void TestCentre()
        {
            Assert.AreEqual(6.5, new Interval(4, 9).Centre, 1E-10);
        }

        [TestAttribute]
        public void TestExpandToInclude()
        {
            var expected = new Interval(3, 8);
            var actual = new Interval(3, 4);
            actual.ExpandToInclude(new Interval(7, 8));

            Assert.AreEqual(expected.Min, actual.Min);
            Assert.AreEqual(expected.Max, actual.Max);

            expected = new Interval(3, 7);
            actual = new Interval(3, 7);
            actual.ExpandToInclude(new Interval(4, 5));

            Assert.AreEqual(expected.Min, actual.Min);
            Assert.AreEqual(expected.Max, actual.Max);

            expected = new Interval(3, 8);
            actual = new Interval(3, 7);
            actual.ExpandToInclude(new Interval(4, 8));

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
*/