using System;
using System.Collections;
using System.Diagnostics;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Noding.Snapround
{

    /// <summary>
    /// Uses Snap Rounding to compute a rounded,
    /// fully noded arrangement from a set of {@link SegmentString}s.
    /// Implements the Snap Rounding technique described in Hobby, Guibas and Marimont, and Goodrich et al.
    /// Snap Rounding assumes that all vertices lie on a uniform grid
    /// (hence the precision model of the input must be fixed precision,
    /// and all the input vertices must be rounded to that precision).
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
        private LineIntersector li = null;
        private readonly double scaleFactor;
        private MCIndexNoder noder = null;
        private MCIndexPointSnapper pointSnapper = null;
        private IList nodedSegStrings = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MCIndexSnapRounder"/> class.
        /// </summary>
        /// <param name="pm">The <see cref="PrecisionModel" /> to use.</param>
        public MCIndexSnapRounder(PrecisionModel pm) 
        {
            li = new RobustLineIntersector();
            li.PrecisionModel = pm;
            scaleFactor = pm.Scale;
        }

        /// <summary>
        /// Returns a <see cref="IList"/> of fully noded <see cref="SegmentString"/>s.
        /// The <see cref="SegmentString"/>s have the same context as their parent.
        /// </summary>
        /// <returns></returns>
        public IList GetNodedSubstrings()
        {
            return SegmentString.GetNodedSubstrings(nodedSegStrings);
        }

        /// <summary>
        /// Computes the noding for a collection of <see cref="SegmentString" />s.
        /// Some Noders may add all these nodes to the input <see cref="SegmentString" />s;
        /// others may only add some or none at all.
        /// </summary>
        /// <param name="inputSegmentStrings"></param>
        public void ComputeNodes(IList inputSegmentStrings)
        {
            this.nodedSegStrings = inputSegmentStrings;
            noder = new MCIndexNoder();
            pointSnapper = new MCIndexPointSnapper(noder.MonotoneChains, noder.Index);
            SnapRound(inputSegmentStrings, li);            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputSegmentStrings"></param>
        private void CheckCorrectness(IList inputSegmentStrings)
        {
            IList resultSegStrings = SegmentString.GetNodedSubstrings(inputSegmentStrings);
            NodingValidator nv = new NodingValidator(resultSegStrings);
            try
            {
                nv.CheckValid();
            }
            catch (Exception ex) { Trace.WriteLine(ex.ToString()); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segStrings"></param>
        /// <param name="li"></param>
        private void SnapRound(IList segStrings, LineIntersector li)
        {
            IList intersections = FindInteriorIntersections(segStrings, li);
            ComputeIntersectionSnaps(intersections);
            ComputeVertexSnaps(segStrings);        
        }

        /// <summary>
        /// Computes all interior intersections in the collection of <see cref="SegmentString" />s,
        /// and returns their <see cref="Coordinate" />s.
        ///
        /// Does NOT node the segStrings.
        /// </summary>
        /// <param name="segStrings"></param>
        /// <param name="li"></param>
        /// <returns>A list of Coordinates for the intersections.</returns>
        private IList FindInteriorIntersections(IList segStrings, LineIntersector li)
        {
            IntersectionFinderAdder intFinderAdder = new IntersectionFinderAdder(li);
            noder.SegmentIntersector = intFinderAdder;
            noder.ComputeNodes(segStrings);
            return intFinderAdder.InteriorIntersections;
        }

        /// <summary>
        /// Computes nodes introduced as a result of snapping segments to snap points (hot pixels).
        /// </summary>
        /// <param name="snapPts"></param>
        private void ComputeIntersectionSnaps(IList snapPts)
        {
            foreach (ICoordinate snapPt in snapPts)
            {
                HotPixel hotPixel = new HotPixel(snapPt, scaleFactor, li);
                pointSnapper.Snap(hotPixel);
            }
        }

        /// <summary>
        /// Computes nodes introduced as a result of
        /// snapping segments to vertices of other segments.
        /// </summary>
        /// <param name="edges"></param>
        public void ComputeVertexSnaps(IList edges)
        {
            foreach (SegmentString edge in edges)
                ComputeVertexSnaps(edge);            
        }

        /// <summary>
        /// Performs a brute-force comparison of every segment in each <see cref="SegmentString" />.
        /// This has n^2 performance.
        /// </summary>
        /// <param name="e"></param>
        private void ComputeVertexSnaps(SegmentString e)
        {
            ICoordinate[] pts0 = e.Coordinates;
            for(int i = 0; i < pts0.Length - 1; i++)
            {
                HotPixel hotPixel = new HotPixel(pts0[i], scaleFactor, li);
                bool isNodeAdded = pointSnapper.Snap(hotPixel, e, i);
                // if a node is created for a vertex, that vertex must be noded too
                if (isNodeAdded)
                    e.AddIntersection(pts0[i], i);
            }
        }
    }
}
