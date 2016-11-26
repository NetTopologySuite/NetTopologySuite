using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;

[assembly: InternalsVisibleTo("NetTopologySuite.Tests.NUnit, PublicKey=" +
    "0024000004800000940000000602000000240000525341310004000001000100e5a9697e3d378d"+
    "e4bdd1607b9a6ea7884823d3909f8de55b573416d9adb0ae25eebc39007d71a7228c500d6e846d"+
    "54dcc2cd839056c38c0a5e86b73096d90504f753ea67c9b5e61ecfdb8edf0f1dfaf0455e9a0f9e"+
    "124e16777baefcda2af9a5a9e48f0c3502891c79444dc2d75aa50b75d148e16f1401dcb18bc163"+
    "8cc764a9")]

namespace NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// Locates a subgraph inside a set of subgraphs,
    /// in order to determine the outside depth of the subgraph.
    /// The input subgraphs are assumed to have had depths
    /// already calculated for their edges.
    /// </summary>
    internal class SubgraphDepthLocater
    {
        private readonly IList<BufferSubgraph> _subgraphs;
        private readonly LineSegment _seg = new LineSegment();

        /// <summary>
        ///
        /// </summary>
        /// <param name="subgraphs"></param>
        public SubgraphDepthLocater(IList<BufferSubgraph> subgraphs)
        {
            _subgraphs = subgraphs;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public int GetDepth(Coordinate p)
        {
            //ArrayList stabbedSegments = new ArrayList(FindStabbedSegments(p).CastPlatform());
            var stabbedSegments = new List<DepthSegment>(FindStabbedSegments(p));
            // if no segments on stabbing line subgraph must be outside all others.
            if (stabbedSegments.Count == 0)
                return 0;
            stabbedSegments.Sort();
            var ds = stabbedSegments[0];
            return ds.LeftDepth;
        }

        /// <summary>
        /// Finds all non-horizontal segments intersecting the stabbing line.
        /// The stabbing line is the ray to the right of stabbingRayLeftPt.
        /// </summary>
        /// <param name="stabbingRayLeftPt">The left-hand origin of the stabbing line.</param>
        /// <returns>A List of {DepthSegments} intersecting the stabbing line.</returns>
        private IList<DepthSegment> FindStabbedSegments(Coordinate stabbingRayLeftPt)
        {
            IList<DepthSegment> stabbedSegments = new List<DepthSegment>();
            foreach (var bsg in _subgraphs)
            {
                FindStabbedSegments(stabbingRayLeftPt, bsg.DirectedEdges, stabbedSegments);
            }
            return stabbedSegments;
        }

        /// <summary>
        /// Finds all non-horizontal segments intersecting the stabbing line
        /// in the list of dirEdges.
        /// The stabbing line is the ray to the right of stabbingRayLeftPt.
        /// </summary>
        /// <param name="stabbingRayLeftPt">The left-hand origin of the stabbing line.</param>
        /// <param name="dirEdges"></param>
        /// <param name="stabbedSegments">The current list of DepthSegments intersecting the stabbing line.</param>
        private void FindStabbedSegments(Coordinate stabbingRayLeftPt, IEnumerable<DirectedEdge> dirEdges, IList<DepthSegment> stabbedSegments)
        {
            /*
            * Check all forward DirectedEdges only.  This is still general,
            * because each Edge has a forward DirectedEdge.
            */
            foreach (DirectedEdge de in dirEdges)
            {
                if (!de.IsForward)
                    continue;
                FindStabbedSegments(stabbingRayLeftPt, de, stabbedSegments);
            }
        }

        /// <summary>
        /// Finds all non-horizontal segments intersecting the stabbing line
        /// in the input dirEdge.
        /// The stabbing line is the ray to the right of stabbingRayLeftPt.
        /// </summary>
        /// <param name="stabbingRayLeftPt">The left-hand origin of the stabbing line.</param>
        /// <param name="dirEdge"></param>
        /// <param name="stabbedSegments">The current list of DepthSegments intersecting the stabbing line.</param>
        private void FindStabbedSegments(Coordinate stabbingRayLeftPt, DirectedEdge dirEdge, IList<DepthSegment> stabbedSegments)
        {
            Coordinate[] pts = dirEdge.Edge.Coordinates;
            for (int i = 0; i < pts.Length - 1; i++)
            {
                _seg.P0 = pts[i];
                _seg.P1 = pts[i + 1];
                // ensure segment always points upwards
                if (_seg.P0.Y > _seg.P1.Y)
                    _seg.Reverse();

                // skip segment if it is left of the stabbing line
                var maxx = Math.Max(_seg.P0.X, _seg.P1.X);
                if (maxx < stabbingRayLeftPt.X) continue;

                // skip horizontal segments (there will be a non-horizontal one carrying the same depth info
                if (_seg.IsHorizontal) continue;

                // skip if segment is above or below stabbing line
                if (stabbingRayLeftPt.Y < _seg.P0.Y || stabbingRayLeftPt.Y > _seg.P1.Y) continue;

                // skip if stabbing ray is right of the segment
                if (CGAlgorithms.ComputeOrientation(_seg.P0, _seg.P1, stabbingRayLeftPt) == CGAlgorithms.Right) continue;

                // stabbing line cuts this segment, so record it
                int depth = dirEdge.GetDepth(Positions.Left);
                // if segment direction was flipped, use RHS depth instead
                if (!_seg.P0.Equals(pts[i]))
                    depth = dirEdge.GetDepth(Positions.Right);
                var ds = new DepthSegment(_seg, depth);
                stabbedSegments.Add(ds);
            }
        }

        /// <summary>
        /// A segment from a directed edge which has been assigned a depth value
        /// for its sides.
        /// </summary>
        internal class DepthSegment : IComparable<DepthSegment>
        {
            private readonly LineSegment _upwardSeg;

            /// <summary>
            ///
            /// </summary>
            public int LeftDepth { get; set; }

            /// <summary>
            ///
            /// </summary>
            /// <param name="seg"></param>
            /// <param name="depth"></param>
            public DepthSegment(LineSegment seg, int depth)
            {
                // input seg is assumed to be normalized
                _upwardSeg = new LineSegment(seg);
                this.LeftDepth = depth;
            }

            /// <summary>
            /// Defines a comparison operation on DepthSegments
            /// which orders them left to right.
            /// Assumes the segments are normalized.
            /// <para/>
            /// The definition of ordering is:
            /// <list type="Bullet">
            /// <item>-1 : if DS1.seg is left of or below DS2.seg (DS1 &lt; DS2).</item>
            /// <item>1 : if DS1.seg is right of or above DS2.seg (DS1 &gt; DS2).</item>
            /// <item>0 : if the segments are identical</item>
            /// </list>
            /// </summary>
            /// <remarks>
            /// Known Bugs:
            /// <list type="Bullet">
            /// <item>The logic does not obey the <see cref="IComparable.CompareTo"/> contract. 
            /// This is acceptable for the intended usage, but may cause problems if used with some
            /// utilities in the .Net standard library (e.g. <see cref="T:System.Collections.List.Sort()"/>.</item>
            /// </list>
            /// </remarks>
            /// <param name="other">A DepthSegment</param>
            /// <returns>The comparison value</returns>
            public int CompareTo(DepthSegment other)
            {
                // fast check if segments are trivially ordered along X
                if (_upwardSeg.MinX >= other._upwardSeg.MaxX) return 1;
                if (_upwardSeg.MaxX <= other._upwardSeg.MinX) return -1;

                /*
                * try and compute a determinate orientation for the segments.
                * Test returns 1 if other is left of this (i.e. this > other)
                */
                var orientIndex = _upwardSeg.OrientationIndex(other._upwardSeg);
                if (orientIndex != 0) return orientIndex;

                /*
                * If comparison between this and other is indeterminate,
                * try the opposite call order.
                * The sign of the result needs to be flipped
                */
                orientIndex = -1 * other._upwardSeg.OrientationIndex(_upwardSeg);
                if (orientIndex != 0) return orientIndex;

                // otherwise, use standard lexicographic segment ordering
                return _upwardSeg.CompareTo(other._upwardSeg);
            }

            /// <summary>
            /// Compare two collinear segments for left-most ordering.
            /// If segs are vertical, use vertical ordering for comparison.
            /// If segs are equal, return 0.
            /// Segments are assumed to be directed so that the second coordinate is >= to the first
            /// (e.g. up and to the right).
            /// </summary>
            /// <param name="seg0">A segment to compare.</param>
            /// <param name="seg1">A segment to compare.</param>
            /// <returns></returns>
            private static int CompareX(LineSegment seg0, LineSegment seg1)
            {
                var compare0 = seg0.P0.CompareTo(seg1.P0);
                if (compare0 != 0) return compare0;
                return seg0.P1.CompareTo(seg1.P1);
            }

            public override string ToString()
            {
                return _upwardSeg.ToString();
            }
        }
    }
}