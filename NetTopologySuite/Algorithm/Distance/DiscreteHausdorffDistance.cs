using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm.Distance
{
    ///<summary>
    /// Implements algorithm for computing a distance metric which can be thought of as the "Discrete Hausdorff Distance". 
    /// This is the Hausdorff distance restricted to discrete points for one of the geometries.
    ///</summary>
    /// <remarks>
    /// Also determines two points of the Geometries which are separated by the computed distance.
    /// <para>
    /// <b>NOTE:</b>The current implementation supports only vertices as the discrete locations.
    /// This could be extended to allow an arbitrary density of points to be used.
    /// </para>
    /// <para>
    /// <b>NOTE:</b> This algorithm is NOT equivalent to the standard Hausdorff distance.
    /// However, it computes an approximation that is correct for a large subset of useful cases.
    /// One important part of this subset is Linestrings that are roughly parallel to each other,
    /// and roughly equal in length.  This is a useful metric for line matching.
    /// </para>
    /// </remarks>
    public class DiscreteHausdorffDistance
    {
        public static double Distance(IGeometry g0, IGeometry g1)
        {
            DiscreteHausdorffDistance dist = new DiscreteHausdorffDistance(g0, g1);
            return dist.Distance();
        }

        public static double Distance(IGeometry g0, IGeometry g1, double densifyFraction)
        {
            DiscreteHausdorffDistance dist = new DiscreteHausdorffDistance(g0, g1);
            dist.DensifyFraction = densifyFraction;
            return dist.Distance();
        }

        private readonly IGeometry _g0;
        private readonly IGeometry _g1;
        private readonly PointPairDistance _ptDist = new PointPairDistance();
        /**
         * Value of 0.0 indicates not set
         */
        private double _densifyFrac;

        public DiscreteHausdorffDistance(IGeometry g0, IGeometry g1)
        {
            _g0 = g0;
            _g1 = g1;
        }

        ///<summary>
        /// Gets/sets the fraction by which to densify each segment.
        ///</summary>
        /// <remarks>
        /// Each segment will be split into a number of equal-length
        /// subsegments, whose fraction of the total length is closest ]
        /// to the given fraction.
        /// </remarks>
        public double DensifyFraction
        {
            get { return _densifyFrac; }
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

        public ICoordinate[] Coordinates { get { return _ptDist.Coordinates; } }

        private void Compute(IGeometry g0, IGeometry g1)
        {
            ComputeOrientedDistance(g0, g1, _ptDist);
            ComputeOrientedDistance(g1, g0, _ptDist);
        }

        private void ComputeOrientedDistance(IGeometry discreteGeom, IGeometry geom, PointPairDistance ptDist)
        {
            MaxPointDistanceFilter distFilter = new MaxPointDistanceFilter(geom);
            discreteGeom.Apply(distFilter);
            ptDist.SetMaximum(distFilter.MaxPointDistance);

            if (_densifyFrac > 0)
            {
                MaxDensifiedByFractionDistanceFilter fracFilter = new MaxDensifiedByFractionDistanceFilter(geom, _densifyFrac);
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
            private IGeometry geom;

            public MaxPointDistanceFilter(IGeometry geom)
            {
                this.geom = geom;
            }

            public void Filter(ICoordinate pt)
            {
                _minPtDist.Initialize();
                EuclideanDistanceToPoint.ComputeDistance(geom, pt, _minPtDist);
                _maxPtDist.SetMaximum(_minPtDist);
            }

            public PointPairDistance MaxPointDistance { get { return _maxPtDist; }}
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

                ICoordinate p0 = seq.GetCoordinate(index - 1);
                ICoordinate p1 = seq.GetCoordinate(index);

                double delx = (p1.X - p0.X) / _numSubSegs;
                double dely = (p1.Y - p0.Y) / _numSubSegs;

                for (int i = 0; i < _numSubSegs; i++)
                {
                    double x = p0.X + i * delx;
                    double y = p0.Y + i * dely;
                    ICoordinate pt = new Coordinate(x, y);
                    _minPtDist.Initialize();
                    EuclideanDistanceToPoint.ComputeDistance(_geom, pt, _minPtDist);
                    _maxPtDist.SetMaximum(_minPtDist);
                }


            }

            public Boolean GeometryChanged { get { return false; } }

            public Boolean Done { get { return false; } }

            public PointPairDistance MaxPointDistance
            {
                get { return _maxPtDist; }
            }
        }

    }
}