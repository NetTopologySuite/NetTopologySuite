using GeoAPI.Geometries;
using NetTopologySuite.Operation;

namespace NetTopologySuite.Geometries
{
    public interface IGeometryFactoryEx : IGeometryFactory
    {
        ISpatialOperations SpatialOperations { get; }
    }
}
