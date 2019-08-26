using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes a point in the interior of an linear point.
    /// Algorithm:
    /// Find an interior vertex which is closest to
    /// the centroid of the linestring.
    /// If there is no interior vertex, find the endpoint which is
    /// closest to the centroid.
    /// </summary>
    public class InteriorPointLine
    {
        /// <summary>
        /// Computes an interior point for the
        /// linear components of a Geometry.
        /// </summary>
        /// <param name="geom">The geometry to compute.</param>
        /// <returns>
        /// The computed interior point,
        /// or <see langword="null"/> if the geometry has no linear components.
        /// </returns>
        public static Coordinate GetInteriorPoint(Geometry geom)
        {
            var intPt = new InteriorPointLine(geom);
            return intPt.InteriorPoint;
        }

        private readonly Coordinate _centroid;
        private double _minDistance = double.MaxValue;
        private Coordinate _interiorPoint;

        /// <summary>
        ///
        /// </summary>
        /// <param name="g"></param>
        public InteriorPointLine(Geometry g)
        {
            _centroid = g.Centroid.Coordinate;
            AddInterior(g);

            if (_interiorPoint == null)
                AddEndpoints(g);
        }

        /// <summary>
        ///
        /// </summary>
        public Coordinate InteriorPoint => _interiorPoint;

        /// <summary>
        /// Tests the interior vertices (if any)
        /// defined by a linear Geometry for the best inside point.
        /// If a Geometry is not of dimension 1 it is not tested.
        /// </summary>
        /// <param name="geom">The point to add.</param>
        private void AddInterior(Geometry geom)
        {
            if(geom is LineString)
                AddInterior(geom.Coordinates);
            else if(geom is GeometryCollection)
            {
                var gc = (GeometryCollection) geom;
                foreach (var geometry in gc.Geometries)
                    AddInterior(geometry);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pts"></param>
        private void AddInterior(Coordinate[] pts)
        {
            for (int i = 1; i < pts.Length - 1; i++)
                Add(pts[i]);

        }

        /// <summary>
        /// Tests the endpoint vertices
        /// defined by a linear Geometry for the best inside point.
        /// If a Geometry is not of dimension 1 it is not tested.
        /// </summary>
        /// <param name="geom">The point to add.</param>
        private void AddEndpoints(Geometry geom)
        {
            if(geom is LineString)
                AddEndpoints(geom.Coordinates);
            else if(geom is GeometryCollection)
            {
                var gc = (GeometryCollection) geom;
                foreach (var geometry in gc.Geometries)
                    AddEndpoints(geometry);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pts"></param>
        private void AddEndpoints(Coordinate[] pts)
        {
            Add(pts[0]);
            Add(pts[pts.Length - 1]);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="point"></param>
        private void Add(Coordinate point)
        {
            double dist = point.Distance(_centroid);
            if (dist < _minDistance)
            {
                _interiorPoint = point.Copy();
                _minDistance = dist;
            }
        }
    }
}
