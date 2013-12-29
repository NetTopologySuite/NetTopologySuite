using System.Collections.Generic;
using GeoAPI.Geometries;

namespace Open.Topology.TestRunner.Functions
{
    public static class SortingFunctions
    {
        public static IGeometry SortByLength(IGeometry g)
        {
            var geoms = Components(g);
            geoms.Sort(new GeometryLengthComparator());
            return g.Factory.BuildGeometry(geoms);
        }

        private class GeometryLengthComparator : IComparer<IGeometry>
        {
            public int Compare(IGeometry g1, IGeometry g2)
            {
                return g1.CompareTo(g2);
            }
        }

        public static IGeometry SortByArea(IGeometry g)
        {
            var geoms = Components(g);
            geoms.Sort(new GeometryAreaComparator());
            return g.Factory.BuildGeometry(geoms);
        }

        private class GeometryAreaComparator : IComparer<IGeometry>
        {
            public int Compare(IGeometry g1, IGeometry g2)
            {
                return g1.Area.CompareTo(g2.Area);
            }
        }

        private static List<IGeometry> Components(IGeometry g)
        {
            var comp = new List<IGeometry>();
            for (int i = 0; i < g.NumGeometries; i++)
            {
                comp.Add(g.GetGeometryN(i));
            }
            return comp;
        }
    }
}