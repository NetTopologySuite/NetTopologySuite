using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Predicate
{
    /// <summary>
    /// Tests if any line segments in two sets of <see cref="CoordinateSequences"/> intersect.
    /// Optimized for use when at least one input is of small size.
    /// Short-circuited to return as soon an intersection is found.
    /// </summary>
    public class SegmentIntersectionTester
    {
        // for purposes of intersection testing, don't need to set precision model
        private readonly LineIntersector li = new RobustLineIntersector();

        private bool _hasIntersection;
        private readonly Coordinate pt00 = new Coordinate();
        private readonly Coordinate pt01 = new Coordinate();
        private readonly Coordinate pt10 = new Coordinate();
        private readonly Coordinate pt11 = new Coordinate();

        /// <summary>
        ///
        /// </summary>
        /// <param name="seq"></param>
        /// <param name="lines"></param>
        /// <returns></returns>
        public bool HasIntersectionWithLineStrings(CoordinateSequence seq, ICollection<Geometry> lines)
        {
            foreach (LineString line in lines)
            {
                HasIntersection(seq, line.CoordinateSequence);
                if (_hasIntersection)
                    break;
            }
            return _hasIntersection;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="seq0"></param>
        /// <param name="seq1"></param>
        /// <returns></returns>
        public bool HasIntersection(CoordinateSequence seq0, CoordinateSequence seq1)
        {
            for (int i = 1; i < seq0.Count && ! _hasIntersection; i++)
            {
                seq0.GetCoordinate(i - 1, pt00);
                seq0.GetCoordinate(i, pt01);
                for (int j = 1; j < seq1.Count && ! _hasIntersection; j++)
                {
                    seq1.GetCoordinate(j - 1, pt10);
                    seq1.GetCoordinate(j, pt11);
                    li.ComputeIntersection(pt00, pt01, pt10, pt11);
                    if (li.HasIntersection)
                        _hasIntersection = true;
                }
            }
            return _hasIntersection;
        }
    }
}
