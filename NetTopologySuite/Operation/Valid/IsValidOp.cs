#if !DOTNET40
#define C5
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
#if C5
using C5;
#endif
using GeoAPI.Coordinates;
#if !DOTNET40
using GeoAPI.DataStructures.Collections.Generic;
#endif
using GeoAPI.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using NPack;
using NPack.Interfaces;

namespace NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// Implements the algorithsm required to compute the <see cref="Geometry{TCoordinate}.IsValid" />
    /// method for <see cref="Geometry{TCoordinate}" />s.
    /// See the documentation for the various geometry types for a specification of validity.
    /// </summary>
    public class IsValidOp<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly Boolean _isChecked;
        private readonly IGeometry<TCoordinate> _parentGeometry; // the base Geometry to be validated

        // If the following condition is TRUE JTS will validate inverted shells and 
        // exverted holes (the ESRI SDE model).
        private Boolean _isSelfTouchingRingFormingHoleValid;
        private TopologyValidationError _validErr;

        public IsValidOp(IGeometry<TCoordinate> parentGeometry)
        {
            _parentGeometry = parentGeometry;
        }

        /// <summary>
        /// <para>
        /// Gets or sets whether polygons using Self-Touching Rings to form
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
        /// which self-touch at a single point causing the interior to be disconnected,
        /// and "C-shaped" holes which self-touch at a single point causing an island to be formed.
        /// </para>
        /// </summary>
        /// <value>States whether geometry with this condition is valid.</value>
        public Boolean IsSelfTouchingRingFormingHoleValid
        {
            get { return _isSelfTouchingRingFormingHoleValid; }
            set { _isSelfTouchingRingFormingHoleValid = value; }
        }

        public Boolean IsValid
        {
            get
            {
                checkValid(_parentGeometry);
                return _validErr == null;
            }
        }

        public TopologyValidationError ValidationError
        {
            get
            {
                checkValid(_parentGeometry);
                return _validErr;
            }
        }

        /// <summary>
        /// Checks whether a coordinate is valid for processing.
        /// Coordinates are valid iff their x and y ordinates are in the
        /// range of the floating point representation.
        /// </summary>
        /// <param name="coord">The coordinate to validate.</param>
        /// <returns><see langword="true"/> if the coordinate is valid.</returns>
        public static Boolean IsValidCoordinate(TCoordinate coord)
        {
            //DoubleComponent dx, dy;
            //coord.GetComponents(out dx, out dy);

            ////Double x = (Double) dx;
            //if (Double.IsNaN(x))
            if (Double.IsNaN(coord[Ordinates.X]))
            {
                return false;
            }

            //if (Double.IsInfinity(x))
            if (Double.IsInfinity(coord[Ordinates.X]))
            {
                return false;
            }

            //Double y = (Double) dy;
            //if (Double.IsNaN(y))
            if (Double.IsNaN(coord[Ordinates.Y]))
            {
                return false;
            }

            //if (Double.IsInfinity(y))
            if (Double.IsInfinity(coord[Ordinates.Y]))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Find a point from the list of testCoords
        /// that is NOT a node in the edge for the list of searchCoords.
        /// </summary>
        /// <returns>The point found, or <see langword="null" /> if none found.</returns>
        public static TCoordinate FindPointNotNode(IEnumerable<TCoordinate> testCoords,
                                                   ILinearRing<TCoordinate> searchRing, GeometryGraph<TCoordinate> graph)
        {
            // find edge corresponding to searchRing.
            Edge<TCoordinate> searchEdge = graph.FindEdge(searchRing);

            // find a point in the testCoords which is not a node of the searchRing
            EdgeIntersectionList<TCoordinate> eiList = searchEdge.EdgeIntersections;

            // TODO: somewhat inefficient - is there a better way? (Use a node map, for instance?)
            foreach (TCoordinate pt in testCoords)
            {
                if (!eiList.IsIntersection(pt))
                {
                    return pt;
                }
            }

            return default(TCoordinate);
        }

        private void checkValid(IGeometry<TCoordinate> g)
        {
            if (_isChecked)
            {
                return;
            }

            _validErr = null;

            if (g.IsEmpty)
            {
                return;
            }

            if (g is IPoint<TCoordinate>)
            {
                checkValid(g as IPoint<TCoordinate>);
            }
            else if (g is IMultiPoint<TCoordinate>)
            {
                checkValid(g as IMultiPoint<TCoordinate>);
            }
            else if (g is ILinearRing<TCoordinate>) // LineString also handles LinearRings
            {
                checkValid(g as ILinearRing<TCoordinate>);
            }
            else if (g is ILineString<TCoordinate>)
            {
                checkValid(g as ILineString<TCoordinate>);
            }
            else if (g is IPolygon<TCoordinate>)
            {
                checkValid(g as IPolygon<TCoordinate>);
            }
            else if (g is IMultiPolygon<TCoordinate>)
            {
                checkValid(g as IMultiPolygon<TCoordinate>);
            }
            else if (g is IGeometryCollection<TCoordinate>)
            {
                checkValid(g as IGeometryCollection<TCoordinate>);
            }
            else
            {
                throw new NotSupportedException(g.GetType().FullName);
            }
        }

        /// <summary>
        /// Checks validity of a Point.
        /// </summary>
        private void checkValid(IPoint<TCoordinate> g)
        {
            checkInvalidCoordinates(g.Coordinates);
        }

        /// <summary>
        /// Checks validity of a MultiPoint.
        /// </summary>
        private void checkValid(IMultiPoint<TCoordinate> g)
        {
            checkInvalidCoordinates(g.Coordinates);
        }

        /// <summary>
        /// Checks validity of a LineString.  
        /// Almost anything goes for lineStrings!
        /// </summary>
        private void checkValid(ILineString<TCoordinate> g)
        {
            checkInvalidCoordinates(g.Coordinates);

            if (_validErr != null)
            {
                return;
            }

            GeometryGraph<TCoordinate> graph = new GeometryGraph<TCoordinate>(0, g);
            checkTooFewPoints(graph);
        }

        /// <summary>
        /// Checks validity of a LinearRing.
        /// </summary>
        private void checkValid(ILinearRing<TCoordinate> g)
        {
            checkInvalidCoordinates(g.Coordinates);

            if (_validErr != null)
            {
                return;
            }

            checkClosedRing(g);

            if (_validErr != null)
            {
                return;
            }

            GeometryGraph<TCoordinate> graph = new GeometryGraph<TCoordinate>(0, g);
            checkTooFewPoints(graph);

            if (_validErr != null)
            {
                return;
            }

            IGeometryFactory<TCoordinate> geoFactory = g.Factory;

            if (geoFactory == null)
            {
                throw new InvalidOperationException("ILinearRing's factory is null.");
            }

            LineIntersector<TCoordinate> li
                = CGAlgorithms<TCoordinate>.CreateRobustLineIntersector(geoFactory);
            graph.ComputeSelfNodes(li, true);
            checkNoSelfIntersectingRings(graph);
        }

        /// <summary>
        /// Checks the validity of a polygon and sets the validErr flag.
        /// </summary>
        private void checkValid(IPolygon<TCoordinate> g)
        {
            checkInvalidCoordinates(g);

            if (_validErr != null)
            {
                return;
            }

            checkClosedRings(g);

            if (_validErr != null)
            {
                return;
            }

            GeometryGraph<TCoordinate> graph = new GeometryGraph<TCoordinate>(0, g);
            checkTooFewPoints(graph);

            if (_validErr != null)
            {
                return;
            }

            checkConsistentArea(graph);

            if (_validErr != null)
            {
                return;
            }

            if (!IsSelfTouchingRingFormingHoleValid)
            {
                checkNoSelfIntersectingRings(graph);

                if (_validErr != null)
                {
                    return;
                }
            }

            CheckHolesInShell(g, graph);

            if (_validErr != null)
            {
                return;
            }

            checkHolesNotNested(g, graph);

            if (_validErr != null)
            {
                return;
            }

            checkConnectedInteriors(graph);
        }

        private void checkValid(IMultiPolygon<TCoordinate> g)
        {
            foreach (IPolygon<TCoordinate> p in g)
            {
                checkInvalidCoordinates(p);

                if (_validErr != null)
                {
                    return;
                }

                checkClosedRings(p);

                if (_validErr != null)
                {
                    return;
                }
            }

            GeometryGraph<TCoordinate> graph = new GeometryGraph<TCoordinate>(0, g);
            checkTooFewPoints(graph);

            if (_validErr != null)
            {
                return;
            }

            checkConsistentArea(graph);

            if (_validErr != null)
            {
                return;
            }

            if (!IsSelfTouchingRingFormingHoleValid)
            {
                checkNoSelfIntersectingRings(graph);

                if (_validErr != null)
                {
                    return;
                }
            }

            foreach (IPolygon<TCoordinate> p in g)
            {
                CheckHolesInShell(p, graph);

                if (_validErr != null)
                {
                    return;
                }
            }

            foreach (IPolygon<TCoordinate> p in g)
            {
                checkHolesNotNested(p, graph);

                if (_validErr != null)
                {
                    return;
                }
            }

            checkShellsNotNested(g, graph);

            if (_validErr != null)
            {
                return;
            }

            checkConnectedInteriors(graph);
        }

        private void checkValid(IGeometryCollection<TCoordinate> gc)
        {
            foreach (IGeometry<TCoordinate> g in gc)
            {
                checkValid(g);

                if (_validErr != null)
                {
                    return;
                }
            }
        }

        private void checkInvalidCoordinates(IEnumerable<TCoordinate> coords)
        {
            foreach (TCoordinate c in coords)
            {
                if (!IsValidCoordinate(c))
                {
                    _validErr = new TopologyValidationError(TopologyValidationErrors.InvalidCoordinate, c);
                    return;
                }
            }
        }

        private void checkInvalidCoordinates(IPolygon<TCoordinate> poly)
        {
            checkInvalidCoordinates(poly.ExteriorRing.Coordinates);

            if (_validErr != null)
            {
                return;
            }

            foreach (ILineString<TCoordinate> ls in poly.InteriorRings)
            {
                checkInvalidCoordinates(ls.Coordinates);

                if (_validErr != null)
                {
                    return;
                }
            }
        }

        private void checkClosedRings(IPolygon<TCoordinate> poly)
        {
            checkClosedRing(poly.ExteriorRing);

            if (_validErr != null)
            {
                return;
            }

            foreach (ILinearRing<TCoordinate> hole in poly.InteriorRings)
            {
                checkClosedRing(hole);

                if (_validErr != null)
                {
                    return;
                }
            }
        }

        private void checkClosedRing(ICurve<TCoordinate> ring)
        {
            if (!ring.IsClosed)
            {
                _validErr = new TopologyValidationError(TopologyValidationErrors.RingNotClosed,
                                                       ring.Coordinates[0]);
            }
        }

        private void checkTooFewPoints(GeometryGraph<TCoordinate> graph)
        {
            if (graph.HasTooFewPoints)
            {
                _validErr = new TopologyValidationError(TopologyValidationErrors.TooFewPoints,
                                                       graph.InvalidPoint);
                return;
            }
        }

        private void checkConsistentArea(GeometryGraph<TCoordinate> graph)
        {
            ConsistentAreaTester<TCoordinate> cat = new ConsistentAreaTester<TCoordinate>(graph);
            Boolean isValidArea = cat.IsNodeConsistentArea;

            if (!isValidArea)
            {
                _validErr = new TopologyValidationError(TopologyValidationErrors.SelfIntersection, cat.InvalidPoint);
                return;
            }

            if (cat.HasDuplicateRings)
            {
                _validErr = new TopologyValidationError(TopologyValidationErrors.DuplicateRings, cat.InvalidPoint);
                return;
            }
        }

        /// <summary>
        /// Check that there is no ring which self-intersects (except of course at its endpoints).
        /// This is required by OGC topology rules (but not by other models
        /// such as ESRI SDE, which allow inverted shells and exverted holes).
        /// </summary>
        private void checkNoSelfIntersectingRings(GeometryGraph<TCoordinate> graph)
        {
            foreach (Edge<TCoordinate> e in graph.Edges)
            {
                checkNoSelfIntersectingRing(e.EdgeIntersections);

                if (_validErr != null)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Check that a ring does not self-intersect, except at its endpoints.
        /// Algorithm is to count the number of times each node along edge occurs.
        /// If any occur more than once, that must be a self-intersection.
        /// </summary>
        private void checkNoSelfIntersectingRing(EdgeIntersectionList<TCoordinate> eiList)
        {
#if C5
            TreeSet<TCoordinate> nodeSet = new TreeSet<TCoordinate>();
#else
#if DOTNET40
            ISet<TCoordinate> nodeSet = new SortedSet<TCoordinate>();
#else
            ISet<TCoordinate> nodeSet = new ListSet<TCoordinate>();
#endif
#endif
            Boolean isFirst = true;

            foreach (EdgeIntersection<TCoordinate> ei in eiList)
            {
                if (isFirst)
                {
                    isFirst = false;
                    continue;
                }

                if (nodeSet.Contains(ei.Coordinate))
                {
                    _validErr = new TopologyValidationError(TopologyValidationErrors.RingSelfIntersection, ei.Coordinate);
                    return;
                }

                nodeSet.Add(ei.Coordinate);
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
        private void CheckHolesInShell(IPolygon<TCoordinate> p, GeometryGraph<TCoordinate> graph)
        {
            ILinearRing<TCoordinate> shell = p.ExteriorRing as ILinearRing<TCoordinate>;

            IPointInRing<TCoordinate> pir = new MCPointInRing<TCoordinate>(shell);

            foreach (ILinearRing<TCoordinate> hole in p.InteriorRings)
            {
                Debug.Assert(hole != null);
                TCoordinate holePt = FindPointNotNode(hole.Coordinates, shell, graph);

                /*
                 * If no non-node hole vertex can be found, the hole must
                 * split the polygon into disconnected interiors.
                 * This will be caught by a subsequent check.
                 */
                if(typeof(TCoordinate).IsValueType)
                {
                    if (holePt.Equals(default(TCoordinate))) return;
                }
                else
                {
                    if (holePt == null) return;
                }

                Boolean outside = !pir.IsInside(holePt);

                if (outside)
                {
                    _validErr = new TopologyValidationError(TopologyValidationErrors.HoleOutsideShell, holePt);
                    return;
                }
            }
        }

        // Tests that no hole is nested inside another hole.
        // This routine assumes that the holes are disjoint.
        // To ensure this, holes have previously been tested
        // to ensure that:
        // * They do not partially overlap (checked by <c>checkRelateConsistency</c>).
        // * They are not identical (checked by <c>checkRelateConsistency</c>).
        private void checkHolesNotNested(IPolygon<TCoordinate> p, GeometryGraph<TCoordinate> graph)
        {
            if (p.Factory == null)
            {
                throw new InvalidOperationException("IPolygon has a null IGeometryFactory.");
            }

            QuadtreeNestedRingTester<TCoordinate> nestedTester
                = new QuadtreeNestedRingTester<TCoordinate>(p.Factory, graph);
            //IndexedNestedRingTester<TCoordinate> nestedTester =
            //    new IndexedNestedRingTester<TCoordinate>(p.Factory, graph);

            foreach (ILinearRing<TCoordinate> innerHole in p.InteriorRings)
            {
                nestedTester.Add(innerHole);
            }

            Boolean isNonNested = nestedTester.IsNonNested();

            if (!isNonNested)
            {
                _validErr = new TopologyValidationError(TopologyValidationErrors.NestedHoles,
                                                       nestedTester.NestedPoint);
            }
        }

        // Tests that no element polygon is wholly in the interior of another element polygon.
        // Preconditions:
        // Shells do not partially overlap.
        // Shells do not touch along an edge.
        // No duplicate rings exists.
        // This routine relies on the fact that while polygon shells may touch at one or
        // more vertices, they cannot touch at ALL vertices.
        private void checkShellsNotNested(IMultiPolygon<TCoordinate> mp, GeometryGraph<TCoordinate> graph)
        {
            foreach (IPolygon<TCoordinate> p in mp)
            {
                ILinearRing<TCoordinate> shell = p.ExteriorRing as ILinearRing<TCoordinate>;

                foreach (IPolygon<TCoordinate> p2 in mp)
                {
                    if (ReferenceEquals(p, p2))
                    {
                        continue;
                    }

                    checkShellNotNested(shell, p2, graph);

                    if (_validErr != null)
                    {
                        return;
                    }
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
        private void checkShellNotNested(ILinearRing<TCoordinate> shell, IPolygon<TCoordinate> p,
                                         GeometryGraph<TCoordinate> graph)
        {
            IEnumerable<TCoordinate> shellPts = shell.Coordinates;

            // test if shell is inside polygon shell
            ILinearRing<TCoordinate> polyShell = p.ExteriorRing as ILinearRing<TCoordinate>;
            Debug.Assert(polyShell != null);
            IEnumerable<TCoordinate> polyPts = polyShell.Coordinates;

            TCoordinate shellPt = FindPointNotNode(shellPts, polyShell, graph);

            // if no point could be found, we can assume that the shell is outside the polygon
            if (Coordinates<TCoordinate>.IsEmpty(shellPt))
            {
                return;
            }

            Boolean insidePolyShell = CGAlgorithms<TCoordinate>.IsPointInRing(shellPt, polyPts);

            if (!insidePolyShell)
            {
                return;
            }

            // if no holes, this is an error!
            if (p.InteriorRingsCount <= 0)
            {
                _validErr = new TopologyValidationError(TopologyValidationErrors.NestedShells, shellPt);
                return;
            }

            /*
             * Check if the shell is inside one of the holes.
             * This is the case if one of the calls to checkShellInsideHole
             * returns a null coordinate.
             * Otherwise, the shell is not properly contained in a hole, which is an error.
             */
            TCoordinate badNestedPt = default(TCoordinate);

            foreach (ILinearRing<TCoordinate> hole in p.InteriorRings)
            {
                badNestedPt = checkShellInsideHole(shell, hole, graph);

                if (Coordinates<TCoordinate>.IsEmpty(badNestedPt))
                {
                    return;
                }
            }

            _validErr = new TopologyValidationError(TopologyValidationErrors.NestedShells, badNestedPt);
        }

        /// <summary> 
        /// This routine checks to see if a shell is properly contained in a hole.
        /// It assumes that the edges of the shell and hole do not
        /// properly intersect.
        /// </summary>
        /// <returns>
        /// <see langword="null" /> if the shell is properly contained, or
        /// a Coordinate which is not inside the hole if it is not.
        /// </returns>
        private static TCoordinate checkShellInsideHole(ILinearRing<TCoordinate> shell, ILinearRing<TCoordinate> hole,
                                                        GeometryGraph<TCoordinate> graph)
        {
            IEnumerable<TCoordinate> shellPts = shell.Coordinates;
            IEnumerable<TCoordinate> holePts = hole.Coordinates;
            // TODO: improve performance of this - by sorting pointlists?
            TCoordinate shellPt = FindPointNotNode(shellPts, hole, graph);

            // if point is on shell but not hole, check that the shell is inside the hole
            if (!Coordinates<TCoordinate>.IsEmpty(shellPt))
            {
                Boolean insideHole = CGAlgorithms<TCoordinate>.IsPointInRing(shellPt, holePts);

                if (!insideHole)
                {
                    return shellPt;
                }
            }

            TCoordinate holePt = FindPointNotNode(holePts, shell, graph);

            // if point is on hole but not shell, check that the hole is outside the shell
            if (!Coordinates<TCoordinate>.IsEmpty(holePt))
            {
                Boolean insideShell = CGAlgorithms<TCoordinate>.IsPointInRing(holePt, shellPts);

                if (insideShell)
                {
                    return holePt;
                }

                return default(TCoordinate);
            }

            Assert.ShouldNeverReachHere("points in shell and hole appear to be equal");
            return default(TCoordinate);
        }

        private void checkConnectedInteriors(GeometryGraph<TCoordinate> graph)
        {
            ConnectedInteriorTester<TCoordinate> cit = new ConnectedInteriorTester<TCoordinate>(graph,
                                                                                                _parentGeometry.Factory);

            if (!cit.AreInteriorsConnected)
            {
                _validErr = new TopologyValidationError(TopologyValidationErrors.DisconnectedInteriors,
                                                       cit.Coordinate);
            }
        }
    }
}