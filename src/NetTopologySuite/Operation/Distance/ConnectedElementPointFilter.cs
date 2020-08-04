using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Distance
{
    /// <summary>
    /// Extracts a single point
    /// from each connected element in a Geometry
    /// (e.g. a polygon, linestring or point)
    /// and returns them in a list
    /// </summary>
    public class ConnectedElementPointFilter : IGeometryFilter
    {
        /// <summary>
        /// Returns a list containing a Coordinate from each Polygon, LineString, and Point
        /// found inside the specified point. Thus, if the specified point is
        /// not a GeometryCollection, an empty list will be returned.
        /// </summary>
        public static IList<Coordinate> GetCoordinates(Geometry geom)
        {
            var pts = new List<Coordinate>();
            geom.Apply(new ConnectedElementPointFilter(pts));
            return pts;
        }

        private readonly IList<Coordinate> _pts;

        /// <summary>
        ///
        /// </summary>
        /// <param name="pts"></param>
        ConnectedElementPointFilter(IList<Coordinate> pts)
        {
            _pts = pts;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geom"></param>
        public void Filter(Geometry geom)
        {
            if (geom is Point || geom is LineString || geom is Polygon)
                _pts.Add(geom.Coordinate);
        }
    }
}
