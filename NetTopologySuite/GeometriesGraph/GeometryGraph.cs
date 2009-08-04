using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Diagnostics;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.GeometriesGraph.Index;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// A <see cref="GeometryGraph{TCoordinate}"/> is a graph that models a given 
    /// <see cref="Geometry"/>, where nodes and edges in the graph
    /// correspond to vertexes and line segments in the geometry.
    /// </summary>
    public class GeometryGraph<TCoordinate> : PlanarGraph<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        // the index of this point as an argument to a spatial function (used for labeling)
        private readonly Int32 _argIndex;

        private readonly IBoundaryNodeRule _boundaryNodeRule;

        private readonly Dictionary<ILineString<TCoordinate>, Edge<TCoordinate>> _lineEdgeMap
            = new Dictionary<ILineString<TCoordinate>, Edge<TCoordinate>>();

        private readonly IGeometry<TCoordinate> _parentGeometry;

        private IEnumerable<Node<TCoordinate>> _boundaryNodes;
        private Boolean _hasTooFewPoints;
        private TCoordinate _invalidPoint;

        /// <summary>
        /// If this flag is true, the Boundary Determination Rule will used when deciding
        /// whether nodes are in the boundary or not
        /// </summary>
        private Boolean _useBoundaryDeterminationRule = true;

        public GeometryGraph(Int32 argIndex, IGeometry<TCoordinate> parentGeom)
            : this(argIndex, parentGeom, new Mod2BoundaryNodeRule())
        {
        }

        public GeometryGraph(Int32 argIndex,
                             IGeometry<TCoordinate> parentGeom,
                             IBoundaryNodeRule boundaryNodeRule)
        {
            _argIndex = argIndex;
            _parentGeometry = parentGeom;
            _boundaryNodeRule = boundaryNodeRule;

            if (parentGeom != null)
            {
                add(parentGeom);
            }
        }

        public IBoundaryNodeRule BoundaryNodeRule
        {
            get { return _boundaryNodeRule; }
        }

        public Boolean HasTooFewPoints
        {
            get { return _hasTooFewPoints; }
        }

        public ICoordinate InvalidPoint
        {
            get { return _invalidPoint; }
        }

        public IGeometry<TCoordinate> Geometry
        {
            get { return _parentGeometry; }
        }

        public IEnumerable<Node<TCoordinate>> BoundaryNodes
        {
            get
            {
                if (_boundaryNodes == null)
                {
                    _boundaryNodes = NodeMap.GetBoundaryNodes(_argIndex);
                }

                return _boundaryNodes;
            }
        }

        public override string ToString()
        {
            return base.ToString() + String.Format(" Parent Geometry: Index {0}; {1}", _argIndex,
                                                   _parentGeometry);
        }

        public IEnumerable<TCoordinate> GetBoundaryPoints()
        {
            IEnumerable<Node<TCoordinate>> nodes = BoundaryNodes;

            foreach (Node<TCoordinate> node in nodes)
            {
                yield return node.Coordinate.Clone();
            }
        }

        public Edge<TCoordinate> FindEdge(ILineString<TCoordinate> line)
        {
            return _lineEdgeMap[line];
        }

        public IEnumerable<Edge<TCoordinate>> ComputeSplitEdges()
        {
            foreach (Edge<TCoordinate> edge in Edges)
            {
                IEnumerable<Edge<TCoordinate>> splitEdges
                    = edge.EdgeIntersections.GetSplitEdges();

                foreach (Edge<TCoordinate> splitEdge in splitEdges)
                {
                    yield return splitEdge;
                }
            }
        }

        /// <summary> 
        /// Add an <see cref="Edge{TCoordinate}"/> computed externally.  
        /// The <see cref="Label"/> on the edge is assumed to be correct.
        /// </summary>
        public void AddEdge(Edge<TCoordinate> e)
        {
            InsertEdge(e);
            ICoordinateSequence<TCoordinate> coordinates = e.Coordinates;

            // insert the endpoint as a node, to mark that it is on the boundary
            insertPoint(_argIndex, coordinates.First, Locations.Boundary);
            insertPoint(_argIndex, coordinates.Last, Locations.Boundary);
        }

        /// <summary>
        /// Add a point computed externally.  The point is assumed to be a
        /// <see cref="IPoint{TCoordinate}"/> geometry part, 
        /// which has a location of <see cref="Locations.Interior"/>.
        /// </summary>
        public void AddPoint(TCoordinate pt)
        {
            insertPoint(_argIndex, pt, Locations.Interior);
        }

        /// <summary>
        /// Compute self-nodes, taking advantage of the <see cref="OgcGeometryType"/> 
        /// to minimize the number of intersection tests, if possible (e.g. rings are
        /// not tested for self-intersection, since they are assumed to be valid).
        /// </summary>
        /// <param name="li">
        /// The <see cref="LineIntersector{TCoordinate}"/> to use.
        /// </param>
        /// <param name="computeRingSelfNodes">
        /// If <see langword="false"/>, intersection checks are optimized 
        /// to not test rings for self-intersection.
        /// </param>
        /// <returns>
        /// The <see cref="SegmentIntersector{TCoordinate}"/> used, 
        /// containing information about the intersections found.
        /// </returns>
        public SegmentIntersector<TCoordinate> ComputeSelfNodes(
            LineIntersector<TCoordinate> li,
            Boolean computeRingSelfNodes)
        {
            SegmentIntersector<TCoordinate> si = new SegmentIntersector<TCoordinate>(li,
                                                                                     true,
                                                                                     false);
            EdgeSetIntersector<TCoordinate> esi = createEdgeSetIntersector();

            // optimized test for Polygons and Rings: don't test for self-intersections
            // since a property of valid rings and polygons is that they don't
            // self-intersect
            if (!computeRingSelfNodes &&
                (_parentGeometry is ILinearRing<TCoordinate> ||
                 _parentGeometry is IPolygon<TCoordinate> ||
                 _parentGeometry is IMultiPolygon<TCoordinate>))
            {
                esi.ComputeIntersections(Edges, si, false);
            }
            else
            {
                // Lines can self-intersect, so we'll have to test for it
                esi.ComputeIntersections(Edges, si, true);
            }

            addSelfIntersectionNodes(_argIndex);
            return si;
        }

        public SegmentIntersector<TCoordinate> ComputeEdgeIntersections(
            GeometryGraph<TCoordinate> g,
            LineIntersector<TCoordinate> li,
            Boolean includeProper)
        {
            SegmentIntersector<TCoordinate> si
                = new SegmentIntersector<TCoordinate>(li, includeProper, true);

            si.SetBoundaryNodes(BoundaryNodes, g.BoundaryNodes);
            EdgeSetIntersector<TCoordinate> esi = createEdgeSetIntersector();
            esi.ComputeIntersections(Edges, g.Edges, si);
            return si;
        }

        private void add(IGeometry<TCoordinate> g)
        {
            if (g.IsEmpty)
            {
                return;
            }

            // check if this Geometry should obey the Boundary Determination Rule
            // all collections except MultiPolygons obey the rule
            if (g is IGeometryCollection<TCoordinate> && !(g is IMultiPolygon<TCoordinate>))
            {
                _useBoundaryDeterminationRule = true;
            }

            if (g is IPolygon<TCoordinate>)
            {
                addPolygon((IPolygon<TCoordinate>) g);
            }
                // LineString also handles LinearRings
            else if (g is ILineString<TCoordinate>)
            {
                addLineString((ILineString<TCoordinate>) g);
            }
            else if (g is IPoint<TCoordinate>)
            {
                addPoint((IPoint<TCoordinate>) g);
            }
            else if (g is IMultiPoint<TCoordinate>)
            {
                addCollection((IGeometryCollection<TCoordinate>) g);
            }
            else if (g is IMultiLineString<TCoordinate>)
            {
                addCollection((IGeometryCollection<TCoordinate>) g);
            }
            else if (g is IMultiPolygon<TCoordinate>)
            {
                addCollection((IGeometryCollection<TCoordinate>) g);
            }
            else if (g is IGeometryCollection<TCoordinate>)
            {
                addCollection((IGeometryCollection<TCoordinate>) g);
            }
            else
            {
                throw new NotSupportedException(g.GetType().FullName);
            }
        }

        private void addCollection(IGeometryCollection<TCoordinate> gc)
        {
            for (Int32 i = 0; i < gc.Count; i++)
            {
                IGeometry<TCoordinate> g = gc[i];
                add(g);
            }
        }

        /// <summary> 
        /// Add a Point to the graph.
        /// </summary>
        private void addPoint(IPoint<TCoordinate> p)
        {
            TCoordinate coord = p.Coordinate;
            insertPoint(_argIndex, coord, Locations.Interior);
        }

        /// <summary> 
        /// The left and right topological location arguments assume that the ring is oriented CW.
        /// If the ring is in the opposite orientation,
        /// the left and right locations must be interchanged.
        /// </summary>
        private void addPolygonRing(ILineString<TCoordinate> ring, Locations cwLeft, Locations cwRight)
        {
            ICoordinateSequence<TCoordinate> coord = ring.Coordinates.WithoutRepeatedPoints();

            if (coord.Count < 4)
            {
                _hasTooFewPoints = true;
                _invalidPoint = coord[0];
                return;
            }

            Locations left = cwLeft;
            Locations right = cwRight;

            if (CGAlgorithms<TCoordinate>.IsCCW(coord))
            {
                left = cwRight;
                right = cwLeft;
            }

            Label label = new Label(_argIndex, Locations.Boundary, left, right);
            Edge<TCoordinate> e = new Edge<TCoordinate>(_parentGeometry.Factory,
                                                        coord,
                                                        label);

            _lineEdgeMap[ring] = e;
            InsertEdge(e);

            // insert the endpoint as a node, to mark that it is on the boundary
            insertPoint(_argIndex, coord[0], Locations.Boundary);
        }

        private void addPolygon(IPolygon<TCoordinate> p)
        {
            addPolygonRing(p.ExteriorRing, Locations.Exterior, Locations.Interior);

            foreach (ILinearRing<TCoordinate> interiorRing in p.InteriorRings)
            {
                // Holes are topologically labeled opposite to the shell, since
                // the interior of the polygon lies on their opposite side
                // (on the left, if the hole is oriented CW)
                addPolygonRing(interiorRing, Locations.Interior, Locations.Exterior);
            }
        }

        private void addLineString(ILineString<TCoordinate> line)
        {
            ICoordinateSequence<TCoordinate> coord = line.Coordinates.WithoutRepeatedPoints();

            if (coord.Count < 2)
            {
                _hasTooFewPoints = true;
                _invalidPoint = coord[0];
                return;
            }

            // add the edge for the LineString
            // line edges do not have locations for their left and right sides
            Label label = new Label(_argIndex, Locations.Interior);
            Edge<TCoordinate> e = new Edge<TCoordinate>(_parentGeometry.Factory,
                                                        coord,
                                                        label);

            if (_lineEdgeMap.ContainsKey(line))
            {
                _lineEdgeMap.Remove(line);
            }

            _lineEdgeMap.Add(line, e);
            InsertEdge(e);

            /*
            * Add the boundary points of the LineString, if any.
            * Even if the LineString is closed, add both points as if they were endpoints.
            * This allows for the case that the node already exists and is a boundary point.
            */
            Assert.IsTrue(coord.Count >= 2, "found LineString with single point");
            insertBoundaryPoint(_argIndex, coord[0]);
            insertBoundaryPoint(_argIndex, coord[coord.Count - 1]);
        }

        private void insertPoint(Int32 argIndex, TCoordinate coord, Locations onLocation)
        {
            Node<TCoordinate> n = NodeMap.AddNode(coord);
            Label? lbl = n.Label;

            if (lbl == null)
            {
                n.Label = new Label(argIndex, onLocation);
            }
            else
            {
                n.Label = new Label(lbl.Value, argIndex, onLocation);
            }
        }

        /// <summary> 
        /// Adds points using the mod-2 rule of SFS.  This is used to add the boundary
        /// points of dim-1 geometries (Curves/MultiCurves).  According to the SFS,
        /// an endpoint of a Curve is on the boundary
        /// if it is in the boundaries of an odd number of Geometries.
        /// </summary>
        private void insertBoundaryPoint(Int32 argIndex, TCoordinate coord)
        {
            Node<TCoordinate> n = NodeMap.AddNode(coord);
            Label? currentLabel = n.Label;

            // the new point to insert is on a boundary
            Int32 boundaryCount = 1;

            // determine the current location for the point (if any)
            Locations loc = Locations.None;

            if (currentLabel != null)
            {
                loc = currentLabel.Value[argIndex, Positions.On];
            }
            if (loc == Locations.Boundary)
            {
                boundaryCount++;
            }

            // determine the boundary status of the point according to the 
            // Boundary Determination Rule
            Locations newLoc = _boundaryNodeRule.IsInBoundary(boundaryCount)
                                   ? Locations.Boundary
                                   : Locations.Interior;

            n.Label = currentLabel != null
                          ? new Label(currentLabel.Value, argIndex, newLoc)
                          : new Label(argIndex, newLoc);
        }

        private void addSelfIntersectionNodes(Int32 argIndex)
        {
            foreach (Edge<TCoordinate> edge in Edges)
            {
                Debug.Assert(edge.Label.HasValue);
                Locations eLoc = edge.Label.Value[argIndex].On;

                foreach (EdgeIntersection<TCoordinate> intersection in edge.EdgeIntersections)
                {
                    addSelfIntersectionNode(argIndex, intersection.Coordinate, eLoc);
                }
            }
        }

        /// <summary>
        /// Add a node for a self-intersection.
        /// If the node is a potential boundary node (e.g. came from an edge which
        /// is a boundary) then insert it as a potential boundary node.
        /// Otherwise, just add it as a regular node.
        /// </summary>
        private void addSelfIntersectionNode(Int32 argIndex, TCoordinate coord, Locations loc)
        {
            // if this node is already a boundary node, don't change it
            if (IsBoundaryNode(argIndex, coord))
            {
                return;
            }

            if (loc == Locations.Boundary && _useBoundaryDeterminationRule)
            {
                insertBoundaryPoint(argIndex, coord);
            }
            else
            {
                insertPoint(argIndex, coord, loc);
            }
        }

        private static EdgeSetIntersector<TCoordinate> createEdgeSetIntersector()
        {
            // [codekaizen 2008-04-16] Just use the 
            //                         SimpleMonotoneChaingSweepLineIntersector,
            //                         since it is the fastest, and other are commented
            //                         out in JTS source...
            return new SimpleMonotoneChainSweepLineIntersector<TCoordinate>();
        }
    }
}