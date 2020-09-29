using System;

using NetTopologySuite.Mathematics;

using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Mathematics
{
    public class Vector2DTest
    {

        private const double TOLERANCE = 1E-5;

        [Test]
        public void TestLength()
        {
            Assert.AreEqual(1.0, Create(0, 1).Length(), TOLERANCE);
            Assert.AreEqual(1.0, Create(0, -1).Length(), TOLERANCE);
            Assert.AreEqual(Math.Sqrt(2.0), Create(1, 1).Length(), TOLERANCE);
            Assert.AreEqual(5, Create(3, 4).Length(), TOLERANCE);
            Assert.AreEqual(Math.Sqrt(2), Create(1, 1).Length(), TOLERANCE);
            Assert.AreEqual(Math.Sqrt(1 + 4), Create(1, 2).Length(), TOLERANCE);
        }

        [Test]
        public void TestZero()
        {
            AssertEquals(Create(0, 0), Vector2D.Zero);
        }

        [Test]
        public void TestAdd()
        {
            AssertEquals(Create(5, 7), Create(1, 2).Add(Create(4, 5)));
            AssertEquals(Create(5, 7), Create(1, 2) + Create(4, 5));
        }

        [Test]
        public void TestNegate()
        {
            AssertEquals(Create(-3, 5), -Create(3, -5));
        }

        [Test]
        public void TestSubtract()
        {
            AssertEquals(Create(-3, 0), Create(1, 5).Subtract(Create(4, 5)));
            AssertEquals(Create(-3, 0), Create(1, 5) - Create(4, 5));
        }

        [Test]
        public void TestMultiply()
        {
            AssertEquals(Create(2, 4), Create(1, 2).Multiply(2));
            AssertEquals(Create(2, 4), Create(1, 2) * 2);
            AssertEquals(Create(2, 4), 2 * Create(1, 2));
        }

        [Test]
        public void TestDivide()
        {
            AssertEquals(Create(1, 2), Create(2, 4).Divide(2));
            AssertEquals(Create(1, 2), Create(2, 4) / 2);
            AssertEquals(Create(1, 1), Create(2, 4) / Create(2, 4));
        }

        [Test]
        public void TestEquals()
        {
            Vector2D nullVector = null;
            Assert.That(nullVector == null);
            Assert.That(Create(1, 1) == Create(1, 1));
        }

        [Test]
        public void TestNotEquals()
        {
            Assert.That(Create(1, 1) != null);
            Assert.That(null != Create(1, 1));
            Assert.That(Create(1, 1) != Create(1, 2));
            Assert.That(Create(1, 1) != Create(2, 1));
        }

        [Test]
        public void TestDot()
        {
            Assert.AreEqual(8.0, Create(2, 3).Dot(Create(1, 2)));
        }

        [Test]
        public void TestNormalize()
        {
            AssertEquals(Create(-1 / Math.Sqrt(2), 1 / Math.Sqrt(2)),
                Create(-1, 1).Normalize());
            AssertEquals(Create(2 / Math.Sqrt(8), 2 / Math.Sqrt(8)),
                Create(2, 2).Normalize());
            AssertEquals(Create(1 / Math.Sqrt(5), 2 / Math.Sqrt(5)),
                Create(1, 2).Normalize());
        }

        static Vector2D Create(double x, double y)
        {
            return Vector2D.Create(x, y);
        }

        static void AssertEquals(Vector2D expected, Vector2D actual)
        {
            bool isEqual = expected.Equals(actual);
            if (!isEqual)
            {
                TestContext.WriteLine($"Expected {expected} but actual is {actual}");
            }

            Assert.That(isEqual);
        }
    }
}
