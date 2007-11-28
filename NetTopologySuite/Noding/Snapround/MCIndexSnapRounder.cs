using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

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
    public class MCIndexSnapRounder<TCoordinate> : INoder<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private LineIntersector<TCoordinate> _li = null;
        private readonly Double scaleFactor;
        private MCIndexNoder _noder = null;
        private MCIndexPointSnapper _pointSnapper = null;
        private IList nodedSegStrings = null;

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="MCIndexSnapRounder{TCoordinate}"/> class.
        /// </summary>
        /// <param name="pm">The <see cref="PrecisionModel{TCoordinate}" /> to use.</param>
        public MCIndexSnapRounder(PrecisionModel<TCoordinate> pm)
        {
            _li = new RobustLineIntersector<TCoordinate>();
            _li.PrecisionModel = pm;
            scaleFactor = pm.Scale;
        }

        /// <summary>
        /// Returns a <see cref="IList"/> of fully noded <see cref="SegmentString"/>s.
        /// The <see cref="SegmentString"/>s have the same context as their parent.
        /// </summary>
        public IList GetNodedSubstrings()
        {
            return SegmentString<TCoordinate>.GetNodedSubstrings(nodedSegStrings);
        }

        /// <summary>
        /// Computes the noding for a collection of <see cref="SegmentString" />s.
        /// Some Noders may add all these nodes to the input <see cref="SegmentString" />s;
        /// others may only add some or none at all.
        /// </summary>
        public void ComputeNodes(IList inputSegmentStrings)
        {
            nodedSegStrings = inputSegmentStrings;
            _noder = new MCIndexNoder();
            _pointSnapper = new MCIndexPointSnapper(_noder.MonotoneChains, _noder.Index);
            snapRound(inputSegmentStrings, _li);
        }

        private void checkCorrectness(IList inputSegmentStrings)
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

        private void snapRound(IList segStrings, LineIntersector<TCoordinate> li)
        {
            IList intersections = findInteriorIntersections(segStrings, li);
            computeIntersectionSnaps(intersections);
            ComputeVertexSnaps(segStrings);
        }

        /// <summary>
        /// Computes all interior intersections in the collection of <see cref="SegmentString" />s,
        /// and returns their <see cref="Coordinate" />s.
        ///
        /// Does NOT node the segStrings.
        /// </summary>
        /// <returns>A list of Coordinates for the intersections.</returns>
        private IList findInteriorIntersections(IList segStrings, LineIntersector li)
        {
            IntersectionFinderAdder intFinderAdder = new IntersectionFinderAdder(li);
            _noder.SegmentIntersector = intFinderAdder;
            _noder.ComputeNodes(segStrings);
            return intFinderAdder.InteriorIntersections;
        }

        /// <summary>
        /// Computes nodes introduced as a result of snapping segments to snap points (hot pixels).
        /// </summary>
        private void computeIntersectionSnaps(IEnumerable<TCoordinate> snapPts)
        {
            foreach (TCoordinate snapPt in snapPts)
            {
                HotPixel<TCoordinate> hotPixel = new HotPixel<TCoordinate>(
                    snapPt, scaleFactor, _li);

                _pointSnapper.Snap(hotPixel);
            }
        }

        /// <summary>
        /// Computes nodes introduced as a result of
        /// snapping segments to vertices of other segments.
        /// </summary>
        public void ComputeVertexSnaps(IEnumerable<SegmentString<TCoordinate>> edges)
        {
            foreach (SegmentString<TCoordinate> edge in edges)
            {
                computeVertexSnaps(edge);
            }
        }

        /// <summary>
        /// Performs a brute-force comparison of every segment in each 
        /// <see cref="SegmentString{TCoordinate}" />.
        /// This has O(n^2).
        /// </summary>
        private void computeVertexSnaps(SegmentString<TCoordinate> e)
        {
            IEnumerable<TCoordinate> pts0 = e.Coordinates;

            for (Int32 i = 0; i < pts0.Length - 1; i++)
            {
                HotPixel hotPixel = new HotPixel(pts0[i], scaleFactor, _li);
                Boolean isNodeAdded = _pointSnapper.Snap(hotPixel, e, i);
                // if a node is created for a vertex, that vertex must be noded too
                if (isNodeAdded)
                {
                    e.AddIntersection(pts0[i], i);
                }
            }
        }
    }
}