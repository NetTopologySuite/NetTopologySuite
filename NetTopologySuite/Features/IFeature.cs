using GeoAPI.Geometries;

namespace NetTopologySuite.Features
{
    public interface IFeature
    {
        IAttributesTable Attributes { get; set; }

        IGeometry Geometry { get; set; }
    }
}
