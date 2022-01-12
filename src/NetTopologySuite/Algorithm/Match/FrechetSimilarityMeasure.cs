using NetTopologySuite.Algorithm.Distance;
using NetTopologySuite.Geometries;
using System;

namespace NetTopologySuite.Algorithm.Match
{
    /// <summary>
    /// Measures the degree of similarity between two
    /// <see cref="Geometry"/>s using the Fréchet distance metric.
    /// The measure is normalized to lie in the range [0, 1].
    /// Higher measures indicate a great degree of similarity.
    /// <para/>
    /// The measure is computed by computing the Fréchet distance
    /// between the input geometries, and then normalizing
    /// this by dividing it by the diagonal distance across
    /// the envelope of the combined geometries.
    /// <para/>
    /// Note: the input should be normalized, especially when
    /// measuring <see cref="MultiPoint"/> geometries because for the
    /// Fréchet distance the order of {@link Coordinate}s is
    /// important.
    /// </summary>
    /// <author>Felix Obermaier</author>
    public class FrechetSimilarityMeasure : ISimilarityMeasure
    {
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public FrechetSimilarityMeasure()
        { }

        /// <inheritdoc/>
        public double Measure(Geometry g1, Geometry g2)
        {

            // Check if input is of same type
            if (g1.OgcGeometryType != g2.OgcGeometryType)
                throw new ArgumentException("g1 and g2 are of different type");

            // Compute the distance
            double frechetDistance = DiscreteFrechetDistance.Distance(g1, g2);
            if (frechetDistance == 0d) return 1;

            // Compute envelope diagonal size
            var env = new Envelope(g1.EnvelopeInternal);
            env.ExpandToInclude(g2.EnvelopeInternal);
            double envDiagSize = HausdorffSimilarityMeasure.DiagonalSize(env);

            // normalize so that more similarity produces a measure closer to 1
            return 1 - frechetDistance / envDiagSize;
        }
    }

}
