namespace NetTopologySuite.IO.Streams
{
    /// <summary>
    /// Interface for a registry of stream providers that -altogether- form a spatial dataset.
    /// </summary>
    public interface IStreamProviderRegistry
    {
        /// <summary>
        /// Indexer for a stream provider
        /// </summary>
        /// <param name="streamType">The stream type</param>
        /// <returns>A stream provider</returns>
        /// <remarks>
        /// If no stream provider for the <paramref name="streamType"/> requested can be provided, <value>null</value> is to be returned.<br/>
        /// Do not throw an exception!
        /// </remarks>
        IStreamProvider this[string streamType] { get; }
    }
}