using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries.Utilities
{
    ///<summary>
    /// Class to extract geometries of a specific type from a geometry.
    ///</summary>
    /// <typeparam name="TCoordinate"></typeparam>
    public static class GeometryExtracter<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        ///<summary>
        /// Extracts the <see cref="TGeometry"/> elements from a single <see cref="IGeometry"/> and adds them to the provided <see cref="IList{TGeometry}"/>.
        ///</summary>
        /// <param name="geometry">the geometry from which to extract</param>
        /// <param name="list">the list to add the extracted elements to</param>
        /// <typeparam name="TGeometry">The geometry type to extract</typeparam>
        public static IEnumerable<IGeometry<TCoordinate>> Extract<TGeometry>(IGeometry<TCoordinate> geometry, IList<IGeometry<TCoordinate>> list)
            where TGeometry : IGeometry<TCoordinate>
        {
            foreach (var item in ToGeometry(GeometryFilter.Filter<TGeometry, TCoordinate>(geometry)))
                list.Add(item);
            return list;
        }

        ///<summary>
        /// Extracts the <see cref="TGeometry"/> elements from a single <see cref="IGeometry"/>.
        ///</summary>
        /// <param name="geometry">the geometry from which to extract</param>
        /// <typeparam name="TGeometry">The geometry type to extract</typeparam>
        public static IEnumerable<IGeometry<TCoordinate>> Extract<TGeometry>(IGeometry<TCoordinate> geometry)
            where TGeometry : IGeometry<TCoordinate>
        {
            return Extract<TGeometry>(geometry, new List<IGeometry<TCoordinate>>());
        }

        private static IEnumerable<IGeometry<TCoordinate>> ToGeometry<TGeometry>(IEnumerable<TGeometry> input)
            where TGeometry : IGeometry<TCoordinate>
        {
            foreach (var geometry in input)
                yield return geometry;
        }
    }
}