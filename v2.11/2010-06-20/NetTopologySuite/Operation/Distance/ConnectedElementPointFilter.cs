using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Distance
{
    /// <summary>
    /// Extracts a single point
    /// from each connected element in a Geometry
    /// (e.g. a polygon, linestring or point)
    /// and returns them in a list
    /// </summary>
    public class ConnectedElementPointFilter<TCoordinate> : IGeometryFilter<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>, IComputable<TCoordinate>, IConvertible
    {
        /// <summary>
        /// Returns a list containing a Coordinate from each Polygon, LineString, and Point
        /// found inside the specified point. Thus, if the specified point is
        /// not a GeometryCollection, an empty list will be returned.
        /// </summary>
        public static IList<TCoordinate> GetCoordinates(IGeometry<TCoordinate> geom)
        {
            List<TCoordinate> pts = new List<TCoordinate>();
            geom.Apply(new ConnectedElementPointFilter<TCoordinate>(pts));
            return pts;
        }

        private readonly IList<TCoordinate> _coordinates = null;

        private ConnectedElementPointFilter(IList<TCoordinate> pts)
        {
            _coordinates = pts;
        }

        public void Filter(IGeometry<TCoordinate> geom)
        {
            if (geom is IPoint || geom is ILineString || geom is IPolygon)
            {
                _coordinates.Add(Slice.GetFirst(geom.Coordinates));
            }
        }
    }
}