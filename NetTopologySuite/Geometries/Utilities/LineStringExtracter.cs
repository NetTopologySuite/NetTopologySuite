using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.Geometries.Utilities
{
    ///<summary>
    /// Class to extract all <see cref="ILineString{TCoordinate}"/> instances from a geometry.
    ///</summary>
    /// <typeparam name="TCoordinate"></typeparam>
    public class LineStringExtracter<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        ///<summary>
        /// Extracts the <see cref="ILineString{TCoordinate}"/> elements from a single <see cref="IGeometry{TCoordinate}"/> and adds them to the provided <see cref="IList{ILineString{TCoordinate}}"/>.
        ///</summary>
        /// <param name="geometry">the geometry from which to extract</param>
        /// <param name="list">the list to add the extracted elements to</param>
        public static IEnumerable<ILineString<TCoordinate>> GetLines(IGeometry<TCoordinate> geometry, IList<ILineString<TCoordinate>> list)
        {
            foreach (var item in GeometryFilter.Filter<ILineString<TCoordinate>, TCoordinate>(geometry))
                list.Add(item);
            return list;
        }

        ///<summary>
        /// Extracts the <see cref="ILineString{TCoordinate}"/> elements from a single <see cref="IGeometry{TCoordinate}"/>.
        ///</summary>
        /// <param name="geometry">the geometry from which to extract</param>
        public static IEnumerable<ILineString<TCoordinate>> GetLines(IGeometry<TCoordinate> geometry)
        {
            return GetLines(geometry, new List<ILineString<TCoordinate>>());
        }
        
    }
}