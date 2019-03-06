using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.EdgeRay
{
    public static class EdgeRayFunctions
    {
        public static double GetArea(IGeometry g)
        {
            return EdgeRayArea.GetArea(g);
        }

        public static double GetIntersectionArea(IGeometry geom0, IGeometry geom1)
        {
            var area = new EdgeRayIntersectionArea(geom0, geom1);
            return area.Area;
        }

        public static double CheckIntersectionArea(IGeometry geom0, IGeometry geom1)
        {
            double intArea = GetIntersectionArea(geom0, geom1);

            double intAreaStd = geom0.Intersection(geom1).Area;

            double diff = Math.Abs(intArea - intAreaStd) / Math.Max(intArea, intAreaStd);

            return diff;
        }
    }
}
