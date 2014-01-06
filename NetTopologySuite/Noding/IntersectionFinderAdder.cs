using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;

namespace NetTopologySuite.Noding
{
    /// <summary>
    /// Finds <b>proper, interior</b> intersections 
    /// between line segments in <see cref="NodedSegmentString"/>s,
    /// and adds them as nodes
    /// using <see cref="NodedSegmentString.AddIntersection(NetTopologySuite.Algorithm.LineIntersector,int,int,int)"/>.
    /// </summary>
    /// <remarks>
    /// This class is used primarily for Snap-Rounding.  
    /// For general-purpose noding, use <see cref="IntersectionAdder"/>.
    /// </remarks>
    /// <seealso cref="IntersectionAdder"/>
    public class IntersectionFinderAdder : ISegmentIntersector
    {
        private readonly LineIntersector _li;
        private readonly IList<Coordinate> _interiorIntersections;

        /// <summary>
        /// Creates an intersection finder which finds all proper intersections.
        /// </summary>
        /// <param name="li">The <see cref="LineIntersector" /> to use.</param>
        public IntersectionFinderAdder(LineIntersector li)
        {
            _li = li;
            _interiorIntersections = new List<Coordinate>();
        }

        /// <summary>
        /// 
        /// </summary>
        public IList<Coordinate> InteriorIntersections
        {
            get { return _interiorIntersections; }
        }

        /// <summary>
        /// This method is called by clients
        /// of the <see cref="ISegmentIntersector" /> class to process
        /// intersections for two segments of the <see cref="ISegmentString" />s being intersected.<br/>
        /// Note that some clients (such as <c>MonotoneChain</c>s) may optimize away
        /// this call for segment pairs which they have determined do not intersect
        /// (e.g. by an disjoint envelope test).
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="segIndex0"></param>
        /// <param name="e1"></param>
        /// <param name="segIndex1"></param>
        public void ProcessIntersections(ISegmentString e0, int segIndex0, ISegmentString e1, int segIndex1)
        {
            // don't bother intersecting a segment with itself
            if (e0 == e1 && segIndex0 == segIndex1)
                return;

            Coordinate[] coordinates0 = e0.Coordinates;
            Coordinate p00 = coordinates0[segIndex0];
            Coordinate p01 = coordinates0[segIndex0 + 1];
            Coordinate[] coordinates1 = e1.Coordinates;
            Coordinate p10 = coordinates1[segIndex1];
            Coordinate p11 = coordinates1[segIndex1 + 1];
            _li.ComputeIntersection(p00, p01, p10, p11);

            if (_li.HasIntersection)
            {
                if (_li.IsInteriorIntersection())
                {
                    for (int intIndex = 0; intIndex < _li.IntersectionNum; intIndex++)
                        _interiorIntersections.Add(_li.GetIntersection(intIndex));

                    ((NodedSegmentString)e0).AddIntersections(_li, segIndex0, 0);
                    ((NodedSegmentString)e1).AddIntersections(_li, segIndex1, 1);
                }
            }
        }
        ///<summary>
        /// Always process all intersections
        ///</summary>
        public bool IsDone
        {
            get { return false; }
        }

    }
}
