using System;
using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph.Index;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
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
        public static bool IsInBoundary(int boundaryCount)
        {
            // the "Mod-2 Rule"
            return boundaryCount % 2 == 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="boundaryCount"></param>
        /// <returns></returns>
        public static Locations DetermineBoundary(int boundaryCount)
        {
            return IsInBoundary(boundaryCount) ? Locations.Boundary : Locations.Interior;
        }

        private IGeometry parentGeom;

        /// <summary>
        /// The lineEdgeMap is a map of the linestring components of the
        /// parentGeometry to the edges which are derived from them.
        /// This is used to efficiently perform findEdge queries
        /// </summary>
        private IDictionary lineEdgeMap = new Hashtable();

        /// <summary>
        /// If this flag is true, the Boundary Determination Rule will used when deciding
        /// whether nodes are in the boundary or not
        /// </summary>
        private bool useBoundaryDeterminationRule = false;

        private int argIndex;  // the index of this point as an argument to a spatial function (used for labelling)
        private ICollection boundaryNodes;
        private bool hasTooFewPoints = false;
        private ICoordinate invalidPoint = null;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private EdgeSetIntersector CreateEdgeSetIntersector()
        {
            // various options for computing intersections, from slowest to fastest                    
            return new SimpleMCSweepLineIntersector();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="argIndex"></param>
        /// <param name="parentGeom"></param>
        public GeometryGraph(int argIndex, IGeometry parentGeom)
        {
            this.argIndex = argIndex;
            this.parentGeom = parentGeom;
            if (parentGeom != null)                    
                Add(parentGeom);
            
        }

        /// <summary>
        /// 
        /// </summary>
        public  bool HasTooFewPoints
        {
            get
            {
                return hasTooFewPoints;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICoordinate InvalidPoint
        {
            get
            {
                return invalidPoint;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IGeometry Geometry
        {
            get
            {
                return parentGeom;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICollection BoundaryNodes
        {
            get
            {
                if (boundaryNodes == null)
                    boundaryNodes = nodes.GetBoundaryNodes(argIndex);
                return boundaryNodes;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ICoordinate[] GetBoundaryPoints()
        {
            ICollection coll = BoundaryNodes;
            ICoordinate[] pts = new ICoordinate[coll.Count];
            int i = 0;
            for (IEnumerator it = coll.GetEnumerator(); it.MoveNext(); ) 
            {
                Node node = (Node) it.Current;
                pts[i++] = (ICoordinate) node.Coordinate.Clone();
            }
            return pts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public Edge FindEdge(ILineString line)
        {
            return (Edge) lineEdgeMap[line];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edgelist"></param>
        public void ComputeSplitEdges(IList edgelist)
        {
            for (IEnumerator i = edges.GetEnumerator(); i.MoveNext(); ) 
            {                
                Edge e = (Edge) i.Current;                
                e.EdgeIntersectionList.AddSplitEdges(edgelist);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        private void Add(IGeometry g)
        {
            if (g.IsEmpty)
                return;

            // check if this Geometry should obey the Boundary Determination Rule
            // all collections except MultiPolygons obey the rule
            if (g is IGeometryCollection && !(g is IMultiPolygon))
                useBoundaryDeterminationRule = true;

            if (g is IPolygon)                 
                AddPolygon((IPolygon) g);                                
            // LineString also handles LinearRings
            else if (g is ILineString)         
                AddLineString((ILineString) g);
            else if (g is IPoint) 
                AddPoint((IPoint) g);
            else if (g is IMultiPoint)
                AddCollection((IMultiPoint) g);
            else if (g is IMultiLineString) 
                AddCollection((IMultiLineString) g);
            else if (g is IMultiPolygon)
                AddCollection((IMultiPolygon) g);
            else if (g is IGeometryCollection) 
                AddCollection((IGeometryCollection) g);
            else  
                throw new NotSupportedException(g.GetType().FullName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gc"></param>
        private void AddCollection(IGeometryCollection gc)
        {
            for (int i = 0; i < gc.NumGeometries; i++) 
            {
                IGeometry g = gc.GetGeometryN(i);
                Add(g);
            }
        }

        /// <summary> 
        /// Add a Point to the graph.
        /// </summary>
        /// <param name="p"></param>
        private void AddPoint(IPoint p)
        {
            ICoordinate coord = p.Coordinate;
            InsertPoint(argIndex, coord, Locations.Interior);
        }

        /// <summary> 
        /// The left and right topological location arguments assume that the ring is oriented CW.
        /// If the ring is in the opposite orientation,
        /// the left and right locations must be interchanged.
        /// </summary>
        /// <param name="lr"></param>
        /// <param name="cwLeft"></param>
        /// <param name="cwRight"></param>
        private void AddPolygonRing(ILinearRing lr, Locations cwLeft, Locations cwRight)
        {
            ICoordinate[] coord = CoordinateArrays.RemoveRepeatedPoints(lr.Coordinates);
            if (coord.Length < 4) 
            {
                hasTooFewPoints = true;
                invalidPoint = coord[0];
                return;
            }
            Locations left = cwLeft;
            Locations right = cwRight;
            if (CGAlgorithms.IsCCW(coord)) 
            {
                left = cwRight;
                right = cwLeft;
            }
            Edge e = new Edge(coord, new Label(argIndex, Locations.Boundary, left, right));
            if (lineEdgeMap.Contains(lr))
                lineEdgeMap.Remove(lr);
            lineEdgeMap.Add(lr, e);
            InsertEdge(e);
            // insert the endpoint as a node, to mark that it is on the boundary
            InsertPoint(argIndex, coord[0], Locations.Boundary);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        private void AddPolygon(IPolygon p)
        {
            AddPolygonRing(p.Shell, Locations.Exterior, Locations.Interior);

            for (int i = 0; i < p.NumInteriorRings; i++)             
                // Holes are topologically labelled opposite to the shell, since
                // the interior of the polygon lies on their opposite side
                // (on the left, if the hole is oriented CW)
                AddPolygonRing(p.Holes[i], Locations.Interior, Locations.Exterior);            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        private void AddLineString(ILineString line)
        {
            ICoordinate[] coord = CoordinateArrays.RemoveRepeatedPoints(line.Coordinates);
            if (coord.Length < 2) 
            {
                hasTooFewPoints = true;
                invalidPoint = coord[0];
                return;
            }

            // add the edge for the LineString
            // line edges do not have locations for their left and right sides
            Edge e = new Edge(coord, new Label(argIndex, Locations.Interior));
            if (lineEdgeMap.Contains(line))
                lineEdgeMap.Remove(line);
            lineEdgeMap.Add(line, e);            
            InsertEdge(e);

            /*
            * Add the boundary points of the LineString, if any.
            * Even if the LineString is closed, add both points as if they were endpoints.
            * This allows for the case that the node already exists and is a boundary point.
            */
            Assert.IsTrue(coord.Length >= 2, "found LineString with single point");
            InsertBoundaryPoint(argIndex, coord[0]);
            InsertBoundaryPoint(argIndex, coord[coord.Length - 1]);
        }

        /// <summary> 
        /// Add an Edge computed externally.  The label on the Edge is assumed
        /// to be correct.
        /// </summary>
        /// <param name="e"></param>
        public void AddEdge(Edge e)
        {
            InsertEdge(e);
            ICoordinate[] coord = e.Coordinates;
            // insert the endpoint as a node, to mark that it is on the boundary
            InsertPoint(argIndex, coord[0], Locations.Boundary);
            InsertPoint(argIndex, coord[coord.Length - 1], Locations.Boundary);
        }

        /// <summary>
        /// Add a point computed externally.  The point is assumed to be a
        /// Point Geometry part, which has a location of INTERIOR.
        /// </summary>
        /// <param name="pt"></param>
        public void AddPoint(ICoordinate pt)
        {
            InsertPoint(argIndex, pt, Locations.Interior);
        }

        /// <summary>
        /// Compute self-nodes, taking advantage of the Geometry type to
        /// minimize the number of intersection tests.  (E.g. rings are
        /// not tested for self-intersection, since they are assumed to be valid).
        /// </summary>
        /// <param name="li">The <c>LineIntersector</c> to use.</param>
        /// <param name="computeRingSelfNodes">If <c>false</c>, intersection checks are optimized to not test rings for self-intersection.</param>
        /// <returns>The SegmentIntersector used, containing information about the intersections found.</returns>
        public SegmentIntersector ComputeSelfNodes(LineIntersector li, bool computeRingSelfNodes)
        {
            SegmentIntersector si = new SegmentIntersector(li, true, false);
            EdgeSetIntersector esi = CreateEdgeSetIntersector();
            // optimized test for Polygons and Rings
            if (!computeRingSelfNodes &&
               (parentGeom is ILinearRing || parentGeom is IPolygon || parentGeom is IMultiPolygon))
                 esi.ComputeIntersections(edges, si, false);            
            else esi.ComputeIntersections(edges, si, true);      
            AddSelfIntersectionNodes(argIndex);
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
            SegmentIntersector si = new SegmentIntersector(li, includeProper, true);
            si.SetBoundaryNodes(BoundaryNodes, g.BoundaryNodes);
            EdgeSetIntersector esi = CreateEdgeSetIntersector();
            esi.ComputeIntersections(edges, g.edges, si);        
            return si;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="argIndex"></param>
        /// <param name="coord"></param>
        /// <param name="onLocation"></param>
        private void InsertPoint(int argIndex, ICoordinate coord, Locations onLocation)
        {
            Node n = nodes.AddNode((Coordinate) coord);
            Label lbl = n.Label;
            if (lbl == null) 
                 n.Label = new Label(argIndex, onLocation);            
            else lbl.SetLocation(argIndex, onLocation);
        }

        /// <summary> 
        /// Adds points using the mod-2 rule of SFS.  This is used to add the boundary
        /// points of dim-1 geometries (Curves/MultiCurves).  According to the SFS,
        /// an endpoint of a Curve is on the boundary
        /// if it is in the boundaries of an odd number of Geometries.
        /// </summary>
        /// <param name="argIndex"></param>
        /// <param name="coord"></param>
        private void InsertBoundaryPoint(int argIndex, ICoordinate coord)
        {
            Node n = nodes.AddNode((Coordinate) coord);
            Label lbl = n.Label;
            // the new point to insert is on a boundary
            int boundaryCount = 1;
            // determine the current location for the point (if any)
            Locations loc = Locations.Null;
            if (lbl != null) 
                loc = lbl.GetLocation(argIndex, Positions.On);
            if (loc == Locations.Boundary) 
                boundaryCount++;

            // determine the boundary status of the point according to the Boundary Determination Rule
            Locations newLoc = DetermineBoundary(boundaryCount);
            lbl.SetLocation(argIndex, newLoc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="argIndex"></param>
        private void AddSelfIntersectionNodes(int argIndex)
        {
            for (IEnumerator i = edges.GetEnumerator(); i.MoveNext(); ) 
            {
                Edge e = (Edge) i.Current;
                Locations eLoc = e.Label.GetLocation(argIndex);
                for (IEnumerator eiIt = e.EdgeIntersectionList.GetEnumerator(); eiIt.MoveNext(); ) 
                {
                    EdgeIntersection ei = (EdgeIntersection) eiIt.Current;
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
        private void AddSelfIntersectionNode(int argIndex, ICoordinate coord, Locations loc)
        {
            // if this node is already a boundary node, don't change it
            if (IsBoundaryNode(argIndex, coord)) 
                return;
            if (loc == Locations.Boundary && useBoundaryDeterminationRule)
                 InsertBoundaryPoint(argIndex, coord);
            else InsertPoint(argIndex, coord, loc);
        }
    }
}
