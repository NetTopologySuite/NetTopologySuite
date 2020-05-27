using System.Collections.Generic;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Operation.OverlayNg
{
    /**
     * Computes an overlay where one input is Point(s) and one is not.
     * <p>
     * The semantics are:
     * <ul>
     * <li>Duplicates are removed from Point output 
     * <li>Non-point output is rounded and noded using the given precision model
     * <ii>An empty result is an empty atomic geometry 
     * with dimension determined by the inputs and the operation
     * <li>
     * </ul>
     * For efficiency the following optimizations are used:
     * <ul>
     * <li>Input points are not included in the noding of the non-point input geometry
     * (in particular, they do not participate in snap-rounding if that is used).
     * <li>If the non-point input geometry is not included in the output
     * it is not rounded and noded.  This means that points 
     * are compared to the non-rounded geometry, which will be apparent in the result.
     * </ul>
     * This means that overlay is efficient to use for finding points
     * within or outside a polygon.
     * 
     * @author Martin Davis
     *
     */
    class OverlayMixedPoints
    {

        public static Geometry Overlay(SpatialFunction opCode, Geometry geom0, Geometry geom1, PrecisionModel pm)
        {
            var overlay = new OverlayMixedPoints(opCode, geom0, geom1, pm);
            return overlay.getResult();
        }

        private readonly SpatialFunction _opCode;
        private readonly PrecisionModel _pm;
        private readonly Geometry _geomPoint;
        private readonly Geometry _geomNonPointInput;
        private readonly GeometryFactory _geometryFactory;
        private readonly bool _isPointRhs;
  
        private Geometry _geomNonPoint;
        private Dimension _geomNonPointDim;
        private IPointOnGeometryLocator _locator;
        private readonly Dimension _resultDim;

        public OverlayMixedPoints(SpatialFunction opCode, Geometry geom0, Geometry geom1, PrecisionModel pm)
        {
            this._opCode = opCode;
            this._pm = pm;
            _geometryFactory = geom0.Factory;
            _resultDim = OverlayUtility.resultDimension(opCode, geom0.Dimension, geom1.Dimension);


            // name the dimensional geometries
            if (geom0.Dimension == 0)
            {
                _geomPoint = geom0;
                _geomNonPointInput = geom1;
                _isPointRhs = false;
            }
            else
            {
                _geomPoint = geom1;
                _geomNonPointInput = geom0;
                _isPointRhs = true;
            }
        }

        public Geometry getResult()
        {
            // reduce precision of non-point input, if required
            _geomNonPoint = PrepareNonPoint(_geomNonPointInput);
            _geomNonPointDim = _geomNonPoint.Dimension;
            CreateLocator(_geomNonPoint);

            var coords = ExtractCoordinates(_geomPoint, _pm);

            switch (_opCode)
            {
                case OverlayNG.INTERSECTION:
                    return ComputeIntersection(coords);
                case OverlayNG.UNION:
                case OverlayNG.SYMDIFFERENCE:
                    // UNION and SYMDIFFERENCE have same output
                    return ComputeUnion(coords);
                case OverlayNG.DIFFERENCE:
                    return ComputeDifference(coords);
            }
            Assert.ShouldNeverReachHere("Unknown overlay op code");
            return null;
        }

        private void CreateLocator(Geometry geomNonPoint)
        {
            if (_geomNonPointDim == Dimension.Surface)
            {
                _locator = new IndexedPointInAreaLocator(geomNonPoint);
            }
            else
            {
                _locator = new IndexedPointOnLineLocator(geomNonPoint);
            }
        }

        private Geometry PrepareNonPoint(Geometry geomInput)
        {
            // if non-point not in output no need to node it
            if (_resultDim == 0)
            {
                return geomInput;
            }

            // Node and round the non-point geometry for output
            var geomPrep = OverlayNG.Union(_geomNonPointInput, _pm);
            return geomPrep;
        }

        private Geometry ComputeIntersection(Coordinate[] coords)
        {
            return CreatePointResult(FindPoints(true, coords));
        }

        private Geometry ComputeUnion(Coordinate[] coords)
        {
            var resultPointList = FindPoints(false, coords);
            List<LineString> resultLineList = null;
            if (_geomNonPointDim == Dimension.Point)
            {
                resultLineList = ExtractLines(_geomNonPoint);
            }
            List<Polygon> resultPolyList = null;
            if (_geomNonPointDim == Dimension.Surface)
            {
                resultPolyList = ExtractPolygons(_geomNonPoint);
            }

            return OverlayUtility.createResultGeometry(resultPolyList, resultLineList, resultPointList, _geometryFactory);
        }

        private Geometry ComputeDifference(Coordinate[] coords)
        {
            if (_isPointRhs)
            {
                return CopyNonPoint();
            }
            return CreatePointResult(FindPoints(false, coords));
        }

        private Geometry CreatePointResult(IReadOnlyCollection<Point> points)
        {
            if (points.Count == 0)
            {
                return _geometryFactory.CreateEmpty(0);
            }
            return _geometryFactory.BuildGeometry(points);
        }

        private List<Point> FindPoints(bool isCovered, IEnumerable<Coordinate> coords)
        {
            var resultCoords = new HashSet<Coordinate>();
            // keep only points contained
            foreach (var coord in coords)
            {
                if (HasLocation(isCovered, coord))
                {
                    resultCoords.Add(coord);
                }
            }
            return CreatePoints(resultCoords);
        }

        private List<Point> CreatePoints(IEnumerable<Coordinate> coords)
        {
            var points = new List<Point>();
            foreach (var coord in coords)
            {
                var point = _geometryFactory.CreatePoint(coord);
                points.Add(point);
            }
            return points;
        }

        private bool HasLocation(bool isCovered, Coordinate coord)
        {
            bool isExterior = Location.Exterior == _locator.Locate(coord);
            if (isCovered)
            {
                return !isExterior;
            }
            return isExterior;
        }

        /**
         * Copy the non-point input geometry if not
         * already done by precision reduction process.
         * 
         * @return a copy of the non-point geometry
         */
        private Geometry CopyNonPoint()
        {
            if (_geomNonPointInput != _geomNonPoint)
                return _geomNonPoint;
            return _geomNonPoint.Copy();
        }

        private static Coordinate[] ExtractCoordinates(Geometry points, PrecisionModel pm)
        {
            var coords = new CoordinateList();
            int n = points.NumGeometries;
            for (int i = 0; i < n; i++)
            {
                var point = (Point)points.GetGeometryN(i);
                if (point.IsEmpty) continue;
                var coord = OverlayUtility.round(point, pm);
                coords.Add(coord, true);
            }
            return coords.ToCoordinateArray();
        }

        private static List<Polygon> ExtractPolygons(Geometry geom)
        {
            var list = new List<Polygon>();
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var poly = (Polygon)geom.GetGeometryN(i);
                if (!poly.IsEmpty)
                {
                    list.Add(poly);
                }
            }
            return list;
        }

        private static List<LineString> ExtractLines(Geometry geom)
        {
            var list = new List<LineString>();
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var line = (LineString)geom.GetGeometryN(i);
                if (!line.IsEmpty)
                {
                    list.Add(line);
                }
            }
            return list;
        }
    }
}
