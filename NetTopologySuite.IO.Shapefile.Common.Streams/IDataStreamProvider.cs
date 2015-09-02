namespace NetTopologySuite.IO.Common.Streams
{
    public interface IDataStreamProvider
    {
        IStreamProvider DataStream { get; }
    }
}