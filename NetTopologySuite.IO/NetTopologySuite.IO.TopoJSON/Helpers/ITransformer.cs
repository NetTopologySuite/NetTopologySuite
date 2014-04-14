using NetTopologySuite.Features;
using NetTopologySuite.IO.Geometries;

namespace NetTopologySuite.IO.Helpers
{
    public interface ITransformer
    {
        FeatureCollection Create(TopoObject data);
    }
}