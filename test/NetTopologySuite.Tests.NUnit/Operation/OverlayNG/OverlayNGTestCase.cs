using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay;

namespace NetTopologySuite.Tests.NUnit.Operation.OverlayNG
{
    using OverlayNG = NetTopologySuite.Operation.OverlayNG.OverlayNG;

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
            var actual = OverlayNG.Overlay(a, b, overlayOp, pm);
            var expected = Read(wktExpected);
            CheckEqual(expected, actual);
        }

        public static Geometry Difference(Geometry a, Geometry b)
        {
            var pm = new PrecisionModel();
            return OverlayNG.Overlay(a, b, SpatialFunction.Difference, pm);
        }

        public static Geometry SymDifference(Geometry a, Geometry b)
        {
            var pm = new PrecisionModel();
            return OverlayNG.Overlay(a, b, SpatialFunction.SymDifference, pm);
        }

        public static Geometry Intersection(Geometry a, Geometry b)
        {
            var pm = new PrecisionModel();
            return OverlayNG.Overlay(a, b, SpatialFunction.Intersection, pm);
        }

        public static Geometry Union(Geometry a, Geometry b)
        {
            var pm = new PrecisionModel();
            return OverlayNG.Overlay(a, b, SpatialFunction.Union, pm);
        }

        public static Geometry Difference(Geometry a, Geometry b, double scaleFactor)
        {
            var pm = new PrecisionModel(scaleFactor);
            return OverlayNG.Overlay(a, b, SpatialFunction.Difference, pm);
        }

        public static Geometry SymDifference(Geometry a, Geometry b, double scaleFactor)
        {
            var pm = new PrecisionModel(scaleFactor);
            return OverlayNG.Overlay(a, b, SpatialFunction.SymDifference, pm);
        }

        public static Geometry Intersection(Geometry a, Geometry b, double scaleFactor)
        {
            var pm = new PrecisionModel(scaleFactor);
            return OverlayNG.Overlay(a, b, SpatialFunction.Intersection, pm);
        }

        public static Geometry Union(Geometry a, Geometry b, double scaleFactor)
        {
            var pm = new PrecisionModel(scaleFactor);
            return OverlayNG.Overlay(a, b, SpatialFunction.Union, pm);
        }
    }
}
