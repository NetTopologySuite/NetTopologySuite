using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace Open.Topology.TestRunner.Functions
{
    public class SelectionFunctions
    {
        public static IGeometry intersects(IGeometry a, IGeometry mask)
        {
            return select(a, mask.Intersects);
        }

        public static IGeometry covers(IGeometry a, IGeometry mask)
        {
            return select(a, mask.Covers);
        }

        public static IGeometry coveredBy(IGeometry a, IGeometry mask)
        {
            return select(a, mask.CoveredBy);
        }

        public static IGeometry disjoint(IGeometry a, IGeometry mask)
        {
            return select(a, mask.Disjoint);
        }

        public static IGeometry valid(IGeometry a)
        {
            return select(a, g => g.IsValid);
        }

        public static IGeometry invalid(IGeometry a)
        {
            return select(a, g => !g.IsValid);
        }

        public static IGeometry areaGreater(IGeometry a, double minArea)
        {
            return select(a, g => g.Area > minArea);
        }

        public static IGeometry areaZero(IGeometry a)
        {
            return select(a, g => g.Area == 0d);
        }

        public static IGeometry within(IGeometry a, IGeometry mask)
        {
            return select(a, mask.Within);
        }

        private static IGeometry select(IGeometry geom, Func<IGeometry, bool> predicate)
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

        public static IGeometry firstNComponents(IGeometry g, int n)
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