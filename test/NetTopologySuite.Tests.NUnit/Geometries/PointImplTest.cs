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
    }
}
