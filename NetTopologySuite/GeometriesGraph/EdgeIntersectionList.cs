using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// A list of edge intersections along an Edge.
    /// </summary>
    public class EdgeIntersectionList<TCoordinate> : IEnumerable<EdgeIntersection<TCoordinate>>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        // a list of EdgeIntersections      
        private readonly SortedDictionary<EdgeIntersection<TCoordinate>, EdgeIntersection<TCoordinate>> _nodeMap 
            = new SortedDictionary<EdgeIntersection<TCoordinate>, EdgeIntersection<TCoordinate>>();
        private readonly Edge<TCoordinate> _edge; // the parent edge

        public EdgeIntersectionList(Edge<TCoordinate> edge)
        {
            _edge = edge;
        }

        public Boolean IsIntersection(TCoordinate point)
        {
            foreach (EdgeIntersection<TCoordinate> intersection in this)
            {
                if (intersection.Coordinate.Equals(point))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Adds entries for the first and last points of the edge to the list.
        /// </summary>
        public void AddEndpoints()
        {
            Int32 maxSegIndex = _edge.Coordinates.Count - 1;
            Add(_edge.Coordinates[0], 0, 0.0);
            Add(_edge.Coordinates[maxSegIndex], maxSegIndex, 0.0);
        }

        /// <summary> 
        /// Creates and returns new edges for all the edges that the intersections in this
        /// list split the parent edge into.
        /// </summary>
        public IEnumerable<Edge<TCoordinate>> GetSplitEdges()
        {
            // ensure that the list has entries for the first and last point of the edge
            AddEndpoints();

            IEnumerator<EdgeIntersection<TCoordinate>> it = GetEnumerator();
            it.MoveNext();

            // there should always be at least two entries in the list
            EdgeIntersection<TCoordinate> eiPrev = it.Current;

            while (it.MoveNext())
            {
                EdgeIntersection<TCoordinate> ei = it.Current;
                Edge<TCoordinate> newEdge = CreateSplitEdge(eiPrev, ei);
                yield return newEdge;

                eiPrev = ei;
            }
        }

        /// <summary>
        /// Create a new "split edge" with the section of points between
        /// (and including) the two intersections.
        /// The label for the new edge is the same as the label for the parent edge.
        /// </summary>
        public Edge<TCoordinate> CreateSplitEdge(EdgeIntersection<TCoordinate> ei0, EdgeIntersection<TCoordinate> ei1)
        {
            Int32 npts = ei1.SegmentIndex - ei0.SegmentIndex + 2;
            TCoordinate lastSegStartPt = _edge.Coordinates[ei1.SegmentIndex];

            // if the last intersection point is not equal to the its segment start pt,
            // add it to the points list as well.
            // (This check is needed because the distance metric is not totally reliable!)
            // The check for point equality is 2D only - Z values are ignored
            Boolean useIntPt1 = ei1.Distance > 0.0 || ! ei1.Coordinate.Equals(lastSegStartPt);

            if (! useIntPt1)
            {
                npts--;
            }
            
            ICoordinateSequenceFactory<TCoordinate> factory = _edge.Coordinates.CoordinateSequenceFactory;
            ICoordinateSequence<TCoordinate> pts = factory.Create(npts);
            Int32 ipt = 0;
            pts[ipt++] = factory.CoordinateFactory.Create(ei0.Coordinate);

            for (Int32 i = ei0.SegmentIndex + 1; i <= ei1.SegmentIndex; i++)
            {
                pts[ipt++] = _edge.Coordinates[i];
            }

            if (useIntPt1)
            {
                pts[ipt] = ei1.Coordinate;
            }

            return new Edge<TCoordinate>(pts, _edge.Label.Value);
        }

        public void Write(StreamWriter outstream)
        {
            outstream.WriteLine("Intersections:");

            foreach (EdgeIntersection<TCoordinate> intersection in this)
            {
                intersection.Write(outstream);
            }
        }

        /// <summary> 
        /// Adds an intersection into the list, if it isn't already there.
        /// The input segmentIndex and dist are expected to be normalized.
        /// </summary>
        /// <returns>The EdgeIntersection found or added.</returns>
        public EdgeIntersection<TCoordinate> Add(TCoordinate intersection, Int32 segmentIndex, Double dist)
        {
            EdgeIntersection<TCoordinate> eiNew = new EdgeIntersection<TCoordinate>(intersection, segmentIndex, dist);
            EdgeIntersection<TCoordinate> ei;

            if (_nodeMap.TryGetValue(eiNew, out ei))
            {
                return ei;
            }

            _nodeMap.Add(eiNew, eiNew);
            return eiNew;
        }

        public void Clear()
        {
            _nodeMap.Clear();
        }

        public Int32 Count
        {
            get { return _nodeMap.Count; }
        }

        #region IEnumerable<EdgeIntersection<TCoordinate>> Members
        /// <summary> 
        /// Returns an iterator of EdgeIntersections.
        /// </summary>
        public IEnumerator<EdgeIntersection<TCoordinate>> GetEnumerator()
        {
            return _nodeMap.Values.GetEnumerator();
        }
        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}