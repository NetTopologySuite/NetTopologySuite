using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// A RightmostEdgeFinder find the DirectedEdge in a list which has the highest coordinate,
    /// and which is oriented L to R at that point. (I.e. the right side is on the RHS of the edge.)
    /// 
    /// The DirectedEdge returned is guaranteed to have the R of the world on its RHS.
    /// </summary>
    public class RightmostEdgeFinder<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private Int32 _minIndex = -1;
        private TCoordinate _minCoord = default(TCoordinate);
        private DirectedEdge<TCoordinate> _minDe = null;
        private DirectedEdge<TCoordinate> _orientedDe = null;

        public DirectedEdge<TCoordinate> Edge
        {
            get { return _orientedDe; }
        }

        public TCoordinate Coordinate
        {
            get { return _minCoord; }
        }

        public void FindEdge(IEnumerable<DirectedEdge<TCoordinate>> edges)
        {
            /*
             * Check all forward DirectedEdges only.  This is still general,
             * because each edge has a forward DirectedEdge.
             */
            foreach (DirectedEdge<TCoordinate> edge in edges)
            {
                if (!edge.IsForward)
                {
                    continue;
                }

                checkForRightmostCoordinate(edge);
            }

            /*
             * If the rightmost point is a node, we need to identify which of
             * the incident edges is rightmost.
             */
            Assert.IsTrue(_minIndex != 0 || _minCoord.Equals(_minDe.Coordinate), "inconsistency in rightmost processing");
           
            if (_minIndex == 0)
            {
                findRightmostEdgeAtNode();
            }
            else
            {
                findRightmostEdgeAtVertex();
            }

            /*
             * now check that the extreme side is the R side.
             * If not, use the sym instead.
             */
            _orientedDe = _minDe;
            Positions rightmostSide = getRightmostSide(_minDe, _minIndex);

            if (rightmostSide == Positions.Left)
            {
                _orientedDe = _minDe.Sym;
            }
        }

        private void findRightmostEdgeAtNode()
        {
            Node<TCoordinate> node = _minDe.Node;
            DirectedEdgeStar<TCoordinate> star = node.Edges as DirectedEdgeStar<TCoordinate>;
            Debug.Assert(star != null);
            _minDe = star.GetRightmostEdge();

            // the DirectedEdge returned by the previous call is not
            // necessarily in the forward direction. Use the sym edge if it isn't.
            if (!_minDe.IsForward)
            {
                _minDe = _minDe.Sym;
                _minIndex = _minDe.Edge.Coordinates.Count - 1;
            }
        }

        private void findRightmostEdgeAtVertex()
        {
            /*
             * The rightmost point is an interior vertex, so it has a segment on either side of it.
             * If these segments are both above or below the rightmost point, we need to
             * determine their relative orientation to decide which is rightmost.
             */
            ICoordinateSequence<TCoordinate> pts = _minDe.Edge.Coordinates;
            Assert.IsTrue(_minIndex > 0 && _minIndex < pts.Count,
                          "rightmost point expected to be interior vertex of edge");
            
            TCoordinate pPrev = pts[_minIndex - 1];
            TCoordinate pNext = pts[_minIndex + 1];

            Orientation orientation = CGAlgorithms<TCoordinate>.ComputeOrientation(_minCoord, pNext, pPrev);

            Boolean usePrev = false;

            // both segments are below min point
            if (pPrev[Ordinates.Y] < _minCoord[Ordinates.Y]
                && pNext[Ordinates.Y] < _minCoord[Ordinates.Y] 
                && orientation == Orientation.CounterClockwise)
            {
                usePrev = true;
            }
            else if (pPrev[Ordinates.Y] > _minCoord[Ordinates.Y]
                && pNext[Ordinates.Y] > _minCoord[Ordinates.Y] 
                && orientation == Orientation.Clockwise)
            {
                usePrev = true;
            }

            // if both segments are on the same side, do nothing - either is safe
            // to select as a rightmost segment
            if (usePrev)
            {
                _minIndex = _minIndex - 1;
            }
        }

        private void checkForRightmostCoordinate(DirectedEdge<TCoordinate> de)
        {
            IEnumerable<TCoordinate> coordinates = de.Edge.Coordinates;

            Int32 i = 0;

            foreach (TCoordinate coordinate in coordinates)
            {
                // only check vertices which are the start or end point of a non-horizontal segment
                // <FIX> MD 19 Sep 03 - NO!  we can test all vertices, since the rightmost 
                //                    - must have a non-horiz segment adjacent to it
                if (Coordinates<TCoordinate>.IsEmpty(_minCoord) 
                    || coordinate[Ordinates.X] > _minCoord[Ordinates.X])
                {
                    _minDe = de;
                    _minIndex = i;
                    _minCoord = coordinate;
                }

                i += 1;
            }
        }

        private Positions getRightmostSide(DirectedEdge<TCoordinate> de, Int32 index)
        {
            Positions side = getRightmostSideOfSegment(de, index);

            if (side < 0)
            {
                side = getRightmostSideOfSegment(de, index - 1);
            }

            if (side < 0)
            {
                // reaching here can indicate that segment is horizontal                
                _minCoord = default(TCoordinate);
                checkForRightmostCoordinate(de);
            }

            return side;
        }

        private static Positions getRightmostSideOfSegment(DirectedEdge<TCoordinate> de, Int32 i)
        {
            Edge<TCoordinate> e = de.Edge;
            IEnumerable<TCoordinate> coordinates = e.Coordinates;

            Pair<TCoordinate>? pair = Slice.GetPairAt(coordinates, i);

            if (pair == null)
            {
                return Positions.Parallel;
            }

            Double y1 = pair.Value.First[Ordinates.Y];
            Double y2 = pair.Value.Second[Ordinates.Y];

            if (y1 == y2)
            {
                return Positions.Parallel;
            }

            Positions pos = Positions.Left;

            if (y1 < y2)
            {
                pos = Positions.Right;
            }

            return pos;
        }
    }
}