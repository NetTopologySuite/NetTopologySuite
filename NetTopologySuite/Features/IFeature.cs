using GeoAPI.Geometries;

namespace NetTopologySuite.Features
{
    public interface IFeature
    {
        IAttributesTable Attributes { get; }

        IGeometry Geometry { get; }
    }
}
