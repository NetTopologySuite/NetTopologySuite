using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    /**
 * Tests the behaviour of the {@link GeometryOverlay} class.
 * 
 * Currently does not test the reading of the system property.
 * 
 * @author mdavis
 *
 */
    public class GeometryOverlayTest : GeometryTestCase
    {
        private NtsGeometryServices _ngs;

        [OneTimeSetUp]
        public void SetUp()
        {
            _ngs = NtsGeometryServices.Instance;
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            NtsGeometryServices.Instance = _ngs;
        }

        [Test]
        public void TestOverlayNGFixed()
        {
            //GeometryOverlay.setOverlayImpl(GeometryOverlay.OVERLAY_PROPERTY_VALUE_NG);
            NtsGeometryServices.Instance = new NtsGeometryServices {GeometryOverlay = GeometryOverlay.NG};
            var pmFixed = new PrecisionModel(1);
            var expected = Read("POLYGON ((1 2, 4 1, 1 1, 1 2))");

            CheckIntersectionPM(pmFixed, expected);
        }

        [Test]
        public void TestOverlayNGFloat()
        {
            //GeometryOverlay.setOverlayImpl(GeometryOverlay.OVERLAY_PROPERTY_VALUE_NG);
            NtsGeometryServices.Instance = new NtsGeometryServices {GeometryOverlay = GeometryOverlay.NG};
            var pmFloat = new PrecisionModel();
            var expected = Read("POLYGON ((1 1, 1 2, 4 1.25, 4 1, 1 1))");

            CheckIntersectionPM(pmFloat, expected);
        }

        private void CheckIntersectionPM(PrecisionModel pmFixed, Geometry expected)
        {
            var geomFactFixed = new GeometryFactory(pmFixed);
            Assert.That(geomFactFixed.GeometryOverlay, Is.EqualTo(NtsGeometryServices.Instance.GeometryOverlay));

            var a = Read(geomFactFixed, "POLYGON ((1 1, 1 2, 5 1, 1 1))");
            var b = Read(geomFactFixed, "POLYGON ((0 3, 4 3, 4 0, 0 0, 0 3))");
            var actual = a.Intersection(b);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestOverlayOld()
        {
            // must set overlay method explicitly since order of tests is not deterministic
            //GeometryOverlay.setOverlayImpl(GeometryOverlay.OVERLAY_PROPERTY_VALUE_OLD);
            NtsGeometryServices.Instance = new NtsGeometryServices {GeometryOverlay = GeometryOverlay.Old};
            CheckIntersectionFails();
        }

        [Test]
        public void TestOverlayNG()
        {
            //GeometryOverlay.setOverlayImpl(GeometryOverlay.OVERLAY_PROPERTY_VALUE_NG);
            NtsGeometryServices.Instance = new NtsGeometryServices {GeometryOverlay = GeometryOverlay.NG};
            CheckIntersectionSucceeds();
        }

        private void CheckIntersectionFails()
        {
            try
            {
                TryIntersection();
                Assert.Fail("Intersection operation should have failed but did not");
            }
            catch (TopologyException ex)
            {
                // ignore - expected result
            }
        }

        private void CheckIntersectionSucceeds()
        {
            try
            {
                TryIntersection();
            }
            catch (TopologyException ex)
            {
                Assert.Fail("Intersection operation failed.");
            }
        }

        private void TryIntersection()
        {
            var a = Read(NtsGeometryServices.Instance.CreateGeometryFactory(),
                "POLYGON ((-1120500.000000126 850931.058865365, -1120500.0000001257 851343.3885007716, -1120500.0000001257 851342.2386007707, -1120399.762684411 851199.4941312922, -1120500.000000126 850931.058865365))");
            var b = Read(NtsGeometryServices.Instance.CreateGeometryFactory(),
                "POLYGON ((-1120500.000000126 851253.4627870625, -1120500.0000001257 851299.8179383819, -1120492.1498410008 851293.8417889411, -1120500.000000126 851253.4627870625))");
            var result = a.Intersection(b);
        }
    }

}
