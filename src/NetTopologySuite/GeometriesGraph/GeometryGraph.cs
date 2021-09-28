using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph.Index;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// A GeometryGraph is a graph that models a given Geometry.
    /// </summary>
    public class GeometryGraph : PlanarGraph
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
        /*
        public static bool IsInBoundary(int boundaryCount)
        {
            // the "Mod-2 Rule"
            return boundaryCount % 2 == 1;
        }*/

        /// <summary>
        /// Determine boundary
        /// </summary>
        /// <param name="boundaryNodeRule">The boundary node rule to apply for determination of the boundary</param>
        /// <param name="boundaryCount">The number of component boundaries that a point occurs in.</param>
        /// <returns><see cref="Location.Boundary"/> or <see cref="Location.Interior"/></returns>
        public static Location DetermineBoundary(IBoundaryNodeRule boundaryNodeRule, int boundaryCount)
        {
            return boundaryNodeRule.IsInBoundary(boundaryCount)
                ? Location.Boundary : Location.Interior;
        }

        private readonly Geometry _parentGeom;

        /// <summary>
        /// The lineEdgeMap is a map of the linestring components of the
        /// parentGeometry to the edges which are derived from them.
        /// This is used to efficiently perform findEdge queries
        /// </summary>
        private readonly IDictionary<LineString, Edge> _lineEdgeMap = new Dictionary<LineString, Edge>();

        private readonly IBoundaryNodeRule _boundaryNodeRule;

        /// <summary>
        /// If this flag is true, the Boundary Determination Rule will used when deciding
        /// whether nodes are in the boundary or not
        /// </summary>
        private bool _useBoundaryDeterminationRule = true;

        private readonly int _argIndex;  // the index of this point as an argument to a spatial function (used for labelling)
        private IList<Node> _boundaryNodes;
        private bool _hasTooFewPoints;
        private Coordinate _invalidPoint;

        private IPointOnGeometryLocator _areaPtLocator;
        // for use if geometry is not Polygonal
        private readonly PointLocator _ptLocator = new PointLocator();

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private static EdgeSetIntersector CreateEdgeSetIntersector()
        {
            // various options for computing intersections, from slowest to fastest
            return new SimpleMCSweepLineIntersector();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="argIndex"></param>
        /// <param name="parentGeom"></param>
        public GeometryGraph(int argIndex, Geometry parentGeom)
            : this(argIndex, parentGeom, BoundaryNodeRules.OgcSfsBoundaryRule)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="argIndex"></param>
        /// <param name="parentGeom"></param>
        /// <param name="boundaryNodeRule"></param>
        public GeometryGraph(int argIndex, Geometry parentGeom, IBoundaryNodeRule boundaryNodeRule)
        {
            _argIndex = argIndex;
            _boundaryNodeRule = boundaryNodeRule;
            _parentGeom = parentGeom;
            if (parentGeom != null)
                Add(parentGeom);
        }

        /// <summary>
        ///
        /// </summary>
        public bool HasTooFewPoints => _hasTooFewPoints;

        /// <summary>
        ///
        /// </summary>
        public Coordinate InvalidPoint => _invalidPoint;

        /// <summary>
        ///
        /// </summary>
        public Geometry Geometry => _parentGeom;

        /// <summary>
        /// Gets the <see cref="IBoundaryNodeRule"/> used with this geometry graph.
        /// </summary>
        public IBoundaryNodeRule BoundaryNodeRule => _boundaryNodeRule;

        /// <summary>
        ///
        /// </summary>
        public IList<Node> BoundaryNodes
        {
            get
            {
                if (_boundaryNodes == null)
                    _boundaryNodes = NodeMap.GetBoundaryNodes(_argIndex);
                return _boundaryNodes;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public Coordinate[] GetBoundaryPoints()
        {
            var coll = BoundaryNodes;
            var pts = new Coordinate[coll.Count];
            int i = 0;
            foreach (var node in coll)
            {
                pts[i++] = (Coordinate)node.Coordinate.Copy();
            }
            return pts;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public Edge FindEdge(LineString line)
        {
            return _lineEdgeMap[line];
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="edgelist"></param>
        public void ComputeSplitEdges(IList<Edge> edgelist)
        {
            foreach (var e in Edges)
            {
                e.EdgeIntersectionList.AddSplitEdges(edgelist);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="g"></param>
        private void Add(Geometry g)
        {
            if (g.IsEmpty)
                return;

            // check if this Geometry should obey the Boundary Determination Rule
            // all collections except MultiPolygons obey the rule
            if (g is MultiPolygon)
                _useBoundaryDeterminationRule = false;

            if (g is Polygon)
                AddPolygon((Polygon)g);
            // LineString also handles LinearRings
            else if (g is LineString)
                AddLineString((LineString)g);
            else if (g is Point)
                AddPoint((Point)g);
            else if (g is MultiPoint)
                AddCollection((MultiPoint)g);
            else if (g is MultiLineString)
                AddCollection((MultiLineString)g);
            else if (g is MultiPolygon)
                AddCollection((MultiPolygon)g);
            else if (g is GeometryCollection)
                AddCollection((GeometryCollection)g);
            else
                throw new NotSupportedException(g.GetType().FullName);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="gc"></param>
        private void AddCollection(GeometryCollection gc)
        {
            for (int i = 0; i < gc.NumGeometries; i++)
            {
                var g = gc.GetGeometryN(i);
                Add(g);
            }
        }

        /// <summary>
        /// Add a Point to the graph.
        /// </summary>
        /// <param name="p"></param>
        private void AddPoint(Point p)
        {
            var coord = p.Coordinate;
            InsertPoint(_argIndex, coord, Location.Interior);
        }

        /// <summary>
        /// Adds a polygon ring to the graph. Empty rings are ignored.
        /// The left and right topological location arguments assume that the ring is oriented CW.
        /// If the ring is in the opposite orientation,
        /// the left and right locations must be interchanged.
        /// </summary>
        /// <param name="lr"></param>
        /// <param name="cwLeft"></param>
        /// <param name="cwRight"></param>
        private void AddPolygonRing(LinearRing lr, Location cwLeft, Location cwRight)
        {
            // don't bother adding empty holes
            if (lr.IsEmpty)
                return;

            var coord = CoordinateArrays.RemoveRepeatedPoints(lr.Coordinates);
            if (coord.Length < 4)
            {
                _hasTooFewPoints = true;
                _invalidPoint = coord[0];
                return;
            }
            var left = cwLeft;
            var right = cwRight;
            if (Orientation.IsCCW(coord))
            {
                left = cwRight;
                right = cwLeft;
            }
            var e = new Edge(coord, new Label(_argIndex, Location.Boundary, left, right));
            _lineEdgeMap[lr] = e;
            InsertEdge(e);
            // insert the endpoint as a node, to mark that it is on the boundary
            InsertPoint(_argIndex, coord[0], Location.Boundary);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p"></param>
        private void AddPolygon(Polygon p)
        {
            AddPolygonRing(p.Shell, Location.Exterior, Location.Interior);

            for (int i = 0; i < p.NumInteriorRings; i++)
            {
                var hole = p.Holes[i];
                // Holes are topologically labelled opposite to the shell, since
                // the interior of the polygon lies on their opposite side
                // (on the left, if the hole is oriented CW)
                AddPolygonRing(hole, Location.Interior, Location.Exterior);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="line"></param>
        private void AddLineString(LineString line)
        {
            var coord = CoordinateArrays.RemoveRepeatedPoints(line.Coordinates);
            if (coord.Length < 2)
            {
                _hasTooFewPoints = true;
                _invalidPoint = coord[0];
                return;
            }

            // add the edge for the LineString
            // line edges do not have locations for their left and right sides
            var e = new Edge(coord, new Label(_argIndex, Location.Interior));
            _lineEdgeMap[line] = e;
            InsertEdge(e);

            /*
             * Add the boundary points of the LineString, if any.
             * Even if the LineString is closed, add both points as if they were endpoints.
             * This allows for the case that the node already exists and is a boundary point.
             */
            Assert.IsTrue(coord.Length >= 2, "found LineString with single point");
            InsertBoundaryPoint(_argIndex, coord[0]);
            InsertBoundaryPoint(_argIndex, coord[coord.Length - 1]);
        }

        /// <summary>
        /// Add an Edge computed externally.  The label on the Edge is assumed
        /// to be correct.
        /// </summary>
        /// <param name="e">An <c>Edge</c></param>
        public void AddEdge(Edge e)
        {
            InsertEdge(e);
            var coord = e.Coordinates;
            // insert the endpoint as a node, to mark that it is on the boundary
            InsertPoint(_argIndex, coord[0], Location.Boundary);
            InsertPoint(_argIndex, coord[coord.Length - 1], Location.Boundary);
        }

        /// <summary>
        /// Add a point computed externally.  The point is assumed to be a
        /// Point Geometry part, which has a location of INTERIOR.
        /// </summary>
        /// <param name="pt">A <c>Coordinate</c></param>
        public void AddPoint(Coordinate pt)
        {
            InsertPoint(_argIndex, pt, Location.Interior);
        }

        /// <summary>
        /// Compute self-nodes, taking advantage of the Geometry type to
        /// minimize the number of intersection tests.  (E.g. rings are
        /// not tested for self-intersection, since they are assumed to be valid).
        /// </summary>
        /// <param name="li">The <c>LineIntersector</c> to use.</param>
        /// <param name="computeRingSelfNodes">If <c>false</c>, intersection checks are optimized to not test rings for self-intersection.</param>
        /// <returns>The computed SegmentIntersector, containing information about the intersections found.</returns>
        public SegmentIntersector ComputeSelfNodes(LineIntersector li, bool computeRingSelfNodes)
        {
            return ComputeSelfNodes(li, computeRingSelfNodes, false);
        }

        /// <summary>
        /// Compute self-nodes, taking advantage of the Geometry type to
        /// minimize the number of intersection tests.  (E.g.rings are
        /// not tested for self-intersection, since they are assumed to be valid).
        /// </summary >
        /// <param name="li">The <c>LineIntersector</c> to use</param>
        /// <param name="computeRingSelfNodes">If <c>false</c>, intersection checks are optimized to not test rings for self-intersection</param>
        /// <param name="isDoneIfProperInt">Short-circuit the intersection computation if a proper intersection is found</param>
        public SegmentIntersector ComputeSelfNodes(LineIntersector li, bool computeRingSelfNodes, bool isDoneIfProperInt)
        {
            var si = new SegmentIntersector(li, true, false);
            si.IsDoneIfProperInt = isDoneIfProperInt;
            var esi = CreateEdgeSetIntersector();
            // optimize intersection search for valid Polygons and LinearRings
            bool isRings = _parentGeom is LinearRing
                          || _parentGeom is Polygon
                          || _parentGeom is MultiPolygon;
            bool computeAllSegments = computeRingSelfNodes || !isRings;
            esi.ComputeIntersections(Edges, si, computeAllSegments);

            //System.out.println("SegmentIntersector # tests = " + si.numTests);
            AddSelfIntersectionNodes(_argIndex);
            return si;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="g"></param>
        /// <param name="li"></param>
        /// <param name="includeProper"></param>
        /// <returns></returns>
        public SegmentIntersector ComputeEdgeIntersections(GeometryGraph g,
            LineIntersector li, bool includeProper)
        {
            var si = new SegmentIntersector(li, includeProper, true);
            si.SetBoundaryNodes(BoundaryNodes, g.BoundaryNodes);
            var esi = CreateEdgeSetIntersector();
            esi.ComputeIntersections(Edges, g.Edges, si);
            return si;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="argIndex"></param>
        /// <param name="coord"></param>
        /// <param name="onLocation"></param>
        private void InsertPoint(int argIndex, Coordinate coord, Location onLocation)
        {
            var n = NodeMap.AddNode(coord);
            var lbl = n.Label;
            if (lbl == null)
                n.Label = new Label(argIndex, onLocation);
            else lbl.SetLocation(argIndex, onLocation);
        }

        /// <summary>
        /// Adds candidate boundary points using the current <see cref="IBoundaryNodeRule"/>.
        /// This is used to add the boundary
        /// points of dim-1 geometries (Curves/MultiCurves).
        /// </summary>
        /// <param name="argIndex"></param>
        /// <param name="coord"></param>
        private void InsertBoundaryPoint(int argIndex, Coordinate coord)
        {
            var n = NodeMap.AddNode(coord);
            // nodes always have labels
            var lbl = n.Label;
            // the new point to insert is on a boundary
            int boundaryCount = 1;
            // determine the current location for the point (if any)
            //Location loc = Location.Null;
            var loc = lbl.GetLocation(argIndex, Geometries.Position.On);
            if (loc == Location.Boundary)
                boundaryCount++;

            // determine the boundary status of the point according to the Boundary Determination Rule
            var newLoc = DetermineBoundary(_boundaryNodeRule, boundaryCount);
            lbl.SetLocation(argIndex, newLoc);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="argIndex"></param>
        private void AddSelfIntersectionNodes(int argIndex)
        {
            foreach (var e in Edges)
            {
                var eLoc = e.Label.GetLocation(argIndex);
                foreach (var ei in e.EdgeIntersectionList)
                {
                    AddSelfIntersectionNode(argIndex, ei.Coordinate, eLoc);
                }
            }
        }

        /// <summary>
        /// Add a node for a self-intersection.
        /// If the node is a potential boundary node (e.g. came from an edge which
        /// is a boundary) then insert it as a potential boundary node.
        /// Otherwise, just add it as a regular node.
        /// </summary>
        /// <param name="argIndex"></param>
        /// <param name="coord"></param>
        /// <param name="loc"></param>
        private void AddSelfIntersectionNode(int argIndex, Coordinate coord, Location loc)
        {
            // if this node is already a boundary node, don't change it
            if (IsBoundaryNode(argIndex, coord))
                return;
            if (loc == Location.Boundary && _useBoundaryDeterminationRule)
                InsertBoundaryPoint(argIndex, coord);
            else InsertPoint(argIndex, coord, loc);
        }

        // MD - experimental for now
        /// <summary>
        /// Determines the <see cref="Location"/> of the given <see cref="Coordinate"/> in this geometry.
        /// </summary>
        /// <param name="pt">The point to test</param>
        /// <returns>
        /// The location of the point in the geometry
        /// </returns>
        public Location Locate(Coordinate pt)
        {
            if (_parentGeom is IPolygonal && _parentGeom.NumGeometries > 50)
            {
                // lazily init point locator
                if (_areaPtLocator == null)
                {
                    _areaPtLocator = new IndexedPointInAreaLocator(_parentGeom);
                }
                return _areaPtLocator.Locate(pt);
            }
            return _ptLocator.Locate(pt, _parentGeom);
        }
    }
}
