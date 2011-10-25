using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes an approximate intersection of two line segments
    /// by taking the most central of the endpoints of the segments.
    /// This is effective in cases where the segments are nearly parallel
    /// and should intersect at an endpoint.
    /// It is also a reasonable strategy for cases where the 
    /// endpoint of one segment lies on or almost on the interior of another one.
    /// Taking the most central endpoint ensures that the computed intersection
    /// point lies in the envelope of the segments.
    /// Also, by always returning one of the input points, this should result 
    /// in reducing segment fragmentation.
    /// Intended to be used as a last resort for 
    /// computing ill-conditioned intersection situations which 
    /// cause other methods to fail.
    /// </summary>
    public class CentralEndpointIntersector
    {
        /// <summary>
        /// Computes an approximate intersection of two line segments
        /// by taking the most central of the endpoints of the segments.
        /// This is effective in cases where the segments are nearly parallel
        /// and should intersect at an endpoint.
        /// </summary>
        /// <param name="p00">The 1st coordinate of the 1st line segement.</param>
        /// <param name="p01">The 2nd coordinate of the 1st line segemen.</param>
        /// <param name="p10">The 1st coordinate of the 2nd line segement.</param>
        /// <param name="p11">The 2nd coordinate of the 2nd line segement.</param>
        /// <returns></returns>
        public static Coordinate GetIntersection(Coordinate p00, Coordinate p01, Coordinate p10, Coordinate p11)
        {
            var intor = new CentralEndpointIntersector(p00, p01, p10, p11);
            return intor.Intersection;
        }

        private readonly Coordinate[] _pts;

        /// <summary>
        /// Creates an instance of this class using the provided input coordinates
        /// </summary>
        /// <param name="p00">The 1st coordinate of the 1st line segement.</param>
        /// <param name="p01">The 2nd coordinate of the 1st line segemen.</param>
        /// <param name="p10">The 1st coordinate of the 2nd line segement.</param>
        /// <param name="p11">The 2nd coordinate of the 2nd line segement.</param>
        public CentralEndpointIntersector(Coordinate p00, Coordinate p01, Coordinate p10, Coordinate p11)
        {
            _pts = new[] { p00, p01, p10, p11 };
            Compute();
        }

        private void Compute()
        {
            var centroid = Average(_pts);
            Intersection = FindNearestPoint(centroid, _pts);
        }

        /// <summary>
        /// Gets the intersection point
        /// </summary>
        public Coordinate Intersection { get; private set; }

        private static Coordinate Average(Coordinate[] pts)
        {
            var avg = new Coordinate();
            var n = pts.Length;
            for (int i = 0; i < pts.Length; i++)
            {
                avg.X += pts[i].X;
                avg.Y += pts[i].Y;
            }
            if (n > 0)
            {
                avg.X /= n;
                avg.Y /= n;
            }
            return avg;
        }

        /// <summary>
        /// Determines a point closest to the given point.
        /// </summary>        
        private static Coordinate FindNearestPoint(Coordinate p, Coordinate[] pts)
        {
            double minDist = Double.MaxValue;
            Coordinate result = null;
            for (int i = 0; i < pts.Length; i++)
            {
                double dist = p.Distance(pts[i]);
                if (dist < minDist)
                {
                    minDist = dist;
                    result = pts[i];
                }
            }
            return result;
        }
    }
}
