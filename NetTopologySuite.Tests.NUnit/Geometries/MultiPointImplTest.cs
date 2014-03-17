using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixtureAttribute]
    public class MultiPointImplTest
    {
        private IPrecisionModel precisionModel;
        private IGeometryFactory geometryFactory;
        WKTReader reader;

        public MultiPointImplTest()
        {
            precisionModel = new PrecisionModel(1000);
            geometryFactory = new GeometryFactory(precisionModel, 0);
            reader = new WKTReader(geometryFactory);
        }

        // TODO: Enable when #isSimple implemented
        [TestAttribute]
        [IgnoreAttribute("Enable when IsSimple implemented")]
        public void TestIsSimple1()
        {
            //    MultiPoint m = (MultiPoint) reader.read("MULTIPOINT(1.111 2.222, 3.333 4.444, 5.555 6.666)");
            //    Assert.IsTrue(m.isSimple());
        }

        // TODO: Enable when #isSimple implemented
        [TestAttribute]
        [IgnoreAttribute("Enable when IsSimple implemented")]
        public void TestIsSimple2()
        {
            //    MultiPoint m = (MultiPoint) reader.read("MULTIPOINT(1.111 2.222, 3.333 4.444, 3.333 4.444)");
            //    Assert.IsTrue(! m.isSimple());
        }

        [TestAttribute]
        public void TestGetGeometryN()  {
            MultiPoint m = (MultiPoint) reader.Read("MULTIPOINT(1.111 2.222, 3.333 4.444, 3.333 4.444)");
            IGeometry g = m.GetGeometryN(1);
            Assert.IsTrue(g is Point);
            Point p = (Point) g;
            Coordinate externalCoordinate = new Coordinate();
            Coordinate internaCoordinate = p.Coordinate;
            externalCoordinate.X = internaCoordinate.X;
            externalCoordinate.Y = internaCoordinate.Y;
            Assert.AreEqual(3.333, externalCoordinate.X, 1E-10);
            Assert.AreEqual(4.444, externalCoordinate.Y, 1E-10);
        }

        [TestAttribute]
        public void TestGetEnvelope()
        {
            MultiPoint m = (MultiPoint)reader.Read("MULTIPOINT(1.111 2.222, 3.333 4.444, 3.333 4.444)");
            Envelope e = m.EnvelopeInternal;
            Assert.AreEqual(1.111, e.MinX, 1E-10);
            Assert.AreEqual(3.333, e.MaxX, 1E-10);
            Assert.AreEqual(2.222, e.MinY, 1E-10);
            Assert.AreEqual(4.444, e.MaxY, 1E-10);
        }

        [TestAttribute]
        public void TestEquals()
        {
            MultiPoint m1 = (MultiPoint)reader.Read("MULTIPOINT(5 6, 7 8)");
            MultiPoint m2 = (MultiPoint)reader.Read("MULTIPOINT(5 6, 7 8)");
            Assert.IsTrue(m1.Equals(m2));
        }
    }
}
