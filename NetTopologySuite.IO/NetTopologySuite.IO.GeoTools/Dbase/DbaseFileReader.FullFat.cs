using System;
using System.IO;
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
                Stream stream = parent._streamProviderRegistry[StreamTypes.Data].OpenRead();
                _dbfStream = new BinaryReader(stream, parent._header.Encoding);
                ReadHeader();
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, 
            /// or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                _dbfStream.Close();
            }
        }

        /// <summary>
        /// Initializes a new instance of the DbaseFileReader class.
        /// </summary>
        /// <param name="filename"></param>
        public DbaseFileReader(string filename) : this(new ShapefileStreamProviderRegistry(null, new FileStreamProvider(filename, true), false, true))
        {


        }


        public DbaseFileReader(IStreamProviderRegistry streamProviderRegistry)
        {
            if (streamProviderRegistry == null)
            {
                throw new ArgumentNullException(nameof(streamProviderRegistry));
            }
            _streamProviderRegistry = streamProviderRegistry;

        }


        /// <summary>
        /// Gets the header information for the dbase file.
        /// </summary>
        /// <returns>DbaseFileHeader contain header and field information.</returns>
        public DbaseFileHeader GetHeader()
        {
            if (_header == null)
            {
                using (var stream = _streamProviderRegistry[StreamTypes.Data].OpenRead())
                using (var dbfStream = new BinaryReader(stream))
                {
                    _header = new DbaseFileHeader();
                    // read the header
                    _header.ReadHeader(dbfStream, _streamProviderRegistry[StreamTypes.Data] is FileStreamProvider ? ((FileStreamProvider)_streamProviderRegistry[StreamTypes.Data]).Path : null);
                }
            }
            return _header;
        }
    }
}
