using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm.Match
{
    /// <summary>
    /// An interface for classes which measures the degree of similarity between two {@link Geometry}s.
    /// </summary>
    /// <remarks>
    /// The computed measure lies in the range [0, 1].
    /// Higher measures indicate a great degree of similarity.
    /// A measure of 1.0 indicates that the input geometries are identical
    /// A measure of 0.0 indicates that the geometries have essentially no similarity.
    /// The precise definition of "identical" and "no similarity" may depend on the  exact algorithm being used.
    /// </remarks>
    /// <author>mbdavis</author>
    public interface ISimilarityMeasure
    {

        /// <summary>
        /// Function to measure the similarity between two <see cref="Geometry"/>s.
        /// </summary>
        /// <param name="g1">A geometry</param>
        /// <param name="g2">A geometry</param>
        /// <returns>The similarity value between two <see cref="Geometry"/>s</returns>
        double Measure(Geometry g1, Geometry g2);
    }
}