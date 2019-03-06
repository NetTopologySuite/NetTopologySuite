using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace Open.Topology.TestRunner.Functions
{
    public static class SelectionFunctions
    {
        public static IGeometry Intersects(IGeometry a, IGeometry mask)
        {
            return Select(a, mask.Intersects);
        }

        public static IGeometry Covers(IGeometry a, IGeometry mask)
        {
            return Select(a, mask.Covers);
        }

        public static IGeometry CoveredBy(IGeometry a, IGeometry mask)
        {
            return Select(a, mask.CoveredBy);
        }

        public static IGeometry Disjoint(IGeometry a, IGeometry mask)
        {
            return Select(a, mask.Disjoint);
        }

        public static IGeometry Valid(IGeometry a)
        {
            return Select(a, g => g.IsValid);
        }

        public static IGeometry Invalid(IGeometry a)
        {
            return Select(a, g => !g.IsValid);
        }

        public static IGeometry AreaGreater(IGeometry a, double minArea)
        {
            return Select(a, g => g.Area > minArea);
        }

        public static IGeometry AreaZero(IGeometry a)
        {
            return Select(a, g => g.Area == 0d);
        }

        public static IGeometry Within(IGeometry a, IGeometry mask)
        {
            return Select(a, mask.Within);
        }

        public static IGeometry InteriorPointWithin(IGeometry a, IGeometry mask)
        {
            return Select(a, g => g.InteriorPoint.Within(mask));
        }

        private static IGeometry Select(IGeometry geom, Func<IGeometry, bool> predicate)
        {
            var selected = new List<IGeometry>();
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var g = geom.GetGeometryN(i);
                if (predicate(g))
                {
                    selected.Add(g);
                }
            }
            return geom.Factory.BuildGeometry(selected);
        }

        public static IGeometry FirstNComponents(IGeometry g, int n)
        {
            var comp = new List<IGeometry>();
            for (int i = 0; i < g.NumGeometries && i < n; i++)
            {
                comp.Add(g.GetGeometryN(i));
            }
            return g.Factory.BuildGeometry(comp);
        }
    }
}
