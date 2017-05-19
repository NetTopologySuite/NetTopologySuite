using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Triangulate.QuadEdge
{
    public class LocateFailureException : Exception
    {
        public LocateFailureException(string msg)
            : base(msg)
        {
        }

        public LocateFailureException(string msg, LineSegment seg)
            : base(MsgWithSpatial(msg, seg))
        {
            Segment = new LineSegment(seg);
        }

        public LocateFailureException(LineSegment seg)
            : base("Locate failed to converge (at edge: "
                   + seg
                   + ").  Possible causes include invalid Subdivision topology or very close sites")
        {
            Segment = new LineSegment(seg);
        }

        public LineSegment Segment { get; private set; }

        private static string MsgWithSpatial(string msg, LineSegment seg)
        {
            if (seg != null)
                return msg + " [ " + seg + " ]";
            return msg;
        }
    }
}