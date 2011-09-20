using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

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
        public static Coordinate GetIntersection(Coordinate p00, Coordinate p01, Coordinate p10, Coordinate p11)
        {
            CentralEndpointIntersector intor = new CentralEndpointIntersector(p00, p01, p10, p11);
            return intor.Intersection;
        }

        private readonly Coordinate[] _pts;
        private Coordinate _intPt;

        public CentralEndpointIntersector(Coordinate p00, Coordinate p01, Coordinate p10, Coordinate p11)
        {
            _pts = new[] { p00, p01, p10, p11 };
            Compute();
        }

        private void Compute()
        {
            Coordinate centroid = Average(_pts);
            _intPt = FindNearestPoint(centroid, _pts);
        }

        public Coordinate Intersection
        {
            get { return _intPt; }
        }

        private static Coordinate Average(Coordinate[] pts)
        {
            Coordinate avg = new Coordinate();
            int n = pts.Length;
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
