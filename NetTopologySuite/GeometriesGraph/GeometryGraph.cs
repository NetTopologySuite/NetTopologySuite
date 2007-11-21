using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.GeometriesGraph.Index;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// A GeometryGraph is a graph that models a given Geometry.
    /// </summary>
    public class GeometryGraph<TCoordinate> : PlanarGraph<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        /// <summary> 
        /// This method implements the Boundary Determination Rule
        /// for determining whether
        /// a component (node or edge) that appears multiple times in elements
        /// of a MultiGeometry is in the boundary or the interior of the Geometry.
        /// The SFS uses the "Mod-2 Rule", which this function implements.
        /// An alternative (and possibly more intuitive) rule would be
        /// the "At Most One Rule":
        /// isInBoundary = (componentCount == 1)
        /// </summary>
        public static Boolean IsInBoundary(Int32 boundaryCount)
        {
            // the "Mod-2 Rule"
            return boundaryCount%2 == 1;
        }

        public static Locations DetermineBoundary(Int32 boundaryCount)
        {
            return IsInBoundary(boundaryCount) ? Locations.Boundary : Locations.Interior;
        }

        private readonly IGeometry<TCoordinate> _parentGeometry;

        // The lineEdgeMap is a map of the linestring components of the
        // parentGeometry to the edges which are derived from them.
        // This is used to efficiently perform findEdge queries.
        private readonly Dictionary<ILineString<TCoordinate>, Edge<TCoordinate>> _lineEdgeMap
            = new Dictionary<ILineString<TCoordinate>, Edge<TCoordinate>>();

        /// <summary>
        /// If this flag is true, the Boundary Determination Rule will used when deciding
        /// whether nodes are in the boundary or not
        /// </summary>
        private Boolean _useBoundaryDeterminationRule = false;

        private readonly Int32 _argIndex; // the index of this point as an argument to a spatial function (used for labeling)
        private IEnumerable<Node<TCoordinate>> _boundaryNodes;
        private Boolean _hasTooFewPoints = false;
        private TCoordinate _invalidPoint = default(TCoordinate);

        private static EdgeSetIntersector<TCoordinate> createEdgeSetIntersector()
        {
            // various options for computing intersections, from slowest to fastest                    
            return new SimpleMCSweepLineIntersector<TCoordinate>();
        }

        public GeometryGraph(Int32 argIndex, IGeometry<TCoordinate> parentGeom)
        {
            _argIndex = argIndex;
            _parentGeometry = parentGeom;

            if (parentGeom != null)
            {
                Add(parentGeom);
            }
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

        public IEnumerable<TCoordinate> GetBoundaryPoints()
        {
            IEnumerable<Node<TCoordinate>> nodes = BoundaryNodes;

            foreach (Node<TCoordinate> node in nodes)
            {
                yield return (TCoordinate)node.Coordinate.Clone();
            }
        }

        public Edge<TCoordinate> FindEdge(ILineString<TCoordinate> line)
        {
            return _lineEdgeMap[line];
        }

        public void ComputeSplitEdges(IEnumerable<Edge<TCoordinate>> edgelist)
        {
            foreach (Edge<TCoordinate> edge in edgelist)
            {
                // TODO: How did this ever work?
                edge.EdgeIntersectionList.AddSplitEdges(edgelist);
            }
        }

        private void Add(IGeometry<TCoordinate> g)
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
                AddPolygon((IPolygon<TCoordinate>)g);
            }                                
                // LineString also handles LinearRings
            else if (g is ILineString<TCoordinate>)
            {
                AddLineString((ILineString<TCoordinate>)g);
            }
            else if (g is IPoint<TCoordinate>)
            {
                AddPoint((IPoint<TCoordinate>)g);
            }
            else if (g is IMultiPoint<TCoordinate>)
            {
                AddCollection((IGeometryCollection<TCoordinate>)g);
            }
            else if (g is IMultiLineString<TCoordinate>)
            {
                AddCollection((IGeometryCollection<TCoordinate>)g);
            }
            else if (g is IMultiPolygon<TCoordinate>)
            {
                AddCollection((IGeometryCollection<TCoordinate>)g);
            }
            else if (g is IGeometryCollection<TCoordinate>)
            {
                AddCollection((IGeometryCollection<TCoordinate>) g);
            }
            else
            {
                throw new NotSupportedException(g.GetType().FullName);
            }
        }

        private void AddCollection(IGeometryCollection<TCoordinate> gc)
        {
            for (Int32 i = 0; i < gc.Count; i++)
            {
                IGeometry<TCoordinate> g = gc[i];
                Add(g);
            }
        }

        /// <summary> 
        /// Add a Point to the graph.
        /// </summary>
        private void AddPoint(IPoint<TCoordinate> p)
        {
            TCoordinate coord = p.Coordinate;
            insertPoint(_argIndex, coord, Locations.Interior);
        }

        /// <summary> 
        /// The left and right topological location arguments assume that the ring is oriented CW.
        /// If the ring is in the opposite orientation,
        /// the left and right locations must be interchanged.
        /// </summary>
        private void AddPolygonRing(ILinearRing<TCoordinate> lr, Locations cwLeft, Locations cwRight)
        {
            IList<TCoordinate> coord = CoordinateArrays.RemoveRepeatedPoints(lr.Coordinates);
            
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

            Edge<TCoordinate> e = new Edge<TCoordinate>(coord, new Label(_argIndex, Locations.Boundary, left, right));
            
            if (_lineEdgeMap.Contains(lr))
            {
                _lineEdgeMap.Remove(lr);
            }

            _lineEdgeMap.Add(lr, e);
            InsertEdge(e);
            // insert the endpoint as a node, to mark that it is on the boundary
            insertPoint(_argIndex, coord[0], Locations.Boundary);
        }

        private void AddPolygon(IPolygon<TCoordinate> p)
        {
            AddPolygonRing(p.Shell, Locations.Exterior, Locations.Interior);

            for (Int32 i = 0; i < p.InteriorRingsCount; i++)
            {
                // Holes are topologically labeled opposite to the shell, since
                // the interior of the polygon lies on their opposite side
                // (on the left, if the hole is oriented CW)
                AddPolygonRing(p.Holes[i], Locations.Interior, Locations.Exterior);
            }
        }

        private void AddLineString(ILineString<TCoordinate> line)
        {
            IList<TCoordinate> coord = CoordinateArrays.RemoveRepeatedPoints(line.Coordinates);
            
            if (coord.Count < 2)
            {
                _hasTooFewPoints = true;
                _invalidPoint = coord[0];
                return;
            }

            // add the edge for the LineString
            // line edges do not have locations for their left and right sides
            Edge<TCoordinate> e = new Edge<TCoordinate>(coord, new Label(_argIndex, Locations.Interior));
            
            if (_lineEdgeMap.Contains(line))
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

        /// <summary> 
        /// Add an Edge computed externally.  The label on the Edge is assumed
        /// to be correct.
        /// </summary>
        public void AddEdge(Edge<TCoordinate> e)
        {
            InsertEdge(e);
            IList<TCoordinate> coord = e.Coordinates;
            // insert the endpoint as a node, to mark that it is on the boundary
            insertPoint(_argIndex, coord[0], Locations.Boundary);
            insertPoint(_argIndex, coord[coord.Count - 1], Locations.Boundary);
        }

        /// <summary>
        /// Add a point computed externally.  The point is assumed to be a
        /// Point Geometry part, which has a location of INTERIOR.
        /// </summary>
        public void AddPoint(TCoordinate pt)
        {
            insertPoint(_argIndex, pt, Locations.Interior);
        }

        /// <summary>
        /// Compute self-nodes, taking advantage of the Geometry type to
        /// minimize the number of intersection tests.  (E.g. rings are
        /// not tested for self-intersection, since they are assumed to be valid).
        /// </summary>
        /// <param name="li">The <c>LineIntersector</c> to use.</param>
        /// <param name="computeRingSelfNodes">If <c>false</c>, intersection checks are optimized to not test rings for self-intersection.</param>
        /// <returns>The SegmentIntersector used, containing information about the intersections found.</returns>
        public SegmentIntersector<TCoordinate> ComputeSelfNodes(
            LineIntersector<TCoordinate> li, Boolean computeRingSelfNodes)
        {
            SegmentIntersector<TCoordinate> si = new SegmentIntersector<TCoordinate>(li, true, false);
            EdgeSetIntersector<TCoordinate> esi = createEdgeSetIntersector();
            
            // optimized test for Polygons and Rings
            if (!computeRingSelfNodes &&
                (_parentGeometry is ILinearRing<TCoordinate> 
                    || _parentGeometry is IPolygon<TCoordinate> 
                    || _parentGeometry is IMultiPolygon<TCoordinate>))
            {
                esi.ComputeIntersections(Edges, si, false);
            }
            else
            {
                esi.ComputeIntersections(Edges, si, true);
            }

            addSelfIntersectionNodes(_argIndex);
            return si;
        }

        public SegmentIntersector<TCoordinate> ComputeEdgeIntersections(
            GeometryGraph<TCoordinate> g, LineIntersector<TCoordinate> li, Boolean includeProper)
        {
            SegmentIntersector<TCoordinate> si = new SegmentIntersector<TCoordinate>(li, includeProper, true);
            si.SetBoundaryNodes(BoundaryNodes, g.BoundaryNodes);
            EdgeSetIntersector<TCoordinate> esi = createEdgeSetIntersector();
            esi.ComputeIntersections(Edges, g.Edges, si);
            return si;
        }

        private void insertPoint(Int32 argIndex, TCoordinate coord, Locations onLocation)
        {
            Node<TCoordinate> n = NodeMap.AddNode(coord);
            Label lbl = n.Label;

            if (lbl == null)
            {
                n.Label = new Label(argIndex, onLocation);
            }
            else
            {
                lbl.SetLocation(argIndex, onLocation);
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
            Label lbl = n.Label;

            // the new point to insert is on a boundary
            Int32 boundaryCount = 1;

            // determine the current location for the point (if any)
            Locations loc = Locations.None;

            if (lbl != null)
            {
                loc = lbl.GetLocation(argIndex, Positions.On);
            }
            if (loc == Locations.Boundary)
            {
                boundaryCount++;
            }

            // determine the boundary status of the point according to the Boundary Determination Rule
            Locations newLoc = DetermineBoundary(boundaryCount);
            lbl.SetLocation(argIndex, newLoc);
        }

        private void addSelfIntersectionNodes(Int32 argIndex)
        {
            foreach (Edge<TCoordinate> edge in Edges)
            {
                Locations eLoc = edge.Label.GetLocation(argIndex);

                foreach (EdgeIntersection<TCoordinate> intersection in edge.EdgeIntersectionList)
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
    }
}