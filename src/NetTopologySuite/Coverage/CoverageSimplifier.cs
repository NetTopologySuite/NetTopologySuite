using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace NetTopologySuite.Coverage
{
    /// <summary>
    /// Simplifies the boundaries of the polygons in a polygonal coverage
    /// while preserving the original coverage topology.
    /// An area-based simplification algorithm
    /// (similar to Visvalingam-Whyatt simplification)
    /// is used to provide high-quality results.
    /// Also supports simplifying just the inner edges in a coverage,
    /// which allows simplifying "patches" without affecting their boundary.
    /// <para/>
    /// The amount of simplification is determined by a tolerance value,
    /// which is a non-negative quantity. It equates roughly to the maximum
    /// distance by which a simplified line can change from the original.
    /// (In fact, it is the square root of the area tolerance used
    /// in the Visvalingam-Whyatt algorithm.)
    /// <para/>
    /// The simplified result coverage has the following characteristics:
    /// <list type="bullet">
    /// <item><description>It has the same number and types of polygonal geometries as the input</description></item>
    /// <item><description>Coverage node points (inner vertices shared by three or more polygons,
    /// or boundary vertices shared by two or more) are not changed</description></item>
    /// <item><description>if the input is a valid coverage, then so is the result</description></item>
    /// </list>
    /// </summary>
    /// <author>Martin Davis</author>
    public sealed class CoverageSimplifier
    {
        /// <summary>
        /// Simplify the boundaries of a set of polygonal geometries forming a coverage,
        /// preserving the coverage topology.
        /// </summary>
        /// <param name="coverage">A set of polygonal geometries forming a coverage</param>
        /// <param name="tolerance">The simplification tolerance</param>
        /// <returns>The simplified polygons</returns>
        public static Geometry[] Simplify(Geometry[] coverage, double tolerance)
        {
            var simplifier = new CoverageSimplifier(coverage);
            return simplifier.Simplify(tolerance);
        }

        /// <summary>
        /// Simplify the inner boundaries of a set of polygonal geometries forming a coverage,
        /// preserving the coverage topology.
        /// </summary>
        /// <param name="coverage">A set of polygonal geometries forming a coverage</param>
        /// <param name="tolerance">The simplification tolerance</param>
        /// <returns>The simplified polygons</returns>
        public static Geometry[] SimplifyInner(Geometry[] coverage, double tolerance)
        {
            var simplifier = new CoverageSimplifier(coverage);
            return simplifier.SimplifyInner(tolerance);
        }

        private readonly Geometry[] _input;
        private readonly GeometryFactory _geomFactory;

        /// <summary>
        /// Create a new simplifier instance.
        /// </summary>
        /// <param name="coverage">A set of polygonal geometries forming a coverage</param>
        public CoverageSimplifier(Geometry[] coverage)
        {
            _input = coverage;
            _geomFactory = coverage[0].Factory;
        }

        /// <summary>
        /// Computes the simplified coverage, preserving the coverage topology.
        /// </summary>
        /// <param name="tolerance">The simplification tolerance</param>
        /// <returns>The simplified polygons</returns>
        public Geometry[] Simplify(double tolerance)
        {
            var cov = CoverageRingEdges.Create(_input);
            SimplifyEdges(cov.Edges, null, tolerance);
            var result = cov.BuildCoverage();
            return result;
        }

        /// <summary>
        /// Computes the inner-boundary simplified coverage,
        /// preserving the coverage topology.
        /// </summary>
        /// <param name="tolerance">The simplification tolerance</param>
        /// <returns>The simplified polygons</returns>
        public Geometry[] SimplifyInner(double tolerance)
        {
            var cov = CoverageRingEdges.Create(_input);
            var innerEdges = cov.SelectEdges(2);
            var outerEdges = cov.SelectEdges(1);
            var constraint = CreateLines(outerEdges);

            SimplifyEdges(innerEdges, constraint, tolerance);
            var result = cov.BuildCoverage();
            return result;
        }

        private void SimplifyEdges(IList<CoverageEdge> edges, MultiLineString constraints, double tolerance)
        {
            var lines = CreateLines(edges);
            var linesSimp = TPVWSimplifier.Simplify(lines, constraints, tolerance);
            //Assert: mlsSimp.getNumGeometries = edges.length

            SetCoordinates(edges, linesSimp);
        }

        private void SetCoordinates(IList<CoverageEdge> edges, MultiLineString lines)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                edges[i].Coordinates = lines.GetGeometryN(i).Coordinates;
            }
        }

        private MultiLineString CreateLines(IList<CoverageEdge> edges)
        {
            var lines = new LineString[edges.Count];
            for (int i = 0; i < edges.Count; i++)
            {
                lines[i] = _geomFactory.CreateLineString(edges[i].Coordinates);
            }
            var mls = _geomFactory.CreateMultiLineString(lines);
            return mls;
        }

    }

}
