using NetTopologySuite.Features;
using NetTopologySuite.IO.TopoJSON.Geometries;

namespace NetTopologySuite.IO.TopoJSON.Helpers
{
    public interface ITransformer
    {
        FeatureCollection Create(TopoObject data);
    }
}