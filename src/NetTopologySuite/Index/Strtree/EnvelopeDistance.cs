using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Index.Strtree
{
    ///<summary>
    /// Utility functions for working with <see cref="Envelope"/>s.
    /// </summary>
    /// <author>mdavis</author>
    public class EnvelopeDistance
    {
        ///<summary>
        /// Computes the maximum distance between the points defining two envelopes.
        /// This is the distance between the two corners which are farthest apart.
        /// <para/>
        /// Note that this is NOT the MinMax distance, which is a tighter bound on
        /// the distance between the points in the envelopes.
        /// </summary>
        /// <param name="env1">An envelope</param>
        /// <param name="env2">An envelope</param>
        /// <returns>The maximum distance between the points defining the envelopes</returns>
        public static double MaximumDistance(Envelope env1, Envelope env2)
        {
            double minx = Math.Min(env1.MinX, env2.MinX);
            double miny = Math.Min(env1.MinY, env2.MinY);
            double maxx = Math.Max(env1.MaxX, env2.MaxX);
            double maxy = Math.Max(env1.MaxY, env2.MaxY);
            return Distance(minx, miny, maxx, maxy);
        }

        private static double Distance(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Computes the Min-Max Distance between two <see cref="Envelope"/>s.
        /// It is equal to the minimum of the maximum distances between all pairs of
        /// edge segments from the two envelopes.
        /// This is the tight upper bound on the distance between
        /// geometric items bounded by the envelopes.
        /// <para/>
        /// Theoretically this bound can be used in the R-tree nearest-neighbour branch-and-bound search
        /// instead of <see cref="MaximumDistance"/>.
        /// However, little performance improvement is observed in practice.
        /// </summary>
        /// <param name="a">An envelope</param>
        /// <param name="b">An envelope</param>
        /// <returns>The min-max-distance between the envelopes</returns>
        public static double MinMaxDistance(Envelope a, Envelope b)
        {
            double aminx = a.MinX;
            double aminy = a.MinY;
            double amaxx = a.MaxX;
            double amaxy = a.MaxY;
            double bminx = b.MinX;
            double bminy = b.MinY;
            double bmaxx = b.MaxX;
            double bmaxy = b.MaxY;

            double dist = MaxDistance(aminx, aminy, aminx, amaxy, bminx, bminy, bminx, bmaxy);
            dist = Math.Min(dist, MaxDistance(aminx, aminy, aminx, amaxy, bminx, bminy, bmaxx, bminy));
            dist = Math.Min(dist, MaxDistance(aminx, aminy, aminx, amaxy, bmaxx, bmaxy, bminx, bmaxy));
            dist = Math.Min(dist, MaxDistance(aminx, aminy, aminx, amaxy, bmaxx, bmaxy, bmaxx, bminy));

            dist = Math.Min(dist, MaxDistance(aminx, aminy, amaxx, aminy, bminx, bminy, bminx, bmaxy));
            dist = Math.Min(dist, MaxDistance(aminx, aminy, amaxx, aminy, bminx, bminy, bmaxx, bminy));
            dist = Math.Min(dist, MaxDistance(aminx, aminy, amaxx, aminy, bmaxx, bmaxy, bminx, bmaxy));
            dist = Math.Min(dist, MaxDistance(aminx, aminy, amaxx, aminy, bmaxx, bmaxy, bmaxx, bminy));

            dist = Math.Min(dist, MaxDistance(amaxx, amaxy, aminx, amaxy, bminx, bminy, bminx, bmaxy));
            dist = Math.Min(dist, MaxDistance(amaxx, amaxy, aminx, amaxy, bminx, bminy, bmaxx, bminy));
            dist = Math.Min(dist, MaxDistance(amaxx, amaxy, aminx, amaxy, bmaxx, bmaxy, bminx, bmaxy));
            dist = Math.Min(dist, MaxDistance(amaxx, amaxy, aminx, amaxy, bmaxx, bmaxy, bmaxx, bminy));

            dist = Math.Min(dist, MaxDistance(amaxx, amaxy, amaxx, aminy, bminx, bminy, bminx, bmaxy));
            dist = Math.Min(dist, MaxDistance(amaxx, amaxy, amaxx, aminy, bminx, bminy, bmaxx, bminy));
            dist = Math.Min(dist, MaxDistance(amaxx, amaxy, amaxx, aminy, bmaxx, bmaxy, bminx, bmaxy));
            dist = Math.Min(dist, MaxDistance(amaxx, amaxy, amaxx, aminy, bmaxx, bmaxy, bmaxx, bminy));

            return dist;
        }

        /// <summary>
        /// Computes the maximum distance between two line segments.
        /// </summary>
        /// <param name="ax1">x-ordinate of first endpoint of segment 1</param>
        /// <param name="ay1">y-ordinate of first endpoint of segment 1</param>
        /// <param name="ax2">x-ordinate of second endpoint of segment 1</param>
        /// <param name="ay2">y-ordinate of second endpoint of segment 1</param>
        /// <param name="bx1">x-ordinate of first endpoint of segment 2</param>
        /// <param name="by1">y-ordinate of first endpoint of segment 2</param>
        /// <param name="bx2">x-ordinate of second endpoint of segment 2</param>
        /// <param name="by2">y-ordinate of second endpoint of segment 2</param>
        ///<returns>Maximum distance between the segments</returns>
        private static double MaxDistance(double ax1, double ay1, double ax2, double ay2,
            double bx1, double by1, double bx2, double by2)
        {
            double dist = Distance(ax1, ay1, bx1, by1);
            dist = Math.Max(dist, Distance(ax1, ay1, bx2, by2));
            dist = Math.Max(dist, Distance(ax2, ay2, bx1, by1));
            dist = Math.Max(dist, Distance(ax2, ay2, bx2, by2));
            return dist;
        }
    }
}
