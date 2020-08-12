using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;

namespace NetTopologySuite.Operation.OverlayNg
{
    /// <summary>
    /// Unions a valid coverage of polygons or lines
    /// in a robust, efficient way.
    /// <para/>
    /// A valid coverage is determined by the following conditions:
    /// <list type="bullet">
    /// <item><term>Homogeneous</term><description>all elements of the collection must have the same dimension.</description>
    /// <item><term>Fully noded</term</term><description>Line segments within the collection
    /// must either be identical or intersect only at endpoints.</description></item>
    /// <item><term>Non-overlapping</term></term><description>(Polygonal coverage only) No two polygons
    /// may overlap.Equivalently, polygons must be interior-disjoint.</description></item>
    /// </list>
    /// <para/>
    /// Currently no checking is done to determine whether the input is a valid coverage.
    /// This is because coverage validation involves segment intersection detection,
    /// which is much more expensive than the union phase.
    /// If the input is not a valid coverage
    /// then in some cases this will detected during processing 
    /// and a error will be thrown.
    /// Otherwise, the computation will produce output, but it will be invalid.
    /// <para/>
    /// Unioning a valid coverage implies that no new vertices are created.
    /// This means that a precision model does not need to be specified.
    /// The precision of the vertices in the output geometry is not changed.
    /// Because of this no precision reduction is performed.
    /// <para/>
    /// Unioning a linear network is a way of performing 
    /// line merging and line dissolving.
    /// </summary>
    /// <author>Martin Davis</author>
    /// <seealso cref="SegmentExtractingNoder"/>
    public class CoverageUnion
    {
        /// <summary>
        /// Unions a valid polygonal coverage or linear network.
        /// </summary>
        /// <param name="coverage">A coverage of polygons or lines</param>
        /// <returns>The union of the coverage</returns>
        public static Geometry Union(Geometry coverage)
        {
            var noder = new SegmentExtractingNoder();
            // a precision model is not needed since no noding is done
            return OverlayNG.Union(coverage, null, noder);
        }

        private CoverageUnion()
        {
            // No instantiation for now
        }
    }
}
