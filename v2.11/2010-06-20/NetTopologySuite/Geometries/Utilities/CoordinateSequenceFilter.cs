using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries.Utilities
{
    internal static class CoordinateSequenceFilter
    {
        public static IEnumerable<ICoordinateSequence<TCoordinate>> Filter<TCoordinate>(IGeometry<TCoordinate> geometry)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<Double, TCoordinate>, IConvertible
        {
            if (geometry is IHasGeometryComponents<TCoordinate>)
            {
                foreach (IGeometry<TCoordinate> g in ((IHasGeometryComponents<TCoordinate>) geometry).Components)
                {
                    if (g is ICurve<TCoordinate>)
                        yield return geometry.Coordinates;
                }
            }

            if (geometry is ICurve<TCoordinate>)
            {
                yield return geometry.Coordinates;
            }
        }

        public static IEnumerable<ICoordinateSequence<TCoordinate>> Filter<TCoordinate>(
            IHasGeometryComponents<TCoordinate> geometries)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<Double, TCoordinate>, IConvertible
        {
            foreach (IGeometry<TCoordinate> geometry in geometries.Components)
            {
                if (geometry is IHasGeometryComponents<TCoordinate>)
                {
                    foreach (IGeometry<TCoordinate> g in Filter((IHasGeometryComponents<TCoordinate>) geometry))
                    {
                        if (g is ICurve<TCoordinate>)
                            yield return geometry.Coordinates;
                    }
                }

                if (geometry is ICurve<TCoordinate>)
                    yield return geometry.Coordinates;
            }
        }

        public static void Apply<TCoordinate>(IEnumerable<ICoordinateSequence<TCoordinate>> sequences,
                                              Action<ICoordinateSequence<TCoordinate>> action)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<Double, TCoordinate>, IConvertible
        {
            foreach (ICoordinateSequence<TCoordinate> sequence in sequences)
            {
                action(sequence);
            }
        }

        public static IEnumerable<TOutput> Apply<TCoordinate, TOutput>(
            IEnumerable<ICoordinateSequence<TCoordinate>> sequences,
            Func<ICoordinateSequence<TCoordinate>, TOutput> function)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<Double, TCoordinate>, IConvertible
        {
            foreach (ICoordinateSequence<TCoordinate> sequence in sequences)
            {
                yield return function(sequence);
            }
        }
    }
}