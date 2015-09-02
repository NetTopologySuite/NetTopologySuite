namespace NetTopologySuite.IO.Common.Streams
{
    public interface IShapeStreamProvider
    {
        IStreamProvider ShapeStream { get; }
    }


    public interface IIndexStreamProvider
    {
        IStreamProvider IndexStream { get; }
    }
}