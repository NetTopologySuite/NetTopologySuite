using System;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm.Distance
{
    public class DiscreteHausdorffDistance<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly IGeometry<TCoordinate> _g0;
        private readonly IGeometry<TCoordinate> _g1;
        private readonly PointPairDistance<TCoordinate> _ptDist = new PointPairDistance<TCoordinate>();
        /**
        * Value of 0.0 indicates not set
        */
        private Double _densifyFrac;

        ///<summary>
        /// Constructs an item of this class
        ///</summary>
        ///<param name="g0">first geometry</param>
        ///<param name="g1">second geometry</param>
        public DiscreteHausdorffDistance(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1)
        {
            _g0 = g0;
            _g1 = g1;
        }

        /**
         * Sets the fraction by which to densify each segment.
         * Each segment will be split into a number of equal-length
         * subsegments, whose fraction of the total length is closest ]
         * to the given fraction.
         * 
         * @param densifyPercent
         */

        public Double DensifyFraction
        {
            get { return _densifyFrac; }
            set
            {
                if (value > 1.0
                    || value <= 0.0)
                    throw new ArgumentOutOfRangeException("value", "Fraction is not in range (0.0 - 1.0]");

                _densifyFrac = value;
            }
        }

        public Pair<TCoordinate> Coordinates
        {
            get { return _ptDist.Coordinates; }
        }

        public static Double Distance(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1)
        {
            DiscreteHausdorffDistance<TCoordinate> dist = new DiscreteHausdorffDistance<TCoordinate>(g0, g1);
            return dist.Distance();
        }

        public static Double Distance(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1, Double densityFraction)
        {
            DiscreteHausdorffDistance<TCoordinate> dist = new DiscreteHausdorffDistance<TCoordinate>(g0, g1);
            dist.DensifyFraction = densityFraction;
            return dist.Distance();
        }

        public Double Distance()
        {
            compute(_g0, _g1);
            return _ptDist.Distance;
        }

        public Double OrientedDistance()
        {
            computeOrientedDistance(_g0, _g1, _ptDist);
            return _ptDist.Distance;
        }

        private void compute(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1)
        {
            computeOrientedDistance(_g0, _g1, _ptDist);
            computeOrientedDistance(_g1, _g0, _ptDist);
        }

        private void computeOrientedDistance(IGeometry<TCoordinate> discreteGeom, IGeometry<TCoordinate> geom,
                                             PointPairDistance<TCoordinate> ptDist)
        {
            MaxPointDistanceFilter distFilter = new MaxPointDistanceFilter(geom);
            ptDist.SetMaximum(distFilter.GetMaxPointDistance(discreteGeom));

            if (_densifyFrac > 0)
            {
                MaxDensifiedByFractionDistanceFilter fracFilter = new MaxDensifiedByFractionDistanceFilter(geom,
                                                                                                           _densifyFrac);
                ptDist.SetMaximum(fracFilter.GetMaxPointDistance(discreteGeom));
            }
        }

        #region Nested type: MaxDensifiedByFractionDistanceFilter

        public class MaxDensifiedByFractionDistanceFilter
        {
            private readonly ICoordinateFactory<TCoordinate> _coordFact;
            private readonly IGeometry<TCoordinate> _geom;
            private readonly PointPairDistance<TCoordinate> _maxPtDist = new PointPairDistance<TCoordinate>();
            private readonly PointPairDistance<TCoordinate> _minPtDist = new PointPairDistance<TCoordinate>();
            private readonly int _numSubSegs;

            public MaxDensifiedByFractionDistanceFilter(IGeometry<TCoordinate> geom, Double fraction)
            {
                _geom = geom;
                _coordFact = geom.Coordinates.CoordinateFactory;
                _numSubSegs = Convert.ToInt32(1d/fraction); //Math.rint(1.0/fraction);
            }

            public void Filter(ICoordinateSequence<TCoordinate> seq)
                //Pair<ICoordinateSequence<TCoordinate>, int> arg) // ICoordinateSequence<TCoordinate> seq, int index) 
            {
                /**
                 * This logic also handles skipping Point geometries
                 */
                if (seq.Count < 2)
                    return;

                foreach (Pair<TCoordinate> pair in Slice.GetOverlappingPairs(seq))
                {
                    TCoordinate p0 = pair.First;
                    TCoordinate p1 = pair.Second;

                    Double delx = (p1[Ordinates.X] - p0[Ordinates.X])/_numSubSegs;
                    Double dely = (p1[Ordinates.Y] - p0[Ordinates.Y])/_numSubSegs;

                    for (int i = 0; i < _numSubSegs; i++)
                    {
                        Double x = p0[Ordinates.X] + i*delx;
                        Double y = p0[Ordinates.Y] + i*dely;
                        TCoordinate pt = _coordFact.Create(x, y); // new Coordinate(x, y);
                        _minPtDist.Initialize();
                        EuclideanDistanceToPoint<TCoordinate>.ComputeDistance(_coordFact, _geom, pt, _minPtDist);
                        _maxPtDist.SetMaximum(_minPtDist);
                    }
                }
                //TCoordinate p0 = seq[index - 1];
                //TCoordinate p1 = seq[index];

                //Double delx = (p1[Ordinates.X] - p0[Ordinates.X]) / _numSubSegs;
                //Double dely = (p1[Ordinates.Y] - p0[Ordinates.Y]) / _numSubSegs;

                //for (int i = 0; i < _numSubSegs; i++)
                //{
                //    Double x = p0[Ordinates.X] + i * delx;
                //    Double y = p0[Ordinates.Y] + i * dely;
                //    TCoordinate pt = _coordFact.Create(x, y);// new Coordinate(x, y);
                //    _minPtDist.Initialize();
                //    EuclideanDistanceToPoint<TCoordinate>.ComputeDistance(_coordFact, _geom, pt, _minPtDist);
                //    _maxPtDist.SetMaximum(_minPtDist);
                //}
            }

            public PointPairDistance<TCoordinate> GetMaxPointDistance(IGeometry<TCoordinate> discreteGeom)
            {
                CoordinateSequenceFilter.Apply(
                    CoordinateSequenceFilter.Filter(discreteGeom), Filter);
                return _maxPtDist;
            }
        }

        #endregion

        #region Nested type: MaxPointDistanceFilter

        public class MaxPointDistanceFilter //: CoordinateFilter
        {
            private readonly ICoordinateFactory<TCoordinate> _coordFact;
            private readonly IGeometry<TCoordinate> _geom;
            private readonly PointPairDistance<TCoordinate> _maxPtDist = new PointPairDistance<TCoordinate>();
            private readonly PointPairDistance<TCoordinate> _minPtDist = new PointPairDistance<TCoordinate>();

            public MaxPointDistanceFilter(IGeometry<TCoordinate> geom)
            {
                _geom = geom;
                _coordFact = geom.Coordinates.CoordinateFactory;
            }

            private void Filter(TCoordinate pt)
            {
                _minPtDist.Initialize();
                EuclideanDistanceToPoint<TCoordinate>.ComputeDistance(_coordFact, _geom, pt, _minPtDist);
                _maxPtDist.SetMaximum(_minPtDist);
            }

            public PointPairDistance<TCoordinate> GetMaxPointDistance(IGeometry<TCoordinate> discreteGeom)
            {
                Enumerable.Apply(discreteGeom.Coordinates, Filter);
                return _maxPtDist;
            }
        }

        #endregion
    }
}