namespace GeoAPI.Geometries
{
    public interface IPoint : IGeometry, IPuntal
    {
        double X { get; set; }

        double Y { get; set; }

        double Z { get; set; }

        double M { get; set; }

        ICoordinateSequence CoordinateSequence { get; }
    }
}
