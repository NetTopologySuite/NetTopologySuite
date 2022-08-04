using NetTopologySuite.Algorithm.Construct;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using System.Collections.Generic;

namespace NetTopologySuite.Coverage
{
    /// <summary>
    /// Finds gaps in a polygonal coverage.
    /// Gaps are holes in the coverage which are narrower than a given width.
    /// <para/>
    /// The coverage should be valid according to {@link CoverageValidator}.
    /// If this is not the case, some gaps may not be reported, or the invocation may fail.
    /// <para/>
    /// This is a more accurate way of identifying gaps
    /// than using {@link CoverageValidator#setGapWidth(double)}.
    /// Gaps which separate the coverage into two disjoint regions are not detected.
    /// Gores are not identified as gaps.>
    /// </summary>
    /// <author>Martin Davis</author>
    public class CoverageGapFinder
    {
        /// <summary>
        /// Finds gaps in a polygonal coverage.
        /// Returns lines indicating the locations of the gaps.
        /// </summary>
        /// <param name="coverage">A set of polygons forming a polygonal coverage</param>
        /// <param name="gapWidth">The maximum width of gap to detect</param>
        /// <returns>A geometry indicating the locations of gaps (which is empty if no gaps were found), or null if the coverage was empty</returns>
        public static Geometry FindGaps(Geometry[] coverage, double gapWidth)
        {
            var finder = new CoverageGapFinder(coverage);
            return finder.FindGaps(gapWidth);
        }

        private readonly Geometry[] _coverage;

        /// <summary>
        /// Creates a new polygonal coverage gap finder
        /// </summary>
        /// <param name="coverage">A set of polygons forming a polygonal coverage</param>
        public CoverageGapFinder(Geometry[] coverage)
        {
            _coverage = coverage;
        }

        /// <summary>
        /// Finds gaps in the coverage.
        /// Returns lines indicating the locations of the gaps.
        /// </summary>
        /// <param name="gapWidth">The maximum width of gap to detect</param>
        /// <returns>A geometry indicating the locations of gaps (which is empty if no gaps were found), or <c>null</c> if the coverage was empty</returns>
        public Geometry FindGaps(double gapWidth)
        {
            var union = CoverageUnion.Union(_coverage);
            var polygons = PolygonExtracter.GetPolygons(union);

            var gapLines = new List<LineString>();
            foreach (Polygon poly in polygons)
            {
                for (int i = 0; i < poly.NumInteriorRings; i++)
                {
                    var hole = (LinearRing)poly.GetInteriorRingN(i);
                    if (IsGap(hole, gapWidth))
                    {
                        gapLines.Add(CopyLine(hole));
                    }
                }
            }
            return union.Factory.BuildGeometry(gapLines);
        }

        private static LineString CopyLine(LinearRing hole)
        {
            var pts = hole.Coordinates;
            return hole.Factory.CreateLineString(pts);
        }

        private bool IsGap(LinearRing hole, double gapWidth)
        {
            var holePoly = hole.Factory.CreatePolygon(hole);
            //-- guard against bad input
            if (gapWidth <= 0.0)
                return false;

            double tolerance = gapWidth / 100;
            //TODO: improve MIC class to allow short-circuiting when radius is larger than a value
            var line = MaximumInscribedCircle.GetRadiusLine(holePoly, tolerance);
            double width = line.Length * 2;
            return width <= gapWidth;
        }
    }
}
