using NetTopologySuite.IO.Geometries;

namespace NetTopologySuite.IO.Builders
{
    internal interface ITopoBuilder
    {
        TopoObject Build();
    }
}