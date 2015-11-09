using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeoAPI.Geometries;

namespace NetTopologySuite.SnapRound
{
    public class SnapRoundOverlayFunctions
    {


        public static IGeometry Intersection(IGeometry geomA, IGeometry geomB, double scaleFactor)
        {
            IGeometry[] geom = SnapClean(geomA, geomB, scaleFactor);
            return geom[0].Intersection(geom[1]);
        }

        public static IGeometry Difference(IGeometry geomA, IGeometry geomB, double scaleFactor)
        {
            IGeometry[] geom = SnapClean(geomA, geomB, scaleFactor);
            return geom[0].Difference(geom[1]);
        }

        public static IGeometry SymmetricDifference(IGeometry geomA, IGeometry geomB, double scaleFactor)
        {
            IGeometry[] geom = SnapClean(geomA, geomB, scaleFactor);
            return geom[0].SymmetricDifference(geom[1]);
        }

        public static IGeometry Union(IGeometry geomA, IGeometry geomB, double scaleFactor)
        {
            IGeometry[] geom = SnapClean(geomA, geomB, scaleFactor);
            return geom[0].Union(geom[1]);
        }

        private static IGeometry[] SnapClean(
            IGeometry geomA, IGeometry geomB,
            double scaleFactor)
        {
            IGeometry snapped = SnapRoundFunctions.SnapRound(geomA, geomB, scaleFactor);
            // TODO: don't need to clean once GeometrySnapRounder ensures all components are valid
            IGeometry aSnap = Clean(snapped.GetGeometryN(0));
            IGeometry bSnap = Clean(snapped.GetGeometryN(1));
            return new IGeometry[] { aSnap, bSnap };
        }

        private static IGeometry Clean(IGeometry geom)
        {
            // TODO: only buffer if it is a polygonal IGeometry
            if (!(geom is IPolygonal) ) return geom;
            return geom.Buffer(0);
        }


    }
}
