using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Mathematics;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Mathematics
{
    public class Vector3DTest
    {

        private const double TOLERANCE = 1E-5;

        [Test]
        public void TestLength()
        {
            Assert.AreEqual(1.0, Create(0, 1, 0).Length(), TOLERANCE);
            Assert.AreEqual(1.0, Create(0, -1, 0).Length(), TOLERANCE);
            Assert.AreEqual(Math.Sqrt(2.0), Create(1, 1, 0).Length(), TOLERANCE);
            Assert.AreEqual(5, Create(3, 4, 0).Length(), TOLERANCE);
            Assert.AreEqual(Math.Sqrt(3), Create(1, 1, 1).Length(), TOLERANCE);
            Assert.AreEqual(Math.Sqrt(1 + 4 + 9), Create(1, 2, 3).Length(), TOLERANCE);
        }

        [Test]
        public void TestZero()
        {
            AssertEquals(Create(0, 0, 0), Vector3D.Zero);
        }

        [Test]
        public void TestCreateFromVector2D()
        {
            AssertEquals(Create(1, 2, 3), new Vector3D(new Vector2D(1, 2), 3));
        }

        [Test]
        public void TestUnaryPlus()
        {
            AssertEquals(Create(5, 7, 9), +Create(5, 7, 9));
        }

        [Test]
        public void TestNegate()
        {
            AssertEquals(Create(-3, 5, -7), -Create(3, -5, 7));
        }

        [Test]
        public void TestAdd()
        {
            AssertEquals(Create(5, 7, 9), Create(1, 2, 3).Add(Create(4, 5, 6)));
            AssertEquals(Create(5, 7, 9), Create(1, 2, 3) + Create(4, 5, 6));
        }

        [Test]
        public void TestSubtract()
        {
            AssertEquals(Create(-3, 0, 3), Create(1, 5, 9).Subtract(Create(4, 5, 6)));
            AssertEquals(Create(-3, 0, 3), Create(1, 5, 9) - Create(4, 5, 6));
        }

        [Test]
        public void TestMultiply()
        {
            AssertEquals(Create(2, 4, 6), Create(1, 2, 3) * 2);
            AssertEquals(Create(2, 4, 6), 2 * Create(1, 2, 3));
            AssertEquals(Create(2, 8, 18), Create(1, 2, 3) * Create(2, 4, 6));
        }

        [Test]
        public void TestDivide()
        {
            AssertEquals(Create(1, 2, 3), Create(2, 4, 6).Divide(2));
            AssertEquals(Create(1, 2, 3), Create(2, 4, 6) / 2);
            AssertEquals(Create(1, 2, 3), Create(2, 8, 18) / Create(2, 4, 6));
        }

        [Test]
        public void TestEquals()
        {
            Vector3D nullVector = null;
            Assert.That(nullVector == null);
            Assert.That(Create(1, 1, 1) == Create(1, 1, 1));
        }

        [Test]
        public void TestNotEquals()
        {
            Assert.That(Create(1, 1, 1) != null);
            Assert.That(null != Create(1, 1, 1));
            Assert.That(Create(1, 1, 1) != Create(1, 1, 2));
            Assert.That(Create(1, 1, 1) != Create(1, 2, 1));
            Assert.That(Create(1, 1, 1) != Create(2, 1, 1));
        }

        [Test]
        public void TestDot()
        {
            Assert.AreEqual(20.0, Create(2, 3, 4).Dot(Create(1, 2, 3)));
        }

        [Test]
        public void TestCross()
        {
            Assert.AreEqual(Create(-3, 6, -3), Create(2, 3, 4).Cross(Create(5, 6, 7)));
            Assert.AreEqual(Create(-3, 6, -3), Vector3D.Cross(Create(2, 3, 4), Create(5, 6, 7)));
        }

        [Test]
        public void TestDotABCD()
        {
            double dot = Vector3D.Dot(
                Coord(2, 3, 4), Coord(3, 4, 5),
                Coord(0, 1, -1), Coord(1, 5, 2));
            Assert.AreEqual(8.0, dot);
            Assert.AreEqual(dot, Create(1, 1, 1).Dot(Create(1, 4, 3)));
        }

        [Test]
        public void TestNormalize()
        {
            AssertEquals(Create(-0.5773502691896258, 0.5773502691896258, 0.5773502691896258),
                Create(-1, 1, 1).Normalize());
            AssertEquals(Create(0.5773502691896258, 0.5773502691896258, 0.5773502691896258),
                Create(2, 2, 2).Normalize());
            AssertEquals(Create(0.2672612419124244, 0.5345224838248488, 0.8017837257372732),
                Create(1, 2, 3).Normalize());
        }

        static CoordinateZ Coord(double x, double y, double z)
        {
            return new CoordinateZ(x, y, z);
        }

        static Vector3D Create(double x, double y, double z)
        {
            return Vector3D.Create(x, y, z);
        }

        static void AssertEquals(Vector3D expected, Vector3D actual)
        {
            bool isEqual = expected.Equals(actual);
            if (!isEqual)
            {
                TestContext.WriteLine($"Expected {expected} but actual is {actual}");
            }

            Assert.That(isEqual);
        }

        static void AssertEquals(Vector3D expected, Vector3D actual, double tolerance)
        {
            Assert.AreEqual(expected.X, actual.X, tolerance);
            Assert.AreEqual(expected.Y, actual.Y, tolerance);
            Assert.AreEqual(expected.Z, actual.Z, tolerance);
        }
    }

}
