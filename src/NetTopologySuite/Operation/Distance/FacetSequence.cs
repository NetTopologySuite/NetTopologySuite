using System.Text;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Distance
{
    /// <summary>
    /// Represents a sequence of facets (points or line segments) of a <see cref="Geometry"/>
    /// specified by a subsequence of a <see cref="CoordinateSequence"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    public class FacetSequence
    {
        private readonly Geometry _geom;
        private readonly CoordinateSequence _pts;
        private readonly int _start;
        private readonly int _end;

        /// <summary>
        /// Creates a new sequence of facets based on a <see cref="CoordinateSequence"/>
        /// contained in the given <see cref="Geometry"/>.
        /// </summary>
        /// <param name="geom">The geometry containing the facets.</param>
        /// <param name="pts">The sequence containing the facet points.</param>
        /// <param name="start">The index of the start point.</param>
        /// <param name="end">The index of the end point.</param>
        public FacetSequence(Geometry geom, CoordinateSequence pts, int start, int end)
        {
            _geom = geom;
            _pts = pts;
            _start = start;
            _end = end;
        }
        /// <summary>
        /// Creates a new sequence of facets based on a <see cref="CoordinateSequence"/>.
        /// </summary>
        /// <param name="pts">The sequence containing facet points.</param>
        /// <param name="start">The index of the start point</param>
        /// <param name="end">The index of the end point + 1</param>
        public FacetSequence(CoordinateSequence pts, int start, int end)
        {
            _pts = pts;
            _start = start;
            _end = end;
        }

        /// <summary>
        /// Creates a new sequence for a single point from a CoordinateSequence.
        /// </summary>
        /// <param name="pts">The sequence containing the facet point.</param>
        /// <param name="index">the index of the point</param>
        public FacetSequence(CoordinateSequence pts, int index)
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
        /// Computes the distance between this and another
        /// <see cref="FacetSequence"/>.
        /// </summary>
        /// <param name="facetSeq">The sequence to compute the distance to.</param>
        /// <returns>The minimum distance between the sequences.</returns>
        public double Distance(FacetSequence facetSeq)
        {
            bool isPoint = IsPoint;
            bool isPointOther = facetSeq.IsPoint;
            double distance;

            if (isPoint && isPointOther)
            {
                var pt = _pts.GetCoordinate(_start);
                var seqPt = facetSeq._pts.GetCoordinate(facetSeq._start);
                distance = pt.Distance(seqPt);
            }
            else if (isPoint)
            {
                var pt = _pts.GetCoordinate(_start);
                distance = ComputeDistancePointLine(pt, facetSeq, null);
            }
            else if (isPointOther)
            {
                var seqPt = facetSeq._pts.GetCoordinate(facetSeq._start);
                distance = ComputeDistancePointLine(seqPt, this, null);
            }
            else
            {
                distance = ComputeDistanceLineLine(facetSeq, null);
            }

            return distance;
        }

        /// <summary>
        /// Computes the locations of the nearest points between this sequence
        /// and another sequence.
        /// The locations are presented in the same order as the input sequences.
        /// </summary>
        /// <returns>A pair of <see cref="GeometryLocation"/>s for the nearest points.</returns>
        public GeometryLocation[] NearestLocations(FacetSequence facetSeq)
        {
            bool isPoint = IsPoint;
            bool isPointOther = facetSeq.IsPoint;
            var locs = new GeometryLocation[2];

            if (isPoint && isPointOther)
            {
                // DEVIATION (minor): JTS uses "new Coordinate(GetCoordinate(int))", which is worse
                // than "GetCoordinateCopy(int)" for two reasons: 1) doesn't copy M (or, in NTS, Z),
                // and 2) might allocate two Coordinate instances instead of one.
                var pt = _pts.GetCoordinateCopy(_start);
                var seqPt = facetSeq._pts.GetCoordinateCopy(facetSeq._start);
                locs[0] = new GeometryLocation(_geom, _start, pt);
                locs[1] = new GeometryLocation(facetSeq._geom, facetSeq._start, seqPt);
            }
            else if (isPoint)
            {
                var pt = _pts.GetCoordinateCopy(_start);
                ComputeDistancePointLine(pt, facetSeq, locs);
            }
            else if (isPointOther)
            {
                var seqPt = facetSeq._pts.GetCoordinateCopy(facetSeq._start);
                ComputeDistancePointLine(seqPt, this, locs);

                // unflip the locations
                (locs[0], locs[1]) = (locs[1], locs[0]);
            }
            else
            {
                ComputeDistanceLineLine(facetSeq, locs);
            }

            return locs;
        }

        private double ComputeDistanceLineLine(FacetSequence facetSeq, GeometryLocation[] locs)
        {
            // both linear - compute minimum segment-segment distance
            double minDistance = double.MaxValue;

            for (int i = _start; i < _end - 1; i++)
            {
                var p0 = _pts.GetCoordinate(i);
                var p1 = _pts.GetCoordinate(i + 1);
                for (int j = facetSeq._start; j < facetSeq._end - 1; j++)
                {
                    var q0 = facetSeq._pts.GetCoordinate(j);
                    var q1 = facetSeq._pts.GetCoordinate(j + 1);

                    double dist = DistanceComputer.SegmentToSegment(p0, p1, q0, q1);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        if (locs != null)
                        {
                            UpdateNearestLocationsLineLine(i, p0, p1, facetSeq, j, q0, q1, locs);
                        }

                        if (minDistance <= 0)
                        {
                            return minDistance;
                        }
                    }
                }
            }
            return minDistance;
        }

        private void UpdateNearestLocationsLineLine(int i, Coordinate p0, Coordinate p1, FacetSequence facetSeq, int j,
            Coordinate q0, Coordinate q1, GeometryLocation[] locs)
        {
            var seg0 = new LineSegment(p0, p1);
            var seg1 = new LineSegment(q0, q1);
            var closestPt = seg0.ClosestPoints(seg1);
            locs[0] = new GeometryLocation(_geom, i, closestPt[0].Copy());
            locs[1] = new GeometryLocation(facetSeq._geom, j, closestPt[1].Copy());
        }

        private double ComputeDistancePointLine(Coordinate pt, FacetSequence facetSeq, GeometryLocation[] locs)
        {
            double minDistance = double.MaxValue;

            for (int i = facetSeq._start; i < facetSeq._end - 1; i++)
            {
                var q0 = facetSeq._pts.GetCoordinate(i);
                var q1 = facetSeq._pts.GetCoordinate(i + 1);
                double dist = DistanceComputer.PointToSegment(pt, q0, q1);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    if (locs != null)
                    {
                        UpdateNearestLocationsPointLine(pt, facetSeq, i, q0, q1, locs);
                    }

                    if (minDistance <= 0)
                    {
                        return minDistance;
                    }
                }
            }
            return minDistance;
        }

        private void UpdateNearestLocationsPointLine(Coordinate pt,
            FacetSequence facetSeq, int i, Coordinate q0, Coordinate q1,
            GeometryLocation[] locs)
        {
            locs[0] = new GeometryLocation(_geom, _start, pt.Copy());
            var seg = new LineSegment(q0, q1);
            var segClosestPoint = seg.ClosestPoint(pt);
            locs[1] = new GeometryLocation(facetSeq._geom, i, segClosestPoint.Copy());
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
