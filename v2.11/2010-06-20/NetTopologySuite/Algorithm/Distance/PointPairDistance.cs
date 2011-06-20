using System;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm.Distance
{
    public class PointPairDistance<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private Double _distance = Double.NaN;
        private Boolean _isNull = true;
        private Pair<TCoordinate> _pair;

        public Double Distance
        {
            get { return _distance; }
        }

        public Pair<TCoordinate> Coordinates
        {
            get { return _pair; }
        }

        public void Initialize()
        {
            _isNull = true;
            _distance = Double.NaN;
            _pair = new Pair<TCoordinate>();
        }

        public void Initialize(Pair<TCoordinate> pair)
        {
            _pair = pair;
            _distance = pair.First.Distance(pair.Second);
            _isNull = false;
        }

        public void Initialize(Pair<TCoordinate> pair, Double distance)
        {
            _pair = pair;
            _distance = distance;
            _isNull = false;
        }

        //public ICoordinate<TCoordinate>[] Coordinate
        //{
        //    get
        //    {
        //        return _pt[index];
        //    }
        //}

        public void SetMaximum(PointPairDistance<TCoordinate> ptDist)
        {
            SetMaximum(ptDist.Coordinates);
        }

        public void SetMaximum(Pair<TCoordinate> pair)
        {
            if (_isNull)
            {
                Initialize(pair);
                return;
            }
            Double dist = pair[0].Distance(pair[1]);
            if (dist > Distance)
                Initialize(pair, dist);
        }

        public void SetMinimum(PointPairDistance<TCoordinate> ptDist)
        {
            SetMinimum(ptDist.Coordinates);
        }

        public void SetMinimum(Pair<TCoordinate> pair)
        {
            if (_isNull)
            {
                Initialize(pair);
                return;
            }
            Double dist = pair[0].Distance(pair[1]);
            if (dist < Distance)
                Initialize(pair, dist);
        }
    }
}