using System;
using System.Collections;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries.Utilities
{
    ///<summary>
    /// Class to extract all <see cref="IPoint{TCoordinate}"/> instances from a geometry.
    ///</summary>
    /// <typeparam name="TCoordinate"></typeparam>
    public class PointExtracter<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        ///<summary>
        /// Extracts the <see cref="IPoint{TCoordinate}"/> elements from a single <see cref="IGeometry{TCoordinate}"/> and adds them to the provided <see cref="IList{IPoint{TCoordinate}}"/>.
        ///</summary>
        /// <param name="geometry">the geometry from which to extract</param>
        /// <param name="list">the list to add the extracted elements to</param>
        public static IEnumerable<IPoint<TCoordinate>> GetPoints(IGeometry<TCoordinate> geometry, IList<IPoint<TCoordinate>> list)
        {
            foreach (var item in GeometryFilter.Filter<IPoint<TCoordinate>, TCoordinate>(geometry))
                list.Add(item);
            return list;
        }

        ///<summary>
        /// Extracts the <see cref="IPoint{TCoordinate}"/> elements from a single <see cref="IGeometry{TCoordinate}"/>.
        ///</summary>
        /// <param name="geometry">the geometry from which to extract</param>
        public static IEnumerable<IPoint<TCoordinate>> GetPoints(IGeometry<TCoordinate> geometry)
        {
            return GetPoints(geometry, new List<IPoint<TCoordinate>>());
        }

    }
}