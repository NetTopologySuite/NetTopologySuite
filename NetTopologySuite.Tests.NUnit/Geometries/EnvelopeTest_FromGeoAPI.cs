using System;
using GeoAPI.Geometries;
using NUnit.Framework;
using Coordinate=GeoAPI.Geometries.Coordinate;

namespace GeoAPI.Tests.Geometries
{
    [TestFixture]
    public class EnvelopeTest
    {
        [Test]
        public void TestEverything()
        {
            Envelope e1 = new Envelope();
            Assert.IsTrue(e1.IsNull);
            Assert.AreEqual(0, e1.Width, 1E-3);
            Assert.AreEqual(0, e1.Height, 1E-3);
            e1.ExpandToInclude(100, 101);
            e1.ExpandToInclude(200, 202);
            e1.ExpandToInclude(150, 151);
            Assert.AreEqual(200, e1.MaxX, 1E-3);
            Assert.AreEqual(202, e1.MaxY, 1E-3);
            Assert.AreEqual(100, e1.MinX, 1E-3);
            Assert.AreEqual(101, e1.MinY, 1E-3);
            Assert.IsTrue(e1.Contains(120, 120));
            Assert.IsTrue(e1.Contains(120, 101));
            Assert.IsTrue(!e1.Contains(120, 100));
            Assert.AreEqual(101, e1.Height, 1E-3);
            Assert.AreEqual(100, e1.Width, 1E-3);
            Assert.IsTrue(!e1.IsNull);

            Envelope e2 = new Envelope(499, 500, 500, 501);
            Assert.IsTrue(!e1.Contains(e2));
            Assert.IsTrue(!e1.Intersects(e2));
            e1.ExpandToInclude(e2);
            Assert.IsTrue(e1.Contains(e2));
            Assert.IsTrue(e1.Intersects(e2));
            Assert.AreEqual(500, e1.MaxX, 1E-3);
            Assert.AreEqual(501, e1.MaxY, 1E-3);
            Assert.AreEqual(100, e1.MinX, 1E-3);
            Assert.AreEqual(101, e1.MinY, 1E-3);

            Envelope e3 = new Envelope(300, 700, 300, 700);
            Assert.IsTrue(!e1.Contains(e3));
            Assert.IsTrue(e1.Intersects(e3));

            Envelope e4 = new Envelope(300, 301, 300, 301);
            Assert.IsTrue(e1.Contains(e4));
            Assert.IsTrue(e1.Intersects(e4));
        }

        [Test]
        public void TestIntersects()
        {
            CheckIntersectsPermuted(1, 1, 2, 2, 2, 2, 3, 3, true);
            CheckIntersectsPermuted(1, 1, 2, 2, 3, 3, 4, 4, false);
        }

        [Test]
        public void TestIntersectsEmpty()
        {
            Assert.IsTrue(!new Envelope(-5, 5, -5, 5).Intersects(new Envelope()));
            Assert.IsTrue(!new Envelope().Intersects(new Envelope(-5, 5, -5, 5)));
            Assert.IsTrue(!new Envelope().Intersects(new Envelope(100, 101, 100, 101)));
            Assert.IsTrue(!new Envelope(100, 101, 100, 101).Intersects(new Envelope()));
        }

        [Test]
        public void TestContainsEmpty()
        {
            Assert.IsTrue(!new Envelope(-5, 5, -5, 5).Contains(new Envelope()));
            Assert.IsTrue(!new Envelope().Contains(new Envelope(-5, 5, -5, 5)));
            Assert.IsTrue(!new Envelope().Contains(new Envelope(100, 101, 100, 101)));
            Assert.IsTrue(!new Envelope(100, 101, 100, 101).Contains(new Envelope()));
        }

