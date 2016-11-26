using GeoAPI.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    ///     Computes a point in the interior of an point point.
    ///     Algorithm:
    ///     Find a point which is closest to the centroid of the point.
    /// </summary>
    public class InteriorPointPoint
    {
        private readonly Coordinate _centroid;
        private double _minDistance = double.MaxValue;

        /// <summary>
        /// </summary>
        /// <param name="g"></param>
        public InteriorPointPoint(IGeometry g)
        {
            _centroid = g.Centroid.Coordinate;
            Add(g);
        }

        /// <summary>
        /// </summary>
        public Coordinate InteriorPoint { get; private set; }

        /// <summary>
        ///     Tests the point(s) defined by a Geometry for the best inside point.
        ///     If a Geometry is not of dimension 0 it is not tested.
        /// </summary>
        /// <param name="geom">The point to add.</param>
        private void Add(IGeometry geom)
        {
            if (geom is IPoint)
                Add(geom.Coordinate);
            else if (geom is IGeometryCollection)
            {
                var gc = (IGeometryCollection) geom;
                foreach (var geometry in gc.Geometries)
                    Add(geometry);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="point"></param>
        private void Add(Coordinate point)
        {
            var dist = point.Distance(_centroid);
            if (dist < _minDistance)
            {
                InteriorPoint = new Coordinate(point);
                _minDistance = dist;
            }
        }
    }
}