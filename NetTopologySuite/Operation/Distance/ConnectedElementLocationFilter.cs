using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Distance
{
    /// <summary>
    /// A ConnectedElementPointFilter extracts a single point
    /// from each connected element in a Geometry
    /// (e.g. a polygon, linestring or point)
    /// and returns them in a list. The elements of the list are
    /// <c>com.vividsolutions.jts.operation.distance.GeometryLocation</c>s.
    /// </summary>
    public class ConnectedElementLocationFilter : IGeometryFilter
    {
        /// <summary>
        /// Returns a list containing a point from each Polygon, LineString, and Point
        /// found inside the specified point. Thus, if the specified point is
        /// not a GeometryCollection, an empty list will be returned. The elements of the list
        /// are <c>com.vividsolutions.jts.operation.distance.GeometryLocation</c>s.
        /// </summary>
        public static IList<GeometryLocation> GetLocations(Geometry geom)
        {
            var locations = new List<GeometryLocation>();
            geom.Apply(new ConnectedElementLocationFilter(locations));
            return locations;
        }

        private readonly IList<GeometryLocation> _locations;

        /// <summary>
        ///
        /// </summary>
        /// <param name="locations"></param>
        ConnectedElementLocationFilter(IList<GeometryLocation> locations)
        {
            _locations = locations;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geom"></param>
        public void Filter(Geometry geom)
        {
            if (geom is Point || geom is LineString || geom is Polygon)
                _locations.Add(new GeometryLocation(geom, 0, geom.Coordinate));
        }
    }
}
