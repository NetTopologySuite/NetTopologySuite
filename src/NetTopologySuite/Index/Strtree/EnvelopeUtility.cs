using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Index.Strtree
{
    ///<summary>
    /// Utility functions for working with <see cref="Envelope"/>s.
    /// </summary>
    /// <author>mdavis</author>
    class EnvelopeUtility
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
    }
}
