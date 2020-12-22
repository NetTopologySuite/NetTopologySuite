using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    /// <summary>
    /// Tests the behaviour of the <see cref="GeometryOverlay"/> class.
    /// </summary>
    public class GeometryOverlayTest : GeometryTestCase
    {
        private static (Geometry a, Geometry b) Create()
        {
            var i = NtsGeometryServices.Instance;
            var gf = new GeometryFactory(i.DefaultPrecisionModel, 0, i.DefaultCoordinateSequenceFactory, GeometryOverlay.Legacy);
            var p1 = gf.CreatePoint(new Coordinate(10, 10));
            gf = new GeometryFactory(i.DefaultPrecisionModel, 0, i.DefaultCoordinateSequenceFactory, GeometryOverlay.NG);
            var p2 = gf.CreatePoint(new Coordinate(11, 11));

            return (p1, p2);
        }

        [TestCase(SpatialFunction.Intersection)]
        [TestCase(SpatialFunction.Difference)]
        [TestCase(SpatialFunction.Union)]
        [TestCase(SpatialFunction.SymDifference)]
        public void TestOverlayOfGeometriesWithDifferentGeometryOverlayFails(SpatialFunction opCode)
        {
            (var a, var b) = Create();

            switch (opCode)
            {
                case SpatialFunction.Intersection:
                    Assert.Throws<ArgumentException>(() => a.Intersection(b));
                    break;
                case SpatialFunction.Difference:
                    Assert.Throws<ArgumentException>(() => a.Difference(b));
                    break;
                case SpatialFunction.Union:
                    Assert.Throws<ArgumentException>(() => a.Union(b));
                    break;
                case SpatialFunction.SymDifference:
                    Assert.Throws<ArgumentException>(() => a.SymmetricDifference(b));
                    break;
            }
        }

        [Test]
        public void TestOverlayNGFixed()
        {
            //GeometryOverlay.setOverlayImpl(GeometryOverlay.OVERLAY_PROPERTY_VALUE_NG);
            var pmFixed = new PrecisionModel(1);
            var ntsGeometryServices = new NtsGeometryServices(NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory, pmFixed, NtsGeometryServices.Instance.DefaultSRID, GeometryOverlay.NG);
            var expected = Read(ntsGeometryServices, "POLYGON ((1 2, 4 1, 1 1, 1 2))");

            CheckIntersectionPM(ntsGeometryServices, expected);
        }

        [Test]
        public void TestOverlayNGFloat()
        {
            //GeometryOverlay.setOverlayImpl(GeometryOverlay.OVERLAY_PROPERTY_VALUE_NG);
            var pmFloat = new PrecisionModel();
            var ntsGeometryServices = new NtsGeometryServices(NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory, pmFloat, NtsGeometryServices.Instance.DefaultSRID, GeometryOverlay.NG);
            var expected = Read(ntsGeometryServices, "POLYGON ((1 1, 1 2, 4 1.25, 4 1, 1 1))");

            CheckIntersectionPM(ntsGeometryServices, expected);
        }

        private void CheckIntersectionPM(NtsGeometryServices ntsGeometryServices, Geometry expected)
        {
            var a = Read(ntsGeometryServices, "POLYGON ((1 1, 1 2, 5 1, 1 1))");
            var b = Read(ntsGeometryServices, "POLYGON ((0 3, 4 3, 4 0, 0 0, 0 3))");
            var actual = a.Intersection(b);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestOverlayOld()
        {
            // must set overlay method explicitly since order of tests is not deterministic
            //GeometryOverlay.setOverlayImpl(GeometryOverlay.OVERLAY_PROPERTY_VALUE_OLD);
            CheckIntersectionFails(new NtsGeometryServices(GeometryOverlay.Legacy));
        }

        [Test]
        public void TestOverlayNG()
        {
            //GeometryOverlay.setOverlayImpl(GeometryOverlay.OVERLAY_PROPERTY_VALUE_NG);
            CheckIntersectionSucceeds(new NtsGeometryServices(GeometryOverlay.NG));
        }

        private void CheckIntersectionFails(NtsGeometryServices ntsGeometryServices)
        {
            try
            {
                TryIntersection(ntsGeometryServices);
                Assert.Fail("Intersection operation should have failed but did not");
            }
            catch (TopologyException)
            {
                // ignore - expected result
            }
        }

        private void CheckIntersectionSucceeds(NtsGeometryServices ntsGeometryServices)
        {
            try
            {
                TryIntersection(ntsGeometryServices);
            }
            catch (TopologyException)
            {
                Assert.Fail("Intersection operation failed.");
            }
        }

        private void TryIntersection(NtsGeometryServices ntsGeometryServices)
        {
            var a = Read(ntsGeometryServices,
                "POLYGON ((-1120500.000000126 850931.058865365, -1120500.0000001257 851343.3885007716, -1120500.0000001257 851342.2386007707, -1120399.762684411 851199.4941312922, -1120500.000000126 850931.058865365))");
            var b = Read(ntsGeometryServices,
                "POLYGON ((-1120500.000000126 851253.4627870625, -1120500.0000001257 851299.8179383819, -1120492.1498410008 851293.8417889411, -1120500.000000126 851253.4627870625))");
            var result = a.Intersection(b);
        }
    }

}
