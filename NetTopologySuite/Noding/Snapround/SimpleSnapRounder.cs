using System;
using System.Collections;
using System.Diagnostics;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Noding.Snapround
{
    /// <summary>
    /// Uses Snap Rounding to compute a rounded,
    /// fully noded arrangement from a set of <see cref="SegmentString" />s.
    /// Implements the Snap Rounding technique described in Hobby, Guibas and Marimont, and Goodrich et al.
    /// Snap Rounding assumes that all vertices lie on a uniform grid
    /// (hence the precision model of the input must be fixed precision,
    /// and all the input vertices must be rounded to that precision).
    /// <para>
    /// This implementation uses simple iteration over the line segments.
    /// This implementation appears to be fully robust using an integer precision model.
    /// It will function with non-integer precision models, but the
    /// results are not 100% guaranteed to be correctly noded.
    /// </para>
    /// </summary>
    public class SimpleSnapRounder : INoder
    {
        private LineIntersector li = null;
        private readonly Double scaleFactor;
        private IList nodedSegStrings = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleSnapRounder"/> class.
        /// </summary>
        /// <param name="pm">The <see cref="PrecisionModel" /> to use.</param>
        public SimpleSnapRounder(PrecisionModel pm)
        {
            li = new RobustLineIntersector();
            li.PrecisionModel = pm;
            scaleFactor = pm.Scale;
        }

        /// <summary>
        /// Returns a <see cref="IList"/> of fully noded <see cref="SegmentString"/>s.
        /// The <see cref="SegmentString"/>s have the same context as their parent.
        /// </summary>
        public IList GetNodedSubstrings()
        {
            return SegmentString.GetNodedSubstrings(nodedSegStrings);
        }

        /// <summary>
        /// Computes the noding for a collection of <see cref="SegmentString" />s.
        /// Some Noders may add all these nodes to the input <see cref="SegmentString" />s;
        /// others may only add some or none at all.
        /// </summary>
        public void ComputeNodes(IList inputSegmentStrings)
        {
            nodedSegStrings = inputSegmentStrings;
            SnapRound(inputSegmentStrings, li);
        }

        private void CheckCorrectness(IList inputSegmentStrings)
        {
            IList resultSegStrings = SegmentString.GetNodedSubstrings(inputSegmentStrings);
            NodingValidator nv = new NodingValidator(resultSegStrings);
            try
            {
                nv.CheckValid();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }

        private void SnapRound(IList segStrings, LineIntersector li)
        {
            IList intersections = FindInteriorIntersections(segStrings, li);
            ComputeSnaps(segStrings, intersections);
            ComputeVertexSnaps(segStrings);
        }

        /// <summary>
        /// Computes all interior intersections in the collection of <see cref="SegmentString" />s,
        /// and returns their <see cref="Coordinate" />s.
        /// Does NOT node the segStrings.
        /// </summary>
        /// <returns>A list of <see cref="Coordinate" />s for the intersections.</returns>
        private IList FindInteriorIntersections(IList segStrings, LineIntersector li)
        {
            IntersectionFinderAdder intFinderAdder = new IntersectionFinderAdder(li);
            SinglePassNoder noder = new MCIndexNoder(intFinderAdder);
            noder.ComputeNodes(segStrings);
            return intFinderAdder.InteriorIntersections;
        }

        /// <summary>
        /// Computes nodes introduced as a result of snapping segments to snap points (hot pixels).
        /// </summary>
        private void ComputeSnaps(IList segStrings, IList snapPts)
        {
            foreach (SegmentString ss in segStrings)
            {
                ComputeSnaps(ss, snapPts);
            }
        }

        private void ComputeSnaps(SegmentString ss, IList snapPts)
        {
            foreach (ICoordinate snapPt in snapPts)
            {
                HotPixel hotPixel = new HotPixel(snapPt, scaleFactor, li);
                for (Int32 i = 0; i < ss.Count - 1; i++)
                {
                    AddSnappedNode(hotPixel, ss, i);
                }
            }
        }

        /// <summary>
        /// Computes nodes introduced as a result of
        /// snapping segments to vertices of other segments.
        /// </summary>
        public void ComputeVertexSnaps(IList edges)
        {
            foreach (SegmentString edge0 in edges)
            {
                foreach (SegmentString edge1 in edges)
                {
                    ComputeVertexSnaps(edge0, edge1);
                }
            }
        }

        /// <summary>
        /// Performs a brute-force comparison of every segment in each <see cref="SegmentString" />.
        /// This has n^2 performance.
        /// </summary>
        private void ComputeVertexSnaps(SegmentString e0, SegmentString e1)
        {
            ICoordinate[] pts0 = e0.Coordinates;
            ICoordinate[] pts1 = e1.Coordinates;
            for (Int32 i0 = 0; i0 < pts0.Length - 1; i0++)
            {
                HotPixel hotPixel = new HotPixel(pts0[i0], scaleFactor, li);
                for (Int32 i1 = 0; i1 < pts1.Length - 1; i1++)
                {
                    // don't snap a vertex to itself
                    if (e0 == e1)
                    {
                        if (i0 == i1)
                        {
                            continue;
                        }
                    }

                    Boolean isNodeAdded = AddSnappedNode(hotPixel, e1, i1);
                    // if a node is created for a vertex, that vertex must be noded too
                    if (isNodeAdded)
                    {
                        e0.AddIntersection(pts0[i0], i0);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a new node (equal to the snap pt) to the segment
        /// if the segment passes through the hot pixel.
        /// </summary>
        public static Boolean AddSnappedNode(HotPixel hotPix, SegmentString segStr, Int32 segIndex)
        {
            ICoordinate p0 = segStr.GetCoordinate(segIndex);
            ICoordinate p1 = segStr.GetCoordinate(segIndex + 1);

            if (hotPix.Intersects(p0, p1))
            {
                segStr.AddIntersection(hotPix.Coordinate, segIndex);
                return true;
            }
            return false;
        }
    }
}