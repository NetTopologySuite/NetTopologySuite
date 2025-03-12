using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;

namespace NetTopologySuite.Coverage
{
    /// <summary>
    /// Finds coverage segments which occur in only a single coverage element.
    /// In a valid coverage, these are exactly the line segments which lie
    /// on the boundary of the coverage.
    /// <para/>
    /// In an invalid coverage, segments might occur in 3 or more elements.
    /// This situation is not detected.
    /// </summary>
    /// <author>Martin Davis</author>
    internal sealed class CoverageBoundarySegmentFinder : IEntireCoordinateSequenceFilter
    {
        public static ISet<LineSegment> FindBoundarySegments(Geometry[] geoms)
        {
            var segs = new HashSet<LineSegment>();
            var finder = new CoverageBoundarySegmentFinder(segs);
            foreach (var geom in geoms)
            {
                geom.Apply(finder);
            }
            return segs;
        }

        public static bool IsBoundarySegment(ISet<LineSegment> boundarySegs, CoordinateSequence seq, int i)
        {
            var seg = CreateSegment(seq, i);
            return boundarySegs.Contains(seg);
        }

        private readonly ISet<LineSegment> _boundarySegs;

        public CoverageBoundarySegmentFinder(ISet<LineSegment> segs)
        {
            _boundarySegs = segs;
        }

        public void Filter(CoordinateSequence seq)
        {
            //-- final point does not start a segment
            for (int i = 0; i < seq.Count - 1; i++)
            {
                var seg = CreateSegment(seq, i);
                /*
                 * Records segments with an odd number of occurrences.
                 * In a valid coverage each segment can occur only 1 or 2 times.
                 * This does not detect invalid situations, where a segment might occur 3 or more times.
                 */
                if (_boundarySegs.Contains(seg))
                {
                    _boundarySegs.Remove(seg);
                }
                else
                {
                    _boundarySegs.Add(seg);
                }
            }
        }

        private static LineSegment CreateSegment(CoordinateSequence seq, int i)
        {
            var seg = new LineSegment(seq.GetCoordinate(i), seq.GetCoordinate(i + 1));
            seg.Normalize();
            return seg;
        }

        public bool Done => false;

        public bool GeometryChanged => false;

    }

}
