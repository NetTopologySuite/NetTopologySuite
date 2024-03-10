using NetTopologySuite.Geometries;

namespace NetTopologySuite.Elevation
{
    public class ConstElevationModel : ElevationModel
    {
        private readonly double _z;

        public ConstElevationModel() : this(Coordinate.NullOrdinate)
        { }

        public ConstElevationModel(double z)
        {
            _z = z;
        }

        public override double GetZ(double x, double y)
        {
            return _z;
        }
    }
}
