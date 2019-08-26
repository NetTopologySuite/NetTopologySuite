using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm
{

    /// <summary>
    /// Functions for computing length.
    /// </summary>
    /// <author>
    /// Martin Davis
    /// </author>
    public class Length
    {
        /// <summary>
        /// Computes the length of a <c>LineString</c> specified by a sequence of points.
        /// </summary>
        /// <param name="pts">The points specifying the <c>LineString</c></param>
        /// <returns>The length of the <c>LineString</c></returns>
        public static double OfLine(CoordinateSequence pts)
        {
            // optimized for processing CoordinateSequences
            int n = pts.Count;
            if (n <= 1)
                return 0.0;

            double len = 0.0;

            var p = pts.GetCoordinateCopy(0);
            double x0 = p.X;
            double y0 = p.Y;

            for (int i = 1; i < n; i++)
            {
                pts.GetCoordinate(i, p);
                double x1 = p.X;
                double y1 = p.Y;
                double dx = x1 - x0;
                double dy = y1 - y0;

                len += Math.Sqrt(dx * dx + dy * dy);

                x0 = x1;
                y0 = y1;
            }
            return len;
        }

    }
}
