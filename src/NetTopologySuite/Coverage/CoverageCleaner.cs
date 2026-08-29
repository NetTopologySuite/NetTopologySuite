using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm.Construct;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Dissolve;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snap;
using NetTopologySuite.Operation.Polygonize;
using Point = NetTopologySuite.Geometries.Point;

namespace NetTopologySuite.Coverage
{
    /// <summary>
    /// Cleans the linework of a set of valid polygonal geometries to form a valid polygonal coverage.
    /// The input is an array of valid <see cref="Polygon"/> or <see cref="MultiPolygon"/> geometries
    /// which may contain topological errors such as overlaps and gaps.
    /// Empty or non-polygonal inputs are removed.
    /// Linework is snapped together to eliminate small discrepancies and ensure common edges are identically noded.
    /// Overlaps are merged with a parent polygon, according to a given merge strategy.
    /// Gaps narrower than a given width are filled and merged with an adjacent polygon.
    /// The output is an array of polygonal geometries forming a valid polygonal coverage.
    /// <h3>Snapping</h3>
    /// <para/>
    /// Snapping to nearby vertices and line segment snapping is used to improve noding robustness
    /// and eliminate small errors in an efficient way.
    /// By default this uses a small snapping distance based on the extent of the input data.
    /// The snapping distance may be specified explicitly.
    /// This can reduce the number of overlaps and gaps that need to be merged,
    /// and reduce the risk of spikes formed by merged gaps.
    /// However, a large snapping distance may introduce undesirable data alteration.
    /// Snapping is disabled if a zero snapping distance is specified.
    /// (Note that disabling snapping may prevent collinear linework from being noded correctly.)
    /// <h3>Overlap Merging</h3>
    /// <para/>
    /// Overlaps are merged into a parent polygon chosen according to a specified merge strategy.
    /// The supported strategies are:
    /// <list type="bullet">
    /// <item><description><b>Longest Border</b> (default): merge with the polygon with longest shared border
    /// (<see cref="MergeLongestBorder"/>)</description></item>
    /// <item><description><b>Maximum/Minimum Area</b>: merge with the polygon with largest or smallest area
    /// (<see cref="MergeMaxArea"/>, <see cref="MergeMinArea"/>)</description></item>
    /// <item><description><b>Minimum Index</b>: merge with the polygon with the lowest index in the input array
    /// (<see cref="MergeMinIndex"/>). This allows sorting the input according to some criteria to provide
    /// a priority for merging overlaps.</description></item>
    /// </list>
    /// <h3>Gap Merging</h3>
    /// <para/>
    /// Gaps which are wider than a given distance are merged with an adjacent polygon.
    /// Polygon width is determined as twice the radius of the <see cref="MaximumInscribedCircle"/>
    /// of the gap polygon.
    /// Gaps are merged with the adjacent polygon with longest shared border.
    /// Empty holes in input polygons are treated as gaps, and may be filled in.
    /// Gaps which are not fully enclosed ("inlets") are not removed.
    /// <para/>
    /// Cleaning can be run on a valid coverage to remove gaps.
    /// <para/>
    /// The clean result is an array of polygonal geometries which match one-to-one with the input array.
    /// A result item may be an empty polygon if:
    /// <list type="bullet">
    /// <item><description>the input item is non-polygonal or empty</description></item>
    /// <item><description>the input item is so small it is snapped to collapse</description></item>
    /// <item><description>the input item is covered by another input item
    /// (which may be a larger or a duplicate (nearly or exactly) geometry)</description></item>
    /// </list>
    /// The result is a valid coverage according to <see cref="CoverageValidator.IsValid(Geometry[])"/>.
    /// <h3>Known Issues</h3>
    /// <list type="bullet">
    /// <item><description>Long narrow gaps adjacent to multiple polygons may form spikes when merged with a single polygon.</description></item>
    /// </list>
    /// </summary>
    /// <seealso cref="CoverageValidator"/>
    /// <author>Martin Davis</author>
    public class CoverageCleaner
    {
        /// <summary>Merge strategy that chooses polygon with longest common border.</summary>
        public const int MergeLongestBorder = 0;

        /// <summary>Merge strategy that chooses polygon with maximum area.</summary>
        public const int MergeMaxArea = 1;

        /// <summary>Merge strategy that chooses polygon with minimum area.</summary>
        public const int MergeMinArea = 2;

        /// <summary>Merge strategy that chooses polygon with smallest input index.</summary>
        public const int MergeMinIndex = 3;

