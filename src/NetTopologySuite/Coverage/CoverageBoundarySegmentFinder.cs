using NetTopologySuite.Geometries;
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

        public static LineSegment CreateSegment(CoordinateSequence seq, int i)
        {
            var seg = new LineSegment(seq.GetCoordinate(i), seq.GetCoordinate(i + 1));
            seg.Normalize();
            return seg;
        }

        public bool Done => false;

        public bool GeometryChanged => false;

    }

}
