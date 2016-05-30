﻿using System;
using System.IO;
using System.Text;
using NetTopologySuite.IO.Streams;

namespace NetTopologySuite.IO
{
    public partial class DbaseFileReader
    {
        private partial class DbaseFileEnumerator
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DbaseFileEnumerator"/> class.
            /// </summary>
            /// <param name="parent"></param>
            public DbaseFileEnumerator(DbaseFileReader parent)
            {
                _parent = parent;
                Stream stream = parent._streamProvider.OpenRead();
                _dbfReader = new BinaryReader(stream, parent._header.Encoding);
                ReadHeader();
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, 
            /// or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                _dbfReader.Close();
            }
        }

        /// <summary>
        /// Initializes a new instance of the DbaseFileReader class.
        /// </summary>
        /// <param name="path">The path to the Dbase file</param>
        public DbaseFileReader(string path) 
            : this(CreateStreamProviderRegistry(path))
        {
        }

        /// <summary>
        /// Initializes a new instance of the DbaseFileReader class.
        /// </summary>
        /// <param name="path">The path to the Dbase file</param>
        /// <param name="encoding">The encoding to use</param>
        public DbaseFileReader(string path, Encoding encoding)
            : this(CreateStreamProviderRegistry(path, encoding))
        {
        }

        /// <summary>
        /// Initializes a new instance of the DbaseFileReader class.
        /// </summary>
        /// <param name="streamProviderRegistry">A stream provider registry</param>
        public DbaseFileReader(IStreamProviderRegistry streamProviderRegistry)
        {
            if (streamProviderRegistry == null)
                throw new ArgumentNullException("streamProviderRegistry");

            _streamProvider = streamProviderRegistry[StreamTypes.Data];
            if (_streamProvider == null)
                throw new ArgumentException("Stream provider registry does not provide a data stream provider", "streamProviderRegistry");

            if (_streamProvider.Kind != StreamTypes.Data)
                throw new ArgumentException(string.Format(
                    "Misconfigured stream provider registry does provide a {0} stream provider when requested data stream provider", 
                    _streamProvider.Kind), "streamProviderRegistry");

            _encodingProvider = streamProviderRegistry[StreamTypes.DataEncoding];
            if (_encodingProvider != null && _encodingProvider.Kind != StreamTypes.DataEncoding)
                throw new ArgumentException(string.Format(
                    "Misconfigured stream provider registry does provide a {0} stream provider when requested data encoding stream provider",
                    _streamProvider.Kind), "streamProviderRegistry");
        }

        /// <summary>
        /// Initializes a new instance of the DbaseFileReader class.
        /// </summary>
        /// <param name="streamProvider">A stream provider</param>
        [Obsolete("Do not use!")]
        public DbaseFileReader(IStreamProvider streamProvider)
        {
            if (streamProvider == null)
                throw new ArgumentNullException("streamProvider");

            if (streamProvider.Kind != StreamTypes.Data)
                throw new ArgumentException("Not a data stream provider", "streamProvider");

            _streamProvider = streamProvider;
        }

        /// <summary>
        /// Gets the header information for the dbase file.
        /// </summary>
        /// <returns>DbaseFileHeader contain header and field information.</returns>
        public DbaseFileHeader GetHeader()
        {
            if (_header == null)
            {
                using (var dbfReader = new BinaryReader(_streamProvider.OpenRead()))
                {
                    // read the header
                    _header = new DbaseFileHeader();
                    _header.ReadHeader(dbfReader, _encodingProvider);
                }
            }
            return _header;
        }
    }
}
