using System;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Index.Chain;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Computes the intersections between two line segments in <see cref="SegmentString{TCoordinate}" />s
    /// and adds them to each string.
    /// The <see cref="ISegmentIntersector{TCoordinate}" /> is passed to a <see cref="INoder{TCoordinate}" />.
    /// The <see cref="SegmentString{TCoordinate}.AddIntersections" /> method is called whenever the <see cref="INoder{TCoordinate}" />
    /// detects that two <see cref="SegmentString{TCoordinate}" />s might intersect.
    /// This class is an example of the Strategy pattern.
    /// </summary>
    public class IntersectionAdder<TCoordinate> : ISegmentIntersector<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        public static Boolean IsAdjacentSegments(Int32 i1, Int32 i2)
        {
            return Math.Abs(i1 - i2) == 1;
        }

        /**
         * These variables keep track of what types of intersections were
         * found during ALL edges that have been intersected.
         */
        private Boolean _hasIntersection = false;
        private Boolean _hasProper = false;
        private Boolean _hasProperInterior = false;
        private Boolean _hasInterior = false;

        // the proper intersection point found
        private TCoordinate _properIntersectionPoint;

        private readonly LineIntersector<TCoordinate> _li = null;

        private Int32 _intersectionCount = 0;
        private Int32 _interiorIntersectionCount = 0;
        private Int32 _properIntersectionCount = 0;
        private Int32 _testCount = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntersectionAdder{TCoordinate}"/> class.
        /// </summary>
        public IntersectionAdder(LineIntersector<TCoordinate> li)
        {
            _li = li;
        }

        public LineIntersector<TCoordinate> LineIntersector
        {
            get { return _li; }
        }

        /// <summary>
        /// Returns the proper intersection point, or <see langword="null" /> if none was found.
        /// </summary>
        public TCoordinate ProperIntersectionPoint
        {
            get { return _properIntersectionPoint; }
        }

        public Boolean HasIntersection
        {
            get { return _hasIntersection; }
        }

        /// <summary>
        /// A proper intersection is an intersection which is interior to at least two
        /// line segments.  Note that a proper intersection is not necessarily
        /// in the interior of the entire <see cref="Geometry{TCoordinate}" />, since another edge may have
        /// an endpoint equal to the intersection, which according to SFS semantics
        /// can result in the point being on the Boundary of the <see cref="Geometry{TCoordinate}" />.
        /// </summary>
        public Boolean HasProperIntersection
        {
            get { return _hasProper; }
        }

        /// <summary>
        /// A proper interior intersection is a proper intersection which is not
        /// contained in the set of boundary nodes set for this <see cref="ISegmentIntersector{TCoordinate}" />.
        /// </summary>
        public Boolean HasProperInteriorIntersection
        {
            get { return _hasProperInterior; }
        }

        /// <summary>
        /// An interior intersection is an intersection which is
        /// in the interior of some segment.
        /// </summary>
        public Boolean HasInteriorIntersection
        {
            get { return _hasInterior; }
        }

        public int IntersectionCount
        {
            get { return _intersectionCount; }
            set { _intersectionCount = value; }
        }

        public int InteriorIntersectionCount
        {
            get { return _interiorIntersectionCount; }
            set { _interiorIntersectionCount = value; }
        }

        public int ProperIntersectionCount
        {
            get { return _properIntersectionCount; }
            set { _properIntersectionCount = value; }
        }

        public int TestCount
        {
            get { return _testCount; }
            set { _testCount = value; }
        }

        /// <summary>
        /// A trivial intersection is an apparent self-intersection which in fact
        /// is simply the point shared by adjacent line segments.
        /// Note that closed edges require a special check for the point shared by the beginning and end segments.
        /// </summary>
        private Boolean IsTrivialIntersection(SegmentString<TCoordinate> e0, Int32 segIndex0, SegmentString<TCoordinate> e1, Int32 segIndex1)
        {
            if (e0 == e1)
            {
                if (_li.IntersectionType == LineIntersectionType.Intersects)
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
        /// of the <see cref="ISegmentIntersector{TCoordinate}" /> class to process
        /// intersections for two segments of the <see cref="SegmentString{TCoordinate}" /> being intersected.
        /// Note that some clients (such as <see cref="MonotoneChain{TCoordinate}" />s) may optimize away
        /// this call for segment pairs which they have determined do not intersect
        /// (e.g. by an disjoint envelope test).
        /// </summary>
        public void ProcessIntersections(SegmentString<TCoordinate> e0, Int32 segIndex0, SegmentString<TCoordinate> e1, Int32 segIndex1)
        {
            if (e0 == e1 && segIndex0 == segIndex1)
            {
                return;
            }

            TestCount++;
            Pair<TCoordinate> p0 = Slice.GetPairAt(e0.Coordinates, segIndex0);
            Pair<TCoordinate> p1 = Slice.GetPairAt(e1.Coordinates, segIndex1);

            _li.ComputeIntersection(p0.First, p0.Second, p1.First, p1.Second);

            if (_li.HasIntersection)
            {
                IntersectionCount++;

                if (_li.IsInteriorIntersection())
                {
                    InteriorIntersectionCount++;
                    _hasInterior = true;
                }

                // if the segments are adjacent they have at least one trivial intersection,
                // the shared endpoint.  Don't bother adding it if it is the
                // only intersection.
                if (!IsTrivialIntersection(e0, segIndex0, e1, segIndex1))
                {
                    _hasIntersection = true;
                    e0.AddIntersections(_li, segIndex0, 0);
                    e1.AddIntersections(_li, segIndex1, 1);

                    if (_li.IsProper)
                    {
                        ProperIntersectionCount++;
                        _hasProper = true;
                        _hasProperInterior = true;
                    }
                }
            }
        }
    }
}