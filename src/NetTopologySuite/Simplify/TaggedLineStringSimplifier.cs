using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using System;
using static System.Collections.Specialized.BitVector32;
using System.Net.NetworkInformation;

namespace NetTopologySuite.Simplify
{
    /// <summary>
    /// Simplifies a TaggedLineString, preserving topology
    /// (in the sense that no new intersections are introduced).
    /// Uses the recursive Douglas-Peucker  algorithm.
    /// </summary>
    public class TaggedLineStringSimplifier
    {
        private readonly LineIntersector _li = new RobustLineIntersector();
        private readonly LineSegmentIndex _inputIndex = new LineSegmentIndex();
        private readonly LineSegmentIndex _outputIndex = new LineSegmentIndex();
        private TaggedLineString _line;
        private Coordinate[] _linePts;
        private double _distanceTolerance;

        public TaggedLineStringSimplifier(LineSegmentIndex inputIndex, LineSegmentIndex outputIndex)
        {
            _inputIndex = inputIndex;
            _outputIndex = outputIndex;
        }

        /// <summary>
        /// Sets the distance tolerance for the simplification.
        /// All vertices in the simplified geometry will be within this
        /// distance of the original geometry.
        /// </summary>
        public double DistanceTolerance
        {
            get => _distanceTolerance;
            set => _distanceTolerance = value;
        }

        /// <summary>
        /// Simplifies the given <see cref="TaggedLineString"/>
        /// using the distance tolerance specified.
        /// </summary>
        /// <param name="line">The linestring to simplify.</param>
        public void Simplify(TaggedLineString line)
        {
            _line = line;
            _linePts = line.ParentCoordinates;
            SimplifySection(0, _linePts.Length - 1, 0);

            if (!line.PreserveEndpoint && CoordinateArrays.IsRing(_linePts))
            {
                SimplifyRingEndpoint();
            }
        }

        private void SimplifySection(int i, int j, int depth)
        {
            depth += 1;
            if ((i + 1) == j)
            {
                var newSeg = _line.GetSegment(i);
                _line.AddToResult(newSeg);
                // leave this segment in the input index, for efficiency
                return;
            }

            bool isValidToSimplify = true;

            /*
             * Following logic ensures that there is enough points in the output line.
             * If there is already more points than the minimum, there's nothing to check.
             * Otherwise, if in the worst case there wouldn't be enough points,
             * don't flatten this segment (which avoids the worst case scenario)
             */
            if (_line.ResultSize < _line.MinimumSize)
            {
                int worstCaseSize = depth + 1;
                if (worstCaseSize < _line.MinimumSize)
                    isValidToSimplify = false;
            }

            double[] distance = new double[1];
            int furthestPtIndex = FindFurthestPoint(_linePts, i, j, distance);
            // flattening must be less than distanceTolerance
            if (distance[0] > _distanceTolerance)
                isValidToSimplify = false;
            // test if flattened section would cause intersection
            var candidateSeg = new LineSegment();
            candidateSeg.P0 = _linePts[i];
            candidateSeg.P1 = _linePts[j];
            if (HasBadIntersection(_line, i, j, candidateSeg))
                isValidToSimplify = false;

            if (isValidToSimplify)
            {
                var newSeg = Flatten(i, j);
                _line.AddToResult(newSeg);
                return;
            }
            SimplifySection(i, furthestPtIndex, depth);
            SimplifySection(furthestPtIndex, j, depth);
        }

        private void SimplifyRingEndpoint()
        {
            if (_line.ResultSize > _line.MinimumSize)
            {
                var firstSeg = _line.GetResultSegment(0);
                var lastSeg = _line.GetResultSegment(-1);

                var simpSeg = new LineSegment(lastSeg.P0, firstSeg.P1);
                //-- the excluded segments are the ones containing the endpoint
                if (simpSeg.Distance(firstSeg.P0) <= _distanceTolerance
                    && !HasBadIntersection(_line, _line.Segments.Length - 2, 0, simpSeg))
                {
                    _line.RemoveRingEndpoint();
                }
            }
        }

        private int FindFurthestPoint(Coordinate[] pts, int i, int j, double[] maxDistance)
        {
            var seg = new LineSegment();
            seg.P0 = pts[i];
            seg.P1 = pts[j];
            double maxDist = -1.0;
            int maxIndex = i;
            for (int k = i + 1; k < j; k++)
            {
                var midPt = pts[k];
                double distance = seg.Distance(midPt);
                if (distance > maxDist)
                {
                    maxDist = distance;
                    maxIndex = k;
                }
            }
            maxDistance[0] = maxDist;
            return maxIndex;
        }

