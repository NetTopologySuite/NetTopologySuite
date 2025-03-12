using NetTopologySuite.Geometries;

namespace NetTopologySuite.Coverage
{
    /// <summary>
    /// Unions a polygonal coverage in an efficient way.
    /// <para/>
    /// Valid polygonal coverage topology allows merging polygons in a very efficient way.
    /// </summary>
    /// <author>Martin Davis</author>
    public static class CoverageUnion
    {
        /// <summary>
        /// Unions a polygonal coverage.
        /// </summary>
        /// <param name="coverage">The polygons in the coverage</param>
        /// <returns>The union of the coverage polygons</returns>
        public static Geometry Union(Geometry[] coverage)
        {
            // union of an empty coverage is null, since no factory is available
            if (coverage.Length == 0)
                return null;

            var geomFact = coverage[0].Factory;
            var geoms = geomFact.CreateGeometryCollection(coverage);
            return Operation.OverlayNG.CoverageUnion.Union(geoms);
        }
    }
}
