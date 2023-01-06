using NetTopologySuite.Geometries;
using NetTopologySuite.Elevation;

namespace NetTopologySuite.Algorithm
{
    public sealed class RobustLineIntersectorWithElevationModel : RobustLineIntersector
    {
        private readonly ElevationModel _elevationModel;

        public RobustLineIntersectorWithElevationModel(ElevationModel elevationModel)
        {
            _elevationModel = elevationModel;
        }

        protected override double zGet(Coordinate p, Coordinate q)
        {
            if (!double.IsNaN(p.Z))
                return p.Z;

            return _elevationModel.GetZ(p.X, p.Y);
        }

        protected override double zGetOrInterpolate(Coordinate p, Coordinate p1, Coordinate p2)
        {
            if (!double.IsNaN(p.Z))
                return p.Z;

            return _elevationModel.GetZ(p.X, p.Y);
        }

        protected override double zInterpolate(Coordinate p, Coordinate p1, Coordinate p2)
        {
            if (!double.IsNaN(p.Z))
                return p.Z;

            return _elevationModel.GetZ(p.X, p.Y);
        }
        protected override double zInterpolate(Coordinate p, Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2)
        {
            if (!double.IsNaN(p.Z))
                return p.Z;

            return _elevationModel.GetZ(p.X, p.Y);
        }
    }
}
