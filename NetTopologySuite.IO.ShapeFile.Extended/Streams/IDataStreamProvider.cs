namespace NetTopologySuite.IO.ShapeFile.Extended.Streams
{
    public interface IDataStreamProvider
    {
        IStreamProvider DataStream { get; }
    }
}