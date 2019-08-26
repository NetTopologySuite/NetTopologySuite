namespace NetTopologySuite.Geometries.Prepared
{
    /// <summary>
    /// Computes the <c>Covers</c> spatial relationship predicate for a <see cref="PreparedPolygon"/> relative to all other <see cref="Geometry"/> classes.
    /// </summary>
    /// <remarks>
    /// Uses short-circuit tests and indexing to improve performance.
    /// <para>
    /// It is not possible to short-circuit in all cases, in particular in the case where the test geometry touches the polygon linework.
    /// In this case full topology must be computed.
    /// </para></remarks>
    /// <author>Martin Davis</author>
    internal class PreparedPolygonCovers : AbstractPreparedPolygonContains
    {
        /// <summary>
        /// Computes the <c>Covers</c> spatial relationship predicate for a <see cref="PreparedPolygon"/> relative to all other <see cref="Geometry"/> classes.
        /// </summary>
        /// <param name="prep">The prepared polygon</param>
        /// <param name="geom">A test geometry</param>
        /// <returns>true if the polygon covers the geometry</returns>
        public static bool Covers(PreparedPolygon prep, Geometry geom)
        {
            var polyInt = new PreparedPolygonCovers(prep);
            return polyInt.Covers(geom);
        }

        /// <summary>
        /// Creates an instance of this operation.
        /// </summary>
        /// <param name="prepPoly">The PreparedPolygon to evaluate</param>
        public PreparedPolygonCovers(PreparedPolygon prepPoly)
            : base(prepPoly)
        {
            RequireSomePointInInterior = false;
        }

        /// <summary>
        /// Tests whether this PreparedPolygon <c>covers</c> a given geometry.
        /// </summary>
        /// <param name="geom">The test geometry</param>
        /// <returns>true if the test geometry is covered</returns>
        public bool Covers(Geometry geom)
        {
            return Eval(geom);
        }

        /// <summary>
        /// Computes the full topological <c>covers</c> predicate. Used when short-circuit tests are not conclusive.
        /// </summary>
        /// <param name="geom">The test geometry</param>
        /// <returns>true if this prepared polygon covers the test geometry</returns>
        protected override bool FullTopologicalPredicate(Geometry geom)
        {
            bool result = prepPoly.Geometry.Covers(geom);
            return result;
        }
    }
}