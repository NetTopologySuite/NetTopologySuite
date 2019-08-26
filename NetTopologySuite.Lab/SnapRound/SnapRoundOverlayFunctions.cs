using NetTopologySuite.Geometries;

namespace NetTopologySuite.SnapRound
{
    public static class SnapRoundOverlayFunctions
    {
        public static Geometry SnappedIntersection(this Geometry geomA, Geometry geomB, double scaleFactor)
        {
            return Intersection(geomA, geomB, scaleFactor);
        }

        public static Geometry Intersection(Geometry geomA, Geometry geomB, double scaleFactor)
        {
            var geom = SnapClean(geomA, geomB, scaleFactor);
            return geom[0].Intersection(geom[1]);
        }

        public static Geometry SnappedDifference(this Geometry geomA, Geometry geomB, double scaleFactor)
        {
            return Difference(geomA, geomB, scaleFactor);
        }

        public static Geometry Difference(Geometry geomA, Geometry geomB, double scaleFactor)
        {
            var geom = SnapClean(geomA, geomB, scaleFactor);
            return geom[0].Difference(geom[1]);
        }

        public static Geometry SnappedSymmetricDifference(this Geometry geomA, Geometry geomB, double scaleFactor)
        {
            return SymmetricDifference(geomA, geomB, scaleFactor);
        }

        public static Geometry SymmetricDifference(Geometry geomA, Geometry geomB, double scaleFactor)
        {
            var geom = SnapClean(geomA, geomB, scaleFactor);
            return geom[0].SymmetricDifference(geom[1]);
        }

        public static Geometry SnappedUnion(this Geometry geomA, Geometry geomB, double scaleFactor)
        {
            return Union(geomA, geomB, scaleFactor);
        }

        public static Geometry Union(Geometry geomA, Geometry geomB, double scaleFactor)
        {
            var geom = SnapClean(geomA, geomB, scaleFactor);
            return geom[0].Union(geom[1]);
        }

        public static Geometry UnaryUnion(Geometry geomA, double scaleFactor)
        {
            var geom = SnapClean(geomA, null, scaleFactor);
            return geom[0].Union();
        }
        private static Geometry[] SnapClean(
            Geometry geomA, Geometry geomB,
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

        //private static Geometry Clean(Geometry geom)
        //{
        //    // TODO: only buffer if it is a polygonal Geometry
        //    if (!(geom is IPolygonal) ) return geom;
        //    return geom.Buffer(0);
        //}

    }
}
