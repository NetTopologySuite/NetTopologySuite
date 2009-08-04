using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Geometries.Utilities
{
    internal static class GeometryFilter
    {
        public static IEnumerable<TGeometry> Filter<TGeometry>(IGeometry geometry)
            where TGeometry : IGeometry
        {
            if (geometry is TGeometry)
            {
                yield return (TGeometry) geometry;
            }

            if (geometry is IEnumerable<IGeometry>)
            {
                foreach (TGeometry g in Filter<TGeometry>(geometry as IEnumerable<IGeometry>))
                {
                    yield return g;
                }
            }
        }

        public static IEnumerable<TGeometry> Filter<TGeometry>(IEnumerable<IGeometry> geometries)
            where TGeometry : IGeometry
        {
            foreach (IGeometry geometry in geometries)
            {
                if (geometry is TGeometry)
                {
                    yield return (TGeometry) geometry;
                }
            }
        }

        public static void Apply<TGeometry>(IEnumerable<TGeometry> geometries, Action<TGeometry> action)
            where TGeometry : IGeometry
        {
            foreach (TGeometry geometry in geometries)
            {
                action(geometry);
            }
        }

        public static IEnumerable<TOutput> Apply<TGeometry, TOutput>(IEnumerable<TGeometry> geometries,
                                                                     Func<TGeometry, TOutput> function)
            where TGeometry : IGeometry
        {
            foreach (TGeometry geometry in geometries)
            {
                yield return function(geometry);
            }
        }
    }
}