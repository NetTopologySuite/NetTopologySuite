using System;
using NetTopologySuite.Algorithm.Distance;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm.Match
{
    /// <summary>
    /// Measures the degree of similarity between two <see cref="Geometry"/>s using the Hausdorff distance metric.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The measure is normalized to lie in the range [0, 1]. Higher measures indicate a great degree of similarity.
    /// </para>
    /// <para>
    /// The measure is computed by computing the Hausdorff distance between the input geometries, and then normalizing
    /// this by dividing it by the diagonal distance across the envelope of the combined geometries.
    /// </para>
    /// </remarks>
    /// <author>mbdavis</author>
    public class HausdorffSimilarityMeasure : ISimilarityMeasure
    {
        /*
        public static double measure(Geometry a, Geometry b)
        {
            HausdorffSimilarityMeasure gv = new HausdorffSimilarityMeasure(a, b);
            return gv.measure();
        }
        */

        /*
         * Densify a small amount to increase accuracy of Hausdorff distance
         */
        private const double DensifyFraction = 0.25;

        /// <inheritdoc/>
        public double Measure(Geometry g1, Geometry g2)
        {
            double distance = DiscreteHausdorffDistance.Distance(g1, g2, DensifyFraction);
            if (distance == 0d) return 1d;

            var env = new Envelope(g1.EnvelopeInternal);
            env.ExpandToInclude(g2.EnvelopeInternal);
            double envSize = DiagonalSize(env);
            // normalize so that more similarity produces a measure closer to 1
            double measure = 1 - distance/envSize;

            //System.out.println("Hausdorff distance = " + distance + ", measure = " + measure);
            return measure;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        public static double DiagonalSize(Envelope env)
        {
            if (env.IsNull) return 0.0;

            double width = env.Width;
            double hgt = env.Height;
            return Math.Sqrt(width*width + hgt*hgt);
        }
    }
}
