using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;
using GeoAPI.Diagnostics;
using GeoAPI.DataStructures;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// A base class for a ring of <see cref="Edge{TCoordinate}"/>s of a graph.
    /// </summary>
    /// <typeparam name="TCoordinate">The type of the coordinate to use.</typeparam>
    public abstract class EdgeRing<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        // The maximum degree of vertexes permitted in this ring
        private Int32 _maxNodeDegree = -1;

        // The directed edge which starts the list of edges for this EdgeRing.
        private DirectedEdge<TCoordinate> _startingEdge;

        // the DirectedEdges making up this EdgeRing
        private readonly List<DirectedEdge<TCoordinate>> _edges 
            = new List<DirectedEdge<TCoordinate>>(); 

        private readonly List<TCoordinate> _coordinates = new List<TCoordinate>();
        
        // label stores the locations of each point on the face surrounded by this ring
        private Label _label = new Label(Locations.None);

        // the ring created for this EdgeRing
        private ILinearRing<TCoordinate> _ring; 

        private Boolean _isHole;

        // if non-null, the ring is a hole and this EdgeRing is its containing shell
        private EdgeRing<TCoordinate> _shell;

        // a list of EdgeRings which are holes in this EdgeRing
        private readonly List<EdgeRing<TCoordinate>> _holes 
            = new List<EdgeRing<TCoordinate>>(); 

        protected IGeometryFactory<TCoordinate> _geometryFactory;

        public EdgeRing(DirectedEdge<TCoordinate> start, IGeometryFactory<TCoordinate> geometryFactory)
        {
            _geometryFactory = geometryFactory;
            ComputePoints(start);
            ComputeRing();
        }

        public Boolean IsIsolated
        {
            get { return _label.GeometryCount == 1; }
        }

        public Boolean IsHole
        {
            get { return _isHole; }
        }

        public TCoordinate GetCoordinate(Int32 i)
        {
            return _coordinates[i];
        }

        public ILinearRing<TCoordinate> LinearRing
        {
            get { return _ring; }
        }

        public Label Label
        {
            get { return _label; }
        }

        public Boolean IsShell
        {
            get { return _shell == null; }
        }

        public EdgeRing<TCoordinate> Shell
        {
            get { return _shell; }
            set
            {
                _shell = value;

                if (value != null)
                {
                    _shell.AddHole(this);
                }
            }
        }

        public void AddHole(EdgeRing<TCoordinate> ring)
        {
            _holes.Add(ring);
        }

        public IPolygon<TCoordinate> ToPolygon(IGeometryFactory<TCoordinate> geometryFactory)
        {
            IPolygon<TCoordinate> poly = geometryFactory.CreatePolygon(LinearRing, 
                                                                       getLinearRings(_holes));
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
            {
                return; // don't compute more than once
            }

            _ring = _geometryFactory.CreateLinearRing(_coordinates);
            _isHole = CGAlgorithms<TCoordinate>.IsCCW(_ring.Coordinates);
        }

        public abstract DirectedEdge<TCoordinate> GetNext(DirectedEdge<TCoordinate> de);

        public abstract void SetEdgeRing(DirectedEdge<TCoordinate> de, 
                                         EdgeRing<TCoordinate> er);

        /// <summary> 
        /// Returns the list of DirectedEdges that make up this EdgeRing.
        /// </summary>
        public IEnumerable<DirectedEdge<TCoordinate>> Edges
        {
            get
            {
                foreach (DirectedEdge<TCoordinate> edge in _edges)
                {
                    yield return edge;
                }
            }
        }

        protected DirectedEdge<TCoordinate> StartingEdge
        {
            get { return _startingEdge; }
        }

        protected IGeometryFactory<TCoordinate> GeometryFactory
        {
            get { return _geometryFactory; }
        }

        /// <summary> 
        /// Collect all the points from the <see cref="DirectedEdge{TCooordinate}"/>s 
        /// of this ring into a contiguous list.
        /// </summary>
        protected void ComputePoints(DirectedEdge<TCoordinate> start)
        {
            _startingEdge = start;
            DirectedEdge<TCoordinate> de = start;
            Boolean isFirstEdge = true;

            do
            {
                Debug.Assert(de != null);
                //Assert.IsTrue(de != null, "found null Directed Edge");

                if (de.EdgeRing == this)
                {
                    throw new TopologyException("Directed Edge visited twice during "+
                                                "ring-building at " + de.Coordinate);
                }

                _edges.Add(de);
                Debug.Assert(de.Label.HasValue);
                Label label = de.Label.Value;
                Assert.IsTrue(label.IsArea());
                MergeLabel(label);
                AddPoints(de.Edge, de.IsForward, isFirstEdge);
                isFirstEdge = false;
                SetEdgeRing(de, this);
                de = GetNext(de);
            } while (de != _startingEdge);
        }

        public Int32 MaxNodeDegree
        {
            get
            {
                if (_maxNodeDegree < 0)
                {
                    computeMaxNodeDegree();
                }

                return _maxNodeDegree;
            }
        }

        public void SetInResult()
        {
            DirectedEdge<TCoordinate> de = _startingEdge;

            do
            {
                de.Edge.InResult = true;
                de = de.Next;
            } while (de != _startingEdge);
        }

        /// <summary> 
        /// This method will cause the ring to be computed.
        /// It will also check any holes, if they have been assigned.
        /// </summary>
        public Boolean ContainsPoint(TCoordinate p)
        {
            ILinearRing<TCoordinate> shell = LinearRing;
            IExtents<TCoordinate> extents = shell.Extents;

            if (!extents.Contains(p))
            {
                return false;
            }

            if (!CGAlgorithms<TCoordinate>.IsPointInRing(p, shell.Coordinates))
            {
                return false;
            }

            foreach (EdgeRing<TCoordinate> hole in _holes)
            {
                if (hole.ContainsPoint(p))
                {
                    return false;
                }
            }

            return true;
        }

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
        /// does not contribute any information to the overall labeling, and is simply skipped.
        /// </summary>
        protected void MergeLabel(Label deLabel, Int32 geomIndex)
        {
            Locations loc = deLabel[geomIndex, Positions.Right];

            // no information to be had from this label
            if (loc == Locations.None)
            {
                return;
            }

            // if there is no current RHS value, set it
            if (_label[geomIndex].On == Locations.None)
            {
                _label = new Label(_label, geomIndex, loc);
                return;
            }
        }

        protected void AddPoints(Edge<TCoordinate> edge, Boolean isForward, Boolean isFirstEdge)
        {
            IEnumerable<TCoordinate> edgePts = edge.Coordinates;

            if (isForward)
            {
                Int32 startIndex = 1;

                if (isFirstEdge)
                {
                    startIndex = 0;
                }

                foreach (TCoordinate coordinate in Enumerable.Skip(edgePts, startIndex))
                {
                    _coordinates.Add(coordinate);
                }
            }
            else
            {
                // is backward
                Int32 startIndex = 1;

                if (isFirstEdge)
                {
                    startIndex = 0;
                }

                _coordinates.AddRange(Slice.ReverseStartAt(edgePts, startIndex));
            }
        }

        private void computeMaxNodeDegree()
        {
            _maxNodeDegree = 0;
            DirectedEdge<TCoordinate> de = _startingEdge;

            do
            {
                Node<TCoordinate> node = de.Node;
                DirectedEdgeStar<TCoordinate> star = node.Edges as DirectedEdgeStar<TCoordinate>;
                Debug.Assert(star != null);
                Int32 degree = star.GetOutgoingDegree(this);

                if (degree > _maxNodeDegree)
                {
                    _maxNodeDegree = degree;
                }

                de = GetNext(de);
            } while (de != _startingEdge);

            _maxNodeDegree *= 2;
        }

        private static IEnumerable<ILinearRing<TCoordinate>> getLinearRings(
            IEnumerable<EdgeRing<TCoordinate>> rings)
        {
            foreach (EdgeRing<TCoordinate> ring in rings)
            {
                yield return ring.LinearRing;
            }
        }
    }
}