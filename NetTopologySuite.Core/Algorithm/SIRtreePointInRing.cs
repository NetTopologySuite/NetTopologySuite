using IList = System.Collections.Generic.IList<object>;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;

namespace NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Implements <c>PointInRing</c> using a <c>SIRtree</c> index to increase performance.
    /// </summary>
    public class SIRtreePointInRing : IPointInRing 
    {
        private readonly ILinearRing _ring;
        private SIRtree<LineSegment> _sirTree;
        private int _crossings;  // number of segment/ray crossings

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ring"></param>
        public SIRtreePointInRing(ILinearRing ring)
        {
            _ring = ring;
            BuildIndex();
        }

        /// <summary>
        /// 
        /// </summary>
        private void BuildIndex()
        {
            _sirTree = new SIRtree<LineSegment>();
            Coordinate[] pts = _ring.Coordinates;
            for (int i = 1; i < pts.Length; i++) 
            {
                if (pts[i - 1].Equals(pts[i])) 
                    continue;

                LineSegment seg = new LineSegment(pts[i - 1], pts[i]);
                _sirTree.Insert(seg.P0.Y, seg.P1.Y, seg);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public bool IsInside(Coordinate pt)
        {
            _crossings = 0;

            // test all segments intersected by vertical ray at pt
            var segs = _sirTree.Query(pt.Y);
            foreach (var seg in segs)
            {
                TestLineSegment(pt, seg);
            }

            /*
            *  p is inside if number of crossings is odd.
            */
            if ((_crossings % 2) == 1) 
                return true;            
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="seg"></param>
        private void TestLineSegment(Coordinate p, LineSegment seg) 
        {
            double xInt;  // x intersection of segment with ray
            double x1;    // translated coordinates
            double y1;
            double x2;
            double y2;

            /*
            *  Test if segment crosses ray from test point in positive x direction.
            */
            Coordinate p1 = seg.P0;
            Coordinate p2 = seg.P1;
            x1 = p1.X - p.X;
            y1 = p1.Y - p.Y;
            x2 = p2.X - p.X;
            y2 = p2.Y - p.Y;

            if (((y1 > 0) && (y2 <= 0)) || ((y2 > 0) && (y1 <= 0))) 
            {
                /*
                *  segment straddles x axis, so compute intersection.
                */
                xInt = RobustDeterminant.SignOfDet2x2(x1, y1, x2, y2) / (y2 - y1);
                
                /*
                *  crosses ray if strictly positive intersection.
                */
                if (0.0 < xInt) 
                    _crossings++;            
            }
        }
    }
}
