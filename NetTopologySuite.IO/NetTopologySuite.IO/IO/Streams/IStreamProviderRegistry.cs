namespace NetTopologySuite.IO.Streams
{
    /// <summary>
    /// Interface for a registry of stream providers
    /// </summary>
    public interface IStreamProviderRegistry
    {
        /// <summary>
        /// Indexer for a stream provider
        /// </summary>
        /// <param name="streamType">The stream type</param>
        /// <returns>A stream provider</returns>
        IStreamProvider this[string streamType] { get; }
    }

    /// <summary>
    /// An enumeration of stream types
    /// </summary>
    public static class StreamTypes
    {
        /// <summary>
        /// A shape stream
        /// </summary>
        public const string Shape = "SHAPESTREAM";
        /// <summary>
        /// A data stream (dbf)
        /// </summary>
        public const string Data = "DATASTREAM";
        /// <summary>
        /// An index stream
        /// </summary>
        public const string Index = "INDEXSTREAM";
    }
}