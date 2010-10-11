using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Coordinates;

namespace NetTopologySuite
{
    public static class GeometryServices
    {
        private static readonly Dictionary<PrecisionModelType, IGeometryFactory<BufferedCoordinate>> _factories =
            new Dictionary<PrecisionModelType, IGeometryFactory<BufferedCoordinate>>();

        public static IGeometryFactory<BufferedCoordinate> GetGeometryFactory(PrecisionModelType pmtype)
        {
            if (!_factories.ContainsKey(pmtype))
                _factories.Add(pmtype, CreateGeometryFactory(pmtype));

            return _factories[pmtype];
        }

        private static IGeometryFactory<BufferedCoordinate> CreateGeometryFactory(PrecisionModelType pmtype)
        {
            return new GeometryFactory<BufferedCoordinate>(
                new BufferedCoordinateSequenceFactory(
                    new BufferedCoordinateFactory(pmtype))
                );
        }
    }
}