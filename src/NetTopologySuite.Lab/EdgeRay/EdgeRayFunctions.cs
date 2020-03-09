#nullable disable
using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.EdgeRay
{
    public static class EdgeRayFunctions
    {
        public static double GetArea(Geometry g)
        {
            return EdgeRayArea.GetArea(g);
        }

        public static double GetIntersectionArea(Geometry geom0, Geometry geom1)
        {
            var area = new EdgeRayIntersectionArea(geom0, geom1);
            return area.Area;
        }

        public static double CheckIntersectionArea(Geometry geom0, Geometry geom1)
        {
            double intArea = GetIntersectionArea(geom0, geom1);

            double intAreaStd = geom0.Intersection(geom1).Area;

            double diff = Math.Abs(intArea - intAreaStd) / Math.Max(intArea, intAreaStd);

            return diff;
        }
    }
}
