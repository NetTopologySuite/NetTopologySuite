﻿using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm.Distance;

namespace NetTopologySuite.Algorithm.Match
{
    /// <summary>
    /// Measures the degree of similarity between two <see cref="IGeometry"/>s using the Hausdorff distance metric.
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
        private static readonly double DensifyFraction = 0.25;

        public double Measure(IGeometry g1, IGeometry g2)
        {
            double distance = DiscreteHausdorffDistance.Distance(g1, g2, DensifyFraction);

            Envelope env = new Envelope(g1.EnvelopeInternal);
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