        private const double DefaultSnappingFactor = 1.0e8;

        /// <summary>
        /// Cleans a set of polygonal geometries to form a valid coverage,
        /// allowing all cleaning parameters to be specified.
        /// </summary>
        /// <param name="coverage">An array of polygonal geometries to clean</param>
        /// <param name="snappingDistance">The distance tolerance for snapping</param>
        /// <param name="overlapMergeStrategy">The strategy to use for merging overlaps</param>
        /// <param name="maxGapWidth">The maximum width of gaps to merge</param>
        /// <returns>The clean coverage</returns>
        public static Geometry[] Clean(Geometry[] coverage, double snappingDistance,
            int overlapMergeStrategy, double maxGapWidth)
        {
            var cc = new CoverageCleaner(coverage)
            {
                SnappingDistance = snappingDistance,
                GapMaximumWidth = maxGapWidth,
                OverlapMergeStrategy = overlapMergeStrategy,
            };
            cc.Clean();
            return cc.Result;
        }

        /// <summary>
        /// Cleans a set of polygonal geometries to form a valid coverage,
        /// using the default overlap merge strategy <see cref="MergeLongestBorder"/>.
        /// </summary>
        /// <param name="coverage">An array of polygonal geometries to clean</param>
        /// <param name="snappingDistance">The distance tolerance for snapping</param>
        /// <param name="maxGapWidth">The maximum width of gaps to merge</param>
        /// <returns>The clean coverage</returns>
        public static Geometry[] Clean(Geometry[] coverage, double snappingDistance, double maxGapWidth)
        {
            var cc = new CoverageCleaner(coverage)
            {
                SnappingDistance = snappingDistance,
                GapMaximumWidth = maxGapWidth,
            };
            cc.Clean();
            return cc.Result;
        }

        /// <summary>
        /// Cleans a set of polygonal geometries to form a valid coverage,
        /// using the default snapping distance tolerance.
        /// </summary>
        /// <param name="coverage">An array of polygonal geometries to clean</param>
        /// <param name="overlapMergeStrategy">The strategy to use for merging overlaps</param>
        /// <param name="maxGapWidth">The maximum width of gaps to merge</param>
        /// <returns>The clean coverage</returns>
        public static Geometry[] CleanOverlapGap(Geometry[] coverage, int overlapMergeStrategy, double maxGapWidth)
        {
            return Clean(coverage, -1, overlapMergeStrategy, maxGapWidth);
        }

        /// <summary>
        /// Cleans a set of polygonal geometries to form a valid coverage,
        /// with default snapping tolerance and overlap merging,
        /// and merging gaps which are narrower than a specified width.
        /// </summary>
        /// <param name="coverage">An array of polygonal geometries to clean</param>
        /// <param name="maxGapWidth">The maximum width of gaps to merge</param>
        /// <returns>The clean coverage</returns>
        public static Geometry[] CleanGapWidth(Geometry[] coverage, double maxGapWidth)
        {
            return Clean(coverage, -1, maxGapWidth);
        }

        private readonly Geometry[] _coverage;
        private double _snappingDistance;
        private double _gapMaximumWidth;
        private int _overlapMergeStrategy = MergeLongestBorder;

        private readonly GeometryFactory _geomFactory;
        private STRtree<int> _covIndex;
        private Polygon[] _resultants;
        private CleanCoverage _cleanCov;
        private readonly Dictionary<int, List<int>> _overlapParentMap = new Dictionary<int, List<int>>();
        private readonly List<Polygon> _overlaps = new List<Polygon>();
        private readonly List<Polygon> _gaps = new List<Polygon>();
        private List<Polygon> _mergableGaps;

        /// <summary>
        /// Create a new cleaner instance for a set of polygonal geometries.
        /// Null entries and empty / non-polygonal elements are tolerated;
        /// each produces an empty output in the corresponding result slot.
        /// </summary>
        /// <param name="coverage">An array of polygonal geometries to clean</param>
        /// <exception cref="ArgumentNullException">If <paramref name="coverage"/> itself is null.</exception>
        public CoverageCleaner(Geometry[] coverage)
        {
            _coverage = coverage ?? throw new ArgumentNullException(nameof(coverage));
            _geomFactory = FindFactory(coverage) ?? NtsGeometryServices.Instance.CreateGeometryFactory();
            _snappingDistance = ComputeDefaultSnappingDistance(coverage);
        }

