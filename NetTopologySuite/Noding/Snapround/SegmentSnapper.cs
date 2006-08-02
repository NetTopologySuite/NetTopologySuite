using System;
using System.Collections;
using System.Text;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Noding;

namespace GisSharpBlog.NetTopologySuite.Noding.Snapround
{
    /// <summary>
    /// 
    /// </summary>
    public class SegmentSnapper
    {
        // NOTE: modified for "safe" assembly in Sql 2005
        // Const added!
        private const double Tolerance = 0.5;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <returns><c>true</c> if the point p is within the snap tolerance of the line p0-p1.</returns>
        public static bool IsWithinTolerance(Coordinate p, Coordinate p0, Coordinate p1)
        {
            double minx = p.X - Tolerance;
            double maxx = p.X + Tolerance;
            double miny = p.Y - Tolerance;
            double maxy = p.Y + Tolerance;
            double segMinx = Math.Min(p0.X, p1.X);
            double segMaxx = Math.Max(p0.X, p1.X);
            double segMiny = Math.Min(p0.Y, p1.Y);
            double segMaxy = Math.Max(p0.Y, p1.Y);
            if (maxx < segMinx || minx > segMaxx || maxy < segMiny || miny > segMaxy) 
                return false;

            double dx = p1.X - p0.X;
            double dy = p1.Y - p0.Y;

            double px = p.X - p0.X;
            double py = p.Y - p0.Y;

            double discy = px * dy - py * dx;
            if (Math.Abs(discy) < Math.Abs(0.5 * dx)) 
                return true;

            double discx = py * dx - px * dy;
            if (Math.Abs(discx) < Math.Abs(0.5 * dy))
                return true;            
            
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public SegmentSnapper() { }

        /// <summary> 
        /// Adds a new node (equal to the snap pt) to the segment
        /// if the snapPt is within tolerance of the segment.
        /// </summary>
        /// <param name="snapPt"></param>
        /// <param name="segStr"></param>
        /// <param name="segIndex"></param>
        /// <returns><c>true</c> if a node was added.</returns>
        public virtual bool AddSnappedNode(Coordinate snapPt, SegmentString segStr, int segIndex)
        {
            Coordinate p0 = segStr.GetCoordinate(segIndex);
            Coordinate p1 = segStr.GetCoordinate(segIndex + 1);

            // no need to snap if the snapPt equals an endpoint of the segment
            if (snapPt.Equals(p0)) return false;
            if (snapPt.Equals(p1)) return false;

            if (IsWithinTolerance(snapPt, p0, p1))
            {
                segStr.AddIntersection(snapPt, segIndex);
                return true;
            }
            return false;
        }
    }
}