        /// <summary>
        /// Flattens a section of the line between
        /// indexes <paramref name="start"/> and <paramref name="end"/>,
        /// replacing them with a line between the endpoints.
        /// The input and output indexes are updated
        /// to reflect this.
        /// </summary>
        /// <param name="start">The start index of the flattened section.</param>
        /// <param name="end">The end index of the flattened section.</param>
        /// <returns>The new segment created.</returns>
        private LineSegment Flatten(int start, int end)
        {
            // make a new segment for the simplified geometry
            var p0 = _linePts[start];
            var p1 = _linePts[end];
            var newSeg = new LineSegment(p0, p1);
            // update the indexes
            Remove(_line, start, end);
            _outputIndex.Add(newSeg);
            return newSeg;
        }

        /// <summary>
        /// Tests if a flattening segment intersects a line
        /// (excluding a given section of segments).
        /// The excluded section is being replaced by the flattening segment,
        /// so there is no need to test it
        /// (and it may well intersect the segment).
        /// </summary>
        private bool HasBadIntersection(TaggedLineString line,
                             int excludeStart, int excludeEnd,
                             LineSegment candidateSeg)
        {
            if (HasBadOutputIntersection(candidateSeg)) return true;
            if (HasBadInputIntersection(line, excludeStart, excludeEnd, candidateSeg)) return true;
            return false;
        }

        private bool HasBadOutputIntersection(LineSegment candidateSeg)
        {
            var querySegs = _outputIndex.Query(candidateSeg);
            foreach (var querySeg in querySegs)
            {
                bool interior = HasInvalidIntersection(querySeg, candidateSeg);
                if (interior)
                    return true;
            }
            return false;
        }

        private bool HasBadInputIntersection(TaggedLineString parentLine,
                        int excludeStart, int excludeEnd,
                        LineSegment candidateSeg)
        {
            var querySegs = _inputIndex.Query(candidateSeg);
            foreach (TaggedLineSegment querySeg in querySegs)
            {
                bool interior = HasInvalidIntersection(querySeg, candidateSeg);
                if (interior)
                {
                    /*
                     * Ignore the intersection if the intersecting segment is part of the section being collapsed
                     * to the candidate segment
                     */
                    if (IsInLineSection(_line, excludeStart, excludeEnd, querySeg))
                        continue;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Tests whether a segment is in a section of a TaggedLineString.
        /// Sections may wrap around the endpoint of the line,
        /// to support ring endpoint simplification.
        /// This is indicated by excludedStart > excludedEnd
        /// </summary>
        /// <param name="line">The TaggedLineString containing the section segments</param>
        /// <param name="excludeStart">The index of the first segment in the excluded section  </param>
        /// <param name="excludeEnd">The index of the last segment in the excluded section</param>
        /// <param name="seg">The segment to test</param>
        /// <returns><c>true</c> if the test segment intersects some segment in the line not in the excluded section</returns>
        private static bool IsInLineSection(TaggedLineString line,
            int excludeStart, int excludeEnd,
            TaggedLineSegment seg)
        {
            //-- test segment is not in this line
            if (seg.Parent != line.Parent)
                return false;
            int segIndex = seg.Index;
            if (excludeStart <= excludeEnd)
            {
                //-- section is contiguous
                if (segIndex >= excludeStart && segIndex < excludeEnd)
                    return true;
            }
            else
            {
                //-- section wraps around the end of a ring
                if (segIndex >= excludeStart || segIndex <= excludeEnd)
                    return true;
            }
            return false;
        }

        private bool HasInvalidIntersection(LineSegment seg0, LineSegment seg1)
        {
            //-- segments must not be equal
            if (seg0.EqualsTopologically(seg1))
                return true;
            _li.ComputeIntersection(seg0.P0, seg0.P1, seg1.P0, seg1.P1);
            return _li.IsInteriorIntersection();
        }

        /// <summary>
        /// Remove the segs in the section of the line.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void Remove(TaggedLineString line, int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                var seg = line.GetSegment(i);
                _inputIndex.Remove(seg);
            }
        }
    }
}
