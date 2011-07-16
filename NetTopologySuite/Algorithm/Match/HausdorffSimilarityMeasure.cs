using System;
using System.Collections.Generic;
using System.Text;
using NPack.Interfaces;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm.Distance;

namespace NetTopologySuite.Algorithm.Match
{
    /// <summary>
    /// Measures the degree of similarity between two <see cref="IGeometry{TCoordinate}"/>s
    /// using the Hausdorff distance metric.
    /// The measure is normalized to lie in the range [0, 1].
    /// Higher measures indicate a great degree of similarity.
    ///
    /// The measure is computed by computing the Hausdorff distance
    /// between the input geometries, and then normalizing
    /// this by dividing it by the diagonal distance across 
    /// the envelope of the combined geometries.
    /// </summary>
    /// <typeparam name="TCoordinate"></typeparam>
    public class HausdorffSimilarityMeasure<TCoordinate> : ISimilarityMeasure<TCoordinate>
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
    {
        /*
        public static double measure(Geometry a, Geometry b)
        {
            HausdorffSimilarityMeasure gv = new HausdorffSimilarityMeasure(a, b);
            return gv.measure();
        }
	
        public HausdorffSimilarityMeasure()
        {
        }
        */

        /// <summary>
        ///  Densify a small amount to increase accuracy of Hausdorff distance
        /// </summary>
        private const Double DensifyFraction = 0.25;

        public Double Measure(IGeometry<TCoordinate> g1, IGeometry<TCoordinate> g2)
        {
            Double distance = DiscreteHausdorffDistance<TCoordinate>.Distance(g1, g2, DensifyFraction);

            IExtents<TCoordinate> extents = (IExtents<TCoordinate>)g1.Extents.Clone();
            extents.ExpandToInclude(g2.Extents);
            Double extentsSize = DiagonalSize(extents);
            // normalize so that more similarity produces a measure closer to 1
            Double measure = 1 - distance / extentsSize;

            return measure;
        }

        public static Double DiagonalSize(IExtents<TCoordinate> extents)
        {
            if (extents.IsEmpty) return 0.0;

            TCoordinate min, max;
            min = extents.Min;
            max = extents.Max;
            double width = max[Ordinates.X] - min[Ordinates.X];
            double height = max[Ordinates.Y] - min[Ordinates.Y];
            return Math.Sqrt(width * width + height * height);
        }

    }
}
