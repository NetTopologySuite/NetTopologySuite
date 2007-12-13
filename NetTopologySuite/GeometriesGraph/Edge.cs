using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph.Index;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    public class Edge<TCoordinate> : GraphComponent<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        /// <summary> 
        /// Updates an <see cref="IntersectionMatrix"/> from the label for an edge.
        /// Handles edges from both L and A geometries.
        /// </summary>
        public static void UpdateIntersectionMatrix(Label label, IntersectionMatrix im)
        {
            im.SetAtLeastIfValid(label[0, Positions.On], label[1, Positions.On],
                                 Dimensions.Curve);

            if (label.IsArea())
            {
                im.SetAtLeastIfValid(label[0, Positions.Left], label[1, Positions.Left],
                                     Dimensions.Surface);

                im.SetAtLeastIfValid(label[0, Positions.Right], label[1, Positions.Right],
                                     Dimensions.Surface);
            }
        }

        private readonly ICoordinateSequence<TCoordinate> _coordinates 
            = CoordinateSequences.CreateEmpty<TCoordinate>();
        private readonly EdgeIntersectionList<TCoordinate> _edgeIntersectionList = null;
        private IExtents<TCoordinate> _extents;
        private string _name;
        private MonotoneChainEdge<TCoordinate> _monotoneChainEdge;
        private Boolean _isIsolated = true;
        private readonly Depth _depth = new Depth();
        private Int32 _depthDelta = 0; // the change in area depth from the R to Curve side of this edge

        public Edge(IEnumerable<TCoordinate> coordinates, Label? label)
        {
            _edgeIntersectionList = new EdgeIntersectionList<TCoordinate>(this);
            _coordinates.AddRange(coordinates);
            Label = label;
        }

        public Edge(IEnumerable<TCoordinate> coordinates) : this(coordinates, null) { }

        //public IList<TCoordinate> Points
        //{
        //    get { return _coordinates; }
        //    set
        //    {
        //        _coordinates.Clear();
        //        _coordinates.AddRange(value);
        //    }
        //}

        public Int32 PointCount
        {
            get { return _coordinates.Count; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public ICoordinateSequence<TCoordinate> Coordinates
        {
            get
            {
                return _coordinates;
            }
        }

        public ICoordinateSequence<TCoordinate> CoordinatesReversed
        {
            get
            {
                return _coordinates.Reversed;
            }
        }

        //public TCoordinate GetCoordinate(Int32 i)
        //{
        //    return Points[i];
        //}

        public override TCoordinate Coordinate
        {
            get
            {
                if (Points.Count > 0)
                {
                    return Points[0];
                }

                return default(TCoordinate);
            }
        }

        public IExtents<TCoordinate> Extents
        {
            get
            {
                // compute envelope lazily
                if (_extents == null)
                {
                    _extents = new Extents<TCoordinate>();
                    _extents.ExpandToInclude(_coordinates);
                }

                return _extents;
            }
        }

        public Depth Depth
        {
            get { return _depth; }
        }

        /// <summary>
        /// The <see cref="DepthDelta"/> is the change in depth as an edge is 
        /// crossed from R to L.
        /// </summary>
        /// <returns>The change in depth as the edge is crossed from R to L.</returns>
        public Int32 DepthDelta
        {
            get { return _depthDelta; }
            set { _depthDelta = value; }
        }

        public Int32 MaximumSegmentIndex
        {
            get { return Points.Count - 1; }
        }

        public EdgeIntersectionList<TCoordinate> EdgeIntersectionList
        {
            get { return _edgeIntersectionList; }
        }

        public MonotoneChainEdge<TCoordinate> MonotoneChainEdge
        {
            get
            {
                if (_monotoneChainEdge == null)
                {
                    _monotoneChainEdge = new MonotoneChainEdge<TCoordinate>(this);
                }

                return _monotoneChainEdge;
            }
        }

        public Boolean IsClosed
        {
            get { return Points[0].Equals(Points[Points.Count - 1]); }
        }

        /// <summary> 
        /// An Edge is collapsed if it is an Area edge and it consists of
        /// two segments which are equal and opposite (eg a zero-width V).
        /// </summary>
        public Boolean IsCollapsed
        {
            get
            {
                if (!Label.Value.IsArea())
                {
                    return false;
                }

                if (Points.Count != 3)
                {
                    return false;
                }

                if (Points[0].Equals(Points[2]))
                {
                    return true;
                }

                return false;
            }
        }

        public Edge<TCoordinate> CollapsedEdge
        {
            get
            {
                TCoordinate[] newPts = new TCoordinate[2];
                newPts[0] = Points[0];
                newPts[1] = Points[1];
                Label lineLabel = GeometriesGraph.Label.ToLineLabel(Label.Value);
                Edge<TCoordinate> newEdge = new Edge<TCoordinate>(newPts, lineLabel);
                return newEdge;
            }
        }

        public Boolean Isolated
        {
            get { return _isIsolated; }
            set { _isIsolated = value; }
        }

        public override Boolean IsIsolated
        {
            get { return _isIsolated; }
        }

        /// <summary>
        /// Adds EdgeIntersections for one or both
        /// intersections found for a segment of an edge to the edge intersection list.
        /// </summary>
        public void AddIntersections(LineIntersector<TCoordinate> li, Int32 segmentIndex, Int32 geometryIndex)
        {
            for (Int32 i = 0; i < (Int32)li.IntersectionType; i++)
            {
                AddIntersection(li, segmentIndex, geometryIndex, i);
            }
        }

        /// <summary>
        /// Add an EdgeIntersection for intersection intIndex.
        /// An intersection that falls exactly on a vertex of the edge is normalized
        /// to use the higher of the two possible segmentIndexes.
        /// </summary>
        public void AddIntersection(LineIntersector<TCoordinate> intersector, 
            Int32 segmentIndex, Int32 geometryIndex, Int32 intersectionIndex)
        {
            TCoordinate intersection = new TCoordinate(intersector.GetIntersection(intersectionIndex));
            Int32 normalizedSegmentIndex = segmentIndex;
            Double dist = intersector.GetEdgeDistance(geometryIndex, intersectionIndex);

            // normalize the intersection point location
            Int32 nextSegIndex = normalizedSegmentIndex + 1;

            if (nextSegIndex < Points.Count)
            {
                TCoordinate nextPt = Points[nextSegIndex];

                // Normalize segment index if intPt falls on vertex
                // The check for point equality is 2D only - Z values are ignored
                if (intersection.Equals(nextPt))
                {
                    normalizedSegmentIndex = nextSegIndex;
                    dist = 0.0;
                }

                // Add the intersection point to edge intersection list.                
                EdgeIntersectionList.Add(intersection, normalizedSegmentIndex, dist);
            }
        }

        /// <summary>
        /// Update the IM with the contribution for this component.
        /// A component only contributes if it has a labeling for both parent geometries.
        /// </summary>
        public override void ComputeIntersectionMatrix(IntersectionMatrix im)
        {
            UpdateIntersectionMatrix(Label.Value, im);
        }

        /// <summary>
        /// Equals is defined to be:
        /// e1 equals e2
        /// iff
        /// the coordinates of e1 are the same or the reverse of the coordinates in e2.
        /// </summary>
        public override Boolean Equals(object o)
        {
            Edge<TCoordinate> other = o as Edge<TCoordinate>;

            if (other == null)
            {
                return false;
            }

            return Equals(other as Edge<TCoordinate>);
        }

        /// <summary>
        /// Equals is defined to be:
        /// e1 equals e2
        /// iff
        /// the coordinates of e1 are the same or the reverse of the coordinates in e2.
        /// </summary>
        protected Boolean Equals(Edge<TCoordinate> e)
        {
            if (Points.Count != e.Points.Count)
            {
                return false;
            }

            Boolean isEqualForward = true;
            Boolean isEqualReverse = true;
            Int32 iRev = Points.Count;

            for (Int32 i = 0; i < Points.Count; i++)
            {
                if (!Points[i].Equals(e.Points[i]))
                {
                    isEqualForward = false;
                }

                if (!Points[i].Equals(e.Points[--iRev]))
                {
                    isEqualReverse = false;
                }

                if (!isEqualForward && !isEqualReverse)
                {
                    return false;
                }
            }

            return true;
        }

        public static Boolean operator ==(Edge<TCoordinate> obj1, Edge<TCoordinate> obj2)
        {
            return Equals(obj1, obj2);
        }

        public static Boolean operator !=(Edge<TCoordinate> obj1, Edge<TCoordinate> obj2)
        {
            return !(obj1 == obj2);
        }

        /// <returns> 
        /// <see langword="true"/> if the coordinate sequences of the Edges are identical.
        /// </returns>
        public Boolean IsPointwiseEqual(Edge<TCoordinate> e)
        {
            if (Points.Count != e.Points.Count)
            {
                return false;
            }

            for (Int32 i = 0; i < Points.Count; i++)
            {
                if (! Points[i].Equals(e.Points[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public void Write(StreamWriter outstream)
        {
            outstream.Write("edge " + _name + ": ");
            outstream.Write("LINESTRING (");

            for (Int32 i = 0; i < Points.Count; i++)
            {
                if (i > 0)
                {
                    outstream.Write(",");
                }
                outstream.Write(Points[i][Ordinates.X] + " " + Points[i][Ordinates.Y]);
            }

            outstream.Write(")  " + Label + " " + _depthDelta);
        }

        public void WriteReverse(StreamWriter outstream)
        {
            outstream.Write("edge " + _name + ": ");

            for (Int32 i = Points.Count - 1; i >= 0; i--)
            {
                outstream.Write(Points[i] + " ");
            }

            outstream.WriteLine(String.Empty);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("edge " + _name + ": ");
            sb.Append("LINESTRING (");

            for (Int32 i = 0; i < Points.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(",");
                }

                sb.Append(Points[i][Ordinates.X] + " " + Points[i][Ordinates.Y]);
            }

            sb.Append(")  " + Label + " " + _depthDelta);
            return sb.ToString();
        }
    }
}