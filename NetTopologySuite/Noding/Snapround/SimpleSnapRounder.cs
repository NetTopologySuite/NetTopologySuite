using System.Collections;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Noding.Snapround
{
    /// <summary>
    /// Uses Snap Rounding to compute a rounded,
    /// fully noded arrangement from a set of <see cref="ISegmentString" />s.
    /// Implements the Snap Rounding technique described in
    /// the papers by Hobby, Guibas &amp; Marimont, and Goodrich et al.
    /// Snap Rounding assumes that all vertices lie on a uniform grid;
    /// hence the precision model of the input must be fixed precision,
    /// and all the input vertices must be rounded to that precision.
    /// <para>
    /// This implementation uses simple iteration over the line segments.
    /// This is not the most efficient approach for large sets of segments.
    /// This implementation appears to be fully robust using an integer precision model.
    /// It will function with non-integer precision models, but the
    /// results are not 100% guaranteed to be correctly noded.
    /// </para>
    /// </summary>
    public class SimpleSnapRounder : INoder
    {
        private readonly LineIntersector _li;
        private readonly double _scaleFactor;
        private IList<ISegmentString> _nodedSegStrings;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleSnapRounder"/> class.
        /// </summary>
        /// <param name="pm">The <see cref="PrecisionModel" /> to use.</param>
        public SimpleSnapRounder(PrecisionModel pm)
        {
            _li = new RobustLineIntersector { PrecisionModel = pm };
            _scaleFactor = pm.Scale;
        }

        /// <summary>
        /// Returns a <see cref="IList"/> of fully noded <see cref="ISegmentString"/>s.
        /// The <see cref="ISegmentString"/>s have the same context as their parent.
        /// </summary>
        /// <returns>A Collection of NodedSegmentStrings representing the substrings</returns>
        public IList<ISegmentString> GetNodedSubstrings()
        {
            return NodedSegmentString.GetNodedSubstrings(_nodedSegStrings);
        }

        /// <summary>
        /// Computes the noding for a collection of <see cref="ISegmentString" />s.
        /// Some Noders may add all these nodes to the input <see cref="ISegmentString" />s;
        /// others may only add some or none at all.
        /// </summary>
        /// <param name="inputSegmentStrings">A collection of NodedSegmentStrings</param>
        public void ComputeNodes(IList<ISegmentString> inputSegmentStrings)
        {
            _nodedSegStrings = inputSegmentStrings;
            SnapRound(inputSegmentStrings, _li);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="segStrings"></param>
        /// <param name="li"></param>
        private void SnapRound(IList<ISegmentString> segStrings, LineIntersector li)
        {
            var intersections = FindInteriorIntersections(segStrings, li);
            ComputeSnaps(segStrings, intersections);
            ComputeVertexSnaps(segStrings);
        }

        /// <summary>
        /// Computes all interior intersections in the collection of <see cref="ISegmentString" />s,
        /// and returns their <see cref="Coordinate" />s.
        /// Does NOT node the segStrings.
        /// </summary>
        /// <param name="segStrings"></param>
        /// <param name="li"></param>
        /// <returns>A list of <see cref="Coordinate" />s for the intersections.</returns>
        private static IList<Coordinate> FindInteriorIntersections(IList<ISegmentString> segStrings, LineIntersector li)
        {
            var intFinderAdder = new InteriorIntersectionFinderAdder(li);
            var noder = new MCIndexNoder(intFinderAdder);
            noder.ComputeNodes(segStrings);
            return intFinderAdder.InteriorIntersections;
        }

        /// <summary>
        /// Computes nodes introduced as a result of snapping segments to snap points (hot pixels).
        /// </summary>
        /// <param name="segStrings"></param>
        /// <param name="snapPts"></param>
        private void ComputeSnaps(IEnumerable<ISegmentString> segStrings, ICollection<Coordinate> snapPts)
        {
            foreach (INodableSegmentString ss in segStrings)
                ComputeSnaps(ss, snapPts);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ss"></param>
        /// <param name="snapPts"></param>
        private void ComputeSnaps(INodableSegmentString ss, IEnumerable<Coordinate> snapPts)
        {
            foreach (var snapPt in snapPts)
            {
                var hotPixel = new HotPixel(snapPt, _scaleFactor, _li);
                for (int i = 0; i < ss.Count - 1; i++)
                    //AddSnappedNode(hotPixel, ss, i);
                    hotPixel.AddSnappedNode(ss, i);
            }
        }

        /// <summary>
        /// Computes nodes introduced as a result of
        /// snapping segments to vertices of other segments.
        /// </summary>
        /// <param name="edges">The list of segment strings to snap together</param>
        public void ComputeVertexSnaps(IList<ISegmentString> edges)
        {
            foreach (INodableSegmentString edge0 in edges)
                foreach (INodableSegmentString edge1 in edges)
                    ComputeVertexSnaps(edge0, edge1);
        }

        /// <summary>
        /// Performs a brute-force comparison of every segment in each <see cref="ISegmentString" />.
        /// This has n^2 performance.
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="e1"></param>
        private void ComputeVertexSnaps(INodableSegmentString e0, INodableSegmentString e1)
        {
            var pts0 = e0.Coordinates;
            var pts1 = e1.Coordinates;
            for (int i0 = 0; i0 < pts0.Length - 1; i0++)
            {
                var hotPixel = new HotPixel(pts0[i0], _scaleFactor, _li);
                for (int i1 = 0; i1 < pts1.Length - 1; i1++)
                {
                    // don't snap a vertex to itself
                    if (e0 == e1)
                        if (i0 == i1)
                            continue;

                    bool isNodeAdded = //AddSnappedNode(hotPixel, e1, i1);
                                       hotPixel.AddSnappedNode(e1, i1);
                    // if a node is created for a vertex, that vertex must be noded too
                    if (isNodeAdded)
                        e0.AddIntersection(pts0[i0], i0);
                }
            }
        }
    }
}