using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixture]
    public class MultiPointImplTest
    {
        private PrecisionModel precisionModel;
        private GeometryFactory geometryFactory;
        WKTReader reader;

        public MultiPointImplTest()
        {
            precisionModel = new PrecisionModel(1000);
            geometryFactory = new GeometryFactory(precisionModel, 0);
            reader = new WKTReader(geometryFactory);
        }

        // TODO: Enable when #isSimple implemented
        [Test]
        [Ignore("Enable when IsSimple implemented")]
        public void TestIsSimple1()
        {
            //    MultiPoint m = (MultiPoint) reader.read("MULTIPOINT(1.111 2.222, 3.333 4.444, 5.555 6.666)");
            //    Assert.IsTrue(m.isSimple());
        }

        // TODO: Enable when #isSimple implemented
        [Test]
        [Ignore("Enable when IsSimple implemented")]
        public void TestIsSimple2()
        {
            //    MultiPoint m = (MultiPoint) reader.read("MULTIPOINT(1.111 2.222, 3.333 4.444, 3.333 4.444)");
            //    Assert.IsTrue(! m.isSimple());
        }

        [Test]
        public void TestGetGeometryN()  {
            var m = (MultiPoint) reader.Read("MULTIPOINT(1.111 2.222, 3.333 4.444, 3.333 4.444)");
            var g = m.GetGeometryN(1);
            Assert.IsTrue(g is Point);
            var p = (Point) g;
            var externalCoordinate = new Coordinate();
            var internaCoordinate = p.Coordinate;
            externalCoordinate.X = internaCoordinate.X;
            externalCoordinate.Y = internaCoordinate.Y;
            Assert.AreEqual(3.333, externalCoordinate.X, 1E-10);
            Assert.AreEqual(4.444, externalCoordinate.Y, 1E-10);
        }

        [Test]
        public void TestGetEnvelope()
        {
            var m = (MultiPoint)reader.Read("MULTIPOINT(1.111 2.222, 3.333 4.444, 3.333 4.444)");
            var e = m.EnvelopeInternal;
            Assert.AreEqual(1.111, e.MinX, 1E-10);
            Assert.AreEqual(3.333, e.MaxX, 1E-10);
            Assert.AreEqual(2.222, e.MinY, 1E-10);
            Assert.AreEqual(4.444, e.MaxY, 1E-10);
        }

        [Test]
        public void TestEquals()
        {
            var m1 = (MultiPoint)reader.Read("MULTIPOINT(5 6, 7 8, 9 10)");
            var m2 = (MultiPoint)reader.Read("MULTIPOINT(5 6, 7 8, 9 10)");
            Assert.That(m1, Is.EqualTo(m2).Using(GeometryTestCase.EqualityComparer));
            Assert.IsTrue(m1.Equals((object)m2));
        }

        [Test]
        public void TestEquals2()
        {
            var m1 = (MultiPoint)reader.Read("MULTIPOINT (20.564 46.3493254, 45 32, 23 54)");
            var m2 = (MultiPoint)reader.Read("MULTIPOINT (20.564 46.3493254, 45 32, 23 54)");

            Assert.That(m1, Is.EqualTo(m2).Using(GeometryTestCase.EqualityComparer));
            Assert.IsTrue(m1.Equals(m2));
        }
    }
}
