using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Linemerge;
using NetTopologySuite.Operation.Overlay;

namespace NetTopologySuite.Operation.Union
{
    /// <summary>
    /// Unions a <c>Collection</c> of <see cref="Geometry"/>s or a single Geometry (which may be a <see cref="GeometryCollection"/>) together.
    /// </summary>
    /// <remarks>
    /// <para>
    /// By using this special-purpose operation over a collection of geometries
    /// it is possible to take advantage of various optimizations to improve performance.
    /// </para>
    /// <para>
    /// Heterogeneous <see cref="GeometryCollection"/>s are fully supported.
    /// </para>
    /// <para>
    /// The result obeys the following contract:
    /// <list type="Bullet">
    /// <item>Unioning a set of <see cref="Polygon"/>s has the effect of merging the areas (i.e. the same effect as iteratively unioning all individual polygons together).</item>
    /// <item>Unioning a set of <see cref="LineString"/>s has the effect of <b>fully noding</b>
    /// and <b>dissolving</b> the input linework.
    /// In this context "fully noded" means that there will be
    /// an endpoint or node in the result
    /// for every endpoint or line segment crossing in the input.
    /// "Dissolved" means that any duplicate (e.g. coincident) line segments or portions
    /// of line segments will be reduced to a single line segment in the output.
    /// This is consistent with the semantics of the
    /// <see cref="Geometry.Union(Geometry)"/> operation.
    /// If <b>merged</b> linework is required, the <see cref="LineMerger"/> class can be used.</item>
    /// <item>Unioning a set of <see cref="Point"/>s has the effect of merging all identical points (producing a set with no duplicates).</item> </list>
    /// </para>
    /// <para>
    /// <c>UnaryUnion</c> always operates on the individual components of MultiGeometries.
    /// So it is possible to use it to "clean" invalid self-intersecting MultiPolygons
    /// (although the polygon components must all still be individually valid.)
    /// </para>
    /// </remarks>
    /// <author>
    /// mbdavis
    /// </author>
    public class UnaryUnionOp
    {
        /// <summary>
        /// Computes the geometric union of a <see cref="IList{Geometry}"/>
        /// </summary>
        /// <param name="geoms">A collection of geometries</param>
        /// <returns>The union of the geometries,
        /// or <c>null</c> if the input is empty</returns>
        public static Geometry Union(IList<Geometry> geoms)
        {
            var op = new UnaryUnionOp(geoms);
            return op.Union();
        }

        /// <summary>
        /// Computes the geometric union of a <see cref="IList{Geometry}"/><para/>
        /// If no input geometries were provided but a <see cref="GeometryFactory"/> was provided,
        /// an empty <see cref="GeometryCollection"/> is returned.
        /// </summary>
        /// <param name="geoms">A collection of geometries</param>
        /// <param name="geomFact">The geometry factory to use if the collection is empty</param>
        /// <returns>The union of the geometries
        /// or an empty GEOMETRYCOLLECTION</returns>
        public static Geometry Union(IList<Geometry> geoms, GeometryFactory geomFact)
        {
            var op = new UnaryUnionOp(geoms, geomFact);
            return op.Union();
        }

        /// <summary>Constructs a unary union operation for a <see cref="Geometry"/>
        /// (which may be a <see cref="GeometryCollection"/>).
        /// </summary>
        /// <param name="geom">A geometry to union</param>
        /// <returns>The union of the elements of the geometry
        /// or an empty GEOMETRYCOLLECTION</returns>
        public static Geometry Union(Geometry geom)
        {
            var op = new UnaryUnionOp(geom);
            return op.Union();
        }

        private readonly List<Geometry> _polygons = new List<Geometry>();
        private readonly List<Geometry> _lines = new List<Geometry>();
        private readonly List<Geometry> _points = new List<Geometry>();

        private GeometryFactory _geomFact;

        /// <summary>
        /// Constructs a unary union operation for an enumeration
        /// of <see cref="Geometry"/>s, using the <see cref="GeometryFactory"/>
        /// of the input geometries.
        /// </summary>
        /// <param name="geoms">An enumeration of geometries</param>
        public UnaryUnionOp(IEnumerable<Geometry> geoms)
        {
            Extract(geoms);
        }

        /// <summary>
        /// Constructs a unary union operation for an enumeration
        /// of <see cref="Geometry"/>s. <para/>
        /// If no input geometries were provided but a <see cref="GeometryFactory"/> was provided,
        /// </summary>
        /// <param name="geoms">An enumeration of geometries</param>
        /// <param name="geomFact">The geometry factory to use if the enumeration is empty</param>
        public UnaryUnionOp(IEnumerable<Geometry> geoms, GeometryFactory geomFact)
        {
            _geomFact = geomFact;
            Extract(geoms);
        }

