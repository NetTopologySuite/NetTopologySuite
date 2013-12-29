using System.Collections.Generic;
using GeoAPI.Geometries;
using Open.Topology.TestRunner.Utility;

namespace Open.Topology.TestRunner.Functions
{
    public static class OverlayFunctions
    {
        public static IGeometry Intersection(IGeometry a, IGeometry b)
        {
            return a.Intersection(b);
        }

        public static IGeometry Union(IGeometry a, IGeometry b)
        {
            return a.Union(b);
        }

        public static IGeometry SymDifference(IGeometry a, IGeometry b)
        {
            return a.SymmetricDifference(b);
        }

        public static IGeometry Difference(IGeometry a, IGeometry b)
        {
            return a.Difference(b);
        }

        public static IGeometry DifferenceBa(IGeometry a, IGeometry b)
        {
            return b.Difference(a);
        }

        public static IGeometry UnaryUnion(IGeometry a)
        {
            return a.Union();
        }

        public static IGeometry UnionUsingGeometryCollection(IGeometry a, IGeometry b)
        {
            var gc = a.Factory.CreateGeometryCollection(
                new[] { a, b });
            return gc.Union();
        }

        public static IGeometry Clip(IGeometry a, IGeometry mask)
        {
            var geoms = new List<IGeometry>();
            for (var i = 0; i < a.NumGeometries; i++)
            {
                var clip = a.GetGeometryN(i).Intersection(mask);
                geoms.Add(clip);
            }
            return FunctionsUtil.BuildGeometry(geoms, a);
        }
    }
}