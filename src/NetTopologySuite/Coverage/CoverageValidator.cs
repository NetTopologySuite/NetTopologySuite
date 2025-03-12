using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Coverage
{

    /// <summary>
    /// Validates a polygonal coverage, and returns the locations of
    /// invalid polygon boundary segments if found.
    /// <para/>
    /// A polygonal coverage is a set of polygons which may be edge-adjacent but do
    /// not overlap.
    /// Coverage algorithms(such as { @link CoverageUnion}
    /// or simplification)
    /// generally require the input coverage to be valid to produce correct results.
    /// A polygonal coverage is valid if:
    /// <list type="number">
    /// <item><description>The interiors of all polygons do not intersect(are disjoint).
    /// This is the case if no polygon has a boundary which intersects the interior of another polygon,
    /// and no two polygons are identical.</description></item>
    /// <item><description>If the boundaries of polygons intersect, the vertices
    /// and line segments of the intersection match exactly.</description></item>  
    /// </list>
    /// <para/>
    /// A valid coverage may contain holes(regions of no coverage).
    /// Sometimes it is desired to detect whether coverages contain
    /// narrow gaps between polygons
    /// (which can be a result of digitizing error or misaligned data).
    /// This class can detect narrow gaps,
    /// by specifying a maximum gap width using {@link #setGapWidth(double)}.
    /// Note that this also identifies narrow gaps separating disjoint coverage regions,
    /// and narrow gores.
    /// In some situations it may also produce false positives
    /// (linework identified as part of a gap which is actually wider).
    /// See <see cref="CoverageGapFinder"/> for an alternate way to detect gaps which may be more accurate.
    /// </summary>
    /// <author>Martin Davis</author>
    public class CoverageValidator
    {
        /// <summary>
        /// Tests whether a polygonal coverage is valid.
        /// </summary>
        /// <param name="coverage">An array of polygons forming a coverage</param>
        /// <returns><c>true</c> if the coverage is valid</returns>
        /// <remarks>Named <c>isValid</c> in JTS</remarks>
        public static bool IsValid(Geometry[] coverage)
        {
            var v = new CoverageValidator(coverage);
            return !HasInvalidResult(v.Validate());
        }

        /// <summary>
        /// Tests if some element of an array of geometries is a coverage invalidity
        /// indicator.
        /// </summary>
        /// <param name="validateResult">An array produced by a polygonal coverage validation</param>
        /// <returns><c>true</c> if the result has at least one invalid indicator</returns>
        public static bool HasInvalidResult(Geometry[] validateResult)
        {
            foreach (var geom in validateResult)
            {
                if (geom != null)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Validates that a set of polygons forms a valid polygonal coverage,
        /// and returns linear geometries indicating the locations of invalidities, if any.
        /// </summary>
        /// <param name="coverage">An array of polygons forming a coverage</param>
        /// <returns>An array of linear geometries indicating coverage errors, or nulls</returns>
        public static Geometry[] Validate(Geometry[] coverage)
        {
            var v = new CoverageValidator(coverage);
            return v.Validate();
        }

        /// <summary>
        /// Validates that a set of polygons forms a valid polygonal coverage
        /// and contains no gaps narrower than a specified width.
        /// The result is an array of linear geometries indicating the locations of invalidities,
        /// or null if the polygon is coverage-valid.
        /// </summary>
        /// <param name="coverage">An array of polygons forming a coverage</param>
        /// <param name="gapWidth">The maximum width of invalid gaps</param>
        /// <returns>An array of linear geometries indicating coverage errors, or nulls</returns>
        public static Geometry[] Validate(Geometry[] coverage, double gapWidth)
        {
            var v = new CoverageValidator(coverage);
            v.GapWidth = gapWidth;
            return v.Validate();
        }

        private readonly Geometry[] _coverage;
        private double _gapWidth;

        /// <summary>
        /// Creates a new coverage validator
        /// </summary>
        /// <param name="coverage">An array of polygons representing a polygonal coverage</param>
        public CoverageValidator(Geometry[] coverage)
        {
            _coverage = coverage;
        }

        /// <summary>
        /// Gets or sets a value indicating the maximum gap, if narrow gaps are to be detected.
        /// </summary>
        public double GapWidth
        {
            get => _gapWidth; set => _gapWidth = value;
        }

        /// <summary>
        /// Validates the polygonal coverage.
        /// The result is an array of the same size as the input coverage.
        /// Each array entry is either null, or if the polygon does not form a valid coverage,
        /// a linear geometry containing the boundary segments
        /// which intersect polygon interiors, which are mismatched,
        /// or form gaps (if checked).
        /// </summary>
        /// <returns>An array of nulls or linear geometries</returns>
        public Geometry[] Validate()
        {
            var index = new STRtree<Geometry>();
            foreach (var geom in _coverage)
            {
                index.Insert(geom.EnvelopeInternal, geom);
            }
            var invalidLines = new Geometry[_coverage.Length];
            for (int i = 0; i < _coverage.Length; i++)
            {
                var geom = _coverage[i];
                invalidLines[i] = Validate(geom, index);
            }
            return invalidLines;
        }

        private Geometry Validate(Geometry targetGeom, STRtree<Geometry> index)
        {
            var queryEnv = targetGeom.EnvelopeInternal;
            queryEnv.ExpandBy(_gapWidth);
            var nearGeomList = index.Query(queryEnv);
            //-- the target geometry is returned in the query, so must be removed from the set
            nearGeomList.Remove(targetGeom);

            var nearGeoms = GeometryFactory.ToGeometryArray(nearGeomList);
            var result = CoveragePolygonValidator.Validate(targetGeom, nearGeoms, _gapWidth);
            return result.IsEmpty ? null : result;
        }
    }

}
