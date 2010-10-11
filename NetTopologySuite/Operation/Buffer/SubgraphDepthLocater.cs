using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;

namespace NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// Locates a subgraph inside a set of subgraphs,
    /// in order to determine the outside depth of the subgraph.
    /// The input subgraphs are assumed to have had depths
    /// already calculated for their edges.
    /// </summary>
    public class SubgraphDepthLocater<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly IEnumerable<BufferSubgraph<TCoordinate>> _subgraphs;
        private LineSegment<TCoordinate> _segment;

        public SubgraphDepthLocater(IEnumerable<BufferSubgraph<TCoordinate>> subgraphs)
        {
            _subgraphs = subgraphs;
        }

        public Int32 GetDepth(TCoordinate p)
        {
            List<DepthSegment> stabbedSegments = new List<DepthSegment>(findStabbedSegments(p));

            // if no segments on stabbing line subgraph must be outside all others.
            if (stabbedSegments.Count == 0)
            {
                return 0;
            }

            stabbedSegments.Sort();
            DepthSegment ds = stabbedSegments[0];
            return ds.LeftDepth;
        }

        /// <summary>
        /// Finds all non-horizontal segments intersecting the stabbing line.
        /// The stabbing line is the ray to the right of stabbingRayLeftPt.
        /// </summary>
        /// <param name="stabbingRayLeftPt">The left-hand origin of the stabbing line.</param>
        /// <returns>A set of <see cref="DepthSegment"/>s intersecting the stabbing line.</returns>
        private IEnumerable<DepthSegment> findStabbedSegments(TCoordinate stabbingRayLeftPt)
        {
            List<DepthSegment> retval = new List<DepthSegment>();
            foreach (BufferSubgraph<TCoordinate> subgraph in _subgraphs)
            {
                retval.AddRange(findStabbedSegments(stabbingRayLeftPt, subgraph.DirectedEdges));
                //IEnumerable<DepthSegment> segments = findStabbedSegments(stabbingRayLeftPt, subgraph.DirectedEdges);

                //foreach (DepthSegment segment in segments)
                //{
                //    yield return segment;
                //}
            }
            return retval;
        }

        /// <summary>
        /// Finds all non-horizontal segments intersecting the stabbing line
        /// in the list of dirEdges.
        /// The stabbing line is the ray to the right of stabbingRayLeftPt.
        /// </summary>
        /// <param name="stabbingRayLeftPt">The left-hand origin of the stabbing line.</param>
        /// <returns>
        /// A set of <see cref="DepthSegment"/>s intersecting the stabbing line.
        /// </returns>
        private IEnumerable<DepthSegment> findStabbedSegments(TCoordinate stabbingRayLeftPt,
                                                              IEnumerable<DirectedEdge<TCoordinate>> dirEdges)
        {
            /*
            * Check all forward DirectedEdges only.  This is still general,
            * because each Edge has a forward DirectedEdge.
            */
            foreach (DirectedEdge<TCoordinate> de in dirEdges)
            {
                if (!de.IsForward)
                {
                    continue;
                }

                IEnumerable<DepthSegment> stabbedSegments = findStabbedSegments(stabbingRayLeftPt, de);

                foreach (DepthSegment segment in stabbedSegments)
                {
                    yield return segment;
                }
            }
        }

        /// <summary>
        /// Finds all non-horizontal segments intersecting the stabbing line
        /// in the input dirEdge.
        /// The stabbing line is the ray to the right of stabbingRayLeftPt.
        /// </summary>
        /// <param name="stabbingRayLeftPt">The left-hand origin of the stabbing line.</param>
        /// <returns>
        /// A set of <see cref="DepthSegment"/>s intersecting the stabbing line.
        /// </returns>
        private IEnumerable<DepthSegment> findStabbedSegments(TCoordinate stabbingRayLeftPt,
                                                              DirectedEdge<TCoordinate> dirEdge)
        {
            IEnumerable<TCoordinate> coordinates = dirEdge.Edge.Coordinates;

            foreach (Pair<TCoordinate> pair in Slice.GetOverlappingPairs(coordinates))
            {
                _segment = new LineSegment<TCoordinate>(pair.First, pair.Second);

                // ensure segment always points upwards
                if (_segment.P0[Ordinates.Y] > _segment.P1[Ordinates.Y])
                {
                    _segment = _segment.Reversed;
                }

                // skip segment if it is left of the stabbing line
                Double maxx = Math.Max(_segment.P0[Ordinates.X], _segment.P1[Ordinates.X]);

                if (maxx < stabbingRayLeftPt[Ordinates.X])
                {
                    continue;
                }

                // skip horizontal segments (there will be a non-horizontal one carrying the same depth info
                if (_segment.IsHorizontal)
                {
                    continue;
                }

                // skip if segment is above or below stabbing line
                if (stabbingRayLeftPt[Ordinates.Y] < _segment.P0[Ordinates.Y]
                    || stabbingRayLeftPt[Ordinates.Y] > _segment.P1[Ordinates.Y])
                {
                    continue;
                }

                // skip if stabbing ray is right of the segment
                Orientation rayOrientation =
                    CGAlgorithms<TCoordinate>.ComputeOrientation(_segment.P0, _segment.P1, stabbingRayLeftPt);

                if (rayOrientation == Orientation.Right)
                {
                    continue;
                }

                // stabbing line cuts this segment, so record it
                Int32 depth = dirEdge.GetDepth(Positions.Left);

                // if segment direction was flipped, use RHS depth instead
                if (!_segment.P0.Equals(pair.First))
                {
                    depth = dirEdge.GetDepth(Positions.Right);
                }

                DepthSegment ds = new DepthSegment(_segment, depth);
                yield return ds;
            }
        }

        #region Nested type: DepthSegment

        /// <summary>
        /// A segment from a directed edge which has been assigned a depth value
        /// for its sides.
        /// </summary>
        private struct DepthSegment : IComparable<DepthSegment>
        {
            private readonly Int32 _leftDepth;
            private readonly LineSegment<TCoordinate> _upwardSeg;

            public DepthSegment(LineSegment<TCoordinate> seg, Int32 depth)
            {
                // input seg is assumed to be normalized
                _upwardSeg = new LineSegment<TCoordinate>(seg);
                _leftDepth = depth;
            }

            public Int32 LeftDepth
            {
                get { return _leftDepth; }
            }

            #region IComparable<SubgraphDepthLocater<TCoordinate>.DepthSegment> Members

            /// <summary>
            /// Defines a comparision operation on DepthSegments
            /// which orders them left to right:
            /// DS1 smaller DS2   if   DS1.seg is left of DS2.seg.
            /// DS1 bigger  DS2   if   DS1.seg is right of DS2.seg.
            /// </summary>
            public Int32 CompareTo(DepthSegment other)
            {
                /*
                * try and compute a determinate orientation for the segments.
                * Test returns 1 if other is left of this (i.e. this > other)
                */
                Int32 orientIndex = _upwardSeg.OrientationIndex(other._upwardSeg);

                /*
                * If comparison between this and other is indeterminate,
                * try the opposite call order.
                * orientationIndex value is 1 if this is left of other,
                * so have to flip sign to get proper comparison value of
                * -1 if this is leftmost
                */
                if (orientIndex == 0)
                {
                    orientIndex = -1*other._upwardSeg.OrientationIndex(_upwardSeg);
                }

                // if orientation is determinate, return it
                if (orientIndex != 0)
                {
                    return orientIndex;
                }

                // otherwise, segs must be collinear - sort based on minimum X value
                return compareXOrdinate(_upwardSeg, other._upwardSeg);
            }

            #endregion

            /// <summary>
            /// Compare two collinear segments for left-most ordering.
            /// If segs are vertical, use vertical ordering for comparison.
            /// If segs are equal, return 0.
            /// Segments are assumed to be directed so that the second coordinate is >= to the first
            /// (e.g. up and to the right).
            /// </summary>
            /// <param name="seg0">A segment to compare.</param>
            /// <param name="seg1">A segment to compare.</param>
            private static Int32 compareXOrdinate(LineSegment<TCoordinate> seg0, LineSegment<TCoordinate> seg1)
            {
                Int32 compare0 = seg0.P0.CompareTo(seg1.P0);

                if (compare0 != 0)
                {
                    return compare0;
                }

                return seg0.P1.CompareTo(seg1.P1);
            }
        }

        #endregion
    }
}