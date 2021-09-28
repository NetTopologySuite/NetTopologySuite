using System;
using System.IO;
using System.Text;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph.Index;

namespace NetTopologySuite.GeometriesGraph
{
    /// <summary>
    ///
    /// </summary>
    public class Edge : GraphComponent
    {
        /// <summary>
        /// Updates an IM from the label for an edge.
        /// Handles edges from both L and A geometries.
        /// </summary>
        /// <param name="im">An intersection matrix</param>
        /// <param name="label">A label</param>
        public static void UpdateIM(Label label, IntersectionMatrix im)
        {
            im.SetAtLeastIfValid(label.GetLocation(0, Geometries.Position.On), label.GetLocation(1, Geometries.Position.On), Dimension.Curve);
            if (label.IsArea())
            {
                im.SetAtLeastIfValid(label.GetLocation(0, Geometries.Position.Left), label.GetLocation(1, Geometries.Position.Left), Dimension.Surface);
                im.SetAtLeastIfValid(label.GetLocation(0, Geometries.Position.Right), label.GetLocation(1, Geometries.Position.Right), Dimension.Surface);
            }
        }

        private Coordinate[] _pts;

        private Envelope _env;
        private readonly EdgeIntersectionList _eiList;

        private string _name;
        private MonotoneChainEdge _mce;
        private bool _isIsolated = true;
        private readonly Depth _depth = new Depth();
        private int _depthDelta;   // the change in area depth from the R to Curve side of this edge

