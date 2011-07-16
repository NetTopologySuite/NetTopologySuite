using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// <c>Geometry</c> classes support the concept of applying
    /// an <c>IGeometryComponentFilter</c> filter to the <c>Geometry</c>.
    /// The filter is applied to every component of the <c>Geometry</c>
    /// which is itself a <c>Geometry</c>.
    /// (For instance, all the LinearRings in Polygons are visited.)
    /// An <c>IGeometryComponentFilter</c> filter can either
    /// record information about the <c>Geometry</c>
    /// or change the <c>Geometry</c> in some way.
    /// <c>IGeometryComponentFilter</c> is an example of the Gang-of-Four Visitor pattern.
    /// </summary>    
    internal static class GeometryComponentFilter<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        /// <summary>
        /// Filters all <see cref="TGeometry"/> components from <see cref="IGeometry{TCoordinate}"/>.
        /// </summary>
        /// <typeparam name="TGeometry">The type of geometry to filter</typeparam>
        /// <param name="geometry">The geometry to filter from</param>
        /// <returns>an enumeration of <typeparamref name="TGeometry"/> items.</returns>
        public static IEnumerable<TGeometry> Filter<TGeometry>(IGeometry<TCoordinate> geometry)
            where TGeometry : IGeometry<TCoordinate>
        {
            if (geometry is TGeometry)
                yield return (TGeometry)geometry;
            else
            {
                var geometryWithCompontents = geometry as IHasGeometryComponents<TCoordinate>;
                if (geometryWithCompontents != null)
                {
                    foreach (var component in Filter<TGeometry>(geometryWithCompontents))
                       yield return component;
                }
            }
        }

        /// <summary>
        /// Filters all <see cref="TGeometry"/> components from <see cref="IGeometry{TCoordinate}"/>.
        /// </summary>
        /// <typeparam name="TGeometry">The type of geometry to filter</typeparam>
        /// <param name="geometry">The geometry to filter from</param>
        /// <returns>an enumeration of <typeparamref name="TGeometry"/> items.</returns>
        public static IEnumerable<IGeometry<TCoordinate>> FilterBase<TGeometry>(IGeometry<TCoordinate> geometry)
            where TGeometry : IGeometry<TCoordinate>
        {
            if (geometry is TGeometry)
                yield return geometry;
            else
            {
                var geometryWithCompontents = geometry as IHasGeometryComponents<TCoordinate>;
                if (geometryWithCompontents != null)
                {
                    foreach (var component in Filter<TGeometry>(geometryWithCompontents))
                        yield return component;
                }
            }
        }

        /// <summary>
        /// Filters all <see cref="TGeometry1"/> or <see cref="TGeometry2"/> components from an <see cref="IGeometry{TCoordinate}"/>.
        /// </summary>
        /// <typeparam name="TGeometry1">The 1st type of geometry to filter</typeparam>
        /// <typeparam name="TGeometry2">The 2nd type of geometry to filter</typeparam>
        /// <param name="geometry">The geometry to filter from</param>
        /// <returns>an enumeration of <typeparamref name="TGeometry1"/> or <typeparam name="TGeometry2"/> items.</returns>
        public static IEnumerable<IGeometry<TCoordinate>> Filter<TGeometry1, TGeometry2>(IGeometry<TCoordinate> geometry)
            where TGeometry1 : IGeometry<TCoordinate>
            where TGeometry2 : IGeometry<TCoordinate>
        {
            if (geometry is TGeometry1 || geometry is TGeometry2)
                yield return geometry;
            else
            {
                var geometryWithCompontents = geometry as IHasGeometryComponents<TCoordinate>;
                if (geometryWithCompontents != null)
                {
                    foreach (var component in Filter<TGeometry1, TGeometry2>(geometryWithCompontents))
                        yield return component;
                }
            }
        }

        /// <summary>
        /// Filters all <see cref="TGeometry1"/>, <see cref="TGeometry2"/> or see <see cref="TGeometry3"/> components from an <see cref="IGeometry{TCoordinate}"/>.
        /// </summary>
        /// <typeparam name="TGeometry1">The 1st type of geometry to filter</typeparam>
        /// <typeparam name="TGeometry2">The 2nd type of geometry to filter</typeparam>
        /// <typeparam name="TGeometry3">The 3nd type of geometry to filter</typeparam>
        /// <param name="geometry">The geometry to filter from</param>
        /// <returns>an enumeration of <typeparamref name="TGeometry1"/> or <typeparam name="TGeometry2"/> items.</returns>
        public static IEnumerable<IGeometry<TCoordinate>> Filter<TGeometry1, TGeometry2, TGeometry3>(IGeometry<TCoordinate> geometry)
            where TGeometry1 : IGeometry<TCoordinate>
            where TGeometry2 : IGeometry<TCoordinate>
            where TGeometry3 : IGeometry<TCoordinate>
        {
            if (geometry is TGeometry1 || geometry is TGeometry2 || geometry is TGeometry3)
                yield return geometry;
            else
            {
                var geometryWithCompontents = geometry as IHasGeometryComponents<TCoordinate>;
                if (geometryWithCompontents != null)
                {
                    foreach (var component in Filter<TGeometry1, TGeometry2, TGeometry3>(geometryWithCompontents))
                        yield return component;
                }
            }
        }

        private static IEnumerable<TGeometry> Filter<TGeometry>(IHasGeometryComponents<TCoordinate> geometry)
        {
            foreach (var component in geometry.Components)
            {
                if (component is TGeometry)
                    yield return (TGeometry)component;
                else
                {
                    var geometryWithComponents = component as IHasGeometryComponents<TCoordinate>;
                    if (geometryWithComponents != null)
                    {
                        foreach (var component2 in Filter<TGeometry>(geometryWithComponents))
                            yield return component2;
                    }
                }
            }
        }

        private static IEnumerable<IGeometry<TCoordinate>> Filter<TGeometry1, TGeometry2>(IHasGeometryComponents<TCoordinate> geometry)
        {
            foreach (var component in geometry.Components)
            {
                if (component is TGeometry1 || component is TGeometry2)
                    yield return component;
                else
                {
                    var geometryWithComponents = component as IHasGeometryComponents<TCoordinate>;
                    if (geometryWithComponents != null)
                    {
                        foreach (var component2 in Filter<TGeometry1, TGeometry2>(geometryWithComponents))
                            yield return component2;
                    }
                }
            }
        }

        private static IEnumerable<IGeometry<TCoordinate>> Filter<TGeometry1, TGeometry2, TGeometry3>(IHasGeometryComponents<TCoordinate> geometry)
        {
            foreach (var component in geometry.Components)
            {
                if (component is TGeometry1 || component is TGeometry2 || component is TGeometry3)
                    yield return component;
                
                else
                {
                    var geometryWithComponents = component as IHasGeometryComponents<TCoordinate>;
                    if (geometryWithComponents != null)
                    {
                        foreach (var component2 in Filter<TGeometry1, TGeometry2, TGeometry3>(geometryWithComponents))
                            yield return component2;
                    }
                }
            }
        }
    }


}