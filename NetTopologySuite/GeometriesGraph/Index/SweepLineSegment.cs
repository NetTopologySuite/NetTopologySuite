using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using NPack.Interfaces;

namespace NetTopologySuite.GeometriesGraph.Index
{
    public class SweepLineSegment<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly Edge<TCoordinate> _edge;
        private readonly Int32 _ptIndex;
        private readonly IEnumerable<TCoordinate> _pts;

        public SweepLineSegment(Edge<TCoordinate> edge, Int32 ptIndex)
        {
            _edge = edge;
            _ptIndex = ptIndex;
            _pts = edge.Coordinates;
        }

        public Double MinX
        {
            get
            {
                Pair<TCoordinate> pair = Slice.GetPairAt(_pts, _ptIndex).Value;
                Double x1 = pair.First[Ordinates.X];
                Double x2 = pair.Second[Ordinates.X];
                return x1 < x2 ? x1 : x2;
            }
        }

        public Double MaxX
        {
            get
            {
                Pair<TCoordinate> pair = Slice.GetPairAt(_pts, _ptIndex).Value;
                Double x1 = pair.First[Ordinates.X];
                Double x2 = pair.Second[Ordinates.X];
                return x1 > x2 ? x1 : x2;
            }
        }

        public void ComputeIntersections(SweepLineSegment<TCoordinate> ss, SegmentIntersector<TCoordinate> si)
        {
            si.AddIntersections(_edge, _ptIndex, ss._edge, ss._ptIndex);
        }
    }
}