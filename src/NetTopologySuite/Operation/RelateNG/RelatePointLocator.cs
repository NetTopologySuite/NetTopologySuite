using NetTopologySuite.Algorithm;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace NetTopologySuite.Operation.RelateNG
{
    /// <summary>
    /// Locates a point on a geometry, including mixed-type collections.
    /// The dimension of the containing geometry element is also determined.
    /// GeometryCollections are handled with union semantics;
    /// i.e. the location of a point is that location of that point
    /// on the union of the elements of the collection.
    /// <para/>
    /// Union semantics for GeometryCollections has the following behaviours:
    /// <list type="number">
    /// <item><description>For a mixed-dimension (heterogeneous) collection
    /// a point may lie on two geometry elements with different dimensions.
    /// In this case the location on the largest-dimension element is reported.</description></item>
    /// <item><description>For a collection with overlapping or adjacent polygons,
    /// points on polygon element boundaries may lie in the effective interior
    /// of the collection geometry.</description></item>
    /// </list>
    /// Prepared mode is supported via cached spatial indexes.
    /// </summary>
    /// <author>Martin Davis</author>
    internal class RelatePointLocator
    {

        private readonly Geometry _geom;
        private readonly bool _isPrepared;
        private readonly IBoundaryNodeRule _boundaryRule;
        private AdjacentEdgeLocator _adjEdgeLocator;
        private HashSet<Coordinate> _points;
        private List<LineString> _lines;
        private List<Geometry> _polygons;
        private IPointOnGeometryLocator[] _polyLocator;
        private LinearBoundary _lineBoundary;
        private bool _isEmpty;

        public RelatePointLocator(Geometry geom)
            : this(geom, false, BoundaryNodeRules.OgcSfsBoundaryRule)
        {
        }

        public RelatePointLocator(Geometry geom, bool isPrepared, IBoundaryNodeRule bnRule)
        {
            _geom = geom;
            _isPrepared = isPrepared;
            _boundaryRule = bnRule;
            Init(geom);
        }

        private void Init(Geometry geom)
        {
            //-- cache empty status, since may be checked many times
            _isEmpty = geom.IsEmpty;
            ExtractElements(geom);

            if (_lines != null)
            {
                _lineBoundary = new LinearBoundary(_lines, _boundaryRule);
            }

            if (_polygons != null)
            {
                _polyLocator = new IPointOnGeometryLocator[_polygons.Count];
            }
        }

        public bool HasBoundary =>  _lineBoundary.HasBoundary;

        private void ExtractElements(Geometry geom)
        {
            if (_isEmpty) return;

            if (geom is Point pt) {
                AddPoint(pt);
            }
            else if (geom is LineString ls) {
                AddLine(ls);
            }
            else if (geom is IPolygonal /* geom is Polyogn 
                  || geom is MultiPolygon */) {
                AddPolygonal(geom);
            }
            else if (geom is GeometryCollection){
                for (int i = 0; i < geom.NumGeometries; i++)
                {
                    var g = geom.GetGeometryN(i);
                    ExtractElements(g);
                }
            }
        }

        private void AddPoint(Point pt)
        {
            if (_points == null)
            {
                _points = new HashSet<Coordinate>();
            }
            _points.Add(pt.Coordinate);
        }

        private void AddLine(LineString line)
        {
            if (_lines == null)
            {
                _lines = new List<LineString>();
            }
            _lines.Add(line);
        }

        private void AddPolygonal(Geometry polygonal)
        {
            if (_polygons == null)
            {
                _polygons = new List<Geometry>();
            }
            _polygons.Add(polygonal);
        }

        public Location Locate(Coordinate p)
        {
            return DimensionLocation.Location(LocateWithDim(p));
        }

        public Location LocateLineEnd(Coordinate p)
        {
            return _lineBoundary.IsBoundary(p) ? Location.Boundary : Location.Interior;
        }

        /// <summary>
        /// Locates a point which is known to be a node of the geometry
        /// (i.e. a point or on an edge).
        /// </summary>
        /// <param name="p">The node point to locate</param>
        /// <param name="parentPolygonal">The parent polygonal</param>
        /// <returns>The location of the node point</returns>
        public Location LocateNode(Coordinate p, Geometry parentPolygonal)
        {
            return DimensionLocation.Location(LocateNodeWithDim(p, parentPolygonal));
        }

        public int LocateNodeWithDim(Coordinate p, Geometry parentPolygonal)
        {
            return LocateWithDim(p, true, parentPolygonal);
        }

        /**
         * Computes the topological location ({@link Location}) of a single point
         * in a Geometry, as well as the dimension of the geometry element the point
         * is located in (if not in the Exterior).
         * It handles both single-element and multi-element Geometries.
         * The algorithm for multi-part Geometries
         * takes into account the SFS Boundary Determination Rule.
         *
         * @param p the point to locate
         * @return the {@link Location} of the point relative to the input Geometry
         */
        public int LocateWithDim(Coordinate p)
        {
            return LocateWithDim(p, false, null);
        }

        /**
         * Computes the topological location ({@link Location}) of a single point
         * in a Geometry, as well as the dimension of the geometry element the point
         * is located in (if not in the Exterior).
         * It handles both single-element and multi-element Geometries.
         * The algorithm for multi-part Geometries
         * takes into account the SFS Boundary Determination Rule.
         *
         * @param p the coordinate to locate
         * @param isNode whether the coordinate is a node (on an edge) of the geometry
         * @param polygon 
         * @return the {@link Location} of the point relative to the input Geometry
         */
        private int LocateWithDim(Coordinate p, bool isNode, Geometry parentPolygonal)
        {
            if (_isEmpty) return DimensionLocation.EXTERIOR;

            /*
             * In a polygonal geometry a node must be on the boundary.
             * (This is not the case for a mixed collection, since 
             * the node may be in the interior of a polygon.)
             */
            if (isNode && (_geom is Polygon || _geom is MultiPolygon))
                return DimensionLocation.AREA_BOUNDARY;

            int dimLoc = ComputeDimLocation(p, isNode, parentPolygonal);
            return dimLoc;
        }

        private int ComputeDimLocation(Coordinate p, bool isNode, Geometry parentPolygonal)
        {
            //-- check dimensions in order of precedence
            if (_polygons != null)
            {
                var locPoly = LocateOnPolygons(p, isNode, parentPolygonal);
                if (locPoly != Location.Exterior)
                    return DimensionLocation.LocationArea(locPoly);
            }
            if (_lines != null)
            {
                var locLine = LocateOnLines(p, isNode);
                if (locLine != Location.Exterior)
                    return DimensionLocation.LocationLine(locLine);
            }
            if (_points != null)
            {
                var locPt = LocateOnPoints(p);
                if (locPt != Location.Exterior)
                    return DimensionLocation.LocationPoint(locPt);
            }
            return DimensionLocation.EXTERIOR;
        }

        private Location LocateOnPoints(Coordinate p)
        {
            if (_points.Contains(p))
            {
                return Location.Interior;
            }
            return Location.Exterior;
        }

        private Location LocateOnLines(Coordinate p, bool isNode)
        {
            if (_lineBoundary != null
                  && _lineBoundary.IsBoundary(p))
            {
                return Location.Boundary;
            }
            //-- must be on line, in interior
            if (isNode)
                return Location.Interior;

            //TODO: index the lines
            foreach (var line in _lines)
            {
                //-- have to check every line, since any/all may contain point
                var loc = LocateOnLine(p, isNode, line);
                if (loc != Location.Exterior)
                    return loc;
                //TODO: minor optimization - some BoundaryNodeRules can short-circuit
            }
            return Location.Exterior;
        }

        private Location LocateOnLine(Coordinate p, bool isNode, LineString l)
        {
            // bounding-box check
            if (!l.EnvelopeInternal.Intersects(p))
                return Location.Exterior;

            var seq = l.CoordinateSequence;
            if (PointLocation.IsOnLine(p, seq))
            {
                return Location.Interior;
            }
            return Location.Exterior;
        }

        private Location LocateOnPolygons(Coordinate p, bool isNode, Geometry parentPolygonal)
        {
            int numBdy = 0;
            //TODO: use a spatial index on the polygons
            for (int i = 0; i < _polygons.Count; i++)
            {
                var loc = LocateOnPolygonal(p, isNode, parentPolygonal, i);
                if (loc == Location.Interior)
                {
                    return Location.Interior;
                }
                if (loc == Location.Boundary)
                {
                    numBdy += 1;
                }
            }
            if (numBdy == 1)
            {
                return Location.Boundary;
            }
            //-- check for point lying on adjacent boundaries
            else if (numBdy > 1)
            {
                if (_adjEdgeLocator == null)
                {
                    _adjEdgeLocator = new AdjacentEdgeLocator(_geom);
                }
                return _adjEdgeLocator.Locate(p);
            }
            return Location.Exterior;
        }

        private Location LocateOnPolygonal(Coordinate p, bool isNode, Geometry parentPolygonal, int index)
        {
            var polygonal = _polygons[index];
            if (isNode && parentPolygonal == polygonal)
            {
                return Location.Boundary;
            }
            var locator = GetLocator(index);
            return locator.Locate(p);
        }

        private IPointOnGeometryLocator GetLocator(int index)
        {
            var locator = _polyLocator[index];
            if (locator == null)
            {
                var polygonal = _polygons[index];
                locator = _isPrepared
                    ? (IPointOnGeometryLocator)new IndexedPointInAreaLocator(polygonal)
                    : new SimplePointInAreaLocator(polygonal);
                _polyLocator[index] = locator;
            }
            return locator;
        }

    }
}
