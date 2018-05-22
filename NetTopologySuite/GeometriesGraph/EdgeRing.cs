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
        private readonly Label _label = new Label(Location.Null); // label stores the locations of each point on the face surrounded by this ring
        private ILinearRing _ring;  // the ring created for this EdgeRing
        private bool _isHole;
        private EdgeRing _shell;   // if non-null, the ring is a hole and this EdgeRing is its containing shell
        private readonly List<EdgeRing> _holes = new List<EdgeRing>(); // a list of EdgeRings which are holes in this EdgeRing

        /// <summary>
        /// 
        /// </summary>
        private readonly IGeometryFactory _geometryFactory;

        protected IGeometryFactory GeometryFactory { get { return _geometryFactory; } }

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
        public bool IsIsolated
        {
            get
            {
                return _label.GeometryCount == 1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsHole
        {
            get
            {             
                return _isHole;
            }
        }

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
        public ILinearRing LinearRing
        {
            get
            {
                return _ring;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Label Label
        {
            get
            {
                return _label;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsShell
        {
            get
            {
                return _shell == null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public EdgeRing Shell
        {
            get
            {
                return _shell;
            }
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
            ILinearRing[] holeLR = new ILinearRing[_holes.Count];
            for (int i = 0; i < _holes.Count; i++)
                holeLR[i] = _holes[i].LinearRing;
            IPolygon poly = geometryFactory.CreatePolygon(LinearRing, holeLR);
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
            Coordinate[] coord = _pts.ToArray();
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
        public IList<DirectedEdge> Edges
        {
            get
            {
                return _edges;
            }
        }

        /// <summary> 
        /// Collect all the points from the DirectedEdges of this ring into a contiguous list.
        /// </summary>
        /// <param name="start"></param>
        protected void ComputePoints(DirectedEdge start)
        {
            startDe = start;
            DirectedEdge de = start;
            bool isFirstEdge = true;
            do
            {
                if (de == null)
                    throw new TopologyException("found null Directed Edge");
                if (de.EdgeRing == this)
                    throw new TopologyException("Directed Edge visited twice during ring-building at " + de.Coordinate);

                _edges.Add(de);                
                Label label = de.Label;
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
            DirectedEdge de = startDe;
            do
            {
                Node node = de.Node;
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
            DirectedEdge de = startDe;
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
            Location loc = deLabel.GetLocation(geomIndex, Positions.Right);
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
            Coordinate[] edgePts = edge.Coordinates;
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
        /// <param name="p"></param>
        public bool ContainsPoint(Coordinate p)
        {
            ILinearRing shell = LinearRing;
            Envelope env = shell.EnvelopeInternal;
            if (!env.Contains(p)) 
                return false;
            if (!PointLocation.IsInRing(p, shell.Coordinates)) 
                return false;
            foreach (EdgeRing hole in _holes)
            {
                if (hole.ContainsPoint(p))
                    return false;
            }
            return true;
        }
    }
}
