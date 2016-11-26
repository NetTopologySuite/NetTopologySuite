//using System.Collections;
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
    /// papers by Hobby, Guibas and Marimont, and Goodrich et al.
    /// Snap Rounding assumes that all vertices lie on a uniform grid;
    /// hence the precision model of the input must be fixed precision,
    /// and all the input vertices must be rounded to that precision.
    /// <para>
    /// This implementation uses a monotone chains and a spatial index to
    /// speed up the intersection tests.
    /// This implementation appears to be fully robust using an integer precision model.
    /// It will function with non-integer precision models, but the
    /// results are not 100% guaranteed to be correctly noded.
    /// </para>
    /// </summary>
    public class MCIndexSnapRounder : INoder
    {
        private readonly LineIntersector _li;
        private readonly double _scaleFactor;
        private MCIndexNoder _noder;
        private MCIndexPointSnapper _pointSnapper;
        private IList<ISegmentString> _nodedSegStrings;

        /// <summary>
        /// Initializes a new instance of the <see cref="MCIndexSnapRounder"/> class.
        /// </summary>
        /// <param name="pm">The <see cref="PrecisionModel" /> to use.</param>
        public MCIndexSnapRounder(IPrecisionModel pm)
        {
            _li = new RobustLineIntersector { PrecisionModel = pm };
            _scaleFactor = pm.Scale;
        }

        /// <summary>
        /// Returns a <see cref="IList{ISegmentString}"/> of fully noded <see cref="ISegmentString"/>s.
        /// The <see cref="ISegmentString"/>s have the same context as their parent.
        /// </summary>
        /// <returns></returns>
        public IList<ISegmentString> GetNodedSubstrings()
        {
            return NodedSegmentString.GetNodedSubstrings(_nodedSegStrings);
        }

        /// <summary>
        /// Computes the noding for a collection of <see cref="ISegmentString" />s.
        /// Some Noders may add all these nodes to the input <see cref="ISegmentString" />s;
        /// others may only add some or none at all.
        /// </summary>
        /// <param name="inputSegmentStrings"></param>
        public void ComputeNodes(IList<ISegmentString> inputSegmentStrings)
        {
            _nodedSegStrings = inputSegmentStrings;
            _noder = new MCIndexNoder();
            _pointSnapper = new MCIndexPointSnapper(_noder.Index);
            SnapRound(inputSegmentStrings, _li);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="segStrings"></param>
        /// <param name="li"></param>
        private void SnapRound(IList<ISegmentString> segStrings, LineIntersector li)
        {
            IList<Coordinate> intersections = FindInteriorIntersections(segStrings, li);
            ComputeIntersectionSnaps(intersections);
            ComputeVertexSnaps(segStrings);
        }

        /// <summary>
        /// Computes all interior intersections in the collection of <see cref="ISegmentString" />s,
        /// and returns their <see cref="Coordinate" />s.
        ///
        /// Does NOT node the segStrings.
        /// </summary>
        /// <param name="segStrings"></param>
        /// <param name="li"></param>
        /// <returns>A list of Coordinates for the intersections.</returns>
        private IList<Coordinate> FindInteriorIntersections(IList<ISegmentString> segStrings, LineIntersector li)
        {
            InteriorIntersectionFinderAdder intFinderAdder = new InteriorIntersectionFinderAdder(li);
            _noder.SegmentIntersector = intFinderAdder;
            _noder.ComputeNodes(segStrings);
            return intFinderAdder.InteriorIntersections;
        }

        /// <summary>
        /// Snaps segments to nodes created by segment intersections.
        /// </summary>
        /// <param name="snapPts"></param>
        private void ComputeIntersectionSnaps(IEnumerable<Coordinate> snapPts)
        {
            foreach (var snapPt in snapPts)
            {
                var hotPixel = new HotPixel(snapPt, _scaleFactor, _li);
                _pointSnapper.Snap(hotPixel);
            }
        }

        /// <summary>
        /// Snaps segments to all vertices
        /// </summary>
        /// <param name="edges">The list of segment strings to snap together</param>
        public void ComputeVertexSnaps(IList<ISegmentString> edges)
        {
            foreach (INodableSegmentString edge in edges)
                ComputeVertexSnaps(edge);
        }

        /// <summary>
        /// Snaps segments to the vertices of a Segment String.
        /// </summary>
        /// <param name="e"></param>
        private void ComputeVertexSnaps(INodableSegmentString e)
        {
            var pts0 = e.Coordinates;
            for (var i = 0; i < pts0.Length; i++)
            {
                var hotPixel = new HotPixel(pts0[i], _scaleFactor, _li);
                var isNodeAdded = _pointSnapper.Snap(hotPixel, e, i);
                // if a node is created for a vertex, that vertex must be noded too
                if (isNodeAdded)
                    e.AddIntersection(pts0[i], i);
            }
        }
    }
}