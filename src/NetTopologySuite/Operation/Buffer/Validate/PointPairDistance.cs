using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Buffer.Validate
{
    /// <summary>
    /// Contains a pair of points and the distance between them.
    /// Provides methods to update with a new point pair with
    /// either maximum or minimum distance.
    /// </summary>
    public class PointPairDistance
    {
        private readonly Coordinate[] _pt = { new Coordinate(), new Coordinate() };
        private double _distance = double.NaN;
        private bool _isNull = true;

        public void Initialize() { _isNull = true; }

        public void Initialize(Coordinate p0, Coordinate p1)
        {
            _pt[0].CoordinateValue = p0;
            _pt[1].CoordinateValue = p1;
            _distance = p0.Distance(p1);
            _isNull = false;
        }

        /// <summary>
        /// Initializes the points, avoiding recomputing the distance.
        /// </summary>
        /// <param name="p0">The first point</param>
        /// <param name="p1">The second point</param>
        /// <param name="distance">The distance between <paramref name="p0"/> and <paramref name="p1"/></param>
        private void Initialize(Coordinate p0, Coordinate p1, double distance)
        {
            _pt[0].CoordinateValue = p0;
            _pt[1].CoordinateValue = p1;
            _distance = distance;
            _isNull = false;
        }

        public double Distance => _distance;

        public Coordinate[] Coordinates => _pt;

        public Coordinate GetCoordinate(int i) { return _pt[i]; }

        public void SetMaximum(PointPairDistance ptDist)
        {
            SetMaximum(ptDist._pt[0], ptDist._pt[1]);
        }

        public void SetMaximum(Coordinate p0, Coordinate p1)
        {
            if (_isNull)
            {
                Initialize(p0, p1);
                return;
            }
            double dist = p0.Distance(p1);
            if (dist > _distance)
                Initialize(p0, p1, dist);
        }

        public void SetMinimum(PointPairDistance ptDist)
        {
            SetMinimum(ptDist._pt[0], ptDist._pt[1]);
        }

        public void SetMinimum(Coordinate p0, Coordinate p1)
        {
            if (_isNull)
            {
                Initialize(p0, p1);
                return;
            }
            double dist = p0.Distance(p1);
            if (dist < _distance)
                Initialize(p0, p1, dist);
        }
    }
}