using System;
using System.IO;

namespace NetTopologySuite.IO.Streams
{
    /// <summary>
    /// An implementation of <see cref="IStreamProvider"/> that gives acces to file streams
    /// </summary>
    public class FileStreamProvider : IStreamProvider
    {
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="kind">The kind of stream</param>
        /// <param name="path">The path to the stream</param>
        /// <param name="validatePath">A value indicating if the provided path is to be validated</param>
        public FileStreamProvider(string kind, string path, bool validatePath = false)
        {
            if (path == null)
                throw new ArgumentNullException("path");

#if NET40
            if (string.IsNullOrWhiteSpace(path))
#else
            if (string.IsNullOrEmpty(path))
#endif
                throw new ArgumentException("Invalid Path", "path");

            if (validatePath && !File.Exists(path))
                throw new FileNotFoundException(path);

            Kind = kind;
            Path = path;
        }

        /// <summary>
        /// Gets a value indicating the path to the file
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Gets a value indicating that the underlying stream is read-only
        /// </summary>
        public bool UnderlyingStreamIsReadonly
        {
            get { return false; }
        }

        /// <summary>
        /// Function to open the underlying stream for reading purposes
        /// </summary>
        /// <returns>An opened stream</returns>
        public Stream OpenRead()
        {
            return File.Open(Path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        /// <summary>
        /// Function to open the underlying stream for writing purposes
        /// </summary>
        /// <remarks>If <see cref="IStreamProvider.UnderlyingStreamIsReadonly"/> is not <value>true</value> this method shall fail</remarks>
        /// <returns>An opened stream</returns>
        public Stream OpenWrite(bool truncate)
        {
            if (truncate)
            {
                return File.Open(Path, FileMode.Create, FileAccess.Write, FileShare.None);
            }
            return File.Open(Path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite); //jd: I would like to use FileShare.None however the GeoTools shapefilewriter writes a dummy file while holding an existing handle
        }

        /// <summary>
        /// Gets a value indicating the kind of stream
        /// </summary>
        public string Kind { get; private set; }
    }
}