        private static GeometryFactory FindFactory(Geometry[] coverage)
        {
            foreach (var g in coverage)
            {
                if (g != null) return g.Factory;
            }
            return null;
        }

        /// <summary>
        /// Gets or sets the snapping distance tolerance.
        /// The default is a small fraction of the input extent diameter.
        /// A distance of zero prevents snapping from being used.
        /// Setting a negative value leaves the current value unchanged
        /// (so callers can pass <c>-1</c> as a "use default" sentinel).
        /// </summary>
        public double SnappingDistance
        {
            get => _snappingDistance;
            set
            {
                if (value < 0) return;
                _snappingDistance = value;
            }
        }

        /// <summary>
        /// Gets or sets the overlap merge strategy to use.
        /// The default is <see cref="MergeLongestBorder"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If the value is not one of the supported strategy codes.</exception>
        public int OverlapMergeStrategy
        {
            get => _overlapMergeStrategy;
            set
            {
                if (value < MergeLongestBorder || value > MergeMinIndex)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Invalid merge strategy code");
                _overlapMergeStrategy = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum width of the gaps that will be filled and merged.
        /// The width of a gap is twice the radius of the Maximum Inscribed Circle in the gap polygon.
        /// A width of zero prevents gaps from being merged.
        /// Setting a negative value leaves the current value unchanged.
        /// </summary>
        public double GapMaximumWidth
        {
            get => _gapMaximumWidth;
            set
            {
                if (value < 0) return;
                _gapMaximumWidth = value;
            }
        }

        /// <summary>
        /// Cleans the coverage.
        /// </summary>
        public void Clean()
        {
            ComputeResultants(_snappingDistance);
            MergeOverlaps(_overlapParentMap);
            _cleanCov.MergeGaps(_mergableGaps);
        }

        /// <summary>
        /// Gets the cleaned coverage.
        /// </summary>
        public Geometry[] Result => _cleanCov.ToCoverage(_geomFactory);

        /// <summary>
        /// Gets polygons representing the overlaps in the input, which have been merged.
        /// </summary>
        public IList<Polygon> Overlaps => _overlaps;

        /// <summary>
        /// Gets polygons representing the gaps in the input which have been merged.
        /// </summary>
        public IList<Polygon> MergedGaps => _mergableGaps;

        //-------------------------------------------------

        private static double ComputeDefaultSnappingDistance(Geometry[] geoms)
        {
            double diameter = Extent(geoms).Diameter;
            return diameter / DefaultSnappingFactor;
        }

        private static Envelope Extent(Geometry[] geoms)
        {
            var env = new Envelope();
            foreach (var geom in geoms)
            {
                if (geom == null || geom.IsEmpty) continue;
                env.ExpandToInclude(geom.EnvelopeInternal);
            }
            return env;
        }

        private void MergeOverlaps(Dictionary<int, List<int>> overlapParentMap)
        {
            foreach (var entry in overlapParentMap)
            {
                _cleanCov.MergeOverlap(_resultants[entry.Key], MergeStrategy(_overlapMergeStrategy), entry.Value);
            }
        }

        private static CleanCoverage.IMergeStrategy MergeStrategy(int mergeStrategyId)
        {
            switch (mergeStrategyId)
            {
                case MergeLongestBorder: return new CleanCoverage.BorderMergeStrategy();
                case MergeMaxArea: return new CleanCoverage.AreaMergeStrategy(true);
                case MergeMinArea: return new CleanCoverage.AreaMergeStrategy(false);
                case MergeMinIndex: return new CleanCoverage.IndexMergeStrategy(false);
            }
            throw new ArgumentOutOfRangeException(nameof(mergeStrategyId), mergeStrategyId, "Unknown merge strategy");
        }

        private void ComputeResultants(double tolerance)
        {
            _cleanCov = new CleanCoverage(_coverage.Length);
            _mergableGaps = new List<Polygon>();

            var nodedEdges = Node(_coverage, tolerance);
            if (nodedEdges == null || nodedEdges.IsEmpty)
            {
                //-- Empty input, all-null input, or all-non-polygonal input: nothing to do.
                _resultants = System.Array.Empty<Polygon>();
                return;
            }

            var cleanEdges = LineDissolver.Dissolve(nodedEdges);
            _resultants = Polygonize(cleanEdges);

            CreateCoverageIndex();
            ClassifyResult(_resultants);

            _mergableGaps = FindMergableGaps(_gaps);
        }

        private void CreateCoverageIndex()
        {
            _covIndex = new STRtree<int>();
            for (int i = 0; i < _coverage.Length; i++)
            {
                if (_coverage[i] == null || _coverage[i].IsEmpty) continue;
                _covIndex.Insert(_coverage[i].EnvelopeInternal, i);
            }
        }

        private void ClassifyResult(Polygon[] resultants)
        {
            for (int i = 0; i < resultants.Length; i++)
            {
                ClassifyResultant(i, resultants[i]);
            }
        }

        private void ClassifyResultant(int resultIndex, Polygon resPoly)
        {
            var intPt = resPoly.InteriorPoint;
            int parentIndex = -1;
            List<int> overlapIndexes = null;

            var candidateParentIndex = _covIndex.Query(intPt.EnvelopeInternal);
            foreach (int i in candidateParentIndex)
            {
                var parent = _coverage[i];
                if (Covers(parent, intPt))
                {
                    if (parentIndex < 0)
                    {
                        parentIndex = i;
                    }
                    else
                    {
                        //-- more than one parent - record them all
                        if (overlapIndexes == null)
                        {
                            overlapIndexes = new List<int>();
                        }
                        overlapIndexes.Add(parentIndex);
                        overlapIndexes.Add(i);
                    }
                }
            }
            /*
             * Classify resultant based on # of parents:
             *   0 - gap
             *   1 - single polygon face
             *  >1 - overlap
             */
            if (parentIndex < 0)
            {
                _gaps.Add(resPoly);
            }
            else if (overlapIndexes != null)
            {
                _overlapParentMap[resultIndex] = overlapIndexes;
                _overlaps.Add(resPoly);
            }
            else
            {
                _cleanCov.Add(parentIndex, resPoly);
            }
        }

        private static bool Covers(Geometry poly, Point intPt)
        {
            return SimplePointInAreaLocator.IsContained(intPt.Coordinate, poly);
        }

        private List<Polygon> FindMergableGaps(List<Polygon> gaps)
        {
            var result = new List<Polygon>();
            foreach (var gap in gaps)
            {
                if (IsMergableGap(gap))
                    result.Add(gap);
            }
            return result;
        }

        private bool IsMergableGap(Polygon gap)
        {
            if (_gapMaximumWidth <= 0)
            {
                return false;
            }
            return MaximumInscribedCircle.IsRadiusWithin(gap, _gapMaximumWidth / 2.0);
        }

        private static Polygon[] Polygonize(Geometry cleanEdges)
        {
            var polygonizer = new Polygonizer();
            polygonizer.Add(cleanEdges);
            var polys = polygonizer.GetGeometry();
            return ToPolygonArray(polys);
        }

        /// <summary>
        /// Snaps and nodes the linework of a set of polygonal geometries.
        /// Null, empty, and non-polygonal elements are skipped.
        /// Returns an empty MultiLineString if no polygonal linework remains.
        /// </summary>
        public static Geometry Node(Geometry[] coverage, double snapDistance)
        {
            var segs = new List<ISegmentString>();
            GeometryFactory factory = null;
            foreach (var geom in coverage)
            {
                if (geom == null) continue;
                if (factory == null) factory = geom.Factory;
                //-- skip non-polygonal and empty elements
                if (!IsPolygonal(geom)) continue;
                if (geom.IsEmpty) continue;
                ExtractNodedSegmentStrings(geom, segs);
            }
            if (factory == null) factory = NtsGeometryServices.Instance.CreateGeometryFactory();
            if (segs.Count == 0)
            {
                return factory.CreateMultiLineString(System.Array.Empty<LineString>());
            }
            var noder = new SnappingNoder(snapDistance);
            noder.ComputeNodes(segs);
            var nodedSegStrings = noder.GetNodedSubstrings();
            return SegmentStringUtil.ToGeometry(nodedSegStrings, factory);
        }

        private static bool IsPolygonal(Geometry geom)
        {
            return geom is Polygon || geom is MultiPolygon;
        }

        private static void ExtractNodedSegmentStrings(Geometry geom, List<ISegmentString> segs)
        {
            var segsGeom = SegmentStringUtil.ExtractNodedSegmentStrings(geom);
            foreach (var s in segsGeom) segs.Add(s);
        }

        private static Polygon[] ToPolygonArray(Geometry geom)
        {
            var geoms = new Polygon[geom.NumGeometries];
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                geoms[i] = (Polygon)geom.GetGeometryN(i);
            }
            return geoms;
        }
    }
}
