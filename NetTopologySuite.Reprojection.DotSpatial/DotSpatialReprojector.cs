using System;
using DotSpatial.Projections;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;

namespace NetTopologySuite.Reprojection
{
    public class DotSpatialReprojector : Reprojector
    {
        public DotSpatialReprojector()
        :this(NtsGeometryServices.Instance.DefaultPrecisionModel)
        {
            LookUp = new EpsgIoSpatialReferenceLookUp("esriwkt");
        }

        private DotSpatialReprojector(PrecisionModel precisionModel)
            :base(DotSpatialAffineCoordinateSequenceFactory.Instance, precisionModel)
        {
            LookUp = new EpsgIoSpatialReferenceLookUp("esriwkt");
        }

        protected override SpatialReference Create(string definition, int srid)
        {
            ProjectionInfo pi;
            switch (LookUp.DefinitionKind)
            {
                case "wkt":
                case "esriwkt":  // <-- this is the one!
                case "prettywkt":
                    pi = ProjectionInfo.FromEsriString(definition);
                    break;
                case "proj4":
                    pi = ProjectionInfo.FromProj4String(definition);
                    break;

                default:
                    throw new InvalidOperationException();
            }

            pi.Authority = "EPSG";
            pi.AuthorityCode = srid;

            return new DotSpatialSpatialReference(definition, new GeometryFactory(PrecisionModel, srid, CoordinateSequenceFactory), pi);
        }

        public override Coordinate Reproject(Coordinate coordinate, SpatialReference @from, SpatialReference to)
        {
            var dsFrom = CastSpatialReference<DotSpatialSpatialReference>(from);
            var dsTo = CastSpatialReference<DotSpatialSpatialReference>(to);

            if (!CheckArguments(coordinate, nameof(coordinate), dsFrom, dsTo, out var e))
                throw e;

            double[] xy = new[] { coordinate.X, coordinate.Y };
            double[] z = new[] { coordinate.Z };

            DotSpatial.Projections.Reproject.ReprojectPoints(xy, z, dsFrom.Info, dsTo.Info, 0, 1);

            var res = coordinate.Copy();
            res.X = to.Factory.PrecisionModel.MakePrecise(xy[0]);
            res.Y = to.Factory.PrecisionModel.MakePrecise(xy[1]);
            if (res is CoordinateZ)
                res.Z = z[0];

            return res;
        }

        public override Envelope Reproject(Envelope envelope, SpatialReference @from, SpatialReference to)
        {
            var dsFrom = CastSpatialReference<DotSpatialSpatialReference>(from);
            var dsTo = CastSpatialReference<DotSpatialSpatialReference>(to);

            if (!CheckArguments(envelope, nameof(envelope), dsFrom, dsTo, out var e))
                throw e;

            double[] xy = { envelope.MinX, envelope.MinY, envelope.MaxX, envelope.MinY,
                envelope.MaxX, envelope.MaxY, envelope.MinX, envelope.MaxY};
            double[] z = new double[4];


            DotSpatial.Projections.Reproject.ReprojectPoints(xy, z, dsFrom.Info, dsTo.Info, 0, 4);
            var res = new Envelope();
            var pm = to.Factory.PrecisionModel;
            for (int i = 0; i < 4; i+=2)
                res.ExpandToInclude(pm.MakePrecise( xy[i]),pm.MakePrecise(xy[i+1]));

            return res;
        }

        public override CoordinateSequence Reproject(CoordinateSequence coordinateSequence, SpatialReference @from, SpatialReference to)
        {
            var dsseq = coordinateSequence as DotSpatialAffineCoordinateSequence;
            var dsFrom = CastSpatialReference<DotSpatialSpatialReference>(from);
            var dsTo = CastSpatialReference<DotSpatialSpatialReference>(to);

            if (!CheckArguments(dsseq, nameof(coordinateSequence), dsFrom, dsTo, out var e))
                throw e;

            dsseq = (DotSpatialAffineCoordinateSequence)dsseq.Copy();
            DotSpatial.Projections.Reproject.ReprojectPoints(dsseq.XY, dsseq.Z, dsFrom.Info, dsTo.Info, 0, coordinateSequence.Count);

            return dsseq;
        }
    }
}
