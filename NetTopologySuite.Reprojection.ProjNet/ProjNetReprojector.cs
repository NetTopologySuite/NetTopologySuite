using NetTopologySuite.Geometries;

namespace NetTopologySuite.Reprojection
{
    public class ProjNetReprojector : Reprojector
    {
        public ProjNetReprojector(string definitionKind = null)
            : this(NtsGeometryServices.Instance.DefaultPrecisionModel, definitionKind)
        {
        }

        private ProjNetReprojector(PrecisionModel precisionModel, string definitionKind)
            : this(new EpsgIoSpatialReferenceFactory(
                GeometryFactory.Default.CoordinateSequenceFactory,
                precisionModel, definitionKind ?? "esriwkt"))
        {
        }

        private ProjNetReprojector(SpatialReferenceFactory spatialReferenceFactory)
            : base(spatialReferenceFactory, new ProjNetReprojectionFactory())
        {
        }

        private sealed class ProjNetReprojectionFactory : ReprojectionFactory
        {
            public override Reprojection Create(SpatialReference source, SpatialReference target, bool cache = false)
            {
                return new ProjNetReprojection(source, target);
            }
        }
    }

}
