using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Reprojection.GDAL;

namespace NetTopologySuite.Reprojection
{
    public class OsrReprojector : Reprojector, IDisposable
    {
        static OsrReprojector () { GdalConfiguration.ConfigureGdal(); }

        public OsrReprojector()
        {
            LookUp = new EpsgIoSpatialReferenceLookUp("wkt");
        }
        public OsrReprojector(CoordinateSequenceFactory coordinateSequenceFactory)
            :base(coordinateSequenceFactory)
        {
            LookUp = new EpsgIoSpatialReferenceLookUp("wkt");
        }
        public OsrReprojector(PrecisionModel precisionModel)
            :base(precisionModel)
        {
            LookUp = new EpsgIoSpatialReferenceLookUp("wkt");
        }
        public OsrReprojector(CoordinateSequenceFactory coordinateSequenceFactory, PrecisionModel precisionModel)
            :base(coordinateSequenceFactory, precisionModel)
        {
            LookUp = new EpsgIoSpatialReferenceLookUp("wkt");
        }

        void IDisposable.Dispose()
        {
            foreach (var spatialReferencesValue in SpatialReferences.Values)
                ((OsrSpatialReference)spatialReferencesValue)?.Instance?.Dispose();
        }

        protected override SpatialReference Create(string definition, int srid)
        {
            var srInstance = new OSGeo.OSR.SpatialReference(definition);
            var factory = new GeometryFactory(PrecisionModel, srid, CoordinateSequenceFactory);
            return new OsrSpatialReference(definition, factory, srInstance);
        }

        public override Coordinate Reproject(Coordinate coordinate, SpatialReference @from, SpatialReference to)
        {
            var osrFrom = CastSpatialReference<OsrSpatialReference>(from);
            var osrTo = CastSpatialReference<OsrSpatialReference>(to);

            if (!CheckArguments(coordinate, nameof(coordinate), from, to, out var e))
                throw e;

            using (var ct = new OSGeo.OSR.CoordinateTransformation(osrFrom.Instance, osrTo.Instance))
            {
                double[] ordinates = new[] {coordinate.X, coordinate.Y, coordinate.Z};
                ct.TransformPoint(ordinates);

                var res = coordinate.Copy();
                res.X = ordinates[0];
                res.Y = ordinates[1];
                if (res is CoordinateZ)
                    res.Z = ordinates[2];

                to.Factory.PrecisionModel.MakePrecise(res);
                return res;
            }
        }

        public override Envelope Reproject(Envelope envelope, SpatialReference @from, SpatialReference to)
        {
            var osrFrom = CastSpatialReference<OsrSpatialReference>(from);
            var osrTo = CastSpatialReference<OsrSpatialReference>(to);

            if (!CheckArguments(envelope, nameof(envelope), osrFrom, osrTo, out var e))
                throw e;

            double[] x = { envelope.MinX, envelope.MaxX, envelope.MaxX, envelope.MinX};
            double[] y = { envelope.MinY, envelope.MinY, envelope.MaxY, envelope.MaxY};
            double[] z = new double [4];


            ReprojectInternal(x, y, z, osrFrom, osrTo);
            var res = new Envelope();
            for (int i = 0; i < 4; i++)
                res.ExpandToInclude(x[i], y[i]);

            return res;
        }

        private void ReprojectInternal(double[] x, double[] y, double[] z, OsrSpatialReference @from, OsrSpatialReference @to)
        {
            using (var ct = new OSGeo.OSR.CoordinateTransformation(from.Instance, to.Instance))
                ct.TransformPoints(x.Length, x, y, z);

            for (int i = 0; i < 12; i += 3) {
                x[i] = PrecisionModel.MakePrecise(y[i]);
                y[i] = PrecisionModel.MakePrecise(y[i]);
            }
        }

        public override CoordinateSequence Reproject(CoordinateSequence coordinateSequence, SpatialReference @from, SpatialReference to)
        {
            var osrFrom = CastSpatialReference<OsrSpatialReference>(from);
            var osrTo = CastSpatialReference<OsrSpatialReference>(to);

            if (!CheckArguments(coordinateSequence, nameof(coordinateSequence), osrFrom, osrTo, out var e))
                throw e;

            ExtractOrdinates(coordinateSequence, out double[] x, out double[] y, out double[] z);
            ReprojectInternal(x, y, z, osrFrom, osrTo);

            var res = CoordinateSequenceFactory.Create(coordinateSequence.Count, coordinateSequence.Ordinates);
            bool hasZ = res.HasZ;
            bool hasM = res.HasM;
            for (int i = 0; i < x.Length; i++) {
                res.SetX(i, x[i]);
                res.SetY(i, y[i]);
                if (hasZ) res.SetX(i, z[i]);
                if (hasM) res.SetM(i, coordinateSequence.GetM(i));
            }

            return res;
        }

        private void ExtractOrdinates(CoordinateSequence coordinateSequence, out double[] x, out double[] y, out double[] z)
        {
            x = new double[coordinateSequence.Count];
            y = new double[coordinateSequence.Count];
            z = new double[coordinateSequence.Count];

            bool hasZ = coordinateSequence.HasZ;
            for (int i = 0; i < x.Length; i++)
            {
                x[i] = coordinateSequence.GetX(i);
                y[i] = coordinateSequence.GetY(i);
                if (hasZ) z[i] = coordinateSequence.GetZ(i);
            }
        }
    }
}
