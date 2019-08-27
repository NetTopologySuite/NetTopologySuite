using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace Open.Topology.TestRunner.Functions
{
    public static class SortingFunctions
    {
        public static Geometry SortByLength(Geometry g)
        {
            var geoms = Components(g);
            geoms.Sort(new GeometryLengthComparator());

            // annotate geometries with length
            foreach (var geom in geoms)
            {
                geom.UserData = geom.Length;
            }
            return g.Factory.BuildGeometry(geoms);
        }

        private class GeometryLengthComparator : IComparer<Geometry>
        {
            public int Compare(Geometry g1, Geometry g2)
            {
                return g1.CompareTo(g2);
            }
        }

        public static Geometry SortByArea(Geometry g)
        {
            var geoms = Components(g);
            geoms.Sort(new GeometryAreaComparator());

            // annotate geometries with area
            foreach (var geom in geoms)
            {
                geom.UserData = geom.Area;
            }
            return g.Factory.BuildGeometry(geoms);
        }

        private class GeometryAreaComparator : IComparer<Geometry>
        {
            public int Compare(Geometry g1, Geometry g2)
            {
                return g1.Area.CompareTo(g2.Area);
            }
        }

        private static List<Geometry> Components(Geometry g)
        {
            var comp = new List<Geometry>();
            for (int i = 0; i < g.NumGeometries; i++)
            {
                comp.Add(g.GetGeometryN(i));
            }
            return comp;
        }
    }
}