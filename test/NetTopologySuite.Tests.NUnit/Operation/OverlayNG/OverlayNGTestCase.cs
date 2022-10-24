using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay;

namespace NetTopologySuite.Tests.NUnit.Operation.OverlayNG
{
    public class OverlayNGTestCase : GeometryTestCase
    {
        protected void CheckIntersection(string wktA, string wktB, string wktExpected)
        {
            CheckOverlay(wktA, wktB, SpatialFunction.Intersection, wktExpected);
        }

        protected void CheckUnion(string wktA, string wktB, string wktExpected)
        {
            CheckOverlay(wktA, wktB, SpatialFunction.Union, wktExpected);
        }

        protected void CheckOverlay(string wktA, string wktB, SpatialFunction overlayOp, string wktExpected)
        {
            var a = Read(wktA);
            var b = Read(wktB);
            var pm = new PrecisionModel();
            var actual = NetTopologySuite.Operation.OverlayNG.OverlayNG.Overlay(a, b, overlayOp, pm);
            var expected = Read(wktExpected);
            CheckEqual(expected, actual);
        }
    }
}
