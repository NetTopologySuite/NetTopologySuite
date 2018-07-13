using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm.Distance
{
    ///<summary>
    /// An algorithm for computing a distance metric
    /// which is an approximation to the Hausdorff Distance
    /// based on a discretization of the input <see cref="IGeometry"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The algorithm computes the Hausdorff distance restricted to discrete points
    /// for one of the geometries.
    /// The points can be either the vertices of the geometries (the default),
    /// or the geometries with line segments densified by a given fraction.
    /// Also determines two points of the Geometries which are separated by the computed distance.
    /// </para>
    /// <para>
    /// This algorithm is an approximation to the standard Hausdorff distance.
    /// Specifically,
    /// <code>
    /// for all geometries a, b:    DHD(a, b) &lt;= HD(a, b)
    /// </code>
    /// The approximation can be made as close as needed by densifying the input geometries.
    /// In the limit, this value will approach the true Hausdorff distance:
    /// <code>
    /// DHD(A, B, densifyFactor) -> HD(A, B) as densifyFactor -> 0.0
    /// </code>
    /// The default approximation is exact or close enough for a large subset of useful cases.
    /// </para>
    /// <para>
    /// Examples of these are:
    /// <list type="Bullet">
    /// <item>
    /// computing distance between Linestrings that are roughly parallel to each other,
    /// and roughly equal in length.  This occurs in matching linear networks.
    /// </item>
    /// <item>Testing similarity of geometries.</item>
    /// </list>
    /// </para>
    /// <para>
    /// An example where the default approximation is not close is:
    /// <code>
    /// A = LINESTRING (0 0, 100 0, 10 100, 10 100)
    /// B = LINESTRING (0 100, 0 10, 80 10)
    ///
    /// DHD(A, B) = 22.360679774997898
    /// HD(A, B) ~= 47.8
    /// </code>
    /// </para>
    /// </remarks>
    public class DiscreteHausdorffDistance
    {
        /// <summary>
        /// Computes the Discrete Hausdorff Distance of two <see cref="IGeometry"/>s.
        /// </summary>
        /// <param name="g0">A geometry</param>
        /// <param name="g1">A geometry</param>
        /// <returns>The Discrete Hausdorff Distance</returns>
        public static double Distance(IGeometry g0, IGeometry g1)
        {
            var dist = new DiscreteHausdorffDistance(g0, g1);
            return dist.Distance();
        }

        /// <summary>
        /// Computes the Discrete Hausdorff Distance of two <see cref="IGeometry"/>s.
        /// </summary>
        /// <param name="g0">A geometry</param>
        /// <param name="g1">A geometry</param>
        /// <param name="densifyFraction">The densify fraction. A value of 0 indicates, that no densification should take place</param>
        /// <returns>The Discrete Hausdorff Distance</returns>
        public static double Distance(IGeometry g0, IGeometry g1, double densifyFraction)
        {
            var dist = new DiscreteHausdorffDistance(g0, g1);
            dist.DensifyFraction = densifyFraction;
            return dist.Distance();
        }

        private readonly IGeometry _g0;
        private readonly IGeometry _g1;
        private readonly PointPairDistance _ptDist = new PointPairDistance();
        /**
         * Value of 0.0 indicates that no densification should take place
         */
        private double _densifyFrac;

        /// <summary>
        /// Creates an instance of this class using the provided geometries
        /// </summary>
        /// <param name="g0">A geometry</param>
        /// <param name="g1">Another geometry</param>
        public DiscreteHausdorffDistance(IGeometry g0, IGeometry g1)
        {
            _g0 = g0;
            _g1 = g1;
        }

        ///<summary>
        /// Gets/sets the fraction by which to densify each segment.
        ///</summary>
        /// <remarks>
        /// Each segment will be (virtually) split into a number of equal-length
        /// subsegments, whose fraction of the total length is closest
        /// to the given fraction.
        /// </remarks>
        public double DensifyFraction
        {
            get => _densifyFrac;
            set
            {
                if (value > 1.0
                    || value <= 0.0)
                    throw new ArgumentOutOfRangeException("value", @"Fraction is not in range (0.0 - 1.0]");

                _densifyFrac = value;
            }
        }

        public double Distance()
        {
            Compute(_g0, _g1);
            return _ptDist.Distance;
        }

        public double OrientedDistance()
        {
            ComputeOrientedDistance(_g0, _g1, _ptDist);
            return _ptDist.Distance;
        }

        public Coordinate[] Coordinates => _ptDist.Coordinates;

        private void Compute(IGeometry g0, IGeometry g1)
        {
            ComputeOrientedDistance(g0, g1, _ptDist);
            ComputeOrientedDistance(g1, g0, _ptDist);
        }

        private void ComputeOrientedDistance(IGeometry discreteGeom, IGeometry geom, PointPairDistance ptDist)
        {
            var distFilter = new MaxPointDistanceFilter(geom);
            discreteGeom.Apply(distFilter);
            ptDist.SetMaximum(distFilter.MaxPointDistance);

            if (_densifyFrac > 0)
            {
                var fracFilter = new MaxDensifiedByFractionDistanceFilter(geom, _densifyFrac);
                discreteGeom.Apply(fracFilter);
                ptDist.SetMaximum(fracFilter.MaxPointDistance);

            }
        }

        public class MaxPointDistanceFilter
            : ICoordinateFilter
        {
            private readonly PointPairDistance _maxPtDist = new PointPairDistance();
            private readonly PointPairDistance _minPtDist = new PointPairDistance();
            //private EuclideanDistanceToPoint euclideanDist = new EuclideanDistanceToPoint();
            private readonly IGeometry _geom;

            public MaxPointDistanceFilter(IGeometry geom)
            {
                _geom = geom;
            }

            public void Filter(Coordinate pt)
            {
                _minPtDist.Initialize();
                DistanceToPoint.ComputeDistance(_geom, pt, _minPtDist);
                _maxPtDist.SetMaximum(_minPtDist);
            }

            public PointPairDistance MaxPointDistance => _maxPtDist;
        }

        public class MaxDensifiedByFractionDistanceFilter
        : ICoordinateSequenceFilter
        {
            private readonly PointPairDistance _maxPtDist = new PointPairDistance();
            private readonly PointPairDistance _minPtDist = new PointPairDistance();
            private readonly IGeometry _geom;
            private readonly int _numSubSegs;

            public MaxDensifiedByFractionDistanceFilter(IGeometry geom, double fraction)
            {
                _geom = geom;
                _numSubSegs = (int)Math.Round(1.0 / fraction, MidpointRounding.ToEven); //see Java's Math.rint
            }

            public void Filter(ICoordinateSequence seq, int index)
            {
                /**
                 * This logic also handles skipping Point geometries
                 */
                if (index == 0)
                    return;

                var p0 = seq.GetCoordinate(index - 1);
                var p1 = seq.GetCoordinate(index);

                double delx = (p1.X - p0.X) / _numSubSegs;
                double dely = (p1.Y - p0.Y) / _numSubSegs;

                for (int i = 0; i < _numSubSegs; i++)
                {
                    double x = p0.X + i * delx;
                    double y = p0.Y + i * dely;
                    var pt = new Coordinate(x, y);
                    _minPtDist.Initialize();
                    DistanceToPoint.ComputeDistance(_geom, pt, _minPtDist);
                    _maxPtDist.SetMaximum(_minPtDist);
                }

            }

            public bool GeometryChanged => false;

            public bool Done => false;

            public PointPairDistance MaxPointDistance => _maxPtDist;
        }

    }
}