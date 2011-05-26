using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
#if BUFFERED
using coord = NetTopologySuite.Coordinates.BufferedCoordinate;
using coordFac = NetTopologySuite.Coordinates.BufferedCoordinateFactory;
using coordSeqFac = NetTopologySuite.Coordinates.BufferedCoordinateSequenceFactory;
#else
using coord = NetTopologySuite.Coordinates.Simple.Coordinate;
using coordFac = NetTopologySuite.Coordinates.Simple.CoordinateFactory;
using coordSeqFac = NetTopologySuite.Coordinates.Simple.CoordinateSequenceFactory;
#endif

namespace NetTopologySuite
{
    public static class GeometryServices
    {
        private static readonly Dictionary<PrecisionModelType, IGeometryFactory<coord>> Factories =
            new Dictionary<PrecisionModelType, IGeometryFactory<coord>>();

        public static IGeometryFactory<coord> GetGeometryFactory(PrecisionModelType pmtype)
        {
            if (!Factories.ContainsKey(pmtype))
                Factories.Add(pmtype, CreateGeometryFactory(pmtype));

            return Factories[pmtype];
        }

        private static IGeometryFactory<coord> CreateGeometryFactory(PrecisionModelType pmtype)
        {
            return new GeometryFactory<coord>(
                new coordSeqFac(
                    new coordFac(pmtype))
                );
        }
    }
}