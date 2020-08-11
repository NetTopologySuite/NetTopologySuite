using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay;
using NUnit.Framework;
using NetTopologySuite.Operation;

namespace NetTopologySuite.Tests.NUnit.Operation.OverlayNG
{
    public partial class OverlayGraphTest
    {
        /// <summary>
        /// Tests primarily the API for OverlayNG with floating precision.
        /// </summary>
        public class OverlayNGFloatingPrecisionTest : GeometryTestCase
        {
            [Test]
            public void TestTriangleIntersectionn()
            {
                var a = Read("POLYGON ((0 0, 8 0, 8 3, 0 0))");
                var b = Read("POLYGON ((0 5, 5 0, 0 0, 0 5))");
                var expected = Read("POLYGON ((0 0, 3.6363636363636362 1.3636363636363635, 5 0, 0 0))");
                var actual = Intersection(a, b);
                CheckEqual(expected, actual);
            }

            static Geometry Intersection(Geometry a, Geometry b)
            {
                return NetTopologySuite.Operation.OverlayNg.OverlayNG.Overlay(a, b, SpatialFunction.Intersection);
            }
        }
    }
}
