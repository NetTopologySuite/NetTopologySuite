namespace GeoAPI.Geometries
{
    public interface IMultiCurve : IGeometryCollection
    {
        bool IsClosed { get; }
    }
}