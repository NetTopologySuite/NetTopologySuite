
using System;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm.Distance;
using NetTopologySuite.Geometries.Utilities;
using NPack.Interfaces;
#if DOTNET35
using sl = System.Linq;
#endif

namespace NetTopologySuite.Operation.Buffer.Validate
{
    public class BufferCurveMaximumDistanceFinder<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IDivisible<Double, TCoordinate>, IConvertible
    {
        private IGeometry<TCoordinate> _inputGeom;
        private PointPairDistance<TCoordinate> _maxPtDist = new PointPairDistance<TCoordinate>();

        public BufferCurveMaximumDistanceFinder(IGeometry<TCoordinate> inputGeom)
        {
            _inputGeom = inputGeom;
        }

        public Double FindDistance(IGeometry<TCoordinate> bufferCurve)
        {
            ComputeMaxVertexDistance(bufferCurve);
            ComputeMaxMidpointDistance(bufferCurve);
            return _maxPtDist.Distance;
        }

        public PointPairDistance<TCoordinate> DistancePoints
        {
            get
            {
                return _maxPtDist;
            }
        }

        private void ComputeMaxVertexDistance(IGeometry<TCoordinate> curve)
        {
            MaxPointDistanceFilter distFilter = new MaxPointDistanceFilter(curve);
            _maxPtDist.SetMaximum(distFilter.GetMaxPointDistance());
        }

        private void ComputeMaxMidpointDistance(IGeometry<TCoordinate> curve)
        {
            MaxMidpointDistanceFilter distFilter = new MaxMidpointDistanceFilter(curve);
            _maxPtDist.SetMaximum(distFilter.GetMaxPointDistance());
        }

        public class MaxPointDistanceFilter //implements CoordinateFilter
        {
            private PointPairDistance<TCoordinate> _maxPtDist = new PointPairDistance<TCoordinate>();
            private PointPairDistance<TCoordinate> _minPtDist = new PointPairDistance<TCoordinate>();
            private IGeometry<TCoordinate> _geom;
            private ICoordinateFactory<TCoordinate> _coordFact;

            public MaxPointDistanceFilter(IGeometry<TCoordinate> geom)
            {
                _geom = geom;
                _coordFact = geom.Coordinates.CoordinateFactory;
            }

            public void Filter(TCoordinate pt)
            {
                _minPtDist.Initialize();
                DistanceToPointFinder<TCoordinate>.ComputeDistance(_coordFact,_geom, pt, _minPtDist);
                _maxPtDist.SetMaximum(_minPtDist);
            }

            public PointPairDistance<TCoordinate> GetMaxPointDistance()
            {
                Enumerable.Apply(_geom.Coordinates, Filter);
                return _maxPtDist;
            }
        }

        public class MaxMidpointDistanceFilter // implements CoordinateSequenceFilter 
        {
            private PointPairDistance<TCoordinate> _maxPtDist = new PointPairDistance<TCoordinate>();
            private PointPairDistance<TCoordinate> _minPtDist = new PointPairDistance<TCoordinate>();
            private IGeometry<TCoordinate> _geom;

            public MaxMidpointDistanceFilter(IGeometry<TCoordinate> geom)
            {
                _geom = geom;
            }

            public void Filter(ICoordinateSequence<TCoordinate> seq)
            {
                /**
                 * This logic also handles skipping Point geometries
                 */
                if (seq.Count < 2)
                    return;
                ICoordinateFactory<TCoordinate> coordFact = seq.CoordinateFactory;
                foreach (var pair in Slice.GetOverlappingPairs(seq))
                {
                    TCoordinate p0 = pair.First;
                    TCoordinate p1 = pair.Second;

                    Double x = (p0[Ordinates.X] + p1[Ordinates.X]) * 0.5d;
                    Double y = (p0[Ordinates.Y] + p1[Ordinates.Y]) * 0.5d;

                    TCoordinate midPt = coordFact.Create(x, y);// new Coordinate(x, y);
                    _minPtDist.Initialize();
                    DistanceToPointFinder<TCoordinate>.ComputeDistance(coordFact, _geom, midPt, _minPtDist);
                    _maxPtDist.SetMaximum(_minPtDist);
                }
            }


            public PointPairDistance<TCoordinate> GetMaxPointDistance()
            {
                CoordinateSequenceFilter.Apply(CoordinateSequenceFilter.Filter(_geom), Filter);
                return _maxPtDist;
            }
        }
    }
    
}
