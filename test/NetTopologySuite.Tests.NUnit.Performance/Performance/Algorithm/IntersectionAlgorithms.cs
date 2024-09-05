using System;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Precision;

namespace NetTopologySuite.Tests.NUnit.Performance.Algorithm
{
    /**
     * Alternate implementations of line intersection algorithms.
     * Used for test purposes only.
     * 
     * @author Martin Davis
     *
     */
    public static class IntersectionAlgorithms
    {

        public static Coordinate IntersectionBasic(Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2)
        {
            double px = p1.Y - p2.Y;
            double py = p2.X - p1.X;
            double pw = p1.X * p2.Y - p2.X * p1.Y;

            double qx = q1.Y - q2.Y;
            double qy = q2.X - q1.X;
            double qw = q1.X * q2.Y - q2.X * q1.Y;

            double x = py * qw - qy * pw;
            double y = qx * pw - px * qw;
            double w = px * qy - qx * py;

            double xInt = x / w;
            double yInt = y / w;

            if (!Coordinate.IsValidOrdinateValue(xInt) || !Coordinate.IsValidOrdinateValue(yInt))
            {
                return null;
            }
            return new Coordinate(xInt, yInt);
        }

        public static Coordinate IntersectionDDWithFilter(Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2)
        {
            var intPt = IntersectionDDFilter(p1, p2, q1, q2);
            if (intPt != null)
                return intPt;
            return CGAlgorithmsDD.Intersection(p1, p2, q1, q2);
        }

        private const double FILTER_TOL = 1.0E-6;

        private static Coordinate IntersectionDDFilter(Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2)
        {
            // Compute using DP math
            var intPt = IntersectionBasic(p1, p2, q1, q2);
            if (intPt == null)
                return null;
            if (DistanceComputer.PointToLinePerpendicular(intPt, p1, p2) > FILTER_TOL)
                return null;
            if (DistanceComputer.PointToLinePerpendicular(intPt, q1, q2) > FILTER_TOL)
                return null;
            return intPt;
        }

        public static Coordinate IntersectionCB(Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2)
        {
            var common = ComputeCommonCoord(p1, p2, q1, q2);
            p1 = SubtractCoord(p1, common);
            p2 = SubtractCoord(p2, common);
            q1 = SubtractCoord(q1, common);
            q2 = SubtractCoord(q2, common);

            // unrolled computation
            double px = p1.Y - p2.Y;
            double py = p2.X - p1.X;
            double pw = p1.X * p2.Y - p2.X * p1.Y;

            double qx = q1.Y - q2.Y;
            double qy = q2.X - q1.X;
            double qw = q1.X * q2.Y - q2.X * q1.Y;

            double x = py * qw - qy * pw;
            double y = qx * pw - px * qw;
            double w = px * qy - qx * py;

            double xInt = x / w;
            double yInt = y / w;

            if (!Coordinate.IsValidOrdinateValue(xInt) || !Coordinate.IsValidOrdinateValue(yInt))
            {
                return null;
            }
            return new Coordinate(xInt + common.X, yInt + common.Y);
        }

        private static Coordinate SubtractCoord(Coordinate c0, Coordinate c1)
        {
            var res = c0.Copy();
            res.X -= c1.X;
            res.Y -= c1.Y;
            return res;
        }

        private static Coordinate ComputeCommonCoord(Coordinate c0, Coordinate c1, Coordinate c2, Coordinate c3)
        {
            return new Coordinate(GetCommonBits(c0.X, c1.X, c2.X, c3.X), GetCommonBits(c0.Y, c1.Y, c2.Y, c3.Y));
        }

        private static double GetCommonBits(double v0, double v1, double v2, double v3)
        {
            var cb = new CommonBits();
            cb.Add(v0);
            cb.Add(v1);
            cb.Add(v2);
            cb.Add(v3);
            return cb.Common;
        }

    }
}
