using System;
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
            CheckCoordinatesValid(g.CoordinateSequence);
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
            CheckCoordinatesValid(g.Coordinates);
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
            CheckCoordinatesValid(g.CoordinateSequence);
            if (HasInvalidError) return false;
            CheckPointSize(g, MinSizeLineString);
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
            CheckCoordinatesValid(g.CoordinateSequence);
            if (HasInvalidError) return false;

            CheckRingClosed(g);
            if (HasInvalidError) return false;

            CheckRingPointSize(g);
            if (HasInvalidError) return false;

            CheckRingSimple(g);
            return _validErr == null;
        }

        /// <summary>
        /// Tests validity of a <c>Polygon</c>.<br/>
        /// </summary>
        /// <param name="g">The <c>Polygon</c> to test</param>
        /// <remarks>In JTS this function is called <c>IsValid</c></remarks>
        private bool IsValidGeometry(Polygon g)
        {
            CheckCoordinatesValid(g);
            if (HasInvalidError) return false;

            CheckRingsClosed(g);
            if (HasInvalidError) return false;

            CheckRingsPointSize(g);
            if (HasInvalidError) return false;

            var areaAnalyzer = new PolygonTopologyAnalyzer(g, _isInvertedRingValid);

            CheckAreaIntersections(areaAnalyzer);
            if (HasInvalidError) return false;

            CheckHolesInShell(g);
            if (HasInvalidError) return false;

            CheckHolesNotNested(g);
            if (HasInvalidError) return false;

            CheckInteriorConnected(areaAnalyzer);
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
                CheckCoordinatesValid(p);
                if (HasInvalidError) return false;

                CheckRingsClosed(p);
                if (HasInvalidError) return false;
                CheckRingsPointSize(p);
                if (HasInvalidError) return false;
            }

            var areaAnalyzer = new PolygonTopologyAnalyzer(g, _isInvertedRingValid);

            CheckAreaIntersections(areaAnalyzer);
            if (HasInvalidError) return false;

            for (int i = 0; i < g.NumGeometries; i++)
            {
                var p = (Polygon) g.GetGeometryN(i);
                CheckHolesInShell(p);
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

            CheckInteriorConnected(areaAnalyzer);
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

        private void CheckCoordinatesValid(Coordinate[] coords)
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

        private void CheckCoordinatesValid(CoordinateSequence sequence)
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

        private void CheckCoordinatesValid(Polygon poly)
        {
            CheckCoordinatesValid(poly.ExteriorRing.CoordinateSequence);
            if (HasInvalidError) return;
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                CheckCoordinatesValid(poly.GetInteriorRingN(i).CoordinateSequence);
                if (HasInvalidError) return;
            }
        }

        private void CheckRingClosed(LineString ring)
        {
            if (ring.IsEmpty) return;
            if (!ring.IsClosed)
            {
                var pt = ring.NumPoints >= 1 ? ring.GetCoordinateN(0) : null;
                LogInvalid(TopologyValidationErrors.RingNotClosed, pt);
            }
        }

        private void CheckRingsClosed(Polygon poly)
        {
            CheckRingClosed(poly.ExteriorRing);
            if (HasInvalidError) return;
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                CheckRingClosed(poly.GetInteriorRingN(i));
                if (HasInvalidError) return;
            }
        }

        private void CheckRingsPointSize(Polygon poly)
        {
            CheckRingPointSize(poly.ExteriorRing);
            if (HasInvalidError) return;
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                CheckRingPointSize(poly.GetInteriorRingN(i));
                if (HasInvalidError) return;
            }
        }

        private void CheckRingPointSize(LineString ring)
        {
            if (ring.IsEmpty) return;
            CheckPointSize(ring, MinSizeLinearRing);
        }

        /// <summary>
        /// Check the number of non-repeated points is at least a given size.
        /// </summary>
        /// <param name="line">The line to test</param>
        /// <param name="minSize">The minimum number of points in <paramref name="line"/></param>
        /// <returns><c>true</c> if the line has the required number of points</returns>
        private void CheckPointSize(LineString line, int minSize)
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

        private void CheckAreaIntersections(PolygonTopologyAnalyzer areaAnalyzer)
        {
            if (areaAnalyzer.HasInvalidIntersection)
            {
                LogInvalid(areaAnalyzer.InvalidCode,
                    areaAnalyzer.IntersectionLocation);
                return;
            }
        }

        /// <summary>
        /// Check whether a ring self-intersects (except at its endpoints).
        /// </summary>
        /// <param name="ring">The linear ring to check</param>
        private void CheckRingSimple(LinearRing ring)
        {
            var intPt = PolygonTopologyAnalyzer.FindSelfIntersection(ring);
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
        private void CheckHolesInShell(Polygon poly)
        {
            // skip test if no holes are present
            if (poly.NumInteriorRings <= 0) return;

            var shell = poly.ExteriorRing;
            bool isShellEmpty = shell.IsEmpty;

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
                    invalidPt = FindHoleOutsideShellPoint(hole, shell);
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
        /// and if not returns a point indicating this.
        /// The hole is known to be wholly inside or outside the shell,
        /// so it suffices to find a single point which is interior or exterior,
        /// or check the edge topology at a point on the boundary of the shell.
        /// </summary>
        /// <param name="hole">The hole to test</param>
        /// <param name="shell">The polygon shell to test against</param>
        /// <returns>A hole point outside the shell, or <c>null</c> if it is inside.</returns>
        private Coordinate FindHoleOutsideShellPoint(LineString hole, LineString shell)
        {
            var holePt0 = hole.GetCoordinateN(0);
            /*
             * If hole envelope is not covered by shell, it must be outside
             */
            if (!shell.EnvelopeInternal.Covers(hole.EnvelopeInternal))
                //TODO: find hole pt outside shell env
                return holePt0;

            if (PolygonTopologyAnalyzer.IsRingNested(hole, shell))
                return null;

            //TODO: find hole point outside shell
            return holePt0;
        }

        /// <summary>
        /// Checks if any polygon hole is nested inside another.
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
        /// Checks that no element polygon is in the interior of another element polygon.
        /// <para/>Preconditions:
        /// <list type="bullet">
        /// <item><description>shells do not partially overlap</description></item>
        /// <item><description>shells do not touch along an edge</description></item>
        /// <item><description>no duplicate rings exist</description></item></list>
        /// These have been confirmed by the <see cref="PolygonTopologyAnalyzer"/>.
        /// </summary>
        private void CheckShellsNotNested(MultiPolygon mp)
        {
            // skip test if only one shell present
            if (mp.NumGeometries <= 1) return;

            var nestedTester = new IndexedNestedPolygonTester(mp);
            if (nestedTester.IsNested())
            {
                LogInvalid(TopologyValidationErrors.NestedShells,
                    nestedTester.NestedPoint);
            }
        }

        private void CheckInteriorConnected(PolygonTopologyAnalyzer analyzer)
        {
            if (analyzer.IsInteriorDisconnected())
                LogInvalid(TopologyValidationErrors.DisconnectedInteriors,
                    analyzer.DisconnectionLocation);
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
