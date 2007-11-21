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
    public class EdgeIntersectionList<TCoordinate> : IList<EdgeIntersection<TCoordinate>>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
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
            Int32 maxSegIndex = _edge.Points.Count - 1;
            Add(_edge.Points[0], 0, 0.0);
            Add(_edge.Points[maxSegIndex], maxSegIndex, 0.0);
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
            ICoordinate lastSegStartPt = _edge.Points[ei1.SegmentIndex];

            // if the last intersection point is not equal to the its segment start pt,
            // add it to the points list as well.
            // (This check is needed because the distance metric is not totally reliable!)
            // The check for point equality is 2D only - Z values are ignored
            Boolean useIntPt1 = ei1.Distance > 0.0 || ! ei1.Coordinate.Equals(lastSegStartPt);

            if (! useIntPt1)
            {
                npts--;
            }

            TCoordinate[] pts = new TCoordinate[npts];
            Int32 ipt = 0;
            pts[ipt++] = new TCoordinate(ei0.Coordinate);

            for (Int32 i = ei0.SegmentIndex + 1; i <= ei1.SegmentIndex; i++)
            {
                pts[ipt++] = _edge.Points[i];
            }

            if (useIntPt1)
            {
                pts[ipt] = ei1.Coordinate;
            }

            return new Edge<TCoordinate>(pts, new Label(_edge.Label));
        }

        public void Write(StreamWriter outstream)
        {
            outstream.WriteLine("Intersections:");

            foreach (EdgeIntersection<TCoordinate> intersection in this)
            {
                intersection.Write(outstream);
            }
        }

        #region IList<EdgeIntersection<TCoordinate>> Members

        public int IndexOf(EdgeIntersection<TCoordinate> item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, EdgeIntersection<TCoordinate> item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public EdgeIntersection<TCoordinate> this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection<EdgeIntersection<TCoordinate>> Members

        public void Add(EdgeIntersection<TCoordinate> item)
        {
            throw new NotImplementedException();
        }

        /// <summary> 
        /// Adds an intersection into the list, if it isn't already there.
        /// The input segmentIndex and dist are expected to be normalized.
        /// </summary>
        /// <returns>The EdgeIntersection found or added.</returns>
        public EdgeIntersection<TCoordinate> Add(TCoordinate intersection, Int32 segmentIndex, Double dist)
        {
            EdgeIntersection<TCoordinate> eiNew = new EdgeIntersection<TCoordinate>(intersection, segmentIndex, dist);
            EdgeIntersection<TCoordinate> ei = _nodeMap[eiNew];

            if (ei != null)
            {
                return ei;
            }

            _nodeMap.Add(eiNew, eiNew);
            return eiNew;
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(EdgeIntersection<TCoordinate> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(EdgeIntersection<TCoordinate>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public Int32 Count
        {
            get { return _nodeMap.Count; }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(EdgeIntersection<TCoordinate> item)
        {
            throw new NotImplementedException();
        }

        #endregion

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