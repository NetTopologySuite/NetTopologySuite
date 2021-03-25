using System;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    public class CoordinateTest
    {
        [Test]
        public void TestConstructor3D()
        {
            var c = new CoordinateZ(350.2, 4566.8, 5266.3);
            Assert.AreEqual(c.X, 350.2);
            Assert.AreEqual(c.Y, 4566.8);
            Assert.AreEqual(c.Z, 5266.3);
        }

        [Test]
        public void TestConstructor2D()
        {
            var c = new CoordinateZ(350.2, 4566.8);
            Assert.AreEqual(c.X, 350.2);
            Assert.AreEqual(c.Y, 4566.8);
            Assert.AreEqual(c.Z, Coordinate.NullOrdinate);
        }

        [Test]
        public void TestDefaultConstructor()
        {
            var c = new CoordinateZ();
            Assert.AreEqual(c.X, 0.0);
            Assert.AreEqual(c.Y, 0.0);
            Assert.AreEqual(c.Z, Coordinate.NullOrdinate);
        }

        [Test]
        public void TestCopyConstructor3D()
        {
            var orig = new CoordinateZ(350.2, 4566.8, 5266.3);
            var c = new CoordinateZ(orig);
            Assert.AreEqual(c.X, 350.2);
            Assert.AreEqual(c.Y, 4566.8);
            Assert.AreEqual(c.Z, 5266.3);
        }

        [Test]
        public void TestSetCoordinate()
        {
            var orig = new CoordinateZ(350.2, 4566.8, 5266.3);
            var c = new CoordinateZ { CoordinateValue = orig };
            Assert.AreEqual(c.X, 350.2);
            Assert.AreEqual(c.Y, 4566.8);
            Assert.AreEqual(c.Z, 5266.3);
        }

        [Test]
        public void TestGetOrdinate()
        {
            var c = new CoordinateZ(350.2, 4566.8, 5266.3);
            Assert.AreEqual(c[Ordinate.X], 350.2);
            Assert.AreEqual(c[Ordinate.Y], 4566.8);
            Assert.AreEqual(c[Ordinate.Z], 5266.3);
        }

        [Test]
        public void TestSetOrdinate()
        {
            var c = new CoordinateZ();
            c[Ordinate.X] = 111;
            c[Ordinate.Y] = 222;
            c[Ordinate.Z] = 333;
            Assert.AreEqual(c[Ordinate.X], 111.0);
            Assert.AreEqual(c[Ordinate.Y], 222.0);
            Assert.AreEqual(c[Ordinate.Z], 333.0);
        }

        [Test]
        public void TestEquals()
        {
            var c1 = new CoordinateZ(1, 2, 3);
            const string s = "Not a coordinate";
            Assert.IsFalse(c1.Equals(s));

            var c2 = new CoordinateZ(1, 2, 3);
            Assert.IsTrue(c1.Equals2D(c2));

            var c3 = new CoordinateZ(1, 22, 3);
            Assert.IsFalse(c1.Equals2D(c3));
        }

        [Test]
        public void TestEquals2D()
        {
            var c1 = new CoordinateZ(1, 2, 3);
            var c2 = new CoordinateZ(1, 2, 3);
            Assert.IsTrue(c1.Equals2D(c2));

            var c3 = new CoordinateZ(1, 22, 3);
            Assert.IsFalse(c1.Equals2D(c3));
        }

        [Test]
        public void TestEquals3D()
        {
            var c1 = new CoordinateZ(1, 2, 3);
            var c2 = new CoordinateZ(1, 2, 3);
            Assert.IsTrue(c1.Equals3D(c2));

            var c3 = new CoordinateZ(1, 22, 3);
            Assert.IsFalse(c1.Equals3D(c3));
        }

        [Test]
        public void TestEquals2DWithinTolerance()
        {
            var c = new CoordinateZ(100.0, 200.0, 50.0);
            var aBitOff = new CoordinateZ(100.1, 200.1, 50.0);
            Assert.IsTrue(c.Equals2D(aBitOff, 0.2));
        }

        [Test]
        public void TestEqualsInZ()
        {

            var c = new CoordinateZ(100.0, 200.0, 50.0);
            var withSameZ = new CoordinateZ(100.1, 200.1, 50.1);
            Assert.IsTrue(c.EqualInZ(withSameZ, 0.2));
        }

        [Test]
        public void TestCompareTo()
        {
            var lowest = new CoordinateZ(10.0, 100.0, 50.0);
            var highest = new CoordinateZ(20.0, 100.0, 50.0);
            var equalToHighest = new CoordinateZ(20.0, 100.0, 50.0);
            var higherStill = new CoordinateZ(20.0, 200.0, 50.0);

            Assert.AreEqual(-1, lowest.CompareTo(highest));
            Assert.AreEqual(1, highest.CompareTo(lowest));
            Assert.AreEqual(-1, highest.CompareTo(higherStill));
            Assert.AreEqual(0, highest.CompareTo(equalToHighest));
        }

        [Test]
        public void TestToString()
        {
            const string expectedResult = "(100, 200, 50)";
            string actualResult = new CoordinateZ(100, 200, 50).ToString();
            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        public void TestClone()
        {
            var c = new CoordinateZ(100.0, 200.0, 50.0);
            var clone = (CoordinateZ)c.Copy();
            Assert.IsTrue(c.Equals3D(clone));
        }

        [Test]
        public void TestDistance()
        {
            var coord1 = new CoordinateZ(0.0, 0.0, 0.0);
            var coord2 = new CoordinateZ(100.0, 200.0, 50.0);
            double distance = coord1.Distance(coord2);
            Assert.AreEqual(distance, 223.60679774997897, 0.00001);
        }

        [Test]
        public void TestDistance3D()
        {
            var coord1 = new CoordinateZ(0.0, 0.0, 0.0);
            var coord2 = new CoordinateZ(100.0, 200.0, 50.0);
            double distance = coord1.Distance3D(coord2);
            Assert.AreEqual(distance, 229.128784747792, 0.000001);
        }

        [Test]
        public void TestSettingOrdinateValuesViaIndexer()
        {
            var c = new CoordinateZ();
            Assert.DoesNotThrow(() => c[Ordinate.X] = 1);
            Assert.AreEqual(1d, c.X);
            Assert.AreEqual(c.X, c[Ordinate.X]);

            Assert.DoesNotThrow(() => c[Ordinate.Y] = 2);
            Assert.AreEqual(2d, c.Y);
            Assert.AreEqual(c.Y, c[Ordinate.Y]);

            Assert.DoesNotThrow(() => c[Ordinate.Z] = 3);
            Assert.AreEqual(3d, c.Z);
            Assert.AreEqual(c.Z, c[Ordinate.Z]);

            Assert.Throws<InvalidOperationException>(() => c[Ordinate.M] = 4);
        }

        [Test]
        public void TestCoordinateXY()
        {
            var xy = new Coordinate();
            CheckZUnsupported(xy);
            CheckMUnsupported(xy);

            xy = new Coordinate(1.0, 1.0);   // 2D
            var coord = new CoordinateZ(xy); // copy
            Assert.That(coord, Is.EqualTo(xy));
            Assert.That(coord.Z, Is.NaN);
            Assert.That(coord.M, Is.NaN);

            coord = new CoordinateZ(1.0, 1.0, 1.0); // 2.5d
            xy = new Coordinate(coord); // copy
            Assert.That(xy, Is.EqualTo(coord));
        }

        [Test]
        public void TestCoordinateXYM()
        {
            var xym = new CoordinateM();
            CheckZUnsupported(xym);

            xym.M = 1.0;
            Assert.That(xym.M, Is.EqualTo(1.0));

            var coord = new CoordinateZ(xym); // copy
            Assert.That(coord, Is.EqualTo(xym));
            Assert.That(coord.Z, Is.NaN);
            Assert.That(coord.M, Is.NaN);

            coord = new CoordinateZ(1.0, 1.0, 1.0); // 2.5d
            xym = new CoordinateM(coord); // copy
            Assert.That(xym, Is.EqualTo(coord));
        }

        [Test]
        public void TestCoordinateXYZM()
        {
            var xyzm = new CoordinateZM();
            xyzm.Z = 1.0;
            Assert.That(xyzm.Z, Is.EqualTo(1.0));
            xyzm.M = 1.0;
            Assert.That(xyzm.M, Is.EqualTo(1.0));

            var coord = new CoordinateZ(xyzm); // copy
            Assert.That(coord, Is.EqualTo(xyzm));
            Assert.That(coord.Z, Is.EqualTo(1.0));
            Assert.That(coord.M, Is.NaN);

            coord = new CoordinateZ(1.0, 1.0, 1.0); // 2.5d
            xyzm = new CoordinateZM(coord); // copy
            Assert.That(xyzm, Is.EqualTo(coord));
            Assert.That(xyzm.Z, Is.EqualTo(coord.Z).Within(0.000001));
        }

        [Test]
        public void TestCreate()
        {
            DoTestCreate(new Coordinate());
            DoTestCreate(new CoordinateZ());
            DoTestCreate(new CoordinateM());
            DoTestCreate(new CoordinateZM());
        }

        private void DoTestCreate(Coordinate p)
        {
            var test = p.Create();
            Assert.That(test.GetType(), Is.EqualTo(p.GetType()));
            Assert.That(test.X, Is.EqualTo(0d));
            Assert.That(test.Y, Is.EqualTo(0d));
            Assert.That(test.Z, Is.EqualTo(double.NaN));
            Assert.That(test.M, Is.EqualTo(double.NaN));


            test = p.Create(1, 2, 3, 4);
            Assert.That(test.GetType(), Is.EqualTo(p.GetType()));
            Assert.That(test.X, Is.EqualTo(1d));

            Assert.That(test.Y, Is.EqualTo(2d));

            if (test is CoordinateZ || test is CoordinateZM)
                Assert.That(test.Z, Is.EqualTo(3d));
            else
                Assert.That(test.Z, Is.EqualTo(double.NaN));

            if (test is CoordinateM || test is CoordinateZM)
                Assert.That(test.M, Is.EqualTo(4d));
            else
                Assert.That(test.M, Is.EqualTo(double.NaN));
        }

        [Test]
        public void TestCoordinateHash()
        {
            DoTestCoordinateHash(true, new Coordinate(1, 2), new Coordinate(1, 2));
            DoTestCoordinateHash(false, new Coordinate(1, 2), new Coordinate(3, 4));
            DoTestCoordinateHash(false, new Coordinate(1, 2), new Coordinate(1, 4));
            DoTestCoordinateHash(false, new Coordinate(1, 2), new Coordinate(3, 2));
            DoTestCoordinateHash(false, new Coordinate(1, 2), new Coordinate(2, 1));
        }

        private void DoTestCoordinateHash(bool equal, Coordinate a, Coordinate b)
        {
            Assert.That(a.Equals(b), Is.EqualTo(equal));
            Assert.That(a.GetHashCode() == b.GetHashCode(), Is.EqualTo(equal));
        }

        private static void CheckZUnsupported(Coordinate coord)
        {
            Assert.That(() => coord.Z = 0, Throws.InvalidOperationException);
            Assert.That(coord.Z, Is.NaN);
        }

        private static void CheckMUnsupported(Coordinate coord)
        {
            Assert.That(() => coord.M = 0, Throws.InvalidOperationException);
            Assert.That(coord.M, Is.NaN);
        }
    }
}
