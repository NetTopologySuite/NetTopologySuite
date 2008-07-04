using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Index.Strtree;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Implements <c>PointInRing</c> using a <c>SIRtree</c> index to increase performance.
    /// </summary>
    public class SIRtreePointInRing : IPointInRing 
    {
        private ILinearRing ring;
        private SIRtree sirTree;
        private int crossings = 0;  // number of segment/ray crossings

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ring"></param>
        public SIRtreePointInRing(ILinearRing ring)
        {
            this.ring = ring;
            BuildIndex();
        }

        /// <summary>
        /// 
        /// </summary>
        private void BuildIndex()
        {
            sirTree = new SIRtree();
            ICoordinate[] pts = ring.Coordinates;
            for (int i = 1; i < pts.Length; i++) 
            {
                if (pts[i - 1].Equals(pts[i])) 
                    continue;

                LineSegment seg = new LineSegment(pts[i - 1], pts[i]);
                sirTree.Insert(seg.P0.Y, seg.P1.Y, seg);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public bool IsInside(ICoordinate pt)
        {
            crossings = 0;

            // test all segments intersected by vertical ray at pt
            IList segs = sirTree.Query(pt.Y);        
            for(IEnumerator i = segs.GetEnumerator(); i.MoveNext(); ) 
            {
                LineSegment seg = (LineSegment) i.Current;
                TestLineSegment(pt, seg);
            }

            /*
            *  p is inside if number of crossings is odd.
            */
            if ((crossings % 2) == 1) 
                return true;            
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="seg"></param>
        private void TestLineSegment(ICoordinate p, LineSegment seg) 
        {
            double xInt;  // x intersection of segment with ray
            double x1;    // translated coordinates
            double y1;
            double x2;
            double y2;

            /*
            *  Test if segment crosses ray from test point in positive x direction.
            */
            ICoordinate p1 = seg.P0;
            ICoordinate p2 = seg.P1;
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
                    crossings++;            
            }
        }
    }
}
