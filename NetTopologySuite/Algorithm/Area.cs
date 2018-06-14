using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Functions for computing area.
    /// </summary>
    /// <author>Martin Davis</author>
    public static class Area
    {

        /// <summary>
        /// Computes the area for a ring.
        /// </summary>
        /// <param name="ring">The coordinates forming the ring</param>
        /// <returns>The area of the ring</returns>
        public static double OfRing(Coordinate[] ring)
        {
            return Math.Abs(OfRingSigned(ring));
        }

        /// <summary>
        /// Computes the area for a ring.
        /// </summary>
        /// <param name="ring">The coordinates forming the ring</param>
        /// <returns>The area of the ring</returns>
        public static double OfRing(ICoordinateSequence ring)
        {
            return Math.Abs(OfRingSigned(ring));
        }

        /// <summary>
        /// Computes the signed area for a ring. The signed area is positive if the
        /// ring is oriented CW, negative if the ring is oriented CCW, and zero if the
        /// ring is degenerate or flat.
        /// </summary>
        /// <param name="ring">The coordinates forming the ring</param>
        /// <returns>The signed area of the ring</returns>
        public static double OfRingSigned(Coordinate[] ring)
        {
            if (ring.Length < 3)
                return 0.0;
            double sum = 0.0;
            /**
             * Based on the Shoelace formula.
             * http://en.wikipedia.org/wiki/Shoelace_formula
             */
            double x0 = ring[0].X;
            for (int i = 1; i < ring.Length - 1; i++)
            {
                double x = ring[i].X - x0;
                double y1 = ring[i + 1].Y;
                double y2 = ring[i - 1].Y;
                sum += x * (y2 - y1);
            }
            return sum / 2.0;
        }

        /// <summary>
        /// Computes the signed area for a ring. The signed area is positive if the
        /// <list type="Table">
        /// <listheader>
        /// <term>value</term>
        /// <description>meaning</description>
        /// </listheader>
        /// <item><term>&gt; 0</term>
        /// <description>The ring is oriented clockwise (CW)</description></item>
        /// <item><term>&lt; 0</term>
        /// <description>The ring is oriented counter clockwise (CCW)</description></item>
        /// <item><term>== 0</term>
        /// <description>The ring is degenerate or flat</description></item>
        /// </list>
        /// ring is oriented CW, negative if the ring is oriented CCW, and zero if the
        /// ring is degenerate or flat.
        /// </summary>
        /// <param name="ring">The coordinates forming the ring</param>
        /// <returns>The signed area of the ring</returns>
        public static double OfRingSigned(ICoordinateSequence ring)
        {
            int n = ring.Count;
            if (n < 3)
                return 0.0;
            /**
             * Based on the Shoelace formula.
             * http://en.wikipedia.org/wiki/Shoelace_formula
             */
            var p0 = new Coordinate();
            var p1 = new Coordinate();
            var p2 = new Coordinate();
            ring.GetCoordinate(0, p1);
            ring.GetCoordinate(1, p2);
            double x0 = p1.X;
            p2.X -= x0;
            double sum = 0.0;
            for (int i = 1; i < n - 1; i++)
            {
                p0.Y = p1.Y;
                p1.X = p2.X;
                p1.Y = p2.Y;
                ring.GetCoordinate(i + 1, p2);
                p2.X -= x0;
                sum += p1.X * (p0.Y - p2.Y);
            }
            return sum / 2.0;
        }
    }
}