using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

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
        }

        private void SimplifySection(int i, int j, int depth)
        {
            depth += 1;
            int[] sectionIndex = new int[2];
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
            sectionIndex[0] = i;
            sectionIndex[1] = j;
            if (HasBadIntersection(_line, sectionIndex, candidateSeg))
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

        private bool HasBadIntersection(TaggedLineString parentLine,
            int[] sectionIndex, LineSegment candidateSeg)
        {
            bool badOutput = HasBadOutputIntersection(candidateSeg);
            if (badOutput)
                return true;
            bool badInput = HasBadInputIntersection(parentLine, sectionIndex, candidateSeg);
            if (badInput)
                return true;
            return false;
        }

        private bool HasBadOutputIntersection(LineSegment candidateSeg)
        {
            var querySegs = _outputIndex.Query(candidateSeg);
            foreach (var querySeg in querySegs)
            {
                bool interior = HasInteriorIntersection(querySeg, candidateSeg);
                if (interior)
                    return true;
            }
            return false;
        }

        private bool HasBadInputIntersection(TaggedLineString parentLine,
            int[] sectionIndex, LineSegment candidateSeg)
        {
            var querySegs = _inputIndex.Query(candidateSeg);
            foreach (TaggedLineSegment querySeg in querySegs)
            {
                bool interior = HasInteriorIntersection(querySeg, candidateSeg);
                if (interior)
                {
                    bool inline = IsInLineSection(parentLine, sectionIndex, querySeg);
                    if (inline)
                        continue;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Tests whether a segment is in a section of a <see cref="TaggedLineString"/>.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="sectionIndex"></param>
        /// <param name="seg"></param>
        /// <returns></returns>
        private static bool IsInLineSection(TaggedLineString line,
            int[] sectionIndex, TaggedLineSegment seg)
        {
            // not in this line
            if (seg.Parent != line.Parent)
                return false;
            int segIndex = seg.Index;
            if (segIndex >= sectionIndex[0] &&
                segIndex < sectionIndex[1])
                return true;
            return false;
        }

        private bool HasInteriorIntersection(LineSegment seg0, LineSegment seg1)
        {
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
