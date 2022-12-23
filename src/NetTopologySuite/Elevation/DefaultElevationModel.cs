using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm.Elevation
{
    internal class DefaultElevationModel : IElevationModel
    {
        readonly ElevationModel _em;

        public DefaultElevationModel() {
        }

        private DefaultElevationModel(ElevationModel em) {
            this._em = em;
        }

        public Coordinate CopyWithZ(Coordinate p, double z)
        {
            return ZInterpolate.CopyWithZ(p, z);
        }

        public Coordinate CopyWithZInterpolate(Coordinate p, Coordinate p1, Coordinate p2)
        {
            return ZInterpolate.CopyWithZInterpolate(p, p1, p2);
        }

        public IElevationModel Create(Geometry geom1, Geometry geom2)
        {
            return new DefaultElevationModel(ElevationModel.Create(geom1, geom2));
        }

        public double GetZ(double x, double y)
        {
            return _em.GetZ(x, y);
        }

        public void PopulateZ(Geometry geom)
        {
            _em.PopulateZ(geom);
        }

        public double zGet(Coordinate p, Coordinate q)
        {
            return ZInterpolate.zGet(p, q);
        }

        public double zGetOrInterpolate(Coordinate p, Coordinate p1, Coordinate p2)
        {
            return ZInterpolate.zGetOrInterpolate(p, p1, p2);
        }

        public double zInterpolate(Coordinate p, Coordinate p1, Coordinate p2)
        {
            return ZInterpolate.zInterpolate(p, p1, p2);
        }

        public double zInterpolate(Coordinate p, Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2)
        {
            return ZInterpolate.zInterpolate(p, p1, p2, q1, q2);
        }
    }
}