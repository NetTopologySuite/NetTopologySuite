#define usecs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph.Index;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// Represents an edge in a <see cref="GeometryGraph{TCoordinate}"/>.
    /// </summary>
    /// <typeparam name="TCoordinate">The type of coordinate.</typeparam>
    public class Edge<TCoordinate> : GraphComponent<TCoordinate>, 
                                     IBoundable<IExtents<TCoordinate>> //, IBoundable<Interval>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, 
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
#if usecs
        private readonly ICoordinateSequence<TCoordinate> _coordinates;
#else
        private readonly TCoordinate[] _coordinates;
#endif
        private readonly Depth _depth = new Depth();
        private readonly EdgeIntersectionList<TCoordinate> _edgeIntersectionList;
        private readonly IGeometryFactory<TCoordinate> _geoFactory;
        private Int32 _depthDelta;
        private IExtents<TCoordinate> _extents;
        private Boolean _isIsolated = true;
        private MonotoneChainEdge<TCoordinate> _monotoneChainEdge;
        //private String _name;

        public Edge(IGeometryFactory<TCoordinate> geoFactory,
                    IEnumerable<TCoordinate> coordinates)
            : this(geoFactory, coordinates, null)
        {
        }

        public Edge(IGeometryFactory<TCoordinate> geoFactory,
                    IEnumerable<TCoordinate> coordinates,
                    Label? label)
        {
            _geoFactory = geoFactory;
            _edgeIntersectionList
                = new EdgeIntersectionList<TCoordinate>(geoFactory, this);
#if usecs
            _coordinates = coordinates as ICoordinateSequence<TCoordinate>;
#else
            List<TCoordinate> tmp = new List<TCoordinate>(coordinates);
            _coordinates = tmp.ToArray();
#endif
            Label = label;
        }

        public Int32 PointCount
        {
#if usecs
            get { return _coordinates.Count; }
#else
            get { return _coordinates.Length; }
#endif
        }

#if usecs
        public ICoordinateSequence<TCoordinate> Coordinates
        {
            get { return _coordinates; }
        }
        public ICoordinateSequence<TCoordinate> CoordinatesReversed
        {
            get { return _coordinates.Reversed; }
        }
        public override TCoordinate Coordinate
        {
            get
            {
                if (Coordinates.Count > 0)
                {
                    return Coordinates.First;
                }

                return default(TCoordinate);
            }
        }
#else
        public IEnumerable<TCoordinate> Coordinates
        {
            get { return _coordinates; }
        }

        public IEnumerable<TCoordinate> CoordinatesReversed
        {
            get { return new Stack<TCoordinate>(_coordinates); }
        }

        protected TCoordinate[] CoordinatesInternal
        {
            get { return _coordinates; }
        }

        protected TCoordinate[] CoordinatesReversedInternal
        {
            get { return new Stack<TCoordinate>(_coordinates).ToArray(); }
        }

        public override TCoordinate Coordinate
        {
            get
            {
                if (_coordinates != null)
                    return _coordinates[0];
                return default(TCoordinate);
            }
        }
