using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Index.Bintree;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Index.Bintree
{
    public class IntervalTest
    {
        [Test]
        public void TestConstructor()
        {
            var interval = new Interval();
            Assert.That(interval.Min, Is.Zero);
            Assert.That(interval.Centre, Is.Zero);
            Assert.That(interval.Max, Is.Zero);

            interval = new Interval(1, 3);
            Assert.That(interval.Min, Is.EqualTo(1));
            //Assert.That(interval.Centre, Is.Zero);
            Assert.That(interval.Max, Is.EqualTo(3d));
            Assert.That(interval.Width, Is.EqualTo(2d));
        }

        [TestCase(0d, 10d)]
        [TestCase(10d, 0d)]
        public void TestValues(double v1, double v2)
        {
            var interval = new Interval(v1, v2);
            double min = Math.Min(v1, v2);
            double max = Math.Max(v1, v2);
            double width = max - min;
            double centre = min + width * 0.5d;

            Assert.That(interval.Min, Is.EqualTo(min));
            Assert.That(interval.Centre, Is.EqualTo(centre));
            Assert.That(interval.Max, Is.EqualTo(max));
            Assert.That(interval.Width, Is.EqualTo(width));
        }

        [Test]
        public void TestExpandToInclude()
        {
            var interval = new Interval();
            interval.ExpandToInclude(new Interval(-2d, -1d));
            interval.ExpandToInclude(new Interval( 1d, 2d));

            Assert.That(interval.Min, Is.EqualTo(-2));
            Assert.That(interval.Centre, Is.EqualTo(0d));
            Assert.That(interval.Max, Is.EqualTo(2));
            Assert.That(interval.Width, Is.EqualTo(4));

        }
        [Test]
        public void TestInit()
        {
            var interval = new Interval();
            interval.Init(3, 6);
            Assert.That(interval.Min, Is.EqualTo(3d));
            Assert.That(interval.Centre, Is.EqualTo(4.5d));
            Assert.That(interval.Max, Is.EqualTo(6d));
            Assert.That(interval.Width, Is.EqualTo(3d));

            interval.Init(-3, -6);
            Assert.That(interval.Min, Is.EqualTo(-6d));
            Assert.That(interval.Centre, Is.EqualTo(-4.5d));
            Assert.That(interval.Max, Is.EqualTo(-3d));
            Assert.That(interval.Width, Is.EqualTo(3d));
        }

        [Test]
        public void TestOverlaps()
        {
            var int1 = new Interval(3, 6);

            var int2 = new Interval(4, 5);
            Assert.That(int1.Overlaps(int2), Is.True);

            int2 = new Interval(6, 7);
            Assert.That(int1.Overlaps(int2), Is.True);

            int2 = new Interval(7, 8);
            Assert.That(int1.Overlaps(int2), Is.False);

            int2 = new Interval(5, 6.1);
            Assert.That(int1.Overlaps(int2), Is.True);

            int2 = new Interval(2, 3);
            Assert.That(int1.Overlaps(int2), Is.True);

            int2 = new Interval(1.9, 4);
            Assert.That(int1.Overlaps(int2), Is.True);

            int2 = new Interval(1, 2);
            Assert.That(int1.Overlaps(int2), Is.False);
        }

        [Test]
        public void TestContains()
        {
            var int1 = new Interval(3, 6);

            var int2 = new Interval(4, 5);
            Assert.That(int1.Contains(int2), Is.True);

            int2 = new Interval(6, 7);
            Assert.That(int1.Contains(int2), Is.False);

            int2 = new Interval(7, 8);
            Assert.That(int1.Contains(int2), Is.False);

            int2 = new Interval(5, 6.1);
            Assert.That(int1.Contains(int2), Is.False);

            int2 = new Interval(2, 3);
            Assert.That(int1.Contains(int2), Is.False);

            int2 = new Interval(1.9, 4);
            Assert.That(int1.Contains(int2), Is.False);

            int2 = new Interval(1, 2);
            Assert.That(int1.Contains(int2), Is.False);
        }
    }
}
