using System;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;

namespace NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// Implements the algorithms required to compute the <see cref="Geometry.IsValid" />
    /// method for <see cref="Geometry" />s.
    /// See the documentation for the various geometry types for a specification of validity.
    /// </summary>
    public class IsValidOp
    {
        private const int MinSizeLineString = 2;
        private const int MinSizeLinearRing = 4;

        /// <summary>
        /// Tests whether a <see cref="Geometry"/> is valid.
        /// </summary>
        /// <param name="geom">The geometry to test</param>
        /// <returns><c>true</c> if the geometry is valid</returns>
        /// <remarks>In JTS this function is called <c>IsValid</c></remarks>
        public static bool CheckValid(Geometry geom)
        {
            var isValidOp = new IsValidOp(geom);
            return isValidOp.IsValid;
        }

        /// <summary>
        /// Checks whether a coordinate is valid for processing.
        /// Coordinates are valid if their x and y ordinates are in the
        /// range of the floating point representation.
        /// </summary>
        /// <param name="coord">The coordinate to validate</param>
        /// <returns><c>true</c> if the coordinate is valid</returns>
        [Obsolete("Use Coordinate.IsValid")]
        public static bool IsValidCoordinate(Coordinate coord)
        {
            return coord.IsValid;
            //if (double.IsNaN(coord.X)) return false;
            //if (double.IsInfinity(coord.X)) return false;
            //if (double.IsNaN(coord.Y)) return false;
            //if (double.IsInfinity(coord.Y)) return false;
            //return true;
        }

        ///// <summary>
        ///// Checks whether a coordinate is valid for processing.
        ///// Coordinates are valid if their x and y ordinates are in the
        ///// range of the floating point representation.
        ///// </summary>
        ///// <param name="coord">The coordinate to validate</param>
        ///// <returns><c>true</c> if the coordinate is valid</returns>
        //[Obsolete("Use Coordinate.IsValid")]
        //public static bool IsValid(Coordinate coord)
        //{
        //    return coord.IsValid;
        //    //if (double.IsNaN(coord.X)) return false;
        //    //if (double.IsInfinity(coord.X)) return false;
        //    //if (double.IsNaN(coord.Y)) return false;
        //    //if (double.IsInfinity(coord.Y)) return false;
        //    //return true;
        //}

        /// <summary>
        /// The geometry being validated
        /// </summary>
        private readonly Geometry _inputGeometry;

        /// <summary>
        /// If the following condition is TRUE JTS will validate inverted shells and exverted holes
        /// (the ESRI SDE model)
        /// </summary>
        private bool _isInvertedRingValid;

        private TopologyValidationError _validErr;

        /// <summary>
        /// Creates a new validator for a geometry
        /// </summary>
        /// <param name="inputGeometry">The geometry to validate</param>
        public IsValidOp(Geometry inputGeometry)
        {
            _inputGeometry = inputGeometry;
        }


        /// <summary>
        /// Gets or sets a value indicating whether polygons using <b>Self-Touching Rings</b> to form
        /// holes are reported as valid.
        /// If this flag is set, the following Self-Touching conditions
        /// are treated as being valid:
        /// <list type="bullet">
        /// <item><description>the shell ring self-touches to create a hole touching the shell</description></item>
        /// <item><description>a hole ring self-touches to create two holes touching at a point</description></item>
        /// </list>
        /// <para/>
        /// The default (following the OGC SFS standard)
        /// is that this condition is <b>not</b> valid (<c>false</c>).
        /// <para/>
        /// Self-Touching Rings which disconnect the
        /// the polygon interior are still considered to be invalid
        /// (these are <b>invalid</b> under the SFS, and many other
        /// spatial models as well).
        /// This includes:
        /// <list type="bullet">
        /// <item><description>exverted ("bow-tie") shells which self-touch at a single point</description></item>
        /// <item><description>inverted shells with the inversion touching the shell at another point</description></item>
        /// <item><description>exverted holes with exversion touching the hole at another point</description></item>
        /// <item><description>inverted ("C-shaped") holes which self-touch at a single point causing an island to be formed</description></item>
        /// <item><description>inverted shells or exverted holes which form part of a chain of touching rings
        /// (which disconnect the interior)</description></item>
        /// </list>
        /// </summary>
        public bool SelfTouchingRingFormingHoleValid
        {
            get => _isInvertedRingValid;
            set => _isInvertedRingValid = value;
        }

        /// <summary>
        /// <para>
        /// Gets/Sets whether polygons using Self-Touching Rings to form
        /// holes are reported as valid.
        /// If this flag is set, the following Self-Touching conditions
        /// are treated as being valid:<br/>
        /// - The shell ring self-touches to create a hole touching the shell.<br/>
        /// - A hole ring self-touches to create two holes touching at a point.<br/>
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
        [Obsolete("Use SelfTouchingRingFormingHoleValid")]
        public bool IsSelfTouchingRingFormingHoleValid
        {
            get => SelfTouchingRingFormingHoleValid;
            set => SelfTouchingRingFormingHoleValid = value;
        }

        /// <summary>
        /// Tests the validity of the input geometry.
        /// </summary>
        /// <returns><c>true</c> if the geometry is valid.</returns>
        public bool IsValid
        {
            get => IsValidGeometry(_inputGeometry);
        }

        /// <summary>
        /// Gets a value indicating the validity of the geometry
        /// If not valid, returns the validation error for the geometry,
        /// or <c>null</c> if the geometry is valid.
        /// </summary>
        /// <returns>The validation error, if the geometry is invalid
        /// or null if the geometry is valid</returns>
        public TopologyValidationError ValidationError
        {
            get
            {
                IsValidGeometry(_inputGeometry);
                return _validErr;
            }
        }

        private void LogInvalid(TopologyValidationErrors code, Coordinate pt)
        {
            _validErr = new TopologyValidationError(code, pt);
        }

        private bool HasInvalidError
        {
            get => _validErr != null;

        }

        private bool IsValidGeometry(Geometry g)
        {
            _validErr = null;

            // empty geometries are always valid
            if (g.IsEmpty) return true;

            switch (g)
            {
                case Point pt:
                    return IsValidGeometry(pt);
                case MultiPoint mpt:
                    return IsValidGeometry(mpt);
                case LinearRing lr:
                    return IsValidGeometry(lr);
                case LineString ls:
                    return IsValidGeometry(ls);
                case Polygon pl:
                    return IsValidGeometry(pl);
                case MultiPolygon mpl:
                    return IsValidGeometry(mpl);
                case GeometryCollection gc:
                    return IsValidGeometry(gc);
                default:
                    // geometry type not known
                    throw new NotSupportedException(g.GetType().Name);
            }
        }

        /// <summary>
        /// Tests validity of a <c>Point</c>.
        /// </summary>
        /// <param name="g">The <c>Point</c> to test</param>
        /// <returns><c>true</c> if the <c>Point</c> is valid</returns>
        /// <remarks>In JTS this function is called <c>IsValid</c></remarks>
        private bool IsValidGeometry(Point g)
        {
            CheckCoordinateInvalid(g.CoordinateSequence);
            if (HasInvalidError) return false;
            return true;
        }

        /// <summary>
        /// Tests validity of a <c>MultiPoint</c>.
        /// </summary>
        /// <param name="g">The <c>MultiPoint</c> to test</param>
        /// <returns><c>true</c> if the <c>MultiPoint</c> is valid</returns>
        /// <remarks>In JTS this function is called <c>IsValid</c></remarks>
        private bool IsValidGeometry(MultiPoint g)
        {
            CheckCoordinateInvalid(g.Coordinates);
            if (HasInvalidError) return false;
            return true;
        }

        /// <summary>
        /// Tests validity of a <c>LineString</c>.<br/>
        /// Almost anything goes for <c>LineString</c>s!
        /// </summary>
        /// <param name="g">The <c>LineString</c> to test</param>
        /// <remarks>In JTS this function is called <c>IsValid</c></remarks>
        private bool IsValidGeometry(LineString g)
        {
            CheckCoordinateInvalid(g.CoordinateSequence);
            if (HasInvalidError) return false;
            CheckTooFewPoints(g, MinSizeLineString);
            if (HasInvalidError) return false;
            return true;
        }

        /// <summary>
        /// Tests validity of a <c>LinearRing</c>.<br/>
        /// </summary>
        /// <param name="g">The <c>LinearRing</c> to test</param>
        /// <remarks>In JTS this function is called <c>IsValid</c></remarks>
        private bool IsValidGeometry(LinearRing g)
        {
            CheckCoordinateInvalid(g.CoordinateSequence);
            if (HasInvalidError) return false;

            CheckRingNotClosed(g);
            if (HasInvalidError) return false;

            CheckRingTooFewPoints(g);
            if (HasInvalidError) return false;

            CheckSelfIntersectingRing(g);
            return _validErr == null;
        }

        /// <summary>
        /// Tests validity of a <c>Polygon</c>.<br/>
        /// </summary>
        /// <param name="g">The <c>Polygon</c> to test</param>
        /// <remarks>In JTS this function is called <c>IsValid</c></remarks>
        private bool IsValidGeometry(Polygon g)
        {
            CheckCoordinateInvalid(g);
            if (HasInvalidError) return false;

            CheckRingsNotClosed(g);
            if (HasInvalidError) return false;

            CheckRingsTooFewPoints(g);
            if (HasInvalidError) return false;

            var areaAnalyzer = new AreaTopologyAnalyzer(g, _isInvertedRingValid);

            CheckAreaIntersections(areaAnalyzer);
            if (HasInvalidError) return false;

            CheckHolesOutsideShell(g);
            if (HasInvalidError) return false;

            CheckHolesNotNested(g);
            if (HasInvalidError) return false;

            CheckInteriorDisconnected(areaAnalyzer);
            if (HasInvalidError) return false;

            return true;
        }

        /// <summary>
        /// Tests validity of a <c>MultiPolygon</c>.<br/>
        /// </summary>
        /// <param name="g">The <c>MultiPolygon</c> to test</param>
        /// <remarks>In JTS this function is called <c>IsValid</c></remarks>
        private bool IsValidGeometry(MultiPolygon g)
        {
            for (int i = 0; i < g.NumGeometries; i++)
            {
                var p = (Polygon) g.GetGeometryN(i);
                CheckCoordinateInvalid(p);
                if (HasInvalidError) return false;

                CheckRingsNotClosed(p);
                if (HasInvalidError) return false;
                CheckRingsTooFewPoints(p);
                if (HasInvalidError) return false;
            }

            var areaAnalyzer = new AreaTopologyAnalyzer(g, _isInvertedRingValid);

            CheckAreaIntersections(areaAnalyzer);
            if (HasInvalidError) return false;

            for (int i = 0; i < g.NumGeometries; i++)
            {
                var p = (Polygon) g.GetGeometryN(i);
                CheckHolesOutsideShell(p);
                if (HasInvalidError) return false;
            }

            for (int i = 0; i < g.NumGeometries; i++)
            {
                var p = (Polygon) g.GetGeometryN(i);
                CheckHolesNotNested(p);
                if (HasInvalidError) return false;
            }

            CheckShellsNotNested(g);
            if (HasInvalidError) return false;

            CheckInteriorDisconnected(areaAnalyzer);
            if (HasInvalidError) return false;

            return true;
        }

        /// <summary>
        /// Tests validity of a <c>GeometryCollection</c>.<br/>
        /// </summary>
        /// <param name="gc">The <c>GeometryCollection</c> to test</param>
        /// <remarks>In JTS this function is called <c>IsValid</c></remarks>
        private bool IsValidGeometry(GeometryCollection gc)
        {
            for (int i = 0; i < gc.NumGeometries; i++)
            {
                if (!IsValidGeometry(gc.GetGeometryN(i)))
                    return false;
            }

            return true;
        }

        private void CheckCoordinateInvalid(Coordinate[] coords)
        {
            for (int i = 0; i < coords.Length; i++)
            {
                if (!coords[i].IsValid)
                {
                    LogInvalid(TopologyValidationErrors.InvalidCoordinate, coords[i]);
                    return;
                }
            }
        }

        private void CheckCoordinateInvalid(CoordinateSequence sequence)
        {
            for (int i = 0; i < sequence.Count; i++)
            {
                var coord = sequence.GetCoordinate(i);
                if (!coord.IsValid)
                {
                    LogInvalid(TopologyValidationErrors.InvalidCoordinate, coord);
                    return;
                }
            }
        }

        private void CheckCoordinateInvalid(Polygon poly)
        {
            CheckCoordinateInvalid(poly.ExteriorRing.CoordinateSequence);
            if (HasInvalidError) return;
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                CheckCoordinateInvalid(poly.GetInteriorRingN(i).CoordinateSequence);
                if (HasInvalidError) return;
            }
        }

        private void CheckRingNotClosed(LineString ring)
        {
            if (ring.IsEmpty) return;
            if (!ring.IsClosed)
            {
                var pt = ring.NumPoints >= 1 ? ring.GetCoordinateN(0) : null;
                LogInvalid(TopologyValidationErrors.RingNotClosed, pt);
            }
        }

        private void CheckRingsNotClosed(Polygon poly)
        {
            CheckRingNotClosed(poly.ExteriorRing);
            if (HasInvalidError) return;
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                CheckRingNotClosed(poly.GetInteriorRingN(i));
                if (HasInvalidError) return;
            }
        }

        private void CheckRingsTooFewPoints(Polygon poly)
        {
            CheckRingTooFewPoints(poly.ExteriorRing);
            if (HasInvalidError) return;
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                CheckRingTooFewPoints(poly.GetInteriorRingN(i));
                if (HasInvalidError) return;
            }
        }

        private void CheckRingTooFewPoints(LineString ring)
        {
            if (ring.IsEmpty) return;
            CheckTooFewPoints(ring, MinSizeLinearRing);
        }

        /// <summary>
        /// Check the number of non-repeated points is at least a given size.
        /// </summary>
        /// <param name="line">The line to test</param>
        /// <param name="minSize">The minimum number of points in <paramref name="line"/></param>
        /// <returns><c>true</c> if the line has the required number of points</returns>
        private void CheckTooFewPoints(LineString line, int minSize)
        {
            if (!IsNonRepeatedSizeAtLeast(line, minSize))
            {
                var pt = line.NumPoints >= 1 ? line.GetCoordinateN(0) : null;
                LogInvalid(TopologyValidationErrors.TooFewPoints, pt);
            }
        }

        /// <summary>
        /// Test if the number of non-repeated points in a line
        /// is at least a given minimum size.
        /// </summary>
        /// <param name="line">The line to test</param>
        /// <param name="minSize">The minimum number of points in <paramref name="line"/></param>
        /// <returns><c>true</c> if the line has the required number of non-repeated points</returns>
        private bool IsNonRepeatedSizeAtLeast(LineString line, int minSize)
        {
            int numPts = 0;
            Coordinate prevPt = null;
            for (int i = 0; i < line.NumPoints; i++)
            {
                if (numPts >= minSize) return true;
                var pt = line.GetCoordinateN(i);
                if (prevPt == null || !pt.Equals2D(prevPt))
                    numPts++;
                prevPt = pt;
            }

            return numPts >= minSize;
        }

        private void CheckAreaIntersections(AreaTopologyAnalyzer areaAnalyzer)
        {
            if (areaAnalyzer.HasIntersection)
            {
                LogInvalid(TopologyValidationErrors.SelfIntersection,
                    areaAnalyzer.IntersectionLocation);
                return;
            }

            if (areaAnalyzer.HasDoubleTouch)
            {
                LogInvalid(TopologyValidationErrors.DisconnectedInteriors,
                    areaAnalyzer.IntersectionLocation);
                return;
            }

            if (areaAnalyzer.IsInteriorDisconnectedBySelfTouch())
            {
                LogInvalid(TopologyValidationErrors.DisconnectedInteriors,
                    areaAnalyzer.DisconnectionLocation);
            }

        }

        /// <summary>
        /// Check whether a ring self-intersects (except at its endpoints).
        /// </summary>
        /// <param name="ring">The linear ring to check</param>
        private void CheckSelfIntersectingRing(LinearRing ring)
        {
            var intPt = AreaTopologyAnalyzer.FindSelfIntersection(ring);
            if (intPt != null)
            {
                LogInvalid(TopologyValidationErrors.RingSelfIntersection,
                    intPt);
            }
        }

        /// <summary>
        /// Tests that each hole is inside the polygon shell.
        /// This routine assumes that the holes have previously been tested
        /// to ensure that all vertices lie on the shell or on the same side of it
        /// (i.e. that the hole rings do not cross the shell ring).
        /// Given this, a simple point-in-polygon test of a single point in the hole can be used,
        /// provided the point is chosen such that it does not lie on the shell.
        /// </summary>
        /// <param name="poly">The polygon to be tested for hole inclusion</param>
        private void CheckHolesOutsideShell(Polygon poly)
        {
            // skip test if no holes are present
            if (poly.NumInteriorRings <= 0) return;

            var shell = poly.ExteriorRing;
            bool isShellEmpty = shell.IsEmpty;
            var pir = new IndexedPointInAreaLocator(shell);

            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                var hole = poly.GetInteriorRingN(i);
                if (hole.IsEmpty) continue;

                Coordinate invalidPt;
                if (isShellEmpty)
                {
                    invalidPt = hole.Coordinate;
                }
                else
                {
                    invalidPt = FindHoleOutsideShellPoint(pir, hole);
                }

                if (invalidPt != null)
                {
                    LogInvalid(TopologyValidationErrors.HoleOutsideShell,
                        invalidPt);
                    return;
                }
            }
        }

        /// <summary>
        /// Checks if a polygon hole lies inside its shell
        /// and if not returns the point indicating this.
        /// The hole is known to be wholly inside or outside the shell,
        /// so it suffices to find a single point which is interior or exterior.
        /// A valid hole may only have a single point touching the shell
        /// (since otherwise it creates a disconnected interior).
        /// So there should be at least one point which is interior or exterior,
        /// and this should be the first or second point tested.
        /// </summary>
        /// <param name="shellLocator"></param>
        /// <param name="hole"></param>
        /// <returns>A hole point outside the shell, or <c>null</c> if valid.</returns>
        private Coordinate FindHoleOutsideShellPoint(IPointOnGeometryLocator shellLocator, LineString hole)
        {
            for (int i = 0; i < hole.NumPoints - 1; i++)
            {
                var holePt = hole.GetCoordinateN(i);
                var loc = shellLocator.Locate(holePt);
                if (loc == Location.Boundary) continue;
                if (loc == Location.Interior) return null;
                /*
                 * Location is EXTERIOR, so hole is outside shell
                 */
                return holePt;
            }

            return null;
        }

        /// <summary>
        /// Tests if any polygon hole is nested inside another.
        /// Assumes that holes do not cross (overlap),
        /// This is checked earlier.
        /// </summary>
        /// <param name="poly">The polygon with holes to test</param>
        private void CheckHolesNotNested(Polygon poly)
        {
            // skip test if no holes are present
            if (poly.NumInteriorRings <= 0) return;

            var nestedTester = new IndexedNestedHoleTester(poly);
            if (nestedTester.IsNested())
            {
                LogInvalid(TopologyValidationErrors.NestedHoles,
                    nestedTester.NestedPoint);
            }
        }

        /// <summary>
        /// Tests that no element polygon is in the interior of another element polygon.
        /// <para/>Preconditions:
        /// <list type="bullet">
        /// <item><description>shells do not partially overlap</description></item>
        /// <item><description>shells do not touch along an edge</description></item>
        /// <item><description>no duplicate rings exist</description></item></list>
        /// These have been confirmed by the <see cref="AreaTopologyAnalyzer"/>.
        /// </summary>
        private void CheckShellsNotNested(MultiPolygon mp)
        {
            for (int i = 0; i < mp.NumGeometries; i++)
            {
                var p = (Polygon) mp.GetGeometryN(i);
                if (p.IsEmpty)
                    continue;
                var shell = (LinearRing)p.ExteriorRing;
                for (int j = 0; j < mp.NumGeometries; j++)
                {
                    if (i == j) continue;
                    var p2 = (Polygon) mp.GetGeometryN(j);
                    var invalidPt = FindShellSegmentInPolygon(shell, p2);
                    if (invalidPt != null)
                    {
                        LogInvalid(TopologyValidationErrors.NestedShells,
                            invalidPt);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Finds a point of a shell segment which lies inside a polygon, if any.
        /// The shell is assume to touch the polyon only at shell vertices,
        /// and does not cross the polygon.
        /// </summary>
        /// <param name="shell">The shell to test</param>
        /// <param name="poly">The polygon to test</param>
        /// <returns>An interior segment point, or null if the shell is nested correctly</returns>
        private Coordinate FindShellSegmentInPolygon(LinearRing shell, Polygon poly)
        {
            var polyShell = poly.ExteriorRing;
            if (polyShell.IsEmpty) return null;

            //--- if envelope is not covered --> not nested
            if (!poly.EnvelopeInternal.Covers(shell.EnvelopeInternal))
                return null;

            var shell0 = shell.GetCoordinateN(0);
            var shell1 = shell.GetCoordinateN(1);

            if (!AreaTopologyAnalyzer.IsSegmentInRing(shell0, shell1, polyShell))
                return null;

            /*
             * Check if the shell is inside a hole (if there are any). 
             * If so this is valid.
             */
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                var hole = (LinearRing)poly.GetInteriorRingN(i);
                if (hole.EnvelopeInternal.Covers(shell.EnvelopeInternal)
                    && AreaTopologyAnalyzer.IsSegmentInRing(shell0, shell1, hole))
                {
                    return null;
                }
            }

            /*
             * The shell is contained in the polygon, but is not contained in a hole.
             * This is invalid.
             */
            return shell0;
        }

        private void CheckInteriorDisconnected(AreaTopologyAnalyzer areaAnalyzer)
        {
            if (areaAnalyzer.IsInteriorDisconnectedByRingCycle())
                LogInvalid(TopologyValidationErrors.DisconnectedInteriors,
                    areaAnalyzer.DisconnectionLocation);
        }

        /// <summary>
        /// Find a point from the list of testCoords
        /// that is NOT a node in the edge for the list of searchCoords.
        /// </summary>
        /// <param name="testCoords"></param>
        /// <param name="searchRing"></param>
        /// <param name="graph"></param>
        /// <returns>The point found, or <c>null</c> if none found.</returns>
        [Obsolete]
        public static Coordinate FindPointNotNode(Coordinate[] testCoords, LinearRing searchRing, GeometryGraph graph)
        {
            // find edge corresponding to searchRing.
            var searchEdge = graph.FindEdge(searchRing);
            // find a point in the testCoords which is not a node of the searchRing
            var eiList = searchEdge.EdgeIntersectionList;
            // somewhat inefficient - is there a better way? (Use a node map, for instance?)
            foreach(var pt in testCoords)
                if(!eiList.IsIntersection(pt))
                    return pt;
            return null;
        }
    }
}
