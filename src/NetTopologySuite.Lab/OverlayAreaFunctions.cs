using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.OverlayArea;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite
{
    public class OverlayAreaFunctions
    {

        public static double AreaSingle(Geometry g)
        {
            return GeometryArea.Compute(g);
        }

        public static double IntersectionArea(Geometry geom0, Geometry geom1)
        {
            return OverlayArea.IntersectionArea(geom0, geom1);
        }

        private static Geometry _overlayAreaKey;
        private static OverlayArea _overlayAreaCache;

        public static double IntersectionAreaPrep(Geometry geom0, Geometry geom1)
        {
            if (geom0 != _overlayAreaKey)
            {
                _overlayAreaKey = geom0;
                _overlayAreaCache = new OverlayArea(geom0);
            }
            return _overlayAreaCache.IntersectionArea(geom1);
        }

        public static Geometry IntersectionAreaPrepData(Geometry geom0, Geometry geom1)
        {
            double area = IntersectionAreaPrep(geom0, geom1);
            if (area == 0.0) return null;
            var result = geom1.Copy();
            result.UserData = area;
            return result;
        }

        public static Geometry IntersectionAreaData(Geometry geom0, Geometry geom1)
        {
            double area = IntersectionArea(geom0, geom1);
            if (area == 0.0) return null;
            var result = geom1.Copy();
            result.UserData = area;
            return result;
        }

        public static double IntAreaOrig(Geometry geom0, Geometry geom1)
        {
            double intArea = geom0.Intersection(geom1).Area;
            return intArea;
        }

        static IPreparedGeometry _geomPrepCache = null;
        static Geometry _geomPrepKey = null;

        public static double IntAreaOrigPrep(Geometry geom0, Geometry geom1)
        {
            if (geom0 != _geomPrepKey)
            {
                _geomPrepKey = geom0;
                _geomPrepCache = PreparedGeometryFactory.Prepare(geom0);
            }
            return IntAreaFullPrep(geom0, _geomPrepCache, geom1);
        }

        public static Geometry IntAreaOrigData(Geometry geom0, Geometry geom1)
        {
            double area = IntAreaOrig(geom0, geom1);
            if (area == 0.0) return null;

            var result = geom1.Copy();
            result.UserData = area;
            return result;
        }

        public static Geometry IntAreaOrigPrepData(Geometry geom0, Geometry geom1)
        {
            double area = IntAreaOrigPrep(geom0, geom1);
            if (area == 0.0) return null;

            var result = geom1.Copy();
            result.UserData = area;
            return result;
        }

        private static double IntAreaFullPrep(Geometry geom, IPreparedGeometry geomPrep, Geometry geom1)
        {
            if (!geomPrep.Intersects(geom1)) return 0.0;
            if (geomPrep.Contains(geom1)) return geom1.Area;
            double intArea = geom.Intersection(geom1).Area;
            return intArea;
        }

        public static double CheckIntArea(Geometry geom0, Geometry geom1)
        {
            double intArea = IntersectionArea(geom0, geom1);

            double intAreaStd = geom0.Intersection(geom1).Area;

            double diff = Math.Abs(intArea - intAreaStd) / Math.Max(intArea, intAreaStd);

            return diff;
        }
    }
}
