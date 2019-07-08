using System;
using DotSpatial.Projections;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;

namespace NetTopologySuite.Reprojection
{
    public class DotSpatialReprojection : Reprojection
    {

        private readonly ProjectionInfo _sourceProjectionInfo;
        private readonly ProjectionInfo _targetProjectionInfo;

        public DotSpatialReprojection(SpatialReference source, SpatialReference target)
            :base(source, target)
        {
            _sourceProjectionInfo = CreateProjectionInfo(source);
            _targetProjectionInfo = CreateProjectionInfo(target);
        }

        private static ProjectionInfo CreateProjectionInfo(SpatialReference spatialReference)
        {
            switch (spatialReference.DefinitionKind)
            {
                case "esriwkt":
                    return ProjectionInfo.FromEsriString(spatialReference.Definition);
                case "proj4":;
                    return ProjectionInfo.FromProj4String(spatialReference.Definition);
            }

            throw new ArgumentException(nameof(spatialReference));
        }

        protected override CoordinateSequence Apply(CoordinateSequence coordinateSequence)
        {
            var dsseq = coordinateSequence as DotSpatialAffineCoordinateSequence;
            if (dsseq == null)
                throw new ArgumentException(nameof(coordinateSequence));

            dsseq = (DotSpatialAffineCoordinateSequence)dsseq.Copy();
            Reproject.ReprojectPoints(dsseq.XY, dsseq.Z, _sourceProjectionInfo, _targetProjectionInfo, 0, coordinateSequence.Count);

            return dsseq;
        }

        
    }
}
