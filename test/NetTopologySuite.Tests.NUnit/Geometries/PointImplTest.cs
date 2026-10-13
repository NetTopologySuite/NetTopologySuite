using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixture]
    public class PointImplTest
    {
        private readonly WKTReader _reader;

        public PointImplTest()
        {
            var gs = new NtsGeometryServices(new PrecisionModel(1000), 0);
            _reader = new WKTReader(gs);
        }

        [Test]
        public void TestEquals1()
        {
            var p1 = (Point)_reader.Read("POINT(1.234 5.678)");
            var p2 = (Point)_reader.Read("POINT(1.234 5.678)");
            Assert.IsTrue(p1.Equals(p2));
        }

        [Test]
        public void TestEquals2()
        {
            var p1 = (Point)_reader.Read("POINT(1.23 5.67)");
            var p2 = (Point)_reader.Read("POINT(1.23 5.67)");
            Assert.IsTrue(p1.Equals(p2));
        }

        [Test]
        public void TestEquals3()
        {
            var p1 = (Point)_reader.Read("POINT(1.235 5.678)");
            var p2 = (Point)_reader.Read("POINT(1.234 5.678)");
            Assert.IsTrue(!p1.Equals(p2));
        }

        [Test]
        public void TestEquals4()
        {
            var p1 = (Point)_reader.Read("POINT(1.2334 5.678)");
            var p2 = (Point)_reader.Read("POINT(1.2333 5.678)");
            Assert.IsTrue(p1.Equals(p2));
        }

        [Test]
        public void TestEquals5()
        {
            var p1 = (Point)_reader.Read("POINT(1.2334 5.678)");
            var p2 = (Point)_reader.Read("POINT(1.2335 5.678)");
            Assert.IsTrue(!p1.Equals(p2));
        }

        [Test]
        public void TestEquals6()
        {
            var p1 = (Point)_reader.Read("POINT(1.2324 5.678)");
            var p2 = (Point)_reader.Read("POINT(1.2325 5.678)");
            Assert.IsTrue(!p1.Equals(p2));
        }

        [Test]
        public void TestNegRounding1()
        {
            var pLo = (Point)_reader.Read("POINT(-1.233 5.678)");
            var pHi = (Point)_reader.Read("POINT(-1.232 5.678)");

            var p1 = (Point)_reader.Read("POINT(-1.2326 5.678)");
            var p2 = (Point)_reader.Read("POINT(-1.2325 5.678)");
            var p3 = (Point)_reader.Read("POINT(-1.2324 5.678)");

            Assert.IsTrue(!p1.Equals(p2));
            Assert.IsTrue(p3.Equals(p2));

            Assert.IsTrue(p1.Equals(pLo));
            Assert.IsTrue(p2.Equals(pHi));
            Assert.IsTrue(p3.Equals(pHi));
        }

        [Test]
        public void TestIsSimple()
        {
            var p1 = (Point)_reader.Read("POINT(1.2324 5.678)");
            Assert.IsTrue(p1.IsSimple);
            var p2 = (Point)_reader.Read("POINT EMPTY");
            Assert.IsTrue(p2.IsSimple);
        }

        [Test]
        public void TestEmptyPointOrdinatePropertyDoesNotThrow()
        {
            var pt = NtsGeometryServices.Instance.CreateGeometryFactory().CreatePoint();
            double? x = null;
            Assert.That(() => x = pt.X, Throws.Nothing);
            Assert.That(x.HasValue, Is.True);
            Assert.That(x.Value, Is.EqualTo(double.NaN));

            double? y = null;
            Assert.That(() => y = pt.Y, Throws.Nothing);
            Assert.That(x.HasValue, Is.True);
            Assert.That(x.Value, Is.EqualTo(double.NaN));
        }

        [Test]
        public void TestTupleConversions()
        {
            Point p1 = (1, 2);
            Assert.That(p1.X, Is.EqualTo(1));
            Assert.That(p1.Y, Is.EqualTo(2));

            Point p2 = (1, 2, 3);
            Assert.That(p2.X, Is.EqualTo(1));
            Assert.That(p2.Y, Is.EqualTo(2));
            Assert.That(p2.Z, Is.EqualTo(3));
        }

        [Test]
        public void TestPointDeconstruct()
        {
            Point p1 = (1, 2);
            var (x1, y1) = p1;
            Assert.That(x1, Is.EqualTo(1));
            Assert.That(y1, Is.EqualTo(2));

            Point p2 = (1, 2, 3);
            var (x2, y2, z2) = p2;
            Assert.That(x2, Is.EqualTo(1));
            Assert.That(y2, Is.EqualTo(2));
            Assert.That(z2, Is.EqualTo(3));
        }

        [Test]
        public void TestPointPatternMatching()
        {
            Point p1 = (1, 2);
            Assert.That(p1 is (1, 2), Is.True);
            Assert.That(p1 is (_, 2), Is.True);

            Point p2 = (1, 2, 3);

            Assert.That(p2 is (1, 2, 3), Is.True);
            Assert.That(p2 is (1, 2, _), Is.True);
            Assert.That(p2 is (1, _, 3), Is.True);
            Assert.That(p2 is (_, 2, 3), Is.True);
            Assert.That(p2 is (1, _, _), Is.True);
            Assert.That(p2 is (_, 2, _), Is.True);
            Assert.That(p2 is (_, _, 3), Is.True);

            // More complex pattern matching
            Assert.That(p2 is (1, > 1, >= 3), Is.True);
            Assert.That(p2 is (_, >= 2, _), Is.True);
        }

        [Test]
        public void TestPointSwitchPatternMatching()
        {
            Point p = (1, 2, 3);

            Assert.That(p is (_, > 1, _), Is.True);

            bool isValid = p switch
            {
                (1, >1, >=3) => true,
                (_, >=2, _) => true,
                _ => false
            };

            Assert.That(isValid, Is.True);
        }
    }
}
