using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding.Snapround
{
    /// <summary>
    /// Uses Snap Rounding to compute a rounded,
    /// fully noded arrangement from a set of <see cref="SegmentString{TCoordinate}" />s.
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
    public class SimpleSnapRounder<TCoordinate> : INoder<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {

        /// <summary>
        /// Adds a new node (equal to the snap pt) to the segment
        /// if the segment passes through the hot pixel.
        /// </summary>
        public static Boolean AddSnappedNode(HotPixel<TCoordinate> hotPix, SegmentString<TCoordinate> segStr, Int32 segIndex)
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

        private readonly LineIntersector<TCoordinate> _li = null;
        private readonly Double _scaleFactor;
        private IEnumerable<SegmentString<TCoordinate>> _nodedSegStrings = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleSnapRounder{TCoordinate}"/> class.
        /// </summary>
        /// <param name="pm">The <see cref="IPrecisionModel{TCoordinate}" /> to use.</param>
        public SimpleSnapRounder(IPrecisionModel<TCoordinate> pm)
        {
            _li = new RobustLineIntersector<TCoordinate>();
            _li.PrecisionModel = pm;
            _scaleFactor = pm.Scale;
        }

        /// <summary>
        /// Returns a set of fully noded <see cref="SegmentString{TCoordinate}"/>s.
        /// The <see cref="SegmentString{TCoordinate}"/>s have the same context as their parent.
        /// </summary>
        public IEnumerable<SegmentString<TCoordinate>> GetNodedSubstrings()
        {
            return SegmentString<TCoordinate>.GetNodedSubstrings(_nodedSegStrings);
        }

        /// <summary>
        /// Computes the noding for a collection of <see cref="SegmentString{TCoordinate}" />s.
        /// Some Noders may add all these nodes to the input <see cref="SegmentString{TCoordinate}" />s;
        /// others may only add some or none at all.
        /// </summary>
        public void ComputeNodes(IEnumerable<SegmentString<TCoordinate>> inputSegmentStrings)
        {
            _nodedSegStrings = inputSegmentStrings;
            snapRound(inputSegmentStrings, _li);
        }

        private void checkCorrectness(IEnumerable<SegmentString<TCoordinate>> inputSegmentStrings)
        {
            IEnumerable<SegmentString<TCoordinate>> resultSegStrings 
                = SegmentString<TCoordinate>.GetNodedSubstrings(inputSegmentStrings);
            NodingValidator<TCoordinate> nv = new NodingValidator<TCoordinate>(resultSegStrings);

            try
            {
                nv.CheckValid();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }

        private void snapRound(IEnumerable<SegmentString<TCoordinate>> segStrings, LineIntersector<TCoordinate> li)
        {
            IList intersections = findInteriorIntersections(segStrings, li);
            computeSnaps(segStrings, intersections);
            ComputeVertexSnaps(segStrings);
        }

        /// <summary>
        /// Computes all interior intersections in the collection of <see cref="SegmentString{TCoordinate}" />s,
        /// and returns their <typeparamref name="TCoordinate"/>s.
        /// Does NOT node the segStrings.
        /// </summary>
        /// <returns>A list of <typeparamref name="TCoordinate"/>s for the intersections.</returns>
        private IEnumerable<TCoordinate> findInteriorIntersections(IEnumerable<SegmentString<TCoordinate>> segStrings, LineIntersector<TCoordinate> li)
        {
            IntersectionFinderAdder<TCoordinate> intFinderAdder = new IntersectionFinderAdder<TCoordinate>(li);
            SinglePassNoder noder = new MCIndexNoder(intFinderAdder);
            noder.ComputeNodes(segStrings);
            return intFinderAdder.InteriorIntersections;
        }

        /// <summary>
        /// Computes nodes introduced as a result of snapping segments to snap points (hot pixels).
        /// </summary>
        private void computeSnaps(IList segStrings, IList snapPts)
        {
            foreach (SegmentString ss in segStrings)
            {
                computeSnaps(ss, snapPts);
            }
        }

        private void computeSnaps(SegmentString ss, IList snapPts)
        {
            foreach (ICoordinate snapPt in snapPts)
            {
                HotPixel hotPixel = new HotPixel(snapPt, _scaleFactor, _li);
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
                    computeVertexSnaps(edge0, edge1);
                }
            }
        }

        /// <summary>
        /// Performs a brute-force comparison of every segment in each <see cref="SegmentString" />.
        /// This has n^2 performance.
        /// </summary>
        private void computeVertexSnaps(SegmentString e0, SegmentString e1)
        {
            ICoordinate[] pts0 = e0.Coordinates;
            ICoordinate[] pts1 = e1.Coordinates;
            for (Int32 i0 = 0; i0 < pts0.Length - 1; i0++)
            {
                HotPixel hotPixel = new HotPixel(pts0[i0], _scaleFactor, _li);
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
    }
}