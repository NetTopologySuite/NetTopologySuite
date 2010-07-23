using System;
using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using GisSharpBlog.NetTopologySuite.Operation.Overlay;

namespace GisSharpBlog.NetTopologySuite.Operation.Union
{
    /// <summary>
    /// Unions a collection of Geometry or a single Geometry 
    /// (which may be a collection) together.
    /// By using this special-purpose operation over a collection of geometries
    /// it is possible to take advantage of various optimizations to improve performance.
    /// Heterogeneous {@link GeometryCollection}s are fully supported.
    /// 
    /// The result obeys the following contract:
    /// <list type="Bullet">
    /// <item>Unioning a set of overlapping {@link Polygons}s has the effect of
    /// merging the areas (i.e. the same effect as 
    /// iteratively unioning all individual polygons together).</item>
    /// <item>Unioning a set of {@link LineString}s has the effect of <b>fully noding</b> 
    /// and <b>dissolving</b> the input linework.
    /// In this context "fully noded" means that there will be a node or endpoint in the output 
    /// for every endpoint or line segment crossing in the input.
    /// "Dissolved" means that any duplicate (e.g. coincident) line segments or portions
    /// of line segments will be reduced to a single line segment in the output.  
    /// This is consistent with the semantics of the 
    /// {@link Geometry#union(Geometry)} operation.
    /// If <b>merged</b> linework is required, the {@link LineMerger} class can be used.</item>
    /// 
    /// <item>Unioning a set of {@link Points}s has the effect of merging
    /// all identical points (producing a set with no duplicates).</item>
    /// </list>
    /// </summary>
    public class UnaryUnionOp
    {
        private readonly List<ILineString> _lines = new List<ILineString>();
        private readonly List<IPoint> _points = new List<IPoint>();
        private readonly List<IPolygon> _polygons = new List<IPolygon>();

        private IGeometryFactory _geomFact;

        ///<summary>
        /// Constructs an instance of this class
        ///</summary>
        ///<param name="geoms">an <see cref="IEnumerable{T}"/> of <see cref="IGeometry"/>s</param>
        ///<param name="geomFact">an <see cref="IGeometryFactory"/></param>
        public UnaryUnionOp(IEnumerable<IGeometry> geoms, IGeometryFactory geomFact)
        {
            _geomFact = geomFact;
            Extract(geoms);
        }

        ///<summary>
        /// Constructs an instance of this class
        ///</summary>
        ///<param name="geoms">an <see cref="IEnumerable{T}"/> of <see cref="IGeometry"/>s</param>
        public UnaryUnionOp(IEnumerable<IGeometry> geoms)
        {
            Extract(geoms);
        }


        ///<summary>
        /// Constructs an instance of this class
        ///</summary>
        ///<param name="geom">an <see cref="IGeometry"/></param>
        public UnaryUnionOp(IGeometry geom)
        {
            Extract(geom);
        }

        ///<summary>
        ///</summary>
        ///<param name="geoms"></param>
        ///<returns></returns>
        public static IGeometry Union(IEnumerable<IGeometry> geoms)
        {
            UnaryUnionOp op = new UnaryUnionOp(geoms);
            return op.Union();
        }

        ///<summary>
        ///</summary>
        ///<param name="geoms"></param>
        ///<param name="geomFact"></param>
        ///<returns></returns>
        public static IGeometry Union(IEnumerable<IGeometry> geoms,
                                                   IGeometryFactory geomFact)
        {
            UnaryUnionOp op = new UnaryUnionOp(geoms, geomFact);
            return op.Union();
        }

        ///<summary>
        ///</summary>
        ///<param name="geom"></param>
        ///<returns></returns>
        public static IGeometry Union(IGeometry geom)
        {
            UnaryUnionOp op = new UnaryUnionOp(geom);
            return op.Union();
        }

        private void Extract(IEnumerable<IGeometry> geoms)
        {
            foreach (IGeometry geom in geoms)
                Extract(geom);
        }

        private void Extract(IGeometry geom)
        {
            if (_geomFact == null)
            {
                _geomFact = geom.Factory;
            }

            /*
            PolygonExtracter.getPolygons(geom, polygons);
            LineStringExtracter.getLines(geom, lines);
            PointExtracter.getPoints(geom, points);
            */
            _polygons.AddRange(PolygonExtracter.GetPolygons(geom).Cast<IPolygon>());
            _lines.AddRange(LinearComponentExtracter.GetLines(geom).Cast<ILineString>());
            _points.AddRange(PointExtracter.GetPoints(geom).Cast<IPoint>());
        }

        ///<summary>
        /// Gets the union of the input geometries.
        /// If no input geometries were provided, a POINT EMPTY is returned.
        ///</summary>
        ///<returns>a Geometry containing the union</returns>
        /// <returns>an empty GEOMETRYCOLLECTION if no geometries were provided in the input</returns>
        public IGeometry Union()
        {
            if (_geomFact == null)
            {
                return null;
            }

            IGeometry unionPoints = null;

            if (_points.Count > 0)
            {
                IGeometry ptGeom =
                    _geomFact.BuildGeometry(convertPoints(_points).ToList());
                unionPoints = UnionNoOpt(ptGeom);
            }

            IGeometry unionLines = null;

            if (_lines.Count > 0)
            {
                IGeometry lineGeom = _geomFact.BuildGeometry(convertLineStrings(_lines).ToList());
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
            IGeometry unionLA = UnionWithNull(unionLines, unionPolygons);
            IGeometry union = null;

            if (unionPoints == null)
            {
                union = unionLA;
            }
            else if (unionLA == null)
            {
                union = unionPoints;
            }
            else
            {
                union = PointGeometryUnion.Union((IPoint) unionPoints, unionLA);
            }

            if (union == null)
            {
                return _geomFact.CreateGeometryCollection(null);
            }

            return union;
        }

        private IEnumerable<IGeometry> convertPoints(IEnumerable<IPoint> geom)
        {
            foreach (IPoint point in geom)
            {
                yield return point;
            }
        }

        private IEnumerable<IGeometry> convertLineStrings(IEnumerable<ILineString> geom)
        {
            foreach (ILineString point in geom)
            {
                yield return point;
            }
        }

        /// <summary>
        /// Computes the union of two geometries, either of both of which may be null.
        /// </summary>
        /// <param name="g0">a <see cref="IGeometry"/></param>
        /// <param name="g1">a <see cref="IGeometry"/></param>
        /// <returns>the union of the input(s)</returns>
        /// <returns>null if both inputs are null</returns>
        private static IGeometry UnionWithNull(IGeometry g0, IGeometry g1)
        {
            if (g0 == null && g1 == null)
            {
                return null;
            }

            if (g1 == null)
            {
                return g0;
            }

            if (g0 == null)
            {
                return g1;
            }

            return g0.Union(g1);
        }

        /**
        ///Computes a unary union with no extra optimization,
        ///and no short-circuiting.
        ///Due to the way the overlay operations 
        ///are implemented, this is still efficient in the case of linear 
        ///and puntal geometries.
        ///
        ///@param g0 a geometry
        ///@return the union of the input geometry
         */

        private IGeometry UnionNoOpt(IGeometry g0)
        {
            IGeometry empty = _geomFact.CreatePoint((ICoordinate)null);
            return OverlayOp.Overlay(g0, empty, SpatialFunction.Union);
        }
    }
}
