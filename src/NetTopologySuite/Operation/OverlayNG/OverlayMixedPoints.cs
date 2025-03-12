using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Operation.OverlayNG
{
    /// <summary>
    /// Computes an overlay where one input is Point(s) and one is not.
    /// This class supports overlay being used as an efficient way
    /// to find points within or outside a polygon.
    /// <para/>
    /// Input semantics are:
    /// <list type="bullet">
    /// <item><description>Duplicates are removed from Point output</description></item>
    /// <item><description>Non-point output is rounded and noded using the given precision model</description></item>
    /// </list>
    /// Output semantics are:
    /// <list type="bullet">
    /// <item><description>An empty result is an empty atomic geometry
    /// with dimension determined by the inputs and the operation as per overlay semantics
    /// </description></item>
    /// </list>
    /// For efficiency the following optimizations are used:
    /// <list type="bullet">
    /// <item><description>Input points are not included in the noding of the non-point input geometry
    /// (in particular, they do not participate in snap-rounding if that is used).</description></item>
    /// <item><description>If the non-point input geometry is not included in the output
    /// it is not rounded and noded.This means that points
    /// are compared to the non-rounded geometry.
    /// This will be apparent in the result.</description></item>
    /// </list>
    /// This means that overlay is efficient to use for finding points
    /// within or outside a polygon.
    /// </summary>
    /// <author>Martin Davis</author>
    internal sealed class OverlayMixedPoints
    {

        public static Geometry Overlay(SpatialFunction opCode, Geometry geom0, Geometry geom1, PrecisionModel pm)
        {
            var overlay = new OverlayMixedPoints(opCode, geom0, geom1, pm);
            return overlay.GetResult();
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
            _opCode = opCode;
            _pm = pm;
            _geometryFactory = geom0.Factory;
            _resultDim = OverlayUtility.ResultDimension(opCode, geom0.Dimension, geom1.Dimension);


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

        public Geometry GetResult()
        {
            // reduce precision of non-point input, if required
            _geomNonPoint = PrepareNonPoint(_geomNonPointInput);
            _geomNonPointDim = _geomNonPoint.Dimension;
            _locator = CreateLocator(_geomNonPoint);

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

        private IPointOnGeometryLocator CreateLocator(Geometry geomNonPoint)
        {
            if (_geomNonPointDim == Dimension.Surface)
            {
                return new IndexedPointInAreaLocator(geomNonPoint);
            }
            else
            {
                return new IndexedPointOnLineLocator(geomNonPoint);
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
            if (_geomNonPointDim == Dimension.Curve)
            {
                resultLineList = ExtractLines(_geomNonPoint);
            }
            List<Polygon> resultPolyList = null;
            if (_geomNonPointDim == Dimension.Surface)
            {
                resultPolyList = ExtractPolygons(_geomNonPoint);
            }

            return OverlayUtility.CreateResultGeometry(resultPolyList, resultLineList, resultPointList, _geometryFactory);
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
            if (points.Count == 1)
            {
                return points.First();
            }

            var pointsArray = GeometryFactory.ToPointArray(points);
            return _geometryFactory.CreateMultiPoint(pointsArray);
        }

        private List<Point> FindPoints(bool isCovered, IEnumerable<Coordinate> coords)
        {
            var resultCoords = new HashSet<Coordinate>();
            // keep only points contained
            foreach (var coord in coords)
            {
                if (HasLocation(isCovered, coord))
                {
                    // copy coordinate to avoid aliasing
                    resultCoords.Add(coord.Copy());
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

        /// <summary>
        /// Copy the non-point input geometry if not
        /// already done by precision reduction process.
        /// </summary>
        /// <returns>A copy of the non-point geometry</returns>
        private Geometry CopyNonPoint()
        {
            if (_geomNonPointInput != _geomNonPoint)
                return _geomNonPoint;
            return _geomNonPoint.Copy();
        }

        private static Coordinate[] ExtractCoordinates(Geometry points, PrecisionModel pm)
        {
            var coords = new CoordinateList();
            points.Apply(new CoordinateFilter(coord =>
            {
                var p = OverlayUtility.Round(coord, pm);
                coords.Add(p, false);
            }));
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
