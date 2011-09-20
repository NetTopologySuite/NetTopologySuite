using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Index.Chain;
using NetTopologySuite.Noding;

namespace NetTopologySuite.Geometries.Prepared
{
    /// <summary>
    /// Computes the intersections between two line segments in <see cref="ISegmentString"/>s
    /// and adds them to each string.
    /// </summary>
    /// <remarks></remarks>
    /// <para>
    /// The <see cref="ISegmentIntersector"/> is passed to a <see cref="INoder"/>.
    /// </para>
    /// <para>
    /// The <see cref="NodedSegmentString.AddIntersections(LineIntersector, int, int)"/> method is called whenever the <see cref="INoder"/>
    /// detects that two SegmentStrings <i>might</i> intersect.
    /// </para>
    /// <para>This class is an example of the <i>Strategy</i> pattern.</para>
    public class LineIntersectionAdder : ISegmentIntersector
    {
        public static bool IsAdjacentSegments(int i1, int i2)
        {
            return Math.Abs(i1 - i2) == 1;
        }

        /**
         * These variables keep track of what types of intersections were
         * found during ALL edges that have been intersected.
         */
        private bool _hasIntersection;
        private bool _hasProper;
        private bool _hasProperInterior;
        private bool _hasInterior;

        // the proper intersection point found
        private ICoordinate _properIntersectionPoint;

        private readonly LineIntersector li;
        private bool _isSelfIntersection;
        //private boolean intersectionFound;
        public int NumIntersections;
        public int NumInteriorIntersections;
        public int NumProperIntersections;

        // testing only
        public int NumTests;

        public LineIntersectionAdder(LineIntersector li)
        {
            this.li = li;
        }

        public LineIntersector LineIntersector { get { return li; } }

        ///<summary>
        /// Gets the proper intersection point, or <code>null</code> if none was found
        ///</summary>
        public ICoordinate ProperIntersectionPoint { get { return _properIntersectionPoint; } }

        public Boolean HasIntersection { get { return _hasIntersection; } }

        ///<summary>
        /// A proper intersection is an intersection which is interior to at least two
        /// line segments.
        ///</summary>
        /// <remarks>
        /// Note that a proper intersection is not necessarily
        /// in the interior of the entire Geometry, since another edge may have
        /// an endpoint equal to the intersection, which according to SFS semantics
        /// can result in the point being on the Boundary of the Geometry.
        /// </remarks>
        public bool HasProperIntersection
        {
            get { return _hasProper; }
        }

        ///<summary>
        /// A proper interior intersection is a proper intersection which is <b>not</b>
        /// contained in the set of boundary nodes set for this SegmentIntersector.
        ///</summary>
        public bool HasProperInteriorIntersection { get { return _hasProperInterior; } }

        ///<summary>
        /// An interior intersection is an intersection which is in the interior of some segment.
        ///</summary>
        public bool HasInteriorIntersection { get { return _hasInterior; } }

        ///<summary>
        /// A trivial intersection is an apparent self-intersection which in fact
        /// is simply the point shared by adjacent line segments.
        ///</summary>
        /// <remarks>
        /// Note that closed edges require a special check for the point shared by the beginning
        /// and end segments.
        /// </remarks>
        private bool IsTrivialIntersection(ISegmentString e0, int segIndex0, ISegmentString e1, int segIndex1)
        {
            if (e0 == e1)
            {
                if (li.IntersectionNum == 1)
                {
                    if (IsAdjacentSegments(segIndex0, segIndex1))
                        return true;
                    if (e0.IsClosed)
                    {
                        int maxSegIndex = e0.Count - 1;
                        if ((segIndex0 == 0 && segIndex1 == maxSegIndex)
                            || (segIndex1 == 0 && segIndex0 == maxSegIndex))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        ///<summary>
        /// This method is called by clients of the <see cref="ISegmentIntersector"/> class to process
        /// intersections for two segments of the <see cref="ISegmentString"/>s being intersected.
        ///</summary>
        /// <remarks>
        /// Note that some clients (such as <see cref="MonotoneChain"/>s) may optimize away
        /// this call for segment pairs which they have determined do not intersect
        /// (e.g. by an disjoint envelope test).
        /// </remarks>
        public void ProcessIntersections(
          ISegmentString e0, int segIndex0,
          ISegmentString e1, int segIndex1
           )
        {
            // don't intersect segment with itself
            if (e0 == e1 && segIndex0 == segIndex1) return;
            NumTests++;

            var p00 = e0.Coordinates[segIndex0];
            var p01 = e0.Coordinates[segIndex0 + 1];
            var p10 = e1.Coordinates[segIndex1];
            var p11 = e1.Coordinates[segIndex1 + 1];

            li.ComputeIntersection(p00, p01, p10, p11);
            //if (li.hasIntersection() && li.isProper()) Debug.println(li);
            if (li.HasIntersection)
            {
                //intersectionFound = true;
                NumIntersections++;
                if (li.IsInteriorIntersection())
                {
                    NumInteriorIntersections++;
                    _hasInterior = true;
                    //System.out.println(li);
                }
                // if the segments are adjacent they have at least one trivial intersection,
                // the shared endpoint.  Don't bother adding it if it is the
                // only intersection.
                if (!IsTrivialIntersection(e0, segIndex0, e1, segIndex1))
                {
                    _hasIntersection = true;

                    // only add intersection to test geom (the line)
                    ((NodedSegmentString)e1).AddIntersections(li, segIndex1, 1);

                    if (li.IsProper)
                    {
                        NumProperIntersections++;
                        //Debug.println(li.toString());  Debug.println(li.getIntersection(0));
                        //properIntersectionPoint = (Coordinate) li.getIntersection(0).clone();
                        _hasProper = true;
                        _hasProperInterior = true;
                    }
                }
            }
        }

        ///<summary>
        /// Always process all intersections
        ///</summary>
        public Boolean IsDone { get { return false; } }
    }

}
