using System;
using System.Collections.Generic;
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
    /// <item><description><c>LineString</c>: fix coordinate list </description></item>
    /// <item><description><c>LinearRing</c>: fix coordinate list, return as valid ring or else <c>LineString</c></description></item>
    /// <item><description><c>Polygon</c>: transform into a valid polygon, 
    /// preserving as much of the extent and vertices as possible</description></item>
    /// <item><description><c>MultiPolygon</c>: fix each polygon, 
    /// then ensure result is non - overlapping(via union)</description></item>
    /// <item><description><c>GeometryCollection</c>: fix each element </description></item>
    /// <item><description>Collapsed lines and polygons are handled as follows,
    /// depending on the <c>keepCollapsed</c> setting:
    /// <list type="bullet">
    /// <item><description><c>false</c>: (default) collapses are converted to empty geometries</description></item>
    /// <item><description><c>true</c>: collapses are converted to a valid geometry of lower dimension</description></item>
    /// </list></description></item>
    /// </list>
    /// </summary>
    /// <author>Martin Davis</author>
    /// <seealso cref="Geometry.IsValid"/>
    public class GeometryFixer
    {
        /// <summary>
        /// Fixes a geometry to be valid.
        /// </summary>
        /// <param name="geom">The geometry to be fixed</param>
        /// <returns>The valid fixed geometry</returns>
        public static Geometry Fix(Geometry geom)
        {
            var fix = new GeometryFixer(geom);
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

                //-- if not allowing collapses then return empty polygon
                return null;
            }

            // if no holes then done
            if (geom.NumInteriorRings == 0)
            {
                return fixShell;
            }

            var fixHoles = FixHoles(geom);
            var result = RemoveHoles(fixShell, fixHoles);
            return result;
        }

        private Geometry RemoveHoles(Geometry shell, Geometry holes)
        {
            if (holes == null)
                return shell;
            return OverlayNGRobust.Overlay(shell, holes, OverlayNG.DIFFERENCE);
        }

        private Geometry FixHoles(Polygon geom)
        {
            var holes = new List<Geometry>(geom.NumInteriorRings);
            for (int i = 0; i < geom.NumInteriorRings; i++)
            {
                var holeRep = FixRing((LinearRing) geom.GetInteriorRingN(i));
                if (holeRep != null)
                {
                    holes.Add(holeRep);
                }
            }

            if (holes.Count == 0) return null;
            if (holes.Count == 1)
            {
                return holes[0];
            }
            // TODO: replace with holes.union() once OverlayNG is the default
            var holesUnion = OverlayNGRobust.Union(holes);
            return holesUnion;
        }

        private Geometry FixRing(LinearRing ring)
        {
            //-- always execute fix, since it may remove repeated coords etc
            Geometry poly = _factory.CreatePolygon(ring);
            // TOD: check if buffer removes invalid coordinates
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
            var result = OverlayNGRobust.Union(polys);
            return result;
        }

        private Geometry FixCollection(GeometryCollection geom)
        {
            var geomRep = new Geometry[geom.NumGeometries];
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                geomRep[i] = Fix(geom.GetGeometryN(i));
            }

            return _factory.CreateGeometryCollection(geomRep);
        }
    }
}
