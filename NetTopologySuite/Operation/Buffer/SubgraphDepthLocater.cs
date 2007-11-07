using System;
using System.Collections;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;

namespace GisSharpBlog.NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// Locates a subgraph inside a set of subgraphs,
    /// in order to determine the outside depth of the subgraph.
    /// The input subgraphs are assumed to have had depths
    /// already calculated for their edges.
    /// </summary>
    public class SubgraphDepthLocater
    {
        private IList subgraphs;
        private LineSegment seg = new LineSegment();

        public SubgraphDepthLocater(IList subgraphs)
        {
            this.subgraphs = subgraphs;
        }

        public Int32 GetDepth(ICoordinate p)
        {
            ArrayList stabbedSegments = new ArrayList(FindStabbedSegments(p));
            // if no segments on stabbing line subgraph must be outside all others.
            if (stabbedSegments.Count == 0)
            {
                return 0;
            }
            stabbedSegments.Sort();
            DepthSegment ds = (DepthSegment) stabbedSegments[0];
            return ds.LeftDepth;
        }

        /// <summary>
        /// Finds all non-horizontal segments intersecting the stabbing line.
        /// The stabbing line is the ray to the right of stabbingRayLeftPt.
        /// </summary>
        /// <param name="stabbingRayLeftPt">The left-hand origin of the stabbing line.</param>
        /// <returns>A List of {DepthSegments} intersecting the stabbing line.</returns>
        private IList FindStabbedSegments(ICoordinate stabbingRayLeftPt)
        {
            IList stabbedSegments = new ArrayList();
            IEnumerator i = subgraphs.GetEnumerator();
            while (i.MoveNext())
            {
                BufferSubgraph bsg = (BufferSubgraph) i.Current;
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
        /// <param name="stabbedSegments">The current list of DepthSegments intersecting the stabbing line.</param>
        private void FindStabbedSegments(ICoordinate stabbingRayLeftPt, IList dirEdges, IList stabbedSegments)
        {
            /*
            * Check all forward DirectedEdges only.  This is still general,
            * because each Edge has a forward DirectedEdge.
            */
            IEnumerator i = dirEdges.GetEnumerator();
            while (i.MoveNext())
            {
                DirectedEdge de = (DirectedEdge) i.Current;
                if (! de.IsForward)
                {
                    continue;
                }
                FindStabbedSegments(stabbingRayLeftPt, de, stabbedSegments);
            }
        }

        /// <summary>
        /// Finds all non-horizontal segments intersecting the stabbing line
        /// in the input dirEdge.
        /// The stabbing line is the ray to the right of stabbingRayLeftPt.
        /// </summary>
        /// <param name="stabbingRayLeftPt">The left-hand origin of the stabbing line.</param>
        /// <param name="stabbedSegments">The current list of DepthSegments intersecting the stabbing line.</param>
        private void FindStabbedSegments(ICoordinate stabbingRayLeftPt, DirectedEdge dirEdge, IList stabbedSegments)
        {
            ICoordinate[] pts = dirEdge.Edge.Coordinates;
            for (Int32 i = 0; i < pts.Length - 1; i++)
            {
                seg.P0 = pts[i];
                seg.P1 = pts[i + 1];
                // ensure segment always points upwards
                if (seg.P0.Y > seg.P1.Y)
                {
                    seg.Reverse();
                }

                // skip segment if it is left of the stabbing line
                Double maxx = Math.Max(seg.P0.X, seg.P1.X);
                if (maxx < stabbingRayLeftPt.X)
                {
                    continue;
                }

                // skip horizontal segments (there will be a non-horizontal one carrying the same depth info
                if (seg.IsHorizontal)
                {
                    continue;
                }

                // skip if segment is above or below stabbing line
                if (stabbingRayLeftPt.Y < seg.P0.Y || stabbingRayLeftPt.Y > seg.P1.Y)
                {
                    continue;
                }

                // skip if stabbing ray is right of the segment
                if (CGAlgorithms.ComputeOrientation(seg.P0, seg.P1, stabbingRayLeftPt) == CGAlgorithms.Right)
                {
                    continue;
                }

                // stabbing line cuts this segment, so record it
                Int32 depth = dirEdge.GetDepth(Positions.Left);
                // if segment direction was flipped, use RHS depth instead
                if (! seg.P0.Equals(pts[i]))
                {
                    depth = dirEdge.GetDepth(Positions.Right);
                }
                DepthSegment ds = new DepthSegment(seg, depth);
                stabbedSegments.Add(ds);
            }
        }

        /// <summary>
        /// A segment from a directed edge which has been assigned a depth value
        /// for its sides.
        /// </summary>
        private class DepthSegment : IComparable
        {
            private LineSegment upwardSeg;
            private Int32 leftDepth;

            public Int32 LeftDepth
            {
                get { return leftDepth; }
                set { leftDepth = value; }
            }

            public DepthSegment(LineSegment seg, Int32 depth)
            {
                // input seg is assumed to be normalized
                upwardSeg = new LineSegment(seg);
                leftDepth = depth;
            }

            /// <summary>
            /// Defines a comparision operation on DepthSegments
            /// which orders them left to right:
            /// DS1 smaller DS2   if   DS1.seg is left of DS2.seg.
            /// DS1 bigger  DS2   if   DS1.seg is right of DS2.seg.
            /// </summary>
            public Int32 CompareTo(Object obj)
            {
                DepthSegment other = (DepthSegment) obj;

                /*
                * try and compute a determinate orientation for the segments.
                * Test returns 1 if other is left of this (i.e. this > other)
                */
                Int32 orientIndex = upwardSeg.OrientationIndex(other.upwardSeg);

                /*
                * If comparison between this and other is indeterminate,
                * try the opposite call order.
                * orientationIndex value is 1 if this is left of other,
                * so have to flip sign to get proper comparison value of
                * -1 if this is leftmost
                */
                if (orientIndex == 0)
                {
                    orientIndex = -1*other.upwardSeg.OrientationIndex(upwardSeg);
                }

                // if orientation is determinate, return it
                if (orientIndex != 0)
                {
                    return orientIndex;
                }

                // otherwise, segs must be collinear - sort based on minimum X value
                return CompareX(upwardSeg, other.upwardSeg);
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
            private Int32 CompareX(LineSegment seg0, LineSegment seg1)
            {
                Int32 compare0 = seg0.P0.CompareTo(seg1.P0);
                if (compare0 != 0)
                {
                    return compare0;
                }
                return seg0.P1.CompareTo(seg1.P1);
            }
        }
    }
}