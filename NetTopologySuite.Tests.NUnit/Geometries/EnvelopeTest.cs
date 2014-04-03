using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixtureAttribute]
    public class EnvelopeTest
    {
        private IPrecisionModel precisionModel;
        private IGeometryFactory geometryFactory;
        WKTReader reader;

        public EnvelopeTest()
        {
            precisionModel = new PrecisionModel(1);
            geometryFactory = new GeometryFactory(precisionModel, 0);
            reader = new WKTReader(geometryFactory);
        }

        [TestAttribute]
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

        [TestAttribute]
        public void TestIntersectsEmpty()
        {
            Assert.IsTrue(!new Envelope(-5, 5, -5, 5).Intersects(new Envelope()));
            Assert.IsTrue(!new Envelope().Intersects(new Envelope(-5, 5, -5, 5)));
            Assert.IsTrue(!new Envelope().Intersects(new Envelope(100, 101, 100, 101)));
            Assert.IsTrue(!new Envelope(100, 101, 100, 101).Intersects(new Envelope()));
        }

        [TestAttribute]
        public void TestContainsEmpty()
        {
            Assert.IsTrue(!new Envelope(-5, 5, -5, 5).Contains(new Envelope()));
            Assert.IsTrue(!new Envelope().Contains(new Envelope(-5, 5, -5, 5)));
            Assert.IsTrue(!new Envelope().Contains(new Envelope(100, 101, 100, 101)));
            Assert.IsTrue(!new Envelope(100, 101, 100, 101).Contains(new Envelope()));
        }

        [TestAttribute]
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

        [TestAttribute]
        public void TestEmpty()
        {
            Assert.AreEqual(0, new Envelope().Height, 0);
            Assert.AreEqual(0, new Envelope().Width, 0);
            Assert.AreEqual(new Envelope(), new Envelope());
            Envelope e = new Envelope(100, 101, 100, 101);
            e.Init(new Envelope());
            Assert.AreEqual(new Envelope(), e);
        }

        [TestAttribute]
        public void TestAsGeometry()
        {
            Assert.IsTrue(geometryFactory.CreatePoint((Coordinate)null).Envelope
                    .IsEmpty);

            IGeometry g = geometryFactory.CreatePoint(new Coordinate(5, 6))
                    .Envelope;
            Assert.IsTrue(!g.IsEmpty);
            Assert.IsTrue(g is Point);

            Point p = (Point)g;
            Assert.AreEqual(5, p.X, 1E-1);
            Assert.AreEqual(6, p.Y, 1E-1);

            LineString l = (LineString)reader.Read("LINESTRING(10 10, 20 20, 30 40)");
            IGeometry g2 = l.Envelope;
            Assert.IsTrue(!g2.IsEmpty);
            Assert.IsTrue(g2 is Polygon);

            Polygon poly = (Polygon)g2;
            poly.Normalize();
            Assert.AreEqual(5, poly.ExteriorRing.NumPoints);
            Assert.AreEqual(new Coordinate(10, 10), poly.ExteriorRing.GetCoordinateN(
                    0));
            Assert.AreEqual(new Coordinate(10, 40), poly.ExteriorRing.GetCoordinateN(
                    1));
            Assert.AreEqual(new Coordinate(30, 40), poly.ExteriorRing.GetCoordinateN(
                    2));
            Assert.AreEqual(new Coordinate(30, 10), poly.ExteriorRing.GetCoordinateN(
                    3));
            Assert.AreEqual(new Coordinate(10, 10), poly.ExteriorRing.GetCoordinateN(
                    4));
        }

        [TestAttribute]
        public void TestSetToNull()
        {
            Envelope e1 = new Envelope();
            Assert.IsTrue(e1.IsNull);
            e1.ExpandToInclude(5, 5);
            Assert.IsTrue(!e1.IsNull);
            e1.SetToNull();
            Assert.IsTrue(e1.IsNull);
        }

        [TestAttribute]
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

        [TestAttribute]
        public void TestEquals2()
        {
            Assert.IsTrue(new Envelope().Equals(new Envelope()));
            Assert.IsTrue(new Envelope(1, 2, 1, 2).Equals(new Envelope(1, 2, 1, 2)));
            Assert.IsTrue(!new Envelope(1, 2, 1.5, 2).Equals(new Envelope(1, 2, 1, 2)));
        }

        [TestAttribute]
        public void TestCopyConstructor()
        {
            Envelope e1 = new Envelope(1, 2, 3, 4);
            Envelope e2 = new Envelope(e1);
            Assert.AreEqual(1, e2.MinX, 1E-5);
            Assert.AreEqual(2, e2.MaxX, 1E-5);
            Assert.AreEqual(3, e2.MinY, 1E-5);
            Assert.AreEqual(4, e2.MaxY, 1E-5);
        }

        [TestAttribute]
        public void TestGeometryFactoryCreateEnvelope()
        {
            checkExpectedEnvelopeGeometry("POINT (0 0)");
            checkExpectedEnvelopeGeometry("POINT (100 13)");
            checkExpectedEnvelopeGeometry("LINESTRING (0 0, 0 10)");
            checkExpectedEnvelopeGeometry("LINESTRING (0 0, 10 0)");

            String poly10 = "POLYGON ((0 10, 10 10, 10 0, 0 0, 0 10))";
            checkExpectedEnvelopeGeometry(poly10);

            checkExpectedEnvelopeGeometry("LINESTRING (0 0, 10 10)",
                    poly10);
            checkExpectedEnvelopeGeometry("POLYGON ((5 10, 10 6, 5 0, 0 6, 5 10))",
                    poly10);
        }

        void checkExpectedEnvelopeGeometry(String wktInput)
        {
            checkExpectedEnvelopeGeometry(wktInput, wktInput);
        }

        void checkExpectedEnvelopeGeometry(String wktInput, String wktEnvGeomExpected)
        {
            IGeometry input = reader.Read(wktInput);
            IGeometry envGeomExpected = reader.Read(wktEnvGeomExpected);

            Envelope env = input.EnvelopeInternal;
            IGeometry envGeomActual = geometryFactory.ToGeometry(env);
            bool isEqual = envGeomActual.Equals(envGeomExpected);
            Assert.IsTrue(isEqual);
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
            Assert.IsTrue(env.Equals(envFromString));
        }
    }
}
