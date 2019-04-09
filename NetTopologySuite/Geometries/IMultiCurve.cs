namespace NetTopologySuite.Geometries
{
    public interface IMultiCurve : IGeometryCollection
    {
        bool IsClosed { get; }
    }
}