using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph.Index;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    public class Edge<TCoordinate> : GraphComponent<TCoordinate>, IBoundable<IExtents<TCoordinate>>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
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

        private readonly ICoordinateSequence<TCoordinate> _coordinates;
        private readonly EdgeIntersectionList<TCoordinate> _edgeIntersectionList = null;
        private IExtents<TCoordinate> _extents;
        private string _name;
        private MonotoneChainEdge<TCoordinate> _monotoneChainEdge;
        private Boolean _isIsolated = true;
        private readonly Depth _depth = new Depth();
        private Int32 _depthDelta = 0; // the change in area depth from the R to Curve side of this edge

        public Edge(ICoordinateSequence<TCoordinate> coordinates, Label? label)
        {
            _edgeIntersectionList = new EdgeIntersectionList<TCoordinate>(this);
            _coordinates = coordinates;
            Label = label;
        }

        public Edge(ICoordinateSequence<TCoordinate> coordinates) 
            : this(coordinates, null) { }

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
                if (Coordinates.Count > 0)
                {
                    return Coordinates[0];
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
            get { return _coordinates.Count - 1; }
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
            get { return Slice.GetFirst(Coordinates).Equals(Slice.GetLast(Coordinates)); }
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

                if (Coordinates.Count != 3)
                {
                    return false;
                }

                if (Coordinates[0].Equals(Coordinates[2]))
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
                ICoordinateSequence<TCoordinate> newPts = Coordinates.Slice(0, 1);
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
        /// Adds <see cref="EdgeIntersection{TCoordinate}"/>s for one or both
        /// intersections found for a segment of an edge to the edge intersection list.
        /// </summary>
        public void AddIntersections(Intersection<TCoordinate> intersection, Int32 segmentIndex, Int32 geometryIndex)
        {
            for (Int32 i = 0; i < (Int32)intersection.IntersectionType; i++)
            {
                AddIntersection(intersection, segmentIndex, geometryIndex, i);
            }
        }

        /// <summary>
        /// Add an <see cref="EdgeIntersection{TCoordinate}"/> for intersection 
        /// <paramref name="intersectionIndex"/>.
        /// An intersection that falls exactly on a vertex of the edge is normalized
        /// to use the higher of the two possible <paramref name="segmentIndex"/>es.
        /// </summary>
        public void AddIntersection(Intersection<TCoordinate> intersection, 
            Int32 segmentIndex, Int32 geometryIndex, Int32 intersectionIndex)
        {
            TCoordinate intersectionPoint = intersection.GetIntersectionPoint(intersectionIndex);
            Int32 normalizedSegmentIndex = segmentIndex;
            Double dist = intersection.GetEdgeDistance(geometryIndex, intersectionIndex);

            // normalize the intersection point location
            Int32 nextSegIndex = normalizedSegmentIndex + 1;

            if (nextSegIndex < Coordinates.Count)
            {
                TCoordinate nextPt = Coordinates[nextSegIndex];

                // Normalize segment index if intPt falls on vertex
                // The check for point equality is 2D only - Z values are ignored
                if (intersection.Equals(nextPt))
                {
                    normalizedSegmentIndex = nextSegIndex;
                    dist = 0.0;
                }

                // Add the intersection point to edge intersection list.                
                EdgeIntersectionList.Add(intersectionPoint, normalizedSegmentIndex, dist);
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
            if (Coordinates.Count != e.Coordinates.Count)
            {
                return false;
            }

            Boolean isEqualForward = true;
            Boolean isEqualReverse = true;
            Int32 iRev = Coordinates.Count;

            for (Int32 i = 0; i < Coordinates.Count; i++)
            {
                if (!Coordinates[i].Equals(e.Coordinates[i]))
                {
                    isEqualForward = false;
                }

                if (!Coordinates[i].Equals(e.Coordinates[--iRev]))
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
            if (Coordinates.Count != e.Coordinates.Count)
            {
                return false;
            }

            for (Int32 i = 0; i < Coordinates.Count; i++)
            {
                if (!Coordinates[i].Equals(e.Coordinates[i]))
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

            for (Int32 i = 0; i < Coordinates.Count; i++)
            {
                if (i > 0)
                {
                    outstream.Write(",");
                }
                outstream.Write(Coordinates[i][Ordinates.X] + " " + Coordinates[i][Ordinates.Y]);
            }

            outstream.Write(")  " + Label + " " + _depthDelta);
        }

        public void WriteReverse(StreamWriter outstream)
        {
            outstream.Write("edge " + _name + ": ");

            for (Int32 i = Coordinates.Count - 1; i >= 0; i--)
            {
                outstream.Write(Coordinates[i] + " ");
            }

            outstream.WriteLine(String.Empty);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("edge " + _name + ": ");
            sb.Append("LINESTRING (");

            for (Int32 i = 0; i < Coordinates.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(",");
                }

                sb.Append(Coordinates[i][Ordinates.X] + " " + Coordinates[i][Ordinates.Y]);
            }

            sb.Append(")  " + Label + " " + _depthDelta);
            return sb.ToString();
        }

        #region IBoundable<IExtents<TCoordinate>> Members

        IExtents<TCoordinate> IBoundable<IExtents<TCoordinate>>.Bounds
        {
            get { return Extents; }
        }

        Boolean IBoundable<IExtents<TCoordinate>>.Intersects(IExtents<TCoordinate> bounds)
        {
            return Extents.Intersects(bounds);
        }

        #endregion
    }
}