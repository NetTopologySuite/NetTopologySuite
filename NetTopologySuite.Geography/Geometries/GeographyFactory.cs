using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http.Headers;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using NetTopologySuite.CoordinateSystems.Transformations;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace NetTopologySuite.Geography.Geometries
{
    public class GeographyFactory : GeometryFactory
    {
        private static readonly IGeometryFactory _wgs84 = new GeographyFactory(new PrecisionModel(PrecisionModels.Floating),
            4326, GeometryFactory.GetDefaultCoordinateSequenceFactory());
        public static IGeometryFactory WGS84
        {
            get => _wgs84;
        }

        private GeographyFactory(IPrecisionModel precision, int srid, ICoordinateSequenceFactory factory)
            : base(precision, srid, factory)
        {
            SpatialOperations = new GeographySpatialOperations(this);
        }
    }
}
