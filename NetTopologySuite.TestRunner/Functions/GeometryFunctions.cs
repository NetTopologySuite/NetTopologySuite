using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay.Snap;

namespace Open.Topology.TestRunner.Functions
{
    /// <summary>
    /// Implementations for various geometry functions.
    /// </summary>
    /// <author>Martin Davis</author>
    public static class GeometryFunctions
    {
        public static double Length(IGeometry g) { return g.Length; }
        public static double Area(IGeometry g) { return g.Area; }

        public static bool IsSimple(IGeometry g) { return g.IsSimple; }
        public static bool IsValid(IGeometry g) { return g.IsValid; }
        public static bool IsRectangle(IGeometry g) { return g.IsRectangle; }

        public static IGeometry Envelope(IGeometry g) { return g.Envelope; }
        public static IGeometry Reverse(IGeometry g) { return g.Reverse(); }
        public static IGeometry Normalize(IGeometry g)
        {
            IGeometry gNorm = (IGeometry)g.Clone();
            gNorm.Normalize();
            return gNorm;
        }

        public static IGeometry Snap(IGeometry g, IGeometry g2, double distance)
        {
            IGeometry[] snapped = GeometrySnapper.Snap(g, g2, distance);
            return snapped[0];
        }

        public static IGeometry GetGeometryN(IGeometry g, int i)
        {
            return g.GetGeometryN(i);
        }

        public static IGeometry GetCoordinates(IGeometry g)
        {
            var pts = g.Coordinates;
            return g.Factory.CreateMultiPoint(pts);
        }
    }
}