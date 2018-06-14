using GeoAPI.Geometries;

namespace NetTopologySuite.Algorithm.Match
{
    /// <summary>
    /// Measures the degree of similarity between two <see cref="IGeometry"/>s
    /// using the area of intersection between the geometries.
    /// The measure is normalized to lie in the range [0, 1].
    /// Higher measures indicate a great degree of similarity.
    /// </summary>
    /// <remarks>
    /// NOTE: Currently experimental and incomplete.
    /// </remarks>
    /// <author>mbdavis</author>
public class AreaSimilarityMeasure : ISimilarityMeasure
{
    /*
    public static double measure(Geometry a, Geometry b)
    {
        AreaSimilarityMeasure gv = new AreaSimilarityMeasure(a, b);
        return gv.measure();
    }
    */

        public double Measure(IGeometry g1, IGeometry g2)
    {
        double areaInt = g1.Intersection(g2).Area;
        double areaUnion = g1.Union(g2).Area;
        return areaInt / areaUnion;
    }

}
}