#endif

        public IExtents<TCoordinate> Extents
        {
            get
            {
                // compute envelope lazily
                if (_extents == null)
                {
                    _extents = new Extents<TCoordinate>(_geoFactory);
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

        /// <summary>
        /// Gets the index of the last segment's endpoint.
        /// </summary>
        public Int32 MaximumSegmentIndex
        {
#if usecs
            get { return _coordinates.LastIndex; }
#else
            get { return _coordinates.Length - 1; }
#endif
        }

        /// <summary>
        /// Gets an <see cref="EdgeIntersectionList{TCoordinate}"/> for 
        /// the <see cref="Edge{TCoordinate}"/>.
        /// </summary>
        public EdgeIntersectionList<TCoordinate> EdgeIntersections
        {
            get { return _edgeIntersectionList; }
        }

        /// <summary>
        /// Gets a <see cref="MonotoneChainEdge{TCoordinate}"/> from 
        /// the <see cref="Edge{TCoordinate}"/> instance.
        /// </summary>
        public MonotoneChainEdge<TCoordinate> MonotoneChainEdge
        {
            get
            {
                if (_monotoneChainEdge == null)
                {
                    _monotoneChainEdge
                        = new MonotoneChainEdge<TCoordinate>(this, _geoFactory);
                }

                return _monotoneChainEdge;
            }
        }

        /// <summary>
        /// Gets <see langword="true"/> if the first coordinate of the edge
        /// equals the last coordinate.
        /// </summary>
        public Boolean IsClosed
        {
#if usecs
            get { return Coordinates.First.Equals(Coordinates.Last); }
#else
            get { return _coordinates[0].Equals(_coordinates[MaximumSegmentIndex]); }
#endif
        }

        /// <summary> 
        /// An <see cref="Edge{TCoordinate}"/> is collapsed if it is a
        /// <see cref="Dimensions.Surface"/> edge and it consists of 
        /// two segments which are equal and opposite (e.g. a zero-width 'V' shape).
        /// </summary>
        public Boolean IsCollapsed
        {
            get
            {
                if (!Label.Value.IsArea())
                {
                    return false;
                }

#if usecs
                if (Coordinates.Count != 3)
#else
                if (_coordinates.Length != 3)
#endif
                {
                    return false;
                }

                return _coordinates[0].Equals(_coordinates[2]);
            }
        }

        public Edge<TCoordinate> CollapsedEdge
        {
            get
            {
#if usecs
                ICoordinateSequence<TCoordinate> newPts = Coordinates.Slice(0, 1);
#else
                TCoordinate[] newPts = new TCoordinate[] { _coordinates[0], _coordinates[1] };
#endif
                Label lineLabel = GeometriesGraph.Label.ToLineLabel(Label.Value);
                Edge<TCoordinate> newEdge
                    = new Edge<TCoordinate>(_geoFactory, newPts, lineLabel);
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

        #region IBoundable<IExtents<TCoordinate>> Members

        IExtents<TCoordinate> IBoundable<IExtents<TCoordinate>>.Bounds
        {
            get { return Extents; }
        }

        Boolean IIntersectable<IExtents<TCoordinate>>.Intersects(IExtents<TCoordinate> bounds)
        {
            return Extents.Intersects(bounds);
        }

        #endregion

        //#region IBoundable<Interval> Member

        //Interval IBoundable<Interval>.Bounds
        //{
        //    get { return new Interval(Extents.Min[Ordinates.X], Extents.Max[Ordinates.Y]); }
        //}

        //#endregion

        //#region IIntersectable<Interval> Member

        //bool IIntersectable<Interval>.Intersects(Interval other)
        //{
        //    Interval bounds = new Interval(Extents.Min[Ordinates.X], Extents.Max[Ordinates.Y]);
        //    return bounds.Intersects(other);
        //}

        //#endregion

        /// <summary> 
        /// Updates an <see cref="IntersectionMatrix"/> from the label for an edge.
        /// Handles edges from both L and A geometries.
        /// </summary>
        public static void UpdateIntersectionMatrix(Label label, IntersectionMatrix im)
        {
            im.SetAtLeastIfValid(label[0, Positions.On],
                                 label[1, Positions.On],
                                 Dimensions.Curve);

            if (!label.IsArea())
            {
                return;
            }

            im.SetAtLeastIfValid(label[0, Positions.Left],
                                 label[1, Positions.Left],
                                 Dimensions.Surface);

            im.SetAtLeastIfValid(label[0, Positions.Right],
                                 label[1, Positions.Right],
                                 Dimensions.Surface);
        }

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            //sb.Append("edge " + _name + ": ");
            sb.Append("edge: ");
            sb.Append("LINESTRING (");

            for (Int32 i = 0; i <= MaximumSegmentIndex; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.AppendFormat(
                    CultureInfo.InvariantCulture, "{0} {1}",
                    _coordinates[i][Ordinates.X], _coordinates[i][Ordinates.Y]);
            }

            sb.Append(")  " + Label + " " + _depthDelta);
            return sb.ToString();
        }

        /// <summary>
        /// Adds <see cref="EdgeIntersection{TCoordinate}"/>s for one or both
        /// intersections found for a segment of an edge to the edge intersection list.
        /// </summary>
        public void AddIntersections(Intersection<TCoordinate> intersection,
                                     Int32 segmentIndex,
                                     Int32 geometryIndex)
        {
            Int32 count = (Int32) intersection.IntersectionDegree;

            for (Int32 i = 0; i < count; i++)
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
                                    Int32 segmentIndex,
                                    Int32 geometryIndex,
                                    Int32 intersectionIndex)
        {
            TCoordinate intersectionPoint = intersection.GetIntersectionPoint(intersectionIndex);
            Int32 normalizedSegmentIndex = segmentIndex;
            Double dist = intersection.GetEdgeDistance(geometryIndex, intersectionIndex);

            // normalize the intersection point location
            Int32 nextSegIndex = normalizedSegmentIndex + 1;

            if (nextSegIndex > MaximumSegmentIndex)
            {
                return;
            }

            TCoordinate nextPt = _coordinates[nextSegIndex];

            // Normalize segment index if intersectionPoint falls on vertex
            // The check for point equality is 2D only - Z values are ignored
            // 3D_UNSAFE
            if (intersectionPoint.Equals(nextPt))
            {
                normalizedSegmentIndex = nextSegIndex;
                dist = 0.0;
            }

            // Add the intersection point to edge intersection list.                
            EdgeIntersections.Add(intersectionPoint, normalizedSegmentIndex, dist);
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
        /// Computes whether the coordinate sequences of the 
        /// <see cref="Edge{TCoordinate}"/>s are identical.
        /// </summary>
        /// <returns> 
        /// <see langword="true"/> if the coordinate sequences of the 
        /// <see cref="Edge{TCoordinate}"/>s are identical.
        /// </returns>
        public Boolean IsPointwiseEqual(Edge<TCoordinate> e)
        {
            if (MaximumSegmentIndex != e.MaximumSegmentIndex)
            {
                return false;
            }

            for (Int32 i = 0; i < MaximumSegmentIndex; i++)
            {
                if (!_coordinates[i].Equals(e._coordinates[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Equals is defined to be:
        /// e1 equals e2
        /// iff
        /// the coordinates of e1 are the same or the reverse of the coordinates in e2.
        /// </summary>
        public override Boolean Equals(Object o)
        {
            Edge<TCoordinate> other = o as Edge<TCoordinate>;

            if (other == null)
            {
                return false;
            }

            return Equals(other);
        }

        /// <summary>
        /// Equals is defined to be:
        /// e1 equals e2
        /// iff
        /// the coordinates of e1 are the same or the reverse of the coordinates in e2.
        /// </summary>
        protected Boolean Equals(Edge<TCoordinate> e)
        {
            if (MaximumSegmentIndex != e.MaximumSegmentIndex)
            {
                return false;
            }

            Boolean isEqualForward = true;
            Boolean isEqualReverse = true;

            Int32 iRev = MaximumSegmentIndex + 1;

            for (Int32 i = 0; i <= MaximumSegmentIndex; i++)
            {
                if (!_coordinates[i].Equals(e._coordinates[i]))
                {
                    isEqualForward = false;
                }

                if (!_coordinates[i].Equals(e._coordinates[--iRev]))
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

        //public IList<TCoordinate> Points
        //{
        //    get { return _coordinates; }
        //    set
        //    {
        //        _coordinates.Clear();
        //        _coordinates.AddRange(value);
        //    }
        //}

        //public string Name
        //{
        //    get { return _name; }
        //    set { _name = value; }
        //}

        //public TCoordinate GetCoordinate(Int32 i)
        //{
        //    return Points[i];
        //}

        //public void Write(StreamWriter outstream)
        //{
        //    outstream.Write("edge " + _name + ": ");
        //    outstream.Write("LINESTRING (");

        //    for (Int32 i = 0; i < Coordinates.TotalItemCount; i++)
        //    {
        //        if (i > 0)
        //        {
        //            outstream.Write(",");
        //        }
        //        outstream.Write(Coordinates[i][Ordinates.X] + " " + Coordinates[i][Ordinates.Y]);
        //    }

        //    outstream.Write(")  " + Label + " " + _depthDelta);
        //}

        //public void WriteReverse(StreamWriter outstream)
        //{
        //    outstream.Write("edge " + _name + ": ");

        //    for (Int32 i = Coordinates.TotalItemCount - 1; i >= 0; i--)
        //    {
        //        outstream.Write(Coordinates[i] + " ");
        //    }

        //    outstream.WriteLine(String.Empty);
        //}

        //#region IBoundable<Interval> Member

        //Interval IBoundable<Interval>.Bounds
        //{
        //    get { return new Interval(Extents.Min[Ordinates.X], Extents.Max[Ordinates.Y]); }
        //}

        //#endregion

        //#region IIntersectable<Interval> Member

        //bool IIntersectable<Interval>.Intersects(Interval other)
        //{
        //    Interval bounds = new Interval(Extents.Min[Ordinates.X], Extents.Max[Ordinates.Y]);
        //    return bounds.Intersects(other);
        //}

        //#endregion
    }
}