using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Operation.Buffer;
using NetTopologySuite.Operation.OverlayNG;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// Fixes a geometry to be a valid geometry, while preserving as much as
    /// possible of the shape and location of the input.
    /// Validity is determined according to <see cref="Geometry.IsValid"/>.
    /// <para/>
    /// Input geometries are always processed, so even valid inputs may
    /// have some minor alterations.The output is always a new geometry object.
    /// <h2>Semantic Rules</h2>
    /// <list type="number">
    /// <item><description>Vertices with non-finite X or Y ordinates are removed (as per <see cref="Coordinate.IsValid"/>)</description></item>
    /// <item><description>Repeated points are reduced to a single point</description></item>
    /// <item><description>Empty atomic geometries are valid and are returned unchanged</description></item>
    /// <item><description>Empty elements are removed from collections</description></item>
    /// <item><description><c>Point</c>: keep valid coordinate, or EMPTY</description></item>
    /// <item><description><c>LineString</c>: coordinates are fixed</description></item>
    /// <item><description><c>LinearRing</c>: coordinates are feixed, keep valid ring or else convert into <c>LineString</c></description></item>
    /// <item><description><c>Polygon</c>: transform into a valid polygon or multipolygon, 
    /// preserving as much of the extent and vertices as possible.
    /// <list type="bullet">
    /// <item><description>Rings are fixed to ensure they are valid</description></item>
    /// <item><description>Holes intersection the shell are subtracted from the shell</description></item>
    /// <item><description>Holes outside the shell are converted into polygons</description></item>
    /// </list></description></item>
    /// <item><description><c>MultiPolygon</c>: each polygon is fixed, 
    /// then result made non - overlapping (via union)</description></item>
    /// <item><description><c>GeometryCollection</c>: each element is fixed</description></item>
    /// <item><description>Collapsed lines and polygons are handled as follows,
    /// depending on the <c>keepCollapsed</c> setting:
    /// <list type="bullet">
    /// <item><description><c>false</c>: (default) collapses are converted to empty geometries
    /// (and removed if they are elements of collections)</description></item>
    /// <item><description><c>true</c>: collapses are converted to a valid geometry of lower dimension</description></item>
    /// </list></description></item>
    /// </list>
    /// </summary>
    /// <author>Martin Davis</author>
    /// <seealso cref="Geometry.IsValid"/>
    public class GeometryFixer
    {
        private const bool DefaultKeepMulti = true;

        /// <summary>
        /// Fixes a geometry to be valid.
        /// </summary>
        /// <param name="geom">The geometry to be fixed</param>
        /// <returns>The valid fixed geometry</returns>
        public static Geometry Fix(Geometry geom)
        {
            return Fix(geom, DefaultKeepMulti);
        }

        /// <summary>
        /// Fixes a geometry to be valid, allowing to set a flag controlling how
        /// single item results from fixed <c>MULTI</c> geometries should be
        /// returned.
        /// </summary>
        /// <param name="geom">The geometry to be fixed</param>
        /// <param name="isKeepMulti">A flag indicating if <c>MULTI</c> geometries should not
        /// be converted to single instance types if they consist of only one item.</param>
        /// <returns>The valid fixed geometry</returns>
        public static Geometry Fix(Geometry geom, bool isKeepMulti)
        {
            var fix = new GeometryFixer(geom) {
                KeepMulti = isKeepMulti
            };
            return fix.GetResult();
        }

        private readonly Geometry _geom;
        private readonly GeometryFactory _factory;
        
        /// <summary>Creates a new instance to fix a given geometry</summary>
        /// <param name="geom">The geometry to be fixed</param>
        public GeometryFixer(Geometry geom)
        {
            _geom = geom;
            _factory = geom.Factory;
        }

        /// <summary>
        /// Gets or sets a value indicating whether collapsed
        /// geometries are converted to empty,
        /// (which will be removed from collections),
        /// or to a valid geometry of lower dimension.
        /// The default is to convert collapses to empty geometries (<c>false</c>).
        /// </summary>
        public bool KeepCollapsed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether collapsed
        /// geometries are converted to empty,
        /// (which will be removed from collections),
        /// or to a valid geometry of lower dimension.
        /// The default is to convert collapses to empty geometries (<c>false</c>).
        /// </summary>
        public bool KeepMulti { get; set; }

        /// <summary>
        /// Gets the fixed geometry.
        /// </summary>
        /// <returns>The fixed geometry</returns>
        public Geometry GetResult()
        {
            /*
             *  Truly empty geometries are simply copied.
             *  Geometry collections with elements are evaluated on a per-element basis.
             */
            if (_geom.NumGeometries == 0)
            {
                return _geom.Copy();
            }

            switch (_geom)
            {
                case Point geom:
                    return FixPoint(geom);
                //  LinearRing must come before LineString
                case LinearRing ring:
                    return FixLinearRing(ring);
                case LineString geom:
                    return FixLineString(geom);
                case Polygon geom:
                    return FixPolygon(geom);
                case MultiPoint geom:
                    return FixMultiPoint(geom);
                case MultiLineString geom:
                    return FixMultiLineString(geom);
                case MultiPolygon geom:
                    return FixMultiPolygon(geom);
                case GeometryCollection geom:
                    return FixCollection(geom);
                default:
                    throw new NotSupportedException(_geom.GetType().Name);
            }
        }

        private Point FixPoint(Point geom)
        {
            Geometry pt = FixPointElement(geom);
            if (pt == null)
                return _factory.CreatePoint();
            return (Point) pt;
        }

        private Point FixPointElement(Point geom)
        {
            if (geom.IsEmpty || !IsValidPoint(geom))
            {
                return null;
            }

            return (Point) geom.Copy();
        }

        private static bool IsValidPoint(Point pt)
        {
            var p = pt.Coordinate;
            return p.IsValid;
        }

        private Geometry FixMultiPoint(MultiPoint geom)
        {
            var pts = new List<Point>(geom.NumGeometries);
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var pt = (Point) geom.GetGeometryN(i);
                if (pt.IsEmpty) continue;
                var fixPt = FixPointElement(pt);
                if (fixPt != null)
                {
                    pts.Add(fixPt);
                }
            }

            if (!KeepMulti && pts.Count == 1)
                return pts[0];

            return _factory.CreateMultiPoint(GeometryFactory.ToPointArray(pts));
        }

        private Geometry FixLinearRing(LinearRing geom)
        {
            var fix = FixLinearRingElement(geom);
            if (fix == null)
                return _factory.CreateLinearRing();
            return fix;
        }

        private Geometry FixLinearRingElement(LinearRing geom)
        {
            if (geom.IsEmpty) return null;
            var pts = geom.Coordinates;
            var ptsFix = FixCoordinates(pts);
            if (KeepCollapsed)
            {
                if (ptsFix.Length == 1)
                {
                    return _factory.CreatePoint(ptsFix[0]);
                }

                if (ptsFix.Length > 1 && ptsFix.Length <= 3)
                {
                    return _factory.CreateLineString(ptsFix);
                }
            }

            //--- too short to be a valid ring
            if (ptsFix.Length <= 3)
            {
                return null;
            }

            var ring = _factory.CreateLinearRing(ptsFix);
            //--- convert invalid ring to LineString
            if (!ring.IsValid)
            {
                return _factory.CreateLineString(ptsFix);
            }

            return ring;
        }

        private Geometry FixLineString(LineString geom)
        {
            var fix = FixLineStringElement(geom);
            if (fix == null)
                return _factory.CreateLineString();
            return fix;
        }

        private Geometry FixLineStringElement(LineString geom)
        {
            if (geom.IsEmpty) return null;
            var pts = geom.Coordinates;
            var ptsFix = FixCoordinates(pts);
            if (KeepCollapsed && ptsFix.Length == 1)
            {
                return _factory.CreatePoint(ptsFix[0]);
            }

            if (ptsFix.Length <= 1)
            {
                return null;
            }

            return _factory.CreateLineString(ptsFix);
        }

        /// <summary>
        /// Returns a clean copy of the input coordinate array.
        /// </summary>
        /// <param name="pts">Coordinates to clean</param>
        /// <returns>An array of clean coordinates</returns>
        private static Coordinate[] FixCoordinates(Coordinate[] pts)
        {
            var ptsClean = CoordinateArrays.RemoveRepeatedOrInvalidPoints(pts);
            return CoordinateArrays.CopyDeep(ptsClean);
        }

        private Geometry FixMultiLineString(MultiLineString geom)
        {
            var @fixed = new List<Geometry>(geom.NumGeometries);
            bool isMixed = false;
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var line = (LineString) geom.GetGeometryN(i);
                if (line.IsEmpty) continue;

                var fix = FixLineStringElement(line);
                if (fix == null) continue;

                if (!(fix is LineString))
                {
                    isMixed = true;
                }

                @fixed.Add(fix);
            }

            if (@fixed.Count == 1)
            {
                if (!KeepMulti || !(@fixed[0] is LineString))
                    return @fixed[0];
            }

            if (isMixed)
            {
                return _factory.CreateGeometryCollection(GeometryFactory.ToGeometryArray(@fixed));
            }

            return _factory.CreateMultiLineString(GeometryFactory.ToLineStringArray(@fixed));
        }

        private Geometry FixPolygon(Polygon geom)
        {
            var fix = FixPolygonElement(geom);
            if (fix == null)
                return _factory.CreatePolygon();
            return fix;
        }

        private Geometry FixPolygonElement(Polygon geom)
        {
            var shell = geom.ExteriorRing;
            var fixShell = FixRing((LinearRing)shell);
            if (fixShell.IsEmpty)
            {
                if (KeepCollapsed)
                {
                    return FixLineString(shell);
                }

                //--- if not allowing collapses then return empty polygon
                return null;
            }

            //--- if no holes then done
            if (geom.NumInteriorRings == 0)
            {
                return fixShell;
            }

            //--- fix holes, classify, and construct shell-true holes
            var holesFixed = FixHoles(geom);
            var holes = new List<Geometry>();
            var shells = new List<Geometry>();
            ClassifyHoles(fixShell, holesFixed, holes, shells);
            var polyWithHoles = Difference(fixShell, holes);
            if (shells.Count == 0)
            {
                return polyWithHoles;
            }

            //--- if some holes converted to shells, union all shells
            shells.Add(polyWithHoles);
            var result = Union(shells);
            return result;
        }

        private List<Geometry> FixHoles(Polygon geom)
        {
            var holes = new List<Geometry>();
            for (int i = 0; i < geom.NumInteriorRings; i++)
            {
                var holeRep = FixRing((LinearRing)geom.GetInteriorRingN(i));
                if (holeRep != null)
                {
                    holes.Add(holeRep);
                }
            }
            return holes;
        }

        private void ClassifyHoles(Geometry shell, List<Geometry> holesFixed, List<Geometry> holes, List<Geometry> shells)
        {
            var shellPrep = PreparedGeometryFactory.Prepare(shell);
            foreach (var hole in holesFixed)
            {
                if (shellPrep.Intersects(hole))
                {
                    holes.Add(hole);
                }
                else
                {
                    shells.Add(hole);
                }
            }
        }

        /// <summary>Subtracts a list of polygonal geometries from a polygonal geometry.</summary>
        /// <param name="shell">polygonal geometry for shell</param>
        /// <param name="holes">polygonal geometries for holes</param>
        /// <returns>The result geometry</returns>
        private Geometry Difference(Geometry shell, List<Geometry> holes)
        {
            if (holes == null || holes.Count == 0)
                return shell;
            var holesUnion = Union(holes);
            return OverlayNGRobust.Overlay(shell, holesUnion, OverlayNG.DIFFERENCE);
        }

        /// <summary>
        /// Unions a list of polygonal geometries.
        /// Optimizes case of zero or one input geometries.
        /// Requires that the inputs are net new objects.
        /// </summary>
        /// <param name="polys">The polygonal geometries to union</param>
        /// <returns>The union of the inputs</returns>
        private Geometry Union(List<Geometry> polys)
        {
            if (polys.Count == 0) return _factory.CreatePolygon();
            if (polys.Count == 1)
            {
                return polys[0];
            }
            // TODO: replace with holes.union() once OverlayNG is the default
            return OverlayNGRobust.Union(polys);
        }

        private Geometry FixRing(LinearRing ring)
        {
            //-- always execute fix, since it may remove repeated coords etc
            Geometry poly = _factory.CreatePolygon(ring);
            // TODO: check if buffer removes invalid coordinates
            return BufferOp.BufferByZero(poly, true);
        }

        private Geometry FixMultiPolygon(MultiPolygon geom)
        {
            var polys = new List<Geometry>();
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var poly = (Polygon) geom.GetGeometryN(i);
                var polyFix = FixPolygonElement(poly);
                if (polyFix != null && !polyFix.IsEmpty)
                {
                    polys.Add(polyFix);
                }
            }

            if (polys.Count == 0)
            {
                return _factory.CreateMultiPolygon();
            }

            // TODO: replace with polys.union() once OverlayNG is the default
            var result = Union(polys);

            if (KeepMulti && result is Polygon pg)
                result = _factory.CreateMultiPolygon(new Polygon[] { pg });

            return result;
        }

        private Geometry FixCollection(GeometryCollection geom)
        {
            var geomRep = new Geometry[geom.NumGeometries];
            for (int i = 0; i < geom.NumGeometries; i++) {
                geomRep[i] = Fix(geom.GetGeometryN(i), KeepCollapsed, KeepMulti);
            }

            return _factory.CreateGeometryCollection(geomRep);
        }

        private static Geometry Fix(Geometry geom, bool isKeepCollapsed, bool isKeepMulti)
        {
            var fix = new GeometryFixer(geom) {
                KeepCollapsed = isKeepCollapsed,
                KeepMulti = isKeepMulti
            };
            return fix.GetResult();
        }

    }
}
