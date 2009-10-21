using System;

namespace GisSharpBlog.NetTopologySuite.Algorithm.Match
{

    /// <summary>
    ///  Provides methods to mathematically combine <see cref="ISimilarityMeasure{TCoordinate}"/> values.
    /// </summary>
    public class SimilarityMeasureCombiner
    {
        public static Double Combine(Double measure1, Double measure2)
        {
            return Math.Min(measure1, measure2);
        }

    }
}
