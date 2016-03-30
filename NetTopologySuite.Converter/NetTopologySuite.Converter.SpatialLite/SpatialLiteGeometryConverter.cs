using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Converter
{
    public class SpatialLiteGeometryConverter
    {
        private readonly IGeometryFactory _factory;
        private readonly SpatialLiteCoordinateListConverter _clConverter;

        public SpatialLiteGeometryConverter()
            : this(GeometryFactory.Default)
        {
        }

        public SpatialLiteGeometryConverter(IGeometryFactory factory)
        {
            _factory = factory;
            _clConverter = new SpatialLiteCoordinateListConverter(factory.CoordinateSequenceFactory);
        }

        public SpatialLite.Core.API.IGeometry ToSpatialLiteGeometry(IGeometry geometry)
        {
            switch (geometry.OgcGeometryType)
            {
                case OgcGeometryType.Point:
                    return ToSLPoint((IPoint)geometry);
                case OgcGeometryType.LineString:
                    return ToSLLineString((ILineString) geometry);
                case OgcGeometryType.Polygon:
                    return ToSLPolygon((IPolygon)geometry);
                case OgcGeometryType.MultiPoint:
                    return ToSLMultiPoint((IMultiPoint)geometry);
                case OgcGeometryType.MultiLineString:
                    return ToSLMultiLineString((IMultiLineString)geometry);
                case OgcGeometryType.MultiPolygon:
                    return ToSLMultiPolygon((IMultiPolygon)geometry);
                default:
                    throw new ArgumentException("geometry");
            }
        }

        public IGeometry ToGeoAPIGeometry(SpatialLite.Core.API.IGeometry geometry)
        {
            if (geometry is SpatialLite.Core.API.IPoint)
                return ToGeoAPIPoint((SpatialLite.Core.API.IPoint) geometry);
            if (geometry is SpatialLite.Core.API.ILineString)
                return ToGeoAPILineString((SpatialLite.Core.API.ILineString)geometry);
            if (geometry is SpatialLite.Core.API.IPolygon)
                return ToGeoAPIPolygon((SpatialLite.Core.API.IPolygon)geometry);
            if (geometry is SpatialLite.Core.API.IMultiPoint)
                return ToGeoAPIMultiPoint((SpatialLite.Core.API.IMultiPoint)geometry);
            if (geometry is SpatialLite.Core.API.IMultiLineString)
                return ToGeoAPIMultiLineString((SpatialLite.Core.API.IMultiLineString)geometry);
            if (geometry is SpatialLite.Core.API.IMultiPolygon)
                return ToGeoAPIMultiPolygon((SpatialLite.Core.API.IMultiPolygon)geometry);

            throw new ArgumentException("geometry");
        }

#region ToGeoAPI conversion helpers

        private IGeometry ToGeoAPIMultiPoint(SpatialLite.Core.API.IMultiPoint geometry)
        {
            var lst = new List<IPoint>();
            foreach (var point in geometry.Geometries)
                lst.Add((IPoint)ToGeoAPIPoint(point));

            return _factory.CreateMultiPoint(lst.ToArray());
        }

        private IGeometry ToGeoAPIMultiLineString(SpatialLite.Core.API.IMultiLineString geometry)
        {
            var lst = new List<ILineString>();
            foreach (var line in geometry.Geometries)
                lst.Add((ILineString)ToGeoAPILineString(line));

            return _factory.CreateMultiLineString(lst.ToArray());
        }

        private IGeometry ToGeoAPIMultiPolygon(SpatialLite.Core.API.IMultiPolygon geometry)
        {
            var lst = new List<IPolygon>();
            foreach (var poly in geometry.Geometries)
                lst.Add((IPolygon)ToGeoAPIPolygon(poly));

            return _factory.CreateMultiPolygon(lst.ToArray());
        }

        private IGeometry ToGeoAPIPolygon(SpatialLite.Core.API.IPolygon geometry)
        {
            var exterior = _factory.CreateLinearRing(_clConverter.ToSequence(geometry.ExteriorRing));
            var interior = new List<ILinearRing>();
            foreach (var interiorRing in geometry.InteriorRings)
                interior.Add(_factory.CreateLinearRing(_clConverter.ToSequence(interiorRing)));

            return _factory.CreatePolygon(exterior, interior.ToArray());
        }

        private IGeometry ToGeoAPILineString(SpatialLite.Core.API.ILineString geometry)
        {
            return _factory.CreateLineString(_clConverter.ToSequence(geometry.Coordinates));
        }

        private IGeometry ToGeoAPIPoint(SpatialLite.Core.API.IPoint geometry)
        {
            var lst = new SpatialLite.Core.Geometries.CoordinateList();
            lst.Add(geometry.Position);
            return _factory.CreatePoint(_clConverter.ToSequence(lst));
        }
#endregion

#region ToSpatialLite conversion helpers

        private SpatialLite.Core.API.IGeometry ToSLMultiPolygon(IMultiPolygon geometry)
        {
            var polygons = new List<SpatialLite.Core.Geometries.Polygon>(geometry.NumGeometries);
            for (var i = 0; i < geometry.NumGeometries; i++)
                polygons.Add((SpatialLite.Core.Geometries.Polygon)ToSLPolygon((IPolygon)geometry.GetGeometryN(i)));

            return new SpatialLite.Core.Geometries.MultiPolygon(geometry.SRID, polygons);
        }

        private SpatialLite.Core.API.IGeometry ToSLMultiLineString(IMultiLineString geometry)
        {
            var lines = new List<SpatialLite.Core.Geometries.LineString>(geometry.NumGeometries);
            for (var i = 0; i < geometry.NumGeometries; i++)
                lines.Add((SpatialLite.Core.Geometries.LineString)ToSLLineString((ILineString)geometry.GetGeometryN(i)));

            return new SpatialLite.Core.Geometries.MultiLineString(geometry.SRID, lines);
        }

        private SpatialLite.Core.API.IGeometry ToSLMultiPoint(IMultiPoint geometry)
        {
            var points = new List<SpatialLite.Core.Geometries.Point>(geometry.NumGeometries);
            for (var i = 0; i < geometry.NumGeometries; i++)
                points.Add((SpatialLite.Core.Geometries.Point) ToSLPoint((IPoint)geometry.GetGeometryN(i)));

            return new SpatialLite.Core.Geometries.MultiPoint(geometry.SRID, points);
        }

        private SpatialLite.Core.API.IGeometry ToSLPolygon(IPolygon geometry)
        {
            var clExterior = _clConverter.ToList(geometry.ExteriorRing.CoordinateSequence);
            var clsInterior = new List<SpatialLite.Core.API.ICoordinateList>(geometry.NumInteriorRings);
            for (var i = 0; i < geometry.NumInteriorRings; i++)
                clsInterior.Add(_clConverter.ToList(geometry.GetInteriorRingN(i).CoordinateSequence));

            return new SpatialLite.Core.Geometries.Polygon(geometry.SRID,
                (SpatialLite.Core.Geometries.CoordinateList)clExterior, clsInterior);
        }

        private SpatialLite.Core.API.IGeometry ToSLLineString(ILineString geometry)
        {
            var cl = _clConverter.ToList(geometry.CoordinateSequence);
            return new SpatialLite.Core.Geometries.LineString(geometry.SRID, cl);
        }

        SpatialLite.Core.API.IGeometry ToSLPoint(IPoint point)
        {
            if (point.IsEmpty)
                return new SpatialLite.Core.Geometries.Point(SpatialLite.Core.API.Coordinate.Empty);

            return new SpatialLite.Core.Geometries.Point(
                point.SRID,
                _clConverter.ToCoordinate(point.CoordinateSequence, 0));
        }
#endregion
    }
}