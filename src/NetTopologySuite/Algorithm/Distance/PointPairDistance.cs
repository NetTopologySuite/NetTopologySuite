using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Algorithm.Distance
{
    /// <summary>
    /// Contains a pair of points and the distance between them.
    /// </summary>
    /// <remarks>
    /// Provides methods to update with a new point pair with either maximum or minimum distance.
    /// </remarks>
    public class PointPairDistance
    {
        private readonly Coordinate[] _pt = { new Coordinate(), new Coordinate() };
        private double _distance = double.NaN;
        private bool _isNull = true;

        /// <summary>
        /// Initializes to null.
        /// </summary>
        public void Initialize() { _isNull = true; }

        /// <summary>
        /// Initializes the points.
        /// </summary>
        /// <param name="p0">1st coordinate</param>
        /// <param name="p1">2nd coordinate</param>
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
        /// <param name="p0">1st coordinate</param>
        /// <param name="p1">2nd coordinate</param>
        /// <param name="distance">the distance between <see paramref="p0"/> and <see paramref="p1"/></param>
        private void Initialize(Coordinate p0, Coordinate p1, double distance)
        {
            _pt[0].CoordinateValue = p0;
            _pt[1].CoordinateValue = p1;
            _distance = distance;
            _isNull = false;
        }

        /// <summary>
        /// The distance between the paired coordinates
        /// </summary>
        /// <returns>The distance between the paired coordinates</returns>
        public double Distance => _distance;

        /// <summary>
        /// Gets a value indicating the paired coordinates.
        /// </summary>
        /// <returns>An array containing the paired points</returns>
        public Coordinate[] Coordinates => _pt;

        /// <summary>
        /// Gets the value of one of the paired points
        /// </summary>
        /// <param name="i">An index, valid are [0, 1].</param>
        /// <returns>The <c>Coordinate</c> at index <c>i</c>.</returns>
        public Coordinate this[int i] => _pt[i];


        /// <summary>
        /// Updates <c>this</c> <c>PointPairDistance</c> if <paramref name="ptDist"/>
        /// has greater <see cref="Distance"/> than <c>this</c> instance.
        /// </summary>
        /// <param name="ptDist">The <c>PointPairDistance</c> to test.</param>
        public void SetMaximum(PointPairDistance ptDist)
        {
            if (_isNull || ptDist.Distance > Distance)
                Initialize(ptDist[0], ptDist[1], ptDist.Distance);
            //SetMaximum(ptDist._pt[0], ptDist._pt[1]);
        }

        /// <summary>
        /// Updates <c>this</c> <c>PointPairDistance</c> if the distance between
        /// <paramref name="p0"/> and <paramref name="p1"/> is greater than the
        /// <see cref="Distance"/> of <c>this</c> instance.
        /// </summary>
        /// <param name="p0">The 1st point's coordinate</param>
        /// <param name="p1">The 2nd point's coordinate</param>
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

        /// <summary>
        /// Updates <c>this</c> <c>PointPairDistance</c> if <paramref name="ptDist"/>
        /// has a smaller <see cref="Distance"/> than <c>this</c> instance.
        /// </summary>
        /// <param name="ptDist">The <c>PointPairDistance</c> to test.</param>
        public void SetMinimum(PointPairDistance ptDist)
        {
            if (_isNull || ptDist.Distance < Distance)
                Initialize(ptDist[0], ptDist[1], ptDist.Distance);
            //SetMinimum(ptDist._pt[0], ptDist._pt[1]);
        }

        /// <summary>
        /// Updates <c>this</c> <c>PointPairDistance</c> if the distance between
        /// <paramref name="p0"/> and <paramref name="p1"/> is smaller than the
        /// <see cref="Distance"/> of <c>this</c> instance.
        /// </summary>
        /// <param name="p0">The 1st point's coordinate</param>
        /// <param name="p1">The 2nd point's coordinate</param>
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

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            return WKTWriter.ToLineString(_pt[0], _pt[1]);
        }
    }
}
