using NetTopologySuite.IO.TopoJSON.Geometries;

namespace NetTopologySuite.IO.TopoJSON.Builders
{
    internal interface ITopoBuilder
    {
        TopoObject Build();
    }
}