using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes a point in the interior of an point point.
    /// Algorithm:
    /// Find a point which is closest to the centroid of the point.
    /// </summary>
    public class InteriorPointPoint
    {
        /// <summary>
        /// Computes an interior point for the
        /// puntal components of a Geometry.
        /// </summary>
        /// <param name="geom">The geometry to compute.</param>
        /// <returns>
        /// The computed interior point,
        /// or <see langword="null"/> if the geometry has no puntal components.
        /// </returns>
        public static Coordinate GetInteriorPoint(Geometry geom)
        {
            var intPt = new InteriorPointPoint(geom);
            return intPt.InteriorPoint;
        }

        private readonly Coordinate _centroid;
        private double _minDistance = double.MaxValue;
        private Coordinate _interiorPoint;

        /// <summary>
        ///
        /// </summary>
        /// <param name="g"></param>
        public InteriorPointPoint(Geometry g)
        {
            _centroid = g.Centroid.Coordinate;
            Add(g);
        }

        /// <summary>
        /// Tests the point(s) defined by a Geometry for the best inside point.
        /// If a Geometry is not of dimension 0 it is not tested.
        /// </summary>
        /// <param name="geom">The point to add.</param>
        private void Add(Geometry geom)
        {
            if (geom is Point)
                Add(geom.Coordinate);
            else if (geom is GeometryCollection)
            {
                var gc = (GeometryCollection) geom;
                foreach (var geometry in gc.Geometries)
                    Add(geometry);
            }
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

        /// <summary>
        ///
        /// </summary>
        public Coordinate InteriorPoint => _interiorPoint;
    }
}
