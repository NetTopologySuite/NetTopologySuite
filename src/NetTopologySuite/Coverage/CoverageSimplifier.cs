using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Threading;

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
    /// <item><description>It has the same number of polygonal geometries as the input</description></item>
    /// <item><description>If the input is a valid coverage, then so is the result</description></item>
    /// <item><description>Node points (inner vertices shared by three or more polygons,
    /// or boundary vertices shared by two or more) are not changed</description></item>
    /// <item><description>Polygons maintain their line-adjacency (edges are never removed)</description></item>
    /// <item><description>Rings are simplified to a minimum of 4 vertices, to better preserve their shape</description></item>
    /// <item><description>Rings smaller than the area tolerance are removed where possible.
    /// This applies to both holes and "islands" (multipolygon elements
    /// which are disjoint or touch another polygon at a single vertex).
    /// At least one polygon is retained for each input geometry
    /// (the one with largest area).</description></item>
    /// </list>
    /// <para/>
    /// This class supports simplification using different distance tolerances
    /// for inner and outer edges of the coverage(including no simplfication
    /// using a tolerance of 0.0).
    /// This allows, for example, inner simplification, which simplifies
    /// only edges of the coverage which are adjacent to two polygons.
    /// This allows partial simplification of a coverage, since a simplified
    /// subset of a coverage still matches the remainder of the coverage.
    /// <para/>
    /// The class allows specifying a separate tolerance for each element of the input coverage.
    /// <para/>
    /// The input coverage should be valid according to <see cref="CoverageValidator"/>.
    /// Invalid coverages may still be simplified, but the result will likely still be invalid.
    /// <para/>
    /// <b>NOTE:</b><br/>Due to different implementations of the <c>PriorityQueue</c> classes used in JTS and NTS
    /// the results of the <c>CoverageSimplifier</c>'s simplification methods are not guaranteed
    /// to be the same. Nonetheless both results are valid.
    /// </summary>
    /// <remarks>
    /// <h3>FUTURE WORK</h3>
    /// <list type="bullet">
    /// <description>Support geodetic data by computing true geodetic area, and accepting tolerances in metres</description>
    /// </list>
    /// </remarks>
    /// <author>Martin Davis</author>
    public sealed class CoverageSimplifier
    {
        /// <summary>
        /// Simplifies the boundaries of a set of polygonal geometries forming a coverage,
        /// preserving the coverage topology.
        /// </summary>
        /// <param name="coverage">A set of polygonal geometries forming a coverage</param>
        /// <param name="tolerance">The simplification tolerance</param>
        /// <returns>The simplified coverage polygons</returns>
        public static Geometry[] Simplify(Geometry[] coverage, double tolerance)
        {
            var simplifier = new CoverageSimplifier(coverage);
            return simplifier.Simplify(tolerance);
        }

        /// <summary>
        /// Simplifies the boundaries of a set of polygonal geometries forming a coverage,
        /// preserving the coverage topology, using a separate tolerance
        /// for each element of the coverage.
        /// Coverage edges are simplified using the lowest tolerance of each adjacent
        /// element.
        /// </summary>
        /// <param name="coverage">A set of polygonal geometries forming a coverage</param>
        /// <param name="tolerances">The simplification tolerances (one per input element)</param>
        /// <returns>The simplified coverage polygons</returns>
        public static Geometry[] Simplify(Geometry[] coverage, double[] tolerances)
        {
            var simplifier = new CoverageSimplifier(coverage);
            return simplifier.Simplify(tolerances);
        }
        /// <summary>
        /// Simplifies the inner boundaries of a set of polygonal geometries forming a coverage,
        /// preserving the coverage topology.
        /// Edges which form the exterior boundary of the coverage are left unchanged.
        /// </summary>
        /// <param name="coverage">A set of polygonal geometries forming a coverage</param>
        /// <param name="tolerance">The simplification tolerance</param>
        /// <returns>The simplified coverage polygons</returns>
        public static Geometry[] SimplifyInner(Geometry[] coverage, double tolerance)
        {
            var simplifier = new CoverageSimplifier(coverage);
            return simplifier.Simplify(tolerance, 0);
        }

        /// <summary>
        /// Simplifies the outer boundaries of a set of polygonal geometries forming a coverage,
        /// preserving the coverage topology.
        /// Edges in the interior of the coverage are left unchanged.
        /// </summary>
        /// <param name="coverage">A set of polygonal geometries forming a coverage</param>
        /// <param name="tolerance">The simplification tolerance</param>
        /// <returns>The simplified coverage polygons</returns>
        public static Geometry[] SimplifyOuter(Geometry[] coverage, double tolerance)
        {
            var simplifier = new CoverageSimplifier(coverage);
            return simplifier.Simplify(0, tolerance);
        }

        private readonly Geometry[] _coverage;
        private double _smoothWeight = CornerArea.DEFAULT_SMOOTH_WEIGHT;
        private double _removableSizeFactor = 1.0;

        /// <summary>
        /// Create a new coverage simplifier instance.
        /// </summary>
        /// <param name="coverage">A set of polygonal geometries forming a coverage</param>
        public CoverageSimplifier(Geometry[] coverage)
        {
            _coverage = coverage;
        }

        /// <summary>
        /// Gets or sets a value indicating the factor
        /// applied to the area tolerance to determine
        /// if small rings should be removed.
        /// Larger values cause more rings to be removed.
        /// A value of <c>0</c> prevents rings from being removed.
        /// </summary>
        public double RemovableRingSizeFactor
        {
            get => _removableSizeFactor; set
            {
                if (value < 0.0)
                    value = 0.0;
                _removableSizeFactor = value;
            }
        }

        /// <summary>
        /// Gets or sets a value inidcating the weight influencing
        /// how smooth the simplification should be.
        /// The weight must be between 0 and 1.
        /// Larger values increase the smoothness of the simplified edges.
        /// </summary>
        public double SmoothWeight
        {
            get => _smoothWeight;
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentOutOfRangeException(nameof(value), $"The valid range for SmoothWeight is [0, 1]");
            }
        }

        /// <summary>
        /// Computes the simplified coverage using a single distance tolerance,
        /// preserving the coverage topology.
        /// </summary>
        /// <param name="tolerance">The simplification distance tolerance</param>
        /// <returns>The simplified coverage polygons</returns>
        public Geometry[] Simplify(double tolerance)
        {
            return SimplifyEdges(tolerance, tolerance);
        }

        /// <summary>
        /// Computes the simplified coverage using separate distance tolerances
        /// for inner and outer edges, preserving the coverage topology.
        /// </summary>
        /// <param name="toleranceInner">The distance tolerance for inner edges</param>
        /// <param name="toleranceOuter">The distance tolerance for outer edges</param>
        /// <returns>The simplified coverage polygons</returns>
        public Geometry[] Simplify(double toleranceInner, double toleranceOuter)
        {
            return SimplifyEdges(toleranceInner, toleranceOuter);
        }

        /// <summary>
        /// Computes the simplified coverage using separate distance tolerances
        /// for each coverage element, preserving the coverage topology.
        /// </summary>
        /// <param name="tolerances">the distance tolerances for the coverage elements</param>
        /// <returns>The simplified coverage polygons</returns>
        public Geometry[] Simplify(double[] tolerances)
        {
            if (tolerances.Length != _coverage.Length)
                throw new ArgumentException("number of tolerances does not match number of coverage elements", nameof(tolerances));
            return SimplifyEdges(tolerances);
        }

        private Geometry[] SimplifyEdges(double[] tolerances)
        {
            var covRings = CoverageRingEdges.Create(_coverage);
            var covEdges = covRings.Edges;
            var edges = CreateEdges(covEdges, tolerances);
            return Simplify(covRings, covEdges, edges);
        }

        private TPVWSimplifier.Edge[] CreateEdges(IList<CoverageEdge> covEdges, double[] tolerances)
        {
            var edges = new TPVWSimplifier.Edge[covEdges.Count];
            for (int i = 0; i < covEdges.Count; i++)
            {
                var covEdge = covEdges[i];
                double tol = ComputeTolerance(covEdge, tolerances);
                edges[i] = CreateEdge(covEdge, tol);
            }
            return edges;
        }

        private static double ComputeTolerance(CoverageEdge covEdge, double[] tolerances)
        {
            int index0 = covEdge.GetAdjacentIndex(0);
            // assert: index0 >= 0
            double tolerance = tolerances[index0];

            if (covEdge.HasAdjacentIndex(1))
            {
                int index1 = covEdge.GetAdjacentIndex(1);
                double tol1 = tolerances[index1];
                //-- use lowest tolerance for edge
                if (tol1 < tolerance)
                    tolerance = tol1;
            }
            return tolerance;
        }

        private Geometry[] SimplifyEdges(double toleranceInner, double toleranceOuter)
        {
            var covRings = CoverageRingEdges.Create(_coverage);
            var covEdges = covRings.Edges;
            var edges = CreateEdges(covEdges, toleranceInner, toleranceOuter);
            return Simplify(covRings, covEdges, edges);
        }

        private Geometry[] Simplify(CoverageRingEdges covRings, IList<CoverageEdge> covEdges, TPVWSimplifier.Edge[] edges)
        {
            var cornerArea = new CornerArea(_smoothWeight);
            TPVWSimplifier.Simplify(edges, cornerArea, _removableSizeFactor);
            SetCoordinates(covEdges, edges);
            var result = covRings.BuildCoverage();
            return result;
        }

        private static TPVWSimplifier.Edge[] CreateEdges(IList<CoverageEdge> covEdges, double toleranceInner, double toleranceOuter)
        {
            var edges = new TPVWSimplifier.Edge[covEdges.Count];
            for (int i = 0; i < covEdges.Count; i++)
            {
                var covEdge = covEdges[i];
                double tol = ComputeTolerance(covEdge, toleranceInner, toleranceOuter);
                edges[i] = CreateEdge(covEdge, tol);
            }
            return edges;
        }

        private static TPVWSimplifier.Edge CreateEdge(CoverageEdge covEdge, double tol)
        {
            return new TPVWSimplifier.Edge(covEdge.Coordinates, tol,
                covEdge.IsFreeRing, covEdge.IsRemovableRing);
        }

        private static double ComputeTolerance(CoverageEdge covEdge, double toleranceInner, double toleranceOuter)
        {
            return covEdge.IsInner ? toleranceInner : toleranceOuter;
        }

        private void SetCoordinates(IList<CoverageEdge> covEdges, TPVWSimplifier.Edge[] edges)
        {
            for (int i = 0; i < covEdges.Count; i++)
            {
                var edge = edges[i];
                if (edge.Tolerance > 0)
                {
                    covEdges[i].Coordinates = edges[i].Coordinates;
                }
            }
        }

    }
}