        [Test]
        public void TestExpandToIncludeEmpty()
        {
            Assert.AreEqual(new Envelope(-5, 5, -5, 5), ExpandToInclude(new Envelope(-5,
                    5, -5, 5), new Envelope()));
            Assert.AreEqual(new Envelope(-5, 5, -5, 5), ExpandToInclude(new Envelope(),
                    new Envelope(-5, 5, -5, 5)));
            Assert.AreEqual(new Envelope(100, 101, 100, 101), ExpandToInclude(
                    new Envelope(), new Envelope(100, 101, 100, 101)));
            Assert.AreEqual(new Envelope(100, 101, 100, 101), ExpandToInclude(
                    new Envelope(100, 101, 100, 101), new Envelope()));
        }

        private static Envelope ExpandToInclude(Envelope a, Envelope b)
        {
            a.ExpandToInclude(b);
            return a;
        }

        [Test]
        public void TestEmpty()
        {
            Assert.AreEqual(0, new Envelope().Height, 0);
            Assert.AreEqual(0, new Envelope().Width, 0);
            Assert.AreEqual(new Envelope(), new Envelope());
            Envelope e = new Envelope(100, 101, 100, 101);
            e.Init(new Envelope());
            Assert.AreEqual(new Envelope(), e);
        }

        [Test]
        public void TestSetToNull()
        {
            Envelope e1 = new Envelope();
            Assert.IsTrue(e1.IsNull);
            e1.ExpandToInclude(5, 5);
            Assert.IsTrue(!e1.IsNull);
            e1.SetToNull();
            Assert.IsTrue(e1.IsNull);
        }

        [Test]
        public void TestEquals()
        {
            Envelope e1 = new Envelope(1, 2, 3, 4);
            Envelope e2 = new Envelope(1, 2, 3, 4);
            Assert.AreEqual(e1, e2);
            Assert.AreEqual(e1.GetHashCode(), e2.GetHashCode());

            Envelope e3 = new Envelope(1, 2, 3, 5);
            Assert.IsTrue(!e1.Equals(e3));
            Assert.IsTrue(e1.GetHashCode() != e3.GetHashCode());
            e1.SetToNull();
            Assert.IsTrue(!e1.Equals(e2));
            Assert.IsTrue(e1.GetHashCode() != e2.GetHashCode());
            e2.SetToNull();
            Assert.AreEqual(e1, e2);
            Assert.AreEqual(e1.GetHashCode(), e2.GetHashCode());
        }

        [Test]
        public void TestEquals2()
        {
            Assert.IsTrue(new Envelope().Equals(new Envelope()));
            Assert.IsTrue(new Envelope(1, 2, 1, 2).Equals(new Envelope(1, 2, 1, 2)));
            Assert.IsTrue(!new Envelope(1, 2, 1.5, 2).Equals(new Envelope(1, 2, 1, 2)));
        }

        [Test]
        public void TestCopyConstructor()
        {
            Envelope e1 = new Envelope(1, 2, 3, 4);
            Envelope e2 = new Envelope(e1);
            Assert.AreEqual(1, e2.MinX, 1E-5);
            Assert.AreEqual(2, e2.MaxX, 1E-5);
            Assert.AreEqual(3, e2.MinY, 1E-5);
            Assert.AreEqual(4, e2.MaxY, 1E-5);
        }

        [Test]
        public void TestCopyMethod()
        {
            Envelope e1 = new Envelope(1, 2, 3, 4);
            Envelope e2 = e1.Copy();
            Assert.AreEqual(1, e2.MinX, 1E-5);
            Assert.AreEqual(2, e2.MaxX, 1E-5);
            Assert.AreEqual(3, e2.MinY, 1E-5);
            Assert.AreEqual(4, e2.MaxY, 1E-5);

            Assert.That(ReferenceEquals(e1, e2), Is.False);
        }

