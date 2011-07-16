using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.Geometries.Utilities
{
    ///<summary>
    /// Extracts a single representative <see cref="ICoordinate{TCoordinate}"/> from each connected component of a <see cref="IGeometry"/>.
    ///</summary>
    /// <version>1.9</version>
    public class ComponentCoordinateExtracter<TCoordinate> /* : IGeometryComponentFilter<TCoordinate> */
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, 
                            IComparable<TCoordinate>, IConvertible, 
                            IComputable<Double, TCoordinate>
    {
        ///<summary>
        /// Extracts a single representative <see cref="ICoordinate{TCoordinate}"/> from each connected component of a <see cref="IGeometry"/>.
        ///</summary>
        /// <param name="geom">The Geometry from which to extract</param>
        /// <returns>A list of Coordinates</returns>
        public static IList<ICoordinate> GetCoordinates(IGeometry<TCoordinate> geom)
        {
            IList<ICoordinate> coords = new List<ICoordinate>();
            foreach (var item in GeometryComponentFilter<TCoordinate>.Filter<ILineString<TCoordinate>, IPoint<TCoordinate>>(geom))
                coords.Add(item.Coordinates[0]);
            return coords;
        }

        /*
        ///<summary>
        /// Extracts a single representative <see cref="ICoordinate{TCoordinate}"/> from each connected component of a <see cref="IGeometry"/>.
        ///</summary>
        /// <param name="geom">The Geometry from which to extract</param>
        /// <returns>A list of Coordinates</returns>
        public static IList<ICoordinate> GetCoordinates(IGeometry<TCoordinate> geom)
        {
            IList<ICoordinate> coords = new List<ICoordinate>();
            foreach (var item in GeometryFilter.Filter<IGeometry<TCoordinate>>(geom)    )
                coords.Add(item.Coordinates[0]);

        }

        private readonly IList<ICoordinate<TCoordinate>> _coords;

        ///<summary>
        /// Constructs a LineExtracterFilter with a list in which to store LineStrings found.
        ///</summary>
        public ComponentCoordinateExtracter(IList<ICoordinate<TCoordinate>> coords)
        {
            _coords = coords;
        }

        public void Filter(IGeometry<TCoordinate> geom)
        {
            // add coordinates from connected components
            if (geom is ILineString
                || geom is IPoint)
                _coords.Add(geom.Coordinates[0]);
        }
         */

    }
}