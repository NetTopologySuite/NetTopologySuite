using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetTopologySuite.Operation.RelateNG
{
    internal class RelateGeometry
    {

        public const bool GEOM_A = true;
        public const bool GEOM_B = false;

        public static string Name(bool isA)
        {
            return isA ? "A" : "B";
        }

        private readonly Geometry _geom;
        private readonly bool _isPrepared;

        private readonly Envelope _geomEnv;
        private Dimension _geomDim;// = Dimension.False;
        private HashSet<Coordinate> _uniquePoints;
        private readonly IBoundaryNodeRule _boundaryNodeRule;
        private RelatePointLocator _locator;
        private int _elementId = 0;
        private bool _hasPoints;
        private bool _hasLines;
        private bool _hasAreas;
        private readonly bool _isLineZeroLen;
        private readonly bool _isGeomEmpty;

        public RelateGeometry(Geometry input)
        : this(input, false, BoundaryNodeRules.OgcSfsBoundaryRule)
        {
        }

        public RelateGeometry(Geometry input, IBoundaryNodeRule bnRule)
            : this(input, false, bnRule)
        {
        }

        public RelateGeometry(Geometry input, bool isPrepared, IBoundaryNodeRule bnRule)
        {
            _geom = input;
            _geomEnv = input.EnvelopeInternal;
            _isPrepared = isPrepared;
            _boundaryNodeRule = bnRule;
            //-- cache geometry metadata
            _isGeomEmpty = _geom.IsEmpty;
            _geomDim = input.Dimension;
            AnalyzeDimensions();
            _isLineZeroLen = IsZeroLengthLine(_geom);
        }

        private bool IsZeroLengthLine(Geometry geom)
        {
            // avoid expensive zero-length calculation if not linear
            if (Dimension != Dimension.L)
                return false;
            return IsZeroLength(geom);
        }

        private void AnalyzeDimensions()
        {
            if (_isGeomEmpty)
            {
                return;
            }
            if (_geom is IPuntal) { // (geom is Point || geom is MultiPoint)
                _hasPoints = true;
                _geomDim = Dimension.P;
                return;
            }
            if (_geom is ILineal) { // ( geom is LineString || geom is MultiLineString) {
                _hasLines = true;
                _geomDim = Dimension.L;
                return;
            }
            if (_geom is IPolygonal) { // (geom instanceof Polygon || geom instanceof MultiPolygon)
                _hasAreas = true;
                _geomDim = Dimension.A;
                return;
            }
            //-- analyze a (possibly mixed type) collection
            foreach (var elem in new GeometryCollectionEnumerator(_geom))
            {
                if (elem.IsEmpty)
                    continue;
                if (elem is Point) {
                    _hasPoints = true;
                    if (_geomDim < Dimension.P) _geomDim = Dimension.P;
                }
                if (elem is LineString) {
                    _hasLines = true;
                    if (_geomDim < Dimension.L) _geomDim = Dimension.L;
                }
                if (elem is Polygon) {
                    _hasAreas = true;
                    if (_geomDim < Dimension.A) _geomDim = Dimension.A;
                }
            }
        }

        /// <summary>
        /// Tests if all geometry linear elements are zero-length.
        /// For efficiency the test avoids computing actual length.
        /// </summary>
        private static bool IsZeroLength(Geometry geom)
        {
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var elem = geom.GetGeometryN(i);
                if (elem is LineString ls) {
                    if (!IsZeroLength(ls))
                        return false;
                }
            }
            return true;
        }

        private static bool IsZeroLength(LineString line)
        {
            if (line.NumPoints >= 2)
            {
                var p0 = line.GetCoordinateN(0);
                for (int i = 1; i < line.NumPoints; i++)
                {
                    var pi = line.GetCoordinateN(i);
                    //-- most non-zero-len lines will trigger this right away 
                    if (!p0.Equals2D(pi))
                        return false;
                }
            }
            return true;
        }


        public Geometry Geometry { get => _geom; }

        public bool IsPrepared { get => _isPrepared; }

        public Envelope Envelope { get => _geomEnv; }

        public Dimension Dimension { get => _geomDim; }

        public bool HasDimension(Dimension dim)
        {
            switch (dim)
            {
                case Dimension.P: return _hasPoints;
                case Dimension.L: return _hasLines;
                case Dimension.A: return _hasAreas;
            }
            return false;
        }

        /// <summary>
        /// Gets the actual non-empty dimension of the geometry.
        /// Zero-length <c>LineString</c>s are treated as <c>Point</c>s.
        /// </summary>
        public Dimension DimensionReal
        {
            get
            {
                if (_isGeomEmpty) return Dimension.False;
                if (Dimension == Dimension.L && _isLineZeroLen)
                    return Dimension.P;
                if (_hasAreas) return Dimension.A;
                if (_hasLines) return Dimension.L;
                return Dimension.P;
            }
        }

        public bool HasEdges { get => _hasLines || _hasAreas; }

        private RelatePointLocator GetLocator()
        {
            if (_locator == null)
                _locator = new RelatePointLocator(_geom, _isPrepared, _boundaryNodeRule);
            return _locator;
        }

        public bool IsNodeInArea(Coordinate nodePt, Geometry parentPolygonal)
        {
            int loc = GetLocator().LocateNodeWithDim(nodePt, parentPolygonal);
            return loc == DimensionLocation.AREA_INTERIOR;
        }

        public int LocateLineEndWithDim(Coordinate p)
        {
            return GetLocator().LocateLineEndWithDim(p);
        }

        /// <summary>
        /// Locates a vertex of a polygon.
        /// <para/>
        /// A vertex of a <c>Polygon</c> or <c>MultiPolygon</c> is on
        /// the <see cref="Location.Boundary"/>.
        /// But a vertex of an overlapped polygon in a <c>GeometryCollection</c>
        /// may be in the <see cref="Location.Interior"/>.
        /// </summary>
        /// <param name="pt">The polygon vertex</param>
        /// <returns>The location of the vertex</returns>
        public Location LocateAreaVertex(Coordinate pt)
        {
            /*
             * Can pass a null polygon, because the point is an exact vertex,
             * which will be detected as being on the boundary of its polygon
             */
            return LocateNode(pt, null);
        }

        public Location LocateNode(Coordinate pt, Geometry parentPolygonal)
        {
            return GetLocator().LocateNode(pt, parentPolygonal);
        }

        public int LocateWithDim(Coordinate pt)
        {
            int loc = GetLocator().LocateWithDim(pt);
            return loc;
        }

        /// <summary>
        /// Gets a value that indicates whether the geometry requires self-noding
        /// for correct evaluation of specific spatial predicates.
        /// Self-noding is required for geometries which may self-cross
        /// - i.e.lines, and overlapping elements in GeometryCollections.
        /// Self-noding is not required for polygonal geometries,
        /// since they can only touch at vertices.
        /// </summary>
        public bool IsSelfNodingRequired
        {
            get
            {
                if (_geom is IPuntal ||
                    _geom is IPolygonal)
                    return false;

                //-- a GC with a single polygon does not need noding
                if (_hasAreas && _geom.NumGeometries == 1)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Tests whether the geometry has polygonal topology.
        /// This is not the case if it is a GeometryCollection
        /// containing more than one polygon (since they may overlap
        /// or be adjacent).
        /// The significance is that polygonal topology allows more assumptions
        /// about the location of boundary vertices.
        /// </summary>
        public bool IsPolygonal
        {
            get {
                //TODO: also true for a GC containing one polygonal element (and possibly some lower-dimension elements)
                return _geom is IPolygonal;
                //return geom is Polygon
                //    || geom is MultiPolygon;
            }
        }

        public bool IsEmpty { get => _isGeomEmpty; }

        public bool HasBoundary
        {
            get => GetLocator().HasBoundary;
        }

        public ISet<Coordinate> UniquePoints
        {
            get
            {
                //-- will be re-used in prepared mode
                if (_uniquePoints == null)
                {
                    _uniquePoints = CreateUniquePoints();
                }
                return _uniquePoints;
            }
        }

        private HashSet<Coordinate> CreateUniquePoints()
        {
            //-- only called on P geometries
            var pts = Geometries.Utilities.ComponentCoordinateExtracter.GetCoordinates(_geom);
            var set = new HashSet<Coordinate>(pts);
            return set;
        }

        public IReadOnlyCollection<Point> GetEffectivePoints()
        {
            var ptListAll = new List<Point>(Geometries.Utilities.PointExtracter.GetPoints(_geom).Cast<Point>());

            if (DimensionReal <= Dimension.P)
                return ptListAll;

            //-- only return Points not covered by another element
            var ptList = new List<Point>();
            foreach (var p in ptListAll)
            {
                if (p.IsEmpty)
                    continue;
                int locDim = LocateWithDim(p.Coordinate);
                if (DimensionLocation.Dimension(locDim) == Dimension.P)
                {
                    ptList.Add(p);
                }
            }
            return ptList;
        }

        /// <summary>
        /// Extract RelateSegmentStrings from the geometry which
        /// intersect a given envelope.
        /// If the envelope is null all edges are extracted.
        /// </summary>
        /// <param name="isA">Flag indicating if this </param>
        /// <param name="env"></param>
        /// <returns>A list of <c>RelateSegmentString</c>s</returns>
        public IList<RelateSegmentString> ExtractSegmentStrings(bool isA, Envelope env)
        {
            var segStrings = new List<RelateSegmentString>();
            ExtractSegmentStrings(isA, env, _geom, segStrings);
            return segStrings;
        }

        private void ExtractSegmentStrings(bool isA, Envelope env, Geometry geom, List<RelateSegmentString> segStrings)
        {
            //-- record if parent is MultiPolygon
            var parentPolygonal = geom as MultiPolygon;

            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var g = geom.GetGeometryN(i);
                if (g is GeometryCollection) {
                    ExtractSegmentStrings(isA, env, g, segStrings);
                }
                else
                {
                    ExtractSegmentStringsFromAtomic(isA, g, parentPolygonal, env, segStrings);
                }
            }
        }

        private void ExtractSegmentStringsFromAtomic(bool isA, Geometry geom, MultiPolygon parentPolygonal, Envelope env,
            List<RelateSegmentString> segStrings)
        {
            if (geom.IsEmpty)
                return;
            bool doExtract = env == null || env.Intersects(geom.EnvelopeInternal);
            if (!doExtract)
                return;

            _elementId++;
            if (geom is LineString) {
                var ss = RelateSegmentString.CreateLine(geom.Coordinates, isA, _elementId, this);
                segStrings.Add(ss);
            }
            else if (geom is Polygon poly) {
                var parentPoly = parentPolygonal != null ? (Geometry)parentPolygonal : poly;
                ExtractRingToSegmentString(isA, (LinearRing)poly.ExteriorRing, 0, env, parentPoly, segStrings);
                for (int i = 0; i < poly.NumInteriorRings; i++)
                {
                    ExtractRingToSegmentString(isA, (LinearRing)poly.GetInteriorRingN(i), i + 1, env, parentPoly, segStrings);
                }
            }
        }

        private void ExtractRingToSegmentString(bool isA, LinearRing ring, int ringId, Envelope env,
            Geometry parentPoly, List<RelateSegmentString> segStrings)
        {
            if (ring.IsEmpty)
                return;
            if (env != null && !env.Intersects(ring.EnvelopeInternal))
                return;

            //-- orient the points if required
            bool requireCW = ringId == 0;
            var pts = Orient(ring.Coordinates, requireCW);
            var ss = RelateSegmentString.CreateRing(pts, isA, _elementId, ringId, parentPoly, this);
            segStrings.Add(ss);
        }

        public static Coordinate[] Orient(Coordinate[] pts, bool orientCW)
        {
            bool isFlipped = orientCW == Orientation.IsCCW(pts);
            if (isFlipped)
            {
                var newPts = new Coordinate[pts.Length];
                Array.Copy(pts, newPts, pts.Length);
                CoordinateArrays.Reverse(newPts);
                return newPts;
            }
            return pts;
        }

        public override string ToString()
        {
            return _geom.ToString();
        }
    }
}


