using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;
namespace NetTopologySuite.GeometriesGraph
{
    /// <summary>
    ///
    /// </summary>
    public abstract class EdgeRing
    {
        /// <summary>
        /// The directed edge which starts the list of edges for this EdgeRing.
        /// </summary>
        protected DirectedEdge startDe;
        private int _maxNodeDegree = -1;
        private readonly List<DirectedEdge> _edges = new List<DirectedEdge>();  // the DirectedEdges making up this EdgeRing
        private readonly List<Coordinate> _pts = new List<Coordinate>();
        private EdgeRing _shell;   // if non-null, the ring is a hole and this EdgeRing is its containing shell
        private readonly List<EdgeRing> _holes = new List<EdgeRing>(); // a list of EdgeRings which are holes in this EdgeRing
        /// <summary>
        ///
        /// </summary>
        private readonly IGeometryFactory _geometryFactory;
        protected IGeometryFactory GeometryFactory => _geometryFactory;
        /// <summary>
        ///
        /// </summary>
        /// <param name="start"></param>
        /// <param name="geometryFactory"></param>
        protected EdgeRing(DirectedEdge start, IGeometryFactory geometryFactory)
        {
            _geometryFactory = geometryFactory;
            ComputePoints(start);
            ComputeRing();
        }
        /// <summary>
        ///
        /// </summary>
        public bool IsIsolated => Label.GeometryCount == 1;
        /// <summary>
        ///
        /// </summary>
        public bool IsHole { get; private set; }
        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Coordinate GetCoordinate(int i)
        {
            return _pts[i];
        }
        /// <summary>
        ///
        /// </summary>
        public ILinearRing LinearRing { get; private set; }
        /// <summary>
        ///
        /// </summary>
        public Label Label { get; } = new Label(Location.Null);
        /// <summary>
        ///
        /// </summary>
        public bool IsShell => _shell == null;
        /// <summary>
        ///
        /// </summary>
        public EdgeRing Shell
        {
            get => _shell;
            set
            {
                _shell = value;
                if (value != null)
                    _shell.AddHole(this);
            }
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="ring"></param>
        public void AddHole(EdgeRing ring)
        {
            _holes.Add(ring);
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="geometryFactory"></param>
        /// <returns></returns>
        public IPolygon ToPolygon(IGeometryFactory geometryFactory)
        {
            var holeLR = new ILinearRing[_holes.Count];
            for (var i = 0; i < _holes.Count; i++)
                holeLR[i] = _holes[i].LinearRing;
            var poly = geometryFactory.CreatePolygon(LinearRing, holeLR);
            return poly;
        }
        /// <summary>
        /// Compute a LinearRing from the point list previously collected.
        /// Test if the ring is a hole (i.e. if it is CCW) and set the hole flag
        /// accordingly.
        /// </summary>
        public void ComputeRing()
        {
            if (LinearRing != null)
                return;   // don't compute more than once
            var coord = _pts.ToArray();
            /* new Coordinate[_pts.Count];
            for (int i = 0; i < _pts.Count; i++)
                coord[i] = (Coordinate) _pts[i];
             */
            LinearRing = _geometryFactory.CreateLinearRing(coord);
            IsHole = Orientation.IsCCW(LinearRing.Coordinates);
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="de"></param>
        /// <returns></returns>
        abstract public DirectedEdge GetNext(DirectedEdge de);
        /// <summary>
        ///
        /// </summary>
        /// <param name="de"></param>
        /// <param name="er"></param>
        abstract public void SetEdgeRing(DirectedEdge de, EdgeRing er);
        /// <summary>
        /// Returns the list of DirectedEdges that make up this EdgeRing.
        /// </summary>
        public IList<DirectedEdge> Edges => _edges;
        /// <summary>
        /// Collect all the points from the DirectedEdges of this ring into a contiguous list.
        /// </summary>
        /// <param name="start"></param>
        protected void ComputePoints(DirectedEdge start)
        {
            startDe = start;
            var de = start;
            var isFirstEdge = true;
            do
            {
                if (de == null)
                    throw new TopologyException("found null Directed Edge");
                if (de.EdgeRing == this)
                    throw new TopologyException("Directed Edge visited twice during ring-building at " + de.Coordinate);
                _edges.Add(de);
                var label = de.Label;
                Assert.IsTrue(label.IsArea());
                MergeLabel(label);
                AddPoints(de.Edge, de.IsForward, isFirstEdge);
                isFirstEdge = false;
                SetEdgeRing(de, this);
                de = GetNext(de);
            }
            while (de != startDe);
        }
        /// <summary>
        ///
        /// </summary>
        public int MaxNodeDegree
        {
            get
            {
                if (_maxNodeDegree < 0)
                    ComputeMaxNodeDegree();
                return _maxNodeDegree;
            }
        }
        /// <summary>
        ///
        /// </summary>
        private void ComputeMaxNodeDegree()
        {
            _maxNodeDegree = 0;
            var de = startDe;
            do
            {
                var node = de.Node;
                var degree = ((DirectedEdgeStar) node.Edges).GetOutgoingDegree(this);
                if (degree > _maxNodeDegree)
                    _maxNodeDegree = degree;
                de = GetNext(de);
            }
            while (de != startDe);
            _maxNodeDegree *= 2;
        }
        /// <summary>
        ///
        /// </summary>
        public void SetInResult()
        {
            var de = startDe;
            do
            {
                de.Edge.InResult = true;
                de = de.Next;
            }
            while (de != startDe);
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="deLabel"></param>
        protected void MergeLabel(Label deLabel)
        {
            MergeLabel(deLabel, 0);
            MergeLabel(deLabel, 1);
        }
        /// <summary>
        /// Merge the RHS label from a DirectedEdge into the label for this EdgeRing.
        /// The DirectedEdge label may be null.  This is acceptable - it results
        /// from a node which is NOT an intersection node between the Geometries
        /// (e.g. the end node of a LinearRing).  In this case the DirectedEdge label
        /// does not contribute any information to the overall labelling, and is simply skipped.
        /// </summary>
        /// <param name="deLabel"></param>
        /// <param name="geomIndex"></param>
        protected void MergeLabel(Label deLabel, int geomIndex)
        {
            var loc = deLabel.GetLocation(geomIndex, Positions.Right);
            // no information to be had from this label
            if (loc == Location.Null)
                return;
            // if there is no current RHS value, set it
            if (Label.GetLocation(geomIndex) == Location.Null)
            {
                Label.SetLocation(geomIndex, loc);
                return;
            }
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="isForward"></param>
        /// <param name="isFirstEdge"></param>
        protected void AddPoints(Edge edge, bool isForward, bool isFirstEdge)
        {
            var edgePts = edge.Coordinates;
            if (isForward)
            {
                var startIndex = 1;
                if (isFirstEdge)
                    startIndex = 0;
                for (var i = startIndex; i < edgePts.Length; i++)
                    _pts.Add(edgePts[i]);
            }
            else
            {
                // is backward
                var startIndex = edgePts.Length - 2;
                if (isFirstEdge)
                    startIndex = edgePts.Length - 1;
                for (var i = startIndex; i >= 0; i--)
                    _pts.Add(edgePts[i]);
            }
        }
        /// <summary>
        /// This method will cause the ring to be computed.
        /// It will also check any holes, if they have been assigned.
        /// </summary>
        /// <param name="p"></param>
        public bool ContainsPoint(Coordinate p)
        {
            var shell = LinearRing;
            var env = shell.EnvelopeInternal;
            if (!env.Contains(p))
                return false;
            if (!PointLocation.IsInRing(p, shell.Coordinates))
                return false;
            foreach (var hole in _holes)
            {
                if (hole.ContainsPoint(p))
                    return false;
            }
            return true;
        }
    }
}
