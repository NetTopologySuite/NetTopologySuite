using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Overlay;

namespace NetTopologySuite.Operation.Union
{
    ///<summary>
    /// Unions a collection of Geometry or a single Geometry (which may be a collection) together.
    ///</summary>
    /// <remarks>
    /// <para>
    /// By using this special-purpose operation over a collection of geometries
    /// it is possible to take advantage of various optimizations to improve performance.
    /// </para>
    /// <para>
    /// Heterogeneous <see cref="IGeometryCollection"/>s are fully supported.
    /// </para>
    /// <para>
    /// The result obeys the following contract:
    /// <list type="Bullet">
    /// <item>Unioning a set of overlapping <see cref="IPolygon"/>s has the effect of merging the areas (i.e. the same effect as iteratively unioning all individual polygons together).</item>
    /// <item>Unioning a set of <see cref="ILineString"/>s has the effect of <b>fully noding</b> 
    /// and <b>dissolving</b> the input linework.
    /// In this context "fully noded" means that there will be a node or endpoint in the output 
    /// for every endpoint or line segment crossing in the input.
    /// "Dissolved" means that any duplicate (e.g. coincident) line segments or portions
    /// of line segments will be reduced to a single line segment in the output.  
    /// This is consistent with the semantics of the 
    /// <see cref="IGeometry.Union(IGeometry)"/> operation.
    /// If <b>merged</b> linework is required, the {@link LineMerger} class can be used.</item>
    /// <item>Unioning a set of <see cref="IPoint"/>s has the effect of merging all identical points (producing a set with no duplicates).</item> </list>
    /// </para>
    /// <para>
    /// <c>UnaryUnion</c> always operates on the individual components of MultiGeometries.
    /// So it is possible to use it to "clean" invalid self-intersecting MultiPolygons
    /// (although the polygon components must all still be individually valid.)
    /// </para>
    /// </remarks>
    /// <author>
    /// mbdavis
    ///</author>
    public class UnaryUnionOp
    {
        public static IGeometry Union(IList<IGeometry> geoms)
        {
            UnaryUnionOp op = new UnaryUnionOp(geoms);
            return op.Union();
        }

        public static IGeometry Union(IList<IGeometry> geoms, IGeometryFactory geomFact)
        {
            UnaryUnionOp op = new UnaryUnionOp(geoms, geomFact);
            return op.Union();
        }
	
        public static IGeometry Union(IGeometry geom)
        {
            UnaryUnionOp op = new UnaryUnionOp(geom);
            return op.Union();
        }

        private readonly List<IGeometry> _polygons = new List<IGeometry>();
        private readonly List<IGeometry> _lines = new List<IGeometry>();
        private readonly List<IGeometry> _points = new List<IGeometry>();

        private IGeometryFactory _geomFact;

        public UnaryUnionOp(IEnumerable<IGeometry> geoms)
        {
            Extract(geoms);
        }

        public UnaryUnionOp(IEnumerable<IGeometry> geoms, IGeometryFactory geomFact)
        {
            _geomFact = geomFact;
            Extract(geoms);
        }

        public UnaryUnionOp(IGeometry geom)
        {
            Extract(geom);
        }

        private void Extract(IEnumerable<IGeometry> geoms)
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
            GeometryExtracter.Extract<IPolygon>(geom, _polygons);
            GeometryExtracter.Extract<ILineString>(geom, _lines);
            GeometryExtracter.Extract<IPoint>(geom, _points);
        }

        ///<summary>
        /// Gets the union of the input geometries.
	    /// If no input geometries were provided, a POINT EMPTY is returned.
        ///</summary>
        /// <returns>
        /// <list>
        /// <item>a Geometry containing the union</item>
        /// <item>An empty <see cref="IGeometryCollection"/> if no geometries were provided in the input</item>
        /// </list>
        /// </returns>
        public IGeometry Union()
        {
            if (_geomFact == null)
            {
                return null;
            }

            /**
             * For points and lines, only a single union operation is 
             * required, since the OGC model allowings self-intersecting 
             * MultiPoint and MultiLineStrings.
             * This is not the case for polygons, so Cascaded Union is required.
             */
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

            /*
             * Performing two unions is somewhat inefficient,
             * but is mitigated by unioning lines and points first
             */
            var unionLA = UnionWithNull(unionLines, unionPolygons);
            IGeometry union;
            if (unionPoints == null)
                union = unionLA;
            else if (unionLA == null)
                union = unionPoints;
            else
                union = PointGeometryUnion.Union((IPuntal)unionPoints, unionLA);

            if (union == null)
                return _geomFact.CreateGeometryCollection(null);

            return union;
        }


        //private static ICollection<IGeometry> ToCollection<T>(IEnumerable<T> items) where T : IGeometry
        //{
        //    var ret = new List<IGeometry>();
        //    foreach (IGeometry geometry in items)
        //        ret.Add(geometry);
        //    return ret;
        //}

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
            
            IGeometry empty = _geomFact.CreatePoint((Coordinate)null);
            return OverlayOp.Overlay(g0, empty, SpatialFunction.Union);
        }

    }
}
