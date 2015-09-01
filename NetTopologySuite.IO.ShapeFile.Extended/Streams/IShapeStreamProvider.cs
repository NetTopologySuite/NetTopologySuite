namespace NetTopologySuite.IO.ShapeFile.Extended.Streams
{
    public interface IShapeStreamProvider
    {
        IStreamProvider ShapeStream { get; }
    }
}