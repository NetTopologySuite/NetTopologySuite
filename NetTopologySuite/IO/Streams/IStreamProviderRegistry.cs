namespace NetTopologySuite.IO.Streams
{
    public interface IStreamProviderRegistry
    {
        IStreamProvider this[string streamType] { get; }
    }

    public static class StreamTypes
    {
        public const string Shape = "SHAPESTREAM";
        public const string Data = "DATASTREAM";
        public const string Index = "INDEXSTREAM";
    }
}