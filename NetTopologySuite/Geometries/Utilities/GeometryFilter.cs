using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.Geometries.Utilities
{
    internal static class GeometryFilter
    {

        public static IEnumerable<TGeometry> Extract<TGeometry, TCoordinate>(IGeometry<TCoordinate> geometry)
            where TGeometry : IGeometry<TCoordinate>
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<Double, TCoordinate>, IConvertible
        {
            if (geometry is TGeometry)
            {
                yield return (TGeometry)geometry;
            }

            /*if (geometry is IHasGeometryComponents<TCoordinate>)
            {
            }*/

            if (geometry is IGeometryCollection<TCoordinate>)
            {
                foreach (IGeometry<TCoordinate> g in (IGeometryCollection<TCoordinate>)geometry)
                {
                    foreach (TGeometry g2 in Extract<TGeometry, TCoordinate>(g))
                        yield return g2;
                    
                }
            }
        }

        public static IEnumerable<TGeometry> Filter<TGeometry, TCoordinate>(IGeometry<TCoordinate> geometry)
            where TGeometry : IGeometry<TCoordinate>
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<Double, TCoordinate>, IConvertible
        {
            if (geometry is TGeometry)
            {
                yield return (TGeometry)geometry;
            }

            if (geometry is IHasGeometryComponents<TCoordinate>)
            {
                foreach (IGeometry<TCoordinate> g in (((IHasGeometryComponents<TCoordinate>)geometry).Components))
                {
                    foreach( TGeometry g2 in Filter<TGeometry, TCoordinate>(g))
                        yield return g2;
                }
                yield break;
            }

            if (geometry is IGeometryCollection<TCoordinate>)
            {
                foreach (TGeometry g in Filter<TGeometry, TCoordinate>((IGeometryCollection<TCoordinate>)geometry))
                {
                    yield return g;
                }
            }

            //if (geometry is IEnumerable<IGeometry>)
            //{
            //    foreach (TGeometry g in Filter<TGeometry>(geometry as IEnumerable<IGeometry>))
            //    {
            //        yield return g;
            //    }
            //}
        }

        public static IEnumerable<TGeometry> Filter<TGeometry, TCoordinate>(
            IGeometryCollection<TCoordinate> geometries)
            where TGeometry : IGeometry<TCoordinate>
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<Double, TCoordinate>, IConvertible
        {
            foreach (IGeometry<TCoordinate> geometry in geometries)
            {
                if (geometry is TGeometry)
                {
                    yield return (TGeometry)geometry;
                }
            }
        }

        //public static IEnumerable<TGeometry> Filter<TGeometry, TCoordinate>(IGeometry<TCoordinate> geometry)
        //    where TGeometry : IGeometry<TCoordinate>
        //    where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
        //        IComputable<Double, TCoordinate>, IConvertible

        //{
        //    if (geometry is TGeometry)
        //    {
        //        yield return (TGeometry) geometry;
        //    }

        //    if (geometry is IHasGeometryComponents<TCoordinate>)
        //    {
        //        foreach (TGeometry g in Filter<TGeometry, TCoordinate>((IHasGeometryComponents<TCoordinate>) geometry))
        //        {
        //            yield return g;
        //        }
        //    }

        //    //if (geometry is IEnumerable<IGeometry>)
        //    //{
        //    //    foreach (TGeometry g in Filter<TGeometry>(geometry as IEnumerable<IGeometry>))
        //    //    {
        //    //        yield return g;
        //    //    }
        //    //}
        //}

        //public static IEnumerable<TGeometry> Filter<TGeometry, TCoordinate>(
        //    IHasGeometryComponents<TCoordinate> geometries)
        //    where TGeometry : IGeometry<TCoordinate>
        //    where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
        //        IComputable<Double, TCoordinate>, IConvertible
        //{
        //    foreach (IGeometry<TCoordinate> geometry in geometries.Components)
        //    {
        //        if (geometry is TGeometry)
        //        {
        //            yield return (TGeometry) geometry;
        //        }
        //    }
        //}

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