using System;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;

namespace NetTopologySuite.Operation.Distance
{
    /// <summary>
    /// Represents a sequence of facets (points or line segments) of a <see cref="IGeometry"/>
    /// specified by a subsequence of a <see cref="ICoordinateSequence"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    public class FacetSequence
    {
        private readonly ICoordinateSequence _pts;
        private readonly int _start;
        private readonly int _end;

        // temporary Coordinates to materialize points from the CoordinateSequence
        private readonly Coordinate _pt = new Coordinate();
        private readonly Coordinate _seqPt = new Coordinate();

        /// <summary>
        /// Creates a new section based on a CoordinateSequence.
        /// </summary>
        /// <param name="pts">The sequence holding the points in the section</param>
        /// <param name="start">The index of the start point</param>
        /// <param name="end">The index of the end point + 1</param>
        public FacetSequence(ICoordinateSequence pts, int start, int end)
        {
            _pts = pts;
            _start = start;
            _end = end;
        }

        /// <summary>
        /// Creates a new sequence for a single point from a CoordinateSequence.
        /// </summary>
        /// <param name="pts">The sequence holding the points in the facet sequence</param>
        /// <param name="index">the index of the point</param>
        public FacetSequence(ICoordinateSequence pts, int index)
        {
            _pts = pts;
            _start = index;
            _end = index + 1;
        }

        /// <summary>
        /// Gets the envelope of this facet sequence
        /// </summary>
        public Envelope Envelope
        {
            get
            {
                var env = new Envelope();
                for (int i = _start; i < _end; i++)
                {
                    env.ExpandToInclude(_pts.GetX(i), _pts.GetY(i));
                }
                return env;
            }
        }

        /// <summary>
        /// Gets the number of coordinates in this facet sequence
        /// </summary>
        public int Count => _end - _start;

        /// <summary>
        /// Gets the coordinate at the given index
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The coordinate at the given index</returns>
        public Coordinate GetCoordinate(int index)
        {
            return _pts.GetCoordinate(_start + index);
        }

        /// <summary>
        /// Tests if this facet sequence consists of only one point
        /// </summary>
        public bool IsPoint => _end - _start == 1;

        /// <summary>
        /// Computes the distance to another facet sequence
        /// </summary>
        /// <param name="facetSeq">The other facet sequence</param>
        /// <returns>The distance between this and <paramref name="facetSeq"/>.</returns>
        public double Distance(FacetSequence facetSeq)
        {
            bool isPoint = IsPoint;
            bool isPointOther = facetSeq.IsPoint;

            if (isPoint && isPointOther)
            {
                _pts.GetCoordinate(_start, _pt);
                facetSeq._pts.GetCoordinate(facetSeq._start, _seqPt);
                return _pt.Distance(_seqPt);
            }

            if (isPoint)
            {
                _pts.GetCoordinate(_start, _pt);
                return ComputePointLineDistance(_pt, facetSeq);
            }

            if (isPointOther)
            {
                facetSeq._pts.GetCoordinate(facetSeq._start, _seqPt);
                return ComputePointLineDistance(_seqPt, this);
            }

            return ComputeLineLineDistance(facetSeq);

        }

        //// temporary Coordinates to materialize points from the CoordinateSequence
        //private readonly Coordinate _p0 = new Coordinate();
        //private readonly Coordinate _p1 = new Coordinate();
        //private readonly Coordinate _q0 = new Coordinate();
        //private readonly Coordinate _q1 = new Coordinate();

        private double ComputeLineLineDistance(FacetSequence facetSeq)
        {
            // both linear - compute minimum segment-segment distance
            double minDistance = double.MaxValue;

            var p0 = new Coordinate();
            var p1 = new Coordinate();
            var q0 = new Coordinate();
            var q1 = new Coordinate();

            for (int i = _start; i < _end - 1; i++)
            {
                for (int j = facetSeq._start; j < facetSeq._end - 1; j++)
                {
                    _pts.GetCoordinate(i, p0);
                    _pts.GetCoordinate(i + 1, p1);
                    facetSeq._pts.GetCoordinate(j, q0);
                    facetSeq._pts.GetCoordinate(j + 1, q1);

                    double dist = DistanceComputer.SegmentToSegment(p0, p1, q0, q1);
                    if (dist == 0.0)
                        return 0.0;
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                    }
                }
            }
            return minDistance;
        }

        private static double ComputePointLineDistance(Coordinate pt, FacetSequence facetSeq)
        {
            double minDistance = double.MaxValue;

            var q0 = new Coordinate();
            var q1 = new Coordinate();
            for (int i = facetSeq._start; i < facetSeq._end - 1; i++)
            {
                facetSeq._pts.GetCoordinate(i, q0);
                facetSeq._pts.GetCoordinate(i + 1, q1);
                double dist = DistanceComputer.PointToSegment(pt, q0, q1);
                if (dist == 0.0) return 0.0;
                if (dist < minDistance)
                {
                    minDistance = dist;
                }
            }
            return minDistance;
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            var buf = new StringBuilder();
            buf.Append(IsPoint ? "LINESTRING ( " : "POINT (");
            var p = new Coordinate();
            for (int i = _start; i < _end; i++)
            {
                if (i > _start)
                    buf.Append(", ");
                _pts.GetCoordinate(i, p);
                buf.Append(p.X + " " + p.Y);
            }
            buf.Append(" )");
            return buf.ToString();
        }
    }
}