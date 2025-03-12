using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm.Distance
{
    /// <summary>
    /// An algorithm for computing a distance metric
    /// which is an approximation to the Hausdorff Distance
    /// based on a discretization of the input <see cref="Geometry"/>.
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
    /// <list type="bullet">
    /// <item><description>
    /// computing distance between <c>Linestring</c>s that are roughly parallel to each other,
    /// and roughly equal in length.  This occurs in matching linear networks.
    /// </description></item>
    /// <item><description>Testing similarity of geometries.</description></item>
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
        /// Computes the Discrete Hausdorff Distance of two <see cref="Geometry"/>s.
        /// </summary>
        /// <param name="g0">A geometry</param>
        /// <param name="g1">A geometry</param>
        /// <returns>The Discrete Hausdorff Distance</returns>
        public static double Distance(Geometry g0, Geometry g1)
        {
            var dist = new DiscreteHausdorffDistance(g0, g1);
            return dist.Distance();
        }

        /// <summary>
        /// Computes the Discrete Hausdorff Distance of two <see cref="Geometry"/>s.
        /// </summary>
        /// <param name="g0">A geometry</param>
        /// <param name="g1">A geometry</param>
        /// <param name="densifyFraction">The densify fraction. A value of 0 indicates, that no densification should take place</param>
        /// <returns>The Discrete Hausdorff Distance</returns>
        public static double Distance(Geometry g0, Geometry g1, double densifyFraction)
        {
            var dist = new DiscreteHausdorffDistance(g0, g1);
            dist.DensifyFraction = densifyFraction;
            return dist.Distance();
        }

        private readonly Geometry _g0;
        private readonly Geometry _g1;
        private readonly PointPairDistance _ptDist = new PointPairDistance();

        /// <summary>
        /// Value of 0.0 indicates that no densification should take place
        /// </summary>
        private double _densifyFrac;

        /// <summary>
        /// Creates an instance of this class using the provided geometries
        /// </summary>
        /// <param name="g0">A geometry</param>
        /// <param name="g1">Another geometry</param>
        public DiscreteHausdorffDistance(Geometry g0, Geometry g1)
        {
            _g0 = g0;
            _g1 = g1;
        }

        /// <summary>
        /// Gets or sets the fraction by which to densify each segment.
        /// </summary>
        /// <remarks>
        /// Each segment will be (virtually) split into a number of equal-length
        /// sub-segments, whose fraction of the total length is closest
        /// to the given fraction.
        /// </remarks>
        public double DensifyFraction
        {
            get => _densifyFrac;
            set
            {
                if (value > 1.0 || value <= 0.0)
                    throw new ArgumentOutOfRangeException("value", @"Fraction is not in range (0.0 - 1.0]");

                _densifyFrac = value;
            }
        }

        /// <summary>
        /// Computes the discrete hausdorff distance between the two assigned geometries.
        /// </summary>
        /// <returns>The discrete hausdorff distance</returns>
        public double Distance()
        {
            Compute(_g0, _g1);
            return _ptDist.Distance;
        }

        /// <summary>
        /// Computes the discrete hausdorff distance between the 1st and the 2nd assigned geometry
        /// </summary>
        /// <returns>The discrete hausdorff distance.</returns>
        public double OrientedDistance()
        {
            ComputeOrientedDistance(_g0, _g1, _ptDist);
            return _ptDist.Distance;
        }

        /// <summary>
        /// Gets a value indicating the 
        /// </summary>
        public Coordinate[] Coordinates => _ptDist.Coordinates;

        private void Compute(Geometry g0, Geometry g1)
        {
            ComputeOrientedDistance(g0, g1, _ptDist);
            ComputeOrientedDistance(g1, g0, _ptDist);
        }

        private void ComputeOrientedDistance(Geometry discreteGeom, Geometry geom, PointPairDistance ptDist)
        {
            var distFilter = new MaxPointDistanceFilter(geom);
            discreteGeom.Apply(distFilter);
            ptDist.SetMaximum(distFilter.MaxPointDistance);

            if (_densifyFrac > 0)
            {
                var fracFilter = new MaxDensifiedByFractionDistanceFilter(geom, _densifyFrac);
                discreteGeom.Apply((IEntireCoordinateSequenceFilter)fracFilter);
                ptDist.SetMaximum(fracFilter.MaxPointDistance);

            }
        }

        /// <summary>
        /// A coordinate filter that computes the maximum <see cref="PointPairDistance"/> between points of
        /// an assigned <c>Geometry</c> and all filtered geometries.
        /// </summary>
        public class MaxPointDistanceFilter : ICoordinateFilter
        {
            private readonly PointPairDistance _maxPtDist = new PointPairDistance();
            private readonly PointPairDistance _minPtDist = new PointPairDistance();
            private readonly Geometry _geom;

            /// <summary>
            /// Creates an instance of this class
            /// </summary>
            /// <param name="geom">A geometry</param>
            public MaxPointDistanceFilter(Geometry geom)
            {
                _geom = geom;
            }

            /// <inheritdoc cref="ICoordinateFilter.Filter"/>
            public void Filter(Coordinate pt)
            {
                _minPtDist.Initialize();
                DistanceToPoint.ComputeDistance(_geom, pt, _minPtDist);
                _maxPtDist.SetMaximum(_minPtDist);
            }

            /// <summary>
            /// Gets a value indicating the maximum distance between
            /// an assigned <c>Geometry</c> and the filtered one.
            /// </summary>
            public PointPairDistance MaxPointDistance => _maxPtDist;
        }

        /// <summary>
        /// A coordinate filter that computes the maximum <see cref="PointPairDistance"/> between points of
        /// an assigned <c>Geometry</c> and all filtered geometries. The filtered geometries' line segments
        /// are 
        /// </summary>
        public class MaxDensifiedByFractionDistanceFilter : ICoordinateSequenceFilter, IEntireCoordinateSequenceFilter
        {
            private readonly PointPairDistance _maxPtDist = new PointPairDistance();
            //private readonly PointPairDistance _minPtDist = new PointPairDistance();
            private readonly Geometry _geom;
            private readonly int _numSubSegs;

            /// <summary>
            /// Creates an instance of this filter class
            /// </summary>
            /// <param name="geom">The geometry to densify</param>
            /// <param name="fraction">The densification fraction</param>
            public MaxDensifiedByFractionDistanceFilter(Geometry geom, double fraction)
            {
                _geom = geom;
                _numSubSegs = (int)Math.Round(1.0 / fraction, MidpointRounding.ToEven); //see Java's Math.rint
            }

            /// <inheritdoc cref="ICoordinateSequenceFilter.Filter"/>
            public void Filter(CoordinateSequence seq, int index)
            {
                /*
                 * This logic also handles skipping Point geometries
                 */
                if (index < 1)
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
                    var minPtDist = new PointPairDistance();
                    DistanceToPoint.ComputeDistance(_geom, pt, minPtDist);
                    _maxPtDist.SetMaximum(minPtDist);
                }
            }

            /// <inheritdoc cref="ICoordinateSequenceFilter.GeometryChanged"/>
            /// <returns>As this filter does not change the geometry, the return value is always <c>false</c></returns>
            public bool GeometryChanged => false;

            /// <inheritdoc cref="ICoordinateSequenceFilter.Done"/>
            /// <returns>As this filter does not end prematurely, the return value is always <c>false</c></returns>
            public bool Done => false;

            void IEntireCoordinateSequenceFilter.Filter(CoordinateSequence seq)
            {
                /*
                 * This logic also handles skipping Point geometries
                 */
                for (int index = 1; index < seq.Count; index++)
                {
                    var p0 = seq.GetCoordinate(index - 1);
                    var p1 = seq.GetCoordinate(index);

                    double delx = (p1.X - p0.X) / _numSubSegs;
                    double dely = (p1.Y - p0.Y) / _numSubSegs;

                    for (int i = 0; i < _numSubSegs; i++)
                    {
                        double x = p0.X + i * delx;
                        double y = p0.Y + i * dely;
                        var pt = new Coordinate(x, y);
                        var minPtDist = new PointPairDistance();
                        DistanceToPoint.ComputeDistance(_geom, pt, minPtDist);
                        _maxPtDist.SetMaximum(minPtDist);
                    }
                }
            }

            bool IEntireCoordinateSequenceFilter.Done => Done;

            bool IEntireCoordinateSequenceFilter.GeometryChanged => GeometryChanged;

            /// <summary>
            /// Gets a value indicating the maximum distance between p
            /// </summary>
            public PointPairDistance MaxPointDistance => _maxPtDist;
        }

    }
}
