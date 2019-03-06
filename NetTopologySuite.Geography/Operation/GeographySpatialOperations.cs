using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using NetTopologySuite.CoordinateSystems.Transformations;
using NetTopologySuite.Geography;
using NetTopologySuite.Geography.Geometries;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace NetTopologySuite.Operation
{
    internal class GeographySpatialOperations : SpatialOperations
    {
        private readonly ICoordinateSystem _geocs;

        private readonly Dictionary<Tuple<int, int>, IMathTransform> _transforms = new Dictionary<Tuple<int, int>, IMathTransform>();
        private readonly Dictionary<int, IGeometryFactory> _factories = new Dictionary<int, IGeometryFactory>();

        internal GeographySpatialOperations(GeographyFactory factory)
            : base(factory)
        {
            switch (factory.SRID)
            {
                case 4326:
                    _geocs = GeographicCoordinateSystem.WGS84;
                    _factories[4326] = factory;
                    break;
                default:
                    throw new ArgumentException(nameof(factory));
            }

        }

        protected override void PrepareInput(IGeometry g1, out IGeometry g1Out)
        {
            Debug.Assert(g1.Factory == Factory);

            var pt = (LatLon)g1.EnvelopeInternal.Centre;
            GetTransformation(pt, out var mt, out var factory);
            g1Out = GeometryTransform.TransformGeometry(factory, g1, mt);
        }

        protected override void PrepareInput(IGeometry g1, IGeometry g2, out IGeometry g1Out, out IGeometry g2Out)
        {
            Debug.Assert(g1.Factory == Factory);
            Debug.Assert(g2.Factory == Factory);

            var pt = (LatLon)g1.EnvelopeInternal.ExpandedBy(g2.EnvelopeInternal).Centre;
            GetTransformation(pt, out var mt, out var factory);
            g1Out = GeometryTransform.TransformGeometry(factory, g1, mt);
            g2Out = GeometryTransform.TransformGeometry(factory, g2, mt);
        }



        private void GetTransformation(LatLon pt, out IMathTransform mt, out IGeometryFactory factory)
        {
            int zone = (int)((pt.Lon + 180.0) / 6.0 + 1.0);
            bool zoneIsNorth = pt.Lat > 0;
            int code = 32600 + zone + (zoneIsNorth ? 0 : 100);

            var tpl = Tuple.Create(Factory.SRID, code);
            if (!_transforms.TryGetValue(tpl, out mt))
            {
                var ctFactory = new CoordinateTransformationFactory();
                var cs = ProjectedCoordinateSystem.WGS84_UTM(zone, zoneIsNorth);
                var ct = ctFactory.CreateFromCoordinateSystems(_geocs, cs);
                _transforms[tpl] = mt = ct.MathTransform;
                ct = ctFactory.CreateFromCoordinateSystems(cs, _geocs);
                _transforms[Tuple.Create(code, Factory.SRID)] = ct.MathTransform;

                factory = new GeometryFactory(Factory.PrecisionModel, code, Factory.CoordinateSequenceFactory);
                _factories[code] = factory;
            }
            else
                factory = _factories[code];
        }

        protected override IGeometry CreateResult(IGeometry geometry)
        {
            var mts = _transforms[Tuple.Create(geometry.SRID, Factory.SRID)];
            return GeometryTransform.TransformGeometry(Factory, geometry, mts);
        }

        public override double Distance(IGeometry g1, IGeometry g2)
        {

            if (g1 is IPoint p1 && g2 is IPoint p2)
                return Algorithm.LatLonDistance.Distance(p1.Coordinate, p2.Coordinate);

            return base.Distance(g1, g2);
        }

        public override double Length(IGeometry geometry)
        {
            //return base.Length(geometry);

            double length = 0d;
            switch (geometry)
            {
                case IGeometryCollection gc:
                {
                    for (int i = 0; i < gc.NumGeometries; i++)
                        length += Length(gc.GetGeometryN(i));
                    break;
                }
                case IPolygon p:
                {
                    length += Algorithm.LatLonLength.OfLine(p.ExteriorRing.CoordinateSequence);
                    for (int i = 0; i < p.NumInteriorRings; i++)
                        length -= Algorithm.LatLonLength.OfLine(p.GetInteriorRingN(i).CoordinateSequence);
                    return length;
                }
                case ILineString l:
                    length += Algorithm.LatLonLength.OfLine(l.CoordinateSequence);
                    return length;
            }

            return length;
        }
    }

}
