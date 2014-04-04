using NetTopologySuite.Features;

namespace NetTopologySuite.IO.ShapeFile.Extended.Entities
{
    public interface IShapefileFeature : IFeature
    {
        long FeatureId { get; }
    }
}
