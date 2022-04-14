using System.Collections.Generic;
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
        private readonly Label _label = new Label(Location.Null); // label stores the locations of each point on the face surrounded by this ring
        private LinearRing _ring;  // the ring created for this EdgeRing
        private bool _isHole;
        private EdgeRing _shell;   // if non-null, the ring is a hole and this EdgeRing is its containing shell
        private readonly List<EdgeRing> _holes = new List<EdgeRing>(); // a list of EdgeRings which are holes in this EdgeRing

        private readonly GeometryFactory _geometryFactory;


        /// <summary>
        /// A <c>GeometryFactory</c> to use.
        /// </summary>
        protected GeometryFactory GeometryFactory => _geometryFactory;

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="start"/> and <paramref name="geometryFactory"/>.
        /// </summary>
        /// <param name="start">The start <c>DirectedEdge</c> for the <c>EdgeRing</c> </param>
        /// <param name="geometryFactory">A <c>GeometryFactory</c></param>
        protected EdgeRing(DirectedEdge start, GeometryFactory geometryFactory)
        {
            _geometryFactory = geometryFactory;
            ComputePoints(start);
            ComputeRing();
        }

        /// <summary>
        /// Gets a value indicating if this <c>EdgeRing</c> is isolated
        /// </summary>
        public bool IsIsolated => _label.GeometryCount == 1;

        /// <summary>
        /// Gets a value indicating if this <c>EdgeRing</c> is a hole of a <c>Polygon</c>
        /// </summary>
        public bool IsHole => _isHole;

        /// <summary>
        /// Access a <c>Coordinate</c> of this <c>EdgeRing</c> by its index.
        /// </summary>
        /// <param name="i">The index of the <c>Coordinate</c></param>
        /// <returns>The <c>Coordinate</c> at index <paramref name="i"/></returns>
        public Coordinate GetCoordinate(int i)
        {
            return _pts[i];
        }

        /// <summary>
        /// Gets the geometry representation of this <c>EdgeRing</c>
        /// </summary>
        public LinearRing LinearRing => _ring;

        /// <summary>
        /// Gets a value indicating the topological relationship of this <c>EdgeRing</c>
        /// </summary>
        public Label Label => _label;

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
        /// Add an <c>EdgeRing</c> representing a hole
        /// </summary>
        /// <param name="ring">The ring to add</param>
        public void AddHole(EdgeRing ring)
        {
            _holes.Add(ring);
        }

        /// <summary>
        /// Create the <c>Polygon</c> described by this <c>EdgeRing</c>
        /// </summary>
        /// <param name="geometryFactory">The <c>GeometryFactory</c> to use.</param>
        /// <returns>A <c>Polygon</c></returns>
        public Polygon ToPolygon(GeometryFactory geometryFactory)
        {
            var holeLR = new LinearRing[_holes.Count];
            for (int i = 0; i < _holes.Count; i++)
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
            if (_ring != null)
                return;   // don't compute more than once
            var coord = _pts.ToArray();
            /* new Coordinate[_pts.Count];
            for (int i = 0; i < _pts.Count; i++)
                coord[i] = (Coordinate) _pts[i];
             */
            _ring = _geometryFactory.CreateLinearRing(coord);
            _isHole = Orientation.IsCCW(_ring.Coordinates);
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
        /// <returns>A list of <c>DirectedEdge</c>s</returns>
        public IList<DirectedEdge> Edges => _edges;

        /// <summary>
        /// Collect all the points from the DirectedEdges of this ring into a contiguous list.
        /// </summary>
        /// <param name="start"></param>
        protected void ComputePoints(DirectedEdge start)
        {
            startDe = start;
            var de = start;
            bool isFirstEdge = true;
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
                int degree = ((DirectedEdgeStar) node.Edges).GetOutgoingDegree(this);
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
            var loc = deLabel.GetLocation(geomIndex, Geometries.Position.Right);
            // no information to be had from this label
            if (loc == Location.Null)
                return;
            // if there is no current RHS value, set it
            if (_label.GetLocation(geomIndex) == Location.Null)
            {
                _label.SetLocation(geomIndex, loc);
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
                int startIndex = 1;
                if (isFirstEdge)
                    startIndex = 0;
                for (int i = startIndex; i < edgePts.Length; i++)
                    _pts.Add(edgePts[i]);
            }
            else
            {
                // is backward
                int startIndex = edgePts.Length - 2;
                if (isFirstEdge)
                    startIndex = edgePts.Length - 1;
                for (int i = startIndex; i >= 0; i--)
                    _pts.Add(edgePts[i]);
            }
        }

        /// <summary>
        /// This method will cause the ring to be computed.
        /// It will also check any holes, if they have been assigned.
        /// </summary>
        /// <param name="p">The point to test</param>
        /// <returns><c>true</c> if the ring contains point <paramref name="p"/></returns>
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
