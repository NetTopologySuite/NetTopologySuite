using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    /// <summary>
    /// Tests the behaviour of the <see cref="GeometryOverlay"/> class.
    /// </summary>
    public class GeometryOverlayTest : GeometryTestCase
    {
        [Test]
        public void TestOverlayNGFixed()
        {
            //GeometryOverlay.setOverlayImpl(GeometryOverlay.OVERLAY_PROPERTY_VALUE_NG);
            var ntsGeometryServices = new NtsGeometryServices(GeometryOverlay.NG);
            var expected = Read(ntsGeometryServices, "POLYGON ((1 2, 4 1, 1 1, 1 2))");

            var pmFixed = new PrecisionModel(1);
            CheckIntersectionPM(ntsGeometryServices, pmFixed, expected);
        }

        [Test]
        public void TestOverlayNGFloat()
        {
            //GeometryOverlay.setOverlayImpl(GeometryOverlay.OVERLAY_PROPERTY_VALUE_NG);
            var ntsGeometryServices = new NtsGeometryServices(GeometryOverlay.NG);
            var expected = Read(ntsGeometryServices, "POLYGON ((1 1, 1 2, 4 1.25, 4 1, 1 1))");

            var pmFloat = new PrecisionModel();
            CheckIntersectionPM(ntsGeometryServices, pmFloat, expected);
        }

        private void CheckIntersectionPM(NtsGeometryServices ntsGeometryServices, PrecisionModel pmFixed, Geometry expected)
        {
            var ef = expected.Factory;
            var geomFactFixed = new GeometryFactory(pmFixed, ef.SRID, ef.CoordinateSequenceFactory, ef.GeometryOverlay);

            var a = Read(ntsGeometryServices, geomFactFixed, "POLYGON ((1 1, 1 2, 5 1, 1 1))");
            var b = Read(ntsGeometryServices, geomFactFixed, "POLYGON ((0 3, 4 3, 4 0, 0 0, 0 3))");
            var actual = a.Intersection(b);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestOverlayOld()
        {
            // must set overlay method explicitly since order of tests is not deterministic
            //GeometryOverlay.setOverlayImpl(GeometryOverlay.OVERLAY_PROPERTY_VALUE_OLD);
            CheckIntersectionFails(new NtsGeometryServices(GeometryOverlay.Old));
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
            catch (TopologyException ex)
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
            catch (TopologyException ex)
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
