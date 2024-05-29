namespace NetTopologySuite.Operation.RelateNG
{
    /// <summary>
    /// String constants for DE-9IM matrix patterns for topological relationships.
    /// These can be used with <see cref="RelateNG.Evaluate(Geometries.Geometry, string)"/>
    /// and <see cref="RelateNG.Relate(Geometries.Geometry, Geometries.Geometry, string)"/>.
    /// <para/>
    /// <h3> DE - 9IM Pattern Matching </h3>
    /// Matrix patterns are specified as a 9 - character string
    /// containing the pattern symbols for the DE-9IM 3x3 matrix entries,
    /// listed row - wise.
    /// The pattern symbols are:
    /// <list type="table">
    /// <listheader><term>Code</term><description>Description</description></listheader>
    /// <item><term>0</term><description>topological interaction has dimension 0</description></item>
    /// <item><term>1</term><description>topological interaction has dimension 1</description></item>
    /// <item><term>2</term><description>topological interaction has dimension 2</description></item>
    /// <item><term>F</term><description>no topological interaction</description></item>
    /// <item><term>T</term><description>topological interaction of any dimension</description></item>
    /// <item><term>*</term><description>any topological interaction is allowed, including none</description></item>
    /// </list>
    /// </summary>
    /// <author>Martin Davis</author>
    public static class IntersectionMatrixPattern
    {
        /// <summary>
        /// A DE-9IM pattern to detect whether two polygonal geometries are adjacent along
        /// an edge, but do not overlap.
        /// </summary>
        public const string Adjacent = "F***1****";

        /// <summary>
        /// A DE-9IM pattern to detect a geometry which properly contains another
        /// geometry (i.e. which lies entirely in the interior of the first geometry).
        /// </summary>
        public const string ContainsProperly = "T**FF*FF*";

        /// <summary>
        /// A DE-9IM pattern to detect if two geometries intersect in their interiors.
        /// This can be used to determine if a polygonal coverage contains any overlaps
        /// (although not whether they are correctly noded).
        /// </summary>
        public const string InteriorIntersects = "T********";

    }
}