        [Test]
        public void TestCompareTo()
        {
            CheckCompareTo(0, new Envelope(), new Envelope());
            CheckCompareTo(0, new Envelope(1, 2, 1, 2), new Envelope(1, 2, 1, 2));
            CheckCompareTo(1, new Envelope(2, 3, 1, 2), new Envelope(1, 2, 1, 2));
            CheckCompareTo(-1, new Envelope(1, 2, 1, 2), new Envelope(2, 3, 1, 2));
            CheckCompareTo(1, new Envelope(1, 2, 1, 3), new Envelope(1, 2, 1, 2));
            CheckCompareTo(1, new Envelope(2, 3, 1, 3), new Envelope(1, 3, 1, 2));
        }

        private static void CheckCompareTo(int expected, Envelope env1, Envelope env2)
        {
            Assert.IsTrue(expected == env1.CompareTo(env2), "expected == env1.CompareTo(env2)");
            Assert.IsTrue(-expected == env2.CompareTo(env1), "-expected == env2.CompareTo(env1)" );
        }


        [Test]
        public void TestToString()
        {
            TestToString(new Envelope(), "Env[Null]");
            TestToString(new Envelope(new Coordinate(10, 10)), "Env[10 : 10, 10 : 10]");
            TestToString(new Envelope(new Coordinate(10.1, 10.1)), "Env[10.1 : 10.1, 10.1 : 10.1]");
            TestToString(new Envelope(new Coordinate(10.1, 19.9), new Coordinate(19.9, 10.1)), "Env[10.1 : 19.9, 10.1 : 19.9]");
        }

        private static void TestToString(Envelope env, string envString)
        {
            var toString = env.ToString();
            Assert.AreEqual(envString, toString);
        }

        [Test]
        public void TestParse()
        {
            TestParse("Env[Null]", new Envelope());
            TestParse("Env[10 : 10, 10 : 10]", new Envelope(new Coordinate(10, 10)));
            TestParse("Env[10.1 : 10.1, 10.1 : 10.1]", new Envelope(new Coordinate(10.1, 10.1)));
            TestParse("Env[10.1 : 19.9, 10.1 : 19.9]", new Envelope(new Coordinate(10.1, 19.9), new Coordinate(19.9, 10.1)));
            Assert.Throws<ArgumentNullException>(() => TestParse(null, new Envelope()));
            Assert.Throws<ArgumentException>(() => TestParse("no envelope", new Envelope()));
            Assert.Throws<ArgumentException>(() => TestParse("Env[10.1 : 19.9, 10.1 : 19/9]", new Envelope()));
        }

        private static void TestParse(string envString, Envelope env)
        {
            var envFromString = Envelope.Parse(envString);
            Assert.IsTrue(env.Equals(envFromString),"{0} != {1}", env, envFromString);
        }


        private void CheckIntersectsPermuted(double a1x, double a1y, double a2x, double a2y, double b1x, double b1y, double b2x, double b2y, bool expected)
        {
            CheckIntersects(a1x, a1y, a2x, a2y, b1x, b1y, b2x, b2y, expected);
            CheckIntersects(a1x, a2y, a2x, a1y, b1x, b1y, b2x, b2y, expected);
            CheckIntersects(a1x, a1y, a2x, a2y, b1x, b2y, b2x, b1y, expected);
            CheckIntersects(a1x, a2y, a2x, a1y, b1x, b2y, b2x, b1y, expected);
        }

        private void CheckIntersects(double a1x, double a1y, double a2x, double a2y, double b1x, double b1y, double b2x, double b2y, bool expected)
        {
            var a = new Envelope(a1x, a2x, a1y, a2y);
            var b = new Envelope(b1x, b2x, b1y, b2y);
            Assert.AreEqual(expected, a.Intersects(b));

            var a1 = new Coordinate(a1x, a1y);
            var a2 = new Coordinate(a2x, a2y);
            var b1 = new Coordinate(b1x, b1y);
            var b2 = new Coordinate(b2x, b2y);
            Assert.AreEqual(expected, Envelope.Intersects(a1, a2, b1, b2));

            Assert.AreEqual(expected, a.Intersects(b1, b2));
        }


    }
}
