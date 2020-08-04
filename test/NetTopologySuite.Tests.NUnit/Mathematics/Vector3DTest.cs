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
        public void TestAdd()
        {
            AssertEquals(Create(5, 7, 9), Create(1, 2, 3).Add(Create(4, 5, 6)));
        }

        [Test]
        public void TestSubtract()
        {
            AssertEquals(Create(-3, 0, 3), Create(1, 5, 9).Subtract(Create(4, 5, 6)));
        }

        [Test]
        public void TestDivide()
        {
            AssertEquals(Create(1, 2, 3), Create(2, 4, 6).Divide(2));
        }

        [Test]
        public void TestDot()
        {
            Assert.AreEqual(20.0, Create(2, 3, 4).Dot(Create(1, 2, 3)));
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
        public void TestNormlize()
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
