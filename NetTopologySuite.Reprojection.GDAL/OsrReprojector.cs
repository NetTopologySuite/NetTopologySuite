using System;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Reprojection
{
    public class OsrReprojector : Reprojector, IDisposable
    {
        static OsrReprojector () { GdalConfiguration.ConfigureGdal(); }

        public OsrReprojector()
            :this(NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory)
        {
        }

        public OsrReprojector(CoordinateSequenceFactory coordinateSequenceFactory)
            :this(coordinateSequenceFactory, NtsGeometryServices.Instance.DefaultPrecisionModel)
        {
        }
        public OsrReprojector(PrecisionModel precisionModel)
            :this(NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory, precisionModel)
        {
        }
        public OsrReprojector(CoordinateSequenceFactory coordinateSequenceFactory, PrecisionModel precisionModel)
            :this(new EpsgIoSpatialReferenceFactory(coordinateSequenceFactory, precisionModel, "wkt"))
        {
        }

        private OsrReprojector(SpatialReferenceFactory spatialReferenceFactory)
            : base(spatialReferenceFactory, new OsrReprojectionFactory()) { }

        void IDisposable.Dispose()
        {
            if (ReprojectionFactory is IDisposable dr)
                dr.Dispose();
        }

    }

    public class OsrReprojection : Reprojection, IDisposable
    {
        private readonly OSGeo.OSR.SpatialReference _source;
        private readonly OSGeo.OSR.SpatialReference _target;
        private readonly OSGeo.OSR.CoordinateTransformation _transform;

        public OsrReprojection(SpatialReference source, SpatialReference target)
            : base(source,target)
        {
            _source = new OSGeo.OSR.SpatialReference(source.Definition);
            _target = new OSGeo.OSR.SpatialReference(target.Definition);
            _transform = new OSGeo.OSR.CoordinateTransformation(_source, _target);
        }

        void IDisposable.Dispose()
        {
            if (IsDisposed) return;
            
            IsDisposed = true;

            _transform.Dispose();
            _target.Dispose();
            _source.Dispose();
        }

        protected override CoordinateSequence Apply(CoordinateSequence coordinateSequence)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(OsrReprojection));

            double[] x = new double[coordinateSequence.Count];
            double[] y = new double[coordinateSequence.Count];
            double[] z = new double[coordinateSequence.Count];

            bool hasZ = coordinateSequence.HasZ;
            for (int i = 0; i < x.Length; i++)
            {
                x[i] = coordinateSequence.GetX(i);
                y[i] = coordinateSequence.GetY(i);
                if (hasZ) z[i] = coordinateSequence.GetZ(i);
            }

            _transform.TransformPoints(x.Length, x, y, z);

            // make a copy assigning the target factory
            var res = Target.Factory.CoordinateSequenceFactory.Create(coordinateSequence);

            // Set precised x and y
            var pm = Target.Factory.PrecisionModel;
            for (int i = 0; i < x.Length; i++)
            {
                res.SetX(i, pm.MakePrecise(x[i]));
                res.SetY(i, pm.MakePrecise(y[i]));
                if (hasZ) res.SetZ(i, z[i]);
            }

            return res;
        }

        internal bool IsDisposed { get; private set; }
    }
}
