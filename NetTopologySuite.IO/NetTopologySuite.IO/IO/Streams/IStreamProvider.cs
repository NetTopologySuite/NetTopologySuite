using System.IO;

namespace NetTopologySuite.IO.Streams
{
    /// <summary>
    /// Interface for stream provider
    /// </summary>
    public interface IStreamProvider
    {
        /// <summary>
        /// Gets a value indicating that the underlying stream is read-only
        /// </summary>
        bool UnderlyingStreamIsReadonly { get; }

        /// <summary>
        /// Function to open the underlying stream for reading purposes
        /// </summary>
        /// <returns>An opened stream</returns>
        Stream OpenRead();

        /// <summary>
        /// Function to open the underlying stream for writing purposes
        /// 
        /// </summary>
        /// <remarks>If <see cref="UnderlyingStreamIsReadonly"/> is not <value>true</value> this method shall fail</remarks>
        /// <returns>An opened stream</returns>
        Stream OpenWrite(bool truncate);

        /// <summary>
        /// Gets a value indicating the kind of stream
        /// </summary>
        string Kind { get; }
    }
}
