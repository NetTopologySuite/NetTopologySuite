using System;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph.Index
{
    public class SweepLineSegment
    {
        private Edge edge;
        private ICoordinate[] pts;
        private Int32 ptIndex;

        public SweepLineSegment(Edge edge, Int32 ptIndex)
        {
            this.edge = edge;
            this.ptIndex = ptIndex;
            pts = edge.Coordinates;
        }

        public Double MinX
        {
            get
            {
                Double x1 = pts[ptIndex].X;
                Double x2 = pts[ptIndex + 1].X;
                return x1 < x2 ? x1 : x2;
            }
        }

        public Double MaxX
        {
            get
            {
                Double x1 = pts[ptIndex].X;
                Double x2 = pts[ptIndex + 1].X;
                return x1 > x2 ? x1 : x2;
            }
        }

        public void ComputeIntersections(SweepLineSegment ss, SegmentIntersector si)
        {
            si.AddIntersections(edge, ptIndex, ss.edge, ss.ptIndex);
        }
    }
}