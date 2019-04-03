using System;

using GeoAPI.Geometries;

namespace NetTopologySuite.EdgeRay
{
    internal static class EdgeRay
    {
        /// <summary>
        /// Computes the area term for the edge rays in both directions along an edge.
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <returns></returns>
        public static double AreaTermBoth(double x0, double y0, double x1, double y1)
        {
            double dx = x1 - x0;
            double dy = y1 - y0;
            double len = Math.Sqrt(dx * dx + dy * dy);

            double u0x = dx / len;
            double u0y = dy / len;

            // normal vector pointing to R of unit
            double n0x = u0y;
            double n0y = -u0x;

            double u1x = -u0x;
            double u1y = -u0y;

            // normal vector pointing to L of back unit vector
            double n1x = -u1y;
            double n1y = u1x;

            double areaTerm0 = 0.5 * (x0 * u0x + y0 * u0y) * (x0 * n0x + y0 * n0y);
            double areaTerm1 = 0.5 * (x1 * u1x + y1 * u1y) * (x1 * n1x + y1 * n1y);

            return areaTerm0 + areaTerm1;
        }

        public static double AreaTerm(
            double x0, double y0, double x1, double y1, bool isNormalToRight)
        {
            return AreaTerm(x0, y0, x0, y0, x1, y1, isNormalToRight);
        }

        public static double AreaTerm(
            double vx, double vy, double x0, double y0, double x1, double y1, bool isNormalToRight)
        {
            double dx = x1 - x0;
            double dy = y1 - y0;
            double len = Math.Sqrt(dx * dx + dy * dy);

            if (len <= 0)
            {
                return 0;
            }

            double ux = dx / len;
            double uy = dy / len;

            // normal vector pointing to R of unit
            // (assumes CW ring)
            double nx, ny;
            if (isNormalToRight)
            {
                nx = uy;
                ny = -ux;
            }
            else
            {
                nx = -uy;
                ny = ux;
            }

            double areaTerm = 0.5 * (vx * ux + vy * uy) * (vx * nx + vy * ny);

            // Console.WriteLine(areaTerm);
            return areaTerm;
        }

        public static double AreaTerm(Coordinate p0, Coordinate p1, bool isNormalToRight)
        {
            return AreaTerm(p0.X, p0.Y, p1.X, p1.Y, isNormalToRight);
        }

        public static double AreaTerm(Coordinate v, Coordinate p0, Coordinate p1, bool isNormalToRight)
        {
            return AreaTerm(v.X, v.Y, p0.X, p0.Y, p1.X, p1.Y, isNormalToRight);
        }
    }
}