        /// <summary>
        /// Constructs a unary union operation for a <see cref="Geometry"/>
        /// (which may be a <see cref="GeometryCollection"/>).
        /// </summary>
        /// <param name="geom">A geometry to union</param>
        public UnaryUnionOp(Geometry geom)
        {
            Extract(geom);
        }

        private void Extract(IEnumerable<Geometry> geoms)
        {
            foreach (var geom in geoms)
                Extract(geom);
        }

        private void Extract(Geometry geom)
        {
            if (_geomFact == null)
                _geomFact = geom.Factory;

            /*
            PolygonExtracter.getPolygons(geom, polygons);
            LineStringExtracter.getLines(geom, lines);
            PointExtracter.getPoints(geom, points);
            */
            GeometryExtracter.Extract<Polygon>(geom, _polygons);
            GeometryExtracter.Extract<LineString>(geom, _lines);
            GeometryExtracter.Extract<Point>(geom, _points);
        }

        /// <summary>
        /// Gets the union of the input geometries.
        /// If no input geometries were provided but a <see cref="GeometryFactory"/> was provided,
        /// an empty <see cref="GeometryCollection"/> is returned.
        /// <para/>Otherwise, the return value is <c>null</c>
        /// </summary>
        /// <returns>
        /// A Geometry containing the union
        /// or an empty <see cref="GeometryCollection"/> if no geometries were provided in the input,
        /// or <c>null</c> if not GeometryFactory was provided
        /// </returns>
        public Geometry Union()
        {
            if (_geomFact == null)
            {
                return null;
            }

            /**
             * For points and lines, only a single union operation is
             * required, since the OGC model allows self-intersecting
             * MultiPoint and MultiLineStrings.
             * This is not the case for polygons, so Cascaded Union is required.
             */
            Geometry unionPoints = null;
            if (_points.Count > 0)
            {
                var ptGeom = _geomFact.BuildGeometry(_points);
                unionPoints = UnionNoOpt(ptGeom);
            }

            Geometry unionLines = null;
            if (_lines.Count > 0)
            {
                var lineGeom = _geomFact.BuildGeometry(_lines);
                unionLines = UnionNoOpt(lineGeom);
            }

            Geometry unionPolygons = null;
            if (_polygons.Count > 0)
            {
                unionPolygons = CascadedPolygonUnion.Union(_polygons);
            }

            /*
             * Performing two unions is somewhat inefficient,
             * but is mitigated by unioning lines and points first
             */
            var unionLA = UnionWithNull(unionLines, unionPolygons);
            Geometry union;
            if (unionPoints == null)
                union = unionLA;
            else if (unionLA == null)
                union = unionPoints;
            else
                union = PointGeometryUnion.Union((IPuntal)unionPoints, unionLA);

            if (union == null)
                return _geomFact.CreateGeometryCollection();

            return union;
        }

        //private static ICollection<Geometry> ToCollection<T>(IEnumerable<T> items) where T : Geometry
        //{
        //    var ret = new List<Geometry>();
        //    foreach (Geometry geometry in items)
        //        ret.Add(geometry);
        //    return ret;
        //}

        /// <summary>
        /// Computes the union of two geometries, either of both of which may be null.
        /// </summary>
        /// <param name="g0"></param>
        /// <param name="g1"></param>
        /// <returns>
        /// The union of the input(s)
        /// or <value>null</value> if both inputs are <value>null</value>
        /// </returns>
        private static Geometry UnionWithNull(Geometry g0, Geometry g1)
        {
            if (g0 == null && g1 == null)
                return null;

            if (g1 == null)
                return g0;
            if (g0 == null)
                return g1;

            return g0.Union(g1);
        }

        /// <summary>
        /// Computes a unary union with no extra optimization, and no short-circuiting.
        /// </summary>
        /// <remarks>
        /// Due to the way the overlay operations are implemented, this is still efficient in the case of linear and puntal geometries.
        /// </remarks>
        /// <param name="g0">A geometry</param>
        /// <returns>The union of the input geometry</returns>
        private Geometry UnionNoOpt(Geometry g0)
        {

            var empty = _geomFact.CreatePoint();
            return OverlayOp.Overlay(g0, empty, SpatialFunction.Union);
        }

    }
}
