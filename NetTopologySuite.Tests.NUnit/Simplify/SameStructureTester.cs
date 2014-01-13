using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Tests.NUnit.Simplify
{
    /// <summary>
    /// Test if two geometries have the same structure
    /// (but not necessarily the same coordinate sequences or adjacencies).
    /// </summary>
    public class SameStructureTester
    {

        public static bool IsSameStructure(IGeometry g1, IGeometry g2)
        {
            if (g1.GetType() != g2.GetType())
                return false;
            if (g1 is GeometryCollection)
                return IsSameStructureCollection((GeometryCollection)g1, (GeometryCollection)g2);
            if (g1 is Polygon)
                return IsSameStructurePolygon((Polygon)g1, (Polygon)g2);
            if (g1 is LineString)
                return IsSameStructureLineString((LineString)g1, (LineString)g2);
            if (g1 is Point)
                return IsSameStructurePoint((Point)g1, (Point)g2);

            NetTopologySuite.Utilities.Assert.ShouldNeverReachHere("Unsupported Geometry class: " + g1.GetType().FullName);
            return false;
        }

        private static bool IsSameStructureCollection(GeometryCollection g1, GeometryCollection g2)
        {
            if (g1.NumGeometries != g2.NumGeometries)
                return false;
            for (int i = 0; i < g1.NumGeometries; i++)
            {
                if (!IsSameStructure(g1.GetGeometryN(i), g2.GetGeometryN(i)))
                    return false;
            }
            return true;
        }

        private static bool IsSameStructurePolygon(Polygon g1, Polygon g2)
        {
            if (g1.NumInteriorRings != g2.NumInteriorRings)
                return false;
            // could check for both empty or nonempty here
            return true;
        }

        private static bool IsSameStructureLineString(LineString g1, LineString g2)
        {
            // could check for both empty or nonempty here
            return true;
        }

        private static bool IsSameStructurePoint(Point g1, Point g2)
        {
            // could check for both empty or nonempty here
            return true;
        }

        private SameStructureTester()
        {
        }
    }
}