namespace NetTopologySuite.Geometries.Prepared
{
    /// <summary>
    /// Computes the <c>contains</c> spatial relationship predicate for a <see cref="PreparedPolygon"/> relative to all other <see cref="Geometry"/> classes.
    /// Uses short-circuit tests and indexing to improve performance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// It is not possible to short-circuit in all cases, in particular
    /// in the case where the test geometry touches the polygon linework.
    /// In this case full topology must be computed.
    /// </para>
    /// </remarks>
    /// <author>Martin Davis</author>
    internal class PreparedPolygonContains : AbstractPreparedPolygonContains
    {
        /// <summary>
        /// Computes the <c>contains</c> spatial relationship predicate between a <see cref="PreparedPolygon"/> and a <see cref="Geometry"/>.
        /// </summary>
        /// <param name="prep">The prepared polygon</param>
        /// <param name="geom">A test geometry</param>
        /// <returns>true if the polygon contains the geometry</returns>
        public static bool Contains(PreparedPolygon prep, Geometry geom)
        {
            var polyInt = new PreparedPolygonContains(prep);
            return polyInt.Contains(geom);
        }

        /// <summary>
        /// Creates an instance of this operation.
        /// </summary>
        /// <param name="prepPoly">the PreparedPolygon to evaluate</param>
        public PreparedPolygonContains(PreparedPolygon prepPoly)
            : base(prepPoly)
        {
        }

        /// <summary>
        /// Tests whether this PreparedPolygon <c>contains</c> a given geometry.
        /// </summary>
        /// <param name="geom">The test geometry</param>
        /// <returns>true if the test geometry is contained</returns>
        public bool Contains(Geometry geom)
        {
            return Eval(geom);
        }

        /// <summary>
        /// Computes the full topological <c>contains</c> predicate.<br/>
        /// Used when short-circuit tests are not conclusive.
        /// </summary>
        /// <param name="geom">The test geometry </param>
        /// <returns>true if this prepared polygon contains the test geometry</returns>
        protected override bool FullTopologicalPredicate(Geometry geom)
        {
            bool isContained = prepPoly.Geometry.Contains(geom);
            return isContained;
        }
    }
}