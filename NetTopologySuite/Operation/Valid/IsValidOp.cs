using System;
using System.Collections;
using System.Text;

using Iesi.Collections;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Operation;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// Implements the algorithsm required to compute the <c>isValid()</c> 
    /// method for <c>Geometry</c>s.
    /// See the documentation for the various geometry types for a specification of validity.
    /// </summary>
    public class IsValidOp
    {
        /// <summary>
        /// Checks whether a coordinate is valid for processing.
        /// Coordinates are valid iff their x and y ordinates are in the
        /// range of the floating point representation.
        /// </summary>
        /// <param name="coord">The coordinate to validate.</param>
        /// <returns><c>true</c> if the coordinate is valid.</returns>
        public static bool IsValidCoordinate(Coordinate coord)
        {
            if (Double.IsNaN(coord.X))      return false;
            if (Double.IsInfinity(coord.X)) return false;
            if (Double.IsNaN(coord.Y))      return false;
            if (Double.IsInfinity(coord.Y)) return false;
            return true;
        }

        /// <summary>
        /// Find a point from the list of testCoords
        /// that is NOT a node in the edge for the list of searchCoords.
        /// </summary>
        /// <param name="testCoords"></param>
        /// <param name="searchRing"></param>
        /// <param name="graph"></param>
        /// <returns>The point found, or <c>null</c> if none found.</returns>
        public static Coordinate FindPointNotNode(Coordinate[] testCoords, LinearRing searchRing, GeometryGraph graph)
        {
            // find edge corresponding to searchRing.
            Edge searchEdge = graph.FindEdge(searchRing);
            // find a point in the testCoords which is not a node of the searchRing
            EdgeIntersectionList eiList = searchEdge.EdgeIntersectionList;
            // somewhat inefficient - is there a better way? (Use a node map, for instance?)
            for (int i = 0; i < testCoords.Length; i++)
            {
                Coordinate pt = testCoords[i];
                if (!eiList.IsIntersection(pt))
                    return pt;
            }
            return null;
        }

        private Geometry parentGeometry = null;  // the base Geometry to be validated
        private bool isSelfTouchingRingFormingHoleValid = false;
        private bool isChecked = false;
        private TopologyValidationError validErr = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentGeometry"></param>
        public IsValidOp(Geometry parentGeometry)
        {
            this.parentGeometry = parentGeometry;
        }

        /// <summary>
        /// <para>
        /// Sets whether polygons using Self-Touching Rings to form
        /// holes are reported as valid.
        /// If this flag is set, the following Self-Touching conditions
        /// are treated as being valid:
        /// - The shell ring self-touches to create a hole touching the shell.
        /// - A hole ring self-touches to create two holes touching at a point.
        /// </para>
        /// <para>
        /// The default (following the OGC SFS standard)
        /// is that this condition is not valid (<c>false</c>).
        /// </para>
        /// <para>
        /// This does not affect whether Self-Touching Rings
        /// disconnecting the polygon interior are considered valid
        /// (these are considered to be invalid under the SFS, and many other
        /// spatial models as well).
        /// This includes "bow-tie" shells,
        /// which self-touch at a single point causing the interior to
        /// be disconnected,
        /// and "C-shaped" holes which self-touch at a single point causing an island to be formed.
        /// </para>
        /// </summary>
        /// <value>States whether geometry with this condition is valid.</value>
        public bool IsSelfTouchingRingFormingHoleValid
        {
            get
            {
                return isSelfTouchingRingFormingHoleValid;
            }
            set
            {
                isSelfTouchingRingFormingHoleValid = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsValid
        {
            get
            {
                CheckValid(parentGeometry);
                return validErr == null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual TopologyValidationError ValidationError
        {
            get
            {
                CheckValid(parentGeometry);
                return validErr;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        private void CheckValid(Geometry g)
        {
            if (isChecked) return;
            validErr = null;

            if (g.IsEmpty) return;            
            
            if (g is Point) CheckValid((Point)g);            
            else if (g is MultiPoint) CheckValid((MultiPoint)g);
            // LineString also handles LinearRings
            else if (g is LinearRing) CheckValid((LinearRing)g);
            else if (g is LineString) CheckValid((LineString)g);
            else if (g is Polygon) CheckValid((Polygon)g);
            else if (g is MultiPolygon) CheckValid((MultiPolygon)g);
            else if (g is GeometryCollection) CheckValid((GeometryCollection)g);
            else throw new NotSupportedException(g.GetType().FullName);
        }

        /// <summary>
        /// Checks validity of a Point.
        /// </summary>
        /// <param name="g"></param>
        private void CheckValid(Point g)
        {
            CheckInvalidCoordinates(g.Coordinates);
        }

        /// <summary>
        /// Checks validity of a MultiPoint.
        /// </summary>
        /// <param name="g"></param>
        private void CheckValid(MultiPoint g)
        {
            CheckInvalidCoordinates(g.Coordinates);
        }

        /// <summary>
        /// Checks validity of a LineString.  
        /// Almost anything goes for lineStrings!
        /// </summary>
        /// <param name="g"></param>
        private void CheckValid(LineString g)
        {
            CheckInvalidCoordinates(g.Coordinates);
            if (validErr != null) return;
            GeometryGraph graph = new GeometryGraph(0, g);
            CheckTooFewPoints(graph);
        }

        /// <summary>
        /// Checks validity of a LinearRing.
        /// </summary>
        /// <param name="g"></param>
        private void CheckValid(LinearRing g)
        {
            CheckInvalidCoordinates(g.Coordinates);
            if (validErr != null) return;
            CheckClosedRing(g);
            if (validErr != null) return;

            GeometryGraph graph = new GeometryGraph(0, g);
            CheckTooFewPoints(graph);
            if (validErr != null) return;
            LineIntersector li = new RobustLineIntersector();
            graph.ComputeSelfNodes(li, true);
            CheckNoSelfIntersectingRings(graph);
        }

        /// <summary>
        /// Checks the validity of a polygon and Sets the validErr flag.
        /// </summary>
        /// <param name="g"></param>
        private void CheckValid(Polygon g)
        {
            CheckInvalidCoordinates(g);
            if (validErr != null) return;
            CheckClosedRings(g);
            if (validErr != null) return;

            GeometryGraph graph = new GeometryGraph(0, g);
            CheckTooFewPoints(graph);
            if (validErr != null) return;
            CheckConsistentArea(graph);
            if (validErr != null) return;
            if (!IsSelfTouchingRingFormingHoleValid)
            {
                CheckNoSelfIntersectingRings(graph);
                if (validErr != null) return;
            }
            CheckHolesInShell(g, graph);
            if (validErr != null) return;
            CheckHolesNotNested(g, graph);
            if (validErr != null) return;
            CheckConnectedInteriors(graph);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        private void CheckValid(MultiPolygon g)
        {
            for (int i = 0; i < g.NumGeometries; i++)
            {                                
                Polygon p = (Polygon)g.GetGeometryN(i);
                CheckInvalidCoordinates(p);
                if (validErr != null) return;
                CheckClosedRings(p);
                if (validErr != null) return;
            }

            GeometryGraph graph = new GeometryGraph(0, g);
            CheckTooFewPoints(graph);
            if (validErr != null) return;
            CheckConsistentArea(graph);
            if (validErr != null) return;
            if (!IsSelfTouchingRingFormingHoleValid)
            {
                CheckNoSelfIntersectingRings(graph);
                if (validErr != null) return;
            }
            for (int i = 0; i < g.NumGeometries; i++)
            {
                Polygon p = (Polygon)g.GetGeometryN(i);
                CheckHolesInShell(p, graph);
                if (validErr != null) return;
            }
            for (int i = 0; i < g.NumGeometries; i++)
            {                
                Polygon p = (Polygon)g.GetGeometryN(i);
                CheckHolesNotNested(p, graph);
                if (validErr != null) return;
            }
            CheckShellsNotNested(g, graph);
            if (validErr != null) return;
            CheckConnectedInteriors(graph);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gc"></param>
        private void CheckValid(GeometryCollection gc)
        {
            for (int i = 0; i < gc.NumGeometries; i++)
            {
                Geometry g = gc.GetGeometryN(i);
                CheckValid(g);
                if (validErr != null) return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coords"></param>
        private void CheckInvalidCoordinates(Coordinate[] coords)
        {
            for (int i = 0; i < coords.Length; i++)
            {
                if (!IsValidCoordinate(coords[i]))
                {
                    validErr = new TopologyValidationError(TopologyValidationErrors.InvalidCoordinate, coords[i]);
                    return;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="poly"></param>
        private void CheckInvalidCoordinates(Polygon poly)
        {
            CheckInvalidCoordinates(poly.ExteriorRing.Coordinates);
            if (validErr != null) return;
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                CheckInvalidCoordinates(poly.GetInteriorRingN(i).Coordinates);
                if (validErr != null) return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="poly"></param>
        private void CheckClosedRings(Polygon poly)
        {
            CheckClosedRing((LinearRing)poly.ExteriorRing);
            if (validErr != null) return;
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                CheckClosedRing((LinearRing)poly.GetInteriorRingN(i));
                if (validErr != null) return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ring"></param>
        private void CheckClosedRing(LinearRing ring)
        {
            if (!ring.IsClosed)
                validErr = new TopologyValidationError(TopologyValidationErrors.RingNotClosed, ring.GetCoordinateN(0));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph"></param>
        private void CheckTooFewPoints(GeometryGraph graph)
        {
            if (graph.HasTooFewPoints)
            {
                validErr = new TopologyValidationError(TopologyValidationErrors.TooFewPoints, graph.InvalidPoint);
                return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph"></param>
        private void CheckConsistentArea(GeometryGraph graph)
        {
            ConsistentAreaTester cat = new ConsistentAreaTester(graph);
            bool isValidArea = cat.IsNodeConsistentArea;
            if (!isValidArea)
            {
                validErr = new TopologyValidationError(TopologyValidationErrors.SelfIntersection, cat.InvalidPoint);
                return;
            }
            if (cat.HasDuplicateRings)
            {
                validErr = new TopologyValidationError(TopologyValidationErrors.DuplicateRings, cat.InvalidPoint);
                return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph"></param>
        private void CheckNoSelfIntersectingRings(GeometryGraph graph)
        {
            for (IEnumerator i = graph.GetEdgeEnumerator(); i.MoveNext(); )
            {
                Edge e = (Edge)i.Current;
                CheckNoSelfIntersectingRing(e.EdgeIntersectionList);
                if (validErr != null) return;
            }
        }

        /// <summary>
        /// Check that a ring does not self-intersect, except at its endpoints.
        /// Algorithm is to count the number of times each node along edge occurs.
        /// If any occur more than once, that must be a self-intersection.
        /// </summary>
        private void CheckNoSelfIntersectingRing(EdgeIntersectionList eiList)
        {
            ISet nodeSet = new ListSet();    
            bool isFirst = true;
            for (IEnumerator i = eiList.GetEnumerator(); i.MoveNext(); )
            {
                EdgeIntersection ei = (EdgeIntersection)i.Current;
                if (isFirst)
                {
                    isFirst = false;
                    continue;
                }
                if (nodeSet.Contains(ei.Coordinate))
                {
                    validErr = new TopologyValidationError(TopologyValidationErrors.RingSelfIntersection, ei.Coordinate);
                    return;
                }
                else nodeSet.Add(ei.Coordinate);
            }
        }

        /// <summary>
        /// Tests that each hole is inside the polygon shell.
        /// This routine assumes that the holes have previously been tested
        /// to ensure that all vertices lie on the shell or inside it.
        /// A simple test of a single point in the hole can be used,
        /// provide the point is chosen such that it does not lie on the
        /// boundary of the shell.
        /// </summary>
        /// <param name="p">The polygon to be tested for hole inclusion.</param>
        /// <param name="graph">A GeometryGraph incorporating the polygon.</param>
        private void CheckHolesInShell(Polygon p, GeometryGraph graph)
        {            
            LinearRing shell = (LinearRing)p.ExteriorRing;

            IPointInRing pir = new MCPointInRing(shell);
            for (int i = 0; i < p.NumInteriorRings; i++)
            {
                LinearRing hole = (LinearRing)p.GetInteriorRingN(i);
                Coordinate holePt = FindPointNotNode(hole.Coordinates, shell, graph);
                /**
                 * If no non-node hole vertex can be found, the hole must
                 * split the polygon into disconnected interiors.
                 * This will be caught by a subsequent check.
                 */
                if (holePt == null) 
                    return;
                
                bool outside = !pir.IsInside(holePt);
                if (outside)
                {
                    validErr = new TopologyValidationError(TopologyValidationErrors.HoleOutsideShell, holePt);
                    return;
                }
            }
        }

        /// <summary>
        /// Tests that no hole is nested inside another hole.
        /// This routine assumes that the holes are disjoint.
        /// To ensure this, holes have previously been tested
        /// to ensure that:
        /// They do not partially overlap
        /// (checked by <c>checkRelateConsistency</c>).
        /// They are not identical
        /// (checked by <c>checkRelateConsistency</c>).
        /// </summary>
        private void CheckHolesNotNested(Polygon p, GeometryGraph graph)
        {
            QuadtreeNestedRingTester nestedTester = new QuadtreeNestedRingTester(graph);
            for (int i = 0; i < p.NumInteriorRings; i++)
            {
                LinearRing innerHole = (LinearRing)p.GetInteriorRingN(i);
                nestedTester.Add(innerHole);
            }
            bool isNonNested = nestedTester.IsNonNested();
            if (!isNonNested)
                validErr = new TopologyValidationError(TopologyValidationErrors.NestedHoles, nestedTester.NestedPoint);        
        }

        /// <summary>
        /// Tests that no element polygon is wholly in the interior of another element polygon.
        /// Preconditions:
        /// Shells do not partially overlap.
        /// Shells do not touch along an edge.
        /// No duplicate rings exists.
        /// This routine relies on the fact that while polygon shells may touch at one or
        /// more vertices, they cannot touch at ALL vertices.
        /// </summary>
        private void CheckShellsNotNested(MultiPolygon mp, GeometryGraph graph)
        {            
            for (int i = 0; i < mp.NumGeometries; i++)
            {                
                Polygon p = (Polygon)mp.GetGeometryN(i);
                LinearRing shell = (LinearRing)p.ExteriorRing;
                for (int j = 0; j < mp.NumGeometries; j++)
                {                    
                    if (i == j)
                        continue;                    
                    Polygon p2 = (Polygon)mp.GetGeometryN(j);                    
                    CheckShellNotNested(shell, p2, graph);
                    if (validErr != null) return;
                }                
            }         
        }

        /// <summary>
        /// Check if a shell is incorrectly nested within a polygon.  This is the case
        /// if the shell is inside the polygon shell, but not inside a polygon hole.
        /// (If the shell is inside a polygon hole, the nesting is valid.)
        /// The algorithm used relies on the fact that the rings must be properly contained.
        /// E.g. they cannot partially overlap (this has been previously checked by
        /// <c>CheckRelateConsistency</c>).
        /// </summary>
        private void CheckShellNotNested(LinearRing shell, Polygon p, GeometryGraph graph)
        {
            Coordinate[] shellPts = shell.Coordinates;
            // test if shell is inside polygon shell
            LinearRing polyShell = (LinearRing)p.ExteriorRing;
            Coordinate[] polyPts = polyShell.Coordinates;
            Coordinate shellPt = FindPointNotNode(shellPts, polyShell, graph);
            // if no point could be found, we can assume that the shell is outside the polygon
            if (shellPt == null) return;
            bool insidePolyShell = CGAlgorithms.IsPointInRing(shellPt, polyPts);
            if (!insidePolyShell) return;
            // if no holes, this is an error!
            if (p.NumInteriorRings <= 0)
            {
                validErr = new TopologyValidationError(TopologyValidationErrors.NestedShells, shellPt);
                return;
            }

            /*
             * Check if the shell is inside one of the holes.
             * This is the case if one of the calls to checkShellInsideHole
             * returns a null coordinate.
             * Otherwise, the shell is not properly contained in a hole, which is an error.
             */
            Coordinate badNestedPt = null;
            for (int i = 0; i < p.NumInteriorRings; i++)
            {
                LinearRing hole = (LinearRing)p.GetInteriorRingN(i);
                badNestedPt = CheckShellInsideHole(shell, hole, graph);
                if (badNestedPt == null) return;
            }
            validErr = new TopologyValidationError(TopologyValidationErrors.NestedShells, badNestedPt);
        }

        /// <summary> 
        /// This routine checks to see if a shell is properly contained in a hole.
        /// It assumes that the edges of the shell and hole do not
        /// properly intersect.
        /// </summary>
        /// <param name="shell"></param>
        /// <param name="hole"></param>
        /// <param name="graph"></param>
        /// <returns>
        /// <c>null</c> if the shell is properly contained, or
        /// a Coordinate which is not inside the hole if it is not.
        /// </returns>
        private Coordinate CheckShellInsideHole(LinearRing shell, LinearRing hole, GeometryGraph graph)
        {
            Coordinate[] shellPts = shell.Coordinates;
            Coordinate[] holePts = hole.Coordinates;
            // TODO: improve performance of this - by sorting pointlists?
            Coordinate shellPt = FindPointNotNode(shellPts, hole, graph);
            // if point is on shell but not hole, check that the shell is inside the hole
            if (shellPt != null)
            {
                bool insideHole = CGAlgorithms.IsPointInRing(shellPt, holePts);
                if (!insideHole) return shellPt;                
            }
            Coordinate holePt = FindPointNotNode(holePts, shell, graph);
            // if point is on hole but not shell, check that the hole is outside the shell
            if (holePt != null)
            {
                bool insideShell = CGAlgorithms.IsPointInRing(holePt, shellPts);
                if (insideShell) 
                    return holePt;
                return null;
            }
            Assert.ShouldNeverReachHere("points in shell and hole appear to be equal");
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph"></param>
        private void CheckConnectedInteriors(GeometryGraph graph)
        {
            ConnectedInteriorTester cit = new ConnectedInteriorTester(graph);
            if (!cit.IsInteriorsConnected())
                validErr = new TopologyValidationError(TopologyValidationErrors.DisconnectedInteriors, cit.Coordinate);
        }
    }
}
