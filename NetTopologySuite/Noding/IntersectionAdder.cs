using System;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Index.Chain;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Computes the intersections between two line segments in <see cref="SegmentString" />s
    /// and adds them to each string.
    /// The <see cref="ISegmentIntersector" /> is passed to a <see cref="INoder" />.
    /// The <see cref="SegmentString.AddIntersections" /> method is called whenever the <see cref="INoder" />
    /// detects that two <see cref="SegmentString" />s might intersect.
    /// This class is an example of the Strategy pattern.
    /// </summary>
    public class IntersectionAdder : ISegmentIntersector
    {
        public static Boolean IsAdjacentSegments(Int32 i1, Int32 i2)
        {
            return Math.Abs(i1 - i2) == 1;
        }

        /**
         * These variables keep track of what types of intersections were
         * found during ALL edges that have been intersected.
         */
        private Boolean hasIntersection = false;
        private Boolean hasProper = false;
        private Boolean hasProperInterior = false;
        private Boolean hasInterior = false;

        // the proper intersection point found
        private ICoordinate properIntersectionPoint = null;

        private LineIntersector li = null;

        public Int32 NumIntersections = 0;

        public Int32 NumInteriorIntersections = 0;

        public Int32 NumProperIntersections = 0;

        public Int32 NumTests = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntersectionAdder"/> class.
        /// </summary>
        public IntersectionAdder(LineIntersector li)
        {
            this.li = li;
        }

        public LineIntersector LineIntersector
        {
            get { return li; }
        }

        /// <summary>
        /// Returns the proper intersection point, or <c>null</c> if none was found.
        /// </summary>
        public ICoordinate ProperIntersectionPoint
        {
            get { return properIntersectionPoint; }
        }

        public Boolean HasIntersection
        {
            get { return hasIntersection; }
        }

        /// <summary>
        /// A proper intersection is an intersection which is interior to at least two
        /// line segments.  Note that a proper intersection is not necessarily
        /// in the interior of the entire <see cref="Geometry" />, since another edge may have
        /// an endpoint equal to the intersection, which according to SFS semantics
        /// can result in the point being on the Boundary of the <see cref="Geometry" />.
        /// </summary>
        public Boolean HasProperIntersection
        {
            get { return hasProper; }
        }

        /// <summary>
        /// A proper interior intersection is a proper intersection which is not
        /// contained in the set of boundary nodes set for this <see cref="ISegmentIntersector" />.
        /// </summary>
        public Boolean HasProperInteriorIntersection
        {
            get { return hasProperInterior; }
        }

        /// <summary>
        /// An interior intersection is an intersection which is
        /// in the interior of some segment.
        /// </summary>
        public Boolean HasInteriorIntersection
        {
            get { return hasInterior; }
        }

        /// <summary>
        /// A trivial intersection is an apparent self-intersection which in fact
        /// is simply the point shared by adjacent line segments.
        /// Note that closed edges require a special check for the point shared by the beginning and end segments.
        /// </summary>
        private Boolean IsTrivialIntersection(SegmentString e0, Int32 segIndex0, SegmentString e1, Int32 segIndex1)
        {
            if (e0 == e1)
            {
                if (li.IntersectionNum == 1)
                {
                    if (IsAdjacentSegments(segIndex0, segIndex1))
                    {
                        return true;
                    }
                    if (e0.IsClosed)
                    {
                        Int32 maxSegIndex = e0.Count - 1;
                        if ((segIndex0 == 0 && segIndex1 == maxSegIndex) ||
                            (segIndex1 == 0 && segIndex0 == maxSegIndex))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// This method is called by clients
        /// of the <see cref="ISegmentIntersector" /> class to process
        /// intersections for two segments of the <see cref="SegmentString" /> being intersected.
        /// Note that some clients (such as <see cref="MonotoneChain" />s) may optimize away
        /// this call for segment pairs which they have determined do not intersect
        /// (e.g. by an disjoint envelope test).
        /// </summary>
        public void ProcessIntersections(SegmentString e0, Int32 segIndex0, SegmentString e1, Int32 segIndex1)
        {
            if (e0 == e1 && segIndex0 == segIndex1)
            {
                return;
            }

            NumTests++;
            ICoordinate p00 = e0.Coordinates[segIndex0];
            ICoordinate p01 = e0.Coordinates[segIndex0 + 1];
            ICoordinate p10 = e1.Coordinates[segIndex1];
            ICoordinate p11 = e1.Coordinates[segIndex1 + 1];

            li.ComputeIntersection(p00, p01, p10, p11);
            if (li.HasIntersection)
            {
                NumIntersections++;
                if (li.IsInteriorIntersection())
                {
                    NumInteriorIntersections++;
                    hasInterior = true;
                }
                // if the segments are adjacent they have at least one trivial intersection,
                // the shared endpoint.  Don't bother adding it if it is the
                // only intersection.
                if (!IsTrivialIntersection(e0, segIndex0, e1, segIndex1))
                {
                    hasIntersection = true;
                    e0.AddIntersections(li, segIndex0, 0);
                    e1.AddIntersections(li, segIndex1, 1);
                    if (li.IsProper)
                    {
                        NumProperIntersections++;
                        hasProper = true;
                        hasProperInterior = true;
                    }
                }
            }
        }
    }
}