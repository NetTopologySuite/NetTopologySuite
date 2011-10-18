using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay.Snap;

namespace Open.Topology.TestRunner.Functions
{
    /**
     * Implementations for various geometry functions.
     * 
     * @author Martin Davis
     * 
     */
    public class GeometryFunctions
    {
        public static double length(IGeometry g) { return g.Length; }
        public static double area(IGeometry g) { return g.Area; }

        public static bool isSimple(IGeometry g) { return g.IsSimple; }
        public static bool isValid(IGeometry g) { return g.IsValid; }
        public static bool isRectangle(IGeometry g) { return g.IsRectangle; }

        public static IGeometry envelope(IGeometry g) { return g.Envelope; }
        public static IGeometry reverse(IGeometry g) { return g.Reverse(); }
        public static IGeometry normalize(IGeometry g)
        {
            IGeometry gNorm = (IGeometry)g.Clone();
            gNorm.Normalize();
            return gNorm;
        }

        public static IGeometry snap(IGeometry g, IGeometry g2, double distance)
        {
            IGeometry[] snapped = GeometrySnapper.Snap(g, g2, distance);
            return snapped[0];
        }

        public static IGeometry getGeometryN(IGeometry g, int i)
        {
            return g.GetGeometryN(i);
        }

        public static IGeometry getCoordinates(IGeometry g)
        {
            var pts = g.Coordinates;
            return g.Factory.CreateMultiPoint(pts);
        }
    }
}