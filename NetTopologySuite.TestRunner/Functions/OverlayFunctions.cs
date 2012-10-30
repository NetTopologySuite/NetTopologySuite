using System.Collections.Generic;
using GeoAPI.Geometries;
using Open.Topology.TestRunner.Utility;

namespace Open.Topology.TestRunner.Functions
{
    public class OverlayFunctions
    {
        public static IGeometry intersection(IGeometry a, IGeometry b)
        {
            return a.Intersection(b);
        }

        public static IGeometry union(IGeometry a, IGeometry b)
        {
            return a.Union(b);
        }

        public static IGeometry symDifference(IGeometry a, IGeometry b)
        {
            return a.SymmetricDifference(b);
        }

        public static IGeometry difference(IGeometry a, IGeometry b)
        {
            return a.Difference(b);
        }

        public static IGeometry differenceBA(IGeometry a, IGeometry b)
        {
            return b.Difference(a);
        }

        public static IGeometry unaryUnion(IGeometry a)
        {
            return a.Union();
        }

        public static IGeometry unionUsingGeometryCollection(IGeometry a, IGeometry b)
        {
            var gc = a.Factory.CreateGeometryCollection(
                new[] {a, b});
            return gc.Union();
        }

        public static IGeometry clip(IGeometry a, IGeometry mask)
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