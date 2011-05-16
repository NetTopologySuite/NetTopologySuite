using System.Collections;
using System.Collections.Generic;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using GisSharpBlog.NetTopologySuite.Operation.Overlay;

namespace GisSharpBlog.NetTopologySuite.Operation.Union
{
    /**
     * Unions a collection of Geometry or a single Geometry 
     * (which may be a collection) together.
     * By using this special-purpose operation over a collection of geometries
     * it is possible to take advantage of various optimizations to improve performance.
     * Heterogeneous {@link GeometryCollection}s are fully supported.
     * <p>
     * The result obeys the following contract:
     * <ul>
     * <li>Unioning a set of overlapping {@link Polygons}s has the effect of
     * merging the areas (i.e. the same effect as 
     * iteratively unioning all individual polygons together).
     * <li>Unioning a set of {@link LineString}s has the effect of fully noding and dissolving
     * the linework.
     * <li>Unioning a set of {@link Points}s has the effect of merging
     * al identical points (producing a set with no duplicates).
     * </ul>
     * 
     * @author mbdavis
     *
     */
    public class UnaryUnionOp
    {
        public static IGeometry Union(IList geoms)
        {
            UnaryUnionOp op = new UnaryUnionOp(geoms);
            return op.Union();
        }

        public static IGeometry Union(IGeometry geom)
        {
            UnaryUnionOp op = new UnaryUnionOp(geom);
            return op.Union();
        }

        private readonly List<IPolygon> _polygons = new List<IPolygon>();
        private readonly List<ILineString> _lines = new List<ILineString>();
        private readonly List<IPoint> _points = new List<IPoint>();

        private IGeometryFactory _geomFact;

        public UnaryUnionOp(IList geoms)
        {
            Extract(geoms);
        }

        public UnaryUnionOp(IGeometry geom)
        {
            Extract(geom);
        }

        private void Extract(IEnumerable geoms)
        {
            foreach (IGeometry geom in geoms)
                Extract(geom);
        }

        private void Extract(IGeometry geom)
        {
            if (_geomFact == null)
                _geomFact = geom.Factory;

            /*
            PolygonExtracter.getPolygons(geom, polygons);
            LineStringExtracter.getLines(geom, lines);
            PointExtracter.getPoints(geom, points);
            */
            GeometryExtracter.Extract(geom, _polygons);
            GeometryExtracter.Extract(geom, _lines);
            GeometryExtracter.Extract(geom, _points);
        }

        ///<summary>
        /// Gets the union of the input geometries.
        ///</summary>
        /// <returns>
        /// <list>
        /// <item>a Geometry containing the union</item>
        /// <item><c>null</c>if no geometries were provided in the input</item>
        /// </list>
        /// </returns>
        public IGeometry Union()
        {
            if (_geomFact == null)
            {
                return null;
            }


            IGeometry unionPoints = null;
            if (_points.Count > 0)
            {
                IGeometry ptGeom = _geomFact.BuildGeometry(_points);
                unionPoints = UnionNoOpt(ptGeom);
            }

            IGeometry unionLines = null;
            if (_lines.Count > 0)
            {
                IGeometry lineGeom = _geomFact.BuildGeometry(_lines);
                unionLines = UnionNoOpt(lineGeom);
            }

            IGeometry unionPolygons = null;
            if (_polygons.Count > 0)
            {
                unionPolygons = CascadedPolygonUnion.Union(_polygons);
            }

            /**
             * Performing two unions is somewhat inefficient,
             * but is mitigated by unioning lines and points first
             */
            IGeometry unionPointsLines = UnionWithNull(unionPoints, unionLines);
            IGeometry union = UnionWithNull(unionPointsLines, unionPolygons);

            return union;
        }

        ///<summary>
        /// Computes the union of two geometries, either of both of which may be null.
        ///</summary>
        /// <param name="g0"></param>
        /// <param name="g1"></param>
        /// <returns>
        /// <list type="Bullet">
        /// <item>The union of the input(s)</item>
        /// <item>If both inputs are null</item>
        /// </list>
        /// </returns>
        private static IGeometry UnionWithNull(IGeometry g0, IGeometry g1)
        {
            if (g0 == null && g1 == null)
                return null;

            if (g1 == null)
                return g0;
            if (g0 == null)
                return g1;

            return g0.Union(g1);
        }

        ///<summary>
        /// Computes a unary union with no extra optimization, and no short-circuiting.
        ///</summary>
        /// <remarks>
        /// Due to the way the overlay operations are implemented, this is still efficient in the case of linear and puntal geometries.
        /// </remarks>
        /// <param name="g0">A geometry</param>
        /// <returns>The union of the input geometry</returns>
        private IGeometry UnionNoOpt(IGeometry g0)
        {
            
            IGeometry empty = _geomFact.CreatePoint((ICoordinate)null);
            return OverlayOp.Overlay(g0, empty, SpatialFunction.Union);
        }

    }
}