using System;

namespace NetTopologySuite.Algorithm.Match
{
    /// <summary>
    /// Provides methods to mathematically combine <see cref="ISimilarityMeasure"/> values.
    /// </summary>
    /// <author>Martin Davis</author>
    public static class SimilarityMeasureCombiner
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="measure1"></param>
        /// <param name="measure2"></param>
        /// <returns></returns>
        public static double Combine(double measure1, double measure2)
        {
            return Math.Min(measure1, measure2);
        }

    }
}