using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Distance;

namespace Open.Topology.TestRunner.Functions
{
    public static class SelectionFunctions
    {
        public static Geometry Intersects(Geometry a, Geometry mask)
        {
            return Select(a, mask.Intersects);
        }

        public static Geometry Covers(Geometry a, Geometry mask)
        {
            return Select(a, mask.Covers);
        }

        public static Geometry CoveredBy(Geometry a, Geometry mask)
        {
            return Select(a, mask.CoveredBy);
        }

        public static Geometry Disjoint(Geometry a, Geometry mask)
        {
            return Select(a, mask.Disjoint);
        }

        public static Geometry Valid(Geometry a)
        {
            return Select(a, g => g.IsValid);
        }

        public static Geometry Invalid(Geometry a)
        {
            return Select(a, g => !g.IsValid);
        }

        public static Geometry AreaGreater(Geometry a, double minArea)
        {
            return Select(a, g => g.Area > minArea);
        }

        public static Geometry AreaZero(Geometry a)
        {
            return Select(a, g => g.Area == 0d);
        }

        public static Geometry Within(Geometry a, Geometry mask)
        {
            return Select(a, mask.Within);
        }

        public static Geometry InteriorPointWithin(Geometry a, Geometry mask)
        {
            return Select(a, g => g.InteriorPoint.Within(mask));
        }

        public static Geometry withinDistance(Geometry a, Geometry mask, double maximumDistance)
        {
            return Select(a, t => mask.IsWithinDistance(t, maximumDistance));
        }

        public static Geometry withinDistanceIndexed(Geometry a, Geometry mask, double maximumDistance)
        {
            var indexedDist = new IndexedFacetDistance(mask);
            return Select(a, t => indexedDist.IsWithinDistance(t, maximumDistance));
        }

        private static Geometry Select(Geometry geom, Func<Geometry, bool> predicate)
        {
            var selected = new List<Geometry>();
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

        public static Geometry FirstNComponents(Geometry g, int n)
        {
            var comp = new List<Geometry>();
            for (int i = 0; i < g.NumGeometries && i < n; i++)
            {
                comp.Add(g.GetGeometryN(i));
            }
            return g.Factory.BuildGeometry(comp);
        }
    }
}
