using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using ProjNet;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace NetTopologySuite.Reprojection
{
    public class ProjNetReprojection : Reprojection
    {
        public static CoordinateSystemServices CoordinateSystemServices = new CoordinateSystemServices();

        private static readonly CoordinateSystemFactory CoordinateSystemFactory = new CoordinateSystemFactory();
        private static readonly CoordinateTransformationFactory CoordinateTransformationFactory = new CoordinateTransformationFactory();
        private readonly ICoordinateTransformation _coordinateTransformation;
        
        public ProjNetReprojection(SpatialReference source, SpatialReference target)
            : base(source, target)
        {
            _coordinateTransformation = CoordinateTransformationFactory.CreateFromCoordinateSystems(
                CreateCoordinateSystem(source),
                CreateCoordinateSystem(target)
            );
        }

        private static CoordinateSystem CreateCoordinateSystem(SpatialReference spatialReference)
        {
            switch (spatialReference.DefinitionKind)
            {
                case "esriwkt":
                case "wkt":
                    return CoordinateSystemFactory.CreateFromWkt(spatialReference.Definition);
                case "srid":
                    return CoordinateSystemServices.GetCoordinateSystem(int.Parse(spatialReference.Definition));
                //case "authoritycode":
                //    return CoordinateSystemServices.GetCoordinateSystem(int.Parse(spatialReference.Definition));
            }

            throw new ArgumentException(nameof(spatialReference));
        }

        protected override CoordinateSequence Apply(CoordinateSequence coordinateSequence)
        {
            var res = Target.Factory.CoordinateSequenceFactory.Create(coordinateSequence);
            if (res.Dimension > 2)
                Apply3D(res);
            else
                Apply2D(res);
            return res;
        }

        private void Apply2D(CoordinateSequence res)
        {
            var transform = _coordinateTransformation.MathTransform;
            var pm = Target.Factory.PrecisionModel;
            for (int i = 0; i < res.Count; i++)
            {
                (double x, double y) = transform.Transform(res.GetX(i), res.GetY(i));
                res.SetX(i, pm.MakePrecise(x));
                res.SetY(i, pm.MakePrecise(y));
            }
        }

        private void Apply3D(CoordinateSequence res)
        {
            var transform = _coordinateTransformation.MathTransform;
            var pm = Target.Factory.PrecisionModel;
            for (int i = 0; i < res.Count; i++)
            {
                (double x, double y, double z) = transform.Transform(res.GetX(i), res.GetY(i), res.GetZ(i));
                res.SetX(i, pm.MakePrecise(x));
                res.SetY(i, pm.MakePrecise(y));
                res.SetZ(i, z);
            }

        }
    }
}
