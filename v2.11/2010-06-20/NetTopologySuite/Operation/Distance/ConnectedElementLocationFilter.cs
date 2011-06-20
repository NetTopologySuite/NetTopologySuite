using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Distance
{
    /// <summary>
    /// A ConnectedElementPointFilter extracts a single point
    /// from each connected element in an <see cref="IGeometry{TCoordinate}"/>
    /// (e.g. a polygon, linestring or point)
    /// and returns them in a list.
    /// </summary>
    public class ConnectedElementLocationFilter<TCoordinate> : IGeometryFilter<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>, IComputable<TCoordinate>, IConvertible
    {
        /// <summary>
        /// Returns a list containing a point from each Polygon, LineString, and Point
        /// found inside the specified point. Thus, if the specified point is
        /// not a GeometryCollection, an empty list will be returned.
        /// </summary>
        public static IEnumerable<GeometryLocation<TCoordinate>> GetLocations(IGeometry<TCoordinate> geom)
        {
            List<GeometryLocation<TCoordinate>> locations = new List<GeometryLocation<TCoordinate>>();
            geom.Apply(new ConnectedElementLocationFilter<TCoordinate>(locations));
            return locations;
        }

        private readonly IList<GeometryLocation<TCoordinate>> _locations;

        private ConnectedElementLocationFilter(IList<GeometryLocation<TCoordinate>> locations)
        {
            _locations = locations;
        }

        public void Filter(IGeometry<TCoordinate> geom)
        {
            if (geom is IPoint || geom is ILineString || geom is IPolygon)
            {
                TCoordinate coordinate = Slice.GetFirst(geom.Coordinates);
                _locations.Add(new GeometryLocation<TCoordinate>(geom, 0, coordinate));
            }
        }
    }
}