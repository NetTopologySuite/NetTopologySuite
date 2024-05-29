using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph.Index;
using NetTopologySuite.Noding;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Triangulate.Polygon
{
    /// <summary>
    /// Adds node vertices to the rings of a polygon
    /// where holes touch the shell or each other.
    /// The structure of the polygon is preserved.
    /// <para/>
    /// This does not fix invalid polygon topology
    /// (such as self-touching or crossing rings).
    /// Invalid input remains invalid after noding,
    /// and does not trigger an error.
    /// </summary>
    /// <author>Martin Davis</author>
    internal class PolygonNoder
    {

        private readonly bool[] _isHoleTouching;
        private readonly IList<ISegmentString> _nodedRings;

        public PolygonNoder(Coordinate[] shellRing, Coordinate[][] holeRings)
        {
            _nodedRings = CreateNodedSegmentStrings(shellRing, holeRings);
            _isHoleTouching = new bool[holeRings.Length];
        }

        public void Node()
        {
            var nodeAdder = new NodeAdder(_isHoleTouching);
            var noder = new MCIndexNoder(nodeAdder);
            noder.ComputeNodes(_nodedRings);
        }

        public bool IsShellNoded => ((NodedSegmentString)_nodedRings[0]).HasNodes;

        public bool IsHoleNoded(int i)
        {
            return ((NodedSegmentString)_nodedRings[i + 1]).HasNodes;
        }

        public Coordinate[] NodedShell => ((NodedSegmentString)_nodedRings[0]).NodedCoordinates;

        public Coordinate[] GetNodedHole(int i) => ((NodedSegmentString)_nodedRings[i + 1]).NodedCoordinates;

        public bool[] HolesTouching => _isHoleTouching;

        public static IList<ISegmentString> CreateNodedSegmentStrings(Coordinate[] shellRing, Coordinate[][] holeRings)
        {
            var segStr = new List<ISegmentString>(1 + holeRings?.Length ?? 0);
            segStr.Add(CreateNodedSegString(shellRing, -1));
            for (int i = 0; i < holeRings.Length; i++)
            {
                segStr.Add(CreateNodedSegString(holeRings[i], i));
            }
            return segStr;
        }

        private static ISegmentString CreateNodedSegString(Coordinate[] ringPts, int i)
        {
            return new NodedSegmentString(ringPts, i);
        }

        /// <summary>
        /// A <see cref="ISegmentIntersector"/> that added node vertices
        /// to <see cref="NodedSegmentString"/>s where a segment touches another
        /// segment in its interior.
        /// </summary>
        /// <author>Martin Davis</author>
        private class NodeAdder : ISegmentIntersector
        {

            private readonly LineIntersector li = new RobustLineIntersector();
            private readonly bool[] _isHoleTouching;

            public NodeAdder(bool[] isHoleTouching)
            {
                _isHoleTouching = isHoleTouching;
            }

            public void ProcessIntersections(ISegmentString ss0, int segIndex0, ISegmentString ss1, int segIndex1)
            {
                //-- input is assumed valid, so rings do not self-intersect
                if (ss0 == ss1)
                    return;

                var p00 = ss0.Coordinates[segIndex0];
                var p01 = ss0.Coordinates[segIndex0 + 1];
                var p10 = ss1.Coordinates[segIndex1];
                var p11 = ss1.Coordinates[segIndex1 + 1];

                li.ComputeIntersection(p00, p01, p10, p11);
                /*
                 * There should never be 2 intersection points, since
                 * that would imply collinear segments, and an invalid polygon
                 */
                if (li.IntersectionNum == 1)
                {
                    AddTouch(ss0);
                    AddTouch(ss1);
                    var intPt = li.GetIntersection(0);
                    if (li.IsInteriorIntersection(0))
                    {
                        ((NodedSegmentString)ss0).AddIntersectionNode(intPt, segIndex0);
                    }
                    else if (li.IsInteriorIntersection(1))
                    {
                        ((NodedSegmentString)ss1).AddIntersectionNode(intPt, segIndex1);
                    }
                }
            }

            private void AddTouch(ISegmentString ss)
            {
                int holeIndex = (int)((NodedSegmentString)ss).Context;
                if (holeIndex >= 0)
                {
                    _isHoleTouching[holeIndex] = true;
                }
            }

            public bool IsDone => false;
        }
    }
}
