using System;
#if useFullGeoAPI
using GeoAPI.Geometries;
#else
using ICoordinate = NetTopologySuite.Geometries.Coordinate;
using IEnvelope = NetTopologySuite.Geometries.Envelope;
using IGeometry = NetTopologySuite.Geometries.Geometry;
using IPoint = NetTopologySuite.Geometries.Point;
using ILineString = NetTopologySuite.Geometries.LineString;
using ILinearRing = NetTopologySuite.Geometries.LinearRing;
using IPolygon = NetTopologySuite.Geometries.Polygon;
using IGeometryCollection = NetTopologySuite.Geometries.GeometryCollection;
using IMultiPoint = NetTopologySuite.Geometries.MultiPoint;
using IMultiLineString = NetTopologySuite.Geometries.MultiLineString;
using IMultiPolygon = NetTopologySuite.Geometries.MultiPolygon;
using IGeometryFactory = NetTopologySuite.Geometries.GeometryFactory;
using IPrecisionModel = NetTopologySuite.Geometries.PrecisionModel;
#endif
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Buffer.Validate
{
    /**
     * Contains a pair of points and the distance between them.
     * Provides methods to update with a new point pair with
     * either maximum or minimum distance.
     */
    public class PointPairDistance
    {

        private readonly ICoordinate[] _pt = { new Coordinate(), new Coordinate() };
        private double _distance = Double.NaN;
        private bool _isNull = true;

        public void Initialize() { _isNull = true; }

        public void Initialize(ICoordinate p0, ICoordinate p1)
        {
            _pt[0].CoordinateValue = p0;
            _pt[1].CoordinateValue = p1;
            _distance = p0.Distance(p1);
            _isNull = false;
        }

        /**
         * Initializes the points, avoiding recomputing the distance.
         * @param p0
         * @param p1
         * @param distance the distance between p0 and p1
         */
        private void Initialize(ICoordinate p0, ICoordinate p1, double distance)
        {
            _pt[0].CoordinateValue = p0;
            _pt[1].CoordinateValue = p1;
            _distance = distance;
            _isNull = false;
        }

        public double Distance { get { return _distance; } }

        public ICoordinate[] Coordinates { get { return _pt; } }

        public ICoordinate GetCoordinate(int i) { return _pt[i]; }

        public void SetMaximum(PointPairDistance ptDist)
        {
            SetMaximum(ptDist._pt[0], ptDist._pt[1]);
        }

        public void SetMaximum(ICoordinate p0, ICoordinate p1)
        {
            if (_isNull)
            {
                Initialize(p0, p1);
                return;
            }
            double dist = p0.Distance(p1);
            if (dist > _distance)
                Initialize(p0, p1, dist);
        }

        public void SetMinimum(PointPairDistance ptDist)
        {
            SetMinimum(ptDist._pt[0], ptDist._pt[1]);
        }

        public void SetMinimum(ICoordinate p0, ICoordinate p1)
        {
            if (_isNull)
            {
                Initialize(p0, p1);
                return;
            }
            double dist = p0.Distance(p1);
            if (dist < _distance)
                Initialize(p0, p1, dist);
        }
    }
}