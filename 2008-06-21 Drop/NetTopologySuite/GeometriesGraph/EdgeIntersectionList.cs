using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// A list of edge intersections along an <see cref="Edge{TCoordinate}"/>.
    /// </summary>
    public class EdgeIntersectionList<TCoordinate> : IEnumerable<EdgeIntersection<TCoordinate>>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, 
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        // a map of EdgeIntersections      
        private readonly SortedDictionary<EdgeIntersection<TCoordinate>, EdgeIntersection<TCoordinate>> _nodeMap
            = new SortedDictionary<EdgeIntersection<TCoordinate>, EdgeIntersection<TCoordinate>>();

        private readonly IGeometryFactory<TCoordinate> _geoFactory;
        private readonly Edge<TCoordinate> _edge; // the parent edge

        /// <summary>
        /// Creates a new <see cref="EdgeIntersectionList{TCoordinate}"/>.
        /// </summary>
        /// <param name="geoFactory">
        /// An <see cref="IGeometryFactory{TCoordinate}"/> instance.
        /// </param>
        /// <param name="edge">
        /// The containing <see cref="Edge{TCoordinate}"/>.
        /// </param>
        public EdgeIntersectionList(IGeometryFactory<TCoordinate> geoFactory, 
                                    Edge<TCoordinate> edge)
        {
            if (geoFactory == null) throw new ArgumentNullException("geoFactory");
            if (edge == null) throw new ArgumentNullException("edge");

            _geoFactory = geoFactory;
            _edge = edge;
        }

        public override String ToString()
        {
            StringBuilder buffer = new StringBuilder(32);

            buffer.Append("Intersections: ");

            foreach (EdgeIntersection<TCoordinate> intersection in this)
            {
                buffer.Append(intersection);
            }

            return buffer.ToString();
        }

        /// <summary>
        /// Searches all of the contained <see cref="EdgeIntersection{TCoordinate}"/>
        /// entries to find if an itersection exists at <paramref name="point"/>.
        /// </summary>
        /// <param name="point">The coordinate to search for an intersection at.</param>
        /// <returns>
        /// <see langword="true"/> if the <see cref="EdgeIntersectionList{TCoordinate}"/>
        /// contains an <see cref="EdgeIntersection{TCoordinate}"/> at 
        /// <paramref name="point"/>.
        /// </returns>
        public Boolean IsIntersection(TCoordinate point)
        {
            // TODO: consider a map or tree to speed this up
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
            Add(_edge.Coordinates.First, 0, 0.0);
            Add(_edge.Coordinates.Last, _edge.Coordinates.LastIndex, 0.0);
        }

        /// <summary> 
        /// Generates and returns new edges for all the 
        /// edges that the intersections in this list split the parent edge into.
        /// </summary>
        public IEnumerable<Edge<TCoordinate>> GetSplitEdges()
        {
            // ensure that the list has entries for the first and last 
            // point of the edge
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
        public Edge<TCoordinate> CreateSplitEdge(EdgeIntersection<TCoordinate> ei0, 
                                                 EdgeIntersection<TCoordinate> ei1)
        {
            ICoordinateSequence<TCoordinate> pts;

            if (ei0.SegmentIndex == ei1.SegmentIndex)
            {
                ICoordinateSequenceFactory<TCoordinate> seqFactory 
                    = _edge.Coordinates.CoordinateSequenceFactory;

                pts = seqFactory.Create(ei0.Coordinate, ei1.Coordinate);
            }
            else
            {
                // if the last intersection point is not equal to the its segment start pt,
                // add it to the points list as well.
                // (This check is needed because the distance metric is not totally reliable!)
                // The check for point equality is 2D only - Z values are ignored
                // TODO: 3D unsafe
                TCoordinate lastSegStartPt = _edge.Coordinates[ei1.SegmentIndex];
                Boolean useIntersectionPt1 = ei1.Distance > 0.0 || 
                                             !ei1.Coordinate.Equals(lastSegStartPt);

                pts = useIntersectionPt1
                            ? _edge.Coordinates.Splice(ei0.Coordinate, 
                                                       ei0.SegmentIndex + 1, 
                                                       ei1.SegmentIndex,
                                                       ei1.Coordinate)
                            : _edge.Coordinates.Splice(ei0.Coordinate, 
                                                       ei0.SegmentIndex + 1, 
                                                       ei1.SegmentIndex);
            }

            return new Edge<TCoordinate>(_geoFactory, pts, _edge.Label.Value);
        }

        /// <summary> 
        /// Adds an intersection into the list, if it isn't already there.
        /// The input segmentIndex and dist are expected to be normalized.
        /// </summary>
        /// <returns>
        /// The <see cref="EdgeIntersection{TCoordinate}"/> found or added.
        /// </returns>
        public EdgeIntersection<TCoordinate> Add(TCoordinate intersection, 
                                                 Int32 segmentIndex, 
                                                 Double dist)
        {
            EdgeIntersection<TCoordinate> eiNew 
                = new EdgeIntersection<TCoordinate>(intersection, segmentIndex, dist);
            EdgeIntersection<TCoordinate> ei;

            if (_nodeMap.TryGetValue(eiNew, out ei))
            {
                return ei;
            }

            _nodeMap.Add(eiNew, eiNew);
            return eiNew;
        }

        /// <summary>
        /// Clears the <see cref="EdgeIntersectionList{TCoordinate}"/> of all
        /// <see cref="EdgeIntersection{TCoordinate}"/>s.
        /// </summary>
        public void Clear()
        {
            _nodeMap.Clear();
        }

        /// <summary>
        /// Gets the number of <see cref="EdgeIntersection{TCoordinate}"/>s
        /// in the list.
        /// </summary>
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