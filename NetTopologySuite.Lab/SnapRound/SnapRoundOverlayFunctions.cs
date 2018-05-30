using GeoAPI.Geometries;

namespace NetTopologySuite.SnapRound
{
    public static class SnapRoundOverlayFunctions
    {
        public static IGeometry SnappedIntersection(this IGeometry geomA, IGeometry geomB, double scaleFactor)
        {
            return Intersection(geomA, geomB, scaleFactor);
        }

        public static IGeometry Intersection(IGeometry geomA, IGeometry geomB, double scaleFactor)
        {
            IGeometry[] geom = SnapClean(geomA, geomB, scaleFactor);
            return geom[0].Intersection(geom[1]);
        }

        public static IGeometry SnappedDifference(this IGeometry geomA, IGeometry geomB, double scaleFactor)
        {
            return Difference(geomA, geomB, scaleFactor);
        }

        public static IGeometry Difference(IGeometry geomA, IGeometry geomB, double scaleFactor)
        {
            IGeometry[] geom = SnapClean(geomA, geomB, scaleFactor);
            return geom[0].Difference(geom[1]);
        }

        public static IGeometry SnappedSymmetricDifference(this IGeometry geomA, IGeometry geomB, double scaleFactor)
        {
            return SymmetricDifference(geomA, geomB, scaleFactor);
        }

        public static IGeometry SymmetricDifference(IGeometry geomA, IGeometry geomB, double scaleFactor)
        {
            IGeometry[] geom = SnapClean(geomA, geomB, scaleFactor);
            return geom[0].SymmetricDifference(geom[1]);
        }

        public static IGeometry SnappedUnion(this IGeometry geomA, IGeometry geomB, double scaleFactor)
        {
            return Union(geomA, geomB, scaleFactor);
        }

        public static IGeometry Union(IGeometry geomA, IGeometry geomB, double scaleFactor)
        {
            IGeometry[] geom = SnapClean(geomA, geomB, scaleFactor);
            return geom[0].Union(geom[1]);
        }

        public static IGeometry UnaryUnion(IGeometry geomA, double scaleFactor)
        {
            var geom = SnapClean(geomA, null, scaleFactor);
            return geom[0].Union();
        }
        private static IGeometry[] SnapClean(
            IGeometry geomA, IGeometry geomB,
            double scaleFactor)
        {
            var snapped = SnapRoundFunctions.SnapRound(geomA, geomB, scaleFactor);
            //// TODO: don't need to clean once GeometrySnapRounder ensures all components are valid
            //var aSnap = Clean(snapped.GetGeometryN(0));
            //var bSnap = Clean(snapped.GetGeometryN(1));
            if (geomB == null)
                return new [] { snapped, null };
            return new [] { snapped.GetGeometryN(0), snapped.GetGeometryN(1) };
        }

        //private static IGeometry Clean(IGeometry geom)
        //{
        //    // TODO: only buffer if it is a polygonal IGeometry
        //    if (!(geom is IPolygonal) ) return geom;
        //    return geom.Buffer(0);
        //}

    }
}
