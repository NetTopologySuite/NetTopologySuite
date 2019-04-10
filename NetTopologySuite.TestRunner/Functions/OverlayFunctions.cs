using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace Open.Topology.TestRunner.Functions
{
    public static class OverlayFunctions
    {
        public static Geometry Intersection(Geometry a, Geometry b)
        {
            return a.Intersection(b);
        }

        public static Geometry Union(Geometry a, Geometry b)
        {
            return a.Union(b);
        }

        public static Geometry SymDifference(Geometry a, Geometry b)
        {
            return a.SymmetricDifference(b);
        }

        public static Geometry Difference(Geometry a, Geometry b)
        {
            return a.Difference(b);
        }

        public static Geometry DifferenceBa(Geometry a, Geometry b)
        {
            return b.Difference(a);
        }

        public static Geometry UnaryUnion(Geometry a)
        {
            return a.Union();
        }

        public static Geometry UnionUsingGeometryCollection(Geometry a, Geometry b)
        {
            var gc = a.Factory.CreateGeometryCollection(
                new[] { a, b });
            return gc.Union();
        }

        public static Geometry Clip(Geometry a, Geometry mask)
        {
            var geoms = new List<Geometry>();
            for (int i = 0; i < a.NumGeometries; i++)
            {
                var clip = a.GetGeometryN(i).Intersection(mask);
                geoms.Add(clip);
            }
            return FunctionsUtil.BuildGeometry(geoms, a);
        }
    }
}