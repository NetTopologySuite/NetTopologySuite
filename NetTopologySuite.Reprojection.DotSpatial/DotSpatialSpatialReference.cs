using DotSpatial.Projections;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Reprojection
{
    public class DotSpatialSpatialReference : SpatialReference
    {
        internal DotSpatialSpatialReference(string definition, GeometryFactory factory, ProjectionInfo info)
            : base(definition, factory)
        {
            Info = info;
        }

        internal ProjectionInfo Info { get; }
    }
}