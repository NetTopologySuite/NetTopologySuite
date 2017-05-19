﻿using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;

namespace NetTopologySuite.Noding
{
    /// <summary>
    ///     Finds <b>interior</b> intersections
    ///     between line segments in <see cref="NodedSegmentString" />s,
    ///     and adds them as nodes
    ///     using <see cref="NodedSegmentString.AddIntersection(NetTopologySuite.Algorithm.LineIntersector,int,int,int)" />.
    ///     This class is used primarily for Snap-Rounding.
    ///     For general-purpose noding, use <see cref="IntersectionAdder" />.
    /// </summary>
    /// <seealso cref="IntersectionAdder" />
    public class InteriorIntersectionFinderAdder : ISegmentIntersector
    {
        private readonly LineIntersector _li;

        /// <summary>
        ///     Creates an intersection finder which finds all proper intersections.
        /// </summary>
        /// <param name="li">The <see cref="LineIntersector" /> to use.</param>
        public InteriorIntersectionFinderAdder(LineIntersector li)
        {
            _li = li;
            InteriorIntersections = new List<Coordinate>();
        }

        /// <summary>
        /// </summary>
        public IList<Coordinate> InteriorIntersections { get; }

        /// <summary>
        ///     This method is called by clients
        ///     of the <see cref="ISegmentIntersector" /> class to process
        ///     intersections for two segments of the <see cref="ISegmentString" />s being intersected.<br />
        ///     Note that some clients (such as <c>MonotoneChain</c>s) may optimize away
        ///     this call for segment pairs which they have determined do not intersect
        ///     (e.g. by an disjoint envelope test).
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="segIndex0"></param>
        /// <param name="e1"></param>
        /// <param name="segIndex1"></param>
        public void ProcessIntersections(ISegmentString e0, int segIndex0, ISegmentString e1, int segIndex1)
        {
            // don't bother intersecting a segment with itself
            if ((e0 == e1) && (segIndex0 == segIndex1))
                return;

            var coordinates0 = e0.Coordinates;
            var p00 = coordinates0[segIndex0];
            var p01 = coordinates0[segIndex0 + 1];
            var coordinates1 = e1.Coordinates;
            var p10 = coordinates1[segIndex1];
            var p11 = coordinates1[segIndex1 + 1];
            _li.ComputeIntersection(p00, p01, p10, p11);

            if (!_li.HasIntersection) return;
            if (!_li.IsInteriorIntersection()) return;
            for (var intIndex = 0; intIndex < _li.IntersectionNum; intIndex++)
                InteriorIntersections.Add(_li.GetIntersection(intIndex));

            var nss0 = (NodedSegmentString) e0;
            nss0.AddIntersections(_li, segIndex0, 0);
            var nss1 = (NodedSegmentString) e1;
            nss1.AddIntersections(_li, segIndex1, 1);
        }

        /// <summary>
        ///     Always process all intersections
        /// </summary>
        public bool IsDone => false;
    }
}