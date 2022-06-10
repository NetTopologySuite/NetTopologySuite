using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;

namespace NetTopologySuite.Operation.OverlayNG
{
    /// <summary>
    /// Unions a valid coverage of polygons or lines
    /// in an efficient way.
    /// <para/>
    /// A <b>polygonal coverage</b> is a collection of <see cref="Polygon"/>s
    /// which satisfy the following conditions:
    /// <list type="number">
    /// <item><term>Vector-clean</term><description>Line segments within the collection
    /// must either be identical or intersect only at endpoints.</description></item>
    /// <item><term>Non-overlapping</term><description>No two polygons
    /// may overlap. Equivalently, polygons must be interior-disjoint.</description></item>
    /// </list>
    /// <para/>
    /// A <b>linear coverage</b> is a collection of <see cref="LineString"/>s
    /// which satisfies the <b>Vector-clean</b> condition.
    /// Note that this does not require the LineStrings to be fully noded
    /// - i.e. they may contain coincident linework.
    /// Coincident line segments are dissolved by the union.
    /// Currently linear output is not merged (this may be added in a future release.)
    /// <para/>
    /// No checking is done to determine whether the input is a valid coverage.
    /// This is because coverage validation involves segment intersection detection,
    /// which is much more expensive than the union phase.
    /// If the input is not a valid coverage
    /// then in some cases this will be detected during processing 
    /// and a <see cref="TopologyException"/> is thrown.
    /// Otherwise, the computation will produce output, but it will be invalid.
    /// <para/>
    /// Unioning a valid coverage implies that no new vertices are created.
    /// This means that a precision model does not need to be specified.
    /// The precision of the vertices in the output geometry is not changed.
    /// </summary>
    /// <author>Martin Davis</author>
    /// <seealso cref="BoundaryChainNoder"/>
    /// <seealso cref="SegmentExtractingNoder"/>
    public static class CoverageUnion
    {
        /// <summary>
        /// Unions a valid polygonal coverage or linear network.
        /// </summary>
        /// <param name="coverage">A coverage of polygons or lines</param>
        /// <returns>The union of the coverage</returns>
        /// <exception cref="TopologyException">Thrown in some cases if the coverage is invalid</exception>
        public static Geometry Union(Geometry coverage)
        {
            INoder noder = new BoundaryChainNoder();
            //-- these are less performant
            //INoder noder = new SegmentExtractingNoder();
            //INoder noder = new BoundarySegmentNoder();

            //-- linear networks require a segment-extracting noder
            if (coverage.Dimension < Dimension.Surface)
            {
                noder = new SegmentExtractingNoder();
            }

            // a precision model is not needed since no noding is done
            return OverlayNG.Union(coverage, null, noder);
        }
    }
}
