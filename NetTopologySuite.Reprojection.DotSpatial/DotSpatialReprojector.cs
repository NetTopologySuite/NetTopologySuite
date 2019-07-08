using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;

namespace NetTopologySuite.Reprojection
{
    public class DotSpatialReprojector : Reprojector
    {
        public DotSpatialReprojector(string definitionKind = null)
        :this(NtsGeometryServices.Instance.DefaultPrecisionModel, definitionKind)
        {
        }

        private DotSpatialReprojector(PrecisionModel precisionModel, string definitionKind)
            :this(new EpsgIoSpatialReferenceFactory(
                DotSpatialAffineCoordinateSequenceFactory.Instance,
                precisionModel, definitionKind ?? "esriwkt"))
        {
        }

        private DotSpatialReprojector(SpatialReferenceFactory spatialReferenceFactory)
            :base(spatialReferenceFactory, new DotSpatialReprojectionFactory())
        {
        }

        private sealed class DotSpatialReprojectionFactory : ReprojectionFactory
        {
            public override Reprojection Create(SpatialReference source, SpatialReference target)
            {
                return new DotSpatialReprojection(source, target);
            }
        }
    }
}