        /// <summary>
        ///
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="label"></param>
        public Edge(Coordinate[] pts, Label label)
        {
            _eiList = new EdgeIntersectionList(this);

            _pts = pts;
            Label = label;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pts"></param>
        public Edge(Coordinate[] pts) : this(pts, null) { }

        /// <summary>
        ///
        /// </summary>
        public Coordinate[] Points
        {
            get => _pts;
            set => _pts = value;
        }

        /// <summary>
        ///
        /// </summary>
        public int NumPoints => Points.Length;

        /// <summary>
        ///
        /// </summary>
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        /// <summary>
        ///
        /// </summary>
        public Coordinate[] Coordinates => Points;

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Coordinate GetCoordinate(int i)
        {
            try
            {
                return Points[i];
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public override Coordinate Coordinate
        {
            get => Points.Length > 0 ? Points[0] : null;
            protected set => throw new NotSupportedException();
        }

        /// <summary>
        ///
        /// </summary>
        public Envelope Envelope
        {
            get
            {
                // compute envelope lazily
                if (_env == null)
                {
                    _env = new Envelope();
                    for (int i = 0; i < Points.Length; i++)
                        _env.ExpandToInclude(Points[i]);
                }
                return _env;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public Depth Depth => _depth;

        /// <summary>
        /// The depthDelta is the change in depth as an edge is crossed from R to L.
        /// </summary>
        /// <returns>The change in depth as the edge is crossed from R to L.</returns>
        public int DepthDelta
        {
            get => _depthDelta;
            set => _depthDelta = value;
        }

        /// <summary>
        ///
        /// </summary>
        public int MaximumSegmentIndex => Points.Length - 1;

        /// <summary>
        ///
        /// </summary>
        public EdgeIntersectionList EdgeIntersectionList => _eiList;

        /// <summary>
        ///
        /// </summary>
        public MonotoneChainEdge MonotoneChainEdge
        {
            get
            {
                if (_mce == null)
                    _mce = new MonotoneChainEdge(this);
                return _mce;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsClosed => Points[0].Equals(Points[Points.Length - 1]);

        /// <summary>
        /// An Edge is collapsed if it is an Area edge and it consists of
        /// two segments which are equal and opposite (eg a zero-width V).
        /// </summary>
        /// <returns><c>true</c> if edge is consisting of two segments
        /// which are equal and of oppose orientation (Zero-width V area edge)
        /// </returns>
        public bool IsCollapsed
        {
            get
            {
                if (!Label.IsArea())
                    return false;
                if (Points.Length != 3)
                    return false;
                if (Points[0].Equals(Points[2]) )
                    return true;
                return false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public Edge CollapsedEdge
        {
            get
            {
                var newPts = new Coordinate[2];
                newPts[0] = Points[0];
                newPts[1] = Points[1];
                var newe = new Edge(newPts, Label.ToLineLabel(Label));
                return newe;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public bool Isolated
        {
            get => _isIsolated;
            set => _isIsolated = value;
        }

        /// <summary>
        ///
        /// </summary>
        public override bool IsIsolated => _isIsolated;

        /// <summary>
        /// Adds EdgeIntersections for one or both
        /// intersections found for a segment of an edge to the edge intersection list.
        /// </summary>
        /// <param name="li">A line intersector</param>
        /// <param name="segmentIndex">A segment index</param>
        /// <param name="geomIndex">A geometry index</param>
        public void AddIntersections(LineIntersector li, int segmentIndex, int geomIndex)
        {
            for (int i = 0; i < li.IntersectionNum; i++)
                AddIntersection(li, segmentIndex, geomIndex, i);
        }

        /// <summary>
        /// Add an EdgeIntersection for intersection intIndex.
        /// An intersection that falls exactly on a vertex of the edge is normalized
        /// to use the higher of the two possible segmentIndexes.
        /// </summary>
        /// <param name="li">A line intersector</param>
        /// <param name="segmentIndex">A segment index</param>
        /// <param name="geomIndex">A geometry index</param>
        /// <param name="intIndex">The intersection index (0 or 1)</param>
        public void AddIntersection(LineIntersector li, int segmentIndex, int geomIndex, int intIndex)
        {
            var intPt = li.GetIntersection(intIndex).Copy();
            int normalizedSegmentIndex = segmentIndex;
            double dist = li.GetEdgeDistance(geomIndex, intIndex);

            // normalize the intersection point location
            int nextSegIndex = normalizedSegmentIndex + 1;
            if (nextSegIndex < Points.Length)
            {
                var nextPt = Points[nextSegIndex];

                // Normalize segment index if intPt falls on vertex
                // The check for point equality is 2D only - Z values are ignored
                if (intPt.Equals2D(nextPt))
                {
                    normalizedSegmentIndex = nextSegIndex;
                    dist = 0.0;
                }
                // Add the intersection point to edge intersection list.
                EdgeIntersectionList.Add(intPt, normalizedSegmentIndex, dist);
            }
        }

        /// <summary>
        /// Update the IM with the contribution for this component.
        /// A component only contributes if it has a labelling for both parent geometries.
        /// </summary>
        /// <param name="im"></param>
        public override void ComputeIM(IntersectionMatrix im)
        {
            UpdateIM(Label, im);
        }

        /// <summary>
        /// Equals is defined to be:
        /// e1 equals e2
        /// if
        /// the coordinates of e1 are the same or the reverse of the coordinates in e2.
        /// </summary>
        /// <param name="o"></param>
        public override bool Equals(object o)
        {
            if (o == null)
                return false;
            if (!(o is Edge))
                return false;
            return Equals(o as Edge);
        }

        /// <inheritdoc cref="object.GetHashCode()"/>
        public override int GetHashCode()
        {
            const int prime = 31;
            // ReSharper disable NonReadonlyMemberInGetHashCode
            int result = prime + _pts.Length;
            if (_pts.Length > 0)
            {
                var p0 = _pts[0];
                var p1 = _pts[_pts.Length - 1];
                if (1 == p0.CompareTo(p1))
                {
                    p0 = _pts[_pts.Length - 1];
                    p1 = _pts[0];
                }
                result = prime * result + p0.GetHashCode();
                result = prime * result + p1.GetHashCode();
            }
            // ReSharper restore NonReadonlyMemberInGetHashCode
            return result;
        }
        /// <summary>
        /// Equals is defined to be:
        /// e1 equals e2
        /// if
        /// the coordinates of e1 are the same or the reverse of the coordinates in e2.
        /// </summary>
        /// <param name="e"></param>
        protected bool Equals(Edge e)
        {
            if (Points.Length != e.Points.Length)
                return false;

            bool isEqualForward = true;
            bool isEqualReverse = true;
            int iRev = Points.Length;
            for (int i = 0; i < Points.Length; i++)
            {
                if (!Points[i].Equals2D(e.Points[i]))
                    isEqualForward = false;
                if (!Points[i].Equals2D(e.Points[--iRev]))
                    isEqualReverse = false;
                if (!isEqualForward && !isEqualReverse)
                    return false;
            }
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator ==(Edge obj1, Edge obj2)
        {
            return Equals(obj1, obj2);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator !=(Edge obj1, Edge obj2)
        {
            return !(obj1 == obj2);
        }


        /// <summary>
        /// Check if coordinate sequences of the Edges are identical. 
        /// </summary>
        /// <param name="e">The edge to test</param>
        /// <returns>
        /// <c>true</c> if the coordinate sequences of the Edges are identical.
        /// </returns>
        public bool IsPointwiseEqual(Edge e)
        {
            if (Points.Length != e.Points.Length)
                return false;
            for (int i = 0; i < Points.Length; i++)
                if (! Points[i].Equals2D(e.Points[i]))
                    return false;
            return true;
        }

        /// <inheritdoc cref="object.ToString()"/>>
        public override string ToString()
        {
            var buf = new StringBuilder();
            buf.Append("edge " + _name + ": ");
            buf.Append("LINESTRING (");
            for (int i = 0; i < _pts.Length; i++)
            {
                if (i > 0) buf.Append(",");
                buf.Append(_pts[i].X + " " + _pts[i].Y);
            }
            buf.Append(")  " + Label + " " + _depthDelta);
            return buf.ToString();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="outstream"></param>
        public void Write(TextWriter outstream)
        {
            outstream.Write("edge " + _name + ": ");
            outstream.Write("LINESTRING (");
            for (int i = 0; i < Points.Length; i++)
            {
                if (i > 0)  outstream.Write(",");
                outstream.Write(Points[i].X + " " + Points[i].Y);
            }
            outstream.Write(")  " + Label + " " + _depthDelta);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="outstream"></param>
        public void WriteReverse(TextWriter outstream)
        {
            outstream.Write("edge " + _name + ": ");
            for (int i = Points.Length - 1; i >= 0; i--)
                outstream.Write(Points[i] + " ");
            outstream.WriteLine(string.Empty);
        }
    }
}
