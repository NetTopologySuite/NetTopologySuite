using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;

namespace NetTopologySuite.Coverage
{
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
